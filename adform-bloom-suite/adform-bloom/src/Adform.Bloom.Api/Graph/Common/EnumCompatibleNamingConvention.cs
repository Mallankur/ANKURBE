using System;
using Adform.Bloom.Infrastructure.Extensions;
using HotChocolate;
using HotChocolate.Types.Descriptors;

namespace Adform.Bloom.Api.Graph.Common
{
    public class EnumCompatibleNamingConvention
        : DefaultNamingConventions
    {
        public override NameString GetEnumValueName(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return value.ToString()?.ToLowerFirstCharacter()!;
        }
    }
}