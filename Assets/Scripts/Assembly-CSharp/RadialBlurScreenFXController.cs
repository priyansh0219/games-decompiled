using UnityEngine;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(RadialBlurScreenFX))]
public class RadialBlurScreenFXController : MonoBehaviour
{
	private RadialBlurScreenFX fx;

	public float amount;

	private void Start()
	{
		fx = GetComponent<RadialBlurScreenFX>();
		if (Player.main == null)
		{
			Object.Destroy(this);
			Object.Destroy(fx);
		}
		fx.amount = 0f;
	}

	public void SetAmount(float val)
	{
		amount = val;
	}

	private void Update()
	{
		fx.amount = Mathf.Clamp01(amount);
		if (fx.amount > 0f && !fx.enabled)
		{
			fx.enabled = true;
		}
	}
}
