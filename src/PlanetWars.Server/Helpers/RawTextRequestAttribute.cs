using System;
using System.Net.Mime;

namespace PlanetWars.Server.Helpers
{
    public class RawTextRequestAttribute : Attribute
    {
        public RawTextRequestAttribute()
        {
            MediaType = MediaTypeNames.Text.Plain;
        }

        public string MediaType { get; set; }
    }
}