using CSVisualizer.Classes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CSVisualizer.Modules
{
    class Metadata
    {
        private static Dictionary<string, ClassInfo> classMap = new Dictionary<string, ClassInfo>();

        public static void Clear()
        {
            classMap = new Dictionary<string, ClassInfo>();
        }

        public static void RegisterClass(string className, ClassInfo classInfo)
        {
            classMap[className] = classInfo;
        }

        public static ClassInfo GetClassInfo(string className)
        {
            if (!classMap.ContainsKey(className))
                return null;

            return classMap[className];
        }

        public static Guid GetClassGuid(string className)
        {
            if (!classMap.ContainsKey(className))
                return Guid.Empty;
            
            return classMap[className].Guid;
        }

        public static Guid GetMethodGuid(string className, string methodName)
        {
            if (!classMap.ContainsKey(className))
                return Guid.Empty;

            var methodInfo = classMap[className].Methods.Find(m => m.Name == methodName);
            if (methodInfo == null)
                return Guid.Empty;

            return methodInfo.Guid;
        }

        public static MetadataBase FindByGuid(Guid guid)
        {
            foreach (var _class in classMap.Values)
            {
                // 해당 GUID를 갖는 클래스를 발견했을 경우
                if (_class.Guid == guid)
                    return _class;

                foreach (var method in _class.Methods)
                {
                    // 해당 GUID를 갖는 메소드를 발견했을 경우
                    if (method.Guid == guid)
                        return method;
                }

                foreach (var field in _class.Fields)
                {
                    // 해당 GUID를 갖는 필드를 발견했을 경우
                    if (field.Guid == guid)
                        return field;
                }
            }

            return null;
        }

        public static MethodInfo GetEntryMethod()
        {
            MethodInfo method;
            foreach (var classInfo in classMap.Values)
            {
                if ((method = classInfo.Methods.Find(e=>e.Name == "Main")) != null)
                {
                    return method;
                }
            }

            throw new Exception("there is no entry method(Main) !!");
        }

        public static MethodInfo[] GetMethods(string className)
        {
            if (!classMap.ContainsKey(className))
                return null;
            return classMap[className].Methods.ToArray();
        }

        public static MethodInfo GetMethod(string className, string methodName)
        {
            if (!classMap.ContainsKey(className))
                return null;
            return classMap[className].Methods.Find(e => e.Name == methodName);
        }

        public static FieldInfo[] GetFields(string className)
        {
            if (!classMap.ContainsKey(className))
                return null;
            return classMap[className].Fields.ToArray();
        }

        public static FieldInfo GetField(string className, string fieldName)
        {
            if (!classMap.ContainsKey(className))
                return null;
            return classMap[className].Fields.Find(e => e.Name == fieldName);
        }


    }
}
