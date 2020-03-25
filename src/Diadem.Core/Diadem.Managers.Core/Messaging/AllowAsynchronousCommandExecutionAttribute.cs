using System;

namespace Diadem.Managers.Core.Messaging
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class AllowAsynchronousCommandExecutionAttribute : Attribute
    {
    }
}