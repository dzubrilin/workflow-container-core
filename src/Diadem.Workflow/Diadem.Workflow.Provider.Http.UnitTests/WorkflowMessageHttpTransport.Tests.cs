using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Diadem.Core.Configuration;
using Diadem.Workflow.Core.Execution.Activities;
using Diadem.Workflow.Core.Model;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Diadem.Workflow.Provider.Http.UnitTests
{
    public class WorkflowMessageHttpTransportTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task HttpCall_SimpleActivityRequest_Success()
        {
            var addressUri = new Uri("https://localhost/api/endpoint");
            var endpointConfiguration = new EndpointConfiguration("Code_EC", EndpointConfigurationType.Http, addressUri,
                new ConfigurationAuthentication(ConfigurationAuthenticationType.Basic,
                    new List<KeyValuePair<string, string>>(new [] { new KeyValuePair<string, string>("UserName", "flow"), new KeyValuePair<string, string>("Password", "flow-password") })));
            var activityRequestWorkflowMessage = new ActivityRequestWorkflowMessage(Guid.NewGuid(), Guid.NewGuid(), "Code_AR", "{}");
            var activityResponseWorkflowMessage = new ActivityResponseWorkflowMessage
            {
                WorkflowMessageId = Guid.NewGuid(),
                WorkflowId = activityRequestWorkflowMessage.WorkflowId,
                WorkflowInstanceId = activityRequestWorkflowMessage.WorkflowInstanceId,
                ActivityExecutionResult = new ActivityExecutionResult(ActivityExecutionStatus.Completed)
            };
            
            var httpMessageHandlerMock = new MockHttpMessageHandler(HttpStatusCode.OK, JsonConvert.SerializeObject(activityResponseWorkflowMessage));
            var httpClient = new HttpClient(httpMessageHandlerMock);
            var workflowMessageHttpTransport = new WorkflowMessageHttpTransport(httpClient);

            var response = await workflowMessageHttpTransport.Request<ActivityRequestWorkflowMessage, ActivityResponseWorkflowMessage>(
                endpointConfiguration, activityRequestWorkflowMessage, CancellationToken.None).ConfigureAwait(false);

            Assert.IsTrue(response.WorkflowId == activityResponseWorkflowMessage.WorkflowId);
            Assert.IsTrue(response.WorkflowInstanceId == activityResponseWorkflowMessage.WorkflowInstanceId);
            
            Assert.IsNotNull(httpClient.DefaultRequestHeaders.Authorization);
            Assert.IsTrue(string.Equals("Basic", httpClient.DefaultRequestHeaders.Authorization.Scheme, StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public void HttpCall_SimpleActivityRequest_Failure500()
        {
            var addressUri = new Uri("https://localhost/api/endpoint");
            var endpointConfiguration = new EndpointConfiguration("Code_EC", EndpointConfigurationType.Http, addressUri, ConfigurationAuthentication.None);
            var activityRequestWorkflowMessage = new ActivityRequestWorkflowMessage(Guid.NewGuid(), Guid.NewGuid(), "Code_AR", "{}");
            
            var httpMessageHandlerMock = new MockHttpMessageHandler(HttpStatusCode.InternalServerError, string.Empty);
            var httpClient = new HttpClient(httpMessageHandlerMock);
            var workflowMessageHttpTransport = new WorkflowMessageHttpTransport(httpClient);

            Assert.ThrowsAsync<ApplicationException>(() => workflowMessageHttpTransport.Request<ActivityRequestWorkflowMessage, ActivityResponseWorkflowMessage>(
                endpointConfiguration, activityRequestWorkflowMessage, CancellationToken.None));
        }
    }
}