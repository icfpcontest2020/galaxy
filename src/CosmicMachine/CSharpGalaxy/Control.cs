using System.Collections.Generic;
#nullable disable

namespace CosmicMachine.CSharpGalaxy
{
    public class Control
    {
        public Control(IEnumerable<ClickArea> areas, IEnumerable<IEnumerable<V>> screen)
        {
            Areas = areas;
            Screen = screen;
        }

        public IEnumerable<ClickArea> Areas;
        public IEnumerable<IEnumerable<V>> Screen;
    }
}