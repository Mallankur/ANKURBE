using System;

namespace Adform.Bloom.Messages.Commands.AssetsReassignment
{
    public class ReassignUserPublishersAssetsCommand : ReassignUserAssetsCommand
    {
        public ReassignUserPublishersAssetsCommand(Guid correlationId) : base(correlationId)
        {
        }
    }
}