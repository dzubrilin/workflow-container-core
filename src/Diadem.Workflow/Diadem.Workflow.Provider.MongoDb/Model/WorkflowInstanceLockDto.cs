using System;
using Diadem.Workflow.Core.Model;
using MongoDB.Bson;

namespace Diadem.Workflow.Provider.MongoDb.Model
{
    public class WorkflowInstanceLockDto
    {
        public WorkflowInstanceLockDto()
        {
        }

        public WorkflowInstanceLockDto(string lockOwner, WorkflowInstanceLockMode lockMode, BsonDateTime lockedAt, BsonDateTime lockedUntil)
        {
            LockOwner = lockOwner;
            LockMode = lockMode;
            LockedAt = lockedAt;
            LockedUntil = lockedUntil;
        }

        public BsonDateTime LockedAt { get; set; }

        public BsonDateTime LockedUntil { get; set; }

        public string LockOwner { get; set; }

        public WorkflowInstanceLockMode LockMode { get; set; }
    }
}