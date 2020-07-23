using System;
using System.Linq;
using System.Net.NetworkInformation;

namespace Core
{
    public static class SocketHelper
    {
        public static int GetFreeTcpPortNumberInRange(int minPort, int maxPort)
        {
            for (var port = minPort; port <= maxPort; port++)
            {
                if (IsPortFree(port))
                    return port;
            }
            throw new InvalidOperationException($"No free port is available in range [{minPort}, {maxPort}]");
        }

        private static bool IsPortFree(int port)
        {
            var activeTcpListeners = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners();
            return activeTcpListeners.All(endpoint => endpoint.Port != port);
        }
    }
}