using UnityEngine;

public class OnlyInEditor : MonoBehaviour
{
	private void Start()
	{
		if (!Application.isEditor)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
