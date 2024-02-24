using UnityEngine;

public class Deprecate : MonoBehaviour
{
	private void Start()
	{
		Debug.LogWarningFormat(this, "destroying deprecated game object '{0}'", base.gameObject);
		Object.Destroy(base.gameObject);
	}
}
