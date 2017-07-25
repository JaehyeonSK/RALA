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
        CodeUnitManager codeUnitManager;
        string code;

        public Core()
        {
            classifier = new Classifier();
            codeUnitManager = CodeUnitManager.Instance;
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
            classifier.Classify(codeUnitManager, code);

            #region 테스트 용
            Console.WriteLine("==Using List==");
            foreach (var unit in codeUnitManager.UsingList)
            {
                Console.WriteLine(unit);
            }
            Console.WriteLine("====\n");

            Console.WriteLine("==Declare List==");
            foreach (var unit in codeUnitManager.DeclareList)
            {
                Console.WriteLine(unit);
            }
            Console.WriteLine("====\n");

            Console.WriteLine("==Assign List==");
            foreach (var unit in codeUnitManager.AssignList)
            {
                Console.WriteLine(unit);
            }
            Console.WriteLine("====\n");

            Console.WriteLine("==Call List==");
            foreach (var unit in codeUnitManager.CallList)
            {
                Console.WriteLine(unit);
            }
            Console.WriteLine("====\n");

            Console.WriteLine();
            Console.WriteLine("==Execution Result==");

            ScriptEngine.Execute(fileReader.Code);
            #endregion

            // Flow Management
            // 엔트리 메소드를 찾고, 해당 엔트리 메소드를 갖는 클래스를 생성하여 실행
            // ScriptEngine.Execute(...);
        }
    }
}
