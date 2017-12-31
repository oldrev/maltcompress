using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.Compression.Tests.Algorithms
{
    public abstract class AbstractCompressionTest
    {
        public byte[] CreateRandomBytes(int size)
        {
            var rand = new Random();
            var bytes = new byte[size];
            rand.NextBytes(bytes);
            return bytes;
        }
    }
}
