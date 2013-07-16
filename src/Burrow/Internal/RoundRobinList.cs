using System.Collections.Generic;

namespace Burrow.Internal
{
    /// <summary>
    /// A list of T that can help you get next element in a round-robin style
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class RoundRobinList<T>
    {
        private volatile IList<T> _list;
        private volatile int _index;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="list"></param>
        public RoundRobinList(IEnumerable<T> list)
        {
            _list = new List<T>(list);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public RoundRobinList()
        {
            _list = new List<T>();
        }

        /// <summary>
        /// Return all element
        /// </summary>
        public IEnumerable<T> All
        {
            get { return _list; }
        }

        public void ClearAll()
        {
            _list.Clear();
        }

        /// <summary>
        /// Add 1 element to list
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            lock (_list)
            {
                _list.Add(item);
            }
        }

        /// <summary>
        /// Get current element
        /// </summary>
        public T Current
        {
            get
            {
                lock (_list)
                {
                    return _list[_index];
                }
            }
        }

        /// <summary>
        /// Get next element
        /// </summary>
        /// <returns></returns>
        public T GetNext()
        {
            if (_list.Count == 0)
            {
                return default(T);
            }

            lock (_list)
            {
                _index++;
                if (_index >= _list.Count)
                {
                    _index = 0;
                }

                return _list[_index];
            }
        }
    }
}