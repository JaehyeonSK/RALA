using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVisualizerConsole.Classes
{
    class CodeUnit
    {
        public Guid Guid { get; protected set; }
        public string Content { get; set; }

        public CodeUnit(Guid guid)
        {
            Guid = guid;
        }

        public override string ToString()
        {
            return Content + " (" + this.GetType().ToString() + ")";
        }
    }
}
