namespace Adform.Bloom.Contracts.Output
{
    public class RolePaginationResult : CypherPaginationResult<Role>
    {
        public int Type { get; set; }
    }
}