using System;
using Diadem.Workflow.Core.Execution.EventHandlers;

namespace Diadem.Workflow.Core.Runtime
{
    public class RemoteEventHandlerFactory : IEventHandlerFactory
    {
        public ICodeEventHandler CreateEventHandler(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return null;
            }

            if (string.Equals("noOp", code, StringComparison.OrdinalIgnoreCase))
            {
                return new NullEventHandler(code);
            }

            return code.StartsWith("remote.", StringComparison.OrdinalIgnoreCase)
                ? new RemoteEventHandler(code)
                : null;
        }
    }
}