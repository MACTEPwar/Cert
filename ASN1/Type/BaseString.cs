using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASN1.Type
{
    abstract class BaseString : Element, IStringType
    {
        protected string Str { get; set; }

        public BaseString(string str)
        {
            if (!ValidateString(str))
            {
                throw new Exception($"Not a valid %s string. {TagToName(typeTag)}");
            }
            Str = str;
        }
        public override string ToString() => String();
        public string String() => this.Str;

        protected bool ValidateString(string str) => true;
    }
}
