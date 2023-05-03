namespace Adform.Bloom.Domain.ValueObjects
{
    public class ValidationResult
    {
        public ErrorCodes Error { get; private set; }

        public bool HasError(ErrorCodes code)
        {
            return (Error & code) != 0;
        }

        public ValidationResult SetError(ErrorCodes code)
        {
            Error |= code;
            return this;
        }
    }
}
