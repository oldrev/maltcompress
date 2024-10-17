using System;
using Xunit;
using Sandwych.Compression.Tests.Algorithms;
using Sandwych.Compression.IO;
using System.IO;

namespace Sandwych.Compression.Tests;

public class MultiThreadPipedCoderTest : AbstractCompressionTest
{
	[Fact]
	public void SingleThreadShouldBeOk()
	{
		var sourceData = this.CreateRandomBytes(1024 * 1024 * 4);
		using var pipedCoder = new MultiThreadPipedCoder<StreamConnector>(new CopyCoder(1024));
		using var inStream = new MemoryStream(sourceData, false);
		using var outStream = new MemoryStream();
		//Yeah, let's do some multithreaded encoding:
		pipedCoder.Code(inStream, outStream, -1, -1);
		Assert.Equal(inStream.Length, outStream.Length);
		Assert.Equal(inStream.ToArray(), outStream.ToArray());
	}

	[Fact]
	public void Test4ThreadsCoder()
	{
		var sourceData = this.CreateRandomBytes(1024 * 1024 * 4);
		var xorKey = (byte)0x12;
		using var pipedCoder = new MultiThreadPipedCoder<StreamConnector>(
			new CopyCoder(1024), new XorCoder(xorKey, 512), new CopyCoder(2048), new XorCoder(xorKey, 4096));
		using var inStream = new MemoryStream(sourceData, false);
		using var outStream = new MemoryStream();


		//Yeah, let's do some multithreaded encoding:
		pipedCoder.Code(inStream, outStream, -1, -1);
		Assert.Equal(inStream.Length, outStream.Length);
		Assert.Equal(inStream.ToArray(), outStream.ToArray());

	}
}
