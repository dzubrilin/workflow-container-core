using System;

namespace Diadem.Workflow.Core.Model
{
    public interface IWorkflowInstanceLock
    {
        DateTime LockedAt { get; }

        DateTime LockedUntil { get; }

        Guid LockOwner { get; }

        WorkflowInstanceLockMode LockMode { get; }
    }
}