using UnityEngine;

public class RadiationLeak : MonoBehaviour
{
	public GameObject model;

	private LiveMixin liveMixin;

	private void Start()
	{
		liveMixin = base.gameObject.GetComponent<LiveMixin>();
		UpdateLeak();
		LeakingRadiation.main.RegisterLeak(this);
		liveMixin.onHealDamage.AddHandler(base.gameObject, OnHealDamage);
		InvokeRepeating("UpdateLeak", 1f, 1f);
	}

	public bool IsLeaking()
	{
		return liveMixin.GetHealthFraction() < 1f;
	}

	private void UpdateLeak()
	{
		model.SetActive(IsLeaking());
	}

	public void OnHealDamage(float damage)
	{
		UpdateLeak();
		if (damage > 0f && !IsLeaking())
		{
			LeakingRadiation.main.NotifyLeaksFixed();
		}
	}
}
