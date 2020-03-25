using System;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Diadem.Workflow.Core.Configuration;
using Diadem.Workflow.Core.Execution.Events;
using Diadem.Workflow.Core.Scripting;
using Serilog;

namespace Diadem.Workflow.Core.Execution.EventHandlers
{
    public class CSharpScriptEventHandler : ICodeEventHandler
    {
        public CSharpScriptEventHandler(string code, ScriptTypeConfiguration scriptType, string script)
        {
            Code = code;
            Script = script;
            ScriptType = scriptType;
        }

        public string Script { get; }

        public ScriptTypeConfiguration ScriptType { get; }

        public string Code { get; }

        public async Task<EventExecutionResult> Execute(EventExecutionContext eventExecutionContext, CancellationToken cancellationToken = default)
        {
            var workflowContext = eventExecutionContext.WorkflowContext;
            if (ScriptType != ScriptTypeConfiguration.CSharp)
            {
                throw new WorkflowException(string.Format(CultureInfo.InvariantCulture,
                    "CSharpScriptEventHandler can execute only CSharp scripts but [{0}] has been provided for workflow [ID={1:D}]",
                    ScriptType, eventExecutionContext.WorkflowContext.WorkflowInstance.Id));
            }

            var scriptKey = $"EVENTHANDLER::{workflowContext.WorkflowConfiguration.Id:D}::{Code}::{Script.GetHashCode():D}";

            Assembly[] assemblies = null;
            var entity = workflowContext.WorkflowInstance.Entity;
            if (null != entity)
            {
                assemblies = new[] {entity.GetType().Assembly};
            }

            Exception exception;
            try
            {
                var cSharpScript = WorkflowEngine.ScriptingEngine.Value.GetScript<EventHandlerExecutionContextGlobals>(scriptKey, Script, assemblies);
                var scriptState = await cSharpScript.RunAsync(new EventHandlerExecutionContextGlobals
                {
                    context = eventExecutionContext,
                    entity = entity,
                    state = eventExecutionContext.WorkflowContext.WorkflowExecutionState,
                    workflowRuntime = workflowContext.WorkflowEngine,
                    cancellationToken = cancellationToken
                }, cancellationToken).ConfigureAwait(false);

                // if event handler returned "FALSE" --> do not move workflow instance from "AwaitingEvent"
                if (null != scriptState.ReturnValue && scriptState.ReturnValue is bool boolean && !boolean)
                {
                    return new EventExecutionResult(EventExecutionStatus.KeepWaiting);
                }

                if (null == scriptState.Exception)
                {
                    return new EventExecutionResult(EventExecutionStatus.Completed);
                }

                exception = scriptState.Exception;
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            Log.Error(exception, "An error has occurred during execution of scripted event [{state}, {code}] for {workflowInstanceId}]",
                eventExecutionContext.StateEventConfiguration.Code,
                eventExecutionContext.EventConfiguration.Code, eventExecutionContext.WorkflowContext.WorkflowInstance.Id);

            return new EventExecutionResult(EventExecutionStatus.Failed);
        }
    }
}