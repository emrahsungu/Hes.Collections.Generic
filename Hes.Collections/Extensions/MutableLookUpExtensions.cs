namespace Hes.Collections.Extensions {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Generic;

    public static class MutableLookupExtensions {

        /// <summary>
        /// Creates a <see cref="T:Hes.Collections.Generic.MutableLookup`2" /> from an
        /// <see cref="T:System.Collections.Generic.IEnumerable`1" /> according to a specified
        /// key selector function, a comparer, and an element selector function.
        /// </summary>
        /// <param name="source">
        /// An <see cref="T:System.Collections.Generic.IEnumerable`1" /> to create a <see cref="T:Hes.Collections.Generic.MutableLookup`2" /> from.
        /// </param>
        /// <param name="keySelector"> A function to extract a key from each element. </param>
        /// <param name="elementSelector"> A transform function to produce a result element value from each element. </param>
        /// <param name="comparer"> An <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> to compare keys. </param>
        /// <typeparam name="TSource"> The type of the elements of <paramref name="source" />. </typeparam>
        /// <typeparam name="TKey"> The type of the key returned by <paramref name="keySelector" />. </typeparam>
        /// <typeparam name="TElement"> The type of the value returned by <paramref name="elementSelector" />. </typeparam>
        /// <returns>
        /// A <see cref="T:Hes.Collections.Generic.Generic.MutableLookup`2" />
        /// that contains values of type <paramref name="TElement" /> selected from the input sequence.
        /// </returns>
        public static IMutableLookup<TKey, TElement> Create<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer) {
            if (source == null) {
                throw new ArgumentNullException(nameof(source));
            }

            if (keySelector == null) {
                throw new ArgumentNullException(nameof(keySelector));
            }

            if (elementSelector == null) {
                throw new ArgumentNullException(nameof(elementSelector));
            }

            return source.Aggregate(new MutableLookup<TKey, TElement>(comparer), (IMutableLookup<TKey, TElement> lookup, TSource element) => {
                lookup.Add(keySelector(element), elementSelector(element));
                return lookup;
            });
        }
    }
}