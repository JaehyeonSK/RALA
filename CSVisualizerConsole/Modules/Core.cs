#define debug
using CSVisualizerConsole.Classes;
using CSVisualizerConsole.Classes.Units;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            // 실제 실행되는 부분
            ScriptEngine.Execute(entryMethodCallUnit.Content);

            // 데이터 분석
            ProcessNewMethod(entryMethodCallUnit as FuncCallUnit);
        }

        private void ProcessNewMethod(FuncCallUnit funcCallUnit)
        {
            List<CodeUnit> execList = new List<CodeUnit>();

            // 정적 메소드일 경우 확인
            var methodGuid = Metadata.GetMethodGuid(funcCallUnit.ClassName, funcCallUnit.MethodName);
            var methodInfo = Metadata.GetMethod(funcCallUnit.ClassName, funcCallUnit.MethodName);

            Console.WriteLine(funcCallUnit.Content);

            // 인스턴스 메소드일 경우
            if (methodGuid == Guid.Empty)
            {
                var var = memoryManager.GetVariable(funcCallUnit.ClassName);

                // 정적, 인스턴스 둘 다 아님
                // => 라이브러리 메소드 호출인 경우
                if (var == null)
                    return;

                var typeOfClass = var.Type;
                methodGuid = Metadata.GetMethodGuid(typeOfClass, funcCallUnit.MethodName);
                methodInfo = Metadata.GetMethod(typeOfClass, funcCallUnit.MethodName);

                // 라이브러리 메소드 호출할 경우 바로 리턴
                if (methodGuid == Guid.Empty)
                    throw new Exception("Error!");
            }


            Guid objGuid = Guid.Empty;

            // 인스턴스 메소드 호출인 경우
            // 객체 컨텍스트를 변경하기 위해 객체의 Guid를 가져오는 과정
            if (!methodInfo.IsStatic)
            {
                // Non-Static 이므로 ClassName은 참조 변수 이름을 의미
                var refVarName = funcCallUnit.ClassName;
                var refVar = memoryManager.GetVariable(refVarName);
                if (refVar.VarType == CSDV_VarInfo.CSDV_Type.VAR_TYPE)
                    throw new Exception("Not REF_TYPE!!");

                objGuid = (Guid)refVar.Value;
            }

            List<CSDV_VarInfo> args = new List<CSDV_VarInfo>();
            string[] param = classifier.GetParameterValues(funcCallUnit);
            if (param != null)
            {
                // 스택에 메소드 인자들을 생성하기 위한 정보 추가
                for (int i = 0; i < param.Length; i++)
                {
                    args.Add(new CSDV_VarInfo
                    {
                        Name = methodInfo.Parameters[i].Name,
                        Type = methodInfo.Parameters[i].Type,
                        Value = param[i],
                        VarType = methodInfo.Parameters[i].VarType
                    });
                }
            }

            // 새 컨텍스트 생성
            Context.CreateNewScope(methodInfo.IsStatic, objGuid, methodInfo.Guid, args);

            // 호출할 메소드에 포함된 코드 유닛들을 찾음
            var methodDeclUnit = (from declUnit in codeUnitManager.DeclareList
                                  where declUnit.Guid == methodGuid
                                  select declUnit as MethodDeclUnit).ElementAt(0);
            execList.AddRange(methodDeclUnit.CodeUnitList);

            while (execList.Count > 0)
            {
                CodeUnit codeUnit = execList.Pop();

                Console.WriteLine("[E]" + codeUnit.Content);
                //ScriptEngine.Execute(codeUnit.Content);

                if (codeUnit is FuncCallUnit)
                {
                    Analyze(codeUnit as FuncCallUnit);
                    ProcessNewMethod(codeUnit as FuncCallUnit);
                }

                // 선언과 동시에 할당할 경우
                if (codeUnit is VarDeclAssignUnit)
                {
                    VarDeclAssignUnit unit = codeUnit as VarDeclAssignUnit;
                    CSDV_VarInfo varInfo = new CSDV_VarInfo()
                    {
                        Name = unit.Name,
                        Type = unit.Type,
                        VarType = classifier.IsVarType(unit.Type) ?
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

                        Guid refGuid;
                        Guid varGuid = Guid.NewGuid();
                        string refVal;

                        var kind = classifier.RefAssignType(unit.RightVar, out refVal);
                        switch (kind)
                        {
                            case Classifier.RefValueKind.NewObject:
                                refGuid = Guid.NewGuid();

                                List<CSDV_VarInfo> objFields = new List<CSDV_VarInfo>();

                                ClassInfo clsInfo = Metadata.GetClassInfo(refVal);
                                clsInfo.Fields.ForEach(f =>
                                {
                                    objFields.Add(new CSDV_VarInfo()
                                    {
                                        Name = f.Name,
                                        Type = f.Type,
                                        VarType = classifier.IsVarType(f.Type) ?
                                        CSDV_VarInfo.CSDV_Type.VAR_TYPE : CSDV_VarInfo.CSDV_Type.REF_TYPE,
                                    });
                                });

                                memoryManager.CreateObject(refGuid, objFields);
                                break;
                            case Classifier.RefValueKind.Reference:
                                var refVar = memoryManager.GetVariable(refVal);
                                if (refVar.VarType == CSDV_VarInfo.CSDV_Type.VAR_TYPE)
                                    throw new Exception("Error!");

                                refGuid = (Guid)refVar.Value;
                                break;
                            default:
                                throw new Exception("Couldn't decide assign type");
                        }

                        memoryManager.CreateVariable(varGuid, varInfo);
                        memoryManager.AssignReference(varInfo.Name, refGuid);
                    }
                    else // 할당하려는 변수가 값 타입일 경우
                    {
                        if (classifier.IsLiteral(unit.RightVar))
                            varInfo.Value = unit.RightVar;
                        else
                            varInfo.Value = memoryManager.GetVariable(unit.RightVar).Value;

                        memoryManager.CreateVariable(codeUnit.Guid, varInfo);
                    }
#if debug
                    Console.WriteLine($"{varInfo.Type} {varInfo.Name} (Value: {varInfo.Value}) is created in Stack");
#endif
                }
                else if (codeUnit is VarDeclUnit) // 선언만 할 경우
                {
                    VarDeclUnit unit = codeUnit as VarDeclUnit;
                    CSDV_VarInfo varInfo = new CSDV_VarInfo()
                    {
                        Name = unit.Name,
                        Type = unit.Type,
                        VarType = classifier.IsVarType(unit.Type) ?
                        CSDV_VarInfo.CSDV_Type.VAR_TYPE : CSDV_VarInfo.CSDV_Type.REF_TYPE,
                    };
                    memoryManager.CreateVariable(codeUnit.Guid, varInfo);
#if debug
                    Console.WriteLine($"{varInfo.Type} {varInfo.Name} is created in Stack");
#endif
                }
                else if (codeUnit is AssignUnit) // 할당만 할 경우
                {
                    var assignUnit = codeUnit as AssignUnit;
                    var varInfo = memoryManager.GetVariable(assignUnit.LeftVar);

                    // 참조형 변수일 경우
                    if (varInfo.VarType == CSDV_VarInfo.CSDV_Type.REF_TYPE)
                    {
                        var rightVar = memoryManager.GetVariable(assignUnit.RightVar);
                        if (rightVar == null)
                        {
                            memoryManager.AssignReference(assignUnit.LeftVar, Guid.Empty);
#if debug
                            Console.WriteLine($"{varInfo.Type} {varInfo.Name} <= null");
#endif
                        }
                        else
                        {
                            if (rightVar.VarType == CSDV_VarInfo.CSDV_Type.VAR_TYPE)
                                throw new Exception("it must be REF_TYPE!!");
                            memoryManager.AssignReference(assignUnit.LeftVar, (Guid)rightVar.Value);
#if debug
                            Console.WriteLine($"{varInfo.Type} {varInfo.Name} <= {(Guid)rightVar.Value}");
#endif
                        }
                    }
                    else // 값형 변수일 경우
                    {
                        if (classifier.IsLiteral(assignUnit.RightVar))
                            memoryManager.AssignValue(assignUnit.LeftVar, assignUnit.RightVar);
                        else
                            memoryManager.AssignValue(assignUnit.LeftVar, (string)memoryManager.GetVariable(assignUnit.RightVar).Value);
#if debug
                        Console.WriteLine($"{varInfo.Type} {varInfo.Name} <= {varInfo.Value}");
#endif
                    }
                }
            }

            // 함수 종료에 따른 컨텍스트 삭제
            Context.DestroyCurrentScope();
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
