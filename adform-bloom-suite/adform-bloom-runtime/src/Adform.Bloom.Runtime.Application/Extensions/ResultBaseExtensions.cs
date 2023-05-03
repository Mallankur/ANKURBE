using FluentResults;

namespace Adform.Bloom.Application.Extensions;

public static class ResultBaseExtensions
{
    public static T ThrowIfException<T>(this Result<T> input)
    {
        Exception ex = null;
        var result = input.HasException<Exception>(p =>
        {
            if (p is not Exception) return false;
            ex = p;
            return true;
        });
        return result == false ? input.Value : throw ex;
    }
}