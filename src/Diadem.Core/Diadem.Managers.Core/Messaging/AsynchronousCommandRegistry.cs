using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Diadem.Core.Commands;

namespace Diadem.Managers.Core.Messaging
{
    public class AsynchronousCommandRegistry : IAsynchronousCommandRegistry
    {
        private readonly ConcurrentBag<Assembly> _assemblies;

        public AsynchronousCommandRegistry()
        {
            _assemblies = new ConcurrentBag<Assembly>();
        }

        public void RegisterAssembly(Assembly assembly)
        {
            if (!_assemblies.Contains(assembly))
            {
                _assemblies.Add(assembly);
            }
        }

        public IEnumerable<KeyValuePair<Type, Type>> GetAsynchronousCommands()
        {
            return _assemblies.ToArray().SelectMany(GetAsynchronousCommands);
        }

        private IEnumerable<KeyValuePair<Type, Type>> GetAsynchronousCommands(Assembly assembly)
        {
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                var asynchronousCommandAttribute = type.GetCustomAttribute<AllowAsynchronousCommandExecutionAttribute>();
                if (null != asynchronousCommandAttribute)
                {
                    var interfaces = type.GetInterfaces();
                    var @interface = interfaces.FirstOrDefault(i => i.IsGenericType && i.Name.Contains(typeof(IManagerCommandHandler).Name));
                    if (null != @interface)
                    {
                        var genericArguments = @interface.GetGenericArguments();
                        yield return new KeyValuePair<Type, Type>(genericArguments[0], genericArguments[1]);
                    }
                }
            }
        }
    }
}