using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVisualizer.Classes
{
    class ClassDeclUnit : DeclUnit
    {
        public string Name { get; set; }

        public ClassDeclUnit(Guid guid)
            : base(guid)
        {
        }

        public override string ToString()
        {
            return Name + " (" + this.GetType().ToString() + ")";
        }
    }
}
