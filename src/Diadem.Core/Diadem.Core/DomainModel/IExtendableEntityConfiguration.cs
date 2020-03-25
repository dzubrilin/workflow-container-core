namespace Diadem.Core.DomainModel
{
    public interface IExtendableEntityConfiguration
    {
        void SetDynamicProperty<T>(DynamicProperty<T> property);

        void SetDynamicEntity(string name, DynamicExtendableEntity entity);
    }
}