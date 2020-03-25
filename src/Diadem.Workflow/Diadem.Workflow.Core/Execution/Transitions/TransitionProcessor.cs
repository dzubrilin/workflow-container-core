using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Diadem.Workflow.Core.Configuration;
using Diadem.Workflow.Core.Execution.States;
using Serilog;

namespace Diadem.Workflow.Core.Execution.Transitions
{
    public class TransitionProcessor : ITransitionProcessor
    {
        private readonly WorkflowEngine _workflowEngine;

        public TransitionProcessor(WorkflowEngine workflowEngine)
        {
            _workflowEngine = workflowEngine;
        }

        public async Task<TransitionEvaluationResult> Evaluate(StateExecutionContext stateExecutionContext, TransitionConfiguration transitionConfiguration, CancellationToken cancellationToken)
        {
            ITransition transition = null;
            if (!string.IsNullOrEmpty(transitionConfiguration.Code))
            {
                transition = _workflowEngine.WorkflowEngineBuilder
                    .TransitionFactory.CreateTransition(transitionConfiguration.Code);
            }

            if (null == transition && !string.IsNullOrEmpty(transitionConfiguration.ConditionScript)
                                   && transitionConfiguration.ConditionScriptType == ScriptTypeConfiguration.CSharp)
            {
                transition = new CSharpScriptTransition(stateExecutionContext.StateConfiguration.Code,
                    transitionConfiguration.ConditionScriptType, transitionConfiguration.ConditionScript);
            }

            if (null == transition)
            {
                // there is neither code nor condition for transition
                //     ==> assume transition always evaluates to true
                return new TransitionEvaluationResult(TransitionEvaluationStatus.EvaluatedTrue);
            }

            Log.Verbose("Starting evaluation of transition [{code}] [{condition}] for [{workflowInstanceId}]",
                transitionConfiguration.Code, transitionConfiguration.ConditionScriptType,
                stateExecutionContext.WorkflowContext.WorkflowInstance.Id);

            try
            {
                var transitionEvaluationContext = new TransitionEvaluationContext(stateExecutionContext, transitionConfiguration);
                var transitionEvaluationResult = await transition.Evaluate(transitionEvaluationContext, cancellationToken).ConfigureAwait(false);
                if (transitionEvaluationResult.Status == TransitionEvaluationStatus.EvaluationFailed)
                {
                    throw new WorkflowException(string.Format(CultureInfo.InvariantCulture,
                        "Evaluation of an transition [code={0}], [condition={1}] returns [EvaluationFailed] for workflow instance [{2:D}]",
                        transitionConfiguration.Code, transitionConfiguration.ConditionScript,
                        stateExecutionContext.WorkflowContext.WorkflowInstance.Id));
                }

                return transitionEvaluationResult;
            }
            catch (Exception ex)
            {
                throw new WorkflowException(string.Format(CultureInfo.InvariantCulture,
                    "An error has occurred during evaluation of an transition [code={0}], [condition={1}] for workflow instance [{2:D}]",
                    transitionConfiguration.Code, transitionConfiguration.ConditionScript,
                    stateExecutionContext.WorkflowContext.WorkflowInstance.Id), ex);
            }
        }
    }
}