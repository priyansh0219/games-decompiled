using System;

[AttributeUsage(AttributeTargets.Field)]
public sealed class WeakAssetReferenceTypeAttribute : Attribute
{
	public readonly Type assetType;

	public WeakAssetReferenceTypeAttribute(Type assetType)
	{
		this.assetType = assetType;
	}
}
