using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sandwych.Compression
{
    public abstract class AbstractCoder : ICoder
    {
        public IReadOnlyDictionary<int, object> Options { get; private set; }

        public virtual void SetOptions(IReadOnlyDictionary<int, object> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            this.Options = options;
        }

        public abstract void Code(Stream inStream, Stream outStream, ICodingProgress progress = null);
    }
}
