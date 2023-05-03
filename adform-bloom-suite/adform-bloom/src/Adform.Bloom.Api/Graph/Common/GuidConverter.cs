using System;
using HotChocolate.Utilities;

namespace Adform.Bloom.Api.Graph.Common
{
    public class GuidConverter : IChangeTypeProvider
    {
        public bool TryCreateConverter(Type source, Type target, ChangeTypeProvider root, out ChangeType? converter)
        {
            if (source == typeof(Guid) && target == typeof(string))
            {
                converter = input => input is Guid g
                    ? g.ToString()
                    : default;
                return true;
            }

            converter = null;
            return false;
        }
    }
}