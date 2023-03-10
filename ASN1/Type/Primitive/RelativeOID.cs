using ASN1.Component;
using ASN1.Feature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASN1.Type.Primitive
{
    public class RelativeOID : ObjectIdentifier
    {
        public RelativeOID(string oid) : base(oid)
        {
            _oid = oid;
            _subids = ExplodeDottedOID(oid);
            _typeTag = TYPE_RELATIVE_OID;
        }

        override protected string EncodedContentDER()
        {
            return EncodeSubIDs(_subids.ToArray());
        }

        protected static IElementBase DecodeFromDER(Identifier identifier, string data, ref int? offset)
        {
            int? idx = offset;
            int? expected = null;
            var len = Length.ExpectFromDER(data, ref idx, ref expected).IntLength();
            var subids = DecodeSubIDs(data.Substring((int)idx, len));
            offset = (int)idx + len;
            return new RelativeOID(ImplodeSubIDs(subids.ToArray()));
        }
    }
}
