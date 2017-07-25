using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVisualizerConsole.Classes
{
    class ClassDeclUnit : DeclUnit
    {
        public string Name { get; set; }
        public override string ToString()
        {
            return Name + " (" + this.GetType().ToString() + ")";
        }
    }
}
