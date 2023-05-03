using MediatR;
using Moq;
using System.Security.Claims;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Infrastructure.Models;

namespace Adform.Bloom.Unit.Test.Write
{
    public class BaseTests
    {
        protected readonly Mock<IAdminGraphRepository> _adminGraphRepositoryMock = new Mock<IAdminGraphRepository>();
        protected readonly ClaimsPrincipal _claimsPrincipal = Common.BuildPrincipal();
        protected readonly Mock<IMediator> _mediatorMock = new Mock<IMediator>();
        protected readonly Mock<IVisibilityProvider<QueryParamsTenantIds, Contracts.Output.Feature>> _featureAccessRepositoryMock = new Mock<IVisibilityProvider<QueryParamsTenantIds, Contracts.Output.Feature>>();
    }
}