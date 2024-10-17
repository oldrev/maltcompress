using System;
using System.IO;
using System.Threading.Tasks;
using Sandwych.Compression.IO;
using Sandwych.Compression.Tests.Algorithms;
using Xunit;

namespace Sandwych.Compression.Tests;

public class AsyncPipedCoderTest : AbstractCompressionTest {
    [Fact]
    public async Task SingleTaskShouldBeOk() {
        var sourceData = this.CreateRandomBytes(1024 * 1024 * 4);
        await using var pipedCoder = new AsyncPipedCoder<AsyncStreamConnector>(new AsyncCopyCoder(1024));
        using var inStream = new MemoryStream(sourceData, false);
        using var outStream = new MemoryStream();
        //Yeah, let's do some multithreaded encoding:
        await pipedCoder.CodeAsync(inStream, outStream, -1, -1);
        Assert.Equal(inStream.Length, outStream.Length);
        Assert.Equal(inStream.ToArray(), outStream.ToArray());
    }

    [Fact]
    public async Task TwoTasksShouldBeOk() {
        var sourceData = this.CreateRandomBytes(1024 * 1024 * 4);
        await using var pipedCoder = new AsyncPipedCoder<AsyncStreamConnector>(new AsyncCopyCoder(513), new AsyncCopyCoder(333));
        using var inStream = new MemoryStream(sourceData, false);
        using var outStream = new MemoryStream();
        //Yeah, let's do some multithreaded encoding:
        await pipedCoder.CodeAsync(inStream, outStream, -1, -1);
        Assert.Equal(inStream.Length, outStream.Length);
        Assert.Equal(inStream.ToArray(), outStream.ToArray());
    }

    [Fact]
    public async Task Test4TasksCoder() {
        var sourceData = this.CreateRandomBytes(1024 * 1024 * 4);
        var xorKey = (byte)0x12;
        await using var pipedCoder = new AsyncPipedCoder<AsyncStreamConnector>(
               new AsyncCopyCoder(1024), new AsyncXorCoder(xorKey, 512), new AsyncCopyCoder(2048), new AsyncXorCoder(xorKey, 4096));
        using var inStream = new MemoryStream(sourceData, false);
        using var outStream = new MemoryStream();


        //Yeah, let's do some multithreaded encoding:
        await pipedCoder.CodeAsync(inStream, outStream, -1, -1);
        Assert.Equal(inStream.Length, outStream.Length);
        Assert.Equal(inStream.ToArray(), outStream.ToArray());

    }
}
