using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyGenerator
{
    internal class Pair<T, U>
    {
        public T First
        {
            get;
            private set;
        }

        public U Second
        {
            get;
            private set;
        }

        public Pair(Pair<T, U> p)
        {
            if (p == null)
            {
                throw new ArgumentNullException("p");
            }

            First = p.First;
            Second = p.Second;
        }

        public Pair(T f, U s)
        {
            First = f;
            Second = s;
        }

    }
}
