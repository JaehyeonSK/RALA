using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVisualizerConsole.Classes
{
    class CodeUnit
    {
        public string Content { get; set; }

        public override string ToString()
        {
            return Content + " (" + this.GetType().ToString() + ")";
        }
    }
}
