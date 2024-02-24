using System.Collections.Generic;

namespace UWE
{
	public class LRUQueue<T> : ILRUQueue<T>
	{
		private readonly LinkedList<T> orderedList = new LinkedList<T>();

		private readonly Dictionary<T, LinkedListNode<T>> nodeMap = new Dictionary<T, LinkedListNode<T>>();

		private readonly Stack<LinkedListNode<T>> nodePool = new Stack<LinkedListNode<T>>();

		private readonly T nullEntry;

		public int Count => orderedList.Count;

		public void Clear()
		{
			orderedList.Clear();
			nodeMap.Clear();
			nodePool.Clear();
		}

		public void PushBack(T keyElement)
		{
			if (!nodeMap.TryGetValue(keyElement, out var value))
			{
				value = GetNodeFromPool();
				value.Value = keyElement;
				nodeMap.Add(keyElement, value);
			}
			else
			{
				orderedList.Remove(value);
			}
			orderedList.AddLast(value);
		}

		public T Pop()
		{
			LinkedListNode<T> first = orderedList.First;
			orderedList.Remove(first);
			nodeMap.Remove(first.Value);
			T value = first.Value;
			ReturnNodeToPool(first);
			return value;
		}

		public T Peek()
		{
			return orderedList.First.Value;
		}

		public void SwapElements(T a, T b)
		{
			LinkedListNode<T> linkedListNode = nodeMap[a];
			LinkedListNode<T> linkedListNode2 = nodeMap[b];
			linkedListNode.Value = b;
			linkedListNode2.Value = a;
			nodeMap[a] = linkedListNode2;
			nodeMap[b] = linkedListNode;
		}

		public void RemoveElement(T element)
		{
			if (nodeMap.TryGetValue(element, out var value))
			{
				orderedList.Remove(value);
				nodeMap.Remove(element);
				ReturnNodeToPool(value);
			}
		}

		private LinkedListNode<T> GetNodeFromPool()
		{
			if (nodePool.Count <= 0)
			{
				return new LinkedListNode<T>(nullEntry);
			}
			return nodePool.Pop();
		}

		private void ReturnNodeToPool(LinkedListNode<T> node)
		{
			node.Value = nullEntry;
			nodePool.Push(node);
		}
	}
}
