using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.Compression
{
    public interface IBlockCodec
    {
        int CodeBlock(byte[] input, int inputOffset, int inputCount, byte[] output);
    }

}
