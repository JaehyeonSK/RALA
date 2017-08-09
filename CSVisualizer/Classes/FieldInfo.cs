using System;

namespace CSVisualizer.Classes
{
    class FieldInfo : MetadataBase
    {
        public string Type { get; private set; }
        public string Name { get; private set; }
        public bool IsProperty { get; private set; }

        public FieldInfo(Guid guid, string type, string name, bool isProperty)
        {
            Guid = guid;
            Type = type;
            Name = name;
            IsProperty = isProperty;
        }
    }
}
