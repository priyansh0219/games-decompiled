using System;
using UnityEngine;

[Serializable]
public struct WeakAssetReference
{
	[SerializeField]
	private string guid;

	[SerializeField]
	private string path;

	public bool IsValid => !string.IsNullOrEmpty(guid);

	public string Path => path;

	public static bool IsValidAssetPath(string assetPath)
	{
		return !string.IsNullOrEmpty(assetPath);
	}

	public static string AssetPathToResourcePath(string assetPath)
	{
		return assetPath;
	}

	internal WeakAssetReference(string guid, string path)
	{
		this.guid = guid;
		this.path = path;
	}

	public T Load<T>() where T : UnityEngine.Object
	{
		return Resources.Load<T>(Path);
	}
}
