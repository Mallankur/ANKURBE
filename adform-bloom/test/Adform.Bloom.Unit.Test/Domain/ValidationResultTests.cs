using System;
using System.Collections.Generic;
using System.Linq;
using Adform.Bloom.Domain.ValueObjects;
using Xunit;

namespace Adform.Bloom.Unit.Test.Domain
{
    public class ValidationResultTests
    {
        private static readonly ErrorCodes[] Flags = Enum.GetValues(typeof(ErrorCodes)).Cast<ErrorCodes>()
            .ToArray();

        [Theory]
        [MemberData(nameof(GetFlags))]
        public void ValidationResult_Initialized_Contains_No_Errors(ErrorCodes flag)
        {
            var result = new ValidationResult();
            Assert.False(result.HasError(flag));
        }

        [Fact]
        public void ErrorCodes_Dont_SumUp_To_Each_Other()
        {
            var set = new HashSet<double>();

            foreach (var flag in Flags)
            {
                var result = Math.Log2((int) flag);
                Assert.False(result > (int) result);
                Assert.True(set.Add(result));
            }
        }

        [Fact]
        public void ValidationResult_SetError_And_HasError_Work_Correctly()
        {
            var set = new HashSet<int>();
            var random = new Random(Guid.NewGuid().GetHashCode());
            var result = new ValidationResult();
            var firstIndex = GetIndex();

            do
            {
                var secondIndex = GetIndex();

                result.SetError(Flags[firstIndex]);
                result.SetError(Flags[secondIndex]);

                Assert.True(result.HasError(Flags[firstIndex]));
                Assert.True(result.HasError(Flags[secondIndex]));
            } while (set.Count < Flags.Length);
            
            int GetIndex()
            {
                int index;

                do
                {
                    index = random.Next(0, Flags.Length);
                } while (!set.Add(index));

                return index;
            }
        }

        public static TheoryData<ErrorCodes> GetFlags()
        {
            var data = new TheoryData<ErrorCodes>();

            for (var i = 0; i < Flags.Length; i++)
            {
                data.Add(Flags[i]);
            }

            return data;
        }
    }
}