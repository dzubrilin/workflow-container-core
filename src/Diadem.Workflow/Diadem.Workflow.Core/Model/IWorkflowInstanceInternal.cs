using System.Collections.Generic;

namespace Diadem.Workflow.Core.Model
{
    public interface IWorkflowInstanceInternal : IWorkflowInstance
    {
        IList<string> VisitedStatesInternal { get; }
    }
}