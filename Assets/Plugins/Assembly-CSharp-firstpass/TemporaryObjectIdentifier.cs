using UWE;
using UnityEngine;

[AddComponentMenu("")]
public class TemporaryObjectIdentifier : UniqueIdentifier
{
	private void Start()
	{
		Utils.DestroyWrap(base.gameObject);
	}

	public override bool ShouldSerialize(Component comp)
	{
		return false;
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
