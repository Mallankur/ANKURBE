using System;
using System.IO;
using Adform.Bloom.Common.Test.Commons;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Adform.Bloom.Common.Test
{
    public class PsqlBuilder :IDisposable
    {
        private readonly IConfigurationRoot _configuration;
        public NpgsqlConnection PsqlConnection { get; }

        public PsqlBuilder(IConfigurationRoot configuration)
        {
            _configuration = configuration;
            PsqlConnection = new NpgsqlConnection(ConnectionString(_configuration.GetSection("NpgSql").Get<NpgSqlConfiguration>()));
            PsqlConnection.Open();
        }

        public void SeedUserData()
        {
            var sqlSchemaMasterAccounts =
                File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Seed", "schemaMasterAccounts.pql"));
            var sqlSchemaTraffickers =
                File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Seed", "schemaTraffickers.pql"));
            var sqlSchemaLocalLogins =
                File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Seed", "schemaLocalLogins.pql"));
            var sqlViewUsers = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Seed", "viewUsers.pql"));

            var sqlDataMasterAccounts = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Seed", "initLocalLogins.pql"));
            var sqlDataTraffickers = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Seed", "initTraffickers.pql"));
            var sqlDataLocalLogins = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Seed", "initMasterAccounts.pql"));

            using var command = PsqlConnection.CreateCommand();
            command.CommandText = sqlSchemaMasterAccounts;
            command.ExecuteNonQuery();
            command.CommandText = sqlSchemaTraffickers;
            command.ExecuteNonQuery();
            command.CommandText = sqlSchemaLocalLogins;
            command.ExecuteScalar();
            command.CommandText = sqlViewUsers;
            command.ExecuteNonQuery();

            command.CommandText = sqlDataMasterAccounts;
            command.ExecuteNonQuery();
            command.CommandText = sqlDataTraffickers;
            command.ExecuteNonQuery();
            command.CommandText = sqlDataLocalLogins;
            command.ExecuteScalar();
        }

        public void SeedBusinessAccountData()
        {
            var sqlSchemaAdform = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Seed", "schemaAdform.pql"));
            var sqlSchemaAgencies = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Seed", "schemaAgencies.pql"));
            var sqlSchemaDataProviders = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Seed", "schemaDataProviders.pql"));
            var sqlSchemaPublishers = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Seed", "schemaPublishers.pql"));
            var sqlViewBusinessAccounts = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Seed", "viewBusinessAccounts.pql"));

            var sqlDataAdform = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Seed", "initAdform.pql"));
            var sqlDataDataProviders = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Seed", "initDataProviders.pql"));
            var sqlDataAgencies = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Seed", "initAgencies.pql"));
            var sqlDataPublishers = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Seed", "initPublishers.pql"));

            using var command = PsqlConnection.CreateCommand();
            command.CommandText = sqlSchemaAdform;
            command.ExecuteNonQuery();
            command.CommandText = sqlSchemaAgencies;
            command.ExecuteNonQuery();
            command.CommandText = sqlSchemaDataProviders;
            command.ExecuteNonQuery();
            command.CommandText = sqlSchemaPublishers;
            command.ExecuteNonQuery();
            command.CommandText = sqlViewBusinessAccounts;
            command.ExecuteNonQuery();

            command.CommandText = sqlDataAdform;
            command.ExecuteScalar();
            command.CommandText = sqlDataAgencies;
            command.ExecuteScalar();
            command.CommandText = sqlDataDataProviders;
            command.ExecuteScalar();
            command.CommandText = sqlDataPublishers;
            command.ExecuteScalar();
        }


        public void Clean()
        {
            using var command = PsqlConnection.CreateCommand();
            command.CommandText = "drop view if exists public.users";
            command.ExecuteNonQuery();
            command.CommandText = "drop table if exists public.master_accounts";
            command.ExecuteNonQuery();
            command.CommandText = "drop table if exists public.traffickers";
            command.ExecuteNonQuery();
            command.CommandText = "drop table if exists public.local_logins";
            command.ExecuteNonQuery();
            command.CommandText = "drop view if exists public.business_accounts";
            command.ExecuteNonQuery();
            command.CommandText = "drop table if exists public.adform";
            command.ExecuteNonQuery();
            command.CommandText = "drop table if exists public.publishers";
            command.ExecuteNonQuery();
            command.CommandText = "drop table if exists public.data_providers";
            command.ExecuteNonQuery();
            command.CommandText = "drop table if exists public.agencies";
            command.ExecuteNonQuery();
        }


        private static string ConnectionString(dynamic options)
        {
            return $"Host={options.Host};User ID={options.UserName};Password={options.Password};" +
                   $"Port={options.Port};Database={options.Database};Pooling=true;";
        }

        public void Dispose()
        {
            PsqlConnection?.Dispose();
        }
    }
}