using System;

namespace Diadem.Core.Configuration
{
    [Flags]
    public enum ConfigurationSource
    {
        Undefined = 0,

        Environment = 1 << 0,

        ConfigurationFile = 1 << 1,

        Any = Environment | ConfigurationFile
    }
}