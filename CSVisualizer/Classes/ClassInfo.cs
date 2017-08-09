using System;
using System.Collections.Generic;

namespace CSVisualizer.Classes
{
    class ClassInfo : MetadataBase
    {
        public enum AccessModifier
        {
            Public, Protected, Private
        };

        public string Name { get; private set; }
        public List<FieldInfo> Fields { get; private set; }
        public List<MethodInfo> Methods { get; private set; }
        public AccessModifier Modifier { get; private set; }

        public ClassInfo(Guid guid, string name, AccessModifier modifier = AccessModifier.Protected)
        {
            Guid = guid;
            Name = name;
            Fields = new List<FieldInfo>();
            Methods = new List<MethodInfo>();
        }
    }
}
