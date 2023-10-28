using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public abstract class PawnOverlayDrawer
	{
		public enum OverlayLayer
		{
			Body = 0,
			Head = 1
		}

		protected struct DrawCall
		{
			public Mesh overlayMesh;

			public Matrix4x4 TRS;

			public Material overlayMat;

			public bool displayOverApparel;

			public bool useSkinColor;

			public Vector4? maskTexScale;

			public Vector4? maskTexOffset;

			public Vector4? mainTexScale;

			public Vector4? mainTexOffset;
		}

		protected struct CacheKey : IEquatable<CacheKey>
		{
			public Vector3 drawLoc;

			public Mesh bodyMesh;

			public Quaternion quat;

			public Rot4 pawnRot;

			public OverlayLayer layer;

			private int hash;

			public CacheKey(Vector3 drawLoc, Mesh bodyMesh, Quaternion quat, Rot4 pawnRot, OverlayLayer layer)
			{
				this.drawLoc = drawLoc;
				this.bodyMesh = bodyMesh;
				this.quat = quat;
				this.pawnRot = pawnRot;
				this.layer = layer;
				hash = Gen.HashCombineInt((!(bodyMesh == null)) ? bodyMesh.GetHashCode() : 0, drawLoc.GetHashCode());
				hash = Gen.HashCombineInt(quat.GetHashCode(), hash);
				hash = Gen.HashCombineInt(pawnRot.GetHashCode(), hash);
				hash = Gen.HashCombineInt(layer.GetHashCode(), hash);
			}

			public override int GetHashCode()
			{
				return hash;
			}

			public bool Equals(CacheKey other)
			{
				if (other.drawLoc == drawLoc && other.bodyMesh == bodyMesh && other.quat == quat && other.pawnRot == pawnRot)
				{
					return other.layer == layer;
				}
				return false;
			}

			public override bool Equals(object other)
			{
				if (other is CacheKey other2)
				{
					return Equals(other2);
				}
				return false;
			}
		}

		protected Pawn pawn;

		private static MaterialPropertyBlock propBlock = new MaterialPropertyBlock();

		protected Dictionary<CacheKey, List<DrawCall>> drawCallCache = new Dictionary<CacheKey, List<DrawCall>>();

		protected static List<List<DrawCall>> drawCallListPool = new List<List<DrawCall>>();

		public PawnOverlayDrawer(Pawn pawn)
		{
			this.pawn = pawn;
		}

		private static List<DrawCall> GetDrawCallList()
		{
			if (drawCallListPool.Count == 0)
			{
				return new List<DrawCall>();
			}
			List<DrawCall> result = drawCallListPool[drawCallListPool.Count - 1];
			drawCallListPool.RemoveAt(drawCallListPool.Count - 1);
			return result;
		}

		private static void ReturnDrawCallList(List<DrawCall> lst)
		{
			lst.Clear();
			drawCallListPool.Add(lst);
		}

		public void ClearCache()
		{
			foreach (List<DrawCall> value in drawCallCache.Values)
			{
				ReturnDrawCallList(value);
			}
			drawCallCache.Clear();
		}

		protected abstract void WriteCache(CacheKey key, List<DrawCall> writeTarget);

		public void RenderPawnOverlay(Vector3 drawLoc, Mesh bodyMesh, Quaternion quat, bool drawNow, OverlayLayer layer, Rot4 pawnRot, bool? overApparel = null)
		{
			CacheKey key = new CacheKey(drawLoc, bodyMesh, quat, pawnRot, layer);
			if (!drawCallCache.TryGetValue(key, out var value))
			{
				value = GetDrawCallList();
				WriteCache(key, value);
				drawCallCache.Add(key, value);
			}
			foreach (DrawCall item in value)
			{
				if (!overApparel.HasValue || overApparel == item.displayOverApparel)
				{
					DoDrawCall(item, drawNow);
				}
			}
		}

		private void DoDrawCall(DrawCall call, bool drawNow)
		{
			if (drawNow)
			{
				if (call.maskTexOffset.HasValue)
				{
					call.overlayMat.SetVector(ShaderPropertyIDs.MaskTextureOffset, call.maskTexOffset.Value);
					call.overlayMat.SetVector(ShaderPropertyIDs.MaskTextureScale, call.maskTexScale.Value);
				}
				if (call.mainTexOffset.HasValue)
				{
					call.overlayMat.SetVector(ShaderPropertyIDs.MainTextureOffset, call.mainTexOffset.Value);
				}
				if (call.mainTexScale.HasValue)
				{
					call.overlayMat.SetVector(ShaderPropertyIDs.MainTextureScale, call.mainTexScale.Value);
				}
				if (call.useSkinColor && pawn.story != null)
				{
					call.overlayMat.SetColor(ShaderPropertyIDs.Color, pawn.story.SkinColor);
				}
				call.overlayMat.SetPass(0);
				Graphics.DrawMeshNow(call.overlayMesh, call.TRS);
				call.overlayMat.SetVector(ShaderPropertyIDs.MaskTextureOffset, Vector4.zero);
				call.overlayMat.SetVector(ShaderPropertyIDs.MaskTextureScale, Vector4.one);
				call.overlayMat.SetVector(ShaderPropertyIDs.MainTextureOffset, Vector4.zero);
				call.overlayMat.SetVector(ShaderPropertyIDs.MainTextureScale, Vector4.one);
				if (call.useSkinColor && pawn.story != null)
				{
					call.overlayMat.SetColor(ShaderPropertyIDs.Color, Color.white);
				}
			}
			else
			{
				propBlock.Clear();
				if (call.maskTexOffset.HasValue)
				{
					propBlock.SetVector(ShaderPropertyIDs.MaskTextureOffset, call.maskTexOffset.Value);
					propBlock.SetVector(ShaderPropertyIDs.MaskTextureScale, call.maskTexScale.Value);
				}
				if (call.mainTexOffset.HasValue)
				{
					propBlock.SetVector(ShaderPropertyIDs.MainTextureOffset, call.mainTexOffset.Value);
				}
				if (call.mainTexScale.HasValue)
				{
					propBlock.SetVector(ShaderPropertyIDs.MainTextureScale, call.mainTexScale.Value);
				}
				if (call.useSkinColor && pawn.story != null)
				{
					propBlock.SetColor(ShaderPropertyIDs.Color, pawn.story.SkinColor);
				}
				Graphics.DrawMesh(call.overlayMesh, call.TRS, call.overlayMat, 0, null, 0, propBlock);
			}
		}
	}
}
