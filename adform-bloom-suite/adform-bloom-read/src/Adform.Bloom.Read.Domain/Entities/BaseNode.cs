using System;
using System.ComponentModel.DataAnnotations;

namespace Adform.Bloom.Read.Domain.Entities;

public abstract class BaseEntity
{
    [Key] public Guid Id { get; set; }
}