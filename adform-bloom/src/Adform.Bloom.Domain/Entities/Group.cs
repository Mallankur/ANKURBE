using Newtonsoft.Json;
using System.IO;

namespace Adform.Bloom.Domain.Entities
{
    public class Group : NamedNode
    {
        private static readonly JsonSerializer Serializer = new JsonSerializer();

        public Group() : base(string.Empty)
        {
        }

        public Group(string name) : base(name)
        {
        }

        [JsonIgnore] public override long CreatedAt { get; set; }

        [JsonIgnore] public override long UpdatedAt { get; set; }

        public string ToCypherWithoutQuotes()
        {
            return SerializeWithoutPropertyDoubleQuotes(this);
        }

        private static string SerializeWithoutPropertyDoubleQuotes(object obj)
        {
            using var stringWriter = new StringWriter();
            using var writer = new JsonTextWriter(stringWriter) {QuoteName = false};
            Serializer.Serialize(writer, obj);
            return stringWriter.ToString();
        }
    }
}