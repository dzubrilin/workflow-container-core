namespace Diadem.Core.Commands
{
    public sealed class CommandExecutionSuccessResult : CommandExecutionResult
    {
        public CommandExecutionSuccessResult()
        {
        }

        public CommandExecutionSuccessResult(string description)
        {
            Description = description;
        }
    }
}