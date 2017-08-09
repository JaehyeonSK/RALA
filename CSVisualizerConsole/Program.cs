using CSVisualizerConsole.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVisualizerConsole
{
    class Program
    {
        static string[] files = { "testcode.cs", "linkedlist.cs" };

        static void Main(string[] args)
        {
            Core core = new Core();
            core.Start(files[1]);
        }
    }
}
