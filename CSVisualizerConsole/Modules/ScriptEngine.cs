using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace CSVisualizerConsole
{
    public class ScriptEngine
    {
        private static ScriptState<object> state = null;
        public static object Execute(string code)
        {
            state = state == null ? CSharpScript.RunAsync(code).Result : state.ContinueWithAsync(code).Result;
            if (state.ReturnValue != null && !string.IsNullOrEmpty(state.ReturnValue.ToString()))
            {
                return state.ReturnValue;
            }
            return null;
        }
    }
}
