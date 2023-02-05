using ASN1.Component;
using ASN1.Feature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASN1.Type.Primitive
{
    public class Boolean: Element
    {
        private bool _bool;

        public Boolean(bool boolean)
        {
            typeTag = TYPE_BOOLEAN;
            _bool = boolean;
        }

        public bool Value() => _bool;

        protected string EncodedContentDER() => _bool ? "\xff" : "\x00";

        protected static ElementBase DecodeFromDER(Identifier identifier, string data, ref int? offset)
        {
            int? idx = offset;
            int? expected = 1;
            Length.ExpectFromDER(data, ref idx, ref expected);
            byte byteValue = (byte)data[(int)idx++];
            if (byteValue != 0)
            {
                if (byteValue != 0xff)
                {
                    throw new Exception("DER encoded boolean true must have all bits set to 1.");
                }
            }
            offset = idx;
            return new Boolean(byteValue != 0);
        }
    }
}
