using MassTransit;

namespace Diadem.Messaging.Core
{
    public interface IBusControlFactory
    {
        IBusControl CreateBusControl();
    }
}