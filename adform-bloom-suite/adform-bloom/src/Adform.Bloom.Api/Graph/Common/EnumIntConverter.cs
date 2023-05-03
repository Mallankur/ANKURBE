using HotChocolate.Utilities;
using System;

namespace Adform.Bloom.Api.Graph.Common
{
    public class EnumIntConverter
   : IChangeTypeProvider
    {
        public bool TryCreateConverter(Type source, Type target, ChangeTypeProvider root, out ChangeType? converter)
        {
            if (source == typeof(int) && target.IsEnum)
            {
                converter = input => Enum.ToObject(target, source);
                return true;
            }


            if (source.IsEnum && target == typeof(int))
            {
                converter = input => (int)input;
                return true;
            }

            converter = null;
            return false;
        }
    }
}
