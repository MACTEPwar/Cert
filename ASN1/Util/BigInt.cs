using System.Text;

namespace ASN1.Util
{
    public class BigInt
    {
        private Org.BouncyCastle.Math.BigInteger _gmp;
        private string? _num;
        private int? _intNum;

        public BigInt(Org.BouncyCastle.Math.BigInteger num)
        {
            _gmp = num;
        }

        public BigInt(int num)
        {
            try
            {
                _gmp = new Org.BouncyCastle.Math.BigInteger(num.ToString());
            }
            catch(Exception ex)
            {
                throw new Exception($"Unable to convert '{num}' to integer.");
            }
        }

        public BigInt(string num)
        {
            try
            {
                _gmp = new Org.BouncyCastle.Math.BigInteger(num);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to convert '{num}' to integer.");
            }
        }

        public override string ToString()
        {
            return Base10();
        }

        public static BigInt FromUnsignedOctets(byte[] octets)
        {
            if (octets.Length == 0)
            {
                throw new Exception("Empty octets.");
            }
            byte[] bytes = octets;
            Array.Reverse(bytes);
            return new BigInt(new Org.BouncyCastle.Math.BigInteger(1, bytes));
        }

        public static BigInt FromSignedOctets(byte[] octets)
        {
            if (octets.Length == 0)
            {
                throw new Exception("Empty octets.");
            }

            bool neg = (octets[0] & 0x80) != 0;

            if (neg)
            {
                byte[] octetBytes = octets;
                for (int i = 0; i < octetBytes.Length; i++)
                {
                    octetBytes[i] = (byte)~octetBytes[i];
                }
                octets = octetBytes;
            }

            Org.BouncyCastle.Math.BigInteger num = new Org.BouncyCastle.Math.BigInteger(octets);

            if (neg)
            {
                num = num.Negate().Add(Org.BouncyCastle.Math.BigInteger.One);
            }

            return new BigInt(num);
        }

        public string Base10()
        {
            if (_num == null)
            {
                _num = _gmp.ToString();
            }
            return _num;
        }

        public int IntVal()
        {
            if (_intNum == null)
            {
                if (_gmp.CompareTo(IntMaxGmp()) > 0)
                {
                    throw new InvalidOperationException("Integer overflow.");
                }

                if (_gmp.CompareTo(IntMinGmp) < 0)
                {
                    throw new InvalidOperationException("Integer underflow.");
                }

                _intNum = _gmp.IntValue;
            }

            return _intNum.Value;
        }

        public Org.BouncyCastle.Math.BigInteger gmpObj() => new Org.BouncyCastle.Math.BigInteger(this._gmp.ToByteArray());

        public string UnsignedOctets()
        {
            byte[] octets = _gmp.ToByteArray();
            if (octets.Length > 1 && octets[0] == 0)
            {
                // remove leading zero byte
                byte[] tmp = new byte[octets.Length - 1];
                Array.Copy(octets, 1, tmp, 0, tmp.Length);
                octets = tmp;
            }
            Array.Reverse(octets);
            return Convert.ToBase64String(octets);

        }

        public string SignedOctets()
        {
            switch (_gmp.SignValue)
            {
                case 1:
                    return SignedPositiveOctets();
                case -1:
                    return SignedNegativeOctets();
            }
            // zero
            return "\x00";
        }

        private string SignedPositiveOctets()
        {
            byte[] bytes = this._gmp.ToByteArray();
            // if first bit is 1, prepend full zero byte to represent positive two's complement
            if ((bytes[0] & 0x80) != 0)
            {
                byte[] newBytes = new byte[bytes.Length + 1];
                newBytes[0] = 0x00;
                Array.Copy(bytes, 0, newBytes, 1, bytes.Length);
                bytes = newBytes;
            }
            return Encoding.Default.GetString(bytes);
        }

        private string SignedNegativeOctets()
        {
            var num = _gmp.Abs();
            // compute number of bytes required
            var width = 1;
            if (num.CompareTo(new Org.BouncyCastle.Math.BigInteger("128")) > 0)
            {
                var tmp = num;
                do
                {
                    ++width;
                    tmp.ShiftRight(8);
                } while (tmp.CompareTo(new Org.BouncyCastle.Math.BigInteger("128")) > 0);
            }
            // compute two's complement 2^n - x
            num = Org.BouncyCastle.Math.BigInteger.ValueOf(2).Pow(8 * width).Subtract(num);
            //var bin = num.ToByteArray(isUnsigned: false, isBigEndian: true);
            var bin = num.ToByteArray();
            // if first bit is 0, prepend full inverted byte to represent negative two's complement
            if ((bin[0] & 0x80) == 0)
            {
                bin = new byte[width + 1];
                bin[0] = 0xff;
                num.ToByteArray().CopyTo(bin, 1);
            }
            return Encoding.UTF8.GetString(bin);
        }

        private Org.BouncyCastle.Math.BigInteger IntMaxGmp()
        {
            if (_gmp == null)
            {
                _gmp = new Org.BouncyCastle.Math.BigInteger(int.MaxValue.ToString());
            }
            return _gmp;
        }

        private Org.BouncyCastle.Math.BigInteger IntMinGmp()
        {
            if (_gmp == null)
            {
                _gmp = new Org.BouncyCastle.Math.BigInteger(int.MinValue.ToString());
            }
            return _gmp;
        }
    }
}
