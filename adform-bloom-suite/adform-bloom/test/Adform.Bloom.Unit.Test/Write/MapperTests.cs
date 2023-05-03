using System;
using System.Security.Claims;
using Adform.Bloom.Write.Commands;
using Adform.Bloom.Write.Mappers;
using Xunit;

namespace Adform.Bloom.Unit.Test.Write
{
    public class MapperTests
    {
        [Theory]
        [InlineData("aaa", "bbb", true, null)]
        [InlineData("xxx", "bbb", false, null)]
        [InlineData("vvv", null, false, null)]
        [InlineData("zzz", "bbb", true, "fcc8ffcb-65af-43f4-9ed6-97601f7601c7")]
        public void GenericMapper_MapsTestCreateCommandToTestEntity(string name, string description, bool isEnabled, string parentId)
        {
            var m = new NamedNodeMapper<TestCreateCommand, TestEntity>();
            var cmd = new TestCreateCommand(new ClaimsPrincipal(),
                name, description, isEnabled, parentId != null ? Guid.Parse(parentId) : (Guid?)null);

            var entity = m.Map(cmd);

            Assert.NotNull(entity);
            Assert.Equal(cmd.Name, entity.Name);
            Assert.Equal(cmd.Description, entity.Description);
            Assert.Equal(cmd.IsEnabled, entity.IsEnabled);
        }

        [Theory]
        [InlineData("fcc8ffcb-65af-43f4-9ed6-97601f7601c7", true)]
        [InlineData("fcc8ffcb-65af-43f4-9ed6-97601f7601c8", false)]
        [InlineData("fcc8ffcb-65af-43f4-9ed6-97601f7601c9", false)]
        [InlineData("fcc8ffcb-65af-43f4-9ed6-97601f7601c0", true)]
        public void SubjectMapper_MapsTestCreateCommandToTestEntity(string subjectid, bool isEnabled)
        {
            var m = new SubjectMapper();
            var cmd = new CreateSubjectCommand(new ClaimsPrincipal(), Guid.Parse(subjectid),$"{subjectid}@test", isEnabled);

            var entity = m.Map(cmd);

            Assert.NotNull(entity);
            Assert.Equal(cmd.Id, entity.Id);
            Assert.Equal(cmd.Email, $"{entity.Id}@test");
            Assert.Equal(cmd.IsEnabled, entity.IsEnabled);
        }
    }
}
