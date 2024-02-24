using UnityEngine;

public class VFXSandSharkDune : MonoBehaviour
{
	public enum Anims
	{
		none = 0,
		FadeIn = 1,
		DigIn = 2,
		DigOut = 3,
		Taunt = 4,
		FadeOut = 5
	}

	public Anims anim;

	public Anims prevAnim;

	public GameObject DuneTopPrefab;

	public GameObject DuneBotPrefab;

	public GameObject TauntParticlesPrefab;

	public AnimationCurve tauntAlphaCurve;

	public AnimationCurve tauntScaleX;

	public AnimationCurve tauntScaleY;

	public AnimationCurve tauntScaleZ;

	private Vector3 initScale;

	private Vector3 currentScale;

	private GameObject DuneTopInstance;

	private GameObject DuneBotInstance;

	private Color colorStart;

	private Color colorEnd;

	private Color currentColor;

	private bool taunting;

	private bool fadingIn = true;

	private bool fadingOut;

	private bool digingIn;

	private bool digingOut;

	private float fadeVal;

	private float scaleVal;

	private bool init;

	public bool playing;

	public bool looping;

	public float duration = 1f;

	private float animTime;

	private void SpawnDune()
	{
		if (DuneBotPrefab != null && DuneBotInstance == null)
		{
			DuneBotInstance = Object.Instantiate(DuneBotPrefab, base.transform.position + new Vector3(0f, -1f, 0.3f), base.transform.rotation);
			DuneBotInstance.transform.parent = base.transform;
		}
	}

	private void UpdateDune()
	{
		if (!playing)
		{
			return;
		}
		animTime += Time.deltaTime / duration;
		if (initScale == Vector3.zero)
		{
			initScale = DuneBotInstance.transform.localScale;
		}
		switch (anim)
		{
		case Anims.FadeIn:
			scaleVal = (animTime + 1f) / 2f;
			currentScale = initScale * scaleVal;
			currentScale.y = animTime;
			if (currentScale != Vector3.zero)
			{
				DuneBotInstance.transform.localScale = currentScale;
			}
			currentColor = Color.Lerp(colorEnd, colorStart, scaleVal);
			break;
		case Anims.FadeOut:
			scaleVal = 1f - animTime / 2f;
			currentScale = initScale * scaleVal;
			if (currentScale != Vector3.zero)
			{
				DuneBotInstance.transform.localScale = currentScale;
			}
			currentColor = Color.Lerp(colorStart, colorEnd, animTime);
			break;
		case Anims.Taunt:
			if (init)
			{
				if (DuneTopPrefab != null)
				{
					DuneTopInstance = Object.Instantiate(DuneTopPrefab, base.transform.position + new Vector3(0f, -1f, 0.3f), base.transform.rotation);
					DuneTopInstance.transform.parent = base.transform;
				}
				Splash();
				Invoke("Splash", 1.5f);
				init = false;
			}
			currentScale = new Vector3(initScale.x * tauntScaleX.Evaluate(animTime), initScale.y * tauntScaleY.Evaluate(animTime), initScale.z * tauntScaleZ.Evaluate(animTime));
			if (currentScale != Vector3.zero)
			{
				DuneBotInstance.transform.localScale = currentScale;
			}
			currentColor = Color.Lerp(colorStart, colorEnd, tauntAlphaCurve.Evaluate(animTime));
			break;
		case Anims.DigIn:
			if (init)
			{
				Invoke("Splash", 0.22f);
				init = false;
			}
			scaleVal = (animTime + 4f) / 5f;
			currentScale = initScale * scaleVal;
			currentScale.y = initScale.y * (animTime + 1f) / 2f;
			if (currentScale != Vector3.zero)
			{
				DuneBotInstance.transform.localScale = currentScale;
			}
			currentColor = Color.Lerp(colorEnd, colorStart, scaleVal);
			break;
		case Anims.DigOut:
			if (init)
			{
				if (DuneTopPrefab != null)
				{
					DuneTopInstance = Object.Instantiate(DuneTopPrefab, base.transform.position + new Vector3(0f, -1f, 0.3f), base.transform.rotation);
					DuneTopInstance.transform.parent = base.transform;
					DuneTopInstance.transform.localScale *= 0.5f;
				}
				Splash();
				Invoke("Splash", 0.8f);
				init = false;
			}
			scaleVal = 1f - animTime / 2f;
			currentScale = initScale * scaleVal;
			currentScale.y = 1f - animTime;
			if (currentScale != Vector3.zero)
			{
				DuneBotInstance.transform.localScale = currentScale;
			}
			currentColor = Color.Lerp(colorEnd, colorStart, scaleVal);
			break;
		}
		DuneBotInstance.GetComponent<Renderer>().material.color = currentColor;
	}

	private void Splash()
	{
		if (TauntParticlesPrefab != null)
		{
			Vector3 position = base.transform.position;
			position.y -= 0.5f;
			Object.Instantiate(TauntParticlesPrefab, base.transform.position, Quaternion.Euler(base.transform.eulerAngles + new Vector3(-90f, 0f, 0f))).transform.parent = base.transform;
		}
	}

	private void Awake()
	{
		SpawnDune();
		colorStart = DuneBotInstance.GetComponent<Renderer>().material.color;
		colorEnd = new Color(colorStart.r, colorStart.g, colorStart.b, 0f);
	}

	private void Update()
	{
		if (animTime > 0.99f)
		{
			if (looping)
			{
				prevAnim = Anims.none;
			}
			else
			{
				anim = Anims.none;
			}
		}
		if (anim != prevAnim)
		{
			init = true;
			animTime = 0f;
			prevAnim = anim;
			if (anim == Anims.none)
			{
				playing = false;
			}
			else
			{
				playing = true;
			}
		}
		UpdateDune();
	}
}
