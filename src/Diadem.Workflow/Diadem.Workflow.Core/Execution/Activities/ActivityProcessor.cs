using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Diadem.Workflow.Core.Configuration;
using Diadem.Workflow.Core.Model;
using Diadem.Workflow.Core.Runtime;
using Serilog;
using Serilog.Events;

namespace Diadem.Workflow.Core.Execution.Activities
{
    internal class ActivityProcessor : IActivityProcessor
    {
        private static readonly bool IsDebugEnabled = Log.IsEnabled(LogEventLevel.Debug);
        
        private static readonly bool IsVerboseEnabled = Log.IsEnabled(LogEventLevel.Verbose);
        
        private readonly WorkflowEngine _workflowEngine;

        public ActivityProcessor(WorkflowEngine workflowEngine)
        {
            _workflowEngine = workflowEngine;
        }

        public async Task<ActivityExecutionResult> Process(ActivityExecutionContext activityExecutionContext, CancellationToken cancellationToken = default)
        {
            var activity = _workflowEngine.WorkflowEngineBuilder.ActivityFactory.CreateActivity(activityExecutionContext.ActivityConfiguration.Code);

            var tryCount = 0;
            var started = DateTime.UtcNow;
            var stopwatch = Stopwatch.StartNew();
            try
            {
                if (null == activity && !string.IsNullOrEmpty(activityExecutionContext.ActivityConfiguration.Script)
                                     && activityExecutionContext.ActivityConfiguration.ScriptType == ScriptTypeConfiguration.CSharp)
                {
                    activity = new CSharpScriptActivity(activityExecutionContext.ActivityConfiguration.Code,
                        activityExecutionContext.ActivityConfiguration.ScriptType, activityExecutionContext.ActivityConfiguration.Script);
                }

                if (null == activity)
                {
                    throw new WorkflowException(string.Format(CultureInfo.InvariantCulture,
                        "Cannot find activity [{0}] to run [state={1}] [workflowInstanceId={2:D}]",
                        activityExecutionContext.ActivityConfiguration.Code,
                        activityExecutionContext.StateExecutionContext.StateConfiguration.Code,
                        activityExecutionContext.StateExecutionContext.WorkflowContext.WorkflowInstance.Id));
                }

                var activityListener = GetActivityListener(activityExecutionContext.StateExecutionContext.WorkflowContext);
                if (null != activityListener)
                {
                    if (IsVerboseEnabled)
                    {
                        Log.Verbose("Executing activity {code} listener [before], {state} [{workflowInstanceId}]",
                            activityExecutionContext.ActivityConfiguration.Code,
                            activityExecutionContext.StateExecutionContext.StateConfiguration.Code,
                            activityExecutionContext.StateExecutionContext.WorkflowContext.WorkflowInstance.Id);
                    }

                    try
                    {
                        await activityListener.BeforeExecutingActivity(activity, activityExecutionContext, cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        throw new WorkflowException(string.Format(CultureInfo.InvariantCulture,
                            "An error has occurred during execution of an activity [{0}] listener [before] for workflow instance [{1:D}]",
                            activityExecutionContext.ActivityConfiguration.Code,
                            activityExecutionContext.StateExecutionContext.WorkflowContext.WorkflowInstance.Id), ex);
                    }
                }

                ActivityExecutionResult activityExecutionResult = null;
                for (var iteration = 0; ++iteration <= activityExecutionContext.ActivityConfiguration.RetryPolicy.Count;)
                {
                    tryCount = iteration;
                    if (IsVerboseEnabled)
                    {
                        Log.Verbose("Executing activity [{code}] {state} [{iteration}] [{workflowInstanceId}]",
                            activityExecutionContext.ActivityConfiguration.Code,
                            activityExecutionContext.StateExecutionContext.StateConfiguration.Code, iteration,
                            activityExecutionContext.StateExecutionContext.WorkflowContext.WorkflowInstance.Id);
                    }

                    try
                    {
                        activityExecutionResult = await activity.Execute(activityExecutionContext, cancellationToken).ConfigureAwait(false);
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (Log.IsEnabled(LogEventLevel.Warning))
                        {
                            Log.Warning(ex, "An error has occurred during activity [{code}] execution {state} {iteration} [{workflowInstanceId}]",
                                activityExecutionContext.ActivityConfiguration.Code,
                                activityExecutionContext.StateExecutionContext.StateConfiguration.Code, iteration,
                                activityExecutionContext.StateExecutionContext.WorkflowContext.WorkflowInstance.Id);
                        }

                        activityExecutionResult = iteration == activityExecutionContext.ActivityConfiguration.RetryPolicy.Count
                            ? new ActivityExecutionResult(ActivityExecutionStatus.FailedNoRetry)
                            : new ActivityExecutionResult(ActivityExecutionStatus.Failed);
                    }
                    finally
                    {
                        if (null == activityExecutionResult || activityExecutionResult.Status == ActivityExecutionStatus.Failed)
                        {
                            Thread.Sleep(TimeSpan.FromMilliseconds(activityExecutionContext.ActivityConfiguration.RetryPolicy.Delay));
                        }
                    }
                }

                if (null != activityExecutionResult && activityExecutionResult.Status.IsFailed())
                {
                    // if activity have dedicated transition to use on failure then workflow instance should follow to a configured state
                    activityExecutionResult.TransitionToUse = activityExecutionContext.ActivityConfiguration.RetryPolicy.OnFailureTransitionToUse;
                }

                if (null != activityListener)
                {
                    if (IsVerboseEnabled)
                    {
                        Log.Verbose("Executing activity [{code}] listener [after] {state} [{workflowInstanceId}]",
                            activityExecutionContext.ActivityConfiguration.Code,
                            activityExecutionContext.StateExecutionContext.StateConfiguration.Code,
                            activityExecutionContext.StateExecutionContext.WorkflowContext.WorkflowInstance.Id);
                    }

                    try
                    {
                        await activityListener.AfterExecutionActivity(activity, activityExecutionContext, activityExecutionResult, cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        throw new WorkflowException(string.Format(CultureInfo.InvariantCulture,
                            "An error has occurred during execution of an activity [{0}] listener [after] for workflow instance [{1:D}]",
                            activityExecutionContext.ActivityConfiguration.Code,
                            activityExecutionContext.StateExecutionContext.WorkflowContext.WorkflowInstance.Id), ex);
                    }
                }

                return activityExecutionResult;
            }
            finally
            {
                stopwatch.Stop();
                if (IsDebugEnabled)
                {
                    Log.Debug("Finished execution of activity {code} {state} {duration} [{workflowInstanceId}]",
                        activityExecutionContext.ActivityConfiguration.Code, activityExecutionContext.StateExecutionContext.StateConfiguration.Code,
                        stopwatch.Elapsed, activityExecutionContext.StateExecutionContext.WorkflowContext.WorkflowInstance.Id);
                }

                var workflowInstanceActivityLog = new WorkflowInstanceActivityLog(
                    activityExecutionContext.StateExecutionContext.WorkflowContext.WorkflowInstance.Id,
                    activityExecutionContext.ActivityConfiguration.Code, started, (int)stopwatch.ElapsedMilliseconds, tryCount);
                await _workflowEngine.WorkflowEngineBuilder.WorkflowStore.SaveWorkflowInstanceActivityLog(workflowInstanceActivityLog, cancellationToken).ConfigureAwait(false);
            }
        }

        private IActivityListener GetActivityListener(WorkflowContext workflowContext)
        {
            return _workflowEngine.WorkflowEngineBuilder.ListenerFactory?.CreateActivityListener(
                workflowContext.WorkflowConfiguration.Class, workflowContext.WorkflowConfiguration.Id);
        }
    }
}