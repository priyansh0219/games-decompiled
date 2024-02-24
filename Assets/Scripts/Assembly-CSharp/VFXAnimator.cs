using System;
using UnityEngine;

public class VFXAnimator : MonoBehaviour
{
	[Serializable]
	public class VFXAnimatedFloat
	{
		public string name;

		public float startValue;

		public AnimationCurve curve;

		[HideInInspector]
		public int shaderID;
	}

	[Serializable]
	public class VFXAnimatedTextureOffset
	{
		public string name;

		public Vector2 startVector;

		public AnimationCurve curveX;

		public AnimationCurve curveY;

		[HideInInspector]
		public Vector2 currentVector = new Vector2(0f, 0f);

		[HideInInspector]
		public int shaderID;
	}

	public bool toggleRenderer = true;

	public bool PlayOnAwake;

	public bool looping;

	public bool reverse;

	public float duration = 5f;

	public float randomAmount;

	public bool destroyMaterial;

	public bool lerpColor;

	public Color colorEnd;

	private Color colorStart;

	public AnimationCurve blendCurve;

	public bool lerpScale;

	public Vector3 initScale;

	public AnimationCurve scaleX;

	public AnimationCurve scaleY;

	public AnimationCurve scaleZ;

	private Vector3 currentScale;

	public bool lerpFloats;

	private Renderer rend;

	private Material[] mats;

	public VFXAnimatedFloat[] animatedFloats;

	public bool lerpTextureOffset;

	public VFXAnimatedTextureOffset[] animatedTextureOffset;

	private float animTime;

	private float currentTime;

	[HideInInspector]
	public bool isPlaying;

	public void Play()
	{
		currentScale = initScale;
		animTime = (currentTime = 0f);
		isPlaying = true;
		UpdateScale();
		Material[] array = mats;
		foreach (Material mat in array)
		{
			UpdateColor(mat);
			UpdateFloats(mat);
			UpdateTextureOffsets(mat);
		}
		if (!rend.enabled)
		{
			rend.enabled = true;
		}
	}

	public void Stop()
	{
		animTime = (currentTime = 0f);
		isPlaying = false;
		if (rend.enabled && toggleRenderer)
		{
			rend.enabled = false;
		}
	}

	public void StopAndReset()
	{
		Stop();
		UpdateScale();
		Material[] array = mats;
		foreach (Material mat in array)
		{
			UpdateColor(mat);
			UpdateFloats(mat);
			UpdateTextureOffsets(mat);
		}
	}

	private void InitFloats()
	{
		if (lerpFloats)
		{
			for (int i = 0; i < animatedFloats.Length; i++)
			{
				animatedFloats[i].shaderID = Shader.PropertyToID(animatedFloats[i].name);
				animatedFloats[i].startValue = rend.material.GetFloat(animatedFloats[i].shaderID);
			}
		}
	}

	private void InitTextureOffsets()
	{
		if (lerpTextureOffset)
		{
			for (int i = 0; i < animatedTextureOffset.Length; i++)
			{
				animatedTextureOffset[i].shaderID = Shader.PropertyToID(animatedTextureOffset[i].name);
				animatedTextureOffset[i].startVector = rend.material.GetTextureOffset(animatedTextureOffset[i].shaderID);
			}
		}
	}

	private void Awake()
	{
		if (rend == null)
		{
			rend = GetComponent<Renderer>();
			mats = rend.materials;
		}
		if (lerpColor)
		{
			colorStart = rend.material.color;
		}
		if (lerpFloats)
		{
			InitFloats();
		}
		if (lerpTextureOffset)
		{
			InitTextureOffsets();
		}
		if (PlayOnAwake)
		{
			Play();
		}
	}

	private void UpdateScale()
	{
		if (lerpScale)
		{
			currentScale = new Vector3(initScale.x * scaleX.Evaluate(currentTime), initScale.y * scaleY.Evaluate(currentTime), initScale.z * scaleZ.Evaluate(currentTime));
			base.transform.localScale = currentScale;
		}
	}

	private void UpdateColor(Material mat)
	{
		if (lerpColor)
		{
			mat.color = Color.Lerp(colorStart, colorEnd, blendCurve.Evaluate(currentTime));
		}
	}

	private void UpdateFloats(Material mat)
	{
		if (lerpFloats)
		{
			for (int i = 0; i < animatedFloats.Length; i++)
			{
				mat.SetFloat(animatedFloats[i].shaderID, animatedFloats[i].curve.Evaluate(currentTime));
			}
		}
	}

	private void UpdateTextureOffsets(Material mat)
	{
		if (lerpTextureOffset)
		{
			for (int i = 0; i < animatedTextureOffset.Length; i++)
			{
				animatedTextureOffset[i].currentVector.x = animatedTextureOffset[i].curveX.Evaluate(currentTime);
				animatedTextureOffset[i].currentVector.y = animatedTextureOffset[i].curveY.Evaluate(currentTime);
				mat.SetTextureOffset(animatedTextureOffset[i].shaderID, animatedTextureOffset[i].currentVector);
			}
		}
	}

	private void Update()
	{
		if (!isPlaying)
		{
			return;
		}
		animTime += Time.deltaTime / (duration + UnityEngine.Random.Range(0f - randomAmount, randomAmount));
		if (reverse)
		{
			currentTime = 1f - animTime;
		}
		else
		{
			currentTime = animTime;
		}
		if (animTime > 0.99f)
		{
			if (!looping)
			{
				Stop();
				return;
			}
			Play();
		}
		UpdateScale();
		Material[] array = mats;
		foreach (Material mat in array)
		{
			UpdateColor(mat);
			UpdateFloats(mat);
			UpdateTextureOffsets(mat);
		}
	}

	private void OnDestroy()
	{
		if (destroyMaterial && rend != null)
		{
			UnityEngine.Object.Destroy(rend.material);
		}
	}
}
