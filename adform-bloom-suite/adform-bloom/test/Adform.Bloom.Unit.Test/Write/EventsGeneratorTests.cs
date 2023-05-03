using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Adform.Bloom.Messages.Commands.AssetsReassignment;
using Adform.Bloom.Messages.Events;
using Adform.Bloom.Read.Contracts.BusinessAccount;
using Adform.Bloom.Runtime.Contracts.Response;
using Adform.Bloom.Write.Services;
using Adform.Ciam.SharedKernel.Services;
using CorrelationId;
using CorrelationId.Abstractions;
using Moq;
using Xunit;
using AssetsReassignment = Adform.Bloom.Contracts.Input.AssetsReassignment;

namespace Adform.Bloom.Unit.Test.Write;

public class EventsGeneratorTests
{
    private readonly Mock<ICorrelationContextAccessor> _correlationContextAccessor;
    private readonly Mock<IDateTimeProvider> _dateTimeProvider;

    public EventsGeneratorTests()
    {
        _dateTimeProvider = new Mock<IDateTimeProvider>();
        _correlationContextAccessor = new Mock<ICorrelationContextAccessor>();
    }

    [Fact]
    public void GenerateSubjectAssignmentEvents_Return_SubjectAuthorizationResultChangedEvent()
    {
        // Arrange
        var timestamp = DateTimeOffset.UtcNow;
        var correlationId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var subjectId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var tenantLegacyId = 0;
        var tenantName = "tenant";
        var tenantType = BusinessAccountType.Adform;
        var role = "role";
        var permission = "permission";

        _dateTimeProvider
            .Setup(p => p.UtcNowWithOffset)
            .Returns(timestamp);
        _correlationContextAccessor
            .Setup(p => p.CorrelationContext)
            .Returns(new CorrelationContext(correlationId.ToString(), "CorrelationId"));

        var newState = new List<RuntimeResponse>
        {
            new()
            {
                TenantId = tenantId,
                TenantName = tenantName,
                TenantLegacyId = tenantLegacyId,
                TenantType = tenantType.ToString(),
                Permissions = new List<string> {permission},
                Roles = new List<string> {role}
            }
        };

        var expected = Expected_SubjectAuthorizationResultChangedEvent(subjectId,
            tenantId,
            tenantName,
            new List<string> {role},
            new List<string> {permission},
            correlationId,
            timestamp);

        var eventsGenerator = new EventsGenerator(_dateTimeProvider.Object, _correlationContextAccessor.Object);

        // Act
        var result =
            eventsGenerator.GenerateSubjectAuthorizationChangedEvent(subjectId, actorId, newState);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expected.SubjectId, result.SubjectId);
        Assert.Equal(expected.EventId, result.EventId);
        Assert.Equal(expected.Timestamp, result.Timestamp);
        Assert.True(expected.Authorization.Select(p => p.TenantId)
            .SequenceEqual(result.Authorization.Select(p => p.TenantId)));
    }

    [Fact]
    public void GenerateSubjectAssignmentEvents_Return_SubjectAssignmentsNotification()
    {
        // Arrange
        var timestamp = DateTimeOffset.UtcNow;
        var correlationId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var subjectId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var tenantLegacyId = 0;
        var tenantName = "tenant";
        var tenantType = BusinessAccountType.Adform;
        var role = "role";
        var permission = "permission";

        _dateTimeProvider
            .Setup(p => p.UtcNowWithOffset)
            .Returns(timestamp);
        _correlationContextAccessor
            .Setup(p => p.CorrelationContext)
            .Returns(new CorrelationContext(correlationId.ToString(), "CorrelationId"));

        var originalState = new List<RuntimeResponse>();
        var newState = new List<RuntimeResponse>
        {
            new()
            {
                TenantId = tenantId,
                TenantName = tenantName,
                TenantLegacyId = tenantLegacyId,
                TenantType = tenantType.ToString(),
                Permissions = new List<string> {permission},
                Roles = new List<string> {role}
            }
        };

        var expected = Expected_SubjectAssignmentsNotification(subjectId,
            tenantId,
            new List<string> {role},
            new List<string>(),
            tenantName,
            correlationId,
            timestamp);

        var eventsGenerator = new EventsGenerator(_dateTimeProvider.Object, _correlationContextAccessor.Object);

        // Act
        var result =
            eventsGenerator.GenerateSubjectAssignmentsNotifications(subjectId, actorId, originalState, newState);

        // Assert
        var notification = result.First();
        Assert.NotNull(notification);
        Assert.Equal(expected.TenantId, notification.TenantId);
        Assert.Equal(expected.TenantName, notification.TenantName);
        Assert.Equal(expected.SubjectId, notification.SubjectId);
        Assert.True(expected.Assignments.SequenceEqual(notification.Assignments));
        Assert.True(expected.Unassignments.SequenceEqual(notification.Unassignments));
        Assert.Equal(expected.EventId, notification.EventId);
        Assert.Equal(expected.Timestamp, notification.Timestamp);
    }

    [Fact]
    public void GenerateSubjectAssignmentEvents_Return_UnassignedEvents_When_NewState_Doesnt_Contain_Tenant()
    {
        // Arrange
        var timestamp = DateTimeOffset.UtcNow;
        var correlationId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var subjectId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var tenantId1 = Guid.NewGuid();
        var tenantLegacyId = 0;
        var tenantLegacyId1 = 1;
        var tenantName = "tenant";
        var tenantName1 = "tenant1";
        var tenantType = BusinessAccountType.Adform;
        var tenantType1 = BusinessAccountType.Agency;
        var role = "role";
        var permission = "permission";

        _dateTimeProvider
            .Setup(p => p.UtcNowWithOffset)
            .Returns(timestamp);
        _correlationContextAccessor
            .Setup(p => p.CorrelationContext)
            .Returns(new CorrelationContext(correlationId.ToString(), "CorrelationId"));

        var originalState = new List<RuntimeResponse>
        {
            new()
            {
                TenantId = tenantId,
                TenantName = tenantName,
                TenantLegacyId = tenantLegacyId,
                TenantType = tenantType.ToString(),
                Permissions = new List<string> {role},
                Roles = new List<string> {permission}
            },
            new()
            {
                TenantId = tenantId1,
                TenantName = tenantName1,
                TenantLegacyId = tenantLegacyId1,
                TenantType = tenantType1.ToString(),
                Permissions = new List<string> {role},
                Roles = new List<string> {permission}
            }
        };
        var newState = new List<RuntimeResponse>
        {
            new()
            {
                TenantId = tenantId1,
                TenantName = tenantName1,
                TenantLegacyId = tenantLegacyId1,
                TenantType = tenantType1.ToString(),
                Permissions = new List<string> {role},
                Roles = new List<string> {permission}
            }
        };

        var expected = Expected_SubjectUnassignedEvent_DestroyedConnection(subjectId,
            actorId,
            tenantId,
            tenantLegacyId,
            tenantType.ToString(),
            correlationId,
            timestamp);

        var eventsGenerator = new EventsGenerator(_dateTimeProvider.Object, _correlationContextAccessor.Object);

        // Act
        var result =
            eventsGenerator.GenerateSubjectAssignmentEvents(subjectId, actorId, originalState, newState);

        // Assert
        var unassignmentEvent = result.UnassignedEvents.FirstOrDefault();
        var expectedUnassignmentEvent = expected.UnassignedEvents.FirstOrDefault();

        Assert.NotNull(result);
        Assert.Null(result.DisabledEvent);
        Assert.Empty(result.AssignedEvents);
        Assert.Equal(expectedUnassignmentEvent.CorrelationId, unassignmentEvent.CorrelationId);
        Assert.Equal(expectedUnassignmentEvent.Timestamp, unassignmentEvent.Timestamp);
        Assert.Equal(expectedUnassignmentEvent.DestroyedConnection, unassignmentEvent.DestroyedConnection);
        Assert.Equal(expectedUnassignmentEvent.ActorId, unassignmentEvent.ActorId);
        Assert.Equal(expectedUnassignmentEvent.SubjectId, unassignmentEvent.SubjectId);
        Assert.Equal(expectedUnassignmentEvent.TenantId, unassignmentEvent.TenantId);
        Assert.Equal(expectedUnassignmentEvent.TenantLegacyId, unassignmentEvent.TenantLegacyId);
    }

    [Fact]
    public void
        GenerateSubjectAssignmentEvents_Return_AssignedEvents_With_InitialConnection_When_NewState_Contains_New_Tenant()
    {
        // Arrange
        var timestamp = DateTimeOffset.UtcNow;
        var correlationId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var subjectId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var tenantLegacyId = 0;
        var tenantName = "tenant";
        var tenantType = BusinessAccountType.Adform;
        var role = "role";
        var permission = "permission";

        _dateTimeProvider
            .Setup(p => p.UtcNowWithOffset)
            .Returns(timestamp);
        _correlationContextAccessor
            .Setup(p => p.CorrelationContext)
            .Returns(new CorrelationContext(correlationId.ToString(), "CorrelationId"));

        var originalState = new List<RuntimeResponse>();
        var newState = new List<RuntimeResponse>
        {
            new()
            {
                TenantId = tenantId,
                TenantName = tenantName,
                TenantLegacyId = tenantLegacyId,
                TenantType = tenantType.ToString(),
                Permissions = new List<string> {role},
                Roles = new List<string> {permission}
            }
        };

        var expected = Expected_SubjectAssignedEvent_InitialConnection(subjectId,
            actorId,
            tenantId,
            tenantLegacyId,
            tenantType.ToString(),
            correlationId,
            timestamp);

        var eventsGenerator = new EventsGenerator(_dateTimeProvider.Object, _correlationContextAccessor.Object);

        // Act
        var result = eventsGenerator.GenerateSubjectAssignmentEvents(subjectId, actorId, originalState, newState);

        // Assert
        var assignmentEvent = result.AssignedEvents.FirstOrDefault();
        var expectedAssignmentEvent = expected.AssignedEvents.FirstOrDefault();

        Assert.NotNull(result);
        Assert.Null(result.DisabledEvent);
        Assert.Empty(result.UnassignedEvents);
        Assert.Equal(expectedAssignmentEvent.CorrelationId, assignmentEvent.CorrelationId);
        Assert.Equal(expectedAssignmentEvent.Timestamp, assignmentEvent.Timestamp);
        Assert.Equal(expectedAssignmentEvent.InitialConnection, assignmentEvent.InitialConnection);
        Assert.Equal(expectedAssignmentEvent.ActorId, assignmentEvent.ActorId);
        Assert.Equal(expectedAssignmentEvent.SubjectId, assignmentEvent.SubjectId);
        Assert.Equal(expectedAssignmentEvent.TenantId, assignmentEvent.TenantId);
        Assert.Equal(expectedAssignmentEvent.TenantLegacyId, assignmentEvent.TenantLegacyId);
    }

    [Fact]
    public void GenerateSubjectAssignmentEvents_Return_DisabledEvent_When_NewState_Empty()
    {
        // Arrange
        var timestamp = DateTimeOffset.UtcNow;
        var correlationId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var subjectId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var tenantLegacyId = 0;
        var tenantName = "tenant";
        var tenantType = BusinessAccountType.Adform;
        var role = "role";
        var permission = "permission";

        _dateTimeProvider
            .Setup(p => p.UtcNowWithOffset)
            .Returns(timestamp);
        _correlationContextAccessor
            .Setup(p => p.CorrelationContext)
            .Returns(new CorrelationContext(correlationId.ToString(), "CorrelationId"));

        var originalState = new List<RuntimeResponse>
        {
            new()
            {
                TenantId = tenantId,
                TenantName = tenantName,
                TenantLegacyId = tenantLegacyId,
                TenantType = tenantType.ToString(),
                Permissions = new List<string> {role},
                Roles = new List<string> {permission}
            }
        };
        var newState = new List<RuntimeResponse>();

        var expected = Expected_SubjectDisabledEvent(subjectId,
            actorId,
            correlationId,
            timestamp);

        var eventsGenerator = new EventsGenerator(_dateTimeProvider.Object, _correlationContextAccessor.Object);

        // Act
        var result = eventsGenerator.GenerateSubjectAssignmentEvents(subjectId, actorId, originalState, newState);

        // Assert
        var disabledEvent = result.DisabledEvent;
        var expectedDisabledEvent = expected.DisabledEvent;

        Assert.NotNull(result);
        Assert.NotNull(result.DisabledEvent);
        Assert.Empty(result.AssignedEvents);
        Assert.Empty(result.UnassignedEvents);
        Assert.Equal(expectedDisabledEvent.CorrelationId, disabledEvent.CorrelationId);
        Assert.Equal(expectedDisabledEvent.Timestamp, disabledEvent.Timestamp);
        Assert.Equal(expectedDisabledEvent.ActorId, disabledEvent.ActorId);
        Assert.Equal(expectedDisabledEvent.SubjectId, disabledEvent.SubjectId);
    }

    [Fact]
    public void
        GenerateSubjectAssignmentEvents_On_DMP_Return_AssignedEvents_With_InitialConnection_When_NewState_Contains_New_Tenant_And_OldState_DontContain_DMP_Tenant()
    {
        // Arrange
        var timestamp = DateTimeOffset.UtcNow;
        var correlationId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var subjectId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var tenantLegacyId = 0;
        var tenantName = "tenant";
        var tenantType = BusinessAccountType.DataProvider;
        var role = "role";
        var permission = "permission";

        _dateTimeProvider
            .Setup(p => p.UtcNowWithOffset)
            .Returns(timestamp);
        _correlationContextAccessor
            .Setup(p => p.CorrelationContext)
            .Returns(new CorrelationContext(correlationId.ToString(), "CorrelationId"));

        var originalState = new List<RuntimeResponse>();
        var newState = new List<RuntimeResponse>
        {
            new()
            {
                TenantId = tenantId,
                TenantName = tenantName,
                TenantLegacyId = tenantLegacyId,
                TenantType = tenantType.ToString(),
                Permissions = new List<string> {role},
                Roles = new List<string> {permission}
            }
        };

        var expected = Expected_SubjectAssignedEvent_InitialConnection(subjectId,
            actorId,
            tenantId,
            tenantLegacyId,
            tenantType.ToString(),
            correlationId,
            timestamp);

        var eventsGenerator = new EventsGenerator(_dateTimeProvider.Object, _correlationContextAccessor.Object);

        // Act
        var result = eventsGenerator.GenerateSubjectAssignmentEvents(subjectId, actorId, originalState, newState);

        // Assert
        var assignmentEvent = result.AssignedEvents.FirstOrDefault();
        var expectedAssignmentEvent = expected.AssignedEvents.FirstOrDefault();

        Assert.NotNull(result);
        Assert.Null(result.DisabledEvent);
        Assert.Empty(result.UnassignedEvents);
        Assert.Equal(expectedAssignmentEvent.CorrelationId, assignmentEvent.CorrelationId);
        Assert.Equal(expectedAssignmentEvent.Timestamp, assignmentEvent.Timestamp);
        Assert.Equal(expectedAssignmentEvent.InitialConnection, assignmentEvent.InitialConnection);
        Assert.Equal(expectedAssignmentEvent.ActorId, assignmentEvent.ActorId);
        Assert.Equal(expectedAssignmentEvent.SubjectId, assignmentEvent.SubjectId);
        Assert.Equal(expectedAssignmentEvent.TenantId, assignmentEvent.TenantId);
        Assert.Equal(expectedAssignmentEvent.TenantLegacyId, assignmentEvent.TenantLegacyId);
    }

    [Fact]
    public void
        GenerateSubjectAssignmentEvents_On_DMP_Return_AssignedEvents_Without_InitialConnection_When_NewState_Contains_New_Tenant_And_OldState_Contains_DMP_Tenant()
    {
        // Arrange
        var timestamp = DateTimeOffset.UtcNow;
        var correlationId = Guid.NewGuid();
        var actorId = Guid.NewGuid();
        var subjectId = Guid.NewGuid();
        var tenantId0 = Guid.NewGuid();
        var tenantId1 = Guid.NewGuid();
        var tenantLegacyId0 = 0;
        var tenantName0 = "tenant";
        var tenantLegacyId1 = 1;
        var tenantName1 = "tenant1";
        var tenantType = BusinessAccountType.DataProvider;
        var role = "role";
        var permission = "permission";

        _dateTimeProvider
            .Setup(p => p.UtcNowWithOffset)
            .Returns(timestamp);
        _correlationContextAccessor
            .Setup(p => p.CorrelationContext)
            .Returns(new CorrelationContext(correlationId.ToString(), "CorrelationId"));

        var originalState = new List<RuntimeResponse>
        {
            new()
            {
                TenantId = tenantId0,
                TenantName = tenantName0,
                TenantLegacyId = tenantLegacyId0,
                TenantType = tenantType.ToString(),
                Permissions = new List<string> {role},
                Roles = new List<string> {permission}
            }
        };

        var newState = new List<RuntimeResponse>
        {
            new()
            {
                TenantId = tenantId0,
                TenantName = tenantName0,
                TenantLegacyId = tenantLegacyId0,
                TenantType = tenantType.ToString(),
                Permissions = new List<string> {role},
                Roles = new List<string> {permission}
            },
            new()
            {
                TenantId = tenantId1,
                TenantName = tenantName1,
                TenantLegacyId = tenantLegacyId1,
                TenantType = tenantType.ToString(),
                Permissions = new List<string> {role},
                Roles = new List<string> {permission}
            }
        };

        var expected = Expected_SubjectAssignedEvent_InitialConnection(subjectId,
            actorId,
            tenantId1,
            tenantLegacyId1,
            tenantType.ToString(),
            correlationId,
            timestamp);
        expected.AssignedEvents = expected.AssignedEvents.Select(p =>
        {
            p.InitialConnection = false;
            return p;
        });

        var eventsGenerator = new EventsGenerator(_dateTimeProvider.Object, _correlationContextAccessor.Object);

        // Act
        var result = eventsGenerator.GenerateSubjectAssignmentEvents(subjectId, actorId, originalState, newState);

        // Assert
        var assignmentEvent = result.AssignedEvents.FirstOrDefault();
        var expectedAssignmentEvent = expected.AssignedEvents.FirstOrDefault();

        Assert.NotNull(result);
        Assert.Null(result.DisabledEvent);
        Assert.Empty(result.UnassignedEvents);
        Assert.Equal(expectedAssignmentEvent.CorrelationId, assignmentEvent.CorrelationId);
        Assert.Equal(expectedAssignmentEvent.Timestamp, assignmentEvent.Timestamp);
        Assert.Equal(expectedAssignmentEvent.InitialConnection, assignmentEvent.InitialConnection);
        Assert.Equal(expectedAssignmentEvent.ActorId, assignmentEvent.ActorId);
        Assert.Equal(expectedAssignmentEvent.SubjectId, assignmentEvent.SubjectId);
        Assert.Equal(expectedAssignmentEvent.TenantId, assignmentEvent.TenantId);
        Assert.Equal(expectedAssignmentEvent.TenantLegacyId, assignmentEvent.TenantLegacyId);
    }

    [Theory]
    [ClassData(typeof(ReassignAssetsData))]
    public void GenerateSubjectAssignmentEvents_Returns_ReassignUserAssetsCommands_When_Adform_Tenant_AccessLost(
        DateTimeOffset timestamp, Guid correlationId, Guid actorId, Guid subjectId,
        List<RuntimeResponse> originalState, List<RuntimeResponse> newState, List<AssetsReassignment> assetsReassignments,
        EventTuple expectedResult)
    {
        // Arrange
        _dateTimeProvider
            .Setup(p => p.UtcNowWithOffset)
            .Returns(timestamp);
        _correlationContextAccessor
            .Setup(p => p.CorrelationContext)
            .Returns(new CorrelationContext(correlationId.ToString(), "CorrelationId"));

        var eventsGenerator = new EventsGenerator(_dateTimeProvider.Object, _correlationContextAccessor.Object);

        // Act
        var result =
            eventsGenerator.GenerateSubjectAssignmentEvents(subjectId, actorId, originalState, newState,
                assetsReassignments);

        // Assert
        Assert.NotNull(result);
        if (expectedResult.ReassignUserAssetsCommands.Any())
        {
            Assert.NotEmpty(result.ReassignUserAssetsCommands);
            var businessAccountsActual =
                result.ReassignUserAssetsCommands.SelectMany(c =>
                    c.AssetsReassignments.Select(r => r.LegacyBusinessAccountId)).OrderBy(id => id);
            var businessAccountsExpected =
                expectedResult.ReassignUserAssetsCommands.SelectMany(c =>
                    c.AssetsReassignments.Select(r => r.LegacyBusinessAccountId)).OrderBy(id => id);
            var newUsersActual = result.ReassignUserAssetsCommands.SelectMany(c =>
                c.AssetsReassignments.Where(r => r.NewUserId != Guid.Empty).Select(r =>
                    (BusinessAccountType: GetBusinessAccountTypeByReassignCommandType(c), r.LegacyBusinessAccountId,
                        r.NewUserId))).OrderBy(u => u.NewUserId);
            var newUsersExpected = expectedResult.ReassignUserAssetsCommands.SelectMany(c =>
                c.AssetsReassignments.Where(r => r.NewUserId != Guid.Empty).Select(r =>
                    (BusinessAccountType: GetBusinessAccountTypeByReassignCommandType(c), r.LegacyBusinessAccountId,
                        r.NewUserId))).OrderBy(u => u.NewUserId);
            Assert.True(businessAccountsActual.SequenceEqual(businessAccountsExpected));
            Assert.True(newUsersActual.SequenceEqual(newUsersExpected));
        }
        else
        {
            Assert.Empty(result.ReassignUserAssetsCommands);
        }

        Assert.Equal(expectedResult.UnassignedEvents.Count(), result.UnassignedEvents.Count());
        Assert.Empty(result.AssignedEvents);
    }

    private SubjectAuthorizationResultChangedEvent Expected_SubjectAuthorizationResultChangedEvent(Guid subjectId,
        Guid tenantId,
        string tenantName,
        IEnumerable<string> roles,
        IEnumerable<string> permissions,
        Guid correlationId,
        DateTimeOffset timestamp)
    {
        return new SubjectAuthorizationResultChangedEvent
        {
            EventId = correlationId.ToString(),
            Timestamp = timestamp.ToUnixTimeMilliseconds(),
            SubjectId = subjectId.ToString(),
            Authorization = new List<AuthorizationResult>
            {
                new()
                {
                    TenantId = tenantId,
                    TenantName = tenantName,
                    Roles = roles,
                    Permissions = permissions
                }
            }
        };
    }

    private SubjectAssignmentsNotification Expected_SubjectAssignmentsNotification(Guid subjectId,
        Guid tenantId,
        IEnumerable<string> assignmentRole,
        IEnumerable<string> unAssignmentRole,
        string tenantName,
        Guid correlationId,
        DateTimeOffset timestamp)
    {
        return new SubjectAssignmentsNotification
        {
            EventId = correlationId.ToString(),
            Timestamp = timestamp.ToUnixTimeMilliseconds(),
            SubjectId = subjectId.ToString(),
            TenantId = tenantId.ToString(),
            TenantName = tenantName,
            Assignments = assignmentRole,
            Unassignments = unAssignmentRole
        };
    }

    private EventTuple Expected_SubjectAssignedEvent_InitialConnection(
        Guid subjectId,
        Guid actorId,
        Guid tenantId,
        int tenantLegacyId,
        string tenantType,
        Guid correlationId,
        DateTimeOffset timestamp)
    {
        return new EventTuple
        {
            DisabledEvent = null,
            AssignedEvents = new List<SubjectAssignmentEvent>
            {
                new()
                {
                    ActorId = actorId,
                    SubjectId = subjectId,
                    TenantId = tenantId,
                    TenantLegacyId = tenantLegacyId,
                    TenantType = tenantType,
                    InitialConnection = true,
                    CorrelationId = correlationId,
                    Timestamp = timestamp.ToUnixTimeMilliseconds()
                }
            },
            UnassignedEvents = new List<SubjectUnassignedEvent>()
        };
    }

    private EventTuple Expected_SubjectDisabledEvent(
        Guid subjectId,
        Guid actorId,
        Guid correlationId,
        DateTimeOffset timestamp)
    {
        return new EventTuple
        {
            DisabledEvent = new SubjectDisabledEvent
            {
                ActorId = actorId,
                SubjectId = subjectId,
                CorrelationId = correlationId,
                Timestamp = timestamp.ToUnixTimeMilliseconds()
            },
            AssignedEvents = new List<SubjectAssignmentEvent>(),
            UnassignedEvents = new List<SubjectUnassignedEvent>()
        };
    }

    private EventTuple Expected_SubjectUnassignedEvent_DestroyedConnection(
        Guid subjectId,
        Guid actorId,
        Guid tenantId,
        int tenantLegacyId,
        string tenantType,
        Guid correlationId,
        DateTimeOffset timestamp)
    {
        return new EventTuple
        {
            DisabledEvent = null,
            AssignedEvents = new List<SubjectAssignmentEvent>(),
            UnassignedEvents = new List<SubjectUnassignedEvent>
            {
                new()
                {
                    ActorId = actorId,
                    SubjectId = subjectId,
                    TenantId = tenantId,
                    TenantLegacyId = tenantLegacyId,
                    TenantType = tenantType,
                    DestroyedConnection = true,
                    CorrelationId = correlationId,
                    Timestamp = timestamp.ToUnixTimeMilliseconds()
                }
            }
        };
    }

    private ReassignUserAssetsCommand Expected_ReassignUserAssetsCommand_DestroyedConnection(
        Guid subjectId,
        int tenantLegacyId,
        Guid newUserId,
        BusinessAccountType tenantType,
        Guid correlationId)
    {
        var assetsReassignments = new List<Messages.Commands.AssetsReassignment.AssetsReassignment>
            {new() {LegacyBusinessAccountId = tenantLegacyId, NewUserId = newUserId}};
        return tenantType switch
        {
            BusinessAccountType.Agency => new ReassignUserAgenciesAssetsCommand(correlationId)
            {
                AssetsReassignments = assetsReassignments,
                CorrelationId = correlationId,
                CurrentUserId = subjectId
            },
            BusinessAccountType.Publisher => new ReassignUserPublishersAssetsCommand(correlationId)
            {
                AssetsReassignments = assetsReassignments,
                CorrelationId = correlationId,
                CurrentUserId = subjectId
            },
            BusinessAccountType.DataProvider => new ReassignUserDataProvidersAssetsCommand(correlationId)
            {
                AssetsReassignments = assetsReassignments,
                CorrelationId = correlationId,
                CurrentUserId = subjectId
            },
            _ => null
        };
    }

    private class ReassignAssetsData : IEnumerable<object[]>
    {
        private readonly DateTimeOffset timestamp = DateTimeOffset.UtcNow;
        private readonly Guid correlationId = Guid.NewGuid();
        private readonly Guid actorId = Guid.NewGuid();
        private readonly Guid subjectId = Guid.NewGuid();
        private readonly Guid newUserId = Guid.NewGuid();
        private readonly Guid tenantId = Guid.NewGuid();
        private readonly Guid tenantId1 = Guid.NewGuid();
        private readonly Guid tenantId2 = Guid.NewGuid();
        private readonly Guid tenantId3 = Guid.NewGuid();
        private readonly int tenantLegacyId = 0;
        private readonly int tenantLegacyId1 = 1;
        private readonly int tenantLegacyId2 = 2;
        private readonly int tenantLegacyId3 = 3;
        private readonly string tenantName = "tenant";
        private readonly string tenantName1 = "tenant1";
        private readonly string tenantName2 = "tenant2";
        private readonly string tenantName3 = "tenant3";
        private readonly BusinessAccountType tenantType = BusinessAccountType.Adform;
        private readonly BusinessAccountType tenantType1 = BusinessAccountType.Agency;
        private readonly BusinessAccountType tenantType2 = BusinessAccountType.Publisher;
        private readonly BusinessAccountType tenantType3 = BusinessAccountType.DataProvider;
        private readonly string role = "role";
        private readonly string permission = "permission";

        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
                timestamp,
                correlationId,
                actorId,
                subjectId,
                new List<RuntimeResponse>
                {
                    new()
                    {
                        TenantId = tenantId,
                        TenantName = tenantName,
                        TenantLegacyId = tenantLegacyId,
                        TenantType = tenantType.ToString(),
                        Permissions = new List<string> {permission},
                        Roles = new List<string> {role}
                    },
                    new()
                    {
                        TenantId = tenantId1,
                        TenantName = tenantName1,
                        TenantLegacyId = tenantLegacyId1,
                        TenantType = tenantType1.ToString(),
                        Permissions = new List<string> {permission},
                        Roles = new List<string> {role}
                    }
                },
                new List<RuntimeResponse>
                {
                    new()
                    {
                        TenantId = tenantId1,
                        TenantName = tenantName1,
                        TenantLegacyId = tenantLegacyId1,
                        TenantType = tenantType1.ToString(),
                        Permissions = new List<string> {permission},
                        Roles = new List<string> {role}
                    }
                },
                new List<AssetsReassignment>(),
                new EventTuple
                {
                    DisabledEvent = null,
                    AssignedEvents = new List<SubjectAssignmentEvent>(),
                    UnassignedEvents = new List<SubjectUnassignedEvent>
                    {
                        new()
                        {
                            ActorId = actorId,
                            SubjectId = subjectId,
                            TenantId = tenantId,
                            TenantLegacyId = tenantLegacyId,
                            TenantType = tenantType.ToString(),
                            DestroyedConnection = true,
                            CorrelationId = correlationId,
                            Timestamp = timestamp.ToUnixTimeMilliseconds()
                        }
                    }
                }
            };
            yield return new object[]
            {
                timestamp,
                correlationId,
                actorId,
                subjectId,
                new List<RuntimeResponse>
                {
                    new()
                    {
                        TenantId = tenantId,
                        TenantName = tenantName,
                        TenantLegacyId = tenantLegacyId,
                        TenantType = tenantType.ToString(),
                        Permissions = new List<string> {permission},
                        Roles = new List<string> {role}
                    },
                    new()
                    {
                        TenantId = tenantId1,
                        TenantName = tenantName1,
                        TenantLegacyId = tenantLegacyId1,
                        TenantType = tenantType1.ToString(),
                        Permissions = new List<string> {permission},
                        Roles = new List<string> {role}
                    }
                },
                new List<RuntimeResponse>
                {
                    new()
                    {
                        TenantId = tenantId1,
                        TenantName = tenantName1,
                        TenantLegacyId = tenantLegacyId1,
                        TenantType = tenantType1.ToString(),
                        Permissions = new List<string> {permission},
                        Roles = new List<string> {role}
                    }
                },
                new List<AssetsReassignment>
                {
                    new()
                    {
                        LegacyBusinessAccountId = tenantLegacyId,
                        NewUserId = newUserId
                    },
                    new()
                    {
                        LegacyBusinessAccountId = tenantLegacyId1,
                        NewUserId = newUserId
                    }
                },
                new EventTuple
                {
                    DisabledEvent = null,
                    AssignedEvents = new List<SubjectAssignmentEvent>(),
                    UnassignedEvents = new List<SubjectUnassignedEvent>
                    {
                        new()
                        {
                            ActorId = actorId,
                            SubjectId = subjectId,
                            TenantId = tenantId,
                            TenantLegacyId = tenantLegacyId,
                            TenantType = tenantType.ToString(),
                            DestroyedConnection = true,
                            CorrelationId = correlationId,
                            Timestamp = timestamp.ToUnixTimeMilliseconds()
                        }
                    }
                }
            };
            yield return new object[]
            {
                timestamp,
                correlationId,
                actorId,
                subjectId,
                new List<RuntimeResponse>
                {
                    new()
                    {
                        TenantId = tenantId,
                        TenantName = tenantName,
                        TenantLegacyId = tenantLegacyId,
                        TenantType = tenantType.ToString(),
                        Permissions = new List<string> {permission},
                        Roles = new List<string> {role}
                    },
                    new()
                    {
                        TenantId = tenantId1,
                        TenantName = tenantName1,
                        TenantLegacyId = tenantLegacyId1,
                        TenantType = tenantType1.ToString(),
                        Permissions = new List<string> {permission},
                        Roles = new List<string> {role}
                    }
                },
                new List<RuntimeResponse>
                {
                    new()
                    {
                        TenantId = tenantId,
                        TenantName = tenantName,
                        TenantLegacyId = tenantLegacyId,
                        TenantType = tenantType.ToString(),
                        Permissions = new List<string> {permission},
                        Roles = new List<string> {role}
                    }
                },
                new List<AssetsReassignment>(),
                new EventTuple
                {
                    DisabledEvent = null,
                    AssignedEvents = new List<SubjectAssignmentEvent>(),
                    UnassignedEvents = new List<SubjectUnassignedEvent>
                    {
                        new()
                        {
                            ActorId = actorId,
                            SubjectId = subjectId,
                            TenantId = tenantId1,
                            TenantLegacyId = tenantLegacyId1,
                            TenantType = tenantType1.ToString(),
                            DestroyedConnection = true,
                            CorrelationId = correlationId,
                            Timestamp = timestamp.ToUnixTimeMilliseconds()
                        }
                    },
                    ReassignUserAssetsCommands = new List<ReassignUserAssetsCommand>
                    {
                        new ReassignUserAgenciesAssetsCommand(correlationId)
                        {
                            AssetsReassignments = new List<Messages.Commands.AssetsReassignment.AssetsReassignment>
                            {
                                new()
                                {
                                    LegacyBusinessAccountId = tenantLegacyId1
                                }
                            }
                        }
                    }
                }
            };
            yield return new object[]
            {
                timestamp,
                correlationId,
                actorId,
                subjectId,
                new List<RuntimeResponse>
                {
                    new()
                    {
                        TenantId = tenantId,
                        TenantName = tenantName,
                        TenantLegacyId = tenantLegacyId,
                        TenantType = tenantType.ToString(),
                        Permissions = new List<string> {permission},
                        Roles = new List<string> {role}
                    },
                    new()
                    {
                        TenantId = tenantId1,
                        TenantName = tenantName1,
                        TenantLegacyId = tenantLegacyId1,
                        TenantType = tenantType1.ToString(),
                        Permissions = new List<string> {permission},
                        Roles = new List<string> {role}
                    }
                },
                new List<RuntimeResponse>
                {
                    new()
                    {
                        TenantId = tenantId,
                        TenantName = tenantName,
                        TenantLegacyId = tenantLegacyId,
                        TenantType = tenantType.ToString(),
                        Permissions = new List<string> {permission},
                        Roles = new List<string> {role}
                    }
                },
                new List<AssetsReassignment>
                {
                    new()
                    {
                        LegacyBusinessAccountId = tenantLegacyId1,
                        BusinessAccountType = BusinessAccountType.Agency,
                        NewUserId = newUserId
                    }
                },
                new EventTuple
                {
                    DisabledEvent = null,
                    AssignedEvents = new List<SubjectAssignmentEvent>(),
                    UnassignedEvents = new List<SubjectUnassignedEvent>
                    {
                        new()
                        {
                            ActorId = actorId,
                            SubjectId = subjectId,
                            TenantId = tenantId1,
                            TenantLegacyId = tenantLegacyId1,
                            TenantType = tenantType1.ToString(),
                            DestroyedConnection = true,
                            CorrelationId = correlationId,
                            Timestamp = timestamp.ToUnixTimeMilliseconds()
                        }
                    },
                    ReassignUserAssetsCommands = new List<ReassignUserAssetsCommand>
                    {
                        new ReassignUserAgenciesAssetsCommand(correlationId)
                        {
                            AssetsReassignments = new List<Messages.Commands.AssetsReassignment.AssetsReassignment>
                            {
                                new()
                                {
                                    LegacyBusinessAccountId = tenantLegacyId1,
                                    NewUserId = newUserId
                                }
                            }
                        }
                    }
                }
            };
            yield return new object[]
            {
                timestamp,
                correlationId,
                actorId,
                subjectId,
                new List<RuntimeResponse>
                {
                    new()
                    {
                        TenantId = tenantId,
                        TenantName = tenantName,
                        TenantLegacyId = tenantLegacyId,
                        TenantType = tenantType.ToString(),
                        Permissions = new List<string> {permission},
                        Roles = new List<string> {role}
                    },
                    new()
                    {
                        TenantId = tenantId1,
                        TenantName = tenantName1,
                        TenantLegacyId = tenantLegacyId1,
                        TenantType = tenantType1.ToString(),
                        Permissions = new List<string> {permission},
                        Roles = new List<string> {role}
                    },
                    new()
                    {
                        TenantId = tenantId2,
                        TenantName = tenantName2,
                        TenantLegacyId = tenantLegacyId2,
                        TenantType = tenantType2.ToString(),
                        Permissions = new List<string> {permission},
                        Roles = new List<string> {role}
                    }
                },
                new List<RuntimeResponse>
                {
                    new()
                    {
                        TenantId = tenantId,
                        TenantName = tenantName,
                        TenantLegacyId = tenantLegacyId,
                        TenantType = tenantType.ToString(),
                        Permissions = new List<string> {permission},
                        Roles = new List<string> {role}
                    }
                },
                new List<AssetsReassignment>
                {
                    new()
                    {
                        LegacyBusinessAccountId = tenantLegacyId1,
                        BusinessAccountType = BusinessAccountType.Agency,
                        NewUserId = newUserId
                    }
                },
                new EventTuple
                {
                    DisabledEvent = null,
                    AssignedEvents = new List<SubjectAssignmentEvent>(),
                    UnassignedEvents = new List<SubjectUnassignedEvent>
                    {
                        new()
                        {
                            ActorId = actorId,
                            SubjectId = subjectId,
                            TenantId = tenantId1,
                            TenantLegacyId = tenantLegacyId1,
                            TenantType = tenantType1.ToString(),
                            DestroyedConnection = true,
                            CorrelationId = correlationId,
                            Timestamp = timestamp.ToUnixTimeMilliseconds()
                        },
                        new()
                        {
                            ActorId = actorId,
                            SubjectId = subjectId,
                            TenantId = tenantId2,
                            TenantLegacyId = tenantLegacyId2,
                            TenantType = tenantType2.ToString(),
                            DestroyedConnection = true,
                            CorrelationId = correlationId,
                            Timestamp = timestamp.ToUnixTimeMilliseconds()
                        }
                    },
                    ReassignUserAssetsCommands = new List<ReassignUserAssetsCommand>
                    {
                        new ReassignUserAgenciesAssetsCommand(correlationId)
                        {
                            AssetsReassignments = new List<Messages.Commands.AssetsReassignment.AssetsReassignment>
                            {
                                new()
                                {
                                    LegacyBusinessAccountId = tenantLegacyId1,
                                    NewUserId = newUserId
                                },
                                new()
                                {
                                    LegacyBusinessAccountId = tenantLegacyId2
                                }
                            }
                        }
                    }
                }
            };
            yield return new object[]
            {
                timestamp,
                correlationId,
                actorId,
                subjectId,
                new List<RuntimeResponse>(),
                new List<RuntimeResponse>(),
                null,
                new EventTuple()
            };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    private BusinessAccountType GetBusinessAccountTypeByReassignCommandType(ReassignUserAssetsCommand command)
    {
        return command switch
        {
            ReassignUserAgenciesAssetsCommand => BusinessAccountType.Agency,
            ReassignUserPublishersAssetsCommand => BusinessAccountType.Publisher,
            ReassignUserDataProvidersAssetsCommand => BusinessAccountType.DataProvider,
            _ => BusinessAccountType.Adform
        };
    }
}