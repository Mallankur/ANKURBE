using Adform.Bloom.Runtime.Read.Entities;

namespace Adform.Bloom.Runtime.Integration.Test.Utils
{
    public class RoleExistenceCheckResult
    {
        public ExistenceResult RoleExistsCheck { get; set; } 
    }

    public class NodesExistenceCheckResult
    {
        public ExistenceResult NodesExistCheck { get; set; }
    }

    public class LegacyTenantsExistenceCheckResult
    {
        public ExistenceResult LegacyTenantsExistCheck { get; set; }
    }
}
