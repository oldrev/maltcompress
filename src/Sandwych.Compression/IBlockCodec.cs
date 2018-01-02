using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.Compression
{
    public interface IBlockCodec
    {
        int CodeBlock(ReadOnlySpan<byte> input, Span<byte> output);
    }

}
