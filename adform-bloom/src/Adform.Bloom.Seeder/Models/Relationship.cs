namespace Adform.Bloom.Seeder.Models
{
    public class Relationship<TKey>
    {
        public Relationship(TKey from, TKey to, string name)
        {
            From = from;
            To = to;
            Name = name;
        }

        public TKey From { get; set; }
        public TKey To { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return $"{To}|{Name}|{From}";
        }
    }
}