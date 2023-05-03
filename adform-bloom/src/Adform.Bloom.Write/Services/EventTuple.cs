using System.Collections.Generic;
using Adform.Bloom.Messages.Commands.AssetsReassignment;
using Adform.Bloom.Messages.Events;

namespace Adform.Bloom.Write.Services
{
    public class EventTuple
    {
        public SubjectDisabledEvent? DisabledEvent { get; set; }
        public IEnumerable<SubjectAssignmentEvent> AssignedEvents { get; set; } = new List<SubjectAssignmentEvent>();
        public IEnumerable<SubjectUnassignedEvent> UnassignedEvents { get; set; } = new List<SubjectUnassignedEvent>();
        public IEnumerable<ReassignUserAssetsCommand> ReassignUserAssetsCommands { get; set; } = new List<ReassignUserAssetsCommand>();
    }
}