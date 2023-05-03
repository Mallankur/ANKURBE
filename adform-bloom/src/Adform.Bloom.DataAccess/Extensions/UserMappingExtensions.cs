using System.Collections.Generic;
using System.Linq;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.Read.Contracts.User;

namespace Adform.Bloom.DataAccess.Extensions
{
    public static class UserMappingExtensions
    {
        public static User MapFromReadModel(this UserResult node)
        {
            return new User
            {
                Id = node.Id,
                Username = node.Username,
                Name = node.Name,
                Email = node.Email,
                FirstName = node.FirstName,
                LastName = node.LastName,
                Company = node.Company,
                Phone = node.Phone,
                Locale = node.Locale,
                Timezone = node.Timezone,
                TwoFaEnabled = node.TwoFaEnabled,
                SecurityNotificationsEnabled = node.SecurityNotifications,
                Status = node.Status
            };
        }

        public static IReadOnlyCollection<User> MapFromReadModel(this IEnumerable<UserResult> nodes)
        {
            return nodes.Select(n => n.MapFromReadModel()).ToArray();
        }
    }
}