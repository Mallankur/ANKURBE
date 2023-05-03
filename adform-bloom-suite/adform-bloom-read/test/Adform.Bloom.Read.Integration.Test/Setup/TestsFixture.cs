using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Adform.Bloom.Read.Application.Abstractions.Persistence;
using Adform.Bloom.Read.Contracts.BusinessAccount;
using Adform.Bloom.Read.Contracts.User;
using Adform.Bloom.Read.Domain.Entities;
using Adform.Bloom.Read.Infrastructure.Extensions;
using Adform.Bloom.Read.Infrastructure.Repository;
using Bogus;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf.Grpc.Client;

namespace Adform.Bloom.Read.Integration.Test.Setup;

public class TestsFixture : IDisposable
{
    private readonly IDbConnection _dbConnection;

    public IRepository<User, UserWithCount> UserRepository { get; }
    public IRepository<BusinessAccount, BusinessAccountWithCount> BusinessAccountRepository { get; }
    public IUserService UserClient { get; }
    public IBusinessAccountService BusinessAccountClient { get; }
    public Faker<User> UserFaker { get; set; }
    public Faker<BusinessAccount> BusinessAccountFaker { get; set; }

    public TestsFixture()
    {
        try
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "testsettings.json"), false);
#if !DEBUG
            configurationBuilder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "testsettings.testenv.json"), true);
#endif
            var configuration = configurationBuilder.Build();
            var host = configuration.GetValue<string>("Host");
            GrpcClientFactory.AllowUnencryptedHttp2 = true;
            UserClient = GrpcChannel.ForAddress(host).CreateGrpcService<IUserService>();

            BusinessAccountClient = GrpcChannel.ForAddress(host).CreateGrpcService<IBusinessAccountService>();

            var collection = new ServiceCollection();
            collection.ConfigurationNpgSql(configuration);
            var services = collection.BuildServiceProvider();
            _dbConnection = services.GetRequiredService<IDbConnection>();
            UserRepository = new UserRepository(_dbConnection);
            BusinessAccountRepository = new BusinessAccountRepository(_dbConnection);
            SetupFakers();
            SeedDatabase();
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    public void SetupFakers()
    {
        UserFaker = new Faker<User>()
            .RuleFor(o => o.Name, p => p.Person.FullName)
            .RuleFor(o => o.Username, p => p.Person.UserName)
            .RuleFor(o => o.Phone, p => p.Person.Phone)
            .RuleFor(o => o.Locale, p => p.Locale)
            .RuleFor(o => o.Email, p => p.Person.Email)
            .RuleFor(x => x.FirstName, p => p.Person.FirstName)
            .RuleFor(x => x.LastName, p => p.Person.LastName)
            .RuleFor(x => x.Company, p => p.Person.Company.Name)
            .RuleFor(x => x.Title, p => p.Person.UserName)
            .RuleFor(x => x.TwoFaEnabled, p => p.Random.Bool().OrNull(p))
            .RuleFor(x => x.SecurityNotifications, p => p.Random.Bool().OrNull(p))
            .RuleFor(x => x.Status, p => p.Random.Int(0, 4).OrNull(p));

        BusinessAccountFaker = new Faker<BusinessAccount>()
            .RuleFor(x => x.LegacyId, x => x.Random.Int())
            .RuleFor(x => x.Name, _ => Guid.NewGuid().ToString())
            .RuleFor(x => x.Type, x => x.Random.Int(0, 1))
            .RuleFor(x => x.Status, x => x.Random.Int(0, 2));
    }

    public void SeedDatabase()
    {
        var sqlSchemaMasterAccounts =
            File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Seed", "schemaMasterAccounts.pql"));
        var sqlSchemaTraffickers =
            File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Seed", "schemaTraffickers.pql"));
        var sqlSchemaLocalLogins =
            File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Seed", "schemaLocalLogins.pql"));
        var sqlSchemaAdform =
            File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Seed", "schemaAdform.pql"));
        var sqlSchemaDataProviders =
            File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Seed", "schemaDataProviders.pql"));
        var sqlSchemaPublishers =
            File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Seed", "schemaPublishers.pql"));
        var sqlSchemaAgencies =
            File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Seed", "schemaAgencies.pql"));
        var sqlViewBusinessAccounts =
            File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Seed", "viewBusinessAccounts.pql"));
        var sqlViewUsers = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Seed", "viewUsers.pql"));

        var sqlDataMasterAccounts =
            File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Seed", "initMasterAccounts.pql"));
        var sqlDataTraffickers =
            File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Seed", "initTraffickers.pql"));
        var sqlDataLocalLogins =
            File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Seed", "initLocalLogins.pql"));
        var sqlDataAdform =
            File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Seed", "initAdform.pql"));
        var sqlDataDataProviders =
            File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Seed", "initDataProviders.pql"));
        var sqlDataPublishers =
            File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Seed", "initPublishers.pql"));
        var sqlDataAgencies =
            File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Seed", "initAgencies.pql"));

        _dbConnection.Open();
        using var command = _dbConnection.CreateCommand();

        command.CommandText = sqlSchemaMasterAccounts;
        command.ExecuteNonQuery();
        command.CommandText = sqlSchemaTraffickers;
        command.ExecuteNonQuery();
        command.CommandText = sqlSchemaLocalLogins;
        command.ExecuteNonQuery();
        command.CommandText = sqlSchemaAdform;
        command.ExecuteNonQuery();
        command.CommandText = sqlSchemaDataProviders;
        command.ExecuteNonQuery();
        command.CommandText = sqlSchemaPublishers;
        command.ExecuteNonQuery();
        command.CommandText = sqlSchemaAgencies;
        command.ExecuteNonQuery();
        command.CommandText = sqlViewBusinessAccounts;
        command.ExecuteNonQuery();
        command.CommandText = sqlViewUsers;
        command.ExecuteNonQuery();

        command.CommandText = sqlDataMasterAccounts;
        command.ExecuteScalar();
        command.CommandText = sqlDataTraffickers;
        command.ExecuteScalar();
        command.CommandText = sqlDataLocalLogins;
        command.ExecuteScalar();
        command.CommandText = sqlDataAdform;
        command.ExecuteScalar();
        command.CommandText = sqlDataDataProviders;
        command.ExecuteScalar();
        command.CommandText = sqlDataPublishers;
        command.ExecuteScalar();
        command.CommandText = sqlDataAgencies;
        command.ExecuteScalar();
    }

    public void Clear()
    {
        const string sqlMasterAccounts = "DELETE FROM master_accounts";
        const string sqlTraffickers = "DELETE FROM traffickers";
        const string sqlLocalLogins = "DELETE FROM local_logins";
        const string sqlBaAdform = "DELETE FROM adform";
        const string sqlBaPublishers = "DELETE FROM publishers";
        const string sqlBaAgencies = "DELETE FROM agencies";
        const string sqlDataProviders = "DELETE FROM data_providers";
        using var command = _dbConnection.CreateCommand();
        command.CommandText = sqlMasterAccounts;
        command.ExecuteNonQuery();
        command.CommandText = sqlTraffickers;
        command.ExecuteNonQuery();
        command.CommandText = sqlLocalLogins;
        command.ExecuteNonQuery();
        command.CommandText = sqlBaAdform;
        command.ExecuteNonQuery();
        command.CommandText = sqlBaPublishers;
        command.ExecuteNonQuery();
        command.CommandText = sqlBaAgencies;
        command.ExecuteNonQuery();
        command.CommandText = sqlDataProviders;
        command.ExecuteNonQuery();
    }

    public void Dispose()
    {
        Clear();
    }
}