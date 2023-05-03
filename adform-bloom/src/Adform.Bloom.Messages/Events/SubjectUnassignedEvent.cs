namespace Adform.Bloom.Messages.Events
{
    public class SubjectUnassignedEvent : AssignmentBaseEvent
    {
        public bool DestroyedConnection { get; set; }
    }
}