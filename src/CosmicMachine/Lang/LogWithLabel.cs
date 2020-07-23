using System.Diagnostics;

namespace CosmicMachine.Lang
{
    public class LogWithLabel : Exp
    {
        private readonly string label;

        public LogWithLabel(string label)
        {
            this.label = label;
        }

        public override Exp Apply(Exp x)
        {
            var evaluatedX = x.Eval();
            var message = GetMessage(evaluatedX);
            Trace.WriteLine($"{label}: {message}");
            return x;
        }

        private string GetMessage(Exp evaluatedX)
        {
            try
            {
                return evaluatedX.GetD()?.ToString() ?? "null";
            }
            catch
            {
                return evaluatedX.ToString()!;
            }
        }

        public override string ToString() => $"log_{label.Replace(' ', '_')}";
    }
}