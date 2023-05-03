using System.Collections.Generic;

namespace Adform.Bloom.DataAccess.Extensions
{
    public static class ReadOnlyCollectionExtensions
    {
        public static IReadOnlyCollection<T> AsReadOnlyCollection<T>(this IReadOnlyCollection<T> col) => col;
    }
}
