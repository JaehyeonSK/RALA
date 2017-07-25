using CSVisualizerConsole.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVisualizerConsole.Modules
{
    class Core
    {
        Classifier classifier;
        string code;

        public Core()
        {
            classifier = new Classifier();
        }

        public void Start(string path)
        {
            var fileReader = Modules.FileReader.Instance;
            if (!fileReader.OpenTextFile(path))
            {
                Console.WriteLine("the file doesn't exist.");
                return;
            }
            code = fileReader.Code;

            classifier = new Classifier();
            var unitList = classifier.Classify(code);

            foreach (CodeUnit unit in unitList)
            {
                Console.WriteLine(unit);
            }

            #region 테스트 용
            Console.WriteLine("== Input Code ==");
            Console.WriteLine(fileReader.Code);

            Console.WriteLine();
            Console.WriteLine("== Execution Result ==");
            ScriptEngine.Execute(fileReader.Code);
            #endregion
        }
    }
}
