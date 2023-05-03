using System;
using System.Collections.Generic;
using System.Linq;
using Adform.Bloom.Messages.Commands.AssetsReassignment;
using Adform.Bloom.Messages.Events;
using Adform.Bloom.Read.Contracts.BusinessAccount;
using Adform.Bloom.Runtime.Contracts.Response;
using Adform.Ciam.SharedKernel.Services;
using CorrelationId.Abstractions;
using AssetsReassignment = Adform.Bloom.Contracts.Input.AssetsReassignment;

namespace Adform.Bloom.Write.Services
{
    public class EventsGenerator : IEventsGenerator
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ICorrelationContextAccessor _correlationContextAccessor;

        public EventsGenerator(IDateTimeProvider dateTimeProvider,
            ICorrelationContextAccessor correlationContextAccessor)
        {
            _dateTimeProvider = dateTimeProvider;
            _correlationContextAccessor = correlationContextAccessor;
        }

        public EventTuple GenerateSubjectAssignmentEvents(Guid subjectId, Guid actorId,
            IEnumerable<RuntimeResponse> originalState,
            IEnumerable<RuntimeResponse> newState,
            IReadOnlyCollection<AssetsReassignment>? assetsReassignments = null)
        {
            var correlationId = Guid.Parse(_correlationContextAccessor.CorrelationContext.CorrelationId);
            var subjectAssigned = new List<SubjectAssignmentEvent>();
            var subjectUnassigned = new List<SubjectUnassignedEvent>();
            var reassignAssets = new List<ReassignUserAssetsCommand>();

            var oldDict = originalState.GroupBy(x => x.TenantId)
                .ToDictionary(x => x.Key, x => x.First());
            var newDict = newState.GroupBy(x => x.TenantId)
                .ToDictionary(x => x.Key, x => x.First());
            var newUsers = assetsReassignments?
                .GroupBy(t => (t.LegacyBusinessAccountId, t.BusinessAccountType))
                .ToDictionary(t => t.Key, t => t.First().NewUserId);

            var disabled = SubjectDisabled(subjectId, actorId, oldDict, newDict, correlationId);
            if (newDict.Count > 0)
            {
                subjectAssigned.AddRange(SubjectCreated(subjectId, actorId, oldDict, newDict, correlationId));
                subjectUnassigned.AddRange(SubjectDetached(subjectId, actorId, oldDict, newDict, correlationId));
                reassignAssets.AddRange(ReassignUserAssets(subjectId, oldDict, newDict, newUsers, correlationId));
                //Min delta at role level
                // var changes = SubjectUpdated(subjectId, actorId, oldDict, newDict);
                // subjectAssigned.AddRange(changes.AssignedEvents);
                // subjectUnassigned.AddRange(changes.UnassignedEvents);
            }

            return new EventTuple
            {
                DisabledEvent = disabled,
                AssignedEvents = subjectAssigned,
                UnassignedEvents = subjectUnassigned,
                ReassignUserAssetsCommands = reassignAssets
            };
        }

        public IEnumerable<SubjectAssignmentsNotification> GenerateSubjectAssignmentsNotifications(Guid subjectId,
            Guid actorId,
            IEnumerable<RuntimeResponse> originalState,
            IEnumerable<RuntimeResponse> newState)
        {
            var correlationId = Guid.Parse(_correlationContextAccessor.CorrelationContext.CorrelationId);
            var oldDict = originalState.GroupBy(x => x.TenantId)
                .ToDictionary(x => x.Key, x => x.First());
            var newDict = newState.GroupBy(x => x.TenantId)
                .ToDictionary(x => x.Key, x => x.First());

            var oldTenantList = oldDict.Keys.ToArray();
            var newTenantList = newDict.Keys.ToArray();

            var notifications = new List<SubjectAssignmentsNotification>();

            foreach (var tenantKey in newTenantList)
            {
                var tenant = newDict[tenantKey];
                oldDict.TryGetValue(tenantKey, out var oldTenant);

                var notification = new SubjectAssignmentsNotification
                {
                    SubjectId = subjectId.ToString(),
                    TenantId = tenant.TenantId.ToString(),
                    TenantName = tenant.TenantName,
                    EventId = correlationId.ToString(),
                    Timestamp = _dateTimeProvider.UtcNowWithOffset.ToUnixTimeMilliseconds()
                };

                if (oldTenant != null)
                {
                    notification.Assignments = tenant.Roles.Except(oldTenant.Roles).ToArray();
                    notification.Unassignments = oldTenant.Roles.Except(tenant.Roles).ToArray();
                    if (!notification.Assignments.Any() && !notification.Unassignments.Any())
                        continue;
                }
                else
                {
                    notification.Assignments = tenant.Roles.ToArray();
                    notification.Unassignments = new List<string>();
                }

                notifications.Add(notification);
            }

            return notifications;
        }

        public SubjectAuthorizationResultChangedEvent GenerateSubjectAuthorizationChangedEvent(Guid subjectId,
            Guid actorId, IEnumerable<RuntimeResponse> newState)
        {
            var eventId = Guid.Parse(_correlationContextAccessor.CorrelationContext.CorrelationId);
            return new SubjectAuthorizationResultChangedEvent
            {
                SubjectId = subjectId.ToString(),
                Authorization = newState.Select(o => new AuthorizationResult
                {
                    TenantId = o.TenantId,
                    TenantName = o.TenantName,
                    Permissions = o.Permissions,
                    Roles = o.Roles
                }),
                EventId = eventId.ToString(),
                Timestamp = _dateTimeProvider.UtcNowWithOffset.ToUnixTimeMilliseconds()
            };
        }

        private IEnumerable<SubjectAssignmentEvent> SubjectCreated(Guid subjectId, Guid actorId,
            Dictionary<Guid, RuntimeResponse> oldState,
            Dictionary<Guid, RuntimeResponse> newState, Guid correlationId)
        {
            var subjectAssigned = new List<SubjectAssignmentEvent>();
            var existedDMP = oldState.Values
                .Where(p => p.TenantType == BusinessAccountType.DataProvider.ToString())
                .Select(p => p.TenantId).Any();

            foreach (var tenant in newState)
            {
                var tenantExists = oldState.ContainsKey(tenant.Key);
                var subject = default(SubjectAssignmentEvent?);

                if (tenantExists)
                {
                    var oldPermissions = oldState[tenant.Key].Permissions;
                    var permissions = tenant.Value.Permissions.Except(oldPermissions).ToArray();
                    if (permissions.Any())
                    {
                        subject = GetSubject<SubjectAssignmentEvent>(subjectId, actorId, correlationId, tenant.Value, permissions);
                    }
                }
                else
                {
                    subject = GetSubject<SubjectAssignmentEvent>(subjectId, actorId, correlationId, tenant.Value, tenant.Value.Permissions.ToArray());
                }

                if (subject != default)
                {
                    subject.InitialConnection = tenant.Value.TenantType != BusinessAccountType.DataProvider.ToString() || !existedDMP;
                    subjectAssigned.Add(subject);
                }
            }

            return subjectAssigned;
        }

        private IEnumerable<SubjectUnassignedEvent> SubjectDetached(Guid subjectId, Guid actorId,
            Dictionary<Guid, RuntimeResponse> oldState,
            Dictionary<Guid, RuntimeResponse> newState, Guid correlationId)
        {
            var subjectUnassigned = new List<SubjectUnassignedEvent>();

            foreach (var tenant in oldState)
            {
                var tenantExists = newState.ContainsKey(tenant.Key);
                var subject = default(SubjectUnassignedEvent?);

                if (tenantExists)
                {
                    var newPermissions = newState[tenant.Key].Permissions;
                    var permissions = tenant.Value.Permissions.Except(newPermissions).ToArray();
                    if (permissions.Any())
                    {
                        subject = GetSubject<SubjectUnassignedEvent>(subjectId, actorId, correlationId, tenant.Value, permissions);
                    }
                }
                else
                {
                    subject = GetSubject<SubjectUnassignedEvent>(subjectId, actorId, correlationId, tenant.Value, tenant.Value.Permissions.ToArray());
                }

                if (subject != default)
                {
                    subject.DestroyedConnection = true;
                    subjectUnassigned.Add(subject);
                }
            }

            return subjectUnassigned;
        }

        private T GetSubject<T>(Guid subjectId, Guid actorId, Guid correlationId, RuntimeResponse tenant, string[] permissions) where T : AssignmentBaseEvent, new()
        {
            return new T
            {
                SubjectId = subjectId,
                ActorId = actorId,
                TenantId = tenant.TenantId,
                TenantName = tenant.TenantName,
                TenantLegacyId = tenant.TenantLegacyId,
                TenantType = tenant.TenantType,
                Timestamp = _dateTimeProvider.UtcNowWithOffset.ToUnixTimeMilliseconds(),
                CorrelationId = correlationId,
                Permissions = permissions
            };
        }

        private IEnumerable<Guid> DeletedTenants(Dictionary<Guid, RuntimeResponse> oldState,
            Dictionary<Guid, RuntimeResponse> newState)
        {
            var oldTenantList = oldState.Keys.ToArray();
            var newTenantList = newState.Keys.ToArray();
            return oldTenantList.Except(newTenantList).ToArray();
        }

        private (IEnumerable<SubjectAssignmentEvent> AssignedEvents,
            IEnumerable<SubjectUnassignedEvent> UnassignedEventscorrelationId)
            SubjectUpdated(Guid subjectId, Guid actorId, Dictionary<Guid, RuntimeResponse> oldState,
                Dictionary<Guid, RuntimeResponse> newState,
                Guid correlationId)
        {
            var oldTenantList = oldState.Keys.ToArray();
            var newTenantList = newState.Keys.ToArray();

            var subjectAssigned = new List<SubjectAssignmentEvent>();
            var subjectUnassigned = new List<SubjectUnassignedEvent>();
            var possiblyUpdatedTenants = newTenantList.Intersect(oldTenantList).ToArray();
            foreach (var key in possiblyUpdatedTenants)
            {
                var tenant = newState[key];
                var oldRoles = oldState[key].Roles;
                var newRoles = newState[key].Roles;
                var createdRoles = newRoles.Except(oldRoles).ToArray();
                var deletedRoles = oldRoles.Except(newRoles).ToArray();
                if (createdRoles.Any())
                    subjectAssigned.Add(new SubjectAssignmentEvent
                    {
                        SubjectId = subjectId,
                        ActorId = actorId,
                        TenantId = tenant.TenantId,
                        TenantLegacyId = tenant.TenantLegacyId,
                        TenantType = tenant.TenantType,
                        Timestamp = _dateTimeProvider.UtcNowWithOffset.ToUnixTimeMilliseconds(),
                        CorrelationId = correlationId
                    });

                if (deletedRoles.Any())
                    subjectUnassigned.Add(new SubjectUnassignedEvent
                    {
                        SubjectId = subjectId,
                        ActorId = actorId,
                        TenantId = tenant.TenantId,
                        TenantLegacyId = tenant.TenantLegacyId,
                        TenantType = tenant.TenantType,
                        Timestamp = _dateTimeProvider.UtcNowWithOffset.ToUnixTimeMilliseconds(),
                        CorrelationId = correlationId
                    });
            }

            return (subjectAssigned, subjectUnassigned);
        }

        private SubjectDisabledEvent? SubjectDisabled(Guid subjectId, Guid actorId,
            Dictionary<Guid, RuntimeResponse> oldState,
            Dictionary<Guid, RuntimeResponse> newState,
            Guid correlationId)
        {
            SubjectDisabledEvent? disabledSubject = null;
            if (newState.Count == 0)
                disabledSubject = new SubjectDisabledEvent
                {
                    SubjectId = subjectId,
                    ActorId = actorId,
                    Timestamp = _dateTimeProvider.UtcNowWithOffset.ToUnixTimeMilliseconds(),
                    CorrelationId = correlationId
                };

            return disabledSubject;
        }

        private IEnumerable<ReassignUserAssetsCommand> ReassignUserAssets(Guid subjectId,
            Dictionary<Guid, RuntimeResponse> oldState,
            Dictionary<Guid, RuntimeResponse> newState,
            Dictionary<(int, BusinessAccountType), Guid>? newUsers, Guid correlationId)
        {
            var commands = new List<ReassignUserAssetsCommand>();
            var deletedTenantsIds = DeletedTenants(oldState, newState);
            var deletedTenants = oldState
                .Where(t => deletedTenantsIds.Contains(t.Key)).Select(t => t.Value)
                .ToLookup(t => Enum.Parse<BusinessAccountType>(t.TenantType));

            var agenciesReassignments =
                GetAssetsReassignments(BusinessAccountType.Agency, deletedTenants, newUsers).ToList();
            if (agenciesReassignments.Any())
                commands.Add(new ReassignUserAgenciesAssetsCommand(correlationId)
                {
                    AssetsReassignments = agenciesReassignments,
                    CurrentUserId = subjectId
                });

            var publishersReassignments =
                GetAssetsReassignments(BusinessAccountType.Publisher, deletedTenants, newUsers).ToList();
            if (publishersReassignments.Any())
                commands.Add(new ReassignUserPublishersAssetsCommand(correlationId)
                {
                    AssetsReassignments = publishersReassignments,
                    CurrentUserId = subjectId
                });

            var dataProvidersReassignments =
                GetAssetsReassignments(BusinessAccountType.DataProvider, deletedTenants, newUsers).ToList();
            if (dataProvidersReassignments.Any())
                commands.Add(new ReassignUserDataProvidersAssetsCommand(correlationId)
                {
                    AssetsReassignments = dataProvidersReassignments,
                    CurrentUserId = subjectId
                });

            return commands;
        }

        private IEnumerable<Messages.Commands.AssetsReassignment.AssetsReassignment> GetAssetsReassignments(
            BusinessAccountType accountType, ILookup<BusinessAccountType, RuntimeResponse> deletedTenants,
            Dictionary<(int, BusinessAccountType), Guid>? newUsers)
        {
            if (!deletedTenants[accountType].Any())
                return new List<Messages.Commands.AssetsReassignment.AssetsReassignment>();
            return deletedTenants[accountType].Select(t =>
                new Messages.Commands.AssetsReassignment.AssetsReassignment
                {
                    LegacyBusinessAccountId = t.TenantLegacyId,
                    NewUserId =
                        newUsers != null && newUsers.Any() && newUsers.Keys.Contains((t.TenantLegacyId,
                            Enum.Parse<BusinessAccountType>(t.TenantType)))
                            ? newUsers[(t.TenantLegacyId, Enum.Parse<BusinessAccountType>(t.TenantType))]
                            : Guid.Empty
                });
        }
    }
}