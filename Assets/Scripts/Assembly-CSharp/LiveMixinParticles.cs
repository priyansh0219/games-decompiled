using UnityEngine;

public class LiveMixinParticles : MonoBehaviour
{
	public GameObject particleParent;

	public LiveMixin liveMixin;

	public bool particlesEnabled = true;

	private void OnEnable()
	{
		liveMixin.onHealDamage.AddHandler(base.gameObject, CheckHealth);
		CheckHealth(0f);
	}

	private void CheckHealth(float damage)
	{
		float num = liveMixin.health / liveMixin.maxHealth;
		if (Mathf.Approximately(num, 1f) && particlesEnabled)
		{
			Debug.Log("Disabling particle systems");
			ParticleSystem[] componentsInChildren = particleParent.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Stop(withChildren: true);
			}
			particlesEnabled = false;
		}
		if (num < 0.5f && !particlesEnabled)
		{
			Debug.Log("Enabling particle systems");
			ParticleSystem[] componentsInChildren2 = particleParent.GetComponentsInChildren<ParticleSystem>();
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				componentsInChildren2[j].Play(withChildren: true);
			}
			particlesEnabled = true;
		}
	}
}
