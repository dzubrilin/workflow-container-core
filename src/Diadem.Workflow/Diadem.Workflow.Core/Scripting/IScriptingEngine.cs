using System.Reflection;
using Microsoft.CodeAnalysis.Scripting;

namespace Diadem.Workflow.Core.Scripting
{
    internal interface IScriptingEngine
    {
        Script GetScript<TGlobals>(string code, string cSharpScript, Assembly[] assemblies);
    }
}