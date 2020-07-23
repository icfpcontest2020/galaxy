using System;
#nullable disable

namespace CosmicMachine.CSharpGalaxy
{
    public class BitEncodedImage : FakeEnumerable<long>
    {
        public static implicit operator BitEncodedImage(string symbolName) => throw new NotSupportedException();
    }
}