using Diadem.Core.Commands;

namespace Diadem.Managers.Core
{
    public abstract class ManagerCommandResponse
    {
        protected ManagerCommandResponse()
        {
        }

        protected ManagerCommandResponse(CommandExecutionStatus commandExecutionStatus)
        {
            CommandExecutionStatus = commandExecutionStatus;
        }

        protected ManagerCommandResponse(CommandExecutionStatus commandExecutionStatus, CommandExecutionResult commandExecutionResult)
        {
            CommandExecutionStatus = commandExecutionStatus;
            CommandExecutionResult = commandExecutionResult;
        }

        internal void SetExecutionResult(CommandExecutionResult commandExecutionResult)
        {
            CommandExecutionResult = commandExecutionResult;
        }

        internal void SetExecutionStatus(CommandExecutionStatus commandExecutionStatus)
        {
            CommandExecutionStatus = commandExecutionStatus;
        }

        public CommandExecutionStatus CommandExecutionStatus { get; protected set; }

        public CommandExecutionResult CommandExecutionResult { get; protected set; }

        internal static TManagerCommandResponse Create<TManagerCommandResponse>(CommandExecutionStatus executionStatus, CommandExecutionResult executionResult)
            where TManagerCommandResponse : ManagerCommandResponse, new()
        {
            var managerCommandResponse = new TManagerCommandResponse();
            managerCommandResponse.SetExecutionStatus(executionStatus);
            managerCommandResponse.SetExecutionResult(executionResult);
            return managerCommandResponse;
        }
    }
}