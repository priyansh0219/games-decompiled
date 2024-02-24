using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace WorldStreaming
{
	public sealed class ClipmapChunk : TerrainChunkPiece, IVoxelandChunk2, IManagedUpdateBehaviour, IManagedBehaviour
	{
		private struct FadeController
		{
			private enum Fading
			{
				None = 0,
				In = 1,
				Out = 2,
				Waiting = 3
			}

			private readonly List<MeshRenderer> renders;

			private Fading fading;

			private float fadeAmount;

			private float waitAmount;

			public FadeController(List<MeshRenderer> renders)
			{
				this.renders = renders;
				fading = Fading.None;
				fadeAmount = 0.001f;
				waitAmount = 0f;
			}

			private bool UpdateFadeIn(float deltaFade)
			{
				fadeAmount += deltaFade;
				if (fadeAmount < 1f)
				{
					SetRenderersFadeAmount(renders, fadeAmount);
					return true;
				}
				fadeAmount = 1f;
				fading = Fading.None;
				SetRenderersFadeAmount(renders, fadeAmount);
				return false;
			}

			private bool UpdateFadeOut(float deltaFade)
			{
				fadeAmount -= deltaFade;
				if (fadeAmount > 0.001f)
				{
					SetRenderersFadeAmount(renders, fadeAmount);
					return true;
				}
				fadeAmount = 0.001f;
				fading = Fading.None;
				SetRenderersEnabled(renders, enabled: false, 1f);
				return false;
			}

			private bool UpdateWaiting(float deltaFade)
			{
				waitAmount -= deltaFade;
				if (waitAmount > 0f)
				{
					return true;
				}
				fading = Fading.Out;
				return true;
			}

			public bool Update(float deltaFade)
			{
				switch (fading)
				{
				case Fading.None:
					return false;
				case Fading.In:
					return UpdateFadeIn(deltaFade);
				case Fading.Out:
					return UpdateFadeOut(deltaFade);
				case Fading.Waiting:
					return UpdateWaiting(deltaFade);
				default:
					return false;
				}
			}

			public void FadeIn()
			{
				fading = Fading.In;
				SetRenderersEnabled(renders, enabled: true, fadeAmount);
			}

			public void FadeOut()
			{
				fading = Fading.Waiting;
				waitAmount = 1.5f;
				SetRenderersEnabled(renders, enabled: true, fadeAmount);
			}

			public void Show()
			{
				fadeAmount = 1f;
				fading = Fading.None;
				SetRenderersEnabled(renders, enabled: true, 1f);
			}

			public void Hide()
			{
				fadeAmount = 0.001f;
				fading = Fading.None;
				SetRenderersEnabled(renders, enabled: false, 1f);
			}
		}

		private const float fadeDuration = 0.5f;

		private const float waitDuration = 1.5f;

		private const float inverseFadeDuration = 2f;

		private const float minFadeAmount = 0.001f;

		[NonSerialized]
		public readonly List<MeshFilter> hiFilters = new List<MeshFilter>();

		[NonSerialized]
		public readonly List<MeshRenderer> hiRenders = new List<MeshRenderer>();

		[NonSerialized]
		public readonly List<MeshFilter> grassFilters = new List<MeshFilter>();

		[NonSerialized]
		public readonly List<MeshRenderer> grassRenders = new List<MeshRenderer>();

		[NonSerialized]
		public readonly List<TerrainChunkPiece> chunkPieces = new List<TerrainChunkPiece>();

		[NonSerialized]
		public MeshCollider collision;

		[NonSerialized]
		private bool isGrassShowing;

		[NonSerialized]
		private FadeController terrainFadeController;

		[NonSerialized]
		private FadeController grassFadeController;

		[field: NonSerialized]
		public int managedUpdateIndex { get; set; }

		List<MeshFilter> IVoxelandChunk2.hiFilters => hiFilters;

		List<MeshRenderer> IVoxelandChunk2.hiRenders => hiRenders;

		List<MeshFilter> IVoxelandChunk2.grassFilters => grassFilters;

		List<MeshRenderer> IVoxelandChunk2.grassRenders => grassRenders;

		MeshCollider IVoxelandChunk2.collision
		{
			get
			{
				return collision;
			}
			set
			{
				collision = value;
			}
		}

		List<TerrainChunkPiece> IVoxelandChunk2.chunkPieces => chunkPieces;

		public string GetProfileTag()
		{
			return "ClipmapChunk";
		}

		public ClipmapChunk()
		{
			terrainFadeController = new FadeController(hiRenders);
			grassFadeController = new FadeController(grassRenders);
		}

		public void ManagedUpdate()
		{
			float deltaFade = 2f * Time.unscaledDeltaTime;
			if ((0u | (terrainFadeController.Update(deltaFade) ? 1u : 0u) | (grassFadeController.Update(deltaFade) ? 1u : 0u)) == 0)
			{
				BehaviourUpdateUtils.Deregister(this);
			}
		}

		private void OnDestroy()
		{
			BehaviourUpdateUtils.Deregister(this);
		}

		public override void OnReturnToPool()
		{
			base.OnReturnToPool();
			BehaviourUpdateUtils.Deregister(this);
			hiFilters.Clear();
			hiRenders.Clear();
			grassFilters.Clear();
			grassRenders.Clear();
			collision = null;
			isGrassShowing = false;
			chunkPieces.Clear();
		}

		private static void SetRenderersEnabled(List<MeshRenderer> renderers, bool enabled, float fadeAmount)
		{
			for (int i = 0; i < renderers.Count; i++)
			{
				SetRendererEnabled(renderers[i], enabled, fadeAmount);
			}
		}

		private static void SetRenderersFadeAmount(List<MeshRenderer> renderers, float fadeAmount)
		{
			for (int i = 0; i < renderers.Count; i++)
			{
				renderers[i].SetFadeAmount(fadeAmount);
			}
		}

		private static void SetRendererEnabled(Renderer renderer, bool enabled, float fadeAmount)
		{
			renderer.enabled = enabled;
			renderer.SetFadeAmount(fadeAmount);
		}

		[ContextMenu("Fade in")]
		public void FadeIn()
		{
			FadeInTerrain();
			FadeInGrass();
		}

		[ContextMenu("Fade out")]
		public void FadeOut()
		{
			FadeOutTerrain();
			FadeOutGrass();
		}

		[ContextMenu("Show")]
		public void Show()
		{
			ShowTerrain();
			ShowGrass();
		}

		[ContextMenu("Hide")]
		public void Hide()
		{
			Hide(keepGrassVisible: false);
		}

		public void Hide(bool keepGrassVisible)
		{
			FadeOutTerrain();
			if (!keepGrassVisible)
			{
				FadeOutGrass();
			}
		}

		[ContextMenu("Fade in terrain")]
		public void FadeInTerrain()
		{
			terrainFadeController.FadeIn();
			BehaviourUpdateUtils.Register(this);
		}

		[ContextMenu("Fade in grass")]
		public void FadeInGrass()
		{
			isGrassShowing = true;
			grassFadeController.FadeIn();
			BehaviourUpdateUtils.Register(this);
		}

		[ContextMenu("Fade out terrain")]
		public void FadeOutTerrain()
		{
			terrainFadeController.FadeOut();
			BehaviourUpdateUtils.Register(this);
		}

		[ContextMenu("Fade out grass")]
		public void FadeOutGrass()
		{
			isGrassShowing = false;
			grassFadeController.FadeOut();
			BehaviourUpdateUtils.Register(this);
		}

		[ContextMenu("Show terrain")]
		public void ShowTerrain()
		{
			terrainFadeController.Show();
		}

		[ContextMenu("Show grass")]
		public void ShowGrass()
		{
			isGrassShowing = true;
			grassFadeController.Show();
		}

		[ContextMenu("Hide terrain")]
		public void HideTerrain()
		{
			terrainFadeController.Hide();
		}

		[ContextMenu("Hide grass")]
		public void HideGrass()
		{
			isGrassShowing = false;
			grassFadeController.Hide();
		}

		MeshCollider IVoxelandChunk2.EnsureCollision()
		{
			return VoxelandChunk.EnsureCollision(this);
		}

		[SpecialName]
		Transform IVoxelandChunk2.get_transform()
		{
			return base.transform;
		}

		[SpecialName]
		GameObject IVoxelandChunk2.get_gameObject()
		{
			return base.gameObject;
		}
	}
}
