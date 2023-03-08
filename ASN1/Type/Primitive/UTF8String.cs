using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASN1.Type.Primitive
{
    public class UTF8String : PrimitiveString
    {
        public UTF8String(string str): base(str) { 
            typeTag = TYPE_UTF8_STRING;
        }
        
        protected override bool ValidateString(string str)
        {
            return System.Text.Encoding.UTF8.GetByteCount(str) == str.Length;
        }
    }
}
