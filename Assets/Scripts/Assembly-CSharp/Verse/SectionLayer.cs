using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public abstract class SectionLayer
	{
		protected Section section;

		public MapMeshFlag relevantChangeTypes;

		public List<LayerSubMesh> subMeshes = new List<LayerSubMesh>();

		protected Map Map => section.map;

		public virtual bool Visible => true;

		public SectionLayer(Section section)
		{
			this.section = section;
		}

		public LayerSubMesh GetSubMesh(Material material)
		{
			if (material == null)
			{
				return null;
			}
			for (int i = 0; i < subMeshes.Count; i++)
			{
				if (subMeshes[i].material == material)
				{
					return subMeshes[i];
				}
			}
			Mesh mesh = new Mesh();
			if (UnityData.isEditor)
			{
				mesh.name = "SectionLayerSubMesh_" + GetType().Name + "_" + Map.Tile;
			}
			Bounds value = new Bounds(section.botLeft.ToVector3(), Vector3.zero);
			value.Encapsulate(section.botLeft.ToVector3() + new Vector3(17f, 0f, 0f));
			value.Encapsulate(section.botLeft.ToVector3() + new Vector3(17f, 0f, 17f));
			value.Encapsulate(section.botLeft.ToVector3() + new Vector3(0f, 0f, 17f));
			LayerSubMesh layerSubMesh = new LayerSubMesh(mesh, material, value);
			subMeshes.Add(layerSubMesh);
			return layerSubMesh;
		}

		protected void FinalizeMesh(MeshParts tags)
		{
			for (int i = 0; i < subMeshes.Count; i++)
			{
				if (subMeshes[i].verts.Count > 0)
				{
					subMeshes[i].FinalizeMesh(tags);
				}
			}
		}

		public virtual void DrawLayer()
		{
			if (!Visible)
			{
				return;
			}
			int count = subMeshes.Count;
			for (int i = 0; i < count; i++)
			{
				LayerSubMesh layerSubMesh = subMeshes[i];
				if (layerSubMesh.finalized && !layerSubMesh.disabled)
				{
					Graphics.DrawMesh(layerSubMesh.mesh, Matrix4x4.identity, layerSubMesh.material, 0);
				}
			}
		}

		public abstract void Regenerate();

		protected void ClearSubMeshes(MeshParts parts)
		{
			foreach (LayerSubMesh subMesh in subMeshes)
			{
				subMesh.Clear(parts);
			}
		}
	}
}
