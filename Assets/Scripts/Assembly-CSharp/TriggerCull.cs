using UWE;
using UnityEngine;

public class TriggerCull : MonoBehaviour
{
	[SerializeField]
	[AssertNotNull]
	private SphereCollider trigger;

	[SerializeField]
	[AssertNotNull]
	private GameObject objectToCull;

	private float updateInterval = 0.25f;

	private float timeNextUpdate;

	private void Update()
	{
		if (!(Time.time < timeNextUpdate))
		{
			timeNextUpdate = Time.time + updateInterval;
			if (Player.main != null)
			{
				bool active = UWE.Utils.IsInsideCollider(trigger, Player.main.transform.position);
				objectToCull.SetActive(active);
			}
		}
	}
}
