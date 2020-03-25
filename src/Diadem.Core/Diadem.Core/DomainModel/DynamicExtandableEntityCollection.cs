using System;
using System.Collections;
using System.Collections.Generic;

namespace Diadem.Core.DomainModel
{
    public class DynamicExtandableEntityCollection : IList<ExtendableEntity>
    {
        private readonly IList<ExtendableEntity> _list;

        public DynamicExtandableEntityCollection()
        {
            _list = new List<ExtendableEntity>();
        }

        public IEnumerator<ExtendableEntity> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public void Add(ExtendableEntity item)
        {
            _list.Add(item);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(ExtendableEntity item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(ExtendableEntity[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(ExtendableEntity item)
        {
            return _list.Remove(item);
        }

        public int Count => _list.Count;

        public bool IsReadOnly => _list.IsReadOnly;

        public int IndexOf(ExtendableEntity item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, ExtendableEntity item)
        {
            _list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        public ExtendableEntity this[int index]
        {
            get => _list[index];
            set => _list[index] = value;
        }
    }
}