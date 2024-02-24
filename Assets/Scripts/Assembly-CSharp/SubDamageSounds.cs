using UnityEngine;

public class SubDamageSounds : MonoBehaviour
{
	public float distFromPlayer;

	private float subStrengthLightDamageLimit = 0.15f;

	private float subStrengthMediumDamageLimit = 0.3f;

	public void Play(SubRoot.DamageEvent ev)
	{
		(ev.victim.gameObject.transform.position - MainCamera.camera.transform.position).Normalize();
		float amount = ev.amount;
		if ((!(amount > 0f) || !(amount <= subStrengthLightDamageLimit)) && (!(amount > subStrengthLightDamageLimit) || !(amount <= subStrengthMediumDamageLimit)))
		{
			_ = subStrengthMediumDamageLimit;
		}
	}
}
