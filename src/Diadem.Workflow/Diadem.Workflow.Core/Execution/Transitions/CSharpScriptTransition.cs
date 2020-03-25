using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Diadem.Workflow.Core.Configuration;
using Diadem.Workflow.Core.Scripting;
using Serilog;

namespace Diadem.Workflow.Core.Execution.Transitions
{
    public class CSharpScriptTransition : ITransition
    {
        public CSharpScriptTransition(string code, ScriptTypeConfiguration scriptType, string script)
        {
            Code = code;
            Script = script;
            ScriptType = scriptType;
        }

        public string Code { get; }

        public string Script { get; }

        public ScriptTypeConfiguration ScriptType { get; }

        public async Task<TransitionEvaluationResult> Evaluate(TransitionEvaluationContext transitionEvaluationContext, CancellationToken cancellationToken)
        {
            var workflowContext = transitionEvaluationContext.StateExecutionContext.WorkflowContext;
            if (ScriptType != ScriptTypeConfiguration.CSharp)
            {
                throw new WorkflowException(string.Format(CultureInfo.InvariantCulture,
                    "CSharpScriptTransition can execute only CSharp scripts but [{0}] has been provided for workflow [ID={1:D}]",
                    ScriptType, workflowContext.WorkflowInstance.Id));
            }

            var scriptKey = string.Format(CultureInfo.InvariantCulture, "TRANSITION::{0:D}::{1}::{2}::{3:D}",
                workflowContext.WorkflowConfiguration.Id, transitionEvaluationContext.StateExecutionContext.StateConfiguration.Code, Code,
                Script.GetHashCode());

            Assembly[] assemblies = null;
            var entity = workflowContext.WorkflowInstance.Entity;
            if (null != entity)
            {
                assemblies = new[] {entity.GetType().Assembly};
            }

            var cSharpScript = WorkflowEngine.ScriptingEngine.Value.GetScript<TransitionExecutionContextGlobals>(scriptKey, Script, assemblies);
            var scriptState = await cSharpScript.RunAsync(new TransitionExecutionContextGlobals
            {
                context = transitionEvaluationContext,
                entity = entity,
                state = transitionEvaluationContext.StateExecutionContext.WorkflowContext.WorkflowExecutionState,
                workflowRuntime = workflowContext.WorkflowEngine,
                cancellationToken = cancellationToken
            }, cancellationToken).ConfigureAwait(false);

            if (null == scriptState.Exception)
            {
                return (bool) scriptState.ReturnValue
                    ? new TransitionEvaluationResult(TransitionEvaluationStatus.EvaluatedTrue)
                    : new TransitionEvaluationResult(TransitionEvaluationStatus.EvaluatedFalse);
            }

            Log.Error(scriptState.Exception,"An error has occurred during evaluation of transition [{state}, {code}] for {workflowInstanceId}]",
                transitionEvaluationContext.StateExecutionContext.StateConfiguration.Code,
                transitionEvaluationContext.TransitionConfiguration.Code,
                transitionEvaluationContext.StateExecutionContext.WorkflowContext.WorkflowInstance.Id);

            return new TransitionEvaluationResult(TransitionEvaluationStatus.EvaluationFailed);
        }
    }
}