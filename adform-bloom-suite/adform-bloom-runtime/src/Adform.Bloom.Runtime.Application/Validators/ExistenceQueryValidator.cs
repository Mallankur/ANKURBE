using Adform.Bloom.Application.Queries;
using FluentValidation;
using FluentValidation.Results;

namespace Adform.Bloom.Application.Validators
{
    public class ExistenceQueryValidator : IExistenceQueryValidator
    {
        private readonly NodeExistenceQueryValidator _nodeExistenceValidator;
        private readonly RoleExistenceQueryValidator _roleExistenceQueryValidator;
        private readonly LegacyTenantExistenceQueryValidator _tenantExistenceQueryValidator;
        private readonly SubjectIntersectionQueryValidator _subjectIntersectionQueryValidator;

        public ExistenceQueryValidator()
        {
            _nodeExistenceValidator = new NodeExistenceQueryValidator();
            _roleExistenceQueryValidator = new RoleExistenceQueryValidator();
            _tenantExistenceQueryValidator = new LegacyTenantExistenceQueryValidator();
            _subjectIntersectionQueryValidator = new SubjectIntersectionQueryValidator();
        }

        public ValidationResult Validate(NodeExistenceQuery query)
        {
            return _nodeExistenceValidator.Validate(query);
        }

        public ValidationResult Validate(RoleExistenceQuery query)
        {
            return _roleExistenceQueryValidator.Validate(query);
        }

        public ValidationResult Validate(LegacyTenantExistenceQuery query)
        {
            return _tenantExistenceQueryValidator.Validate(query);
        }

        public ValidationResult Validate(SubjectIntersectionQuery query)
        {
            return _subjectIntersectionQueryValidator.Validate(query);
        }

        private class NodeExistenceQueryValidator : AbstractValidator<NodeExistenceQuery>
        {
            public NodeExistenceQueryValidator()
            {
                RuleFor(q => q.NodeDescriptors)
                    .Cascade(CascadeMode.Stop)
                    .NotNull()
                    .NotEmpty()
                    .ForEach(dRule =>
                        dRule.Must(d => !string.IsNullOrEmpty(d.Label) && (!IsNullOrEmpty(d.Id) || !string.IsNullOrEmpty(d.UniqueName)))
                            .WithMessage("Label and at least one of {Id, UniqueName} must be set on NodeDescriptor"));
            }

            private static bool IsNullOrEmpty(Guid? guid)
            {
                return guid == null || guid == Guid.Empty;
            }
        }

        private class RoleExistenceQueryValidator : AbstractValidator<RoleExistenceQuery>
        {
            public RoleExistenceQueryValidator()
            {
                RuleFor(q => q.RoleName).Cascade(CascadeMode.Stop).NotNull().NotEmpty();
                RuleFor(q => q.TenantId).Cascade(CascadeMode.Stop).NotEmpty();
            }
        }

        private class LegacyTenantExistenceQueryValidator : AbstractValidator<LegacyTenantExistenceQuery>
        {
            public LegacyTenantExistenceQueryValidator()
            {
                RuleFor(q => q.TenantType).NotNull().NotEmpty();
                RuleFor(q => q.TenantLegacyIds).NotNull().NotEmpty();
            }
        }

        private class SubjectIntersectionQueryValidator : AbstractValidator<SubjectIntersectionQuery>
        {
            public SubjectIntersectionQueryValidator()
            {
                CascadeMode = CascadeMode.Stop;
                RuleFor(q => q.ActorId).NotNull().NotEmpty();
                RuleFor(q => q.SubjectId).NotNull().NotEmpty();
                RuleFor(q => q.TenantType).NotEmpty().When(q => q.TenantLegacyIds.Any());
                RuleFor(q => q.TenantIds).Empty().When(q => q.TenantType != null && q.TenantLegacyIds.Any());
                RuleFor(q => q.TenantLegacyIds).Empty().When(q => q.TenantIds.Any());
            }
        }
    }
}
