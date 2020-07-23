using static CosmicMachine.Lang.CoreImplementations;

namespace CosmicMachine.Lang
{
    public class EmptyList : Exp
    {
        public override Exp Apply(Exp x)
        {
            return True;
        }

        public override string ToString()
        {
            return "emptyList";
        }
    }
}