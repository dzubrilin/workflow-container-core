using System;

namespace Diadem.Workflow.Core
{
    public interface IWorkflowEntity
    {
        Guid EntityId { get; set; }

        string EntityType { get; set; }
    }
}