using UnityEngine;

public class Blinker : MonoBehaviour
{
	public Light blinkerLight;

	public float soundTime;

	public float baseLightDelay;

	private void Awake()
	{
		baseLightDelay += Random.value * 0.5f;
	}

	private void Update()
	{
		if (!blinkerLight.gameObject.activeInHierarchy && Time.time > soundTime + baseLightDelay)
		{
			blinkerLight.gameObject.SetActive(value: true);
		}
	}
}
