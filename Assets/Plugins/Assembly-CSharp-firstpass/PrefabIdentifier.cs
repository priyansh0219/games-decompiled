using UnityEngine;

public class PrefabIdentifier : UniqueIdentifier
{
	private string prefabKey;

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
		return false;
	}

	public override bool ShouldOverridePrefab()
	{
		return true;
	}

	public override bool ShouldStoreClassId()
	{
		return true;
	}

	protected override void EnsureOnDestroy()
	{
		base.EnsureOnDestroy();
		if (prefabKey != null)
		{
			AddressablesUtility.DecreaseRefCount(prefabKey);
		}
	}

	public void SetPrefabKey(string prefabKey)
	{
		if (prefabKey != null)
		{
			this.prefabKey = prefabKey;
			AddressablesUtility.IncreaseRefCount(prefabKey);
		}
	}
}
