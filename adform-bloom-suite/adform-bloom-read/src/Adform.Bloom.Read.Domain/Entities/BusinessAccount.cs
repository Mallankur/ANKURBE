namespace Adform.Bloom.Read.Domain.Entities;

public class BusinessAccount : BaseEntity
{
    public string? Name { get; set; }
    public int LegacyId { get; set; }
    public int Status { get; set; }
    public int Type { get; set; }
}