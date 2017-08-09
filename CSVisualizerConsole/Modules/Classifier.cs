using CSVisualizerConsole.Classes;
using CSVisualizerConsole.Classes.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CSVisualizerConsole.Modules
{
    class Classifier
    {
        private string[] KEYWORDS = {
            "using", "class", "interface",
            "byte", "sbyte", "char", "short", "ushort", "int", "uint", "long", "ulong", "float", "double", "decimal"
        };

        private static Classifier instance;
        public static Classifier Instance
        {
            get
            {
                if (instance == null)
                    instance = new Classifier();
                return instance;
            }
        }

        private Classifier()
        {
        }

        private CodeUnit ProcessInterface(string code)
        {
            throw new NotImplementedException();
        }

        private string ProcessUsingStatements(CodeUnitManager manager, string code)
        {
            string usingPattern = @"using\s+.+;";

            Regex regex = new Regex(usingPattern);
            var matches = regex.Matches(code);

            StringBuilder sb = new StringBuilder();

            int length = 0;
            foreach (Match match in matches)
            {
                string str = match.Value;
                length += str.Length + Environment.NewLine.Length;

                sb.Append(str + Environment.NewLine);
            }

            manager.UsingList.Add(
                new CodeUnit(Guid.Empty)
                {
                    Content = sb.ToString()
                }
                );
            return code.Substring(length);
        }

        private string ProcessClassDeclarations(CodeUnitManager manager, string code)
        {
            // name: 클래스 이름
            // content: 클래스 블록 내부 코드
            const string classPattern = @"class\s+(?<name>.+)\s*\{(?<content>(?>\{(?<c>)|[^{}]+|\}(?<-c>))*(?(c)(?!)))\}";
            Regex regex = new Regex(classPattern);
            var matches = regex.Matches(code);

            // 모든 클래스 블록에 대한 처리
            foreach (Match match in matches)
            {
                string className = match.Groups["name"].Value.Trim();
                string content = match.Groups["content"].Value;
                Guid newGuid = Guid.NewGuid();

                // 클래스 유닛 생성 및 추가
                ClassDeclUnit classUnit = new ClassDeclUnit(newGuid)
                {
                    Name = className,
                    Content = match.Value
                };
                manager.DeclareList.Add(classUnit);

                var classInfo = MetadataBuilder.BuildClassInfo(newGuid, className, content);
                Metadata.RegisterClass(className, classInfo);

                // 처리된 클래스 선언 제거
                code = code.Replace(match.Value, "");
            }

            return code;
        }

        private CodeUnit SelectType(string codeUnit)
        {
            // type: 변수 타입(null이 아니면 선언문)
            // lval: 좌항
            // rval: 우항
            //string assignPattern = @"(?<type>[a-zA-Z0-9]+)?\s+(?<lval>[a-zA-Z0-9_]+)\s*=\s*(?<rval>.+)";
            string assignPattern = @"((?<type>[a-zA-Z0-9]+)\s+)?(?<lval>[.a-zA-Z0-9_]+)\s*=\s*(?<rval>.+);";

            Regex assignRegex = new Regex(assignPattern);
            var match = assignRegex.Match(codeUnit);
            // 할당에 해당할 경우
            if (match.Length > 0)
            {
                var type = match.Groups["type"].Value;
                var lval = match.Groups["lval"].Value;
                var rval = match.Groups["rval"].Value;

                if (string.IsNullOrWhiteSpace(type))
                    // 단순 할당문
                    return new AssignUnit(Guid.NewGuid(), lval, rval) { Content = match.Value };
                else
                    // 선언 및 초기화문
                    return new VarDeclAssignUnit(Guid.NewGuid(), type, lval, rval) { Content = match.Value };
            }

            // type: 변수 선언 타입
            // name: 변수 이름
            string varDeclPattern = @"(?<type>[a-zA-Z0-9]+)\s+(?<name>[a-zA-Z0-9_]+);";

            Regex varDeclRegex = new Regex(varDeclPattern);
            match = varDeclRegex.Match(codeUnit);
            // 단순 선언에 해당할 경우
            if (match.Length > 0)
            {
                var type = match.Groups["type"].Value;
                var name = match.Groups["name"].Value;

                return new VarDeclUnit(Guid.NewGuid(), type, name) { Content = match.Value };
            }

            //string funcCallPattern = @"(?<classname>([a-zA-Z_][a-zA-Z0-9_]+\.){0,})(?<name>[a-zA-Z_][a-zA-Z0-9_]*)\((\s*\w+\s*,?){0,}\)";
            string funcCallPattern = @"(?<classname>([a-zA-Z_][a-zA-Z0-9_]+\.){0,})(?<name>[a-zA-Z_][a-zA-Z0-9_]*)\((?<params>\s*.+\s*,?){0,}\);";

            Regex funcCallRegex = new Regex(funcCallPattern);
            match = funcCallRegex.Match(codeUnit);

            if (match.Length > 0)
            {
                var className = match.Groups["classname"].Value;
                className = className.Substring(0, className.Length - 1);
                var methodName = match.Groups["name"].Value;
                var parameters = match.Groups["params"].Value;
                var paramList = parameters.Replace(" ", "").Split(',').ToList();

                FuncCallUnit funcCallUnit = new FuncCallUnit(Guid.NewGuid(), className, methodName)
                {
                    Content = match.Value,
                };

                var m = Metadata.GetMethod(className, methodName);
                if (m != null)
                {
                    var ps = m.Parameters;
                    for (int i = 0; i < Math.Min(paramList.Count, ps.Length); i++)
                    {
                        ps[i].Value = paramList[i];
                    }
                    funcCallUnit.ParamList.AddRange(ps);
                }

                return funcCallUnit;
            }

            return new CodeUnit(Guid.Empty);
        }

        public string[] GetParameterValues(FuncCallUnit funcCallUnit)
        {
            string paramValuesPattern = @"\((?<params>.+)\)";
            var callStatement = funcCallUnit.Content;

            Regex regex = new Regex(paramValuesPattern);
            Match match = regex.Match(callStatement);

            string[] param = null;
            if (match.Success)
            {
                string matchedString = match.Groups["params"]?.Value;
                if (!string.IsNullOrEmpty(matchedString))
                {
                    param = matchedString.Split(',');
                    for (int i = 0; i < param.Length; i++)
                        param[i] = param[i].Trim();
                }
            }

            return param;
        }

        /// <summary>
        /// Method Body 내 코드 유닛들을 리스트화 시켜서 반환한다.
        /// </summary>
        /// <param name="methodBodyContent"></param>
        /// <returns></returns>
        public List<CodeUnit> ProcessMethodBody(string methodBodyContent)
        {
            string codeUnitPattern = @"\s*(?<code_unit>.+)\s*;";
            Regex regex = new Regex(codeUnitPattern);
            var matches = regex.Matches(methodBodyContent);

            List<CodeUnit> codeUnitList = new List<CodeUnit>();

            // 메소드의 내용을 코드유닛들로 분리하고 분류하여 리스트에 추가
            foreach (Match match in matches)
            {
                var line = match.Groups["code_unit"].Value + ";";
                CodeUnit unit = SelectType(line);
                codeUnitList.Add(unit);
            }

            return codeUnitList;
        }

        public RefValueKind RefAssignType(string refAssignStatement, out string type)
        {
            Regex regex = new Regex(@"(?<hasNew>new\s)?\s*(?<refVal>[\w]+)(\s*\((?<params>.*)\))?");
            var match = regex.Match(refAssignStatement);

            type = null;

            if (!match.Success)
            {
                return RefValueKind.Other;
            }

            if (match.Groups["hasNew"].Value != null)
            {
                type = match.Groups["refVal"].Value;
                return RefValueKind.NewObject;
            }

            return RefValueKind.Reference;
        }

        public bool IsVarType(string type)
        {
            string[] vartypes = {
                    "byte","sbyte","short","ushort","int","uint","long","ulong",
                    "float", "double", "decimal"
                    };
            return vartypes.Contains(type);
        }

        public bool IsLiteral(string val)
        {
            string literalPattern = @"\s*((?<float>\d+\.\d+)|(?<integer>\d+)|(?<string>"".+"")|(?<null>null))\s*";
            Regex regex = new Regex(literalPattern);
            Match match = regex.Match(val);

            if (match.Success)
            {
                return true;
            }

            return false;
        }

        public void Classify(CodeUnitManager manager, string code)
        {
            code = ProcessUsingStatements(manager, code);
            code = ProcessClassDeclarations(manager, code);
        }

        public enum RefValueKind
        {
            NewObject, Reference, Other
        }
    }
}
