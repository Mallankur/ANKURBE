using Adform.Bloom.Read.Contracts.User;

namespace Adform.Bloom.Contracts.Output
{
    public class User : BaseNodeDto
    {
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Timezone { get; set; } = string.Empty;
        public string Locale { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public bool TwoFaEnabled { get; set; }
        public bool TncReadAndAccepted { get; set; } = false;
        public bool SecurityNotificationsEnabled { get; set; }
        public UserStatus Status { get; set; }
        public UserType Type { get; set; }
    }
}