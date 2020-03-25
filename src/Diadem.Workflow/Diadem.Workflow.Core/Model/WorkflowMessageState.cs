using Diadem.Core.DomainModel;

namespace Diadem.Workflow.Core.Model
{
    public sealed class WorkflowMessageState : IWorkflowMessageState
    {
        private readonly JsonState _jsonState;

        private string _jsonStateString;

        public WorkflowMessageState()
        {
        }

        public WorkflowMessageState(JsonState jsonEntity)
        {
            _jsonState = jsonEntity;
        }

        public WorkflowMessageState(string jsonState)
        {
            JsonState = jsonState;
        }

        public string JsonState
        {
            get
            {
                if (null != _jsonState)
                {
                    _jsonStateString = _jsonState.ToJsonString();
                }

                return _jsonStateString;
            }

            set => _jsonStateString = value;
        }
    }
}