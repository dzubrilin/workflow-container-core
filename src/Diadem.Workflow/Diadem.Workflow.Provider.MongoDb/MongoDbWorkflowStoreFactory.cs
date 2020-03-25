using System.Security;
using Diadem.Core.Configuration;
using Diadem.Workflow.Core.Runtime;
using MongoDB.Driver;

namespace Diadem.Workflow.Provider.MongoDb
{
    public class MongoDbWorkflowStoreFactory : IWorkflowStoreFactory
    {
        private readonly IConfigurationProvider _configurationProvider;

        public MongoDbWorkflowStoreFactory(IConfigurationProvider configurationProvider)
        {
            _configurationProvider = configurationProvider;
        }

        public IWorkflowStore Create()
        {
            var mongoDbConfigurationSection = _configurationProvider.GetSection<IWorkflowMongoDbConfigurationSection>();

            var credentialPassword = new SecureString();
            foreach (var @char in mongoDbConfigurationSection.CredentialPassword)
            {
                credentialPassword.AppendChar(@char);
            }

            var mongoClient = new MongoClient(new MongoClientSettings
            {
                Server = new MongoServerAddress(mongoDbConfigurationSection.ServerHost, mongoDbConfigurationSection.ServerPort),
                Credential = MongoCredential.CreateCredential(
                    mongoDbConfigurationSection.CredentialDatabaseName,
                    mongoDbConfigurationSection.CredentialUserName,
                    credentialPassword),
                ConnectionMode = ConnectionMode.Automatic
            });

            var mongoDatabase = mongoClient.GetDatabase(mongoDbConfigurationSection.DatabaseName);
            var mongoStore = new MongoDbWorkflowStore(mongoDatabase);
            return mongoStore;
        }
    }
}