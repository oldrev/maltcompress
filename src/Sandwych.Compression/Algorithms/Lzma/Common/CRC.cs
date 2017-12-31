// Common/CRC.cs

using System;
using System.Runtime.CompilerServices;

namespace Sandwych.Compression.Algorithms.Lzma
{
    class CRC
    {
        public static readonly uint[] Table;

        static CRC()
        {
            Table = new uint[256];
            const uint kPoly = 0xEDB88320;
            for (uint i = 0; i < 256; i++)
            {
                uint r = i;
                for (int j = 0; j < 8; j++)
                    if ((r & 1) != 0)
                        r = (r >> 1) ^ kPoly;
                    else
                        r >>= 1;
                Table[i] = r;
            }
        }

        uint _value = 0xFFFFFFFF;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Init() { _value = 0xFFFFFFFF; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateByte(byte b)
        {
            var tableIndex = (byte)_value ^ b;
            _value = Table[tableIndex] ^ (_value >> 8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(byte[] data, int offset, int size)
        {
            var slice = new ReadOnlySpan<byte>(data, offset, size);
            this.Update(slice);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(ReadOnlySpan<byte> data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                this.UpdateByte(data[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetDigest() { return _value ^ 0xFFFFFFFF; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static uint CalculateDigest(byte[] data, int offset, int size)
        {
            CRC crc = new CRC();
            // crc.Init();
            crc.Update(data, offset, size);
            return crc.GetDigest();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool VerifyDigest(uint digest, byte[] data, int offset, int size)
        {
            return (CalculateDigest(data, offset, size) == digest);
        }
    }
}
