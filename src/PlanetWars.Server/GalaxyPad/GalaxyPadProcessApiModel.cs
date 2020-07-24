using System.Collections.Generic;
using System.Linq;

using CosmicMachine;

namespace PlanetWars.Server.GalaxyPad
{
    public class GalaxyPadProcessApiModel
    {
        public GalaxyPadProcessApiModel(string pid, CommandType lastCommand, IReadOnlyList<Vec[]> screens, string memory, int timeTakenMs)
        {
            Screens = screens.Select(screen => screen.Select(p => new[] { p.X, p.Y }).ToArray()).ToArray();
            Pid = pid;
            LastCommand = lastCommand;
            Memory = memory;
            TimeTakenMs = timeTakenMs;
        }

        public int[][][] Screens { get; }
        public string Pid { get; }
        public int TimeTakenMs { get; }
        public string Memory { get; }
        public CommandType LastCommand { get; }
    }
}