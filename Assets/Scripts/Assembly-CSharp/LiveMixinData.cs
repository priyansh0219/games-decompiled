using UnityEngine;

[CreateAssetMenu(fileName = "LiveMixinData.asset", menuName = "Subnautica/Create LiveMixin Data Asset")]
public class LiveMixinData : ScriptableObject
{
	public float maxHealth = 100f;

	public float minDamageForSound;

	public float loopEffectBelowPercent;

	public GameObject damageEffect;

	public GameObject deathEffect;

	public GameObject electricalDamageEffect;

	public GameObject loopingDamageEffect;

	public bool destroyOnDeath;

	public bool weldable;

	public bool knifeable = true;

	public bool canResurrect;

	public bool passDamageDataOnDeath;

	public bool broadcastKillOnDeath = true;

	public bool invincibleInCreative;
}
