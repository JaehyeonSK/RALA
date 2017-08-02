using CSVisualizerConsole.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVisualizerConsole.Modules
{
    public class MemoryManager
    {
        private static MemoryManager instance = null;

        public static MemoryManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new MemoryManager();
                return instance;
            }
        }

        private List<Dictionary<Guid, CSDV_VarInfo>> StackMemory { get; }
        private Dictionary<Guid, CSDV_VarInfo> HeapMemory { get; }

        private MemoryManager()
        {
            StackMemory = new List<Dictionary<Guid, CSDV_VarInfo>>();
            HeapMemory = new Dictionary<Guid, CSDV_VarInfo>();
        }

        public void CreateScope()
        {
            StackMemory.Add(new Dictionary<Guid, CSDV_VarInfo>());
        }

        /// <summary>
        /// 스택 최상위 스코프를 제거한 후 삭제되는 변수 리스트를 반환한다.
        /// </summary>
        /// <returns></returns>
        public List<Guid> DestroyScope()
        {
            if (StackMemory.Count < 1)
                throw new Exception("Stack Underflow!!");


            var destroyedGuid = from key in StackMemory.Last().Keys
                                select key;
            StackMemory.RemoveAt(StackMemory.Count - 1);

            return destroyedGuid.ToList();

        }

        public void CreateVariable(Guid guid, CSDV_VarInfo varInfo)
        {
            // 현재 스택에 변수 생성
            var currentScope = StackMemory.Last();
            if (currentScope.ContainsKey(guid))
                throw new Exception(string.Format($"{guid} has already been defined!!"));
            currentScope.Add(guid, varInfo);
        }

        public void CreateObject(Guid guid, CSDV_VarInfo varInfo)
        {
            // 힙에 객체 생성
            if (HeapMemory.ContainsKey(guid))
                throw new Exception(string.Format($"{guid} has already been defined!!"));
            HeapMemory.Add(guid, varInfo);
        }

        /// <summary>
        /// 변수에 객체의 레퍼런스를 할당하고 이전에 갖고 있던 레퍼런스의 Guid를 반환한다.
        /// </summary>
        /// <param name="varGuid">변수의 Guid</param>
        /// <param name="refGuid">객체 레퍼런스의 Guid</param>
        /// <returns></returns>
        public Guid AssignReference(Guid varGuid, Guid refGuid)
        {
            var currentScope = StackMemory.Last();

            // 변수가 현재 스코프에 없을 경우
            if (!currentScope.ContainsKey(varGuid))
                throw new Exception($"There is no variable({varGuid.ToString()}) in Current Scope.");

            // 변수가 레퍼런스 타입이 아닐 경우
            if (currentScope[varGuid].VarType != CSDV_VarInfo.CSDV_Type.REF_TYPE)
                throw new Exception($"Variable({varGuid.ToString()}) is not reference type.");

            // 레퍼런스 변수에 새로운 레퍼런스를 할당 하고 이전 레퍼런스의 Guid 반환
            Guid prevGuid = (Guid)currentScope[varGuid].Value;
            currentScope[varGuid].Value = refGuid;

            return prevGuid;
        }

        public void AssignValue(Guid varGuid, string value)
        {
            var currentScope = StackMemory.Last();

            // 변수가 현재 스코프에 없을 경우
            if (!currentScope.ContainsKey(varGuid))
                throw new Exception($"There is no variable({varGuid.ToString()}) in Current Scope.");

            // 변수가 값 타입이 아닐 경우
            if (currentScope[varGuid].VarType != CSDV_VarInfo.CSDV_Type.VAR_TYPE)
                throw new Exception($"Variable({varGuid.ToString()}) is not value type.");

            // 변수에 새로운 값 할당
            currentScope[varGuid].Value = value;
        }
    }
}
