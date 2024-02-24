using UnityEngine;

[RequireComponent(typeof(LiveMixin))]
public class SnoozeBall : MonoBehaviour, IOnTakeDamage
{
	public ParticleSystem takeDamagePrefab;

	public ParticleSystem deathPrefab;

	public GameObject emitPoint;

	public FMOD_StudioEventEmitter gasSound;

	public FMOD_StudioEventEmitter deathSound;

	private void PlayParticles(ParticleSystem prefab)
	{
		if ((bool)prefab)
		{
			GameObject obj = Object.Instantiate(prefab.gameObject, emitPoint.transform.position, Quaternion.identity);
			ParticleSystem component = obj.GetComponent<ParticleSystem>();
			component.Play();
			Object.Destroy(obj, component.duration);
		}
	}

	public void OnTakeDamage(DamageInfo damageInfo)
	{
		PlayParticles(takeDamagePrefab);
	}

	private void OnKill()
	{
		PlayParticles(deathPrefab);
	}
}
