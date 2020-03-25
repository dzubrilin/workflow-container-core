namespace Diadem.Core.DomainModel
{
    public interface IExtendableEntity : IEntity
    {
        void SetEntity(string name, IEntity entity);
    }
}