using CSVisualizerConsole.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        
    }
}
