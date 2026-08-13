using System;
using System.Security.Cryptography;
using System.Text;
using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Core.Randomness
{
    /// <summary>
    /// Versioned, order-independent derivation of deterministic PCG32 streams.
    /// </summary>
    public static class RandomStreamDerivation
    {
        public const string Id = "sha256-framed-be-pcg32-v1";

        private const string DomainSeparator = "pse.random-stream-derivation.v1";
        private static readonly char[] LowercaseHex = "0123456789abcdef".ToCharArray();

        public static RandomStreamInitialization Derive(
            RandomRootSeed rootSeed,
            StableId<RandomStreamDomainScope> domainId,
            StableId<RandomStreamContextScope> contextId)
        {
            byte[] digest = ComputeDigest(rootSeed, domainId, contextId);
            ulong initialState = ReadUInt64BigEndian(digest, 0);
            ulong streamSelector = ReadUInt64BigEndian(digest, 8) & Pcg32Algorithm.MaximumStreamSelector;
            return new RandomStreamInitialization(initialState, streamSelector);
        }

        /// <summary>
        /// Recreates a stream's initial parameters from save metadata. It does not resume consumed draws;
        /// long-lived streams must restore their persisted <see cref="Pcg32State"/> instead.
        /// </summary>
        public static RandomStreamInitialization DeriveInitializationFromSaveMetadata(
            string rootSeedHex,
            string derivationId,
            string randomAlgorithmId,
            StableId<RandomStreamDomainScope> domainId,
            StableId<RandomStreamContextScope> contextId)
        {
            if (!string.Equals(derivationId, Id, StringComparison.Ordinal))
            {
                throw new NotSupportedException(
                    $"Unsupported random stream derivation ID '{derivationId ?? "<missing>"}'.");
            }

            if (!string.Equals(randomAlgorithmId, Pcg32Algorithm.Id, StringComparison.Ordinal))
            {
                throw new NotSupportedException(
                    $"Unsupported random algorithm ID '{randomAlgorithmId ?? "<missing>"}'.");
            }

            RandomRootSeed rootSeed = RandomRootSeed.ParseCanonical(rootSeedHex);
            return Derive(rootSeed, domainId, contextId);
        }

        /// <summary>
        /// Returns a non-secret diagnostic fingerprint for drift tests and Guardian reports.
        /// It must never be used as a credential or security token.
        /// </summary>
        public static string GetDerivationFingerprint(
            RandomRootSeed rootSeed,
            StableId<RandomStreamDomainScope> domainId,
            StableId<RandomStreamContextScope> contextId)
        {
            return ToLowercaseHex(ComputeDigest(rootSeed, domainId, contextId));
        }

        private static byte[] ComputeDigest(
            RandomRootSeed rootSeed,
            StableId<RandomStreamDomainScope> domainId,
            StableId<RandomStreamContextScope> contextId)
        {
            if (domainId.IsEmpty)
            {
                throw new ArgumentException("A random stream requires a stable domain ID.", nameof(domainId));
            }

            if (contextId.IsEmpty)
            {
                throw new ArgumentException("A random stream requires a stable context ID.", nameof(contextId));
            }

            byte[] preimage = BuildPreimage(rootSeed, domainId.Value, contextId.Value);
            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(preimage);
            }
        }

        private static byte[] BuildPreimage(RandomRootSeed rootSeed, string domainId, string contextId)
        {
            byte[] separatorBytes = Encoding.UTF8.GetBytes(DomainSeparator);
            byte[] algorithmBytes = Encoding.UTF8.GetBytes(Pcg32Algorithm.Id);
            byte[] domainBytes = Encoding.UTF8.GetBytes(domainId);
            byte[] contextBytes = Encoding.UTF8.GetBytes(contextId);
            int totalLength =
                4 + separatorBytes.Length +
                8 +
                4 + algorithmBytes.Length +
                4 + domainBytes.Length +
                4 + contextBytes.Length;
            var preimage = new byte[totalLength];
            int offset = 0;

            WriteFrame(preimage, ref offset, separatorBytes);
            WriteUInt64BigEndian(preimage, ref offset, rootSeed.Value);
            WriteFrame(preimage, ref offset, algorithmBytes);
            WriteFrame(preimage, ref offset, domainBytes);
            WriteFrame(preimage, ref offset, contextBytes);
            return preimage;
        }

        private static void WriteFrame(byte[] destination, ref int offset, byte[] value)
        {
            WriteUInt32BigEndian(destination, ref offset, (uint)value.Length);
            Buffer.BlockCopy(value, 0, destination, offset, value.Length);
            offset += value.Length;
        }

        private static void WriteUInt32BigEndian(byte[] destination, ref int offset, uint value)
        {
            destination[offset++] = (byte)(value >> 24);
            destination[offset++] = (byte)(value >> 16);
            destination[offset++] = (byte)(value >> 8);
            destination[offset++] = (byte)value;
        }

        private static void WriteUInt64BigEndian(byte[] destination, ref int offset, ulong value)
        {
            destination[offset++] = (byte)(value >> 56);
            destination[offset++] = (byte)(value >> 48);
            destination[offset++] = (byte)(value >> 40);
            destination[offset++] = (byte)(value >> 32);
            destination[offset++] = (byte)(value >> 24);
            destination[offset++] = (byte)(value >> 16);
            destination[offset++] = (byte)(value >> 8);
            destination[offset++] = (byte)value;
        }

        private static ulong ReadUInt64BigEndian(byte[] source, int offset)
        {
            return
                ((ulong)source[offset] << 56) |
                ((ulong)source[offset + 1] << 48) |
                ((ulong)source[offset + 2] << 40) |
                ((ulong)source[offset + 3] << 32) |
                ((ulong)source[offset + 4] << 24) |
                ((ulong)source[offset + 5] << 16) |
                ((ulong)source[offset + 6] << 8) |
                source[offset + 7];
        }

        private static string ToLowercaseHex(byte[] value)
        {
            var characters = new char[value.Length * 2];
            for (int index = 0; index < value.Length; index++)
            {
                byte current = value[index];
                characters[index * 2] = LowercaseHex[current >> 4];
                characters[(index * 2) + 1] = LowercaseHex[current & 0x0F];
            }

            return new string(characters);
        }
    }
}
