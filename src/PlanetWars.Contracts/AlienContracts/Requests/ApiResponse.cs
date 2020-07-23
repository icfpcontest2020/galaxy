using System.ComponentModel.DataAnnotations;

namespace PlanetWars.Contracts.AlienContracts.Requests
{
    public class ApiResponse
    {
        [Required]
        public bool Success { get; set; }
    }
}