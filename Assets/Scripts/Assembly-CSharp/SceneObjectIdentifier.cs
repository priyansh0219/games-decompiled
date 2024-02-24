using System.Collections;
using UnityEngine;

public class SceneObjectIdentifier : UniqueIdentifier
{
	public string uniqueName;

	public bool serializeObjectTree;

	public override bool ShouldSerialize(Component comp)
	{
		return true;
	}

	public override bool ShouldCreateEmptyObject()
	{
		return false;
	}

	public override bool ShouldMergeObject()
	{
		return false;
	}

	public override bool ShouldOverridePrefab()
	{
		return false;
	}

	public override bool ShouldStoreClassId()
	{
		return false;
	}

	private IEnumerator Start()
	{
		yield return SceneObjectManager.Instance.RegisterAsync(this);
	}
}
