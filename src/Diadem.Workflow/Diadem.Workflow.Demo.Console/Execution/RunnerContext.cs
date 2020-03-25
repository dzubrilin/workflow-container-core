using Autofac;
using Diadem.Core.DomainModel;

namespace Diadem.Workflow.Demo.Console.Execution
{
    public class RunnerContext
    {
        public RunnerContext(ILifetimeScope lifetimeScope, IDemoRunner runner)
        {
            LifetimeScope = lifetimeScope;
            Runner = runner;
            State = new JsonState();
        }

        public ILifetimeScope LifetimeScope { get; }

        public IDemoRunner Runner { get; }

        public JsonState State { get; }
    }
}