using System;
using System.Collections;
using System.Collections.Generic;

namespace CosmicMachine.CSharpGalaxy
{
    public class FakeEnumerable : IEnumerable
    {
        IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();
    }

    public class FakeEnumerable<T> : FakeEnumerable, IEnumerable<T>
    {
        public IEnumerator<T> GetEnumerator() => throw new NotSupportedException();
    }
}