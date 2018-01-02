using System;

namespace Sandwych.Compression.Algorithms.RangeCoder
{
    public struct RangeBitEncoder
    {

        public const int kNumBitModelTotalBits = 11;
        public const uint kBitModelTotal = (1 << kNumBitModelTotalBits);
        const int kNumMoveBits = 5;
        const int kNumMoveReducingBits = 2;
        public const int kNumBitPriceShiftBits = 6;

        private uint _prob;

        public void Init()
        {
            _prob = kBitModelTotal >> 1;
        }

        public void UpdateModel(uint symbol)
        {
            if (symbol == 0)
                _prob += (kBitModelTotal - _prob) >> kNumMoveBits;
            else
                _prob -= (_prob) >> kNumMoveBits;
        }

        public void Encode(RangeEncoder encoder, uint symbol)
        {
            // encoder.EncodeBit(Prob, kNumBitModelTotalBits, symbol);
            // UpdateModel(symbol);
            uint newBound = (encoder.Range >> kNumBitModelTotalBits) * _prob;
            if (symbol == 0)
            {
                encoder.Range = newBound;
                _prob += (kBitModelTotal - _prob) >> kNumMoveBits;
            }
            else
            {
                encoder.Low += newBound;
                encoder.Range -= newBound;
                _prob -= (_prob) >> kNumMoveBits;
            }
            if (encoder.Range < RangeEncoder.kTopValue)
            {
                encoder.Range <<= 8;
                encoder.ShiftLow();
            }
        }

        private readonly static UInt32[] s_ProbPrices = new UInt32[kBitModelTotal >> kNumMoveReducingBits];

        static RangeBitEncoder()
        {
            const int kNumBits = (kNumBitModelTotalBits - kNumMoveReducingBits);
            for (int i = kNumBits - 1; i >= 0; i--)
            {
                UInt32 start = (UInt32)1 << (kNumBits - i - 1);
                UInt32 end = (UInt32)1 << (kNumBits - i);
                for (UInt32 j = start; j < end; j++)
                {
                    s_ProbPrices[j] = ((UInt32)i << kNumBitPriceShiftBits) +
                        (((end - j) << kNumBitPriceShiftBits) >> (kNumBits - i - 1));
                }
            }
        }

        public uint GetPrice(uint symbol) =>
            s_ProbPrices[(((_prob - symbol) ^ ((-(int)symbol))) & (kBitModelTotal - 1)) >> kNumMoveReducingBits];

        public uint GetPrice0() => s_ProbPrices[_prob >> kNumMoveReducingBits];

        public uint GetPrice1() => s_ProbPrices[(kBitModelTotal - _prob) >> kNumMoveReducingBits];
    }


    public struct RangeBitDecoder
    {
        public const int kNumBitModelTotalBits = 11;
        public const uint kBitModelTotal = (1 << kNumBitModelTotalBits);
        private const int kNumMoveBits = 5;

        private uint _prob;

        public void UpdateModel(int numMoveBits, uint symbol)
        {
            if (symbol == 0)
            {
                _prob += (kBitModelTotal - _prob) >> numMoveBits;
            }
            else
            {
                _prob -= (_prob) >> numMoveBits;
            }
        }

        public void Init()
        {
            _prob = kBitModelTotal >> 1;
        }

        public uint Decode(RangeDecoder rangeDecoder)
        {
            uint newBound = (uint)(rangeDecoder.Range >> kNumBitModelTotalBits) * (uint)_prob;
            if (rangeDecoder.Code < newBound)
            {
                rangeDecoder.Range = newBound;
                _prob += (kBitModelTotal - _prob) >> kNumMoveBits;
                if (rangeDecoder.Range < RangeDecoder.kTopValue)
                {
                    rangeDecoder.Code = (rangeDecoder.Code << 8) | (byte)rangeDecoder._inStream.ReadByte();
                    rangeDecoder.Range <<= 8;
                }
                return 0;
            }
            else
            {
                rangeDecoder.Range -= newBound;
                rangeDecoder.Code -= newBound;
                _prob -= (_prob) >> kNumMoveBits;
                if (rangeDecoder.Range < RangeDecoder.kTopValue)
                {
                    rangeDecoder.Code = (rangeDecoder.Code << 8) | (byte)rangeDecoder._inStream.ReadByte();
                    rangeDecoder.Range <<= 8;
                }
                return 1;
            }
        }
    }
}
