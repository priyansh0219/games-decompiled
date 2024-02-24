using UnityEngine;

public class AcidicBrineDamage : MonoBehaviour
{
	private int numTriggers;

	private LiveMixin liveMixin;

	private Player player;

	private void Start()
	{
		liveMixin = GetComponent<LiveMixin>();
		player = GetComponent<Player>();
		InvokeRepeating("ApplyDamage", 0f, 1f);
		SendMessage("OnAcidEnter", null, SendMessageOptions.DontRequireReceiver);
	}

	private void OnDestroy()
	{
		SendMessage("OnAcidExit", null, SendMessageOptions.DontRequireReceiver);
	}

	public void Increment()
	{
		numTriggers++;
	}

	public void Decrement()
	{
		numTriggers--;
		if (numTriggers <= 0)
		{
			Object.Destroy(this);
		}
	}

	private void ApplyDamage()
	{
		if ((!(player != null) || !player.cinematicModeActive) && liveMixin != null)
		{
			liveMixin.TakeDamage(10f, base.transform.position, DamageType.Acid);
		}
	}
}
