using UnityEngine;

public class Swivel : MonoBehaviour
{
	public float limitDegrees = 30f;

	public float xTarget;

	public float yTarget;

	private float ClampValue(float value)
	{
		if (Mathf.Abs(value) > limitDegrees)
		{
			value = Mathf.Sign(value) * limitDegrees;
		}
		return value;
	}

	public void Update()
	{
		float x = ClampValue(xTarget);
		float y = ClampValue(yTarget);
		base.transform.localEulerAngles = new Vector3(x, y, 0f);
	}

	public void Rotate(float xDiffDegrees, float yDiffDegrees)
	{
		xTarget = ClampValue(base.transform.localEulerAngles.x + xDiffDegrees);
		yTarget = ClampValue(base.transform.localEulerAngles.y + yDiffDegrees);
	}
}
