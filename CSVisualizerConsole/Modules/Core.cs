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
        MemoryManager memoryManager;
        string code;

        public Core()
        {
            classifier = Classifier.Instance;
            codeUnitManager = CodeUnitManager.Instance;
            memoryManager = MemoryManager.Instance;
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
            

            Console.WriteLine();
            Console.WriteLine("==Execution Result==");

            #endregion

            // CodeUnit 실행
            ExecuteCodeUnit();
        }

        private void ExecuteCodeUnit()
        {
            // using 구문 및 선언 유닛 먼저 실행
            foreach (var unit in codeUnitManager.UsingList)
            {
                ScriptEngine.Execute(unit.Content);
            }

            foreach (var unit in codeUnitManager.DeclareList)
            {
                if (unit is MethodDeclUnit)
                    continue;
                ScriptEngine.Execute(unit.Content);
            }

            // 엔트리 메소드를 찾아서 실행 목록에 추가
            var entryMethod = Metadata.GetEntryMethod();
            var entryMethodCallUnit = codeUnitManager.CreateCodeUnitFromMetadata(entryMethod, null);

            ProcessNewMethod(entryMethodCallUnit as FuncCallUnit);
        }

        private void ProcessNewMethod(FuncCallUnit funcCallUnit)
        {
            List<CodeUnit> execList = new List<CodeUnit>();

            var methodGuid = Metadata.GetMethodGuid(funcCallUnit.ClassName, funcCallUnit.MethodName);
            // 라이브러리 메소드 호출할 경우
            if (methodGuid == Guid.Empty)
                return;

            memoryManager.CreateScope();

            // 호출할 메소드에 포함된 코드 유닛들을 찾음
            var methodDeclUnit = (from declUnit in codeUnitManager.DeclareList
                                  where declUnit.Guid == methodGuid
                                  select declUnit as MethodDeclUnit).ElementAt(0);
            execList.AddRange(methodDeclUnit.CodeUnitList);

            while (execList.Count > 0)
            {
                CodeUnit codeUnit = execList.Pop();

                if (codeUnit is FuncCallUnit)
                {
                    Analyze(codeUnit as FuncCallUnit);
                    ProcessNewMethod(codeUnit as FuncCallUnit);
                }

                Console.WriteLine("[E]" + codeUnit.Content);
                ScriptEngine.Execute(codeUnit.Content);

                string[] vartypes = {
                    "byte","sbyte","short","ushort","int","uint","long","ulong",
                    "float", "double", "decimal"
                };
                if (codeUnit is  VarDeclAssignUnit)
                {
                    VarDeclAssignUnit unit = codeUnit as VarDeclAssignUnit;
                    CSDV_VarInfo varInfo = new CSDV_VarInfo()
                    {
                        Name = unit.Name,
                        Type = unit.Type,
                        VarType = vartypes.Contains(unit.Type) ?
                        CSDV_VarInfo.CSDV_Type.VAR_TYPE : CSDV_VarInfo.CSDV_Type.REF_TYPE,
                    };

                    // 선언 후 할당하려는 변수가 참조 타입일 경우
                    if (varInfo.VarType == CSDV_VarInfo.CSDV_Type.REF_TYPE)
                    {
                        // string 형태의 unit.RightVar를 먼저 처리,
                        // 경우 1) 객체 생성 -> 힙에 객체 생성 후 스택에 변수 생성하여 지칭
                        //     2) 다른 참조 변수 -> Stack에서 해당 참조 변수가 가리키는 Guid를 가져와서 변경
                        //                     -> GUI 환경에서는 삭제 처리도 필요
                        //                     -> ex) GUIHandler.Delete(guid);
                        //     3) 리터럴 -> 레퍼런스 타입이므로 리터럴인 경우는 없음
                        // varInfo.Value = (?)
                    }
                    else // 할당하려는 변수가 값 타입일 경우
                    {
                        varInfo.Value = unit.RightVar;
                    }

                    memoryManager.CreateVariable(codeUnit.Guid, varInfo);
                    Console.WriteLine($"{varInfo.Type} {varInfo.Name} (Value: {varInfo.Value}) is created in Stack");
                }
                else if (codeUnit is VarDeclUnit)
                {
                    VarDeclUnit unit = codeUnit as VarDeclUnit;
                    CSDV_VarInfo varInfo = new CSDV_VarInfo()
                    {
                        Name = unit.Name,
                        Type = unit.Type,
                        VarType = vartypes.Contains(unit.Type) ?
                        CSDV_VarInfo.CSDV_Type.VAR_TYPE : CSDV_VarInfo.CSDV_Type.REF_TYPE,
                    };
                    memoryManager.CreateVariable(codeUnit.Guid, varInfo);
                    Console.WriteLine($"{varInfo.Type} {varInfo.Name} is created in Stack");
                }
            }
        }

        private void Analyze(FuncCallUnit callUnit)
        {
            MethodInfo methodInfo = Metadata.GetMethod(callUnit.ClassName, callUnit.MethodName);

            StringBuilder sb = new StringBuilder();
            if (methodInfo != null)
            {
                foreach (var p in methodInfo.Parameters)
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
    Method Body: {codeUnitManager.DeclareList.Find(e => e.Guid == methodInfo.Guid).Content}
");
            }
        }
    }
}
