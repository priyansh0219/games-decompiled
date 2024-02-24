using UnityEngine;

public class CyclopsCameraInput : MonoBehaviour
{
	public float rotationSpeedDamper = 3f;

	private Vector3 startRot;

	private Vector3 rotAngle = Vector3.zero;

	private Vector2 constraintsX = new Vector2(-45f, 45f);

	private Vector2 constraintsY = new Vector2(-65f, 65f);

	private Light cameraLight;

	private void Start()
	{
		startRot = base.transform.localRotation.eulerAngles;
	}

	public void ActivateCamera(Light light = null)
	{
		if ((bool)light)
		{
			cameraLight = light;
		}
	}

	public void DeactivateCamera()
	{
		cameraLight = null;
	}

	public void HandleInput()
	{
		Vector2 vector = GameInput.GetLookDelta() / rotationSpeedDamper;
		Vector3 vector2 = new Vector3(0f - vector.y, vector.x, 0f);
		rotAngle += vector2;
		float x = Mathf.Clamp(rotAngle.x, constraintsX.x, constraintsX.y);
		float y = rotAngle.y;
		rotAngle = new Vector3(x, y, 0f);
		base.transform.localRotation = Quaternion.Euler(startRot + rotAngle);
		uGUI_CameraCyclops.main.SetDirection(rotAngle.y);
		if ((bool)cameraLight)
		{
			cameraLight.transform.position = base.transform.position;
			cameraLight.transform.rotation = base.transform.rotation;
		}
		if (GameInput.GetButtonDown(GameInput.Button.AutoMove))
		{
			GameInput.AutoMove = !GameInput.AutoMove;
		}
	}
}
