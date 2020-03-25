using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Diadem.Workflow.Core.Configuration;
using Diadem.Workflow.Core.Scripting;

namespace Diadem.Workflow.Core.Execution.Activities
{
    public class CSharpScriptActivity : ICodeScriptActivity
    {
        public CSharpScriptActivity(string code, ScriptTypeConfiguration scriptType, string script)
        {
            Code = code;
            Script = script;
            ScriptType = scriptType;
        }

        public ScriptTypeConfiguration ScriptType { get; }

        public string Code { get; }

        public string Script { get; }

        public async Task<ActivityExecutionResult> Execute(ActivityExecutionContext activityExecutionContext, CancellationToken cancellationToken = default)
        {
            var workflowContext = activityExecutionContext.StateExecutionContext.WorkflowContext;
            if (ScriptType != ScriptTypeConfiguration.CSharp)
            {
                throw new WorkflowException(string.Format(CultureInfo.InvariantCulture,
                    "CSharpScriptActivity can execute only CSharp scripts but [{0}] has been provided for workflow [ID={1:D}]",
                    ScriptType, workflowContext.WorkflowInstance.Id));
            }

            var scriptKey = $"ACTIVITY::{workflowContext.WorkflowConfiguration.Id:D}::{Code}::{Script.GetHashCode():D}";

            Assembly[] assemblies = null;
            var entity = workflowContext.WorkflowInstance.Entity;
            if (null != entity)
            {
                assemblies = new[] {entity.GetType().Assembly};
            }

            var cSharpScript = WorkflowEngine.ScriptingEngine.Value.GetScript<ActivityExecutionContextGlobals>(scriptKey, Script, assemblies);
            var scriptState = await cSharpScript.RunAsync(new ActivityExecutionContextGlobals
            {
                context = activityExecutionContext,
                entity = entity,
                state = activityExecutionContext.StateExecutionContext.WorkflowContext.WorkflowExecutionState,
                workflowRuntime = workflowContext.WorkflowEngine,
                cancellationToken = cancellationToken
            }, cancellationToken).ConfigureAwait(false);

            if (null == scriptState.Exception)
            {
                return new ActivityExecutionResult(ActivityExecutionStatus.Completed);
            }

            throw scriptState.Exception;
        }
    }
}