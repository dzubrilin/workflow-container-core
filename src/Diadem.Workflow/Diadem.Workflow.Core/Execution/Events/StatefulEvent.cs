using System;
using Diadem.Core.DomainModel;

namespace Diadem.Workflow.Core.Execution.Events
{
    public class StatefulEvent : IStatefulEvent
    {
        public StatefulEvent(string code, Guid workflowInstanceId, JsonState state)
        {
            Code = code;
            State = state;
            WorkflowInstanceId = workflowInstanceId;
        }

        public string Code { get; }

        public JsonState State { get; }

        public Guid WorkflowInstanceId { get; }
    }
}