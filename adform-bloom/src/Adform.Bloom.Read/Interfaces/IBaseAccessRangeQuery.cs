namespace Adform.Bloom.Read.Interfaces
{
    public interface IBaseAccessRangeQuery<TContext, TFilter, TEntity> : IRangeQuery<TFilter, TEntity>
    {
        public TContext Context { get; }
    }
}