using System;
using System.Collections.Generic;

namespace PlanetWars.Contracts.AlienContracts.Serialization
{
    public class PairData : Data
    {
        public readonly Data? Value;
        public readonly Data? Next;

        public PairData(Data? value, Data? next)
        {
            Value = value;
            Next = next;
        }

        public IEnumerable<Data?> AsList()
        {
            Data? node = this;
            while (node is PairData pair)
            {
                yield return pair.Value;
                node = pair.Next;
            }
            if (node != null)
                throw new Exception("Not a list? Tuple?");
        }

        public IEnumerable<Data?> AsTuple()
        {
            Data? node = this;
            while (node is PairData pair)
            {
                yield return pair.Value;
                node = pair.Next;
            }
            yield return node;
        }
    }
}