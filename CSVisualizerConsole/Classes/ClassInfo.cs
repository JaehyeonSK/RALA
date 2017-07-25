using CSVisualizerConsole.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVisualizerConsole.Classes
{
    class ClassInfo
    {
        public string Name { get; private set; }
        public List<FieldInfo> Fields { get; private set; }
        public List<MethodInfo> Methods { get; private set; }

        public ClassInfo(string name, string code)
        {
            Name = name;
            Fields = new List<FieldInfo>();
            Methods = new List<MethodInfo>();

            CodeUnitParser.BuildClassInfo(this, code);
        }

        private void aa()
        {
            
        }
    }
}
