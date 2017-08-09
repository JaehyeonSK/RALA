using CSVisualizer.Classes;
using System;
using System.Collections.Generic;
using static CSVisualizer.Modules.Context;

namespace CSVisualizer.Modules
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

        // 바깥 Guid: 객체 혹은 메소드를 식별하기 위한 Guid
        // 안쪽 Guid: 내부 변수를 식별하기 위한 Guid
        private Dictionary<Guid, Dictionary<Guid, CSDV_VarInfo>> StackMemory;

        // Guid: 객체를 식별하기 위한 Guid
        // List<CSDV_VarInfo>: 객체의 필드 정보
        private Dictionary<Guid, List<CSDV_VarInfo>> HeapMemory;

        private MemoryManager()
        {
            Init();
        }

        public void Init()
        {
            StackMemory = new Dictionary<Guid, Dictionary<Guid, CSDV_VarInfo>>();
            HeapMemory = new Dictionary<Guid, List<CSDV_VarInfo>>();
        }

        public void CreateStack(Guid guid)
        {
            StackMemory.Add(guid, new Dictionary<Guid, CSDV_VarInfo>());

            GuiHandler.Instance.CreateStack(guid);
        }

        public void DestoryStack(Guid guid)
        {
            StackMemory.Remove(guid);

            GuiHandler.Instance.DestroyStack(guid);
        }

        public void CreateVariable(Guid guid, CSDV_VarInfo varInfo)
        {
            // 현재 스택에 변수 생성
            var currentScope = Context.CurrentMethodContext;
            if (StackMemory[currentScope].ContainsKey(guid))
                throw new Exception($"{guid} has already been defined!!");
            StackMemory[currentScope].Add(guid, varInfo);

            GuiHandler.Instance.CreateVariable(Context.CurrentMethodContext, varInfo);
        }

        public void CreateObject(Guid guid, List<CSDV_VarInfo> varInfos)
        {
            // 힙에 객체 생성
            if (HeapMemory.ContainsKey(guid))
                throw new Exception($"{guid} has already been defined!!");
            HeapMemory.Add(guid, varInfos);

            GuiHandler.Instance.CreateObject(guid, varInfos);
        }

        public List<CSDV_VarInfo> GetObject(Guid objectGuid)
        {
            if (!HeapMemory.ContainsKey(objectGuid))
                throw new Exception($"{objectGuid} has not been defined yet!");
            return HeapMemory[objectGuid];
        }

        public CSDV_VarInfo GetVariable(Guid guid)
        {
            var methodContext = Context.CurrentMethodContext;
            if (StackMemory[methodContext].ContainsKey(guid))
                return StackMemory[methodContext][guid];

            return null;
        }

        public Guid GetVariableGuid(string name, out MemoryType memType)
        {
            var methodContext = Context.CurrentMethodContext;

            // person.name = ? 형태
            if (name.Contains("."))
            {
                var objName = name.Substring(0, name.LastIndexOf("."));
                var fieldName = name.Substring(name.LastIndexOf(".") + 1);
                Guid objGuid = Guid.Empty;

                foreach (var kv in StackMemory[methodContext])
                {
                    if (kv.Value.Name == objName)
                    {
                        objGuid = (Guid)kv.Value.Value;
                        break;
                    }
                }

                if (objGuid == Guid.Empty)
                    throw new Exception($"can't find object named {objName}");

                memType = MemoryType.Heap;
                return objGuid;
            }
            else  
            {
                // name = ? 형태
                foreach (var kv in StackMemory[methodContext])
                {
                    if (kv.Value.Name == name)
                    {
                        memType = MemoryType.Stack;
                        return kv.Key;
                    }
                }

                memType = MemoryType.Heap;
                var objectContext = Context.CurrentObjectContext;
                if (objectContext == Guid.Empty)
                    return Guid.Empty;

                
                /*
                 * class Person 
                 * {
                 *     string name;
                 *     public void Do() 
                 *     {
                 *         name = ? 
                 *     }
                 * }
                 * 위와 같은 형태
                 */
                return objectContext;
            }
        }

        public CSDV_VarInfo GetVariable(string name)
        {
            var methodContext = Context.CurrentMethodContext;

            if (name.Contains("."))
            {
                var objName = name.Substring(0, name.LastIndexOf("."));
                var fieldName = name.Substring(name.LastIndexOf(".") + 1);
                Guid objGuid = Guid.Empty;

                foreach (var kv in StackMemory[methodContext])
                {
                    if (kv.Value.Name == objName)
                    {
                        objGuid = (Guid)kv.Value.Value;
                    }
                }

                if (objGuid == Guid.Empty)
                    throw new Exception($"can't find object named {objName}");

                return HeapMemory[objGuid].Find(var => var.Name == fieldName);
            }
            else
            {

                foreach (CSDV_VarInfo value in StackMemory[methodContext].Values)
                {
                    if (value.Name == name)
                    {
                        return value;
                    }
                }

                var objectContext = Context.CurrentObjectContext;
                if (objectContext == Guid.Empty)
                    return null;

                return HeapMemory[objectContext].Find(var => var.Name == name);
            }
        }

        ///// <summary>
        ///// 변수에 객체의 레퍼런스를 할당하고 이전에 갖고 있던 레퍼런스의 Guid를 반환한다.
        ///// </summary>
        ///// <param name="varGuid">변수의 Guid</param>
        ///// <param name="refGuid">객체 레퍼런스의 Guid</param>
        ///// <returns></returns>
        //public Guid AssignReference(Guid varGuid, Guid refGuid)
        //{
        //    var currentScope = Context.CurrentMethodContext;

        //    // 변수가 현재 메소드 스코프에 없을 경우
        //    if (!StackMemory[currentScope].ContainsKey(varGuid))
        //        throw new Exception($"There is no variable({varGuid.ToString()}) in Current Scope.");

        //    // 변수가 레퍼런스 타입이 아닐 경우
        //    if (StackMemory[currentScope][varGuid].VarType != CSDV_VarInfo.CSDV_Type.REF_TYPE)
        //        throw new Exception($"Variable({varGuid.ToString()}) is not reference type.");

        //    // 레퍼런스 변수에 새로운 레퍼런스를 할당 하고 이전 레퍼런스의 Guid 반환
        //    Guid prevGuid = (Guid)StackMemory[currentScope][varGuid].Value;
        //    StackMemory[currentScope][varGuid].Value = refGuid;

        //    return prevGuid;
        //}

        //public void AssignValue(Guid varGuid, string value)
        //{
        //    var currentScope = Context.CurrentMethodContext;

        //    // 변수가 현재 스코프에 없을 경우
        //    if (!StackMemory[currentScope].ContainsKey(varGuid))
        //        throw new Exception($"There is no variable({varGuid.ToString()}) in Current Scope.");

        //    // 변수가 값 타입이 아닐 경우
        //    if (StackMemory[currentScope][varGuid].VarType != CSDV_VarInfo.CSDV_Type.VAR_TYPE)
        //        throw new Exception($"Variable({varGuid.ToString()}) is not value type.");

        //    // 변수에 새로운 값 할당
        //    StackMemory[currentScope][varGuid].Value = value;

        //    GuiHandler.Instance.AssignVariable(Context.CurrentMethodContext, StackMemory[currentScope][varGuid]);
        //}

        public void AssignReference(string name, Guid refGuid)
        {
            GetVariable(name).Value = refGuid;

            MemoryType memType;
            var guid = GetVariableGuid(name, out memType);
            if (memType == MemoryType.Stack)
                GuiHandler.Instance.AssignReference(guid, GetVariable(name));
            else
                GuiHandler.Instance.AssignReference(guid, GetVariable(name));
        }

        public void AssignValue(string name, string value)
        {
            GetVariable(name).Value = value;

            MemoryType memType;
            var guid = GetVariableGuid(name, out memType);
            if (memType == MemoryType.Stack)
                GuiHandler.Instance.AssignVariable(guid, GetVariable(name));
            else
                GuiHandler.Instance.AssignVariable(guid, GetVariable(name));
        }
    }
}
