using CSVisualizerConsole.Classes;
using CSVisualizerConsole.Classes.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CSVisualizerConsole.Modules
{
    class CodeUnitManager
    {
        public List<CodeUnit> UsingList { get; set; } = new List<CodeUnit>();
        public List<DeclUnit> DeclareList { get; set; } = new List<DeclUnit>();
        public List<AssignUnit> AssignList { get; set; } = new List<AssignUnit>();
        public List<FuncCallUnit> CallList { get; set; } = new List<FuncCallUnit>();

        private static CodeUnitManager instance = null;
        private CodeUnitManager() { }

        public static CodeUnitManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new CodeUnitManager();
                return instance;
            }
        }

        public CodeUnit CreateCodeUnitFromMetadata(MetadataBase metadata, object[] args)
        {
            if (metadata is MethodInfo)
            {
                var info = metadata as MethodInfo;

                var _className = info.ClassName;
                var _name = info.Name;
                var _params = (args == null) ? "null" : string.Join(",", args as string[]);

                if (info.IsStatic)
                {
                    var callString = string.Format($"{_className}.{_name}({_params});");

                    var codeUnit = new FuncCallUnit(Guid.Empty, _className, _name)
                    {
                        Content = callString
                    };

                    return codeUnit;
                }
            }

            return null;
        }
    }
}
