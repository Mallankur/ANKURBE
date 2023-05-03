using System;
using System.Collections.Generic;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Domain.Interfaces;
using Adform.Bloom.Runtime.Contracts.Response;
using Adform.Bloom.Write.Commands;
using Adform.Bloom.Write.Handlers;
using Adform.Bloom.Write.Mappers;
using Adform.Bloom.Write.PostProcessors;
using Adform.Bloom.Write.Services;
using CorrelationId.Abstractions;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Adform.Bloom.Write.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureCommands(this IServiceCollection services,
            IConfigurationRoot configuration)
        {
            var policyId = configuration.GetValue<Guid>("DefaultPolicyId");

            RegisterMappers(services);
            RegisterServices(services);
            RegisterDeleteHandlers(services);
            RegisterCreateHandlers(services, policyId);
            RegisterAssignHandlers(services);
            RegisterUpdateHandlers(services);
            RegisterPostProcessors(services);

            return services;
        }

        private static void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<INotificationService, NotificationService>();
            services.AddSingleton<IEventsGenerator, EventsGenerator>();
        }

        private static void RegisterMappers(IServiceCollection services)
        {
            services.AddSingleton<IRequestToEntityMapper<CreateFeatureCommand, Feature>,
                NamedNodeMapper<CreateFeatureCommand, Feature>>();
            services.AddSingleton<IRequestToEntityMapper<CreatePermissionCommand, Permission>,
                NamedNodeMapper<CreatePermissionCommand, Permission>>();
            services.AddSingleton<IRequestToEntityMapper<CreatePolicyCommand, Policy>,
                NamedNodeMapper<CreatePolicyCommand, Policy>>();
            services.AddSingleton<IRequestToEntityMapper<CreateRoleCommand, Role>,
                NamedNodeMapper<CreateRoleCommand, Role>>();
            services.AddSingleton<IRequestToEntityMapper<UpdateRoleCommand, Role>,
                NamedNodeMapper<UpdateRoleCommand, Role>>();
            services.AddSingleton<IRequestToEntityMapper<CreateTenantCommand, Tenant>,
                NamedNodeMapper<CreateTenantCommand, Tenant>>();
            services.AddSingleton<IRequestToEntityMapper<CreateSubjectCommand, Subject>,
                SubjectMapper>();
        }

        private static void RegisterDeleteHandlers(IServiceCollection services)
        {
            services.AddSingleton(
                typeof(IRequestHandler<DeleteRoleCommand, Unit>),
                typeof(DeleteRoleCommandHandler));
            services.AddSingleton(
                typeof(IRequestHandler<DeleteSubjectCommand, Unit>),
                typeof(DeleteSubjectCommandHandler));
            services.AddSingleton(
                typeof(IRequestHandler<DeletePermissionCommand, Unit>),
                typeof(BaseDeleteCommandHandler<Permission>));
            services.AddSingleton(
                typeof(IRequestHandler<DeletePolicyCommand, Unit>),
                typeof(BaseDeleteCommandHandler<Policy>));
            services.AddSingleton(
                typeof(IRequestHandler<DeleteTenantCommand, Unit>),
                typeof(BaseDeleteCommandHandler<Tenant>));
            services.AddSingleton(
                typeof(IRequestHandler<DeleteFeatureCommand, Unit>),
                typeof(BaseDeleteCommandHandler<Feature>));
        }

        private static void RegisterCreateHandlers(IServiceCollection services, Guid policyId)
        {
            services.AddSingleton(
                typeof(IRequestHandler<CreateRoleCommand, Role>),
                prv => new CreateRoleCommandHandler(
                    policyId,
                    prv.GetRequiredService<IRequestToEntityMapper<CreateRoleCommand, Role>>(),
                    prv.GetRequiredService<IAccessValidator>(),
                    prv.GetRequiredService<IAdminGraphRepository>(),
                    prv.GetRequiredService<IMediator>()));
            services.AddSingleton(
                typeof(IRequestHandler<CreateSubjectCommand, Subject>),
                prv => new CreateSubjectCommandHandler(
                    prv.GetRequiredService<IRequestToEntityMapper<CreateSubjectCommand, Subject>>(),
                    prv.GetRequiredService<IAccessValidator>(),
                    prv.GetRequiredService<IAdminGraphRepository>(),
                    prv.GetRequiredService<IMediator>(),
                    prv.GetRequiredService<ICorrelationContextAccessor>()));

            services.AddSingleton(
                typeof(IRequestHandler<CreatePolicyCommand, Policy>),
                typeof(CreateWithParentIdCommandHandler<CreatePolicyCommand, Policy>));
            services.AddSingleton(
                typeof(IRequestHandler<CreateTenantCommand, Tenant>),
                typeof(CreateWithParentIdCommandHandler<CreateTenantCommand, Tenant>));

            services.AddSingleton(
                typeof(IRequestHandler<CreatePermissionCommand, Permission>),
                typeof(BaseCreateCommandHandler<CreatePermissionCommand, Permission>));
            services.AddSingleton(
                typeof(IRequestHandler<CreateFeatureCommand, Feature>),
                typeof(BaseCreateCommandHandler<CreateFeatureCommand, Feature>));
        }

        private static void RegisterUpdateHandlers(IServiceCollection services)
        {
            services.AddSingleton(
                typeof(IRequestHandler<UpdateRoleCommand, Role>),
                typeof(UpdateRoleCommandHandler));
        }
        
        private static void RegisterPostProcessors(IServiceCollection services)
        {
            services.AddSingleton(typeof(IRequestPostProcessor<UpdateSubjectAssignmentsCommand, IEnumerable<RuntimeResponse>>),
                typeof(UpdateSubjectAssignmentsCommandPostProcessor));
        }
        
        private static void RegisterAssignHandlers(IServiceCollection services)
        {
            services.AddSingleton(
                typeof(IRequestHandler<AssignPermissionToRoleCommand, Unit>),
                typeof(AssignPermissionToRoleCommandHandler));
            services.AddSingleton(
                typeof(IRequestHandler<AssignPermissionToFeatureCommand, Unit>),
                typeof(AssignPermissionToFeatureCommandHandler));
            services.AddSingleton(
                typeof(IRequestHandler<AssignFeatureCoDependencyCommand, Unit>),
                typeof(AssignFeatureCoDependencyCommandHandler));
            services.AddSingleton(
                typeof(IRequestHandler<UpdateLicensedFeatureToTenantAssignmentsCommand, Unit>),
                typeof(UpdateLicensedFeatureToTenantAssignmentsCommandHandler));
            services.AddSingleton(
                typeof(IRequestHandler<UpdateSubjectAssignmentsCommand, IEnumerable<RuntimeResponse>>),
                typeof(UpdateSubjectAssignmentsCommandHandler));
            services.AddSingleton(
                typeof(IRequestHandler<UpdateRoleToFeatureAssignmentsCommand, Unit>),
                typeof(UpdateRoleToFeatureAssignmentsCommandHandler));
        }
    }
}