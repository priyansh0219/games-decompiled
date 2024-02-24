using UWE;
using UnityEngine;

public class Buoyancy : MonoBehaviour
{
	public static float kForcePerVolume = 25f;

	public static float SurfaceMargin = 5f;

	public float volume = 1f;

	private Rigidbody body;

	private SubRoot sub;

	private bool debugDisabled;

	private void Awake()
	{
		DevConsole.RegisterConsoleCommand(this, "buoyoff");
		DevConsole.RegisterConsoleCommand(this, "buoyon");
	}

	private void Start()
	{
		body = base.gameObject.FindAncestor<Rigidbody>();
		sub = base.gameObject.FindAncestor<SubRoot>();
	}

	private void FixedUpdate()
	{
		if (debugDisabled || body == null || body.isKinematic)
		{
			return;
		}
		float num = base.transform.position.y - Ocean.GetOceanLevel();
		if (num < SurfaceMargin)
		{
			float num2 = Mathf.Clamp01(UWE.Utils.Unlerp(num, SurfaceMargin, 0f));
			float num3 = volume * kForcePerVolume * num2;
			Vector3 vector = Vector3.up * num3;
			body.AddForceAtPosition(vector, base.transform.position);
			if (Mathf.Abs(num3) > 0f)
			{
				Debug.DrawLine(base.transform.position, base.transform.position + vector, Color.red, 0f, depthTest: false);
				Debug.DrawLine(base.transform.position, body.worldCenterOfMass, Color.green, 0f, depthTest: false);
			}
		}
	}

	public void OnConsoleCommand_buoyon()
	{
		debugDisabled = false;
	}

	public void OnConsoleCommand_buoyoff()
	{
		debugDisabled = true;
	}
}
