using System.Collections;
using System.Collections.Generic;

namespace CosmicMachine.CSharpGalaxy
{
    public class OsState : FakeEnumerable
    {
        public OsState(long stageId, IEnumerable stageState, long openedBattlesCount, IEnumerable<long> secretKeys)
        {
            StageId = stageId;
            StageState = stageState;
            OpenedBattlesCount = openedBattlesCount;
            SecretKeys = secretKeys;
        }

        public long StageId;
        public IEnumerable StageState;
        public long OpenedBattlesCount;
        public IEnumerable<long> SecretKeys;
    }
}