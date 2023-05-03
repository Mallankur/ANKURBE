using Adform.Bloom.Infrastructure.Audit;
using Adform.Ciam.OngDb.Core.Interfaces;
using Adform.Ciam.SharedKernel.Extensions;
using MediatR;
using Moq;
using System;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Domain.Entities;
using Xunit;

namespace Adform.Bloom.Unit.Test.Write
{
    public static class Extensions
    {
        public static (TestEntity entity, TestCreateCommand command) SetupTestCreateCommand(
            this Mock<IAdminGraphRepository> mock,
            ClaimsPrincipal claimsPrincipal,
            bool createItemThrowsException = false,
            bool createLinkThrowsException = false,
            bool withParentId = true,
            bool parentItemExist = true,
            TestEntity initialEntity = null)
        {
            var entity = initialEntity ?? new TestEntity("aaa");
            var cmd = new TestCreateCommand(claimsPrincipal, 
                entity.Name, entity.Description, true, withParentId ? Guid.NewGuid() : (Guid?) null);
            if (parentItemExist)
            {
                mock.Setup(r => r.GetCountAsync(It.IsAny<Expression<Func<TestEntity, bool>>>(),
                    It.IsAny<string>(),
                    It.IsAny<string>())).ReturnsAsync(1);
            }

            if (createItemThrowsException)
                mock.Setup(r => r.CreateNodeAsync(It.IsAny<TestEntity>())).ThrowsAsync(new Exception());
            else
                mock.Setup(r => r.CreateNodeAsync(It.IsAny<TestEntity>())).ReturnsAsync(entity);

            if (createLinkThrowsException)
                mock.Setup(r => r.CreateRelationshipAsync(
                    It.IsAny<Expression<Func<TestEntity, bool>>>(),
                    It.IsAny<Expression<Func<TestEntity, bool>>>(),
                    It.IsAny<ILink>())).ThrowsAsync(new Exception());

            return (entity, cmd);
        }

        public static void AssertPublishAuditChangeWasNotPublished(this Mock<IMediator> mock)
        {
            mock.Verify(
                m => m.Publish(It.IsAny<AuditChange>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        public static void AssertAuditChangeWasPublished(this Mock<IMediator> mock,
            ClaimsPrincipal claimsPrincipal,
            ConnectedNode oldEntity,
            ConnectedNode newEntity,
            AuditOperation auditOperation)
        {
            mock.Verify(m => m.Publish(It.IsAny<AuditChange>(), It.IsAny<CancellationToken>()), Times.Once);
            mock.Verify(m => m.Publish(
                    It.Is<AuditChange>(e =>
                        e.Subject == claimsPrincipal.GetSubId() &&
                        e.NewEntity == newEntity.ToString() &&
                        e.OldEntity == oldEntity.ToString() &&
                        e.Operation == auditOperation.ToString()),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        public static void AssertPublishAuditEventWasNotPublished(this Mock<IMediator> mock)
        {
            mock.Verify(
                m => m.Publish(It.IsAny<AuditEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        public static void AssertAuditEventWasPublished<TEntity>(this Mock<IMediator> mock, Guid entityId,
            ClaimsPrincipal claimsPrincipal, AuditOperation auditOperation)
            where TEntity : BaseNode
        {
            mock.Verify(m => m.Publish(It.IsAny<AuditEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            mock.Verify(m => m.Publish(
                    It.Is<AuditEvent>(e =>
                        e.EntityId == entityId.ToString() &&
                        e.EntityType == typeof(TEntity).Name &&
                        e.Operation == auditOperation.ToString() &&
                        e.Subject == claimsPrincipal.GetSubId()),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        public static void AssertCreateItem<TEntity>(this Mock<IAdminGraphRepository> mock, TEntity res,
            TEntity entityToCreate, bool useDefaultPolicyId = false)
            where TEntity : NamedNode
        {
            mock.Verify(r => r.CreateNodeAsync(It.IsAny<TEntity>()), Times.Once);
            mock.Verify(r => r.CreateNodeAsync(
                It.Is<TEntity>(
                    e =>
                        e.Name == entityToCreate.Name &&
                        e.Description == entityToCreate.Description &&
                        e.IsEnabled == entityToCreate.IsEnabled)), Times.Once);

            Assert.NotNull(res);
            Assert.Equal(entityToCreate.Id, res.Id);
            Assert.Equal(entityToCreate.Name, res.Name);
        }

        public static void AssertUpdateItem<TEntity>(this Mock<IAdminGraphRepository> mock, TEntity res,
            TEntity entityToUpgrade)
            where TEntity : NamedNode
        {
            mock.Verify(r => r.UpdateNodeAsync(It.IsAny<Expression<Func<TEntity, bool>>>(),It.IsAny<TEntity>()), Times.Once);
            mock.Verify(r => r.UpdateNodeAsync(It.IsAny<Expression<Func<TEntity, bool>>>(),
                It.Is<TEntity>(
                    e =>
                        e.Name == entityToUpgrade.Name &&
                        e.Description == entityToUpgrade.Description &&
                        e.IsEnabled == entityToUpgrade.IsEnabled)), Times.Once);

            Assert.NotNull(res);
            Assert.Equal(entityToUpgrade.Id, res.Id);
            Assert.Equal(entityToUpgrade.Name, res.Name);
        }

        public static void AssertCreateLink<TParent, TChild>(this Mock<IAdminGraphRepository> mock,
            TParent parentEntity, TChild childEntity, ILink link)
            where TParent : NamedNode where TChild : class
        {
            mock.Verify(r => r.CreateRelationshipAsync(
                It.IsAny<Expression<Func<TParent, bool>>>(),
                It.IsAny<Expression<Func<TChild, bool>>>(),
                It.IsAny<ILink>()), Times.Once);

            mock.Verify(r => r.CreateRelationshipAsync(
                It.Is<Expression<Func<TParent, bool>>>(e => e.Compile()(parentEntity)),
                It.Is<Expression<Func<TChild, bool>>>(e => e.Compile()(childEntity)),
                It.Is<ILink>(l => l == link)), Times.Once);
        }
    }
}