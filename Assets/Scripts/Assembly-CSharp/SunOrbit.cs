using UnityEngine;

[ExecuteInEditMode]
public class SunOrbit : MonoBehaviour
{
	public static SunOrbit main;

	public AnimationCurve dayNightCurve;

	public float yaw;

	public float maxAngle = 45f;

	[HideInInspector]
	public Vector3 _sunAngles = Vector3.zero;

	[HideInInspector]
	public Vector3 _lightAngles = Vector3.zero;

	[Range(0f, 1f)]
	public float dayScalar = 0.4f;

	public Vector3 sunAngles => _sunAngles;

	public Vector3 lightAngles => _lightAngles;

	private void Awake()
	{
		main = this;
	}

	private void Update()
	{
	}

	private void EvaluateAngles(float dayScalar)
	{
		float num = dayNightCurve.Evaluate(dayScalar);
		_sunAngles.x = Mathf.Lerp(-90f, 270f, num);
		_sunAngles.y = yaw;
		_sunAngles.z = 0f;
		float num2 = num;
		if (num < 0.25f)
		{
			num2 = Mathf.Clamp(num * 4f, 0f, 1f);
			_lightAngles.x = Mathf.Lerp(0f, 0f - maxAngle, num2);
		}
		else if (num > 0.75f)
		{
			num2 = Mathf.Clamp((num - 0.75f) * 4f, 0f, 1f);
			_lightAngles.x = Mathf.Lerp(maxAngle, 0f, num2);
		}
		else
		{
			num2 = Mathf.Clamp(num * 2f - 0.5f, 0f, 1f);
			_lightAngles.x = Mathf.Lerp(0f - maxAngle, maxAngle, num2);
		}
		_lightAngles.x += 90f;
		_lightAngles.y = yaw;
		_lightAngles.z = 0f;
		base.transform.eulerAngles = _lightAngles;
	}
}
