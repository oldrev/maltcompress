using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Sandwych.Compression
{
    public interface ICoder
    {
        IReadOnlyDictionary<string, object> Options { get; }

        void SetOptions(IReadOnlyDictionary<string, object> options);

        void Code(Stream inStream, Stream outStream, long inSize, long outSize, ICodingProgress progress = null);
    }

    public static class CoderExtensions
    {
        public static async Task CodeAsync(this ICoder self, Stream inStream, Stream outStream, long inSize, long outSize, ICodingProgress progress = null)
        {
            await Task.Factory.StartNew(() => self.Code(inStream, outStream, inSize, outSize, progress))
                .ConfigureAwait(false);
        }
    }
}
