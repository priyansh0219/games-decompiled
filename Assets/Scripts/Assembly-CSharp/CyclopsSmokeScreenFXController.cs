using UnityEngine;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(CyclopsSmokeScreenFX))]
public class CyclopsSmokeScreenFXController : MonoBehaviour
{
	public float intensity;

	public AnimationCurve intensityRemapCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	private CyclopsSmokeScreenFX fx;

	private void Start()
	{
		fx = GetComponent<CyclopsSmokeScreenFX>();
	}

	private void Update()
	{
		if (!(fx != null))
		{
			return;
		}
		if (Player.main == null)
		{
			fx.enabled = false;
			return;
		}
		SubRoot currentSub = Player.main.GetCurrentSub();
		if (currentSub == null)
		{
			fx.enabled = false;
			return;
		}
		if (!currentSub.isCyclops)
		{
			fx.enabled = false;
			return;
		}
		fx.intensity = intensityRemapCurve.Evaluate(intensity);
		if (fx.intensity <= 0.0001f)
		{
			fx.enabled = false;
			return;
		}
		fx.parentTransform = currentSub.transform;
		if (fx.parentTransform != null && fx.intensity > 0f && !fx.enabled)
		{
			fx.enabled = true;
		}
	}
}
