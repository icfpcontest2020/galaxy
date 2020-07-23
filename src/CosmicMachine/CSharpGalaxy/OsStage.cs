using System;
using System.Collections;

namespace CosmicMachine.CSharpGalaxy
{
    public class OsStage
    {
        public OsStage(Func<OsState, IEnumerable, ComputerCommand<OsState>> update, IEnumerable initialStageState)
        {
            Update = update;
            InitialStageState = initialStageState;
        }

        public Func<OsState, IEnumerable, ComputerCommand<OsState>> Update;
        public IEnumerable InitialStageState;
    }
}