using System;

namespace Diadem.Workflow.Core.Runtime
{
    public interface IListenerFactory
    {
        IActivityListener CreateActivityListener(string workflowClass, Guid workflowId);

        IStateListener CreateStateListener(string workflowClass, Guid workflowId);
    }
}