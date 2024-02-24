using UnityEngine;

public class OneScale : MonoBehaviour
{
	private void Start()
	{
		Transform parent = base.transform.parent;
		base.transform.parent = null;
		base.transform.localScale = Vector3.one;
		base.transform.parent = parent;
	}
}
