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
    class MetadataBuilder
    {
        private static FieldInfo BuildFieldInfo(ClassInfo classInfo, string code)
        {
            //TODO: 구현 필요
            throw new NotImplementedException();
        }

        public static ClassInfo BuildClassInfo(Guid guid, string name, string content)
        {
            // modifier:    접근지정자
            // return:      반환 타입
            // name:        메소드이름
            // params:      메소드 파라미터 (타입 이름, ...)
            // content:     메소드 내용
            string methodPattern = @"(?<modifier>private|public|protected)\s+(?<return>\w+)\s+(?<name>[a-zA-Z_][a-zA-Z0-9_]*)\((?<params>.*)\)\s*{(?<content>(?>\{(?<c>)|[^{}]+|\}(?<-c>))*(?(c)(?!)))\}";
            Regex regex = new Regex(methodPattern);

            ClassInfo newClass = new ClassInfo(guid, name, ClassInfo.AccessModifier.Public);

            // 클래스 내의 메소드 찾기
            var matches = regex.Matches(content);
            foreach (Match match in matches)
            {
                string modifier = match.Groups["modifier"].Value;
                string returnType = match.Groups["return"].Value;
                string methodName = match.Groups["name"].Value;
                string param = match.Groups["params"].Value;
                string methodBody = match.Groups["content"].Value;

                // 접근지정자 선택
                ClassInfo.AccessModifier mod = ClassInfo.AccessModifier.Private;
                if (!string.IsNullOrWhiteSpace(modifier))
                {
                    switch (modifier)
                    {
                        case "public": mod = ClassInfo.AccessModifier.Public; break;
                        case "protected": mod = ClassInfo.AccessModifier.Protected; break;
                        case "private": mod = ClassInfo.AccessModifier.Private; break;
                    }
                }
                /*
                 * 문자열로 구성된 파라미터 목록을 분리하여
                 * paramList 리스트에 추가
                 */
                List<CSDV_VarInfo> paramList = new List<CSDV_VarInfo>();
                if (!string.IsNullOrWhiteSpace(param))
                {
                    string[] ps = param.Split(',');
                    foreach (string p in ps)
                    {
                        string[] tn = p.Split(' ');
                        paramList.Add(new CSDV_VarInfo()
                        {
                            Type = tn[0].Trim(),
                            Name = tn[1].Trim()
                        });
                    }
                }

                // 메소드 식별에 사용되는 Guid 생성
                // 메소드 코드 유닛의 Guid와 메소드 메타데이터의 Guid는 동일함.
                Guid methodGuid = Guid.NewGuid();

                // 클래스 메타데이터의 메소드 목록에 추가
                MethodInfo method = new MethodInfo(
                    guid: methodGuid,
                    name: methodName,
                    paramInfo: paramList.ToArray(),
                    isStatic: false,
                    modifier: mod
                    );
                newClass.Methods.Add(method);

                // 메소드 선언 유닛 생성 및 추가
                MethodDeclUnit methodUnit = new MethodDeclUnit(methodGuid)
                {
                    Name = methodName,
                    Content = methodBody
                };
                CodeUnitManager.Instance.DeclareList.Add(methodUnit);
            }

            return newClass;
        }
    }
}
