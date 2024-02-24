using System;
using UnityEngine.AddressableAssets;

[Serializable]
public class AssetReferenceDistanceField : AssetReferenceT<DistanceField>
{
	public AssetReferenceDistanceField(string guid)
		: base(guid)
	{
	}
}
