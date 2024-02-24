using UnityEngine;

public class ModelPlugAwake : ModelPlug
{
	public GameObject socket;

	private void Awake()
	{
		ModelPlug.PlugIntoSocket(base.gameObject.GetComponent<ModelPlug>(), socket.transform);
	}
}
