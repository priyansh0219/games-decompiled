using UnityEngine;

public class VFXSplash : MonoBehaviour
{
	public bool PlayOnAwake = true;

	public bool looping;

	public float duration = 5f;

	public GameObject surfacePrefab;

	public GameObject underPrefab;

	public GameObject particlesPrefab;

	public Vector3 surfOffset;

	public Vector3 underOffset;

	public Vector3 particlesOffset;

	public AnimationCurve surfMaskCurve;

	public AnimationCurve surfScaleX;

	public AnimationCurve surfScaleY;

	public AnimationCurve surfScaleZ;

	private Vector3 startScale;

	private Vector3 newScale = Vector3.zero;

	private float animTime;

	private bool playing;

	private GameObject surfaceSplashModel;

	private GameObject underSplashModel;

	private GameObject particlesSplash;

	public void Init()
	{
		underSplashModel = Utils.SpawnPrefabAt(underPrefab, base.transform, base.transform.position);
		underSplashModel.transform.rotation = base.transform.rotation;
		if (surfacePrefab != null)
		{
			surfaceSplashModel = Utils.SpawnPrefabAt(surfacePrefab, base.transform, base.transform.position);
			surfaceSplashModel.transform.rotation = base.transform.rotation;
		}
		particlesSplash = Utils.SpawnPrefabAt(particlesPrefab, base.transform, base.transform.position);
		particlesSplash.transform.rotation = base.transform.rotation;
		particlesSplash.transform.localPosition = particlesOffset;
		underSplashModel.transform.localPosition = underOffset;
		if (surfacePrefab != null)
		{
			surfaceSplashModel.transform.localPosition = surfOffset;
			startScale = surfaceSplashModel.transform.localScale;
		}
	}

	public void Play()
	{
		particlesSplash.transform.parent = null;
		particlesSplash.transform.position = new Vector3(particlesSplash.transform.position.x, 0f, particlesSplash.transform.position.z);
		particlesSplash.GetComponent<ParticleSystem>().Play();
		if (surfacePrefab != null)
		{
			surfaceSplashModel.transform.parent = null;
			surfaceSplashModel.transform.position = new Vector3(surfaceSplashModel.transform.position.x, 0f, surfaceSplashModel.transform.position.z);
			surfaceSplashModel.transform.localScale = startScale;
		}
		newScale = Vector3.zero;
		animTime = 0f;
		playing = true;
	}

	private void Awake()
	{
		Init();
		if (PlayOnAwake)
		{
			Play();
		}
	}

	private void Update()
	{
		if (!playing)
		{
			return;
		}
		animTime += Time.deltaTime / duration;
		if (animTime > 0.99f)
		{
			if (!looping)
			{
				playing = false;
				return;
			}
			Play();
		}
		if (surfacePrefab != null)
		{
			surfaceSplashModel.GetComponent<Renderer>().material.SetTextureOffset(ShaderPropertyID._MaskTex, new Vector2(surfMaskCurve.Evaluate(animTime), 0.5f));
			newScale.x = startScale.x * surfScaleX.Evaluate(animTime);
			newScale.y = startScale.y * surfScaleY.Evaluate(animTime);
			newScale.z = startScale.z * surfScaleZ.Evaluate(animTime);
			surfaceSplashModel.transform.localScale = newScale;
		}
	}

	private void OnDestroy()
	{
	}
}
