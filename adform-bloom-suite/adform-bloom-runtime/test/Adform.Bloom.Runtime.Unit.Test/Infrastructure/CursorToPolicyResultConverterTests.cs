using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adform.Bloom.Runtime.Infrastructure.Services;
using Adform.Bloom.Runtime.Read.Entities;
using FluentAssertions;
using Moq;
using Neo4j.Driver;
using Xunit;

namespace Adform.Bloom.Runtime.Read.Test.Infrastructure
{
    public class CursorToPolicyResultConverterTests
    {
        private readonly Mock<IDriver> _driver;
        private readonly ICursorToResultConverter _converter;

        public CursorToPolicyResultConverterTests()
        {
            _driver = new Mock<IDriver>();
            _driver.SetupAllProperties();
            _converter = new CursorToResultConverter();
        }


        [Theory]
        [MemberData(nameof(Common.Result), MemberType = typeof(Common))]
        public async Task ConvertToRuntimeResultAsync_Returns_Expected_Result(IEnumerable<RuntimeResult> data)
        {
            // Arrange
            var session = new Mock<IAsyncSession>(MockBehavior.Loose);
            var cursor = BuildRuntimeCursor(data);
            session.Setup(o => o.RunAsync(It.IsAny<string>())).ReturnsAsync(cursor.Object);
            _driver.Setup(o => o.AsyncSession(It.IsAny<Action<SessionConfigBuilder>>())).Returns(session.Object);

            // Act
            var result = await _converter.ConvertToRuntimeResultAsync(cursor.Object);

            // Assert
            var expectedRoles = data.SelectMany(p => p.Roles).OrderBy(i => i).ToList();
            var actualRoles = result.SelectMany(p => p.Roles).OrderBy(i => i).ToList();
            var expectedPermission = data.SelectMany(p => p.Permissions).OrderBy(i => i).ToList();
            var actualPermission = result.SelectMany(p => p.Permissions).OrderBy(i => i).ToList();
            Assert.Equal(expectedRoles.Count, actualRoles.Count);
            Assert.Equal(expectedRoles, actualRoles);
            Assert.Equal(expectedPermission.Count, actualPermission.Count);
            Assert.Equal(expectedPermission, actualPermission);
        }

        [Fact]
        public async Task ConvertToTenantIdAsync_Returns_Expected_Result()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var session = new Mock<IAsyncSession>(MockBehavior.Loose);
            var cursor = BuildTenantIdCursor(tenantId);
            session.Setup(o => o.RunAsync(It.IsAny<string>())).ReturnsAsync(cursor.Object);
            _driver.Setup(o => o.AsyncSession(It.IsAny<Action<SessionConfigBuilder>>())).Returns(session.Object);

            // Act
            var result = await _converter.ConvertToTenantIdAsync(cursor.Object);

            // Assert
            Assert.Equal(tenantId, result);
        }

        [Fact]
        public async Task ConvertToCountResult_ReturnsExpectedResult()
        {
            const int countValue = 12;
            var cursor = new Mock<IResultCursor>();
            var record = new Mock<IRecord>();
            record.Setup(r => r[0]).Returns(countValue);
            cursor.SetupSequence(x => x.FetchAsync()).ReturnsAsync(true).ReturnsAsync(false);
            cursor.Setup(c => c.Current).Returns(record.Object);

            var result = await _converter.ConvertToCountResult(cursor.Object);

            result.Should().Be(countValue);
        }

        private Mock<IResultCursor> BuildRuntimeCursor(IEnumerable<RuntimeResult> result)
        {
            var cursor = new Mock<IResultCursor>();
            cursor.SetupSequence(x => x.FetchAsync()).ReturnsAsync(true).ReturnsAsync(true).ReturnsAsync(true)
                .ReturnsAsync(false);
            var records = new List<IRecord>();
            foreach (var res in result)
            {
                var record = new Mock<IRecord>();
                var dictionary = new Dictionary<string, object>();
                dictionary["TenantLegacyId"] = res.TenantLegacyId;
                dictionary["TenantName"] = res.TenantName;
                dictionary["TenantId"] = res.TenantId;
                dictionary["TenantType"] = res.TenantType;
                dictionary["Roles"] = res.Roles;
                dictionary["Permissions"] = res.Permissions;
                record.Setup(r => r.Values).Returns(dictionary);
                records.Add(record.Object);
            }

            cursor.SetupSequence(x => x.Current).Returns(records[0]).Returns(records[1]).Returns(records[2]);
            return cursor;
        }

        private Mock<IResultCursor> BuildTenantIdCursor(Guid result)
        {
            var cursor = new Mock<IResultCursor>();
            cursor.SetupSequence(x => x.FetchAsync()).ReturnsAsync(true).ReturnsAsync(false);

            var record = new Mock<IRecord>();
            var dictionary = new Dictionary<string, object>();
            dictionary["TenantId"] = result;
            record.Setup(r => r.Values).Returns(dictionary);

            cursor.SetupSequence(x => x.Current).Returns(record.Object);
            return cursor;
        }
    }
}