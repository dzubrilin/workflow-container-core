namespace Diadem.Workflow.Core.Configuration
{
    public enum EventTypeConfiguration
    {
        Undefined = 0,

        Initial = 1,

        /// <summary>
        /// Application specific events
        /// </summary>
        Application = 2,

        /// <summary>
        /// Application wide events (e.g. void/cancel/etc.),
        /// forces the workflow instance to move into the state with no awaiting event check
        /// </summary>
        ApplicationWide = 3,

        ParentToNestedInitial = 4,

        ParentToNestedApplication = 5,

        ParentToNestedFinalized = 6,

        ParentToNestedFailed = 7,

        NestedToParentApplication = 8,

        NestedToParentFinalized = 9,

        NestedToParentFailed = 10
    }
}