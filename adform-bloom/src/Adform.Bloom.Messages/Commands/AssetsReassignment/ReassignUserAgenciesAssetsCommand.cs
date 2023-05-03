using System;

namespace Adform.Bloom.Messages.Commands.AssetsReassignment
{
    public class ReassignUserAgenciesAssetsCommand : ReassignUserAssetsCommand
    {
        public ReassignUserAgenciesAssetsCommand(Guid correlationId) : base(correlationId)
        {
        }
    }
}