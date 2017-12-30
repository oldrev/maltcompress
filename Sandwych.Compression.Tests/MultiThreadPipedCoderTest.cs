using System;
using Xunit;
using Sandwych.Compression.Tests.Algorithms;
using System.IO;

namespace Sandwych.Compression.Tests
{
    public class MultiThreadPipedCoderTest
    {
        [Fact]
        public void Test4ThreadsCoder()
        {
            var sourceData = new byte[1024 * 1024 * 4]; //4MB block
            var rand = new Random();
            rand.NextBytes(sourceData);
            var xorKey = (byte)0x12;
            var pipedCoder = new MultiThreadPipedCoder(new PassThroughCoder(), new XorCoder(xorKey), new PassThroughCoder(), new XorCoder(xorKey));

            using(var inStream = new MemoryStream(sourceData, false))
            using (var outStream = new MemoryStream()) {

                //Yeah, let's do some multithreaded encoding:
                pipedCoder.Code(inStream, outStream);
                Assert.Equal(inStream.Length, outStream.Length);
                Assert.Equal(inStream.ToArray(), outStream.ToArray());
            }
        }
    }
}
