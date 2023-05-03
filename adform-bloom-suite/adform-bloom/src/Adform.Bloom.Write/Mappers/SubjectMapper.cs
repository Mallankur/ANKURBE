using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Write.Commands;

namespace Adform.Bloom.Write.Mappers
{
    public class SubjectMapper : IRequestToEntityMapper<CreateSubjectCommand, Subject>
    {
        public Subject Map(CreateSubjectCommand cmd)
        {
            return new Subject
            {
                Id = cmd.Id,
                Email = cmd.Email,
                IsEnabled = cmd.IsEnabled,
                CreatedAt = cmd.CreatedAt,
                UpdatedAt = cmd.UpdatedAt
            };
        }
    }
}
