using Adform.Bloom.Application.Queries;

namespace Adform.Bloom.Application.Validators
{
    public interface IValidateQuery
    {
        void Validate(SubjectRuntimeQuery query);
    }
}