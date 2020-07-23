using System;

namespace PlanetWars.Contracts.AlienContracts.Serialization
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DataTypeAttribute : Attribute
    {
        public DataTypeAttribute(object dataType)
        {
            if (!dataType.GetType().IsEnum || dataType.GetType().GetEnumUnderlyingType() != typeof(int))
                throw new InvalidOperationException($"{nameof(DataTypeAttribute)} accepts only enums based on {typeof(int)}");
            DataType = (int)dataType;
        }

        public long DataType { get; }
    }
}