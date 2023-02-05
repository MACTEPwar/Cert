using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASN1
{
    public static class Helpers
    {
        public static System.Numerics.BigInteger ToBigInteger(this int self)
        {
            object obj1 = self;
            return (System.Numerics.BigInteger)obj1;
        }
    }
}
