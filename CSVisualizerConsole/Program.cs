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
        static string FILE = "testcode.cs";

        static void Main(string[] args)
        {
            Core core = new Core();
            core.Start(FILE);
        }
    }
}
