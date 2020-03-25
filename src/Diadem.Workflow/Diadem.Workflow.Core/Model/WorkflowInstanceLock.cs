using System;

namespace Diadem.Workflow.Core.Model
{
    public class WorkflowInstanceLock : IWorkflowInstanceLock
    {
        public WorkflowInstanceLock(Guid lockOwner, WorkflowInstanceLockMode lockMode, DateTime lockedAt, DateTime lockedUntil)
        {
            LockOwner = lockOwner;
            LockMode = lockMode;
            LockedAt = lockedAt;
            LockedUntil = lockedUntil;
        }

        public DateTime LockedAt { get; }

        public DateTime LockedUntil { get; internal set; }

        public Guid LockOwner { get; }

        public WorkflowInstanceLockMode LockMode { get; }
    }
}