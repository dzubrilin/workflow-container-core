using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Diadem.Core.DomainModel;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace Diadem.Workflow.Core.Scripting
{
    internal class InProcessScriptingEngine : IScriptingEngine
    {
        private static readonly Lazy<ConcurrentDictionary<string, Script>> ScriptMap =
            new Lazy<ConcurrentDictionary<string, Script>>(() => new ConcurrentDictionary<string, Script>());

        private readonly Assembly[] _defaultAssemblies =
        {
            typeof(object).GetTypeInfo().Assembly,
            typeof(Binder).GetTypeInfo().Assembly,
            typeof(IList).GetTypeInfo().Assembly,
            typeof(IList<>).GetTypeInfo().Assembly,
            typeof(Queryable).GetTypeInfo().Assembly,
            typeof(Enumerable).GetTypeInfo().Assembly,
            typeof(IEntity).GetTypeInfo().Assembly,
            typeof(IWorkflowEngine).GetTypeInfo().Assembly
        };

        private readonly string[] _defaultImports =
        {
            "System",
            "System.Collections",
            "System.Collections.Generic",
            "System.Linq",
            "System.Threading",
            "System.Threading.Tasks",
            "Diadem.Core",
            "Diadem.Core.DomainModel"
        };

        public Script GetScript<TGlobals>(string code, string cSharpScript, Assembly[] assemblies)
        {
            return ScriptMap.Value.GetOrAdd(code, lambdaCode =>
            {
                var referencedAssemblies = null == assemblies ? _defaultAssemblies : _defaultAssemblies.Union(assemblies);
                var scriptOptions = ScriptOptions.Default.WithReferences(referencedAssemblies).WithImports(_defaultImports);
                var script = CSharpScript.Create(cSharpScript, scriptOptions, typeof(TGlobals));
                script.Compile();
                return script;
            });
        }
    }
}