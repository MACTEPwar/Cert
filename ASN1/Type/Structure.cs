using ASN1.Component;
using ASN1.Feature;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASN1.Type
{
    public abstract class Structure : Element, IEnumerable
    {
        protected List<Element> _elements;
        private List<TaggedType> _taggedMap;
        private List<UnspecifiedType> _unspecifiedTypes;

        public Structure(List<IElementBase> elements)
        {
            this._elements = elements.Select(el => el.AsElement()).ToList();
        }

        public object Clone()
        {
            // clear cache-variables
            this._taggedMap = null;
            this._unspecifiedTypes = null;
            return MemberwiseClone();
        }

        public bool IsConstructed() => true;

        public List<string> ExplodeDER(string data)
        {
            int? offset = 0;
            int? expected = null;
            var identifier = Identifier.FromDER(data, ref offset);
            if (!identifier.IsConstructed())
            {
                throw new Exception("Element is not constructed.");
            }
            var length = Length.ExpectFromDER(data, ref offset, ref expected);
            if (length.IsIndefinite())
            {
                throw new Exception("Explode not implemented for indefinite length encoding.");
            }
            var end = offset + length.IntLength();
            var parts = new List<string>();
            int len;
            while (offset < end)
            {
                // start of the element
                var idx = offset;
                // skip identifier
                Identifier.FromDER(data, ref offset);
                // decode element length
                len = Length.ExpectFromDER(data, ref offset, ref expected).IntLength();
                // extract der encoding of the element
                parts.Add(data.Substring((int)idx, (int)offset! - (int)idx + len));
                // update offset over content
                offset += len;
            }
            return parts;
        }

        public Structure WithInserted(int idx, Element el)
        {
            if (_elements.Count < idx || idx < 0)
            {
                throw new Exception($"Index {idx} is out of bounds.");
            }
            var obj = (Structure)Clone();
            List<Element> elements = obj._elements;
            elements.Insert(idx, el);
            obj._elements = elements;
            return obj;
        }

        public Structure WithAppended(Element el)
        {
            var obj = (Structure)Clone();
            obj._elements.Add(el);
            return obj;
        }

        public Structure WithPreppended(Element el)
        {
            var obj = (Structure)Clone();
            obj._elements.Insert(0,el);
            return obj;
        }

        public Structure WithoutElement(int idx)
        {
            if (_elements.Count >= idx || _elements[idx] == null)
            {
                throw new Exception($"Structure doesn't have element at index {idx}.");
            }
            var obj = (Structure)Clone();
            List<Element> elements = obj._elements;
            elements.RemoveAt(idx);
            obj._elements = elements;
            return obj;
        }

        /// <summary>
        /// Get elements in the structure.
        /// </summary>
        /// <returns>UnspecifiedType[]</returns>
        public List<UnspecifiedType> Elements()
        {
            if (_unspecifiedTypes == null)
            {
                _unspecifiedTypes = _elements.Select(el => new UnspecifiedType(el)).ToList();
            }
            return _unspecifiedTypes;
        }

        /// <summary>
        /// Check whether the structure has an element at the given index, optionally
        /// satisfying given tag expectation.
        /// </summary>
        /// <param name="idx">        $idx         Index 0..n</param>
        /// <param name="expectedTag">$expectedTag Optional type tag expectation</param>
        /// <returns></returns>
        public bool Has(int idx, int? expectedTag = null)
        {
            if (_elements.Count >= idx || _elements[idx] == null)
            {
                return false;
            }
            if (expectedTag != null)
            {
                if (!_elements[idx].IsType((int)expectedTag)){
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Get the element at the given index, optionally checking that the element
        /// has a given tag.
        /// </summary>
        /// <param name="idx">$idx Index 0..n</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public UnspecifiedType At(int idx)
        {
            if (idx >= _elements.Count || _elements[idx] == null)
            {
                throw new Exception($"Structure doesn't have an element at index {idx}.");
            }
            return new UnspecifiedType(_elements[idx]);
        }

        /// <summary>
        /// Check whether the structure contains a context specific element with a
        /// given tag.
        /// </summary>
        /// <param name="tag">Tag number</param>
        /// <returns></returns>
        public bool HasTagged(int tag)
        {
            if (_taggedMap == null)
            {
                _taggedMap = new List<TaggedType>();
                _elements.ForEach(element => {
                    if (element.IsTagged())
                    {
                        _taggedMap[element.Tag()] = (TaggedType)element;
                    }
                });
            }
            return _taggedMap[tag] != null;
        }

        /// <summary>
        /// Get a context specific element tagged with a given tag.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        /// <exception cref="Exception">If tag doesn't exists</exception>
        public TaggedType GetTagged(int tag) { 
            if (!HasTagged(tag))
            {
                throw new Exception($"No tagged element for tag {tag}.");
            }
            return _taggedMap[tag];
        }

        /// <summary>
        /// Get count of elements
        /// </summary>
        /// <returns></returns>
        public int Count() => _elements.Count;

        //public void Add(Element item)
        //{
        //    _elements.Add(item);
        //}

        //public void Clear()
        //{
        //    _elements.Clear();
        //}

        //public bool Contains(Element item)
        //{
        //    return _elements.Contains(item);
        //}

        //public void CopyTo(Element[] array, int arrayIndex)
        //{
        //    _elements.CopyTo(array, arrayIndex);
        //}

        public IEnumerator<Element> GetEnumerator()
        {
            return _elements.GetEnumerator();
        }

        //public bool Remove(Element item)
        //{
        //    return _elements.Remove(item);
        //}

        //public function getIterator(): \ArrayIterator
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _elements.GetEnumerator();
        }


    }
}
