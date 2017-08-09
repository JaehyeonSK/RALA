using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVisualizer.Classes.Units
{
    class VarDeclAssignUnit : VarDeclUnit
    {
        public string RightVar { get; private set; }

        public VarDeclAssignUnit(Guid guid, string type, string name, string rval)
            : base(guid, type, name)
        {
            RightVar = rval;
        }
    }
}
