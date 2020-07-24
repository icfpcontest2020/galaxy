using System;
using System.Net.Http;

using CosmicMachine;

using PlanetWars.Contracts.AlienContracts.Serialization;

namespace PlanetWars.Server.GalaxyPad
{
    public class GalaxyPadAlienServerClient
    {
        private readonly HttpClient httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:12345") };

        public Exp Post(Exp request)
        {
            var content = request.GetD().AlienEncode();
            var response = httpClient.PostAsync("/aliens/send", new StringContent(content)).Result;
            response.EnsureSuccessStatusCode();
            var result = response.Content.ReadAsStringAsync().Result;
            return result.AlienDecode().ToExp();
        }
    }
}
