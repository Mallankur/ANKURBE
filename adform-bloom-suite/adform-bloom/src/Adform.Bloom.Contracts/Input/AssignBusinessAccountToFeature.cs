using System;

namespace Adform.Bloom.Contracts.Input
{
    public class AssignBusinessAccountToFeature
    {
        public AssignBusinessAccountToFeature()
        {
        }

        public AssignBusinessAccountToFeature(Guid featureId, Guid businessAccountId, LinkOperation operation)
        {
            FeatureId = featureId;
            BusinessAccountId = businessAccountId;
            Operation = operation;
        }

        public Guid FeatureId { get; set; }
        public Guid BusinessAccountId { get; set; }
        public LinkOperation Operation { get; set; }
    }
}