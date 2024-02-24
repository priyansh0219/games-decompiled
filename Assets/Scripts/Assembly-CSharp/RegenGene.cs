using UnityEngine;

public class RegenGene : Gene
{
	private static string kRegenerateEffectPrefabName = "WorldEntities/VFX/xUnderWaterBubblePop";

	private static float kSecondsForFullHeal = 10f;

	private static float kHealInterval = 2f;

	private LiveMixin liveMixin;

	private void Start()
	{
		liveMixin = base.gameObject.GetComponent<LiveMixin>();
		if ((bool)liveMixin)
		{
			InvokeRepeating("Regenerate", 0.1f, kHealInterval);
		}
	}

	private void Regenerate()
	{
		if ((bool)liveMixin && base.gameObject.activeInHierarchy)
		{
			float healthBack = base.Scalar * liveMixin.maxHealth * (kHealInterval / kSecondsForFullHeal);
			if (liveMixin.AddHealth(healthBack) > float.Epsilon)
			{
				StartCoroutine(Utils.PlayOneShotPSAsync(kRegenerateEffectPrefabName, base.gameObject.transform.position, Quaternion.identity));
			}
		}
	}
}
