using UnityEngine;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(EndSequenceWarpScreenFX))]
public class EndSequenceWarpScreenFXController : MonoBehaviour
{
	public float fadeInDuration = 0.25f;

	public float duration = 10f;

	public float fadeOutDuration = 0.25f;

	private EndSequenceWarpScreenFX fx;

	private float amount;

	private float animTime;

	private bool isFadingIn;

	private bool isFadingOut;

	private void Start()
	{
		fx = GetComponent<EndSequenceWarpScreenFX>();
		if (Player.main == null)
		{
			Object.Destroy(this);
			Object.Destroy(fx);
		}
		fx.amount = 0f;
	}

	public void StartWarp()
	{
		isFadingIn = true;
		isFadingOut = false;
		animTime = 0f;
	}

	public void StopWarp()
	{
		isFadingIn = false;
		isFadingOut = true;
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
			animTime = -1f;
		}
		else if (animTime > -1f)
		{
			animTime += Time.deltaTime / duration;
			isFadingOut = animTime > 1f;
		}
		fx.amount = Mathf.Clamp01(amount);
		if (fx.amount > 0f && !fx.enabled)
		{
			fx.enabled = true;
		}
	}
}
