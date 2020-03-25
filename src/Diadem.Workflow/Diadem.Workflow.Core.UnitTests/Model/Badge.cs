using Diadem.Core.DomainModel;

namespace Diadem.Workflow.Core.UnitTests.Model
{
    public class Badge : PocoEntity
    {
        public Badge(string entityId) : base(entityId)
        {
        }

        public bool IsInitialized { get; set; }

        public bool IsSigned { get; set; }
    }
}