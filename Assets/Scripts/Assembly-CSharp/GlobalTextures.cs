using System;
using UnityEngine;

public class GlobalTextures : MonoBehaviour
{
	[Serializable]
	public struct Entry
	{
		public string name;

		public Texture texture;
	}

	public Entry[] entries;

	public Vector4 unscaledTime;

	private void Awake()
	{
		for (int i = 0; i < entries.Length; i++)
		{
			Entry entry = entries[i];
			if (!string.IsNullOrEmpty(entry.name) && !(entry.texture == null))
			{
				Shader.SetGlobalTexture(Shader.PropertyToID(entry.name), entry.texture);
			}
		}
	}

	private void Update()
	{
		unscaledTime.Set(Time.unscaledTime, 0f, 0f, 0f);
		Shader.SetGlobalVector(ShaderPropertyID._UnscaledTime, unscaledTime);
	}
}
