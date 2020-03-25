namespace Diadem.Workflow.Core.Cache
{
    public class NullWorkflowWorkflowCache : IWorkflowCache
    {
        public bool TryGetValue<TValue>(string key, out TValue value)
        {
            value = default;
            return false;
        }

        public void Put(string key, object value)
        {
        }

        public void Remove(string key)
        {
        }

        public void Dispose()
        {
        }
    }
}