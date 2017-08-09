using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVisualizer.Classes
{
    class AssignUnit : CodeUnit
    {
        public string LeftVar { get; private set; }
        public string RightVar { get; private set; }

        public AssignUnit(Guid guid, string lval, string rval)
            : base(guid)
        {
            LeftVar = lval;
            RightVar = rval;
        }
    }
}
