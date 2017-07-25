using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVisualizerConsole.Classes.Units
{
    class MethodDeclUnit : DeclUnit
    {
        public string Name { get; set; }

        public MethodDeclUnit(Guid guid)
            : base(guid)
        {
        }

        public override string ToString()
        {
            return Name + " (" + this.GetType().ToString() + ")";
        }
    }
}
