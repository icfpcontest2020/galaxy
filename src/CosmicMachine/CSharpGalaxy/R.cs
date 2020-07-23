using System;
using System.Collections;
using System.Collections.Generic;

namespace CosmicMachine.CSharpGalaxy
{
    public class R : IEnumerable<long>
    {
        IEnumerator<long> IEnumerable<long>.GetEnumerator()
        {
            throw new NotSupportedException();
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotSupportedException();
        }
    }
}