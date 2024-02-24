using UnityEngine;

public class VirtualEntitiesManager : MonoBehaviour
{
	public GameObject virtualEntityPrefab;

	public static VirtualEntitiesManager main;

	private void Start()
	{
		main = this;
	}

	public static GameObject GetVirtualEntityPrefab()
	{
		return main.virtualEntityPrefab;
	}
}
