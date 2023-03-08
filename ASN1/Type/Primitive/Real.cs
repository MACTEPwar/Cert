using ASN1.Component;
using ASN1.Feature;
using ASN1.Util;
using System.Text;
using System.Text.RegularExpressions;

namespace ASN1.Type.Primitive
{
    public class Real : Element
    {
        const string NR1_REGEX = @"^\s*(?<s>[+\-])?(?<i>\d+)$";

        const string NR2_REGEX = @"^\s*(?<s>[+\-])?(?<d>(?:\d+[\.,]\d*)|(?:\d*[\.,]\d+))$";

        const string NR3_REGEX = @"^\s*(?<ms>[+\-])?(?<m>(?:\d+[\.,]\d*)|(?:\d*[\.,]\d+))[Ee](?<es>[+\-])?(?<e>\d+)$";

        const string PHP_EXPONENT_DNUM = @"^(?<ms>[+\-])?(?<m>\d+|(?:\d*\.\d+|\d+\.\d*))[eE](?<es>[+\-])?(?<e>\d+)$";

        const int INF_EXPONENT = 2047;

        const int EXP_BIAS = -1023;

        private BigInt _mantissa;

        private BigInt _exponent;

        private int _base;

        private bool _strictDer;

        private float? _float;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mantissa">\GMP|int|string $mantissa Integer mantissa</param>
        /// <param name="exponent">\GMP|int|string $exponent Integer exponent</param>
        /// <param name="baseValue">$base     Base, 2 or 10</param>
        /// <exception cref="ArgumentException"></exception>
        public Real(dynamic mantissa, dynamic exponent, int baseValue = 10)
        {
            if (baseValue != 10 && baseValue != 2)
            {
                throw new ArgumentException("Base must be 2 or 10.");
            }

            typeTag = TYPE_REAL;
            _strictDer = true;
            _mantissa = new BigInt(mantissa);
            _exponent = new BigInt(exponent);
            _base = baseValue;
        }

        public override string ToString()
        {
            return string.Format("{0:g}", FloatVal());
        }

        public static Real FromFloat(float number)
        {
            if (float.IsPositiveInfinity(number) || float.IsNegativeInfinity(number))
            {
                return FromInfinite(number);
            }
            if (double.IsNaN(number))
            {
                throw new Exception("NaN values not supported.");
            }

            byte[] bytes = BitConverter.GetBytes(number);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            string packed = BitConverter.ToString(bytes).Replace("-", "");
            var res = Parse754Double(packed);
            var m = res.Item1;
            var e = res.Item2;

            return new Real(m, e, 2);
        }

        public static Real FromString(string number)
        {
            var parsed = ParseString(number);
            return new Real(parsed.Item1, parsed.Item2, 10);
        }

        public Real WithStrictDER(bool strict)
        {
            var obj = (Real)this.Clone();
            obj._strictDer = strict;
            return obj;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public BigInt Mantissa() => _mantissa;

        public BigInt Exponent() => _exponent;

        public int Base() => _base;

        public float FloatVal()
        {
            if (_float == null)
            {
                var m = _mantissa.IntVal();
                var e = _exponent.IntVal();
                _float = (float)(m * Math.Pow(_base, e));
            }
            return (float)_float;
        }

        public string Nr3Val()
        {
            Org.BouncyCastle.Math.BigInteger m;
            Org.BouncyCastle.Math.BigInteger e;
            string es = string.Empty;
            // convert to base 10
            if (_base == 2)
            {
                var res = ParseString($"{this.FloatVal,15:E}");
                m = res.Item1;
                e = res.Item2;
            }
            else
            {
                m = _mantissa.gmpObj();
                e = _exponent.gmpObj();
            }
            // shift trailing zeroes from the mantissa to the exponent
            // (X.690 07-2002, section 11.3.2.4)
            while (m.SignValue != 0 && m.Remainder(Org.BouncyCastle.Math.BigInteger.Ten).SignValue == 0)
            {
                m = m.Divide(Org.BouncyCastle.Math.BigInteger.Ten);
                e = e.Add(Org.BouncyCastle.Math.BigInteger.One);
            }

            if (e.CompareTo(0) == 0)
            {
                es = "+";
            }
            else
            {
                es = e.CompareTo(0) < 0 ? "-" : string.Empty;
            }

            return string.Format("{0}.E{1}{2}", m.ToString(), es, e.Abs().ToString());
        }

        protected override string EncodedContentDER()
        {
            if (_exponent.gmpObj().CompareTo(INF_EXPONENT) == 0)
            {
                return EncodeSpecial();
            }
            // if the real value is the value zero, there shall be no contents
            // octets in the encoding. (X.690 07-2002, section 8.5.2)
            if (_mantissa.gmpObj() == Org.BouncyCastle.Math.BigInteger.Zero || _mantissa.gmpObj() == null)
            {
                return string.Empty;
            }

            if (_base == 10)
            {
                return EncodeDecimal();
            }

            return EncodeBinary();
        }

        protected string EncodeBinary()
        {
            var res = PrepareBinaryEncoding();
            int baseValue = res.Item1;
            int sign = res.Item2;
            Org.BouncyCastle.Math.BigInteger m = res.Item3;
            Org.BouncyCastle.Math.BigInteger e = res.Item4;
            Org.BouncyCastle.Math.BigInteger zero = Org.BouncyCastle.Math.BigInteger.Zero;
            byte byteValue = 0x80;
            if (sign < 0)
            {
                byteValue |= 0x40;
            }
            // normalization: mantissa must be 0 or odd
            if (baseValue == 2)
            {
                // while last bit is zero
                while (m.SignValue > 0 && m.TestBit(0) == false)
                {
                    m = m.ShiftRight(1);
                    e = e.Add(Org.BouncyCastle.Math.BigInteger.One);
                }
            }
            else if (baseValue == 8)
            {
                byteValue |= 10;
                // while last 3 bits are zero
                while (m.SignValue > 0 && m.TestBit(0) == false && m.TestBit(1) == false && m.TestBit(2) == false)
                {
                    m = m.ShiftRight(3);
                    e = e.Add(Org.BouncyCastle.Math.BigInteger.One);
                }
            }
            else // base === 16
            {
                byteValue |= 20;
                // while last 4 bits are zero
                while (m.SignValue > 0 && m.TestBit(0) == false && m.TestBit(1) == false && m.TestBit(2) == false && m.TestBit(3) == false)
                {
                    m = m.ShiftRight(4);
                    e = e.Add(Org.BouncyCastle.Math.BigInteger.One);
                }
            }
            // scale factor
            int scale = 0;
            while (m.SignValue > 0 && m.TestBit(0) == false)
            {
                m = m.ShiftRight(1);
                ++scale;
            }
            byteValue = (byte)(byteValue | ((scale & 0x03) << 2));
            // encode exponent
            var exp_bytes = new BigInt(e).SignedOctets();
            var exp_len = exp_bytes.Length;
            if (exp_len > 0xff)
            {
                throw new Exception("Exponent encoding is too long.");
            }
            byte[] bytes;
            if (exp_len <= 3)
            {
                byteValue |= (byte)((exp_len - 1) & 0x03);
                bytes = new byte[] { byteValue };
            }
            else
            {
                byteValue |= 0x03;
                bytes = new byte[] { byteValue, (byte)exp_len };
            }
            return Encoding.ASCII.GetString(bytes) + exp_bytes + new BigInt(m).UnsignedOctets();
        }
        protected string EncodeDecimal()
        {
            return Encoding.ASCII.GetString(new byte[] { 0x03 }) + Nr3Val();
        }

        protected string EncodeSpecial()
        {
            switch (_mantissa.IntVal())
            {
                case 1:
                    {
                        return Encoding.ASCII.GetString(new byte[] { 0x40 });
                    }
                case -1:
                    {
                        return Encoding.ASCII.GetString(new byte[] { 0x41 });
                    }
            }
            throw new Exception("Invalid special value.");
        }

        protected static IElementBase DecodeFromDER(Identifier identifier, string data, ref int? offset)
        {
            int? idx = offset;
            int? expected = null;
            var length = Length.ExpectFromDER(data, ref idx, ref expected).IntLength();
            Real obj;
            if (length == 0)
            {
                obj = new Real(0, 0, 10);
            }
            else
            {
                byte[] bytes = Encoding.ASCII.GetBytes(data.Substring((int)idx, length));
                byte b = bytes[0];
                if ((0x80 & b) != 0) // bit 8 = 1
                {
                    obj = DecodeBinaryEncoding(bytes);
                }
                else if ((0x00 == (b >> 6))) // bit 8 = 0, bit 7 = 0
                {
                    obj = DecodeDecimalEncoding(bytes);
                }
                else // bit 8 = 0, bit 7 = 1
                {
                    obj = DecodeSpecialRealValue(bytes);
                }
            }
            offset = idx + length;
            return obj;
        }


        protected static Real DecodeBinaryEncoding(byte[] data)
        {
            byte byteValue = data[0];
            bool neg = (0x40 & byteValue) != 0;
            int baseValue;
            switch ((byteValue >> 4) & 0x03)
            {
                case 0b00:
                    baseValue = 2;
                    break;
                case 0b01:
                    baseValue = 8;
                    break;
                case 0b10:
                    baseValue = 16;
                    break;
                default:
                    throw new Exception("Reserved REAL binary encoding base not supported.");
            }
            // scaling factor in bits 4 and 3
            int scale = (byteValue >> 2) & 0x03;
            int idx = 1;
            // content length in bits 2 and 1
            int length = (byteValue & 0x03) + 1;
            if (length > 3)
            {
                if (data.Length < 2)
                {
                    throw new Exception("Unexpected end of data while decoding REAL exponent length.");
                }
                length = data[1];
                idx = 2;
            }
            if (data.Length < idx + length)
            {
                throw new Exception("Unexpected end of data while decoding REAL exponent.");
            }
            // decode exponent
            byte[] octets = data.Skip(idx).Take(length).ToArray();
            var exp = BigInt.FromSignedOctets(octets).gmpObj();
            if (baseValue == 8)
            {
                exp = exp.Multiply(new Org.BouncyCastle.Math.BigInteger("3"));
            }
            else if (baseValue == 16)
            {
                exp = exp.Multiply(new Org.BouncyCastle.Math.BigInteger("4"));
            }
            if (data.Length <= idx + length)
            {
                throw new Exception("Unexpected end of data while decoding REAL mantissa.");
            }
            // decode mantissa
            octets = data.Skip(idx + length).ToArray();
            var n = BigInt.FromUnsignedOctets(octets).gmpObj();
            n = n.Multiply(Org.BouncyCastle.Math.BigInteger.ValueOf(2).Pow(scale));
            if (neg)
            {
                n = n.Negate();
            }
            return new Real(n, exp, 2);
        }

        protected static Real DecodeDecimalEncoding(byte[] data)
        {
            var nr = data[0] & 0x3f;
            if (!new int[] { 1, 2, 3 }.Contains(nr))
            {
                throw new Exception("Unsupported decimal encoding form.");
            }
            var str = data.Skip(1).ToArray();
            return FromString(Encoding.ASCII.GetString(str));
        }

        protected static Real DecodeSpecialRealValue(byte[] data)
        {
            if (data.Length != 1)
            {
                throw new Exception("SpecialRealValue must have one content octet.");
            }
            var byteValue = data[0];
            if (0x40 == byteValue)
            {
                return FromInfinite(float.PositiveInfinity);
            }
            if (0x41 == byteValue)
            {
                return FromInfinite(float.NegativeInfinity);
            }
            throw new Exception("Invalid SpecialRealValue encoding.");
        }

        protected (int, int, Org.BouncyCastle.Math.BigInteger, Org.BouncyCastle.Math.BigInteger) PrepareBinaryEncoding()
        {
            var baseValue = 2;
            var m = _mantissa.gmpObj();
            var ms = m.CompareTo(Org.BouncyCastle.Math.BigInteger.Zero);
            m = m.Abs();
            var e = _exponent.gmpObj();
            var es = e.CompareTo(Org.BouncyCastle.Math.BigInteger.Zero);
            e = e.Abs();
            if (!_strictDer)
            {
                if (Org.BouncyCastle.Math.BigInteger.Zero.Equals(e.Mod(new Org.BouncyCastle.Math.BigInteger("4"))))
                {
                    baseValue = 16;
                    e = e.Divide(new Org.BouncyCastle.Math.BigInteger("4"));
                }
                else if (Org.BouncyCastle.Math.BigInteger.Zero.Equals(e.Mod(new Org.BouncyCastle.Math.BigInteger("3"))))
                {
                    baseValue = 8;
                    e = e.Divide(new Org.BouncyCastle.Math.BigInteger("3"));
                }
            }
            return (baseValue, ms, m, e.Multiply(Org.BouncyCastle.Math.BigInteger.ValueOf(ms)));
        }

        private static Real FromInfinite(float inf)
        {
            return new Real(inf == float.NegativeInfinity ? -1 : 1, INF_EXPONENT, 2);
        }

        private static (Org.BouncyCastle.Math.BigInteger, Org.BouncyCastle.Math.BigInteger) Parse754Double(string octets)
        {
            byte[] bytes = Encoding.BigEndianUnicode.GetBytes(octets);
            Org.BouncyCastle.Math.BigInteger n = new Org.BouncyCastle.Math.BigInteger(1, bytes); // assumes octets is in big-endian byte order
            bool neg = n.TestBit(63);
            // 11 bits of biased exponent
            var exp = n.And(new Org.BouncyCastle.Math.BigInteger("0x7ff0000000000000")).ShiftRight(52).Add(Org.BouncyCastle.Math.BigInteger.ValueOf(EXP_BIAS) );
            // 52 bits of mantissa
            Org.BouncyCastle.Math.BigInteger man = n.And(new Org.BouncyCastle.Math.BigInteger("0xfffffffffffff"));
            // zero, ASN.1 doesn't differentiate -0 from +0
            if (exp.Equals(Org.BouncyCastle.Math.BigInteger.ValueOf(EXP_BIAS)) && man.Equals(Org.BouncyCastle.Math.BigInteger.Zero))
            {
                return (Org.BouncyCastle.Math.BigInteger.Zero, Org.BouncyCastle.Math.BigInteger.Zero );
            }
            // denormalized value, shift binary point
            if ((exp.Equals(Org.BouncyCastle.Math.BigInteger.ValueOf(EXP_BIAS))))
            {
                exp = exp.Add(Org.BouncyCastle.Math.BigInteger.One);
            }
            // normalized value, insert implicit leading one before the binary point
            else
            {
                man = man.SetBit(52);
            }
            // find the last fraction bit that is set
            int last = man.GetLowestSetBit();
            int bits_for_fraction = 52 - last;
            // adjust mantissa and exponent so that we have integer values
            man = man.ShiftRight(last);
            exp = exp.Subtract(Org.BouncyCastle.Math.BigInteger.ValueOf(bits_for_fraction));
            // negate mantissa if number was negative
            if (neg)
            {
                man = man.Negate();
            }
            return (man, exp);
        }

        private static (Org.BouncyCastle.Math.BigInteger, Org.BouncyCastle.Math.BigInteger) ParseString(string str)
        {
            (Org.BouncyCastle.Math.BigInteger, Org.BouncyCastle.Math.BigInteger) res;
            // PHP exponent format
            Match match1 = new Regex(PHP_EXPONENT_DNUM).Match(str);
            Match match2 = new Regex(NR3_REGEX).Match(str);
            Match match3 = new Regex(NR2_REGEX).Match(str);
            Match match4 = new Regex(NR1_REGEX).Match(str);
            if (match1.Success)
            {
                res = ParsePHPExponentMatch(match1);
            }
            // NR3 format
            else if (match2.Success)
            {
                res = ParseNR3Match(match2);
            }
            // NR2 format
            else if (match3.Success)
            {
                res = ParseNR2Match(match3);
            }
            // NR1 format
            else if (match4.Success)
            {
                res = ParseNR1Match(match4);
            }
            // invalid number
            else
            {
                throw new Exception($"{str} could not be parsed to REAL.");
            }

            var m = res.Item1;
            var e = res.Item2;

            while (!m.Equals(Org.BouncyCastle.Math.BigInteger.Zero) && m.Mod(Org.BouncyCastle.Math.BigInteger.Ten).Equals(Org.BouncyCastle.Math.BigInteger.Zero))
            {
                m = m.Divide(Org.BouncyCastle.Math.BigInteger.Ten);
                e = e.Add(Org.BouncyCastle.Math.BigInteger.One);
            }

            return (m, e);
        }

        private static (Org.BouncyCastle.Math.BigInteger, Org.BouncyCastle.Math.BigInteger) ParsePHPExponentMatch(Match match)
        {
            Org.BouncyCastle.Math.BigInteger ms = match.Groups["ms"].Value == "-" ? Org.BouncyCastle.Math.BigInteger.ValueOf(-1) : Org.BouncyCastle.Math.BigInteger.One;
            string[] m_parts = match.Groups["m"].Value.Split('.');
            string int_part = m_parts[0].TrimStart('0');
            Org.BouncyCastle.Math.BigInteger es = match.Groups["es"].Value == "-" ? Org.BouncyCastle.Math.BigInteger.ValueOf(-1) : Org.BouncyCastle.Math.BigInteger.One;
            Org.BouncyCastle.Math.BigInteger e = new Org.BouncyCastle.Math.BigInteger(match.Groups["e"].Value).Multiply(es);
            if (m_parts.Length == 2)
            {
                string frac_part = m_parts[1].TrimEnd('0');
                e = e.Subtract(Org.BouncyCastle.Math.BigInteger.ValueOf(frac_part.Length));
                int_part += frac_part;
            }
            Org.BouncyCastle.Math.BigInteger m = new Org.BouncyCastle.Math.BigInteger(int_part).Multiply(ms);
            return (m, e);
        }

        private static (Org.BouncyCastle.Math.BigInteger, Org.BouncyCastle.Math.BigInteger) ParseNR3Match(Match match)
        {
            Org.BouncyCastle.Math.BigInteger ms = match.Groups["ms"].Value == "-" ? Org.BouncyCastle.Math.BigInteger.ValueOf(-1) : Org.BouncyCastle.Math.BigInteger.One;

            // explode mantissa to integer and fraction parts
            var parts = match.Groups["m"].Value.Replace(",", ".").Split('.');
            var intPart = parts[0].TrimStart('0');
            var fracPart = parts.Length > 1 ? parts[1].TrimEnd('0') : "";
            // exponent sign
            Org.BouncyCastle.Math.BigInteger es = match.Groups["es"].Value == "-" ? Org.BouncyCastle.Math.BigInteger.ValueOf(-1) : Org.BouncyCastle.Math.BigInteger.One;
            // signed exponent
            var e = new Org.BouncyCastle.Math.BigInteger(match.Groups["e"].Value).Multiply(es);
            // shift exponent by the number of base 10 fractions
            e = e.Subtract(Org.BouncyCastle.Math.BigInteger.ValueOf(fracPart.Length));
            // insert fractions to integer part and produce signed mantissa
            intPart += fracPart;
            if (intPart == "")
            {
                intPart = "0";
            }
            var m = new Org.BouncyCastle.Math.BigInteger(intPart).Multiply(ms);
            return (m, e);
        }

        private static (Org.BouncyCastle.Math.BigInteger, Org.BouncyCastle.Math.BigInteger) ParseNR2Match(Match match)
        {
            // mantissa sign
            var sign = match.Groups["s"].Value == "-" ? Org.BouncyCastle.Math.BigInteger.ValueOf(-1) : Org.BouncyCastle.Math.BigInteger.One;

            // explode decimal number to integer and fraction parts
            var parts = match.Groups["d"].Value.Replace(",", ".").Split('.');
            var intPart = parts[0].TrimStart('0');
            var fracPart = parts.Length > 1 ? parts[1].TrimEnd('0') : "";

            // shift exponent by the number of base 10 fractions
            var e = Org.BouncyCastle.Math.BigInteger.Zero;
            e = e.Subtract(Org.BouncyCastle.Math.BigInteger.ValueOf(fracPart.Length));

            // insert fractions to integer part and produce signed mantissa
            intPart += fracPart;
            if (intPart == "")
            {
                intPart = "0";
            }
            var m = new Org.BouncyCastle.Math.BigInteger(intPart).Multiply(sign);

            return (m, e);
        }

        private static (Org.BouncyCastle.Math.BigInteger, Org.BouncyCastle.Math.BigInteger) ParseNR1Match(Match match)
        {
            // mantissa sign
            var sign = match.Groups["s"].Value == "-" ? Org.BouncyCastle.Math.BigInteger.ValueOf(-1) : Org.BouncyCastle.Math.BigInteger.One;
            var intPart = match.Groups["i"].Value.TrimStart('0');
            if (intPart == "")
            {
                intPart = "0";
            }
            var m = new Org.BouncyCastle.Math.BigInteger(intPart).Multiply(sign);
            return (m, Org.BouncyCastle.Math.BigInteger.Zero);
        }
    }
}
