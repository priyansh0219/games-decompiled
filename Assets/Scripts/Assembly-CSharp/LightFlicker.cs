using System.Collections;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
	public float minFlickerSpeed = 0.01f;

	public float maxFlickerSpeed = 0.1f;

	public float minLightIntensity;

	public float maxLightIntensity = 1f;

	private IEnumerator Start()
	{
		Light theLight = GetComponent<Light>();
		while (true)
		{
			theLight.enabled = true;
			theLight.intensity = Random.Range(minLightIntensity, maxLightIntensity);
			yield return new WaitForSeconds(Random.Range(minFlickerSpeed, maxFlickerSpeed));
			theLight.enabled = false;
			yield return new WaitForSeconds(Random.Range(minFlickerSpeed, maxFlickerSpeed));
		}
	}
}
