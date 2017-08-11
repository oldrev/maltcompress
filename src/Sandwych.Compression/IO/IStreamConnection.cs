using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Sandwych.Compression.IO
{
    public interface IStreamConnection
    {
        Stream UpStream { get; }
        Stream DownStream { get; }
        void Reset();
    }
}
