using ASN1.Component;
using ASN1.Feature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASN1.Type.Primitive
{
    public class BitString : BaseString
    {
        protected int _unusedBits;
        public BitString(string str, int unusedBits = 0) : base(str)
        {
            typeTag = TYPE_BIT_STRING;
            _unusedBits = unusedBits;
        }

        public int NumBits() => Str.Length * 8 - _unusedBits;

        public int UnusedBits() => _unusedBits;

        public bool TestBit(int idx)
        {
            int oi = (int)Math.Floor((decimal)idx / 8);
            if (oi < 0 || oi >= Str.Length)
            {
                throw new Exception("Index is out of bounds.");
            }
            int bi = idx % 8;
            if (oi == Str.Length - 1)
            {
                if (bi >= 8 - _unusedBits)
                {
                    throw new Exception("Index refers to an unused bit.");
                }
            }
            char bte = Str[oi];
            int mask = 0x01 << (7 - bi);
            return ((int)bte & mask) > 0;
        }

        public string Range(int start, int length)
        {
            //if (!$length) {
            //    return '0';
            //}
            if (start + length > NumBits())
            {
                throw new Exception("Not enough bits.");
            }
            Org.BouncyCastle.Math.BigInteger bits = Org.BouncyCastle.Math.BigInteger.Zero;
            var idx = start;
            var end = start + length;
            while (true)
            {
                int bit = TestBit(idx) ? 1 : 0;
                bits = bits.Or(Org.BouncyCastle.Math.BigInteger.ValueOf(bit));
                if (++idx >= end)
                {
                    break;
                }
                bits = bits.ShiftLeft(1);
            }
            return bits.ToString();
        }

        public BitString WithoutTrailingZeroes()
        {
            if (Str.Length == 0)
            {
                return new BitString(string.Empty);
            }
            var bits = Str;
            var unused_octets = 0;
            for (int idx = bits.Length - 1; idx >= 0; --idx, ++unused_octets)
            {
                if (bits[idx] != '\x0')
                {
                    break;
                }
            }
            if (unused_octets != 0)
            {
                bits = bits.Substring(0, -unused_octets);
            }
            if (bits.Length == 0)
            {
                return new BitString(string.Empty);
            }
            var unused_bits = 0;
            var bte = (int)bits[bits.Length - 1];
            while ((bte & 0x01) == 0)
            {
                ++unused_bits;
                bte >>= 1;
            }
            return new BitString(bits, unused_bits);
        }

        override protected string EncodedContentDER()
        {
            StringBuilder der = new StringBuilder();
            der.Append((char)_unusedBits);
            der.Append(Str);

            if (_unusedBits != 0)
            {
                int index = der.Length - 1;
                char octet = der[index];
                octet &= (char)(0xFF & ~((1 << _unusedBits) - 1));
                der[index] = octet;
            }

            return der.ToString();
        }

        protected static IElementBase DecodeFromDER(Identifier identifier, string data, ref int offset)
        {
            int? idx = offset;
            int? expected = null;
            var length = Length.ExpectFromDER(data, ref idx, ref expected);
            if (length.IntLength() < 1)
            {
                throw new Exception("Bit string length must be at least 1.");
            }
            var unused_bits = (int)data[(int)idx++];
            if (unused_bits > 7)
            {
                throw new Exception("Unused bits in a bit string must be less than 8.");
            }
            var str_len = length.IntLength() - 1;
            string str = string.Empty;
            if (str_len > 0)
            {
                str = data.Substring((int)idx, str_len);
                if (unused_bits > 0)
                {
                    byte last_byte = (byte)str[str_len - 1];
                    byte mask = (byte)((1 << unused_bits) - 1);
                    if ((last_byte & mask) != 0)
                    {
                        throw new Exception(
                            "DER encoded bit string must have zero padding.");
                    }
                }
            }
            else
            {
                str = string.Empty;
            }
            offset = (int)idx + str_len;
            return new BitString(str, unused_bits);
        }
    }
}
