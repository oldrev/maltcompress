using System;

namespace Sandwych.Compression.Algorithms.RangeCoder {
    public struct RangeBitTreeEncoder {
        private readonly RangeBitEncoder[] _models;
        private readonly int _numBitLevels;

        public RangeBitTreeEncoder(int numBitLevels) {
            _numBitLevels = numBitLevels;
            _models = new RangeBitEncoder[1 << numBitLevels];
        }

        public void Init() {
            for (uint i = 1; i < (1 << _numBitLevels); i++) {
                _models[i].Init();
            }
        }

        public void Encode(RangeEncoder rangeEncoder, UInt32 symbol) {
            UInt32 m = 1;
            for (int bitIndex = _numBitLevels; bitIndex > 0;) {
                bitIndex--;
                UInt32 bit = (symbol >> bitIndex) & 1;
                _models[m].Encode(rangeEncoder, bit);
                m = (m << 1) | bit;
            }
        }

        public void ReverseEncode(RangeEncoder rangeEncoder, UInt32 symbol) {
            UInt32 m = 1;
            for (UInt32 i = 0; i < _numBitLevels; i++) {
                UInt32 bit = symbol & 1;
                _models[m].Encode(rangeEncoder, bit);
                m = (m << 1) | bit;
                symbol >>= 1;
            }
        }

        public UInt32 GetPrice(UInt32 symbol) {
            UInt32 price = 0;
            UInt32 m = 1;
            for (int bitIndex = _numBitLevels; bitIndex > 0;) {
                bitIndex--;
                UInt32 bit = (symbol >> bitIndex) & 1;
                price += _models[m].GetPrice(bit);
                m = (m << 1) + bit;
            }
            return price;
        }

        public UInt32 ReverseGetPrice(UInt32 symbol) {
            UInt32 price = 0;
            UInt32 m = 1;
            for (int i = _numBitLevels; i > 0; i--) {
                UInt32 bit = symbol & 1;
                symbol >>= 1;
                price += _models[m].GetPrice(bit);
                m = (m << 1) | bit;
            }
            return price;
        }

        public static UInt32 ReverseGetPrice(RangeBitEncoder[] Models, UInt32 startIndex,
            int NumBitLevels, UInt32 symbol) {
            UInt32 price = 0;
            UInt32 m = 1;
            for (int i = NumBitLevels; i > 0; i--) {
                UInt32 bit = symbol & 1;
                symbol >>= 1;
                price += Models[startIndex + m].GetPrice(bit);
                m = (m << 1) | bit;
            }
            return price;
        }

        public static void ReverseEncode(RangeBitEncoder[] Models, UInt32 startIndex,
            RangeEncoder rangeEncoder, int NumBitLevels, UInt32 symbol) {
            UInt32 m = 1;
            for (int i = 0; i < NumBitLevels; i++) {
                UInt32 bit = symbol & 1;
                Models[startIndex + m].Encode(rangeEncoder, bit);
                m = (m << 1) | bit;
                symbol >>= 1;
            }
        }
    }

    public struct RangeBitTreeDecoder {
        readonly RangeBitDecoder[] _models;
        readonly int _numBitLevels;

        public RangeBitTreeDecoder(int numBitLevels) {
            _numBitLevels = numBitLevels;
            _models = new RangeBitDecoder[1 << numBitLevels];
        }

        public void Init() {
            for (uint i = 1; i < (1 << _numBitLevels); i++) {
                _models[i].Init();
            }
        }

        public uint Decode(RangeDecoder rangeDecoder) {
            uint m = 1;
            for (int bitIndex = _numBitLevels; bitIndex > 0; bitIndex--)
                m = (m << 1) + _models[m].Decode(rangeDecoder);
            return m - ((uint)1 << _numBitLevels);
        }

        public uint ReverseDecode(RangeDecoder rangeDecoder) {
            uint m = 1;
            uint symbol = 0;
            for (int bitIndex = 0; bitIndex < _numBitLevels; bitIndex++) {
                uint bit = _models[m].Decode(rangeDecoder);
                m <<= 1;
                m += bit;
                symbol |= (bit << bitIndex);
            }
            return symbol;
        }

        public static uint ReverseDecode(RangeBitDecoder[] Models, UInt32 startIndex,
            RangeDecoder rangeDecoder, int NumBitLevels) {
            uint m = 1;
            uint symbol = 0;
            for (int bitIndex = 0; bitIndex < NumBitLevels; bitIndex++) {
                uint bit = Models[startIndex + m].Decode(rangeDecoder);
                m <<= 1;
                m += bit;
                symbol |= (bit << bitIndex);
            }
            return symbol;
        }
    }
}
