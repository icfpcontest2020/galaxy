using System.Collections.Generic;

using Core;

namespace PlanetWars.Server.StateMachine.InternalLogging
{
    public class InternalLogger
    {
        private readonly List<string> log = new List<string>();

        public void Log(string s)
        {
            var logLine = $"[{Timestamp.Now.ToDateTime():HH:mm:ss.fff}] {s}";
            lock (log)
                log.Add(logLine);
        }

        public string[] GetLog()
        {
            lock (log)
                return log.ToArray();
        }
    }
}