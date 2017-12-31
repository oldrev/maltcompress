using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sandwych.Compression
{
    public abstract class AbstractCoder : ICoder
    {
        public IReadOnlyDictionary<string, object> Options { get; private set; }

        public virtual void SetOptions(IReadOnlyDictionary<string, object> options)
        {
            this.Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public abstract void Code(Stream inStream, Stream outStream, long inSize, long outSize, ICodingProgress progress = null);
    }
}
