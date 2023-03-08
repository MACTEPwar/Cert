namespace ASN1.Type.Primitive
{
    public class Enumerated: Integer
    {
        public Enumerated(int number): base(number)
        {
            typeTag = TYPE_ENUMERATED;
        }

        public Enumerated(string number) : base(number)
        {
            typeTag = TYPE_ENUMERATED;
        }
    }
}
