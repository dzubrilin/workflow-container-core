using System;
using Diadem.Core.DomainModel;
using NUnit.Framework;

namespace Diadem.Workflow.Provider.Http.UnitTests
{
    public class WorkflowMessageHttpUriBuilderTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Parse_NonParametrized_Success()
        {
            var addressUri = new Uri("https://localhost/api/endpoint");
            var uriBuilder = new WorkflowMessageHttpUriBuilder(addressUri);
            var endpointAddressUri = uriBuilder.Build("{}");
            Assert.AreEqual(addressUri.ToString(), endpointAddressUri.ToString());
        }

        [Test]
        public void Parse_SimplePathParametrization_Success()
        {
            var addressUri = new Uri("https://localhost/api/endpoint/{EntityType}/{EntityId}");
            var uriBuilder = new WorkflowMessageHttpUriBuilder(addressUri);
            var endpointAddressUri = uriBuilder.Build("{ 'EntityType': 'Package', 'EntityId': 1 }");
            Assert.AreEqual("https://localhost/api/endpoint/Package/1", endpointAddressUri.ToString());
        }
        
        [Test]
        public void Parse_SimplePathParametrization_FailureNoParameter()
        {
            var addressUri = new Uri("https://localhost/api/endpoint/{EntityType}/{EntityId}");
            var uriBuilder = new WorkflowMessageHttpUriBuilder(addressUri);
            Assert.Throws<Exception>(() => uriBuilder.Build("{ 'EntityType': 'Package' }"));
        }
        
        [Test]
        public void Parse_SimpleQueryParametrization_Success()
        {
            var addressUri = new Uri("https://localhost/api/endpoint/get?EntityType={EntityType}&id={EntityId}");
            var uriBuilder = new WorkflowMessageHttpUriBuilder(addressUri);
            var endpointAddressUri = uriBuilder.Build("{ 'EntityType': 'Package', 'EntityId': 1 }");
            Assert.AreEqual("https://localhost/api/endpoint/get?EntityType=Package&id=1", endpointAddressUri.ToString());
        }
        
        [Test]
        public void Parse_SimplePathAndQueryParametrization_Success()
        {
            var addressUri = new Uri("https://localhost:5000/{LocalPath}?id={EntityId}");
            var uriBuilder = new WorkflowMessageHttpUriBuilder(addressUri);
            var endpointAddressUri = uriBuilder.Build("{ 'EntityType': 'Package', 'EntityId': 1, 'LocalPath': 'api/package/get' }");
            Assert.AreEqual("https://localhost:5000/api/package/get?id=1", endpointAddressUri.ToString());
        }
    }
}