using UnityEngine;

public class TeleportReporter : MonoBehaviour
{
	public enum Mode
	{
		Update = 0,
		LateUpdate = 1,
		FixedUpdate = 2
	}

	public Transform oldParent;

	public Vector3 oldPosition;

	public float threshold = 1f;

	public Mode mode;

	private void Detect()
	{
		Transform parent = base.transform.parent;
		Vector3 position = base.transform.position;
		float num = Vector3.Distance(position, oldPosition);
		if (num > threshold)
		{
			Debug.LogWarningFormat(this, "teleport detected at frame {0} from {1} to {2}, distance {3} (parent '{4}' to '{5}'), {6}", Time.frameCount, oldPosition, position, num, oldParent, parent, mode);
		}
		oldParent = parent;
		oldPosition = position;
	}

	private void Update()
	{
		if (mode == Mode.Update)
		{
			Detect();
		}
	}

	private void LateUpdate()
	{
		if (mode == Mode.LateUpdate)
		{
			Detect();
		}
	}

	private void FixedUpdate()
	{
		if (mode == Mode.FixedUpdate)
		{
			Detect();
		}
	}
}
