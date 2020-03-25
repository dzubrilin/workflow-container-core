using System;

namespace Diadem.Workflow.Core.Cache
{
    public interface IWorkflowCache : IDisposable
    {
        bool TryGetValue<TValue>(string key, out TValue value);

        void Put(string key, object value);

        void Remove(string key);
    }
}