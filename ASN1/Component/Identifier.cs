
namespace ASN1.Component
{
    public class Identifier
    {
        private int _cla;
        private int _pc;
        private System.Numerics.BigInteger _tag;

        public const int CLASS_UNIVERSAL = 0b00;
        public const int CLASS_APPLICATION = 0b01;
        public const int CLASS_CONTEXT_SPECIFIC = 0b10;
        public const int CLASS_PRIVATE = 0b11;

        // P/C enumerations
        const int PRIMITIVE = 0b0;
        const int CONSTRUCTED = 0b1;

        public Identifier(int cla, int pc, System.Numerics.BigInteger tag)
        {
            _cla = _cla & 0b11;
            _pc = pc & 0b1;
            _tag = tag;
        }
        public int TypeClass()
        {
            return _cla;
        }

        public bool IsPrimitive()
        {
            return CLASS_PRIVATE == _cla;
        }

        // 88
        // deafult offset null
        public static Identifier FromDER(string data, ref int? offset)
        {
            int idx = offset ?? 0;
            int datalen = data.Length;
            if (idx >= datalen)
            {
                throw new Exception("Invalid offset.");
            }
            byte bte = Convert.ToByte(data[idx++]);
            // bits 8 and 7 (class)
            // 0 = universal, 1 = application, 2 = context-specific, 3 = private
            int cla = (0b11000000 & bte) >> 6;
            // bit 6 (0 = primitive / 1 = constructed)
            int pc = (0b00100000 & bte) >> 5;
            // bits 5 to 1 (tag number)
            int tag = (0b00011111 & bte);
            // long-form identifier
            if (0x1f == tag)
            {
                tag = (int)DecodeLongFormTag(data, ref idx);
            }
            if (offset != null)
            {
                offset = idx;
            }
            return new Identifier(cla, pc, tag);
        }

        private static System.Numerics.BigInteger DecodeLongFormTag(string data, ref int offset)
        {
            int datalen = data.Length;
            System.Numerics.BigInteger tag = System.Numerics.BigInteger.Zero;
            while (true)
            {
                if (offset >= datalen)
                {
                    throw new Exception(
                    "Unexpected end of data while decoding long form identifier.");
                }
                byte bte = (byte)data[offset++];
                tag = System.Numerics.BigInteger.Multiply(tag, 128.ToBigInteger());
                tag = System.Numerics.BigInteger.Add(tag, (bte & 0x7F).ToBigInteger());
                // last byte has bit 8 set to zero
                if ((bte & 0x80) == 0)
                {
                    break;
                }
            }
            return tag;
        }

        public bool IsConstructed() => CONSTRUCTED == _pc;
    }
}
