using System.Collections;

namespace CosmicMachine.CSharpGalaxy
{
    public class ClickArea : FakeEnumerable
    {
        public ClickArea(R rect, long eventId, long argument)
        {
            Rect = rect;
            EventId = eventId;
            Argument = argument;
        }

        public R Rect;
        public long EventId;
        public long Argument;
    }
}