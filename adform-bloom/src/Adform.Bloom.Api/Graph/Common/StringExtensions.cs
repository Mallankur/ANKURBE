using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Adform.Bloom.Domain.Entities;

namespace Adform.Bloom.Api.Graph.Common
{
    public static class StringExtensions
    {
        public static string ToPascalCase(this string str)
        {
            var regex = new Regex(@"(\p{Lu}\p{Ll}+)");
            var match = regex.Match(str);
            if (!match.Success)
                return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str);
            var splitter = match.Groups[0].Value;
            var split = str.Split(splitter);
            if (string.IsNullOrEmpty(split[0]))
                return str;
            var word0 = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(split[0]);
            return $"{word0}{splitter}{split[1]}";
        }

        public static string ToCamelCase(this string str)
        {
            var regex = new Regex(@"(\p{Lu}\p{Ll}+)");
            var match = regex.Match(str);
            if (!match.Success)
                return str;
            var splitter = match.Groups[0].Value;
            var split = str.Split(splitter);
            if (string.IsNullOrEmpty(split[1]))
                return str.ToLowerInvariant();
            var word0 = splitter.ToLowerInvariant();
            return $"{word0}{split[1]}";
        }

        public static string ToDomain(this string str)
        {
            var map = new Dictionary<string, string>
            {
                {nameof(User), nameof(Subject)},
                {nameof(BusinessAccount), nameof(Tenant)}
            };
            return map.Aggregate(str, (current, item) => current.Replace(item.Key, item.Value));
        }
    }
}