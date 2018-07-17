namespace Hes.Collections.Generic {

    using System.Linq;

    public interface IMutableGrouping<out TKey, TElement> : IGrouping<TKey, TElement> {

        /// <summary>
        /// Adds the given element to this grouping.
        /// </summary>
        /// <param name="element">Element to add.</param>
        void Add(TElement element);

        /// <summary>
        /// Removes the given element from this grouping.
        /// </summary>
        /// <param name="element">Item to remove.</param>
        /// <returns>true if successfully removed.</returns>
        bool Remove(TElement element);

        /// <summary>
        /// Gets a value indicating if this item is contained in this grouping.
        /// </summary>
        /// <param name="element">Item to check</param>
        /// <returns>true if this grouping contains the given item</returns>
        bool Contains(TElement element);
    }
}