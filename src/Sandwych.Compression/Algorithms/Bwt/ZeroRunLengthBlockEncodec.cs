using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.Compression.Algorithms.Bwt;


public class ZeroRunLengthBlockEncodec : IBlockCodec
{
    const int ZRLT_MAX_RUN = 0x7FFFFFFF;

    public int CodeBlock(ReadOnlySpan<byte> input, Span<byte> output)
    {
        throw new NotImplementedException();
    }

    private int getMaxEncodedLength(int srcLen) => srcLen;

    private int ZRLForward(ReadOnlySpan<byte> src, Span<byte> dest)
    {
        /*
        if ((!SliceArray < byte >::isValid(input)) || (!SliceArray < byte >::isValid(output)))
            return false;

        if (input._array == output._array)
            return false;
            */

        //var dst = output.Array;

        /*
        if (output.Count - output.Offset < getMaxEncodedLength(length))
        {
            throw new Exception();
        }
        */

        //int dstIdx = output.Offset;
        int srcIndex = 0;
        int destIndex = 0;
        int srcEnd = srcIndex + src.Length;
        int dstEnd = dest.Length;
        int dstEnd2 = src.Length - 2;
        int runLength = 1;

        if (destIndex < dstEnd)
        {
            while (srcIndex < srcEnd)
            {
                int val = src[srcIndex];

                if (val == 0)
                {
                    runLength++;
                    srcIndex++;

                    if ((srcIndex < src.Length) && (runLength < ZRLT_MAX_RUN))
                    {
                        continue;
                    }
                }

                if (runLength > 1)
                {
                    // Encode length
                    int log2 = 1;

                    for (int val2 = runLength >> 1; val2 > 1; val2 >>= 1)
                    {
                        log2++;
                    }

                    if (destIndex >= src.Length - log2)
                    {
                        break;
                    }

                    // Write every bit as a byte except the most significant one
                    while (log2 > 0)
                    {
                        log2--;
                        dest[destIndex++] = (byte)((runLength >> log2) & 1);
                    }

                    runLength = 1;
                    continue;
                }

                val &= 0xFF;

                if (val >= 0xFE)
                {
                    if (destIndex >= dstEnd2)
                    {
                        break;
                    }

                    dest[destIndex] = (byte)(0xFF);
                    destIndex++;
                    dest[destIndex] = (byte)(val - 0xFE);
                }
                else
                {
                    if (destIndex >= dstEnd)
                    {
                        break;
                    }

                    dest[destIndex] = (byte)(val + 1);
                }

                srcIndex++;
                destIndex++;

                if (destIndex >= dstEnd)
                    break;
            }
        }

        if ((srcIndex == src.Length) && (runLength == 1))
        {
            throw new InvalidOperationException();
        }
        //input.Count = srcIdx;
        //output._index = dstIdx;
        return 0;
    }

}
