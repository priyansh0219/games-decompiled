using UnityEngine;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(RadiationsScreenFX))]
public class RadiationsScreenFXController : MonoBehaviour
{
	public float radiationMultiplier = 0.85f;

	public float minRadiation = 0.35f;

	public float fadeDuration = 0.25f;

	private float prevRadiationAmount;

	private float animTime;

	private RadiationsScreenFX fx;

	private void Start()
	{
		fx = GetComponent<RadiationsScreenFX>();
		if (Player.main == null)
		{
			Object.Destroy(this);
			Object.Destroy(GetComponent<RadiationsScreenFX>());
		}
	}

	private void Update()
	{
		if (Player.main.radiationAmount >= prevRadiationAmount && Player.main.radiationAmount > 0f)
		{
			animTime += Time.deltaTime / fadeDuration;
		}
		else
		{
			animTime -= Time.deltaTime / fadeDuration;
		}
		animTime = Mathf.Clamp01(animTime);
		fx.noiseFactor = Player.main.radiationAmount * radiationMultiplier + minRadiation * animTime;
		if (fx.noiseFactor > 0f && !fx.enabled)
		{
			fx.enabled = true;
		}
		prevRadiationAmount = Player.main.radiationAmount;
	}
}
