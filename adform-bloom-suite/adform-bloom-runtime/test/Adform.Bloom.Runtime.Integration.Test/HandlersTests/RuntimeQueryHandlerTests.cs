using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Application.Handlers;
using Adform.Bloom.Application.Queries;
using Adform.Bloom.Application.Validators;
using Adform.Bloom.Runtime.Read.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Adform.Bloom.Runtime.Integration.Test.HandlersTests
{
    [Collection(nameof(Collection))]
    public class RuntimeQueryHandlerTests
    {
        private readonly TestsFixture _fixture;
        private readonly RuntimeQueryHandler _queryHandler;

        public RuntimeQueryHandlerTests(TestsFixture fixture)
        {
            _fixture = fixture;
            _queryHandler = new RuntimeQueryHandler(new Mock<ILogger<RuntimeQueryHandler>>().Object,
                _fixture.TenantProvider,
                _fixture.RuntimeProvider,
                new ValidateQuery());
        }

        [Theory]
        [MemberData(nameof(Common.QueryInput), MemberType = typeof(Common))]
        public async Task Graph1_SubjectEvaluation_OnCorrectInput_Returns_PolicyResult(SubjectRuntimeQuery data, IReadOnlyList<RuntimeResult> expectedResult)
        {
            var result = (await _queryHandler.Handle(data, CancellationToken.None)).ToArray();

            Assert.Equal(expectedResult.Select(p => p.TenantId).OrderBy(i => i), result.Select(p => p.TenantId).OrderBy(i => i));
            Assert.Equal(expectedResult.Select(p => p.TenantName).OrderBy(i => i), result.Select(p => p.TenantName).OrderBy(i => i));
            Assert.Equal(expectedResult.Select(p => p.TenantType).OrderBy(i => i), result.Select(p => p.TenantType).OrderBy(i => i));
            Assert.Equal(expectedResult.Select(p => p.TenantLegacyId).OrderBy(i => i), result.Select(p => p.TenantLegacyId).OrderBy(i => i));
            Assert.Equal(expectedResult.SelectMany(o => o.Roles).OrderBy(p => p), result.SelectMany(o => o.Roles).OrderBy(p => p));
            Assert.Equal(expectedResult.SelectMany(o => o.Permissions).OrderBy(p => p), result.SelectMany(o => o.Permissions).OrderBy(p => p));
        }

        [Theory]
        [MemberData(nameof(Common.QueryInput2), MemberType = typeof(Common))]
        public async Task Graph2_SubjectEvaluation_OnCorrectInput_Returns_PolicyResult(SubjectRuntimeQuery data, IReadOnlyList<RuntimeResult> expectedResult)
        {
            var result = (await _queryHandler.Handle(data, CancellationToken.None)).ToArray();

            Assert.Equal(expectedResult.Select(p => p.TenantId).OrderBy(i => i), result.Select(p => p.TenantId).OrderBy(i => i));
            Assert.Equal(expectedResult.Select(p => p.TenantName).OrderBy(i => i), result.Select(p => p.TenantName).OrderBy(i => i));
            Assert.Equal(expectedResult.Select(p => p.TenantType).OrderBy(i => i), result.Select(p => p.TenantType).OrderBy(i => i));
            Assert.Equal(expectedResult.Select(p => p.TenantLegacyId).OrderBy(i => i), result.Select(p => p.TenantLegacyId).OrderBy(i => i));
            Assert.Equal(expectedResult.SelectMany(o => o.Roles).OrderBy(p => p), result.SelectMany(o => o.Roles).OrderBy(p => p));
            Assert.Equal(expectedResult.SelectMany(o => o.Permissions).OrderBy(p => p), result.SelectMany(o => o.Permissions).OrderBy(p => p));
        }
    }
}
