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
        private int curPos = 0;
        private int endPos = -1;

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
                    Content = content
                };
                manager.DeclareList.Add(classUnit);

                var classInfo = MetadataBuilder.BuildClassInfo(newGuid, className, content);
                Metadata.RegisterClass(className, classInfo);

                // 처리된 클래스 선언 제거
                code = code.Replace(match.Value, "");
            }

            return code;
        }

        public void Classify(CodeUnitManager manager, string code)
        {
            code = ProcessUsingStatements(manager, code);
            code = ProcessClassDeclarations(manager, code);

        }
    }
}
