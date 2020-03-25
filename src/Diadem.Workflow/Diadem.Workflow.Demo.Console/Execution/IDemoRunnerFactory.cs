namespace Diadem.Workflow.Demo.Console.Execution
{
    public interface IDemoRunnerFactory
    {
        string Name { get; }

        IDemoRunner CreateDemoRunner();
    }
}