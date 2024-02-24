using UnityEngine;

public class PrecursorGunTarget : MonoBehaviour
{
	public float leadingInterval = 10f;

	private Vector3 lastPos;

	private Transform proxyTarget;

	private void Start()
	{
		GameObject gameObject = new GameObject("TargetProxy");
		proxyTarget = gameObject.transform;
		PrecursorGunStoryEvents main = PrecursorGunStoryEvents.main;
		if ((bool)main && (bool)main.gunAim)
		{
			main.gunAim.SetStartingRotation();
			main.gunAim.target = proxyTarget;
		}
	}

	private void OnDestroy()
	{
		proxyTarget.position = base.transform.position;
		Object.Destroy(proxyTarget.gameObject, 30f);
	}

	private void Update()
	{
		if ((bool)proxyTarget && !(Time.deltaTime <= 0f))
		{
			Vector3 position = base.transform.position;
			Vector3 vector = (position - lastPos) / Time.deltaTime;
			lastPos = position;
			proxyTarget.position = position + vector * leadingInterval;
		}
	}
}
