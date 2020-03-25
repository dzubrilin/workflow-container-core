namespace Diadem.Workflow.Core.Model
{
    public enum EntityRequestExecutionStatus
    {
        Undefined = 0,
        
        Completed = 1,
        
        NotFound = 2,
        
        ValidationFailed = 3,

        InternalServerError = 4
    }
}