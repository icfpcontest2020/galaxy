namespace PlanetWars.Contracts.AlienContracts.Serialization
{
    public static class AlienSerializer
    {
        public static string Serialize(object? alienModel)
        {
            var data = DataSerializer.Serialize(alienModel);
            var alienString = data.AlienEncode();
            return alienString;
        }

        /// <summary>
        ///     Throws FormatException on invalid input
        /// </summary>
        public static T? Deserialize<T>(string alienString)
            where T : class
        {
            var data = alienString.AlienDecode();
            if (data == null)
                return null;

            var alienModel = DataSerializer.Deserialize<T>(data);
            return alienModel;
        }
    }
}