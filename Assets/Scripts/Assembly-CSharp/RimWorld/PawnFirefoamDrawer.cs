using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnFirefoamDrawer : PawnOverlayDrawer
	{
		public bool IsCoveredInFoam;

		public const float TextureScaleFactor = 2.8f;

		public const float TextureTiles = 1.4f;

		public const float TextureOffsetVecMagnitude = 2f;

		private static readonly string[] foamTexturePaths = new string[4] { "Things/Pawn/Overlays/Firefoam/FireFoamOverlayA", "Things/Pawn/Overlays/Firefoam/FireFoamOverlayB", "Things/Pawn/Overlays/Firefoam/FireFoamOverlayC", "Things/Pawn/Overlays/Firefoam/FireFoamOverlayD" };

		public PawnFirefoamDrawer(Pawn pawn)
			: base(pawn)
		{
		}

		protected override void WriteCache(CacheKey key, List<DrawCall> writeTarget)
		{
			Rot4 pawnRot = key.pawnRot;
			Quaternion quat = key.quat;
			Mesh bodyMesh = key.bodyMesh;
			Vector3 drawLoc = key.drawLoc;
			OverlayLayer layer = key.layer;
			Rand.PushState(pawn.thingIDNumber * (int)(layer + 1));
			try
			{
				bool num = (pawn.Drawer.renderer.graphics.nakedGraphic.EastFlipped && pawnRot == Rot4.East) || (pawn.Drawer.renderer.graphics.nakedGraphic.WestFlipped && pawnRot == Rot4.West);
				int num2 = (Rand.Range(0, foamTexturePaths.Length) + pawnRot.AsInt) % foamTexturePaths.Length;
				Material material = MaterialPool.MatFrom(foamTexturePaths[num2], ShaderDatabase.FirefoamOverlay, Color.white);
				Mesh mesh = (num ? MeshPool.GridPlaneFlip(Vector2.one * 0.25f) : MeshPool.GridPlane(Vector2.one * 0.25f));
				Vector3 size = bodyMesh.bounds.size;
				Vector3 extents = bodyMesh.bounds.extents;
				float num3 = size.magnitude * 2.8f;
				Vector3 vector = mesh.bounds.size * num3;
				Vector3 vector2 = mesh.bounds.extents * num3;
				Vector3 vector3 = drawLoc + quat * bodyMesh.bounds.center;
				MaterialRequest req = default(MaterialRequest);
				req.maskTex = (Texture2D)pawn.Drawer.renderer.graphics.nakedGraphic.MatAt(pawnRot).mainTexture;
				req.mainTex = material.mainTexture;
				req.color = material.color;
				req.shader = material.shader;
				material = MaterialPool.MatFrom(req);
				Vector3 vector4 = vector3 - vector2;
				Vector3 vector5 = drawLoc - extents;
				Vector3 vector6 = Rand.InsideUnitCircleVec3 * 2f;
				Vector4 value = new Vector4(vector.x / size.x, vector.z / size.z);
				Vector4 value2 = new Vector4((vector4.x - vector5.x) / size.x, (vector4.z - vector5.z) / size.z);
				Vector4 value3 = new Vector4(vector6.x, vector6.z);
				Vector4 value4 = new Vector4(1.4f, 1.4f, 1f, 1f);
				Matrix4x4 tRS = Matrix4x4.TRS(vector3, quat, Vector3.one * num3);
				writeTarget.Add(new DrawCall
				{
					overlayMat = material,
					TRS = tRS,
					overlayMesh = mesh,
					displayOverApparel = true,
					useSkinColor = false,
					maskTexScale = value,
					maskTexOffset = value2,
					mainTexScale = value4,
					mainTexOffset = value3
				});
			}
			finally
			{
				Rand.PopState();
			}
		}
	}
}
