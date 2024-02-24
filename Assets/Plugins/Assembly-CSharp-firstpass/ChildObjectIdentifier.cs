using UnityEngine;

public class ChildObjectIdentifier : UniqueIdentifier, ICompileTimeCheckable
{
	public override bool ShouldSerialize(Component comp)
	{
		if (comp is Collider)
		{
			return false;
		}
		if (comp is Light)
		{
			return false;
		}
		return true;
	}

	public override bool ShouldCreateEmptyObject()
	{
		return false;
	}

	public override bool ShouldMergeObject()
	{
		return true;
	}

	public override bool ShouldOverridePrefab()
	{
		return true;
	}

	public override bool ShouldStoreClassId()
	{
		return true;
	}

	public string CompileTimeCheck()
	{
		Transform parent = base.transform.parent;
		if (!parent)
		{
			return null;
		}
		if (!parent.GetComponent<UniqueIdentifier>())
		{
			return $"ChildObjectIdentifier {base.name}'s parent {parent.name} has no unique identifier.";
		}
		return null;
	}
}
