using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ASN1.Component;
using ASN1.Feature;

namespace ASN1.Type.Primitive
{
    public class Integer : Element
    {
        
        private Org.BouncyCastle.Math.BigInteger _number;
        public Integer(Org.BouncyCastle.Math.BigInteger number)
        {
            typeTag = TYPE_INTEGER;
            if (!ValidateNumber(number))
            {
                throw new Exception($"'{number}' is not a valid number.");
            }
            _number = number;
        }
        public Integer(int number)
        {
            typeTag = TYPE_INTEGER;
            if (!ValidateNumber(number))
            {
                throw new Exception($"'{number}' is not a valid number.");
            }
            _number = new Org.BouncyCastle.Math.BigInteger(number.ToString());
        }

        public Integer(string number)
        {
            typeTag = TYPE_INTEGER;
            if (!ValidateNumber(number))
            {
                throw new Exception($"'{number}' is not a valid number.");
            }
            _number = new Org.BouncyCastle.Math.BigInteger(number);
        }

        public string Number => _number.ToString();

        public int IntNumber => Convert.ToInt32(_number.ToString());

        override protected string EncodedContentDER()
        {
            //DerInteger derInteger = new DerInteger(_number);
            //return derInteger.GetEncoded().ToHexString();
            byte[] bytes = _number.ToByteArray();

            // Ensure two's complement encoding
            if (bytes[0] == 0x00)
            {
                // Remove leading zero byte
                byte[] temp = new byte[bytes.Length - 1];
                Array.Copy(bytes, 1, temp, 0, temp.Length);
                bytes = temp;
            }
            else if (bytes[0] == 0xFF)
            {
                // Add leading zero byte
                byte[] temp = new byte[bytes.Length + 1];
                Array.Copy(bytes, 0, temp, 1, bytes.Length);
                bytes = temp;
            }

            return Convert.ToBase64String(bytes);
        }

        protected static IElementBase DecodeFromDER(Identifier identifier, string data, ref int offset)
        {
            int? idx = offset;
            int? expected = null;
            int length = Length.ExpectFromDER(data, ref idx, ref expected).IntLength();
            //byte[] bytes = Encoding.GetBytes(Encoding.GetString(data, idx, length));
            string bytes = data.Substring((int)idx, length);
            idx += length;
            Org.BouncyCastle.Math.BigInteger num = new Org.BouncyCastle.Math.BigInteger(bytes);
            offset = (int)idx;
            // late static binding since enumerated extends integer type
            return new Integer(num);
        }

        private static bool ValidateNumber(Org.BouncyCastle.Math.BigInteger number)
        {
            return true;
        }

        private static bool ValidateNumber(int number)
        {
            return true;
        }

        private static bool ValidateNumber(string number)
        {
            if (Regex.IsMatch(number, @"^-?\d+"))
            {
                return true;
            }
            else
            {
                return false;
            }
            
        }
    }
}
