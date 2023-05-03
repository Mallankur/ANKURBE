using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.Common.Test.Commons;
using Adform.Bloom.DataAccess.Extensions;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.DataAccess.Providers.ReadModel;
using Adform.Bloom.Infrastructure.Cache;
using Adform.Bloom.Read.Contracts.BusinessAccount;
using Adform.Bloom.Read.Contracts.User;
using Adform.Bloom.Runtime.Contracts.Services;
using Adform.Ciam.Aerospike;
using Adform.Ciam.Aerospike.Configuration;
using Adform.Ciam.Aerospike.Repository;
using Adform.Ciam.TokenProvider.Configuration;
using Adform.Ciam.TokenProvider.Services;
using Grpc.Net.Client;
using IdentityModel.Client;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Neo4jClient.Transactions;
using ProtoBuf.Grpc.Client;

namespace Adform.Bloom.Integration.Test;

public class TestsFixture : IDisposable
{
    public readonly IConfigurationRoot Configuration;
    public readonly ITransactionalGraphClient GraphClient;
    public readonly IAdminGraphRepository GraphRepository;
    public readonly VisibilityRepositoriesContainer VisibilityRepositoriesContainer;

    public readonly AerospikeConfiguration CacheConfig;
    public readonly AerospikeConnection CacheConnection;
    public readonly IBloomCacheManager CacheManager;
    public readonly IDistributedCache Cache;
    public readonly UserReadModelProvider UserReadModel;
    public readonly BusinessAccountReadModelProvider BusinessAccountReadModel;
    public Dictionary<string, ClaimsPrincipal> BloomApiPrincipal;
    public readonly IBloomRuntimeClient BloomRuntimeClient;

    public readonly OngBuilder OngDB;
    public readonly PsqlBuilder SQL;
    public readonly PrincipalBuilder Identities;

    static TestsFixture()
    {
        GrpcClientFactory.AllowUnencryptedHttp2 = true;
    }

    public TestsFixture()
    {
        try
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "testsettings.json"), false);
#if !DEBUG
            configurationBuilder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "testsettings.testenv.json"), true);
#endif

        Configuration = configurationBuilder.Build();

        //Ongdb
        OngDB = new OngBuilder(Configuration);

            GraphClient = OngDB.GraphClient;
            GraphRepository = OngDB.GraphRepository;
            VisibilityRepositoriesContainer = OngDB.VisibilityRepositoriesContainer;
            OngDB.Clean().GetAwaiter().GetResult();
            OngDB.Seed();

            //Postgresql
            SQL = new PsqlBuilder(Configuration);
            SQL.Clean();
            SQL.SeedUserData();
            SQL.SeedBusinessAccountData();

            //Identity
            Identities = new PrincipalBuilder(Configuration);
            BloomApiPrincipal = Identities.GeneratePrincipals();

            //Cache
            CacheConfig = Configuration.GetSection(Paths.Configuration).Get<AerospikeConfiguration>();
            CacheConnection = new AerospikeConnection(CacheConfig);
            Cache = new AerospikeCache(CacheConnection, CacheConfig);
            CacheManager = new BloomCacheManager(CacheConnection, Options.Create(CacheConfig));
            CacheManager.FlushAsync().GetAwaiter().GetResult();

            var readModelToken = GetBloomReadToken().GetAwaiter().GetResult();
            UserReadModel = InitializeUserReadModelProvider(readModelToken);
            BusinessAccountReadModel = InitializeBusinessAccountReadModelProvider(readModelToken);

            BloomRuntimeClient = new RuntimeClientBuilder(Configuration).Client;
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    public void Dispose()
    {
        SQL.Clean();
        SQL.Dispose();
        OngDB.Clean().GetAwaiter().GetResult();
        OngDB.Dispose();
        CacheManager.FlushAsync().GetAwaiter().GetResult();
    }

    private async Task<string> GetBloomReadToken()
    {
        var scope = Configuration.GetValue<string>("ReadModel:OAuth2:Scopes:0");
        var configuration = Configuration.GetSection("ReadModel:OAuth2").Get<TestOAuth>();
        var oAuthClient = new HttpClient
        {
            BaseAddress = new Uri(configuration.TokenEndpointUri)
        };
        var client = configuration.Clients.First();
        var tokenResponse = await oAuthClient.RequestClientCredentialsTokenAsync(
            new ClientCredentialsTokenRequest
            {
                ClientId = client.ClientId,
                ClientSecret = client.ClientSecret,
                Scope = scope
            });

        if (tokenResponse.IsError)
        {
            throw new Exception(tokenResponse.Error);
        }

        return tokenResponse.AccessToken;
    }

    private UserReadModelProvider InitializeUserReadModelProvider(string token)
    {
        var tokenProviderMock = new Mock<ITokenProvider>();
        tokenProviderMock.Setup(x => x.RequestTokenAsync(It.IsAny<string>(), It.IsAny<string[]>()))
            .ReturnsAsync(token);
        var userService = GrpcChannel
            .ForAddress(
                $"{Configuration.GetValue<string>("ReadModel:Host")}:{Configuration.GetValue<string>("ReadModel:GrpcPort")}")
            .CreateGrpcService<IUserService>();
        var oAuthOptionsMock = new Mock<IOptions<OAuth2Configuration>>();
        oAuthOptionsMock.Setup(x => x.Value).Returns(new OAuth2Configuration());
        var settingsOptionsMock = new Mock<IOptions<BloomReadClientSettings>>();
        settingsOptionsMock.Setup(x => x.Value).Returns(new BloomReadClientSettings());
        return new UserReadModelProvider(userService,
            new CallContextEnhancer(tokenProviderMock.Object, oAuthOptionsMock.Object, settingsOptionsMock.Object));
    }

    private BusinessAccountReadModelProvider InitializeBusinessAccountReadModelProvider(string token)
    {
        var tokenProviderMock = new Mock<ITokenProvider>();
        tokenProviderMock.Setup(x => x.RequestTokenAsync(It.IsAny<string>(), It.IsAny<string[]>()))
            .ReturnsAsync(token);
        var businessAccountService = GrpcChannel
            .ForAddress(
                $"{Configuration.GetValue<string>("ReadModel:Host")}:{Configuration.GetValue<string>("ReadModel:GrpcPort")}")
            .CreateGrpcService<IBusinessAccountService>();
        var oAuthOptionsMock = new Mock<IOptions<OAuth2Configuration>>();
        oAuthOptionsMock.Setup(x => x.Value).Returns(new OAuth2Configuration());
        var settingsOptionsMock = new Mock<IOptions<BloomReadClientSettings>>();
        settingsOptionsMock.Setup(x => x.Value).Returns(new BloomReadClientSettings());
        return new BusinessAccountReadModelProvider(businessAccountService,
            new CallContextEnhancer(tokenProviderMock.Object, oAuthOptionsMock.Object, settingsOptionsMock.Object));
    }

}