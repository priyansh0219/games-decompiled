using System;
using UnityEngine;

public class VFXExtinguishableFire : MonoBehaviour
{
	[Serializable]
	public class FireElement
	{
		public bool enable;

		public GameObject gameObject;

		[Range(0f, 1f)]
		public float minAmountOffset;

		[Range(0f, 1f)]
		public float maxAmountOffset = 1f;

		public bool scaleTransform;

		public Vector3 minScaleFactor = Vector3.zero;

		public bool fadeMaterials;

		public bool hasParticles;

		[Range(0f, 1f)]
		public float minRateFactor;

		private Renderer renderer;

		private ParticleSystem particleSystem;

		private MaterialPropertyBlock block;

		private Vector4 initDayColor;

		private Vector4 initNightColor;

		private Vector3 initScale;

		private float initRate;

		private float previousScalar = -1f;

		public void Init()
		{
			if (scaleTransform)
			{
				initScale = gameObject.transform.localScale;
			}
			if (fadeMaterials)
			{
				block = new MaterialPropertyBlock();
				renderer = gameObject.GetComponent<Renderer>();
				initDayColor = renderer.sharedMaterial.GetColor("_ColorStrength");
				initNightColor = renderer.sharedMaterial.GetColor("_ColorStrengthAtNight");
			}
			if (hasParticles)
			{
				particleSystem = gameObject.GetComponent<ParticleSystem>();
				initRate = particleSystem.GetEmissionRate();
			}
		}

		public float GetLocalAmount(float scalar)
		{
			return Mathf.Clamp01((scalar - minAmountOffset) / (maxAmountOffset - minAmountOffset));
		}

		public void UpdateMaterial(float scalar)
		{
			if (block != null)
			{
				Vector4 value = initDayColor;
				value.w *= scalar;
				block.SetVector(ShaderPropertyID._ColorStrength, value);
				value = initNightColor;
				value.w *= scalar;
				block.SetVector(ShaderPropertyID._ColorStrengthAtNight, value);
				renderer.SetPropertyBlock(block);
			}
		}

		public void UpdateScale(float scalar)
		{
			gameObject.transform.localScale = Vector3.Lerp(Vector3.Scale(minScaleFactor, initScale), initScale, scalar);
		}

		public void UpdateParticles(float scalar)
		{
			particleSystem.SetEmissionRate(Mathf.Lerp(initRate * minRateFactor, initRate, scalar));
		}

		public void UpdateAll(float scalar)
		{
			if (enable && scalar != previousScalar)
			{
				float localAmount = GetLocalAmount(scalar);
				if (scaleTransform)
				{
					UpdateScale(localAmount);
				}
				if (fadeMaterials)
				{
					UpdateMaterial(localAmount);
				}
				if (hasParticles)
				{
					UpdateParticles(localAmount);
				}
				previousScalar = scalar;
			}
		}
	}

	[Range(0f, 1f)]
	public float amount = 1f;

	public bool autoStart = true;

	private bool visibleFX = true;

	public FireElement[] elements;

	public void Init()
	{
		for (int i = 0; i < elements.Length; i++)
		{
			if (elements[i] != null && elements[i].enable)
			{
				elements[i].Init();
			}
		}
	}

	public void UpdateElements()
	{
		for (int i = 0; i < elements.Length; i++)
		{
			elements[i].UpdateAll(amount);
		}
	}

	public void DisableFireFX()
	{
		foreach (Transform item in base.transform)
		{
			item.gameObject.SetActive(value: false);
		}
		visibleFX = false;
	}

	public void EnableFireFX()
	{
		foreach (Transform item in base.transform)
		{
			item.gameObject.SetActive(value: true);
		}
		visibleFX = true;
	}

	private void Start()
	{
		Init();
		if (autoStart && visibleFX)
		{
			for (int i = 0; i < elements.Length; i++)
			{
				elements[i].gameObject.SetActive(elements[i].enable);
			}
		}
	}

	private void Update()
	{
		UpdateElements();
	}

	public void StopAndDestroy()
	{
		UnityEngine.Object.Destroy(base.gameObject, 3f);
	}
}
