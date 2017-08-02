﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVisualizerConsole.Modules
{
    class Context
    {
        public static Guid CurrentObject { get; private set; }

        public static void ContextChange(Guid objectGuid)
        {
            CurrentObject = objectGuid;
        }
    }
}
