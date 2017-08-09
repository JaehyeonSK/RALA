using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace CSVisualizer.Modules
{
    public class FileReader
    {
        private static FileReader instance;

        public static FileReader Instance
        {
            get
            {
                if (instance == null)
                    instance = new FileReader();
                return instance;
            }
        }

        public string Code { get; set; }

        public bool OpenTextFile(string path)
        {
            if (File.Exists(path))
            {
                StringBuilder sb = new StringBuilder();
                string text = File.ReadAllText(path);
                
                // 네임스페이스를 제거한 코드 저장
                sb.Append(GetUsingStatement(text));
                sb.Append(RemoveNamespace(text));

                Code = sb.ToString();

                return true;
            }
            else
            {
                return false;
            }
        }

        private string GetUsingStatement(string code)
        {
            var endIndex = code.IndexOf("namespace");

            if (endIndex < 0)
            {
                return code;
            }
            return code.Substring(0, endIndex);
        }

        private string RemoveNamespace(string code)
        {
            // 최상위 여는 괄호, 최하위 닫는 괄호 사이의 문자열을 구하는 패턴
            // const string nsPattern = @"namespace\s+.+\s*{(?<content>\s*(.+\n)+\s*)}";
            // 여는 괄호와 닫는 괄호의 개수를 카운트하여 블록 내의 문자열을 구하는 패턴
            const string nsPattern2 = @"namespace\s+.+\s*\{(?<content>(?>\{(?<c>)|[^{}]+|\}(?<-c>))*(?(c)(?!)))\}";

            Regex regex = new Regex(nsPattern2);
            Match match = regex.Match(code);

            // 네임스페이스 블럭을 찾았을경우
            // 블럭 내부의 내용 반환
            if (match.Success)
            {
                return match.Groups["content"].Value;
            }
            // 네임스페이스 블럭이 없을 경우 예외 발생
            throw new System.Exception("there is no namespace block!!");
        }

        private FileReader()
        {
        }
    }
}
