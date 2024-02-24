using UnityEngine;

public static class GetComponentExtensions
{
	public static T GetComponentInParent<T>(this GameObject gameObject, bool includeInactive) where T : Object
	{
		if (!includeInactive)
		{
			return gameObject.GetComponentInParent<T>();
		}
		return GetComponentInParentIncludeInactive<T>(gameObject.transform);
	}

	public static T GetComponentInParent<T>(this Component component, bool includeInactive) where T : Object
	{
		if (!includeInactive)
		{
			return component.GetComponentInParent<T>();
		}
		return GetComponentInParentIncludeInactive<T>(component.transform);
	}

	private static T GetComponentInParentIncludeInactive<T>(Transform transform) where T : Object
	{
		while ((bool)transform)
		{
			T component = transform.GetComponent<T>();
			if ((bool)component)
			{
				return component;
			}
			transform = transform.parent;
		}
		return null;
	}
}
