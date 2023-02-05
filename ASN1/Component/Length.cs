using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASN1.Component
{
    public class Length
    {
        private System.Numerics.BigInteger _length;
        private bool _indefinite;

        public Length(System.Numerics.BigInteger length, bool indefinite = false)
        {
            _length = length;
            _indefinite = indefinite;
        }

        public static Length FromDER(string data, ref int? offset)
        {
            int idx = offset ?? 0;
            int datalen = data.Length;
            if (idx >= datalen)
            {
                throw new Exception("Unexpected end of data while decoding length.");
            }
            bool indefinite = false;
            int bte = (int)data[idx++];
            // bits 7 to 1
            int length = (0x7f & bte);
            // long form
            if ((0x80 & bte) != 0)
            {
                if (length == 0)
                {
                    indefinite = true;
                }
                else
                {
                    if (idx + length > datalen)
                    {
                        throw new Exception("Unexpected end of data while decoding long form length.");
                    }
                    length = (int)DecodeLongFormLength(length, data, ref idx);
                }
            }
            if (offset != null)
            {
                offset = idx;
            }
            return new Length(length, indefinite);
        }

        // 100
        public static Length ExpectFromDER(string data, ref int? offset, ref int? expected)
        {
            int? idx = offset;
            var length = Length.FromDER(data, ref idx);
            if (expected != null)
            {
                if (length.IsIndefinite())
                {
                    throw new Exception(string.Format("Expected length {0}, got indefinite.", expected));
                }
                if (expected != length.IntLength())
                {
                    throw new Exception(string.Format("Expected length {0}, got {1}.", expected, length.IntLength()));
                }
            }
            if (!length.IsIndefinite() && data.Length < idx + length.IntLength())
            {
                throw new Exception(string.Format("Length {0} overflows data, {1} bytes left.", length.IntLength(), data.Length - idx));
            }
            offset = idx;
            return length;
        }

        public string ToDER()
        {
            List<byte> bytes = new List<byte>();
            if (_indefinite)
            {
                bytes.Add(0x80);
            }
            else
            {
                //System.Numerics.BigInteger num = _length.GmpObj();
                System.Numerics.BigInteger num = _length;
                // long form
                if (num > 127)
                {
                    List<byte> octets = new List<byte>();
                    while (num > 0)
                    {
                        octets.Add((byte)(0xff & num));
                        num >>= 8;
                    }
                    int count = octets.Count;
                    // first octet must not be 0xff
                    if (count >= 127)
                    {
                        throw new Exception("Too many length octets.");
                    }
                    bytes.Add((byte)(0x80 | count));
                    var tempOctets = octets;
                    tempOctets.Reverse();
                    foreach (var octet in tempOctets)
                    {
                        bytes.Add(octet);
                    }
                }
                // short form
                else
                {
                    bytes.Add((byte)num);
                }
            }
            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        public string GetLength()
        {
            if (_indefinite)
            {
                throw new InvalidOperationException("Length is indefinite.");
            }
            return _length.ToString();
        }

        public int IntLength()
        {
            if (_indefinite)
            {
                throw new InvalidOperationException("Length is indefinite.");
            }
            return (int)_length;
        }

        public bool IsIndefinite()
        {
            return _indefinite;
        }

        private static System.Numerics.BigInteger DecodeLongFormLength(int length, string data, ref int offset)
        {
            // first octet must not be 0xff (spec 8.1.3.5c)
            if (127 == length)
            {
                throw new Exception("Invalid number of length octets.");
            }
            System.Numerics.BigInteger num = new System.Numerics.BigInteger(0);
            while (--length >= 0)
            {
                int bte = (int)data[offset++];
                num = num << 8;
                num |= bte;
            }
            return num;
        }
    }
}
