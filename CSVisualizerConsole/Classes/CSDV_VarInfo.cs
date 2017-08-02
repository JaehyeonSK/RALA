using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVisualizerConsole.Classes
{
    public class CSDV_VarInfo
    {
        public enum CSDV_Type { VAR_TYPE, REF_TYPE }

        public string Type { get; set; }
        public string Name { get; set; }
        public object Value {
            get
            {
                switch(VarType)
                {
                    case CSDV_Type.VAR_TYPE: return varValue;
                    case CSDV_Type.REF_TYPE: return refValue;
                    default: throw new Exception("VarType is unknown.");
                }
            }
            set
            {
                switch(VarType)
                {
                    case CSDV_Type.VAR_TYPE:
                        varValue = (string)value;
                        break;
                    case CSDV_Type.REF_TYPE:
                        refValue = (Guid)value;
                        break;
                    default:
                        throw new Exception("VarType is unknown.");
                }
            }
        }

        public CSDV_Type VarType { get; set; }
        private string varValue;
        private Guid refValue;
    }
}
