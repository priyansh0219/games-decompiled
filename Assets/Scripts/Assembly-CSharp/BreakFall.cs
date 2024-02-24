using UWE;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(LiveMixin))]
public class BreakFall : MonoBehaviour, IOnTakeDamage
{
	public GameObject damageParticlePrefab;

	public FMOD_StudioEventEmitter killSoundEvent;

	private void Awake()
	{
		UWE.Utils.SetIsKinematicAndUpdateInterpolation(base.gameObject.GetComponent<Rigidbody>(), isKinematic: true);
	}

	public void OnTakeDamage(DamageInfo damageInfo)
	{
		if ((bool)damageParticlePrefab)
		{
			Utils.PlayOneShotPS(damageParticlePrefab, base.transform.position, base.transform.rotation);
		}
	}

	private void OnKill()
	{
		UWE.Utils.SetIsKinematicAndUpdateInterpolation(GetComponent<Rigidbody>(), isKinematic: false);
		base.transform.parent = null;
		if ((bool)killSoundEvent)
		{
			Utils.PlayEnvSound(killSoundEvent);
		}
	}
}
