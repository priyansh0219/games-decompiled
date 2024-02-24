using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UWE
{
	public class FixedSizeCache<K, V> : IDictionary<K, V>, ICollection<KeyValuePair<K, V>>, IEnumerable<KeyValuePair<K, V>>, IEnumerable
	{
		private int capacity;

		private readonly LinkedList<KeyValuePair<K, V>> queue = new LinkedList<KeyValuePair<K, V>>();

		private readonly Dictionary<K, LinkedListNode<KeyValuePair<K, V>>> entries = new Dictionary<K, LinkedListNode<KeyValuePair<K, V>>>();

		public V this[K key]
		{
			get
			{
				TryGetValue(key, out var value);
				return value;
			}
			set
			{
				Remove(key);
				Add(key, value);
			}
		}

		ICollection<K> IDictionary<K, V>.Keys => entries.Keys;

		ICollection<V> IDictionary<K, V>.Values => queue.Select((KeyValuePair<K, V> p) => p.Value).ToList();

		public int Count => entries.Count;

		bool ICollection<KeyValuePair<K, V>>.IsReadOnly => false;

		public FixedSizeCache(int capacity)
		{
			if (capacity <= 0)
			{
				throw new ArgumentOutOfRangeException("capacity", "Capacity must be strictly positive");
			}
			this.capacity = capacity;
		}

		public void SetCapacity(int capacity)
		{
			if (capacity <= 0)
			{
				throw new ArgumentOutOfRangeException("capacity", "Capacity must be strictly positive");
			}
			this.capacity = capacity;
			EnsureCapacity();
		}

		private void EnsureCapacity()
		{
			while (queue.Count > capacity)
			{
				Remove(queue.Last);
			}
		}

		private void Remove(LinkedListNode<KeyValuePair<K, V>> node)
		{
			entries.Remove(node.Value.Key);
			queue.Remove(node);
		}

		public bool ContainsKey(K key)
		{
			return entries.ContainsKey(key);
		}

		public void Add(K key, V value)
		{
			LinkedListNode<KeyValuePair<K, V>> linkedListNode = new LinkedListNode<KeyValuePair<K, V>>(new KeyValuePair<K, V>(key, value));
			entries.Add(key, linkedListNode);
			queue.AddFirst(linkedListNode);
			EnsureCapacity();
		}

		public bool Remove(K key)
		{
			if (entries.TryGetValue(key, out var value))
			{
				Remove(value);
				return true;
			}
			return false;
		}

		public bool TryGetValue(K key, out V value)
		{
			if (entries.TryGetValue(key, out var value2))
			{
				queue.Remove(value2);
				queue.AddFirst(value2);
				value = value2.Value.Value;
				return true;
			}
			value = default(V);
			return false;
		}

		void ICollection<KeyValuePair<K, V>>.Add(KeyValuePair<K, V> item)
		{
			Add(item.Key, item.Value);
		}

		public void Clear()
		{
			queue.Clear();
			entries.Clear();
		}

		bool ICollection<KeyValuePair<K, V>>.Contains(KeyValuePair<K, V> item)
		{
			return ContainsKey(item.Key);
		}

		void ICollection<KeyValuePair<K, V>>.CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
		{
			queue.CopyTo(array, arrayIndex);
		}

		bool ICollection<KeyValuePair<K, V>>.Remove(KeyValuePair<K, V> item)
		{
			return Remove(item.Key);
		}

		public LinkedList<KeyValuePair<K, V>>.Enumerator GetEnumerator()
		{
			return queue.GetEnumerator();
		}

		IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K, V>>.GetEnumerator()
		{
			return ((IEnumerable<KeyValuePair<K, V>>)queue).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)queue).GetEnumerator();
		}
	}
}
