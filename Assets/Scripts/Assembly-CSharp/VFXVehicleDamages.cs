using System;
using System.Collections.Generic;
using Gendarme;
using UnityEngine;

[SuppressMessage("Subnautica.Rules", "AvoidDoubleInitializationRule")]
public class VFXVehicleDamages : MonoBehaviour, IOnTakeDamage
{
	[Serializable]
	public class OverlayHelper
	{
		public DamageType damageType;

		[AssertNotNull]
		public Material material;

		[AssertNotNull]
		public GameObject particlesPrefab;

		private VFXOverlayMaterial overlay;

		private ParticleSystem ps;

		private Material matInstance;

		private Color initColor;

		private Color startColor;

		private Color targetColor;

		private float animTime;

		private float animDuration;

		private bool isFadingOut;

		public void Init()
		{
			matInstance = UnityEngine.Object.Instantiate(material);
			initColor = material.color;
			startColor = material.color;
			startColor.a = 0f;
			matInstance.color = startColor;
		}

		public void FadeIn(float inSeconds)
		{
			startColor = matInstance.color;
			targetColor = initColor;
			animDuration = inSeconds;
			animTime = 0f;
			isFadingOut = false;
		}

		public void FadeOut(float inSeconds)
		{
			targetColor = initColor;
			targetColor.a = 0f;
			startColor = matInstance.color;
			animDuration = inSeconds;
			animTime = 0f;
			isFadingOut = true;
		}

		public void UpdateMaterial()
		{
			animTime += Time.deltaTime / animDuration;
			if (!(overlay != null))
			{
				return;
			}
			if (animTime < 1f)
			{
				matInstance.color = Color.Lerp(startColor, targetColor, animTime);
			}
			else if (isFadingOut)
			{
				overlay.RemoveOverlay();
				if (ps.isPlaying)
				{
					ps.Stop();
					UnityEngine.Object.Destroy(ps.gameObject, 2f);
				}
			}
			else
			{
				FadeOut(1.5f);
			}
		}

		public void ApplyOverlay(GameObject go, Renderer[] rends)
		{
			if (overlay == null && rends != null)
			{
				overlay = go.AddComponent<VFXOverlayMaterial>();
				overlay.ApplyOverlay(matInstance, "VFXOverlay: Vehicle " + damageType.ToString() + " damages", instantiateMaterial: false, rends);
				GameObject gameObject = Utils.SpawnZeroedAt(particlesPrefab, go.transform);
				ps = gameObject.GetComponent<ParticleSystem>();
				ps.Play();
			}
			FadeIn(2f);
		}

		public void Destroyed()
		{
			if (overlay != null)
			{
				overlay.RemoveOverlay();
			}
			UnityEngine.Object.Destroy(matInstance);
		}
	}

	[Header("Material Overlays")]
	[AssertNotNull]
	public OverlayHelper[] overlays;

	[AssertNotNull]
	public Renderer[] overlayRenderers;

	[Header("Forward OnTakeDamage Messages")]
	public GameObject[] damageFXSpawnGameObjects;

	public List<IOnTakeDamage> forwardToReceivers = new List<IOnTakeDamage>();

	private void Start()
	{
		for (int i = 0; i < damageFXSpawnGameObjects.Length; i++)
		{
			forwardToReceivers.AddRange(damageFXSpawnGameObjects[i].GetComponentsInChildren<IOnTakeDamage>());
		}
		for (int j = 0; j < overlays.Length; j++)
		{
			overlays[j].Init();
		}
	}

	public void OnTakeDamage(DamageInfo damageInfo)
	{
		if (damageInfo.damage <= 0f)
		{
			return;
		}
		for (int i = 0; i < overlays.Length; i++)
		{
			if (damageInfo.type == overlays[i].damageType)
			{
				overlays[i].ApplyOverlay(base.gameObject, overlayRenderers);
			}
		}
		for (int j = 0; j < forwardToReceivers.Count; j++)
		{
			forwardToReceivers[j]?.OnTakeDamage(damageInfo);
		}
	}

	private void Update()
	{
		for (int i = 0; i < overlays.Length; i++)
		{
			overlays[i].UpdateMaterial();
		}
	}

	private void OnDestroy()
	{
		for (int i = 0; i < overlays.Length; i++)
		{
			overlays[i].Destroyed();
		}
	}
}
