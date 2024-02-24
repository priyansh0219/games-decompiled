using System.Collections;
using System.Collections.Generic;

namespace AhoCorasick
{
	public class Trie : Trie<string>
	{
		public static Trie Create(IEnumerable<string> strings)
		{
			Trie trie = new Trie();
			trie.Add(strings);
			trie.Build();
			return trie;
		}

		public void Add(string s)
		{
			Add(s, s);
		}

		public void Add(IEnumerable<string> strings)
		{
			foreach (string @string in strings)
			{
				Add(@string);
			}
		}
	}
	public class Trie<TValue> : Trie<char, TValue>
	{
	}
	public class Trie<T, TValue>
	{
		private class Node<TNode, TNodeValue> : IEnumerable<Node<TNode, TNodeValue>>, IEnumerable
		{
			private readonly TNode word;

			private readonly Node<TNode, TNodeValue> parent;

			private readonly Dictionary<TNode, Node<TNode, TNodeValue>> children = new Dictionary<TNode, Node<TNode, TNodeValue>>();

			private readonly List<TNodeValue> values = new List<TNodeValue>();

			public TNode Word => word;

			public Node<TNode, TNodeValue> Parent => parent;

			public Node<TNode, TNodeValue> Fail { get; set; }

			public Node<TNode, TNodeValue> this[TNode c]
			{
				get
				{
					if (!children.ContainsKey(c))
					{
						return null;
					}
					return children[c];
				}
				set
				{
					children[c] = value;
				}
			}

			public List<TNodeValue> Values => values;

			public Node()
			{
			}

			public Node(TNode word, Node<TNode, TNodeValue> parent)
			{
				this.word = word;
				this.parent = parent;
			}

			public IEnumerator<Node<TNode, TNodeValue>> GetEnumerator()
			{
				return children.Values.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public override string ToString()
			{
				return Word.ToString();
			}
		}

		private readonly Node<T, TValue> root = new Node<T, TValue>();

		public void Add(IEnumerable<T> word, TValue value)
		{
			Node<T, TValue> node = root;
			foreach (T item in word)
			{
				Node<T, TValue> node2 = node[item];
				if (node2 == null)
				{
					Node<T, TValue> node4 = (node[item] = new Node<T, TValue>(item, node));
					node2 = node4;
				}
				node = node2;
			}
			node.Values.Add(value);
		}

		public void Build()
		{
			Queue<Node<T, TValue>> queue = new Queue<Node<T, TValue>>();
			queue.Enqueue(root);
			while (queue.Count > 0)
			{
				Node<T, TValue> node = queue.Dequeue();
				foreach (Node<T, TValue> item in node)
				{
					queue.Enqueue(item);
				}
				if (node == root)
				{
					root.Fail = root;
					continue;
				}
				Node<T, TValue> fail = node.Parent.Fail;
				while (fail[node.Word] == null && fail != root)
				{
					fail = fail.Fail;
				}
				node.Fail = fail[node.Word] ?? root;
				if (node.Fail == node)
				{
					node.Fail = root;
				}
			}
		}

		public IEnumerable<TValue> Find(IEnumerable<T> text)
		{
			Node<T, TValue> node = root;
			foreach (T item in text)
			{
				while (node[item] == null && node != root)
				{
					node = node.Fail;
				}
				node = node[item] ?? root;
				for (Node<T, TValue> t = node; t != root; t = t.Fail)
				{
					foreach (TValue value in t.Values)
					{
						yield return value;
					}
				}
			}
		}
	}
}
