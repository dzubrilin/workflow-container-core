using System;
using System.Collections.Generic;
using System.Reflection;

namespace Diadem.Managers.Core.Messaging
{
    public interface IAsynchronousCommandRegistry
    {
        void RegisterAssembly(Assembly assembly);

        IEnumerable<KeyValuePair<Type, Type>> GetAsynchronousCommands();
    }
}