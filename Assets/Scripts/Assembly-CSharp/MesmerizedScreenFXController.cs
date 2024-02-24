using UnityEngine;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(MesmerizedScreenFX))]
public class MesmerizedScreenFXController : MonoBehaviour
{
	public float fadeInDuration = 0.5f;

	public float fadeOutDuration = 0.25f;

	private MesmerizedScreenFX fx;

	private float amount;

	private bool isFadingIn;

	private bool isFadingOut;

	private void Start()
	{
		fx = GetComponent<MesmerizedScreenFX>();
		if (Player.main == null)
		{
			Object.Destroy(this);
			Object.Destroy(fx);
		}
		fx.amount = 0f;
	}

	public void StartHypnose()
	{
		isFadingIn = true;
		isFadingOut = false;
	}

	public void StopHypnose()
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
		}
		fx.amount = Mathf.Clamp01(amount);
		if (fx.amount > 0f && !fx.enabled)
		{
			fx.enabled = true;
		}
	}
}
