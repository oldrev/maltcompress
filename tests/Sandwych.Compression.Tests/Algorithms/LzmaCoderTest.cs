using Sandwych.Compression.Algorithms.Lzma;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Sandwych.Compression.Tests.Algorithms
{
    public class LzmaCoderTest : AbstractCompressionTest
    {

        [Fact]
        public void LzmaCompressAndDecompressShouldBeOk()
        {
            var source = this.CreateCompressionSourceData();
            byte[] compressedBytes = null;
            byte[] properties = null;
            using (var inputStream = new MemoryStream(source, false))
            using (var outputStream = new MemoryStream())
            using (var propertiesStream = new MemoryStream())
            {
                var encoder = new LzmaEncoder();
                //encoder.SetCoderProperties(new Compression.Algorithms.Lzma.CoderPropID[] { Compression.Algorithms.Lzma.CoderPropID.DictionarySize }, new object[] { 0xFFFF });
                encoder.WriteCoderProperties(propertiesStream);
                properties = propertiesStream.ToArray();

                encoder.Code(inputStream, outputStream, -1, -1, null);
                compressedBytes = outputStream.ToArray();
                Assert.NotEqual(source, outputStream.ToArray());
            }

            using (var inputStream = new MemoryStream(compressedBytes, false))
            using (var outputStream = new MemoryStream())
            {
                var decoder = new LzmaDecoder();
                decoder.SetDecoderProperties(properties);
                decoder.Code(inputStream, outputStream, compressedBytes.Length, source.Length, null);
                Assert.Equal(source, outputStream.ToArray());
            }

        }



    }
}
