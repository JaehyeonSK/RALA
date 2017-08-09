using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVisualizerConsole.Classes
{
    interface IDrawable
    {
        void CreateStack(Guid guid);
        void DestroyStack(Guid guid);

        void CreateVariable(Guid stack, CSDV_VarInfo varInfo);
        void CreateObject(Guid heap, string objType, string objName, List<CSDV_VarInfo> fields);
    }
}
