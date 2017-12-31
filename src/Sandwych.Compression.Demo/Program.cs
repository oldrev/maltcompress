using System;
using System.IO;
using Sandwych.Compression.Algorithms.Bwt;
using Sandwych.Compression.Algorithms.Lzma;

namespace Sandwych.Compression.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            var pipedCoder = new MultiThreadPipedCoder(new PassThroughCoder(), new PassThroughCoder(), new LzmaEncoder(), new PassThroughCoder());
            long inputSize = 0;
            long outputSize = 0;
            using (var inStream = File.OpenRead(@"c:\tmp\test.tar"))
            using (var outStream = File.Create(@"c:\tmp\test.lzma"))
            {
                pipedCoder.Code(inStream, outStream, new ConsoleProgress(inStream.Length));
                inputSize = inStream.Length;
                outputSize = outStream.Length;
            }

            Console.WriteLine();
            var ratio = Math.Round((double)inputSize / (double)outputSize, 2);
            Console.WriteLine("Input Size: {0}", inputSize);
            Console.WriteLine("Output Size: {0}", outputSize);
            Console.WriteLine("ratio: {0}", ratio);
            Console.WriteLine("=============== All done ===============");
            */
            long x = -1;
            ulong y = (ulong)x;
            Console.WriteLine(y);
            Console.ReadKey();
        }

    }
}