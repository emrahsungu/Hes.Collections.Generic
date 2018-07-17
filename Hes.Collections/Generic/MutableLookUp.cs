namespace Hes.Collections.Generic {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class MutableLookup<TKey, TElement> : IMutableLookup<TKey, TElement> {

        /// <summary>
        /// Comparer to compare items.
        /// </summary>
        private readonly IEqualityComparer<TKey> _comparer;

        /// <summary>
        /// Backing field to hold the last mutable grouping.
        /// </summary>
        private MutableGrouping<TKey, TElement> _lastMutableGrouping;

        /// <summary>
        /// Array to hold all the mutable groupings.
        /// </summary>
        private MutableGrouping<TKey, TElement>[] _mutableGroupings;

        /// <summary>
        /// Creates a mutable lookup table, with the default comparer.
        /// </summary>
        public MutableLookup() : this(null) {
        }

        /// <summary>
        /// Creates a mutable lookup table with the given comparer.
        /// </summary>
        /// <param name="comparer">Comparer to compare items.</param>
        public MutableLookup(IEqualityComparer<TKey> comparer) {
            if (comparer == null) {
                comparer = EqualityComparer<TKey>.Default;
            }
            _comparer = comparer;
            _mutableGroupings = new MutableGrouping<TKey, TElement>[7];
        }

        /// <summary>
        /// The number of elements in the lookup table.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// The elements with the given key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>All the elements with the given key.</returns>
        public IEnumerable<TElement> this[TKey key] => GetGrouping(key, false) ?? Enumerable.Empty<TElement>();

        /// <summary>
        /// Gets a value indicating whether the given key is contained.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>true if the key is contained.</returns>
        public bool Contains(TKey key) => GetGrouping(key, false) != null;

        /// <summary>
        /// Adds the element to given key.
        /// </summary>
        /// <param name="key">The key to use.</param>
        /// <param name="element">The element to add.</param>
        public void Add(TKey key, TElement element) {
            GetGrouping(key, true).Add(element);
        }

        /// <summary>
        /// Removes an element from the given key.
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="element">The element.</param>
        /// <returns>true if successfully removed.</returns>
        public bool Remove(TKey key, TElement element) {
            return GetGrouping(key, false)?.Remove(element) ?? false;
        }

        public void Clear() {
            Array.Clear(_mutableGroupings, 0, _mutableGroupings.Length);
            _lastMutableGrouping = null;
            Count = 0;
        }

        /// <summary>
        /// Returns an enumerator to enumerate the collection.
        /// </summary>
        /// <returns>Enumerator.</returns>
        public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator() {
            var g = _lastMutableGrouping;
            if (g == null) {
                yield break;
            }
            do {
                g = g.Next;
                yield return g;
            }
            while (g != _lastMutableGrouping);
        }

        /// <summary>
        /// Returns an enumerator to enumerate the collection.
        /// </summary>
        /// <returns>Enumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets hash code with the given key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>Hashcode of the key.</returns>
        private int InternalGetHashCode(TKey key) {
            if ((object)key != null) {
                return _comparer.GetHashCode(key) & int.MaxValue;
            }
            return 0;
        }

        /// <summary>
        /// Gets the grouping.
        /// </summary>
        /// <param name="key">The key for the grouping.</param>
        /// <param name="create">Option to create if the key does not exist.</param>
        /// <returns></returns>
        private IMutableGrouping<TKey, TElement> GetGrouping(TKey key, bool create) {
            var hashCode = InternalGetHashCode(key);
            for (var grouping = _mutableGroupings[hashCode % _mutableGroupings.Length]; grouping != null; grouping = grouping.HashNext) {
                if (grouping.HashCode == hashCode && _comparer.Equals(grouping.Key, key)) {
                    return grouping;
                }
            }

            if (!create) {
                return null;
            }

            if (Count == _mutableGroupings.Length) {
                Resize();
            }
            var index = hashCode % _mutableGroupings.Length;
            var grouping1 = new MutableGrouping<TKey, TElement>(key, hashCode, _mutableGroupings[index]);
            _mutableGroupings[index] = grouping1;
            if (_lastMutableGrouping == null) {
                grouping1.Next = grouping1;
            }
            else {
                grouping1.Next = _lastMutableGrouping.Next;
                _lastMutableGrouping.Next = grouping1;
            }
            _lastMutableGrouping = grouping1;
            ++Count;
            return grouping1;
        }

        /// <summary>
        /// Resizes the lookuptable to accomodate more items.
        /// </summary>
        private void Resize() {
            var length = checked(Count * 2 + 1);
            var mutableGroupingArray = new MutableGrouping<TKey, TElement>[length];
            var mutableGrouping = _lastMutableGrouping;
            do {
                mutableGrouping = mutableGrouping.Next;
                var index = mutableGrouping.HashCode % length;
                mutableGrouping.HashNext = mutableGroupingArray[index];
                mutableGroupingArray[index] = mutableGrouping;
            }
            while (mutableGrouping != _lastMutableGrouping);
            _mutableGroupings = mutableGroupingArray;
        }

        /// <summary>
        /// Mutable grouping simple etends, IGrouping interface and adds additional methods.
        /// </summary>
        /// <typeparam name="TKey1">Key type.</typeparam>
        /// <typeparam name="TElement1">Element type.</typeparam>
        private class MutableGrouping<TKey1, TElement1> : IMutableGrouping<TKey1, TElement1> {

            /// <summary>
            /// Next hash grouping in the same key.
            /// </summary>
            public MutableGrouping<TKey1, TElement1> HashNext;

            /// <summary>
            /// Next hash grouping with different key.
            /// </summary>
            public MutableGrouping<TKey1, TElement1> Next;

            /// <summary>
            /// Elements for this groupings.
            /// </summary>
            private TElement1[] _elements = new TElement1[1];

            /// <summary>
            /// Creates a mutable groupong with the given key, hashcode and hashNext.
            /// </summary>
            /// <param name="key">The key</param>
            /// <param name="hashCode">Hashcode fo the key.</param>
            /// <param name="hashNext">Hashnext.</param>
            public MutableGrouping(TKey1 key, int hashCode, MutableGrouping<TKey1, TElement1> hashNext) {
                Key = key;
                HashCode = hashCode;
                HashNext = hashNext;
            }

            /// <summary>
            /// Number of elements in this grouping.
            /// </summary>
            public int Count { get; private set; }

            /// <summary>
            /// Hashcode or this grouping. I.e the hashcode of the key.
            /// </summary>
            public int HashCode { get; }

            /// <summary>
            /// The key for this grouping.
            /// </summary>
            public TKey1 Key { get; }

            /// <summary>
            /// Adds the given element to this grouping.
            /// </summary>
            /// <param name="element">Element to add.</param>
            public void Add(TElement1 element) {
                if (_elements.Length == Count) {
                    Array.Resize(ref _elements, checked(Count * 2));
                }
                _elements[Count] = element;
                Count = Count + 1;
            }

            /// <summary>
            /// Clears this grouping.
            /// </summary>
            public void Clear() {
                if (Count <= 0) { return; }
                Array.Clear(_elements, 0, Count);
                Count = 0;
            }

            /// <summary>
            /// Gets a value indicating if this item is contained in this grouping.
            /// </summary>
            /// <param name="item">Item to check</param>
            /// <returns>true if this grouping contains the given item</returns>
            public bool Contains(TElement1 item) {
                return Array.IndexOf(_elements, item, 0, Count) >= 0;
            }

            /// <summary>
            /// Copies the contens of this grouping to the given array starting from the given index.
            /// </summary>
            /// <param name="array">Array to copy to.</param>
            /// <param name="arrayIndex">Starting index.</param>
            public void CopyTo(TElement1[] array, int arrayIndex) {
                Array.Copy(_elements, 0, array, arrayIndex, Count);
            }

            /// <summary>
            /// Returns an enumrator to enumerate the collection.
            /// </summary>
            /// <returns>Enumerator.</returns>
            public IEnumerator<TElement1> GetEnumerator() {
                for (var i = 0; i < Count; ++i) {
                    yield return _elements[i];
                }
            }

            /// <summary>
            /// Returns an enumrator to enumerate the collection.
            /// </summary>
            /// <returns>Enumerator.</returns>
            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

            /// <summary>
            /// Removes the given element from this grouping.
            /// </summary>
            /// <param name="item">Item to remove.</param>
            /// <returns>true if successfully removed.</returns>
            public bool Remove(TElement1 item) {
                var index = IndexOf(item);
                if (index < 0) {
                    return false;
                }
                RemoveAt(index);
                return true;
            }

            /// <summary>
            /// Gets the index of the given item.
            /// </summary>
            /// <param name="item">Item to find index of.</param>
            /// <returns>Index of the given item.</returns>
            private int IndexOf(TElement1 item) {
                return Array.IndexOf(_elements, item, 0, Count);
            }

            /// <summary>
            /// Removes the item at the given index.
            /// </summary>
            /// <param name="index">Index of the item to be removed.</param>
            private void RemoveAt(int index) {
                if ((uint)index >= (uint)Count) {
                    throw new ArgumentOutOfRangeException();
                }
                Count = Count - 1;
                if (index < Count) {
                    Array.Copy(_elements, index + 1, _elements, index, Count - index);
                }
                _elements[Count] = default(TElement1);
            }
        }
    }
}