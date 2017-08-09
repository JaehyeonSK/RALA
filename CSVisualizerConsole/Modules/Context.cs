using CSVisualizerConsole.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVisualizerConsole.Modules
{
    class Context
    {
        private static List<Guid> methodContextList = new List<Guid>();
        private static List<Guid> objectContextList = new List<Guid>();

        public static Guid CurrentObjectContext
        {
            get
            {
                return objectContextList.Last();
            }
        }

        public static Guid CurrentMethodContext
        {
            get
            {
                return methodContextList.Last();
            }
        }

        public static void CreateNewScope(bool isStatic, Guid objectGuid, Guid methodGuid, List<CSDV_VarInfo> args)
        {
            // 1) static, instance method 판단
            // 2) static일 경우
            //  2-1) objectContextList에 Guid.Empty 추가
            //  2-2) contextList에 메소드 Guid 추가
            // 3) instance일 경우
            //  3-1) objectContextList에 객체 Guid 추가
            //  3-2) contextList에 메소드 Guid 추가

            if (isStatic)
            {
                objectContextList.Push(Guid.Empty);
                methodContextList.Push(methodGuid);
            }
            else
            {
                objectContextList.Push(objectGuid);
                methodContextList.Push(methodGuid);
            }

            MemoryManager.Instance.CreateStack(methodGuid);
            if (args != null)
            {
                foreach (var varInfo in args)
                {
                    MemoryManager.Instance.CreateVariable(Guid.Empty, varInfo);
                }
            }
        }

        public static void DestoryCurrentScope()
        {
            MemoryManager.Instance.DestoryStack(methodContextList.Last());
            // 가장 뒤 요소들 제거
            objectContextList.RemoveAt(objectContextList.Count - 1);
            methodContextList.RemoveAt(methodContextList.Count - 1);
        }
    }
}
