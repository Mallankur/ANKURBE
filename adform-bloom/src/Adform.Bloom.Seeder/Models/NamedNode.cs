using System;
using System.Text.Json.Serialization;

namespace Adform.Bloom.Seeder.Models
{
    public abstract class NamedNode : Domain.Entities.NamedNode
    {
        protected NamedNode(string nodeName, string label, Guid? targetId, Guid? secondTargetId, Guid? childId)
            : base(nodeName)
        {
            Name = $"{nodeName}_{Id}";
            Label = label;
            TargetId = targetId;
            SecondTargetId = secondTargetId;
            ChildId = childId;
        }

        [JsonIgnore] public Guid? TargetId { get; set; }

        [JsonIgnore] public Guid? SecondTargetId { get; set; }

        [JsonIgnore] public Guid? ChildId { get; protected set; }

        public string Label { get; set; }

        public abstract string TypeName { get; }

        public virtual string ToCsv()
        {
            return $"{Id}|{Name}|{Label}|{IsEnabled}|{CreatedAt}|{UpdatedAt}|{TypeName}";
        }

        public static string AddHeaders()
        {
            return "Id:ID|Name|:LABEL|IsEnabled|CreatedAt|UpdatedAt|Type";
        }
    }
}