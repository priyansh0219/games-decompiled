using UnityEngine;

[AddComponentMenu("")]
public class StoreInformationIdentifier : UniqueIdentifier
{
	public override bool ShouldSerialize(Component comp)
	{
		return true;
	}

	public override bool ShouldCreateEmptyObject()
	{
		return true;
	}

	public override bool ShouldMergeObject()
	{
		return false;
	}

	public override bool ShouldOverridePrefab()
	{
		return true;
	}

	public override bool ShouldStoreClassId()
	{
		return false;
	}
}
