using System;
using System.Collections.Generic;
using Adform.Bloom.Domain.ValueObjects;
using Xunit;
using static Adform.Bloom.Common.Test.Graph;

namespace Adform.Bloom.Integration.Test
{
    public static class Scenarios
    {
      
        public static TheoryData<Guid?, Guid, Guid>
            PolicyId_TenantId_SubjectId_For_Role_Assignment_Scenarios()
        {
            return new TheoryData<Guid?, Guid, Guid>
            {
                {Guid.Parse(Policy1), Guid.Parse(Tenant1), Guid.Parse(Subject6)},
                {null, Guid.Parse(Tenant1), Guid.Parse(Subject6)}
            };
        }

        public static TheoryData<Guid?, Guid, Guid, Guid>
            PolicyId_TenantId_SubjectId_SubjectId2_For_Role_Assignment_Scenarios()
        {
            return new TheoryData<Guid?, Guid, Guid, Guid>
            {
                {Guid.Parse(Policy1), Guid.Parse(Tenant1), Guid.Parse(Subject6), Guid.Parse(Subject4)},
                {null, Guid.Parse(Tenant1), Guid.Parse(Subject6), Guid.Parse(Subject4)},
            };
        }

        public static TheoryData<Guid, Guid, Guid, ErrorCodes?, bool>
            PolicyId_TenantId_Error_For_Role_Update_Scenarios()
        {
            return new TheoryData<Guid, Guid, Guid, ErrorCodes?, bool>
            {
                {Guid.Parse(Policy1), Guid.Parse(Subject0), Guid.Parse(Tenant1), null, false},
                {Guid.Parse(Policy1), Guid.Parse(Subject0), Guid.Parse(Tenant1), null, true},
                {
                    Guid.Parse(Policy1), Guid.Parse(Subject6), Guid.Parse(Tenant1), ErrorCodes.SubjectCannotAccessRole,
                    false
                },
            };
        }

        public static TheoryData<string,Guid, Guid, Guid, ErrorCodes?, IReadOnlyCollection<Guid>?, IReadOnlyCollection<Guid>?>
            PolicyId_TenantId_Error_For_RoleFeature_Update_Scenarios()
        {
            return new TheoryData<string,Guid, Guid, Guid, ErrorCodes?, IReadOnlyCollection<Guid>?, IReadOnlyCollection<Guid>?>
            {
                {                    
                    "Case 0",
                    Guid.Parse(Subject0), Guid.Parse(Policy1), Guid.Parse(Tenant1),
                    null, new List<Guid> {Guid.Parse(Feature1)}, new List<Guid> {Guid.Parse(Feature1)}
                },
                {                    
                    "Case 1",
                    Guid.Parse(Subject0), Guid.Parse(Policy1), Guid.Parse(Tenant1),
                    null, null, null
                },
                {                    
                    "Case 2",
                    Guid.Parse(Subject5), Guid.Parse(Policy3), Guid.Parse(Tenant4),
                    ErrorCodes.SubjectCannotAccessFeatures,
                    new List<Guid> {Guid.Parse(Feature2)}, null
                },
                {                    
                    "Case 3",
                    Guid.Parse(Subject5), Guid.Parse(Policy3), Guid.Parse(Tenant4),
                    ErrorCodes.SubjectCannotAccessFeatures,
                    new List<Guid> {Guid.Parse(Feature5)}, new List<Guid> {Guid.Parse(Feature7)}
                },
            };
        }

        public static TheoryData<Guid?, Guid, Guid, bool>
            PolicyId_TenantId_SubjectId_Access_For_Role_Delete_Scenarios()
        {
            return new TheoryData<Guid?, Guid, Guid, bool>
            {
                {Guid.Parse(Policy1), Guid.Parse(Tenant1), Guid.Parse(Subject6), false}
            };
        }
    }
}