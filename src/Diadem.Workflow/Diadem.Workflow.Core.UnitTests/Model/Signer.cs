using System;
using System.Collections.Generic;
using Diadem.Core.DomainModel;

namespace Diadem.Workflow.Core.UnitTests.Model
{
    public class Signer : PocoEntity
    {
        public Signer(string entityId) : base(entityId)
        {
            Badges = new List<Badge>();
            SendWelcomeCommunication = true;
        }

        public Signer(string entityId, List<Badge> badges) : base(entityId)
        {
            Badges = badges;
            SendWelcomeCommunication = true;
        }

        public IList<Badge> Badges { get; }

        public DateTime LastVisited { get; set; }

        public bool IsInitialized { get; set; }

        public bool IsSent { get; set; }

        public bool IsSigned { get; set; }

        public bool SendWelcomeCommunication { get; set; }

        public bool VisitedIntermediateState { get; set; }
    }
}