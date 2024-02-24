using System.Collections;
using System.Collections.Generic;
using Gendarme;
using UnityEngine;

[SuppressMessage("Subnautica.Rules", "ValueTypeEnumeratorRule")]
public class CraftNode : TreeNode, IEnumerable<CraftNode>, IEnumerable
{
	public delegate bool Filter(CraftNode sample);

	public TechType techType0;

	public string string0;

	public string string1;

	public MonoBehaviour monoBehaviour0;

	public bool bool0;

	public int int0;

	public TreeAction action { get; private set; }

	public CraftNode(string id, TreeAction action = TreeAction.None, TechType techType = TechType.None)
		: base(id)
	{
		this.action = action;
		techType0 = techType;
	}

	[SuppressMessage("Gendarme.Rules.Maintainability", "AvoidUnnecessarySpecializationRule")]
	public CraftNode AddNode(CraftNode n)
	{
		AddNode((TreeNode)n);
		return this;
	}

	public CraftNode AddNode(params CraftNode[] n)
	{
		AddNode((TreeNode[])n);
		return this;
	}

	public new IEnumerator<CraftNode> GetEnumerator()
	{
		IEnumerator<TreeNode> e = nodes.GetEnumerator();
		while (e.MoveNext())
		{
			yield return (CraftNode)e.Current;
		}
	}

	[SuppressMessage("Subnautica.Rules", "AvoidHidingMethodsRule")]
	public new IEnumerator<CraftNode> Traverse(bool includeSelf = true)
	{
		IEnumerator<TreeNode> e = base.Traverse(includeSelf);
		while (e.MoveNext())
		{
			yield return (CraftNode)e.Current;
		}
	}

	public override TreeNode Copy()
	{
		CraftNode craftNode = new CraftNode(base.id, action, techType0);
		Copy(this, craftNode);
		return craftNode;
	}

	public static void Copy(CraftNode src, CraftNode dst)
	{
		dst.string0 = src.string0;
		dst.string1 = src.string1;
		dst.monoBehaviour0 = src.monoBehaviour0;
		dst.bool0 = src.bool0;
		dst.int0 = src.int0;
	}
}
