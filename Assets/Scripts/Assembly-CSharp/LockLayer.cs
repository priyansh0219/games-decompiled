using UnityEngine;

public class LockLayer : MonoBehaviour
{
	private int layer;

	private void Awake()
	{
		layer = base.gameObject.layer;
	}

	private void LateUpdate()
	{
		if (base.gameObject.layer != layer)
		{
			base.gameObject.layer = layer;
		}
	}
}
