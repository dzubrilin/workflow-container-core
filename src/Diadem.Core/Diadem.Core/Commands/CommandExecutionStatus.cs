namespace Diadem.Core.Commands
{
    public enum CommandExecutionStatus
    {
        Undefined = 0,

        Completed = 1,

        ValidationFailed = 2,

        ExecutionFailed = 3,

        InternalServerError = 4
    }
}