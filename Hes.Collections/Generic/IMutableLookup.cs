namespace Hes.Collections.Generic {

    using System.Linq;

    public interface IMutableLookup<TKey, TElement> : ILookup<TKey, TElement> {

        /// <summary>
        /// Adds the element to given key.
        /// </summary>
        /// <param name="key">The key to use.</param>
        /// <param name="element">The element to add.</param>
        void Add(TKey key, TElement element);

        /// <summary>
        /// Removes an element from the given key.
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="element">The element.</param>
        /// <returns>true if successfully removed.</returns>
        bool Remove(TKey key, TElement element);

        /// <summary>
        /// Clears all elements.
        /// </summary>
        void Clear();
    }
}