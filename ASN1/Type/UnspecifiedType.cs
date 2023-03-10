using ASN1.Feature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASN1.Type
{
    public class UnspecifiedType: IElementBase
    {
        private Element _element;
        public UnspecifiedType(Element el)
        {
            _element = el;
        }
    }
}
