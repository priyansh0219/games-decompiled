using UWE;
using UnityEngine;

public class FreecamController : MonoBehaviour
{
	private const float speed1 = 0.4f;

	private const float speed2 = 2f;

	private const float speed3 = 4f;

	private const float speed4 = 8f;

	private const float speed5 = 64f;

	private Transform parent;

	private bool mode;

	private GameObject inputDummy;

	private bool ghostMode;

	private float speed = 0.4f;

	private Transform tr;

	private float t;

	private bool toggleNextFrame;

	public bool GetActive()
	{
		return mode;
	}

	private void Awake()
	{
		tr = GetComponent<Transform>();
		t = Time.realtimeSinceStartup;
		inputDummy = new GameObject("FreeCameraInputDummy");
		inputDummy.SetActive(value: false);
		DevConsole.RegisterConsoleCommand(this, "freecam");
		DevConsole.RegisterConsoleCommand(this, "ghost");
	}

	private void Update()
	{
		if (toggleNextFrame)
		{
			FreecamToggle();
			toggleNextFrame = false;
		}
		if (inputDummy.activeSelf)
		{
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				speed = 0.4f;
			}
			else if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				speed = 2f;
			}
			else if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				speed = 4f;
			}
			else if (Input.GetKeyDown(KeyCode.Alpha4))
			{
				speed = 8f;
			}
			else if (Input.GetKeyDown(KeyCode.Alpha5))
			{
				speed = 64f;
			}
			if (Input.GetMouseButtonDown(0))
			{
				UWE.Utils.lockCursor = true;
			}
			if (mode && UWE.Utils.lockCursor)
			{
				Vector3 moveDirection = GameInput.GetMoveDirection();
				float num = Time.realtimeSinceStartup - t;
				t = Time.realtimeSinceStartup;
				tr.position += tr.TransformDirection(moveDirection * speed * (Input.GetKey(KeyCode.LeftShift) ? 2f : 1f) * num);
				Vector2 lookDelta = GameInput.GetLookDelta();
				tr.localRotation = Quaternion.AngleAxis(0f - lookDelta.y, tr.right) * tr.localRotation;
				tr.localRotation = Quaternion.AngleAxis(lookDelta.x, (tr.up.y > 0f) ? Vector3.up : Vector3.down) * tr.localRotation;
			}
		}
	}

	private void FreecamToggle()
	{
		mode = !mode;
		if (mode)
		{
			parent = tr.parent;
			tr.SetParent(null, worldPositionStays: true);
			MainCameraControl component = base.gameObject.GetComponent<MainCameraControl>();
			if (component != null)
			{
				component.enabled = false;
			}
			InputHandlerStack.main.Push(inputDummy);
			Screen.lockCursor = true;
			Player.main.EnterLockedMode(null);
			return;
		}
		Vector3 position = tr.position;
		InputHandlerStack.main.Pop(inputDummy);
		tr.SetParent(parent, worldPositionStays: false);
		MainCameraControl component2 = base.gameObject.GetComponent<MainCameraControl>();
		if (component2 != null)
		{
			component2.enabled = true;
		}
		if (ghostMode)
		{
			Player.main.SetPosition(position);
		}
		Player.main.ExitLockedMode(respawn: false, findNewPosition: false);
	}

	private void OnConsoleCommand_freecam()
	{
		toggleNextFrame = true;
		ghostMode = false;
	}

	private void OnConsoleCommand_ghost()
	{
		speed = 8f;
		toggleNextFrame = true;
		ghostMode = true;
	}
}
