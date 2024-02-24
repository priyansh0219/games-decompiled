using System.Collections.Generic;
using UnityEngine;

public class EcoEventFeedback : MonoBehaviour
{
	public GameObject debugSpherePrefab;

	public GameObject shinyParticleFXPrefab;

	public GameObject meatParticleFXPrefab;

	public GameObject vegetableParticleFXPrefab;

	public GameObject motionParticleFXPrefab;

	public GameObject lightFXPrefab;

	public float swapCountersInterval = 1f;

	private float timeNextSwapCounters;

	public Dictionary<EcoEventType, int> eventCounters = new Dictionary<EcoEventType, int>();

	public Dictionary<EcoEventType, int> eventCountersLast = new Dictionary<EcoEventType, int>();

	[HideInInspector]
	public int numSwapCounters;

	public void AddEventFeedback(EcoEvent e)
	{
		if (e.debug)
		{
			CreateSphere(e);
		}
	}

	private void CreateFromPrefab(EcoEvent e, GameObject prefab, float duration)
	{
		if (e.debug)
		{
			Debug.Log(string.Concat("EcoEventFeedback - Creating ", prefab.name, " for event ", e, " (will destroy in: ", duration, ")"));
		}
		GameObject obj = Object.Instantiate(prefab);
		obj.GetComponent<ParticleSystem>();
		obj.transform.position = e.GetPosition();
		obj.transform.parent = base.transform;
		Object.Destroy(obj, duration);
	}

	private void CreateLightFromPrefab(EcoEvent e)
	{
		GameObject obj = Object.Instantiate(lightFXPrefab);
		obj.transform.position = e.GetPosition();
		obj.GetComponent<LightCircle>().ecoEvent = e;
	}

	private void CreateSphere(EcoEvent e)
	{
		GameObject obj = Object.Instantiate(debugSpherePrefab);
		obj.transform.position = e.GetPosition();
		obj.GetComponent<ScaleDie>().ecoEvent = e;
	}
}
