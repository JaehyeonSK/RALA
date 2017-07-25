using CSVisualizerConsole.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVisualizerConsole.Modules
{
    class CodeUnitParser
    {
        public static string NextWord(string code, ref int pos)
        {
            int start = 0, end = 0;

            for (int i = pos; i < code.Length; i++)
            {
                if (code[i] != ' ' && code[i] != '\t' && code[i] != '\r' && code[i] != '\n')
                {
                    pos = start = i;
                    break;
                }
            }

            for (int i = start; i < code.Length; i++)
            {
                if (code[i] == ' ' || code[i] == '\t' || code[i] == '\r' || code[i] == '\n')
                {
                    end = i;
                    break;
                }
            }

            return code.Substring(start, end - start);
        }

        public static string NextWord(string code)
        {
            int start = 0, end = 0;

            for (int i=0; i<code.Length; i++)
            {
                if (code[i] != ' ' || code[i] == '\r' || code[i] == '\n')
                {
                    start = i;
                    break;
                }
            }

            for (int i=start; i<code.Length; i++)
            {
                if (code[i] == ' ' || code[i] == '\r' || code[i] == '\n')
                {
                    end = i;
                    break;
                }
            }

            return code.Substring(start, end - start);
        }

        public static object BuildClassInfo(ClassInfo classInfo, string classContent)
        {
            //TODO: 클래스 정보 생성 메소드 구현

            throw new NotImplementedException();
        }
    }
}
