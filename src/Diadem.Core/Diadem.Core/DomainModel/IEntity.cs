using System.Collections.Generic;

namespace Diadem.Core.DomainModel
{
    public interface IEntity
    {
        string EntityId { get; set; }

        string EntityType { get; }

        IList<IEntity> GetCollection(string name);

        void SetCollection(string name, IList<IEntity> collection);

        IEntity GetEntity(string name);

        T GetProperty<T>(string name);

        void SetProperty<T>(string name, T value);
    }
}