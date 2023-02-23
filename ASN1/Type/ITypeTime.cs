using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASN1.Type
{
    public interface ITypeTime : IStringType
    {
        public DateTime DateTime();
    }
}
