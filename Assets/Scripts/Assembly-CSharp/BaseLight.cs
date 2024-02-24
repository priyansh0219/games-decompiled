using UnityEngine;

public class BaseLight : MonoBehaviour
{
	public MultiStatesLight[] lights;

	private LightingController lightControl;

	private void Start()
	{
		LightingController componentInParent = GetComponentInParent<LightingController>();
		if (componentInParent == null)
		{
			Debug.LogError("BaseLight without a LightingController");
		}
		else
		{
			componentInParent.RegisterLights(lights);
		}
	}

	private void OnDestroy()
	{
		if (lightControl != null)
		{
			lightControl.UnregisterLights(lights);
		}
	}
}
