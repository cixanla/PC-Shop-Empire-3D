using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PCShopEmpire3D.Core.Events
{
    /// <summary>
    /// Culture-independent canonical binary writer used to fingerprint immutable event payloads.
    /// Fields must be written in a stable, schema-versioned order chosen by the event type.
    /// </summary>
    public sealed class DomainEventPayloadWriter
    {
        private readonly MemoryStream _stream = new MemoryStream();

        public void WriteBoolean(bool value)
        {
            _stream.WriteByte(value ? (byte)1 : (byte)0);
        }

        public void WriteInt32(int value)
        {
            WriteUInt32(unchecked((uint)value));
        }

        public void WriteUInt32(uint value)
        {
            _stream.WriteByte((byte)(value >> 24));
            _stream.WriteByte((byte)(value >> 16));
            _stream.WriteByte((byte)(value >> 8));
            _stream.WriteByte((byte)value);
        }

        public void WriteInt64(long value)
        {
            WriteUInt64(unchecked((ulong)value));
        }

        public void WriteUInt64(ulong value)
        {
            _stream.WriteByte((byte)(value >> 56));
            _stream.WriteByte((byte)(value >> 48));
            _stream.WriteByte((byte)(value >> 40));
            _stream.WriteByte((byte)(value >> 32));
            _stream.WriteByte((byte)(value >> 24));
            _stream.WriteByte((byte)(value >> 16));
            _stream.WriteByte((byte)(value >> 8));
            _stream.WriteByte((byte)value);
        }

        public void WriteString(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            byte[] bytes = Encoding.UTF8.GetBytes(value);
            WriteBytes(bytes);
        }

        public void WriteBytes(byte[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            WriteUInt32(checked((uint)value.Length));
            _stream.Write(value, 0, value.Length);
        }

        internal DomainEventPayloadFingerprint ComputeFingerprint()
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] digest = sha256.ComputeHash(_stream.ToArray());
                return DomainEventPayloadFingerprint.FromSha256Digest(digest);
            }
        }
    }
}
