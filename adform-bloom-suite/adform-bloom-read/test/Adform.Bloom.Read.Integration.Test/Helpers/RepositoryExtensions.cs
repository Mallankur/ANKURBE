using System;
using System.Threading.Tasks;
using Adform.Bloom.Read.Application.Abstractions.Persistence;
using Adform.Bloom.Read.Domain.Entities;
using Dapper;

namespace Adform.Bloom.Read.Integration.Test.Helpers;

public static class RepositoryExtensions
{
    public static async Task AddUserAsync(this IRepository<User, UserWithCount> repository, User item)
    {
        item.Id = Guid.NewGuid();
        await repository.Connection.ExecuteAsync(
            "INSERT INTO local_logins (id,two_factor_required,notifications_disabled,status) VALUES(" +
            $"@{nameof(User.Id)},@{nameof(User.TwoFaEnabled)},@{nameof(User.SecurityNotifications)},@{nameof(User.Status)})", item);
        await repository.Connection.ExecuteAsync(
            "INSERT INTO master_accounts (" +
            "id,username,name,email,phone,timezone,locale,first_name,last_name,company,title) VALUES(" +
            $"@{nameof(User.Id)},@{nameof(User.Username)},@{nameof(User.Name)}, @{nameof(User.Email)}," +
            $"@{nameof(User.Phone)},@{nameof(User.Timezone)},@{nameof(User.Locale)},@{nameof(User.FirstName)}," +
            $"@{nameof(User.LastName)},@{nameof(User.Company)},@{nameof(User.Title)})",
            item);
    }

    public static async Task RemoveUserAsync(this IRepository<User, UserWithCount> repository, Guid id)
    {
        await repository.Connection.ExecuteAsync("DELETE FROM local_logins WHERE Id=@Id", new { Id = id });
        await repository.Connection.ExecuteAsync("DELETE FROM master_accounts WHERE Id=@Id", new { Id = id });
    }

    public static Task AddBusinessAccountAsync(this IRepository<BusinessAccount, BusinessAccountWithCount> repository, BusinessAccount item)
    {
        item.Id = Guid.NewGuid();
        var tableType = item.Type switch
        {
            0 => "adform",
            1 => "agencies",
            2 => "publishers",
            3 => "data_providers",
        };
        return repository.Connection.ExecuteAsync(
            $"INSERT INTO public.{tableType} (" +
            $"id,legacy_id,name,status) VALUES(" +
            $"@{nameof(item.Id)}, @{nameof(item.LegacyId)}, @{nameof(item.Name)}, @{nameof(item.Status)})",
            item);
    }

    public static Task RemoveBusinessAccountAsync(this IRepository<BusinessAccount, BusinessAccountWithCount> repository, Guid id, int type)
    {
        var tableType = type switch
        {
            0 => "adform",
            1 => "agencies",
            2 => "publishers",
            3 => "data_providers",
        };
        return repository.Connection.ExecuteAsync($"DELETE FROM {tableType} WHERE Id=@Id", new { Id = id });
    }
}