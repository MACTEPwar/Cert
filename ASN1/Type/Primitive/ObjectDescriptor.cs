using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASN1.Type.Primitive
{
    public class ObjectDescriptor : PrimitiveString
    {
        public ObjectDescriptor(string descriptor): base(descriptor)
        {
            Str = descriptor;
            typeTag= TYPE_OBJECT_DESCRIPTOR;
        }

        public string Descriptor() => Str;
    }
}
