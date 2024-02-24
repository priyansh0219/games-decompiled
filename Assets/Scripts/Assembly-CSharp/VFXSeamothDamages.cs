using System;
using UnityEngine;

public class VFXSeamothDamages : MonoBehaviour, IOnTakeDamage
{
	[Serializable]
	public class ParticlesEmitter
	{
		public ParticleSystem ps;

		public bool isPlaying;

		public void Play()
		{
			if (!isPlaying)
			{
				ps.Play();
				isPlaying = true;
			}
		}

		public void Stop()
		{
			if (isPlaying)
			{
				ps.Stop();
				isPlaying = false;
			}
		}
	}

	public ParticlesEmitter[] bubblesEmitters;

	public ParticleSystem dripsParticles;

	public ParticleSystem bubblesParticles;

	public ParticleSystem smokeParticles;

	public Renderer[] modelsAlphaPow;

	public float minPow = 1f;

	public float maxPow = 4f;

	public Transform[] sparksPoints;

	public GameObject[] sparksPrefabs;

	public float minDelay = 0.5f;

	public float maxDelay = 10f;

	private LiveMixin liveMixin;

	private float delayTimer;

	private float currentDelay = 1f;

	private float healthRatio = 1f;

	private float prevHealthRatio;

	private Color[] modelsInitColor;

	private Color[] modelsFadedColor;

	private void Start()
	{
		liveMixin = Utils.FindAncestorWithComponent<LiveMixin>(base.gameObject);
		modelsInitColor = new Color[modelsAlphaPow.Length];
		modelsFadedColor = new Color[modelsAlphaPow.Length];
		for (int i = 0; i < modelsAlphaPow.Length; i++)
		{
			modelsInitColor[i] = modelsAlphaPow[i].material.GetColor(ShaderPropertyID._Color);
			modelsFadedColor[i] = modelsInitColor[i];
			modelsFadedColor[i].a = 0f;
		}
	}

	public void OnTakeDamage(DamageInfo damageInfo)
	{
		if (damageInfo.damage > 10f)
		{
			SpawnRandomSparks();
		}
		ComputeDelay();
	}

	private void Update()
	{
		healthRatio = liveMixin.health / liveMixin.maxHealth;
		if (healthRatio < 1f)
		{
			if (!(prevHealthRatio < 1f))
			{
				ToggleChildren(enable: true);
				ToggleAlphaPowRenderers(enable: true);
			}
			delayTimer += Time.deltaTime / currentDelay;
			if (delayTimer > 0.99f && healthRatio < 0.5f)
			{
				SpawnRandomSparks();
				ComputeDelay();
				delayTimer = 0f;
			}
			UpdateMaterials();
			UpdateParticles();
		}
		else if (prevHealthRatio < 1f)
		{
			ToggleChildren(enable: false);
			ToggleAlphaPowRenderers(enable: false);
		}
		prevHealthRatio = healthRatio;
	}

	private void UpdateParticles()
	{
		float rate = Mathf.Clamp(0.5f - healthRatio, 0f, 1f) * 50f;
		float rate2 = Mathf.Clamp(0.25f - healthRatio, 0f, 1f) * 50f;
		UpdatePArticlesEmission(dripsParticles, rate);
		UpdatePArticlesEmission(bubblesParticles, rate);
		UpdatePArticlesEmission(smokeParticles, rate2);
		if (healthRatio < 0.6f)
		{
			bubblesEmitters[0].Play();
		}
		else
		{
			bubblesEmitters[0].Stop();
		}
		if (healthRatio < 0.3f)
		{
			bubblesEmitters[1].Play();
		}
		else
		{
			bubblesEmitters[1].Stop();
		}
	}

	private void UpdatePArticlesEmission(ParticleSystem ps, float rate)
	{
		if ((bool)ps)
		{
			if (rate <= 0.01f && ps.isPlaying)
			{
				ps.Stop();
			}
			else if (rate > 0.01f && !ps.isPlaying)
			{
				ps.Play();
			}
			ps.SetEmissionRate(rate);
		}
	}

	private void TogglePArticlesBelow(ParticleSystem ps, float threshold)
	{
		if ((bool)ps)
		{
			if (healthRatio < threshold && !ps.isPlaying)
			{
				ps.Play();
			}
			else if (healthRatio >= threshold && ps.isPlaying)
			{
				ps.Stop();
			}
		}
	}

	private void ComputeDelay()
	{
		float value = UnityEngine.Random.Range(minDelay, maxDelay) * (1f - healthRatio);
		currentDelay = Mathf.Clamp(value, minDelay, maxDelay);
	}

	private void SpawnRandomSparks()
	{
		GameObject prefab = sparksPrefabs[UnityEngine.Random.Range(0, sparksPrefabs.Length)];
		Transform parent = sparksPoints[UnityEngine.Random.Range(0, sparksPoints.Length)];
		Utils.SpawnZeroedAt(prefab, parent);
	}

	private void UpdateMaterials()
	{
		float value = Mathf.Lerp(minPow, maxPow, healthRatio);
		for (int i = 0; i < modelsAlphaPow.Length; i++)
		{
			modelsAlphaPow[i].material.SetFloat(ShaderPropertyID._AlphaPow, value);
			modelsAlphaPow[i].material.SetColor(ShaderPropertyID._Color, Color.Lerp(modelsInitColor[i], modelsFadedColor[i], healthRatio));
		}
	}

	private void ToggleChildren(bool enable)
	{
		Transform[] componentsInChildren = GetComponentsInChildren<Transform>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i] != base.transform)
			{
				componentsInChildren[i].gameObject.SetActive(enable);
			}
		}
	}

	private void ToggleAlphaPowRenderers(bool enable)
	{
		for (int i = 0; i < modelsAlphaPow.Length; i++)
		{
			modelsAlphaPow[i].enabled = enable;
		}
	}

	private void OnDestroy()
	{
		if (modelsInitColor != null)
		{
			for (int i = 0; i < modelsAlphaPow.Length; i++)
			{
				UnityEngine.Object.Destroy(modelsAlphaPow[i].material);
			}
		}
	}
}
