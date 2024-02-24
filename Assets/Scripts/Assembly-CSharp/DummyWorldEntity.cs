using UnityEngine;

public class DummyWorldEntity : MonoBehaviour
{
	private void Awake()
	{
		DevConsole.RegisterConsoleCommand(this, "dummy");
	}

	private void OnConsoleCommand_dummy(NotificationCenter.Notification n)
	{
		switch ((string)n.data[0])
		{
		case "rot":
			base.transform.Rotate(new Vector3(0f, 0f, 15f));
			break;
		case "move":
			base.transform.position += 16f * Vector3.right;
			break;
		case "clone":
			Object.Instantiate(base.gameObject, base.transform.position + Vector3.right * 2f, base.transform.rotation);
			break;
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
