using System;
using Adform.Bloom.Read.Contracts.BusinessAccount;

namespace Adform.Bloom.Contracts.Input
{
    public class AssetsReassignment
    {
        public BusinessAccountType BusinessAccountType { get; set; }
        public int LegacyBusinessAccountId { get; set; }
        public Guid NewUserId { get; set; }
    }
}