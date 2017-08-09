using Sandwych.Compression.Compression.Bwt;
using Sandwych.Compression.Compression.Lzma.Compression.LZMA;
using System;
using System.IO;

namespace Sandwych.Compression.Demo
{
    class Program
    {
        static void Main(string[] args)
        {

            //2MB

            var pipeline = new MultiThreadPipedCoder(new LzmaEncoder());
            long inputSize = 0;
            long outputSize = 0;
            using (var inStream = File.OpenRead(@"c:\tmp\test.tar"))
            using (var outStream = File.Create(@"c:\tmp\test.lzma"))
            {
                pipeline.Code(inStream, outStream, new ConsoleProgress(inStream.Length));
                inputSize = inStream.Length;
                outputSize = outStream.Length;
            }

            Console.WriteLine();
            var ratio = Math.Round((double)inputSize / (double)outputSize, 2);
            Console.WriteLine("Input Size: {0}", inputSize);
            Console.WriteLine("Output Size: {0}", outputSize);
            Console.WriteLine("ratio: {0}", ratio);
            Console.WriteLine("=============== All done ===============");
            Console.ReadKey();
        }

    }
}