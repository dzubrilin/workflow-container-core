using System;
using System.Diagnostics;

namespace Diadem.Workflow.Core.Configuration
{
    [DebuggerDisplay("Type=[{Type}], Code=[{Code}], MoveToState=[{MoveToState}]")]
    public sealed class TransitionConfiguration
    {
        public TransitionConfiguration(ScriptTypeConfiguration conditionScriptType)
        {
            ConditionScriptType = conditionScriptType;
        }

        public TransitionConfiguration(TransitionTypeConfiguration type, string code, string moveToState)
        {
            Code = code;
            MoveToState = moveToState;
            Type = type;
            VerifyInvariants();
        }

        public TransitionConfiguration(TransitionTypeConfiguration type, string code, string parameters,
            string moveToState, ScriptTypeConfiguration conditionScriptType, string conditionScript, int? delay)
        {
            Type = type;
            Code = code;
            Parameters = parameters;
            MoveToState = moveToState;
            ConditionScript = conditionScript;
            Delay = delay;
            ConditionScriptType = conditionScriptType;
            VerifyInvariants();
        }

        public string Code { get; }

        public string Parameters { get; }

        public string ConditionScript { get; }

        public ScriptTypeConfiguration ConditionScriptType { get; }

        public int? Delay { get; }

        public string MoveToState { get; }

        public TransitionTypeConfiguration Type { get; }

        private void VerifyInvariants()
        {
            if (Type == TransitionTypeConfiguration.AsynchronousWithDelay && !Delay.HasValue)
            {
                throw new Exception("Asynchronous transition must have delay specified");
            }
        }
    }
}