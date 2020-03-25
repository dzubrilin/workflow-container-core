using System.Collections.Generic;

namespace Diadem.Core.Commands
{
    public sealed class CommandExecutionValidationFailedResult : CommandExecutionResult
    {
        public CommandExecutionValidationFailedResult()
        {
        }

        public CommandExecutionValidationFailedResult(string description, IList<string> validationMessages)
        {
            Description = description;
            ValidationMessages = validationMessages;
        }

        public CommandExecutionValidationFailedResult(IList<string> validationMessages)
        {
            ValidationMessages = validationMessages;
        }

        public IList<string> ValidationMessages { get; }
    }
}