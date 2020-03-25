namespace Diadem.Core.DomainModel
{
    public enum PropertySubType
    {
        [PropertyType(PropertyType.Undefined)] Undefined = 0,

        [PropertyType(PropertyType.String)] PhoneNumber = 1,

        [PropertyType(PropertyType.String)] Ssn = 2,

        [PropertyType(PropertyType.Decimal)] Money = 3
    }
}