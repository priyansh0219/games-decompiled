using UnityEngine;

public class DistanceCull : MonoBehaviour, ICompileTimeCheckable
{
	[Tooltip("Disable objects when they are in close range")]
	public bool proximityCulling;

	public float distanceSqr = 5000f;

	public bool isLight;

	public bool isParticleSystem;

	public float fadeDuration = 1f;

	public bool isEnabled = true;

	public ParticleSystem ps;

	private bool psIsPlaying;

	private Light theLight;

	private float initIntensity;

	public bool isFadingIn;

	public bool isFadingOut;

	public float fadeScalar = 1f;

	private void Start()
	{
		if (isLight)
		{
			theLight = GetComponent<Light>();
			initIntensity = theLight.intensity;
		}
		if (isParticleSystem)
		{
			ps = GetComponent<ParticleSystem>();
			psIsPlaying = ps.isPlaying;
		}
	}

	public void EnableObject()
	{
		isFadingOut = false;
		isFadingIn = true;
	}

	public void DisableObject()
	{
		isFadingIn = false;
		isFadingOut = true;
		if (ps != null && psIsPlaying)
		{
			psIsPlaying = false;
			ps.Stop();
		}
	}

	private void OnEnable()
	{
		isEnabled = true;
		EnableObject();
	}

	private void OnDisable()
	{
		isEnabled = false;
	}

	private void LateUpdate()
	{
		if (isLight)
		{
			if (isFadingIn || isFadingOut)
			{
				if (isFadingOut)
				{
					fadeScalar -= Time.deltaTime / fadeDuration;
				}
				else
				{
					fadeScalar += Time.deltaTime / fadeDuration;
				}
				fadeScalar = Mathf.Clamp01(fadeScalar);
				theLight.intensity = initIntensity * fadeScalar;
				if (fadeScalar == 0f)
				{
					isFadingOut = false;
					base.gameObject.SetActive(value: false);
				}
				else if (fadeScalar == 1f)
				{
					isFadingIn = false;
				}
			}
		}
		else if (isParticleSystem)
		{
			if (isFadingIn)
			{
				fadeScalar = 1f;
				isFadingIn = false;
				if (!psIsPlaying)
				{
					ps.Play();
					psIsPlaying = true;
				}
			}
			else if (isFadingOut)
			{
				fadeScalar -= Time.deltaTime / fadeDuration;
				fadeScalar = Mathf.Clamp01(fadeScalar);
				if (fadeScalar == 0f)
				{
					ps.Clear();
					isFadingOut = false;
					base.gameObject.SetActive(value: false);
				}
			}
		}
		else if (isFadingOut)
		{
			fadeScalar = 0f;
			isFadingOut = false;
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(base.transform.position, Mathf.Sqrt(distanceSqr));
	}

	public string CompileTimeCheck()
	{
		if (isLight && !GetComponent<Light>())
		{
			return "Missing light component for Distance Cull set to 'is light'";
		}
		if (isParticleSystem && !GetComponent<ParticleSystem>())
		{
			return "Missing particle system for Distance Cull set to 'is light'";
		}
		return null;
	}
}
