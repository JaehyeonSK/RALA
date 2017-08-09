using CSVisualizer.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace CSVisualizer.Modules
{
    public class Context
    {
        public enum MemoryType { Stack, Heap }
        
        private static List<Guid> methodContextList = null;
        private static List<Guid> objectContextList = null;

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

        public static void Init()
        {
            methodContextList = new List<Guid>();
            objectContextList = new List<Guid>();
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

            //MessageBox.Show("Create Stack > " + methodContextList.Last().Shorten());
            //var temp = new StreamWriter(new FileStream("temp.txt", FileMode.Append));
            //temp.WriteLine("Create Stack > " + methodContextList.Last().Shorten());
            //temp.Close();
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
            //var temp = new StreamWriter(new FileStream("temp.txt", FileMode.Append));
            //temp.WriteLine("Destroy Stack > " + methodContextList.Last().Shorten());
            //temp.Close();
            //MessageBox.Show("Destroy Stack > " + methodContextList.Last().Shorten());
            MemoryManager.Instance.DestoryStack(methodContextList.Last());
            // 가장 뒤 요소들 제거
            objectContextList.RemoveAt(objectContextList.Count - 1);
            methodContextList.RemoveAt(methodContextList.Count - 1);
        }
    }
}
