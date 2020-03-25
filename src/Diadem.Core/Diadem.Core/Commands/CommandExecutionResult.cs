namespace Diadem.Core.Commands
{
    public class CommandExecutionResult
    {
        public CommandExecutionResult()
        {
        }

        public CommandExecutionResult(string description)
        {
            Description = description;
        }

        public string Description { get; protected set; }
    }
}