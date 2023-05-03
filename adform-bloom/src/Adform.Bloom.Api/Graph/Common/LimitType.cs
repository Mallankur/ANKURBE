using System;
using Adform.Bloom.Infrastructure;
using HotChocolate;
using HotChocolate.Language;
using HotChocolate.Types;

namespace Adform.Bloom.Api.Graph.Common
{
    public class LimitType : ScalarType<int, IntValueNode>
    {
        public const int MaxValue = 1000;
        public const int MinValue = 1;
        public new const string Name = "Limit";
        public LimitType() : this(Name)
        {

        }
        public LimitType(NameString name, BindingBehavior bind = BindingBehavior.Explicit) : base(name, bind)
        {
        }

        public override IValueNode ParseResult(object? resultValue) => ParseValue(resultValue);
        protected override int ParseLiteral(IntValueNode valueSyntax) => Serialize(valueSyntax);
        protected override IntValueNode ParseValue(int runtimeValue) => new IntValueNode(runtimeValue);

        private new static int Serialize(object value) =>
            value switch
            {
                int intValue => GetIntValue(intValue),
                IntValueNode intValue => GetIntValue(intValue.ToInt32()),
                _ => MaxValue
            };

        private static int GetIntValue(int value) =>
            value is >= MinValue and <= MaxValue
                ? value
                : throw new ArgumentOutOfRangeException(Name.ToCamelCase(),
                    string.Format(ErrorMessages.ArgumentOutOfRange, MinValue, MaxValue));
    }
}