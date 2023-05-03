using System;

namespace Adform.Bloom.Messages.Commands.AssetsReassignment
{
    public class ReassignUserDataProvidersAssetsCommand : ReassignUserAssetsCommand
    {
        public ReassignUserDataProvidersAssetsCommand(Guid correlationId) : base(correlationId)
        {
        }
    }
}