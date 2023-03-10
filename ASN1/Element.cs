using ASN1;
using ASN1.Component;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ASN1.Type;
using ASN1.Feature;
using ASN1.Type.Constructed;

namespace ASN1
{
    public abstract class Element : IElementBase
    {
        protected const int TYPE_EOC = 0x00;
        protected const int TYPE_BOOLEAN = 0x01;
        protected const int TYPE_INTEGER = 0x02;
        protected const int TYPE_BIT_STRING = 0x03;
        protected const int TYPE_OCTET_STRING = 0x04;
        protected const int TYPE_NULL = 0x05;
        protected const int TYPE_OBJECT_IDENTIFIER = 0x06;
        protected const int TYPE_OBJECT_DESCRIPTOR = 0x07;
        protected const int TYPE_EXTERNAL = 0x08;
        protected const int TYPE_REAL = 0x09;
        protected const int TYPE_ENUMERATED = 0x0a;
        protected const int TYPE_EMBEDDED_PDV = 0x0b;
        protected const int TYPE_UTF8_STRING = 0x0c;
        protected const int TYPE_RELATIVE_OID = 0x0d;
        protected const int TYPE_SEQUENCE = 0x10;
        protected const int TYPE_SET = 0x11;
        protected const int TYPE_NUMERIC_STRING = 0x12;
        protected const int TYPE_PRINTABLE_STRING = 0x13;
        protected const int TYPE_T61_STRING = 0x14;
        protected const int TYPE_VIDEOTEX_STRING = 0x15;
        protected const int TYPE_IA5_STRING = 0x16;
        protected const int TYPE_UTC_TIME = 0x17;
        protected const int TYPE_GENERALIZED_TIME = 0x18;
        protected const int TYPE_GRAPHIC_STRING = 0x19;
        protected const int TYPE_VISIBLE_STRING = 0x1a;
        protected const int TYPE_GENERAL_STRING = 0x1b;
        protected const int TYPE_UNIVERSAL_STRING = 0x1c;
        protected const int TYPE_CHARACTER_STRING = 0x1d;
        protected const int TYPE_BMP_STRING = 0x1e;

        public static readonly Dictionary<int, string> MAP_TAG_TO_CLASS = new Dictionary<int, string>
        {
            {TYPE_EOC, typeof(Type.Primitive.EOC).FullName!},
            {TYPE_BOOLEAN, typeof(Type.Primitive.Boolean).FullName!},
            {TYPE_INTEGER, typeof(Type.Primitive.Integer).FullName!},
            {TYPE_BIT_STRING, typeof(Type.Primitive.BitString).FullName!},
            {TYPE_OCTET_STRING, typeof(Type.Primitive.OctetString).FullName!},
            {TYPE_NULL, typeof(Type.Primitive.NullType).FullName!},
            {TYPE_OBJECT_IDENTIFIER, typeof(Type.Primitive.ObjectIdentifier).FullName!},
            {TYPE_OBJECT_DESCRIPTOR, typeof(Type.Primitive.ObjectDescriptor).FullName!},
            {TYPE_REAL, typeof(Type.Primitive.Real).FullName!},
            {TYPE_ENUMERATED, typeof(Type.Primitive.Enumerated).FullName!},
            {TYPE_UTF8_STRING, typeof(Type.Primitive.UTF8String).FullName!},
            {TYPE_RELATIVE_OID, typeof(Type.Primitive.RelativeOID).FullName!},
            {TYPE_SEQUENCE, typeof(Constructed.Sequence).FullName},
            {TYPE_SET, typeof(Constructed.Set).FullName},
            {TYPE_NUMERIC_STRING, typeof(Primitive.NumericString).FullName},
            {TYPE_PRINTABLE_STRING, typeof(Primitive.PrintableString).FullName},
            {TYPE_T61_STRING, typeof(Primitive.T61String).FullName},
            {TYPE_VIDEOTEX_STRING, typeof(Primitive.VideotexString).FullName},
            {TYPE_IA5_STRING, typeof(Primitive.IA5String).FullName},
            {TYPE_UTC_TIME, typeof(Primitive.UTCTime).FullName},
            {TYPE_GENERALIZED_TIME, typeof(Primitive.GeneralizedTime).FullName},
            {TYPE_GRAPHIC_STRING, typeof(Primitive.GraphicString).FullName},
            {TYPE_VISIBLE_STRING, typeof(Primitive.VisibleString).FullName},
            {TYPE_GENERAL_STRING, typeof(Primitive.GeneralString).FullName},
            {TYPE_UNIVERSAL_STRING, typeof(Primitive.UniversalString).FullName},
            {TYPE_CHARACTER_STRING, typeof(Primitive.CharacterString).FullName},
            {TYPE_BMP_STRING, typeof(Primitive.BMPString).FullName},
        };

        const int TYPE_STRING = -1;
        const int TYPE_TIME = -2;
        const int TYPE_CONSTRUCTED_STRING = -3;

        public static readonly Dictionary<int, string> MAP_TYPE_TO_NAME = new Dictionary<int, string> {
            {TYPE_EOC, "EOC" },
            {TYPE_BOOLEAN, "BOOLEAN" },
            {TYPE_INTEGER, "INTEGER" },
            {TYPE_BIT_STRING, "BIT STRING" },
            {TYPE_OCTET_STRING, "OCTET STRING" },
            {TYPE_NULL, "NULL" },
            {TYPE_OBJECT_IDENTIFIER, "OBJECT IDENTIFIER" },
            {TYPE_OBJECT_DESCRIPTOR, "ObjectDescriptor" },
            {TYPE_EXTERNAL, "EXTERNAL" },
            {TYPE_REAL, "REAL" },
            {TYPE_ENUMERATED, "ENUMERATED" },
            {TYPE_EMBEDDED_PDV, "EMBEDDED PDV" },
            {TYPE_UTF8_STRING, "UTF8String" },
            {TYPE_RELATIVE_OID, "RELATIVE-OID" },
            {TYPE_SEQUENCE, "SEQUENCE" },
            {TYPE_SET, "SET" },
            {TYPE_NUMERIC_STRING, "NumericString" },
            {TYPE_PRINTABLE_STRING, "PrintableString" },
            {TYPE_T61_STRING, "T61String" },
            {TYPE_VIDEOTEX_STRING, "VideotexString" },
            {TYPE_IA5_STRING, "IA5String" },
            {TYPE_UTC_TIME, "UTCTime" },
            {TYPE_GENERALIZED_TIME, "GeneralizedTime" },
            {TYPE_GRAPHIC_STRING, "GraphicString" },
            {TYPE_VISIBLE_STRING, "VisibleString" },
            {TYPE_GENERAL_STRING, "GeneralString" },
            {TYPE_UNIVERSAL_STRING, "UniversalString" },
            {TYPE_CHARACTER_STRING, "CHARACTER STRING" },
            {TYPE_BMP_STRING, "BMPString" },
            {TYPE_STRING, "Any String" },
            {TYPE_TIME, "Any Time" },
            {TYPE_CONSTRUCTED_STRING, "Constructed String" },
        };

        protected int _typeTag;

        static void FromDER(string data, int? offset = null)
        {
            //$idx = $offset ?? 0;
            int? idx = offset ?? 0;
            //// decode identifier
            //$identifier = Identifier::fromDER($data, $idx);
            var identifier = Identifier.FromDER(data, ref idx);

            //// determine class that implements type specific decoding
            //$cls = self::_determineImplClass($identifier);
            var cls =
            //// decode remaining element
            //$element = $cls::_decodeFromDER($identifier, $data, $idx);
            //// if called in the context of a concrete class, check
            //// that decoded type matches the type of a calling class
            //$called_class = get_called_class();
            //    if (self::class !== $called_class) {
            //    if (!$element instanceof $called_class) {
            //        throw new \UnexpectedValueException(
            //            sprintf('%s expected, got %s.', $called_class, get_class($element)));
            //    }
            //}
            //// update offset for the caller
            //if (isset($offset))
            //{
            //            $offset = $idx;
            //}
            //return $element;
        }

        protected abstract string EncodedContentDER();

        public bool IsTagged() => this is TaggedType;

        // 381
        protected static string DetermineImplClass(Identifier identifier)
        {
            //    switch ($identifier->typeClass()) 
            switch (identifier.TypeClass())
            {
                //        case Identifier::CLASS_UNIVERSAL:
                case Identifier.CLASS_UNIVERSAL:
                    //            $cls = self::_determineUniversalImplClass($identifier->intTag());

                    //            // constructed strings may be present in BER
                    //            if ($identifier->isConstructed() && is_subclass_of($cls, StringType::class)) 
                    //            {
                    //                $cls = ConstructedString::class;
                    //            }
                    //            return $cls;
                    //        case Identifier::CLASS_CONTEXT_SPECIFIC:
                    //            return ContextSpecificType::class;
                    //        case Identifier::CLASS_APPLICATION:
                    //            return ApplicationType::class;
                    //        case Identifier::CLASS_PRIVATE:
                    //            return PrivateType::class;
            }
            //throw new \UnexpectedValueException(sprintf('%s %d not implemented.', Identifier::classToName($identifier->typeClass()), $identifier->tag()));
            throw new Exception("Exception");
        }

        // 403
        protected static string DetermineUniversalImplClass(int tag)
        {
            //if (!array_key_exists($tag, self::MAP_TAG_TO_CLASS))
            if (!MAP_TAG_TO_CLASS.ContainsKey(tag))
            //{
            {
                //    throw new \UnexpectedValueException("Universal tag {$tag} not implemented.");
                throw new Exception($"Universal tag {tag} not implemented.");
                //}
            }
            //return self::MAP_TAG_TO_CLASS[$tag];
            return MAP_TAG_TO_CLASS[tag];
        }

        public bool IsType(int tag)
        {
            // if element is context specific
            if (Identifier.CLASS_CONTEXT_SPECIFIC == TypeClass())
            {
                return false;
            }
            // negative tags identify an abstract pseudotype
            if (tag < 0)
            {
                return IsPseudoType(tag);
            }
            return IsConcreteType(tag);
        }

        public static string TagToName(int tag)
        {
            if (!MAP_TYPE_TO_NAME.ContainsKey(tag))
            {
                return "TAG " + tag;
            }
            return MAP_TAG_TO_CLASS[tag];
        }

        public abstract int TypeClass();

        public abstract bool IsConstructed();

        public int Tag() => _typeTag;

        /// <summary>
        /// Check whether the element is a concrete type of a given tag.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        private bool IsConcreteType<T>(int tag)
        {
            // if tag doesn't match
            if (Tag() != tag)
            {
                return false;
            }
            // if type is universal check that instance is of a correct class
            if (Identifier.CLASS_UNIVERSAL == TypeClass())
            {
                var cls = DetermineUniversalImplClass(tag);
                if (typeof(T).FullName != cls)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Check whether the element is a pseudotype.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        private bool IsPseudoType(int tag)
        {
            switch (tag)
            {
                case TYPE_STRING:
                    {
                        return this is IStringType;
                    }
                case TYPE_TIME:
                    {
                        return this is ITypeTime;
                    }
                    case TYPE_CONSTRUCTED_STRING:
                    {
                        return this is ConstructedString;
                    }
            }
            return false;
        }

    }
}
