using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVisualizerConsole.Classes
{
    class RefAssignUnit : AssignUnit
    {
        public RefAssignUnit(Guid guid, string lval, string rval)
            : base(guid, lval, rval)
        {
        }
    }
}
