namespace Adform.Bloom.Read.Domain.Entities;

public class UserWithCount : User
{
    public int TotalCount { get; set; }
}