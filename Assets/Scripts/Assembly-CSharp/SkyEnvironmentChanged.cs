using UnityEngine;

public static class SkyEnvironmentChanged
{
	public struct Parameters
	{
		public GameObject environment;
	}

	public static void Send(GameObject target, Component environmnent)
	{
		Send(target, (environmnent != null) ? environmnent.gameObject : null);
	}

	public static void Broadcast(GameObject target, Component environmnent)
	{
		Broadcast(target, (environmnent != null) ? environmnent.gameObject : null);
	}

	public static void Send(GameObject target, GameObject environmnent)
	{
		Parameters parameters = default(Parameters);
		parameters.environment = environmnent;
		SkyApplier[] components = target.GetComponents<SkyApplier>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].OnEnvironmentChanged(parameters);
		}
	}

	public static void Broadcast(GameObject target, GameObject environmnent)
	{
		Parameters parameters = default(Parameters);
		parameters.environment = environmnent;
		SkyApplier[] componentsInChildren = target.GetComponentsInChildren<SkyApplier>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].OnEnvironmentChanged(parameters);
		}
	}
}
