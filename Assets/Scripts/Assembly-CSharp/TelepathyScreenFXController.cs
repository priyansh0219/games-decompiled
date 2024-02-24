using UnityEngine;

public class TelepathyScreenFXController : MonoBehaviour
{
	[AssertNotNull]
	public TelepathyScreenFX fx;

	public float fadeInDuration = 0.5f;

	public float fadeOutDuration = 0.25f;

	private float amount;

	private bool isFadingIn;

	private bool isFadingOut;

	private float currentRandomValue;

	private void Start()
	{
		if (Player.main == null)
		{
			Object.Destroy(this);
			Object.Destroy(fx);
		}
		fx.amount = 0f;
		fx.isFinalSequence = false;
	}

	public void StartFinalTelepathy()
	{
		fx.isFinalSequence = true;
		StartTelepathy(showModel: true);
	}

	public void StartTelepathy(bool showModel)
	{
		isFadingIn = true;
		isFadingOut = false;
		fx.showGhostModel = showModel;
	}

	public void StopTelepathy()
	{
		isFadingIn = false;
		isFadingOut = true;
		fx.isFinalSequence = false;
	}

	private void Update()
	{
		if (isFadingIn)
		{
			amount += Time.deltaTime / fadeInDuration;
			isFadingIn = amount < 1f;
		}
		else if (isFadingOut)
		{
			amount -= Time.deltaTime / fadeOutDuration;
			isFadingOut = amount > 0f;
		}
		if (amount > 0f)
		{
			fx.amount = Mathf.Clamp01(amount * Mathf.Pow(GetSoundVolume(), 0.75f));
		}
		else
		{
			fx.amount = 0f;
		}
		if (fx.amount > 0f && !fx.enabled)
		{
			fx.enabled = true;
		}
	}

	private float GetSoundVolume()
	{
		float meteringVolume = FMODUWE.GetMeteringVolume();
		float masterVolume = SoundSystem.GetMasterVolume();
		if (masterVolume > 0f)
		{
			return meteringVolume / masterVolume;
		}
		currentRandomValue = Mathf.MoveTowards(currentRandomValue, Random.value, Time.deltaTime);
		return currentRandomValue;
	}
}
