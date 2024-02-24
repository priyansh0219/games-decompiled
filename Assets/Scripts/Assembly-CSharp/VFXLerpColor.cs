using UnityEngine;

public class VFXLerpColor : MonoBehaviour, IManagedUpdateBehaviour, IManagedBehaviour, ICompileTimeCheckable
{
	public bool PlayOnAwake = true;

	public bool destroyMaterial = true;

	public bool looping;

	public bool reverse;

	public float duration = 5f;

	public float randomAmount;

	[AssertNotNull]
	public AnimationCurve blendCurve;

	public Color colorEnd;

	private Color[] colorStart;

	private float animTime;

	private float randomDuration;

	private bool playing;

	private Material[] mats;

	private Renderer rend;

	public int managedUpdateIndex { get; set; }

	public string GetProfileTag()
	{
		return "VFXLerpColor";
	}

	private void OnDisable()
	{
		BehaviourUpdateUtils.Deregister(this);
	}

	public void Play()
	{
		animTime = 0f;
		playing = true;
		randomDuration = Random.Range(0f - randomAmount, randomAmount);
		BehaviourUpdateUtils.Register(this);
	}

	public void ResetColor()
	{
		if (colorStart != null)
		{
			for (int i = 0; i < mats.Length; i++)
			{
				if (mats[i] != null)
				{
					mats[i].color = colorStart[i];
				}
			}
		}
		if (playing)
		{
			playing = false;
			BehaviourUpdateUtils.Deregister(this);
		}
	}

	private void Awake()
	{
		rend = GetComponent<Renderer>();
		mats = rend.materials;
		colorStart = new Color[mats.Length];
		for (int i = 0; i < mats.Length; i++)
		{
			if (mats[i] != null)
			{
				colorStart[i] = mats[i].color;
			}
		}
		if (PlayOnAwake)
		{
			Play();
		}
	}

	public void ManagedUpdate()
	{
		if (!playing || mats == null)
		{
			return;
		}
		animTime += Time.deltaTime / (duration + randomDuration);
		if (animTime > 0.99f)
		{
			if (!looping)
			{
				playing = false;
				BehaviourUpdateUtils.Deregister(this);
				animTime = 1f;
			}
			else
			{
				Play();
			}
		}
		if (blendCurve == null || mats == null || !rend)
		{
			return;
		}
		float time = (reverse ? (1f - animTime) : animTime);
		float t = blendCurve.Evaluate(time);
		for (int i = 0; i < mats.Length; i++)
		{
			if (mats[i] != null)
			{
				mats[i].color = Color.Lerp(colorStart[i], colorEnd, t);
			}
		}
		if (!rend.enabled)
		{
			rend.enabled = true;
		}
	}

	private void OnDestroy()
	{
		BehaviourUpdateUtils.Deregister(this);
		if (colorStart != null && destroyMaterial && animTime > 0f)
		{
			for (int i = 0; i < mats.Length; i++)
			{
				Object.Destroy(mats[i]);
			}
		}
	}

	public string CompileTimeCheck()
	{
		if (blendCurve == null)
		{
			return "Missing blend curve";
		}
		Renderer component = GetComponent<Renderer>();
		if (!component)
		{
			return "Missing renderer next to VFXLerpColor";
		}
		Material[] sharedMaterials = component.sharedMaterials;
		if (sharedMaterials == null || sharedMaterials.Length == 0)
		{
			return "Missing materials on renderer next to VFXLerpColor";
		}
		for (int i = 0; i < sharedMaterials.Length; i++)
		{
			if (!sharedMaterials[i])
			{
				return $"Material {i} on renderer next to VFXLerpColor is missing.";
			}
		}
		return null;
	}
}
