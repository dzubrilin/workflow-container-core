using Autofac;

namespace Diadem.TimeZone
{
    public class TimeZoneAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<NodaTimeZoneProvider>()
                   .As<ITimeZoneProvider>()
                   .SingleInstance();
        }
    }
}