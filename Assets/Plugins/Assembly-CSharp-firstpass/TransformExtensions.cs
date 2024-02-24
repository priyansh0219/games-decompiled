using UnityEngine;

public static class TransformExtensions
{
	public static T[] GetAllComponentsInChildren<T>(this Component comp) where T : Component
	{
		return comp.GetComponentsInChildren<T>(includeInactive: true);
	}

	public static T[] GetAllComponentsInChildren<T>(this GameObject go) where T : Component
	{
		return go.GetComponentsInChildren<T>(includeInactive: true);
	}
}
