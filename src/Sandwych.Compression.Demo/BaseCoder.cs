using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.Compression.Demo
{
    public abstract class BaseCoder
    {
        public string Name { get; private set; }

        public BaseCoder(string name)
        {
            this.Name = name;
        }
    }
}
