using System;

namespace Adform.Bloom.Contracts.Input
{
    public class AssignPermissionToFeature
    {
        public AssignPermissionToFeature()
        {
        }

        public AssignPermissionToFeature(Guid permissionId, Guid featureId, LinkOperation operation)
        {
            PermissionId = permissionId;
            FeatureId = featureId;
            Operation = operation;
        }

        public Guid PermissionId { get; set; }
        public Guid FeatureId { get; set; }
        public LinkOperation Operation { get; set; }
    }
}