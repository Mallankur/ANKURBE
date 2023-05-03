namespace Adform.Bloom.Infrastructure
{
    public static class ErrorMessages
    {
        public const string PermissionIsNotInFeature =
            "A given permission cannot be assign to a role due to features filtering.";

        public const string SubjectCannotAccessRole = "The subject of the token does not have access to a role in the given tenant context.";

        public const string SubjectCannotAccessSubject =
            "The subject of the token does not have access to an another subject.";

        public const string SubjectCannotExceedRoleAssignmentLimit =
            "Cannot exceed the limit of role assignments to a subject.";

        public const string SubjectCannotModifyAssignmentsForHimself =
            "The subject of the token cannot modify it's own assignments.";

        public const string SubjectCannotAccessTenant =
            "The subject of the token does not have access to a tenant.";

        public const string SubjectCannotAccessEntity =
            "The subject of the token does not have access to a given entity.";

        public const string SubjectCannotDeleteEntity =
            "The subject of the token cannot delete an entity due to lack of access.";

        public const string ConcurrencyCannotUpdateEntity =
            "The entity couldn't be updated.";

        public const string SubjectCannotAccessFeatures = "The subject of the token does not have access to feature(s)";
        public const string PermissionCannotBeAssignedToFeature = "The permission cannot be assigned to the feature.";

        public const string FeatureCannotBeCoDependentOnFeature =
            "The feature cannot be co-dependentOn on the feature due to circural dependency.";

        public const string FeatureDependencyMissing =
            "The feature dependencies are not fulfilled.";

        public const string ArgumentOutOfRange = "Value must be between {0} and {1}";
    }
}