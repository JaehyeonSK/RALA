using CSVisualizer.Classes;
using CSVisualizer.Classes.Units;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CSVisualizer.Modules
{
    class MetadataBuilder
    {
        private static void BuildFieldInfos(ClassInfo classInfo, string code)
        {
            string fieldPattern = @"\s*((?<modifier>private|public|protected)\s+)?(?<type>\w+)\s+(?<name>\w+);";
            Regex regex = new Regex(fieldPattern);

            var matches = regex.Matches(code);
            foreach (Match match in matches)
            {
                string type = match.Groups["type"].Value;
                string mod = match.Groups["modifier"].Value;
                string name = match.Groups["name"].Value;
                FieldInfo fInfo = new FieldInfo(Guid.NewGuid(), type, name, false);

                classInfo.Fields.Add(fInfo);
            }
        }

        public static ClassInfo BuildClassInfo(Guid guid, string name, string content)
        {
            // modifier:    접근지정자
            // return:      반환 타입
            // name:        메소드이름
            // params:      메소드 파라미터 (타입 이름, ...)
            // content:     메소드 내용
            //string methodPattern = @"(?<modifier>private|public|protected)\s+(?<return>\w+)\s+(?<name>[a-zA-Z_][a-zA-Z0-9_]*)\((?<params>.*)\)\s*{(?<content>(?>\{(?<c>)|[^{}]+|\}(?<-c>))*(?(c)(?!)))\}";
            string methodPattern = @"(?<modifier>private|public|protected)(\s(?<static>static))*\s+(?<return>\w+)\s+(?<name>[a-zA-Z_][a-zA-Z0-9_]*)\((?<params>.*)\)\s*{(?<content>(?>\{(?<c>)|[^{}]+|\}(?<-c>))*(?(c)(?!)))\}";
            Regex regex = new Regex(methodPattern);

            ClassInfo newClass = new ClassInfo(guid, name, ClassInfo.AccessModifier.Public);

            // 클래스 내의 메소드 찾기
            var matches = regex.Matches(content);
            foreach (Match match in matches)
            {
                string modifier = match.Groups["modifier"].Value;
                string returnType = match.Groups["return"].Value;
                string methodName = match.Groups["name"].Value;
                bool isStatic = string.IsNullOrWhiteSpace(match.Groups["static"].Value) ? false : true;
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
                        var _type = tn[0].Trim();
                        var _name = tn[1].Trim();

                        paramList.Add(new CSDV_VarInfo()
                        {
                            Type = _type,
                            Name = _name,
                            VarType = Classifier.Instance.IsVarType(_type) ? 
                            CSDV_VarInfo.CSDV_Type.VAR_TYPE : CSDV_VarInfo.CSDV_Type.REF_TYPE
                        });
                    }
                }

                // 메소드 식별에 사용되는 Guid 생성
                // 메소드 코드 유닛의 Guid와 메소드 메타데이터의 Guid는 동일함.
                Guid methodGuid = Guid.NewGuid();

                // 클래스 메타데이터의 메소드 목록에 추가
                MethodInfo method = new MethodInfo(
                    guid: methodGuid,
                    className: name,
                    name: methodName,
                    returnType: returnType,
                    paramInfo: paramList.ToArray(),
                    isStatic: isStatic,
                    modifier: mod
                    );
                newClass.Methods.Add(method);

                // 메소드 선언 유닛 생성 및 추가
                var methodCodeUnitList = Classifier.Instance.ProcessMethodBody(methodBody);

               MethodDeclUnit methodUnit = new MethodDeclUnit(methodGuid)
                {
                    Name = methodName,
                    Content = methodBody,
                    CodeUnitList = methodCodeUnitList
                };

                CodeUnitManager.Instance.DeclareList.Add(methodUnit);
            }

            BuildFieldInfos(newClass, content);

            return newClass;
        }
    }
}
