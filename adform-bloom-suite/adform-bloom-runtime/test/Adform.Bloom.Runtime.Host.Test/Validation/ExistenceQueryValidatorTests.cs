using System;
using System.Collections.Generic;
using System.Linq;
using Adform.Bloom.Application.Queries;
using Adform.Bloom.Application.Validators;
using Adform.Bloom.Runtime.Infrastructure;
using FluentAssertions;
using FluentResults;
using FluentValidation.Results;
using MediatR;
using Xunit;

namespace Adform.Bloom.Runtime.Api.Test.Validation
{
    public class ExistenceQueryValidatorTests
    {
        private readonly ExistenceQueryValidator _validator;

        public ExistenceQueryValidatorTests()
        {
            _validator = new ExistenceQueryValidator();
        }

        [Theory]
        [MemberData(nameof(TestData), MemberType = typeof(ExistenceQueryValidatorTests))]
        public void Validate_ReturnsCorrectValidationResult(IRequest<Result<bool>> request, string expectedResult)
        {
            var validationResult = Validate(request);

            validationResult.ToString().Should().BeEquivalentTo(expectedResult);
        }

        [Theory]
        [MemberData(nameof(IntersectionTestData), MemberType = typeof(ExistenceQueryValidatorTests))]
        public void ValidateIntersection_ReturnsCorrectValidationResult(SubjectIntersectionQuery request, string expectedResult)
        {
            var lll = request.TenantType != null && request.TenantLegacyIds.Any();
            var validationResult = Validate(request);
            validationResult.ToString().Should().BeEquivalentTo(expectedResult);
        }

        public static TheoryData<IRequest<Result<bool>>, string> TestData()
        {
            var result = new TheoryData<IRequest<Result<bool>>, string>
            {
                // NodeExistence
                {
                    new NodeExistenceQuery(),
                    "'Node Descriptors' must not be empty."
                },
                {
                    new NodeExistenceQuery{ NodeDescriptors = new List<NodeDescriptor> { new NodeDescriptor { }}},
                    "Label and at least one of {Id, UniqueName} must be set on NodeDescriptor"
                },
                {
                    new NodeExistenceQuery{ NodeDescriptors = new List<NodeDescriptor> { new NodeDescriptor { Label = "label"}}},
                    "Label and at least one of {Id, UniqueName} must be set on NodeDescriptor"
                },
                {
                    new NodeExistenceQuery{ NodeDescriptors = new List<NodeDescriptor> { new NodeDescriptor { Label = "label", Id = Guid.Empty } }},
                    "Label and at least one of {Id, UniqueName} must be set on NodeDescriptor"
                },
                {
                    new NodeExistenceQuery{ NodeDescriptors = new List<NodeDescriptor> { new NodeDescriptor { Label = "label", Id = Guid.NewGuid() } }},
                    ""
                },

                // RoleExistence
                {
                    new RoleExistenceQuery { RoleName = "", TenantId = Guid.Empty},
                    $"'Role Name' must not be empty.{Environment.NewLine}'Tenant Id' must not be empty."
                },
                {
                    new RoleExistenceQuery { RoleName = "", TenantId = Guid.NewGuid()},
                    $"'Role Name' must not be empty."
                },
                {
                    new RoleExistenceQuery { RoleName = "roleName", TenantId = Guid.Empty},
                    "'Tenant Id' must not be empty."
                },
                {
                    new RoleExistenceQuery { RoleName = "roleName", TenantId = Guid.NewGuid()},
                    ""
                },

                // LegacyTenantExistence
                {
                    new LegacyTenantExistenceQuery { },
                    $"'Tenant Type' must not be empty.{Environment.NewLine}'Tenant Legacy Ids' must not be empty."
                },
                {
                    new LegacyTenantExistenceQuery { TenantType = "tenantType"},
                    "'Tenant Legacy Ids' must not be empty."
                },
                {
                    new LegacyTenantExistenceQuery { TenantType = "", TenantLegacyIds = new List<int> { 1, 2}},
                    "'Tenant Type' must not be empty."
                },
                {
                    new LegacyTenantExistenceQuery { TenantType = "tenantType", TenantLegacyIds = new List<int> { 1, 2}},
                    ""
                }
            };

            return result;
        }

        public static TheoryData<SubjectIntersectionQuery, string> IntersectionTestData()
        {
            var result = new TheoryData<SubjectIntersectionQuery, string>
            {
                {
                    new SubjectIntersectionQuery(),
                    "'Actor Id' must not be empty."
                },
                {
                    new SubjectIntersectionQuery{ActorId = Guid.Empty},
                    "'Actor Id' must not be empty."
                },
                {
                    new SubjectIntersectionQuery{ActorId = Guid.NewGuid(), SubjectId = new Guid()},
                    "'Subject Id' must not be empty."
                },
                {
                    new SubjectIntersectionQuery{ActorId = Guid.NewGuid(), SubjectId = Guid.NewGuid()},
                    ""
                },
                {
                    new SubjectIntersectionQuery { ActorId = Guid.NewGuid(), SubjectId = Guid.NewGuid(), TenantLegacyIds = new[] {1}},
                    "'Tenant Type' must not be empty."
                },
                {
                    new SubjectIntersectionQuery {
                        ActorId = Guid.NewGuid(),
                        SubjectId = Guid.NewGuid(),
                        TenantLegacyIds = new[] {1},
                        TenantType = Constants.Tenant,
                        TenantIds = new List<Guid> {Guid.Empty}},
                    "'Tenant Ids' must be empty."
                },
                {
                    new SubjectIntersectionQuery {
                        ActorId = Guid.NewGuid(),
                        SubjectId = Guid.NewGuid(),
                        TenantLegacyIds = new[] {1},
                        TenantType = Constants.Tenant,
                        TenantIds = new List<Guid> {Guid.Empty}},
                    "'Tenant Ids' must be empty."
                },
            };
            
            return result;
        }

        private ValidationResult Validate(IRequest<Result<bool>> request)
        {
            return request switch
            {
                NodeExistenceQuery nodeQuery => _validator.Validate(nodeQuery),
                RoleExistenceQuery roleQuery => _validator.Validate(roleQuery),
                LegacyTenantExistenceQuery legacyQuery => _validator.Validate(legacyQuery),
                // unreachable
                _ => new ValidationResult()
            };
        }

        private ValidationResult Validate(SubjectIntersectionQuery request)
        {
            return _validator.Validate(request);
        }
    }
}
