using ASN1.Component;
using ASN1.Feature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASN1.Type
{
    public abstract class PrimitiveString : BaseString
    {
        public PrimitiveString(string str) : base(str)
        {

        }

        override protected string EncodedContentDER() => Str;

        protected static IElementBase DecodeFromDER<T>(Identifier identifier, string data, ref int offset) where T : BaseString
        {
            int? idx = offset;
            int? expected = null;
            if (identifier.IsPrimitive())
            {
                throw new Exception("DER encoded string must be primitive.");
            }
            var length = Length.ExpectFromDER(data, ref idx, ref expected).IntLength();
            var str = length > 0 ? data.Substring((int)idx, length) : string.Empty;
            offset = (int)idx + length;
            try
            {
                return (T)Activator.CreateInstance(typeof(T), str);
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
