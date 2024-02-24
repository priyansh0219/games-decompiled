using UnityEngine;

public class ConstructorExit : MonoBehaviour
{
	public GameObject ConstructPrefabAtExit(GameObject prefab)
	{
		GameObject obj = Utils.SpawnZeroedAt(prefab, base.transform);
		obj.transform.parent = null;
		return obj;
	}
}
