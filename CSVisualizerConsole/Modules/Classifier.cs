using CSVisualizerConsole.Classes;
using CSVisualizerConsole.Classes.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        private CodeUnit ProcessUsing(string code)
        {
            endPos = code.IndexOf(';', curPos) + 1;
            var unit = new CodeUnit()
            {
                Content = code.Substring(curPos, endPos - curPos)
            };
            curPos = endPos;
            return unit;
        }

        private CodeUnit ProcessVariable(string code)
        {
            endPos = code.IndexOf(';', curPos) + 1;
            var unit = new VarDeclUnit()
            {
                Content = code.Substring(curPos, endPos - curPos)
            };
            curPos = endPos;
            return unit;
        }

        private CodeUnit ProcessClass(string code)
        {
            int cnt = 0, idx;

            curPos += 5; // skip class keyword
            var name = CodeUnitParser.NextWord(code, ref curPos);

            idx = code.IndexOf('{', curPos);
            for (idx = curPos; idx < code.Length; idx++)
            {
                if (code[idx] == '{')
                    ++cnt;
                else if (code[idx] == '}')
                    --cnt;

                if (cnt == 0)
                { // 매칭되는 닫는 괄호를 발견했을 경우
                    endPos = idx + 1;
                    break;
                }
            }

            if (idx == code.Length)
            { // 매칭되는 닫는 괄호를 발견하지 못했을 경우
                throw new Exception("bracket doesn't match!!");
            }

            var unit = new ClassDeclUnit()
            {
                Name = name,
                Content = code.Substring(curPos, endPos - curPos)
            };

            // TODO: 아래 2줄 구현 필요
            
            

            curPos = endPos;
            return unit;
        }

        private CodeUnit ProcessInterface(string code)
        {
            throw new NotImplementedException();
        }

        public List<CodeUnit> Classify(string code)
        {
            List<CodeUnit> unitList = new List<CodeUnit>();

            while (!string.IsNullOrWhiteSpace(code))
            {
                bool notKeyword = false;
                string word = CodeUnitParser.NextWord(code, ref curPos);
                CodeUnit codeUnit = null;

                switch (word)
                {
                    case "using": // using 코드유닛 처리
                        codeUnit = ProcessUsing(code);
                        break;
                    case "class": // class 선언 코드유닛 처리
                        codeUnit = ProcessClass(code);
                        break;
                    case "interface": // interface 선언 코드유닛 처리
                        codeUnit = ProcessInterface(code);
                        break;
                    case "byte": // 변수 선언에 대한 코드유닛 처리
                    case "sbyte":
                    case "char":
                    case "short":
                    case "ushort":
                    case "int":
                    case "uint":
                    case "long":
                    case "ulong":
                    case "float":
                    case "double":
                    case "decimal":
                        codeUnit = ProcessVariable(code);
                        break;
                    default:
                        notKeyword = true;
                        break;
                }

                // 키워드가 아닌 것들에 대한 처리
                // 함수 호출, 할당 등
                if (notKeyword)
                {

                }

                // 더 이상 처리할 코드유닛이 없을 경우 리스트 반환
                if (codeUnit == null)
                {
                    return unitList;
                }

                // 리스트에 현재 코드유닛 추가
                unitList.Add(codeUnit);
            }

            return null;
        }
    }
}
