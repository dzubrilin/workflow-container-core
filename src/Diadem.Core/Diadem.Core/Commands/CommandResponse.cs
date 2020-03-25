namespace Diadem.Core.Commands
{
    public abstract class CommandResponse
    {
        protected CommandResponse()
        {
        }

        protected CommandResponse(CommandExecutionStatus commandExecutionStatus)
        {
            CommandExecutionStatus = commandExecutionStatus;
        }

        protected CommandResponse(CommandExecutionStatus commandExecutionStatus, CommandExecutionResult commandExecutionResult)
        {
            CommandExecutionStatus = commandExecutionStatus;
            CommandExecutionResult = commandExecutionResult;
        }

        public CommandExecutionStatus CommandExecutionStatus { get; protected set; }

        public CommandExecutionResult CommandExecutionResult { get; protected set; }
    }
}