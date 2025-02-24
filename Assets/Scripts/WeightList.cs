using System;
using System.Collections.Generic;

namespace BEGroup.Utility
{
    /// <summary>
    /// Represents a weight list that each element has its own weight.
    /// </summary>
    /// <typeparam name="T">Type of element.</typeparam>
    public class WeightList<T>
    {
        #region Fields

        private List<KeyValuePair<T, uint>> _list = new List<KeyValuePair<T, uint>>();
        private uint _total = 0;

        #endregion

        #region Methods

        /// <summary>
        /// Adds an element and its weight into the chance table.
        /// </summary>
        public void Add(T element, uint weight)
        {
            _list.Add(new KeyValuePair<T, uint>(element, weight));
            _total += weight;
        }

        /// <summary>
        /// Removes specified element.
        /// </summary>
        /// <param name="index">Index of element.</param>
        public void Remove(int index)
        {
            _total -= _list[index].Value;
            _list.RemoveAt(index);
        }

        /// <summary>
        /// Gets random element based on their weights.
        /// </summary>
        /// <returns></returns>
        public T GetRandomElement()
        {
            return GetRandomElement(false);
        }

        /// <summary>
        /// Gets random element based on their weights.
        /// </summary>
        /// <param name="removeFromTable">If true, the randomly chosen element will be removed from the table.</param>
        /// <returns></returns>
        public T GetRandomElement(bool removeFromTable)
        {
            uint sum = 0;

            Random rnd = new Random();
            uint chance = (uint)(rnd.NextDouble() * _total);

            for (int i = 0; i < _list.Count; i++)
            {
                sum += _list[i].Value;
                if (chance <= sum)
                {
                    T result = _list[i].Key;
                    if (removeFromTable) this.Remove(i);
                    return result;
                }
            }

            return default(T);
        }

        /// <summary>
        /// Gets random element based on their weights.
        /// </summary>
        /// <param name="randomSeed">Seed of random generator.</param>
        /// <returns></returns>
        public T GetRandomElement(int randomSeed)
        {
            return GetRandomElement(randomSeed, false);
        }

        /// <summary>
        /// Gets random element based on their weights.
        /// </summary>
        /// <param name="randomSeed">Seed of random generator.</param>
        /// <param name="removeFromTable">If true, the randomly chosen element will be removed from the table.</param>
        /// <returns></returns>
        public T GetRandomElement(int randomSeed, bool removeFromTable)
        {
            uint sum = 0;

            Random rnd = new Random(randomSeed);
            uint chance = (uint)(rnd.NextDouble() * _total);

            for (int i = 0; i < _list.Count; i++)
            {
                sum += _list[i].Value;
                if (chance <= sum)
                {
                    T result = _list[i].Key;
                    if (removeFromTable) this.Remove(i);
                    return result;
                }
            }

            return default(T);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the count of elements in this weight list.
        /// </summary>
        public int Count
        {
            get
            {
                return _list.Count;
            }
        }

        #endregion
    }
}
