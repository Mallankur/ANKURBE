namespace Adform.Bloom.Contracts.Output
{
    public class CypherPaginationResult<T> where T: BaseNodeDto
    {
        public T? Node { get; set; }
        public int TotalCount { get; set; }
    }
}