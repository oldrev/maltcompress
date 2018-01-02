using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace Sandwych.Compression.Tests
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

        public byte[] CreateCompressionSourceData()
        {
            var currentAssemblyPath = Assembly.GetAssembly(this.GetType()).Location;
            return File.ReadAllBytes(currentAssemblyPath);
        }
    }
}
