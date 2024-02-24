using UnityEngine;

public class IntroLifepodHealthState : MonoBehaviour
{
	public LiveMixin liveMixin;

	private float healthPercent;

	private void Start()
	{
		healthPercent = liveMixin.GetHealthFraction();
		InvokeRepeating("CheckHealthChange", 1f, 1f);
	}

	private void CheckHealthChange()
	{
		if ((bool)liveMixin)
		{
			float healthFraction = liveMixin.GetHealthFraction();
			if (Mathf.Approximately(healthFraction, 1f))
			{
				CancelInvoke();
			}
			if (healthFraction != healthPercent)
			{
				SendMessageUpwards("UpdateLifepodHealthState", healthFraction, SendMessageOptions.RequireReceiver);
				healthPercent = healthFraction;
			}
		}
	}
}
