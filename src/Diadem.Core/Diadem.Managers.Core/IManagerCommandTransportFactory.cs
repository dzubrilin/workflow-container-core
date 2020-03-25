using System;

namespace Diadem.Managers.Core
{
    public interface IManagerCommandTransportFactory
    {
        IManagerCommandTransport CreateCommandTransport();
    }
}