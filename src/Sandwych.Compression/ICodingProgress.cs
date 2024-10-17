using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.Compression;

public interface ICodingProgress : IProgress<CodingProgressInfo>
{
}
