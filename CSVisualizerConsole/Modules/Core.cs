using CSVisualizerConsole.Classes;
using CSVisualizerConsole.Classes.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVisualizerConsole.Modules
{
    static class ListExtension
    {
        public static void Push<T>(this List<T> list, T item)
        {
            list.Add(item);
        }

        public static T Pop<T>(this List<T> list)
        {
            T item = list[0];
            list.RemoveAt(0);
            return item;
        }
    }

    class Core
    {
        

        Classifier classifier;
        CodeUnitManager codeUnitManager;
        List<CodeUnit> executionList;
        string code;

        public Core()
        {
            classifier = Classifier.Instance;
            codeUnitManager = CodeUnitManager.Instance;
            executionList = new List<CodeUnit>();
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

            classifier = Classifier.Instance;
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

            #endregion

            // CodeUnit 실행
            ExecuteCodeUnit();
        }

        private void ExecuteCodeUnit()
        {
            foreach (var unit in codeUnitManager.UsingList)
            {
                executionList.Push(unit);
            }

            foreach (var unit in codeUnitManager.DeclareList)
            {
                if (unit is MethodDeclUnit)
                    continue;
                executionList.Push(unit);
            }
         

            // 엔트리 메소드를 찾아서 실행 목록에 추가
            var entryMethod = Metadata.GetEntryMethod();
            var entryMethodCallUnit = codeUnitManager.CreateCodeUnitFromMetadata(entryMethod, null);
            executionList.Push(entryMethodCallUnit);

            while (executionList.Count != 0)
            {
                CodeUnit unit = executionList.Pop();

                if (unit is DeclUnit) Console.WriteLine("[선언]");
                else if (unit is FuncCallUnit) Console.WriteLine("[호출]");

                Console.WriteLine(unit);
                if (unit is FuncCallUnit)
                {
                    Analyze(unit as FuncCallUnit);
                }

                ScriptEngine.Execute(unit.Content);
            }
        }

        private void Analyze(FuncCallUnit callUnit)
        {
            MethodInfo methodInfo = Metadata.GetMethod(callUnit.ClassName, callUnit.MethodName);

            StringBuilder sb = new StringBuilder();
            foreach(var p in methodInfo.Parameters)
            {
                sb.Append(p.Type + " ");
            }

            Console.WriteLine($@"
    [{methodInfo.ClassName}.{methodInfo.Name} 메타데이터]
    GUID: {methodInfo.Guid}
    Modifier: {methodInfo.Modifier.ToString().ToLower()}
    Static: {methodInfo.IsStatic}
    Return Type: {methodInfo.ReturnType}
    Parameters: {(string.IsNullOrWhiteSpace(sb.ToString()) ? "null" : sb.ToString())}
    Method Body: {codeUnitManager.DeclareList.Find(e=>e.Guid == methodInfo.Guid).Content}
");

            var methodBodyUnits = ((MethodDeclUnit)codeUnitManager.DeclareList.Find(e=>e.Guid==methodInfo.Guid)).CodeUnitList;
            methodBodyUnits.ForEach(e=>Console.WriteLine(e.Content));
        }
    }
}
