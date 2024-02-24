using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Gendarme;
using UnityEngine;

[SuppressMessage("Subnautica.Rules", "ValueTypeEnumeratorRule")]
public class TreeNode : IEnumerable<TreeNode>, IEnumerable
{
	private static StringBuilder sb = new StringBuilder();

	private static List<string> sPathStrings = new List<string>();

	protected List<TreeNode> nodes = new List<TreeNode>();

	public string id { get; protected set; }

	public TreeNode parent { get; protected set; }

	public int depth
	{
		get
		{
			int num = 0;
			for (TreeNode treeNode = parent; treeNode != null; treeNode = treeNode.parent)
			{
				num++;
			}
			return num;
		}
	}

	public bool isRoot => parent == null;

	public TreeNode root
	{
		get
		{
			TreeNode treeNode = this;
			while (treeNode.parent != null)
			{
				treeNode = treeNode.parent;
			}
			return treeNode;
		}
	}

	public TreeNode topmost
	{
		get
		{
			TreeNode treeNode = this;
			while (treeNode.parent != null && !treeNode.parent.isRoot)
			{
				treeNode = treeNode.parent;
			}
			return treeNode;
		}
	}

	public int childCount => nodes.Count;

	public int siblingCount
	{
		get
		{
			if (parent != null)
			{
				return parent.childCount;
			}
			return 0;
		}
	}

	public TreeNode this[string id]
	{
		get
		{
			int i = 0;
			for (int count = nodes.Count; i < count; i++)
			{
				TreeNode treeNode = nodes[i];
				if (treeNode != null && treeNode.id == id)
				{
					return treeNode;
				}
			}
			return null;
		}
		set
		{
			int i = 0;
			for (int count = nodes.Count; i < count; i++)
			{
				TreeNode treeNode = nodes[i];
				if (treeNode != null && treeNode.id == id)
				{
					nodes[i] = value;
					break;
				}
			}
		}
	}

	public TreeNode this[int index]
	{
		get
		{
			if (index < 0 || index >= nodes.Count)
			{
				return null;
			}
			return nodes[index];
		}
	}

	public TreeNode(string id)
	{
		this.id = id;
	}

	public TreeNode AddNode(TreeNode node)
	{
		if (node != null)
		{
			if (this[node.id] == null)
			{
				nodes.Add(node);
				node.SetParent(this);
			}
			else
			{
				Debug.LogErrorFormat("Prevented attempt to add node with duplicate id '{0}'. Identifiers should be unique!", node.id);
			}
		}
		return this;
	}

	public TreeNode AddNode(params TreeNode[] n)
	{
		if (n != null)
		{
			for (int i = 0; i < n.Length; i++)
			{
				AddNode(n[i]);
			}
		}
		return this;
	}

	public bool RemoveNode(TreeNode node)
	{
		int num = nodes.IndexOf(node);
		if (num < 0)
		{
			return false;
		}
		node.OnDestroy();
		nodes.RemoveAt(num);
		return true;
	}

	protected void SetParent(TreeNode parent)
	{
		this.parent = parent;
	}

	public virtual void OnDestroy()
	{
		SetParent(null);
	}

	public IEnumerator<TreeNode> GetEnumerator()
	{
		return nodes.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return nodes.GetEnumerator();
	}

	public IEnumerator<TreeNode> Traverse(bool includeSelf = true)
	{
		if (includeSelf)
		{
			yield return this;
		}
		using (IEnumerator<TreeNode> e = GetEnumerator())
		{
			while (e.MoveNext())
			{
				TreeNode current = e.Current;
				using (IEnumerator<TreeNode> e2 = current.Traverse())
				{
					while (e2.MoveNext())
					{
						yield return e2.Current;
					}
				}
			}
		}
	}

	public bool FindNodeById(string id, List<TreeNode> path, bool ignoreCase = false)
	{
		if (string.Equals(this.id, id, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
		{
			path.Add(this);
			return true;
		}
		for (int i = 0; i < nodes.Count; i++)
		{
			if (nodes[i].FindNodeById(id, path, ignoreCase))
			{
				path.Add(this);
				return true;
			}
		}
		return false;
	}

	public TreeNode FindNodeById(string id, bool ignoreCase = false)
	{
		if (string.Equals(this.id, id, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
		{
			return this;
		}
		for (int i = 0; i < nodes.Count; i++)
		{
			TreeNode treeNode = nodes[i].FindNodeById(id, ignoreCase);
			if (treeNode != null)
			{
				return treeNode;
			}
		}
		return null;
	}

	public TreeNode FindNodeByPath(params string[] path)
	{
		int num = path.Length;
		if (num <= 0)
		{
			return null;
		}
		TreeNode treeNode = this;
		for (int i = 0; i < num; i++)
		{
			treeNode = treeNode[path[i]];
			if (treeNode == null)
			{
				return null;
			}
		}
		return treeNode;
	}

	public int IndexOf(TreeNode node)
	{
		return nodes.IndexOf(node);
	}

	public virtual TreeNode Copy()
	{
		return new TreeNode(id);
	}

	public void Sort(IComparer<TreeNode> comparer)
	{
		nodes.Sort(comparer);
	}

	public void Clear()
	{
		for (int num = nodes.Count - 1; num >= 0; num--)
		{
			nodes[num]?.OnDestroy();
			nodes.RemoveAt(num);
		}
	}

	public List<TreeNode> GetReversedPath(bool includeSelf)
	{
		List<TreeNode> list = new List<TreeNode>();
		TreeNode treeNode = (includeSelf ? this : parent);
		while (treeNode != null && !treeNode.isRoot)
		{
			list.Add(treeNode);
			treeNode = treeNode.parent;
		}
		return list;
	}

	public string GetPathString(char separatorChar, bool includeSelf, string prefix = null, string postfix = null)
	{
		List<TreeNode> reversedPath = GetReversedPath(includeSelf);
		StringBuilder stringBuilder = new StringBuilder(prefix);
		bool flag = !string.IsNullOrEmpty(postfix);
		if (reversedPath.Count > 0)
		{
			for (int num = reversedPath.Count - 1; num >= 0; num--)
			{
				TreeNode treeNode = reversedPath[num];
				stringBuilder.Append(treeNode.id);
				if (flag || num > 0)
				{
					stringBuilder.Append(separatorChar);
				}
			}
		}
		if (flag)
		{
			stringBuilder.Append(postfix);
		}
		return stringBuilder.ToString();
	}

	public override string ToString()
	{
		LogTree(this);
		string result = sb.ToString();
		sb.Length = 0;
		sPathStrings.Clear();
		return result;
	}

	protected void LogTree(TreeNode node)
	{
		sb.AppendFormat("{0}\n", node.id);
		int i = 0;
		for (int num = node.childCount; i < num; i++)
		{
			TreeNode node2 = node[i];
			for (int j = 0; j < sPathStrings.Count; j++)
			{
				sb.Append(sPathStrings[j]);
			}
			if (i < num - 1)
			{
				sb.Append("├──");
				sPathStrings.Add("│  ");
			}
			else
			{
				sb.Append("└──");
				sPathStrings.Add("   ");
			}
			LogTree(node2);
			sPathStrings.RemoveAt(sPathStrings.Count - 1);
		}
	}
}
