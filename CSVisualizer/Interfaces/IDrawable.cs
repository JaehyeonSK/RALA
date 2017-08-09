using System;
using System.Collections.Generic;

namespace CSVisualizer.Classes
{
    interface IDrawable
    {
        void CreateStack(Guid guid);
        void DestroyStack(Guid guid);

        void CreateVariable(Guid stack, CSDV_VarInfo varInfo);
        void CreateObject(Guid objGuid, List<CSDV_VarInfo> fields);
    }
}
