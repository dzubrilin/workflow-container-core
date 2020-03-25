namespace Diadem.Workflow.Core.Execution.States
{
    public enum StateExecutionProgress
    {
        Undefined = 0,

        Started = 1,

        AwaitingAsyncTransition = 2,

        AwaitingDelayedTransition = 3,

        AwaitingEvent = 4,

        AfterEventProcessed = 5,

        AfterAsyncTransition = 6,

        AfterDelayedTransition = 7,

        BeforeActivities = 8,

        AfterActivitiesProcessed = 9,

        BeforeTransitions = 10,

        AfterTransitionsProcessed = 11,

        Completed = 12
    }
}