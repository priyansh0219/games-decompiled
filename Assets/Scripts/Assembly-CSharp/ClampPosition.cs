using UnityEngine;

public class ClampPosition : MonoBehaviour
{
	public bool enableMaxY;

	public float maxY;

	private void FixedUpdate()
	{
		Vector3 position = base.transform.position;
		if (enableMaxY)
		{
			position.y = Mathf.Min(position.y, maxY);
		}
		base.transform.position = position;
	}
}
