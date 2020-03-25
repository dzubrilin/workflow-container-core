namespace Diadem.Core.Commands
{
    public sealed class CommandExecutionInternalErrorResult : CommandExecutionResult
    {
        public CommandExecutionInternalErrorResult()
        {
        }

        public CommandExecutionInternalErrorResult(string description)
        {
            Description = description;
        }
    }
}