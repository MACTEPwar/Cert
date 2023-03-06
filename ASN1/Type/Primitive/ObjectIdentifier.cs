
using ASN1.Component;
using ASN1.Feature;

namespace ASN1.Type.Primitive
{
    public class ObjectIdentifier : Element
    {
        protected string _oid;
        protected List<Org.BouncyCastle.Math.BigInteger> _subids;

        public ObjectIdentifier(string oid)
        {
            _oid = oid;
            _subids = ExplodeDottedOID(oid);
            if (_subids.Count > 0)
            {
                if (_subids.Count < 2)
                {
                    throw new Exception("OID must have at least two nodes.");
                }
                if (_subids[0].CompareTo(new Org.BouncyCastle.Math.BigInteger("2")) > 0)
                {
                    throw new Exception("Root arc must be in range of 0..2.");
                }
                if (_subids[0].CompareTo(new Org.BouncyCastle.Math.BigInteger("2")) < 0 && _subids[1].CompareTo(new Org.BouncyCastle.Math.BigInteger("40")) >= 0)
                {
                    throw new Exception("Second node must be in 0..39 range for root arcs 0 and 1.");
                }
            }
            typeTag = TYPE_OBJECT_IDENTIFIER;
        }

        public string Oid() => _oid;

        protected override string EncodedContentDER()
        {
            List<Org.BouncyCastle.Math.BigInteger> subids = _subids;
            if (_subids.Count >= 2)
            {
                var num = subids[0].Multiply(new Org.BouncyCastle.Math.BigInteger("40")).Add(subids[1]);
                subids.RemoveRange(0, 2);
                subids.Insert(0, num);
            }
            return EncodeSubIDs(subids.ToArray());
        }

        protected static IElementBase DecodeFromDER(Identifier identifier, string data, ref int? offset)
        {
            int? idx = offset;
            int? expected = null;
            var length = Length.ExpectFromDER(data, ref idx, ref expected).IntLength();
            var subids = DecodeSubIDs(data.Substring((int)idx, length));
            idx += length;
            if (subids[0] != null)
            {
                Org.BouncyCastle.Math.BigInteger x, y;
                if (subids[0].CompareTo(new Org.BouncyCastle.Math.BigInteger("80")) < 0)
                {
                    Org.BouncyCastle.Math.BigInteger[] result = subids[0].DivideAndRemainder(Org.BouncyCastle.Math.BigInteger.ValueOf(40));
                    x = result[0];
                    y = result[1];
                    subids.RemoveAt(0);
                    subids.Insert(0, x);
                    subids.Insert(1, y);
                }
                else
                {
                    x = Org.BouncyCastle.Math.BigInteger.ValueOf(2);
                    y = subids[0].Subtract(Org.BouncyCastle.Math.BigInteger.ValueOf(80));
                    subids.RemoveAt(0);
                    subids.Insert(0, x);
                    subids.Insert(1, y);
                }
            }
            offset = idx;
            return new ObjectIdentifier(ImplodeSubIDs(subids.ToArray()));
        }

        protected static List<Org.BouncyCastle.Math.BigInteger> ExplodeDottedOID(string oid)
        {
            List<Org.BouncyCastle.Math.BigInteger> subids = new List<Org.BouncyCastle.Math.BigInteger>();
            if (oid.Length > 0)
            {
                string[] subidStrings = oid.Split('.');
                subids = subidStrings.Select(s =>
                {
                    try
                    {
                        return new Org.BouncyCastle.Math.BigInteger(s);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"'{s}' is not a number");
                    }
                }).ToList();
            }
            return subids;
        }

        protected static string ImplodeSubIDs(Org.BouncyCastle.Math.BigInteger[] subids)
        {
            return string.Join(".", Array.ConvertAll(subids, num => num.ToString()));
        }

        protected static string EncodeSubIDs(Org.BouncyCastle.Math.BigInteger[] subids)
        {
            string data = string.Empty;
            for(int i = 0; i < subids.Length; i++)
            {
                var subid = subids[i];
                if (subid.CompareTo(new Org.BouncyCastle.Math.BigInteger("128")) < 0)
                {
                    byte[] bytes = subid.ToByteArray();
                    int intValue = BitConverter.ToInt32(bytes);
                    data += (char)intValue;
                }
                else
                {
                    List<byte> bytes = new List<byte>();
                    do
                    {
                        bytes.Insert(0, (byte)(subid.And(new Org.BouncyCastle.Math.BigInteger("7f", 16))).ToByteArray()[0]);
                        subid = subid.ShiftRight(7);
                    } while (subid.CompareTo(new Org.BouncyCastle.Math.BigInteger("0")) > 0);
                    // all bytes except last must have bit 8 set to one
                    foreach (byte b in bytes.Take(bytes.Count - 1))
                    {
                        data += (char)(0x80 | b);
                    }
                    data += (char)bytes.Last();
                }
            }
            return data;
        }

        protected static List<Org.BouncyCastle.Math.BigInteger> DecodeSubIDs(string data)
        {
            List<Org.BouncyCastle.Math.BigInteger> subids = new List<Org.BouncyCastle.Math.BigInteger>();
            var idx = 0;
            var end = data.Length;
            while (idx < end)
            {
                var num = Org.BouncyCastle.Math.BigInteger.Zero;
                while (true)
                {
                    if (idx >= end)
                    {
                        throw new Exception("Unexpected end of data.");
                    }

                    var b = (int)data[idx++];
                    num = num.Or(Org.BouncyCastle.Math.BigInteger.ValueOf(b & 0x7f));
                    if ((b & 0x80) == 0)
                    {
                        break;
                    }
                    num.ShiftLeft(7);
                }
                subids.Add(num);
            }
            return subids;
        }
    }
}
