namespace CosmicMachine.Lang
{
    public class Var : Exp
    {
        public readonly string Name;

        public Var(string name)
        {
            Name = name;
        }

        public override bool HasFreeVar(string name) => name == Name;
        public override string ToString() => "var:" + Name;
    }
}