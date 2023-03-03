using ASN1.Component;
using ASN1.Feature;
using Org.BouncyCastle.Asn1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASN1.Type.Primitive
{
    public class NullType : Element
    {
        public NullType()
        {
            typeTag = TYPE_NULL;
        }

        override protected string EncodedContentDER() => string.Empty;

        protected static IElementBase DecodeFromDER(Identifier identifier, string data, ref int offset)
        {
            int? idx = offset;
            int? expected = 0;
            if (!identifier.IsPrimitive())
            {
                throw new Exception("Null value must be primitive.");
            }
            Length.ExpectFromDER(data, ref idx, ref expected);
            offset = (int)idx;
            return new NullType();
        }
    }
}
