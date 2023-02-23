using ASN1.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASN1.Feature
{
    public interface IElementBase
    {
        public int TypeClass();
        public bool IsConstructed();
        public int Tag();
        public bool IsType(int tag);
        public IElementBase ExpectType(int tag);
        public bool IsTagged();
        public TaggedType ExpectTagged(int? tag = null);
        public Element AsElement();
        public UnspecifiedType AsUnspecified();
    }
}
