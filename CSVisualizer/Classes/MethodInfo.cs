using System;

namespace CSVisualizer.Classes
{
    class MethodInfo : MetadataBase
    {
        public string ClassName { get; private set; }
        public string ReturnType { get; private set; }
        public string Name { get; private set; }
        public CSDV_VarInfo[] Parameters { get; private set; }
        public bool IsStatic { get; private set; }
        public ClassInfo.AccessModifier Modifier { get; private set; }

        public MethodInfo(Guid guid, string className, string name, string returnType, CSDV_VarInfo[] paramInfo, bool isStatic = false, ClassInfo.AccessModifier modifier = ClassInfo.AccessModifier.Protected)
        {
            Guid = guid;
            ClassName = className;
            Name = name;
            ReturnType = returnType;
            Parameters = paramInfo;
            IsStatic = isStatic;
            Modifier = modifier;
        }
    }
}
