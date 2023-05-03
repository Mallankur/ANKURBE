using System;
using System.IO;
using Adform.Bloom.Common.Test;
using Microsoft.Extensions.Configuration;

namespace Seed
{
    class Program
    {
        static void Main(string[] args)
        { 
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "testsettings.json"), false);
#if !DEBUG
            configurationBuilder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "testsettings.testenv.json"), true);
#endif

            var configuration = configurationBuilder.Build();

            //Ongdb
            var ongDB = new OngBuilder(configuration);
            ongDB.Clean().GetAwaiter().GetResult();
            ongDB.Seed();
            
            //Postgresql
            var sQL = new PsqlBuilder(configuration);
            sQL.Clean();
            sQL.SeedUserData();
            sQL.SeedBusinessAccountData();
        }
    }
}