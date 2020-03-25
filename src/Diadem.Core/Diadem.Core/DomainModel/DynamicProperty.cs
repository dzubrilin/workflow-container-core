namespace Diadem.Core.DomainModel
{
    public abstract class DynamicProperty
    {
        protected DynamicProperty(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public PropertyType PropertyType { get; set; }

        public PropertySubType PropertySubType { get; set; }

        public object Value { get; set; }
    }

    public class DynamicProperty<T> : DynamicProperty
    {
        public DynamicProperty(string name) : base(name)
        {
        }

        public DynamicProperty(string name, T value) : base(name)
        {
            Value = value;
        }

        public new T Value { get; set; }
    }
}