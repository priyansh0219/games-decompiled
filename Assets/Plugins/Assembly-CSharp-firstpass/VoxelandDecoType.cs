using System;
using UnityEngine;

[Serializable]
public class VoxelandDecoType : MonoBehaviour
{
	public class VoxelandDecoSettings : VoxelandTypeBase
	{
		public Mesh mesh;

		public Material material;
	}

	public VoxelandDecoSettings settings;

	public void CopyFrom(VoxelandBlockType bt)
	{
		settings.mesh = bt.grassMesh;
		settings.material = bt.grassMaterial;
		VoxelandTypeBase.Copy(bt, settings);
	}

	public void CopyInto(VoxelandBlockType bt)
	{
		bt.grassMesh = settings.mesh;
		bt.grassMaterial = settings.material;
		VoxelandTypeBase.Copy(settings, bt);
	}

	public bool IsVisuallySame(VoxelandBlockType other)
	{
		if (settings.mesh == other.grassMesh && settings.material == other.grassMaterial)
		{
			return VoxelandTypeBase.ApproxEqual(settings, other);
		}
		return false;
	}
}
