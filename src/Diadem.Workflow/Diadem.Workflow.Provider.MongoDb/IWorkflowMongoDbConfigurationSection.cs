using Diadem.Core.Configuration;

namespace Diadem.Workflow.Provider.MongoDb
{
    [ConfigurationSection("WORKFLOWMONGODB")]
    public interface IWorkflowMongoDbConfigurationSection : IConfigurationSection
    {
        string ServerHost { get; }

        int ServerPort { get; }

        string DatabaseName { get; }

        string CredentialDatabaseName { get; }

        string CredentialUserName { get; }

        string CredentialPassword { get; }
    }
}