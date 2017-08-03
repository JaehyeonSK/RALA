using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVisualizerConsole.Classes
{
    class FuncCallUnit : CodeUnit
    {
        public string ClassName { get; private set; }
        public string MethodName { get; private set; }

        public FuncCallUnit(Guid guid, string className, string methodName)
            : base(guid)
        {
            ClassName = className;
            MethodName = methodName;
        }

        public override string ToString()
        {
            Console.Write($"{ClassName}.{MethodName}");
            return base.ToString();
        }
    }
}
