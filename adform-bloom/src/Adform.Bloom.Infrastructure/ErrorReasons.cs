namespace Adform.Bloom.Infrastructure
{
    public static class ErrorReasons
    {
        public const string ConcurrencyFailedReason = "wrongVersion";
        public const string AccessControlValidationFailedReason = "accessControlValidationFailed";
        public const string ConstraintsViolationReason = "constraintsViolated";
    }
}