using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Sandwych.Compression
{
    public interface ICoder
    {
        void Code(Stream inStream, Stream outStream, ICodingProgress progress = null);
    }

    public static class CoderExtensions
    {
        public static async Task CodeAsync(this ICoder self, Stream inStream, Stream outStream, ICodingProgress progress = null)
        {
            await Task.Factory.StartNew(() => self.Code(inStream, outStream, progress))
                .ConfigureAwait(false);
        }
    }
}
