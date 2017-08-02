using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVisualizerConsole.Classes.Units
{
    class VarDeclUnit : DeclUnit
    {
        public string Type { get; private set; }
        public string Name { get; private set; }
        
        public VarDeclUnit(Guid guid, string type, string name)
            : base(guid)
        {
            Type = type;
            Name = name;
        }
    }
}
