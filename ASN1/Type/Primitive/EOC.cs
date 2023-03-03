using ASN1.Component;
using ASN1.Feature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASN1.Type.Primitive
{
    public class EOC: Element
    {
        public EOC()
        {
            typeTag = TYPE_EOC;
        }

        override protected string EncodedContentDER()
        {
            return string.Empty;
        }

        protected static IElementBase DecodeFromDER(Identifier identifier, string data, ref int? offset)
        {
            int? idx = offset;
            if (!identifier.IsPrimitive())
            {
                throw new Exception("EOC value must be primitive.");
            }
            int? expected = 0;
            // EOC type has always zero length
            Length.ExpectFromDER(data, ref idx, ref expected);
            offset = idx;
            return new EOC();
        }
    }
}
