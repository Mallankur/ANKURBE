using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Adform.Bloom.Write.Commands
{
    public abstract class NamedCreateCommand<TEntity> : BaseCreateCommand<TEntity>
    {
        private const string Pattern = "^[a-zA-Z0-9\\s\\-_/]*$";

        protected NamedCreateCommand(ClaimsPrincipal principal, string name, string description = "", bool isEnabled = true)
            : base(principal, isEnabled)
        {
            Name = name;
            Description = description;
        }

        [Required]
        [StringLength(256)]
        [RegularExpression(Pattern)]
        public string Name { get; set; }

        [RegularExpression(Pattern)]
        public string Description { get; set; }

    }
}