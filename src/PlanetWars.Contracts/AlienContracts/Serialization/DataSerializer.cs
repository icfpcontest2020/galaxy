using System;

using PlanetWars.Contracts.AlienContracts.Serialization.Emit;

namespace PlanetWars.Contracts.AlienContracts.Serialization
{
    public static class DataSerializer
    {
        public static T Deserialize<T>(Data data)
        {
            var result = DataDeserializerEmitter.Deserialize(typeof(T), data);
            if (result == null)
                throw new InvalidOperationException($"Deserialize<{typeof(T).Name}>() returned null for non-null data: {data}");

            return (T)result;
        }

        public static Data? Serialize(object? value)
        {
            return DataSerializerEmitter.Serialize(value);
        }
    }
}