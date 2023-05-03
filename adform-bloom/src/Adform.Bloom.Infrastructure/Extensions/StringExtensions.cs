namespace Adform.Bloom.Infrastructure.Extensions
{
    public static class StringExtensions
    {
        public static string ToLowerFirstCharacter(this string baseString)
        {
            return string.Create(baseString.Length, baseString, (c, s) =>
            {
                c[0] = char.ToLower(s[0]);
                for (var i = 1; i < s.Length; i++)
                {
                    c[i] = s[i];
                }
            });
        }
    }
}