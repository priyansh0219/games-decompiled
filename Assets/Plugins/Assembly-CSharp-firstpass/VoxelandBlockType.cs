using System;
using UnityEngine;

[Serializable]
public class VoxelandBlockType : VoxelandTypeBase
{
	[NonSerialized]
	public string name;

	public int layer;

	public bool filled;

	public Material material;

	[NonSerialized]
	public Material opaqueMaterial;

	[NonSerialized]
	public Material alphaTestMat;

	public VoxelandDecoType decoOverride;

	public bool hasGrassAbove;

	public Mesh grassMesh;

	public Material grassMaterial;

	[NonSerialized]
	public Vector3[] grassVerts;

	[NonSerialized]
	public Vector3[] grassNormals;

	[NonSerialized]
	public Vector4[] grassTangents;

	[NonSerialized]
	public Vector2[] grassUVs;

	[NonSerialized]
	public int[] grassTris;

	[NonSerialized]
	public string grassMeshName;

	[NonSerialized]
	public float cachedGloss;

	public static VoxelandBlockType FilledMaterial(Material mat, string name)
	{
		return new VoxelandBlockType
		{
			name = name,
			filled = true,
			material = mat,
			hasGrassAbove = false
		};
	}

	public void RuntimeInit(int myNum)
	{
		cachedGloss = 0f;
		if (material != null && material.HasProperty("_Gloss"))
		{
			cachedGloss = material.GetFloat("_Gloss");
		}
		if (material != null)
		{
			opaqueMaterial = new Material(material)
			{
				renderQueue = 1000
			};
			opaqueMaterial.SetInt(ShaderPropertyID._ZWrite, 1);
			opaqueMaterial.SetInt(ShaderPropertyID._ColorMask, 255);
			opaqueMaterial.SetInt(ShaderPropertyID._BlendSrcFactor, 1);
			opaqueMaterial.SetInt(ShaderPropertyID._BlendDstFactor, 0);
			opaqueMaterial.SetInt(ShaderPropertyID._IsOpaque, 1);
			opaqueMaterial.SetFloat(ShaderPropertyID._AlphaTestValue, 0f);
			alphaTestMat = new Material(material);
			alphaTestMat.SetInt(ShaderPropertyID._BlendSrcFactor, 1);
			alphaTestMat.SetInt(ShaderPropertyID._BlendDstFactor, 0);
			alphaTestMat.SetFloat(ShaderPropertyID._AlphaTestValue, 0.5f);
			alphaTestMat.EnableKeyword("ALBEDO_ONLY");
		}
		if (hasGrassAbove && decoOverride != null)
		{
			Debug.LogError("11/11/2014 3:59:07 PM This feature is deprecated. Steve decided it wasn't all that useful and could lead to more confusion.");
			decoOverride.CopyInto(this);
		}
		if (grassMesh != null)
		{
			if (!grassMesh.isReadable)
			{
				Debug.LogError("The mesh named '" + grassMesh.name + "' is being used in the grass/deco system (for block type " + myNum + "), but its mesh is not read-enabled! Check that box in the mesh import settings for it.");
				hasGrassAbove = false;
			}
			else
			{
				grassVerts = grassMesh.vertices;
				grassNormals = grassMesh.normals;
				grassTangents = grassMesh.tangents;
				grassUVs = grassMesh.uv;
				grassTris = grassMesh.triangles;
				grassMeshName = grassMesh.name;
			}
		}
	}

	public bool IsVisuallySame(VoxelandBlockType other)
	{
		return IsVisuallySame(other, ignoreDecoPlacementSettings: false);
	}

	public bool IsVisuallySame(VoxelandBlockType other, bool ignoreDecoPlacementSettings)
	{
		if (filled != other.filled)
		{
			return false;
		}
		if (filled)
		{
			if (material != other.material || hasGrassAbove != other.hasGrassAbove)
			{
				return false;
			}
			if (hasGrassAbove)
			{
				if (grassMaterial == other.grassMaterial && grassMesh == other.grassMesh)
				{
					if (!ignoreDecoPlacementSettings)
					{
						return VoxelandTypeBase.ApproxEqual(this, other);
					}
					return true;
				}
				return false;
			}
			return true;
		}
		return true;
	}

	public override string ToString()
	{
		if (material == null)
		{
			return "<null material>";
		}
		string text = material.name + " ";
		if (hasGrassAbove)
		{
			text = text + "deco(" + ((grassMesh != null) ? grassMesh.name : "<null>") + ", " + ((grassMaterial != null) ? grassMaterial.name : "<null>") + ")";
		}
		return text;
	}

	public override string Check()
	{
		string text = base.Check();
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		if (!filled)
		{
			return null;
		}
		if (!material)
		{
			return $"Block type '{name}' has no material assigned";
		}
		if (!hasGrassAbove)
		{
			return null;
		}
		if (!grassMesh)
		{
			return "Missing grass mesh";
		}
		if (!grassMesh.isReadable)
		{
			return "Grass mesh must be read-enabled";
		}
		if (!grassMaterial)
		{
			return "Missing grass material";
		}
		return null;
	}

	public static bool Equals(VoxelandBlockType a, VoxelandBlockType b, bool ignoreDecoPlacementSettings)
	{
		if (a.grassMaterial == b.grassMaterial && a.grassMesh == b.grassMesh)
		{
			if (!ignoreDecoPlacementSettings)
			{
				return VoxelandTypeBase.ApproxEqual(a, b);
			}
			return true;
		}
		return false;
	}
}
