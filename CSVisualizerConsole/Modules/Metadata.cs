using CSVisualizerConsole.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVisualizerConsole.Modules
{
    class Metadata
    {
        private static Dictionary<string, object> classMap;

        public static void RegisterClass(string className, ClassInfo classInfo)
        {
            classMap[className] = classInfo;
        }

        public static MethodInfo[] GetMethods(string className)
        {
            // TODO: 반환 자료형 MethodInfo 타입으로
            throw new NotImplementedException();
        }

        public static MethodInfo GetMethod(string className, string methodName)
        {
            throw new NotImplementedException();
        }

        public static FieldInfo[] GetFields(string className)
        {
            throw new NotImplementedException();
        }

        public static FieldInfo GetField(string className, string fieldName)
        {
            throw new NotImplementedException();
        }


    }
}
