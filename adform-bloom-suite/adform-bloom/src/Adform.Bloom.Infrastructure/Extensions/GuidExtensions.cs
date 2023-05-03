using System;

namespace Adform.Bloom.Infrastructure.Extensions
{
    public static class GuidExtensions
    {
        public static Guid Combine(this Guid baseGuid, Guid withGuid)
        {
            const int byteCount = 16;
            Span<byte> destByte = stackalloc byte[byteCount];
            Span<byte> baseByte = stackalloc byte[byteCount];
            Span<byte> withByte = stackalloc byte[byteCount];

            if (!baseGuid.TryWriteBytes(baseByte))
                throw new ArgumentException("Cannot combine guid because of invalid value", nameof(baseGuid));

            if (!withGuid.TryWriteBytes(withByte))
                throw new ArgumentException("Cannot combine guid because of invalid value", nameof(withGuid));

            for (var i = 0; i < byteCount; i++) destByte[i] = (byte) (baseByte[i] ^ withByte[i]);

            return new Guid(destByte);
        }
    }
}