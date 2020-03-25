using System;

namespace Diadem.Core.Configuration
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class JsonSerializableConfigurationValueAttribute : Attribute
    {
    }
}