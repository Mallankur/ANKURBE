using System;

namespace Adform.Bloom.DataAccess
{
    public class BloomDataInconsistencyException : Exception
    {
        public BloomDataInconsistencyException(string message):
            base(message)
        {
        }
    }
}