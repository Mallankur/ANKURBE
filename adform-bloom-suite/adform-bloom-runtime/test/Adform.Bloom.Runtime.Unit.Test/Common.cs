using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Adform.Bloom.Application.Exceptions;
using Adform.Bloom.Application.Queries;
using Adform.Bloom.Client.Contracts;
using Adform.Bloom.Runtime.Read.Entities;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using Adform.Ciam.SharedKernel.Core;
using CorrelationId.DependencyInjection;
using CorrelationId.HttpClient;
using FluentAssertions;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace Adform.Bloom.Runtime.Read.Test
{
    public static class Common
    {
        public static TheoryData<SubjectRuntimeQuery, Exception?> Validation()
        {
            var inputFaker = InputFaker();
            var queries = inputFaker.Generate(4);

            var data = new TheoryData<SubjectRuntimeQuery, Exception>();
            queries[0].TenantIds = new List<Guid>();
            queries[0].TenantType = "";
            data.Add(queries[0],
                new BadRequestException(ErrorReasons.ConstraintsViolationReason,
                    ErrorMessages.LegacyIdsMissingTenantType));
            data.Add(queries[1],
                new BadRequestException(ErrorReasons.ConstraintsViolationReason,
                    ErrorMessages.LegacyIdsAndTenantIdsCannotBeSet));
            queries[2].TenantIds = new List<Guid>();
            data.Add(queries[2], null);
            queries[3].TenantLegacyIds = new List<int>();
            data.Add(queries[3], null);
            return data;
        }

        public static TheoryData<IEnumerable<RuntimeResult>> Result()
        {
            var outputFaker = OutputFaker();
            var data = new TheoryData<IEnumerable<RuntimeResult>>();
            var results = outputFaker.Generate(3);
            for (var i = 0; i < 10; i++)
            {
                data.Add(results);
            }

            return data;
        }

        public static TheoryData<TestData> Data()
        {
            var inputFaker = InputFaker();
            var outputFaker = OutputFaker();
            var fakeData = new Faker<TestData>();
            fakeData.RuleFor(o => o.Input, p => inputFaker.Generate());
            fakeData.RuleFor(o => o.Output, p => outputFaker.Generate(3));
            var data = new TheoryData<TestData>();
            var results = fakeData.Generate(10).ToList();
            results.ForEach(p => data.Add(p));
            return data;
        }

        public static TheoryData<IntersectionTestData> IntersectionData()
        {
            var inputFaker = IntersectionInputFaker();
            var outputFaker = OutputFaker();
            var fakeData = new Faker<IntersectionTestData>();
            fakeData.RuleFor(o => o.Input, p => inputFaker.Generate());
            fakeData.RuleFor(o => o.Output, p => outputFaker.Generate(3));
            var data = new TheoryData<IntersectionTestData>();
            var results = fakeData.Generate(10).ToList();
            results.ForEach(p => data.Add(p));
            return data;
        }

        public class TestData
        {
            public SubjectRuntimeQuery Input { get; set; }
            public IEnumerable<RuntimeResult> Output { get; set; }
        }

        public class IntersectionTestData
        {
            public SubjectIntersectionQuery Input { get; set; }
            public IEnumerable<RuntimeResult> Output { get; set; }
        }

        private static Faker<SubjectRuntimeQuery> InputFaker()
        {
            var inputFaker = new Faker<SubjectRuntimeQuery>();
            inputFaker.RuleFor(p => p.SubjectId, o => Guid.NewGuid());
            inputFaker.RuleFor(p => p.TenantIds, o =>
            {
                return Enumerable.Range(0, 3)
                    .Select(n => o.Random.Guid())
                    .ToList();
            });
            inputFaker.RuleFor(p => p.TenantType, o => o.Hacker.Noun());

            inputFaker.RuleFor(p => p.TenantLegacyIds, o =>
            {
                return Enumerable.Range(0, 3)
                    .Select(n => o.Random.Int(0, 1000))
                    .ToList();
            });
            inputFaker.RuleFor(p => p.PolicyNames, o =>
            {
                return Enumerable.Range(0, 3)
                    .Select(n => o.Random.Word())
                    .ToList();
            });
            inputFaker.RuleFor(p => p.InheritanceEnabled, o => o.Random.Bool());
            return inputFaker;
        }

        private static Faker<SubjectIntersectionQuery> IntersectionInputFaker()
        {
            var inputFaker = new Faker<SubjectIntersectionQuery>();
            inputFaker.RuleFor(p => p.ActorId, o => Guid.NewGuid());
            inputFaker.RuleFor(p => p.SubjectId, o => Guid.NewGuid());
            inputFaker.RuleFor(p => p.TenantIds, o =>
            {
                return Enumerable.Range(0, 3)
                    .Select(n => o.Random.Guid())
                    .ToList();
            });
            inputFaker.RuleFor(p => p.TenantType, o => o.Hacker.Noun());

            inputFaker.RuleFor(p => p.TenantLegacyIds, o =>
            {
                return Enumerable.Range(0, 3)
                    .Select(n => o.Random.Int(0, 1000))
                    .ToList();
            });
            inputFaker.RuleFor(p => p.PolicyNames, o =>
            {
                return Enumerable.Range(0, 3)
                    .Select(n => o.Random.Word())
                    .ToList();
            });
            inputFaker.RuleFor(p => p.InheritanceEnabled, o => o.Random.Bool());
            return inputFaker;
        }

        private static Faker<RuntimeResult> OutputFaker()
        {
            var tenantTypes = new List<string>() {"Tenant"};
            var roles = new List<string>() {"role0", "role1", "role2", "role3"};
            var permissions = new List<string>()
            {
                "permission0", "permission1", "permission2", "permission3", "permission4", "permission5", "permission6"
            };
            var tenantIds = Enumerable.Range(0, 3)
                .Select(n => new Bogus.Faker().Random.Guid())
                .ToList();
            var outputFaker = new Faker<RuntimeResult>();
            outputFaker.RuleFor(p => p.TenantId, o => o.PickRandom(tenantIds));
            outputFaker.RuleFor(p => p.TenantLegacyId, o => o.PickRandom(0, 3));
            outputFaker.RuleFor(p => p.TenantType, o => o.PickRandom(tenantTypes));
            outputFaker.RuleFor(p => p.TenantName, o => o.Company.CompanyName());
            outputFaker.RuleFor(p => p.Roles, o => o.PickRandom(roles, 2).ToList());
            outputFaker.RuleFor(p => p.Permissions, o => o.PickRandom(permissions, 3).ToList());
            return outputFaker;
        }

        public static T Matches<T>(T b)
        {
            Func<T, bool> matcher = (t) =>
            {
                try
                {
                    t.Should().BeEquivalentTo(b);
                    return true;
                }
                catch
                {
                    return false;
                }
            };

            return It.Is<T>(t => matcher(t));
        }



        public static WireMockServer ConfigureAndStartMockServer(string requestPath, object objectToReturn)
        {
            var server = WireMockServer.Start();

            server.Given(
                    Request.Create()
                        .WithPath(requestPath))
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(200)
                        .WithBodyAsJson(objectToReturn));

            return server;
        }

        public static ServiceProvider CreateServiceProviderForRuntimeClient(string clientName, string serverUrl)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddCorrelationId();
            serviceCollection.AddHttpClient(clientName,
                    httpClient => { httpClient.BaseAddress = new Uri(serverUrl); })
                .AddCorrelationIdForwarding();

            return serviceCollection.BuildServiceProvider();
        }
    }
}