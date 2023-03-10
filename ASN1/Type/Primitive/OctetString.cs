using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASN1.Type.Primitive
{
    public class OctetString : PrimitiveString
    {
        public OctetString(string str): base(str)
        {
            _typeTag = TYPE_OCTET_STRING;
        }
    }
}
