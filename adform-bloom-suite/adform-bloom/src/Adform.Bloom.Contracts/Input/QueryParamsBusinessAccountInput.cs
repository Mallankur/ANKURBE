namespace Adform.Bloom.Contracts.Input
{
    public class QueryParamsBusinessAccountInput : QueryParamsTenantIdsInput
    {
        public int? BusinessAccountType { get; set; }
    }
}