using System.Collections.Generic;
using Diadem.Core.DomainModel;

namespace Diadem.Workflow.Core.UnitTests.Model
{
    public class Package : PocoEntity
    {
        public Package(string entityId) : base(entityId)
        {
            Signers = new List<Signer>();
        }

        public Package(string entityId, IList<Signer> signers) : base(entityId)
        {
            Signers = signers;
        }

        public IList<Signer> Signers { get; }

        public bool IsCompleted { get; set; }

        public bool IsInitialized { get; set; }

        public bool IsSent { get; set; }
    }
}