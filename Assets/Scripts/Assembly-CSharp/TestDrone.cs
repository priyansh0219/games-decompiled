using UnityEngine;

public class TestDrone : MonoBehaviour, IEcoEventHandler
{
	public float lightSenseRange;

	private void Start()
	{
	}

	public EcoEventType GetEventType()
	{
		return EcoEventType.Light;
	}

	public float GetRange()
	{
		return lightSenseRange;
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	public string GetName()
	{
		return base.name;
	}

	public void OnEcoEvent(EcoEvent e)
	{
		GetComponent<Renderer>().material.color = Color.green;
	}
}
