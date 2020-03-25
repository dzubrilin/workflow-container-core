using System.Diagnostics;

namespace Diadem.Workflow.Core.Configuration
{
    [DebuggerDisplay("Code=[{Code}], HandlerCode=[{HandlerCode}]")]
    public sealed class StateEventConfiguration
    {
        public StateEventConfiguration()
        {
        }

        public StateEventConfiguration(string code, string handlerCode, ScriptTypeConfiguration scriptType, string script)
        {
            Code = code;
            HandlerCode = handlerCode;
            Script = script;
            ScriptType = scriptType;
        }

        public string Code { get; }

        public string HandlerCode { get; }

        public string Script { get; }

        public ScriptTypeConfiguration ScriptType { get; }
    }
}