using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVisualizerConsole.Classes
{
    class MethodInfo : MetadataBase
    {
        public string Name { get; private set; }
        public CSDV_VarInfo[] Parameters { get; private set; }
        public bool IsStatic { get; private set; }
        public ClassInfo.AccessModifier Modifier { get; private set; }

        public MethodInfo(Guid guid, string name, CSDV_VarInfo[] paramInfo, bool isStatic = false, ClassInfo.AccessModifier modifier = ClassInfo.AccessModifier.Protected)
        {
            Guid = guid;
            Name = name;
            Parameters = paramInfo;
            IsStatic = isStatic;
            Modifier = modifier;
        }
    }
}
