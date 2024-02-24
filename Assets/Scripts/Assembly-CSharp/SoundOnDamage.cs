using UnityEngine;

public class SoundOnDamage : MonoBehaviour, IOnTakeDamage
{
	public DamageType damageType;

	[AssertNotNull]
	public FMODAsset sound;

	public void OnTakeDamage(DamageInfo damageInfo)
	{
		if (damageInfo.damage > 0f && damageInfo.type == damageType)
		{
			Vector3 position = ((damageInfo.position == default(Vector3)) ? base.transform.position : damageInfo.position);
			float soundRadiusObsolete = Mathf.Clamp01(damageInfo.damage / 100f);
			Utils.PlayFMODAsset(sound, position, soundRadiusObsolete);
		}
	}
}
