using System;
using Diadem.Messaging.Core;

namespace Diadem.Managers.Core
{
    public class ManagerCommandTransportFactory : IManagerCommandTransportFactory
    {
        private readonly IBusControlFactory _busControlFactory;

        public ManagerCommandTransportFactory(IBusControlFactory busControlFactory)
        {
            _busControlFactory = busControlFactory;
        }

        public IManagerCommandTransport CreateCommandTransport()
        {
            var busControl = _busControlFactory.CreateBusControl();
            return new ManagerCommandTransport(busControl);
        }
    }
}