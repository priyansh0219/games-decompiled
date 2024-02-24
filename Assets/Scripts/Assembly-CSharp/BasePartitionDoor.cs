using UnityEngine;

public class BasePartitionDoor : MonoBehaviour
{
	[AssertNotNull]
	public Collider collider;

	[AssertNotNull]
	public Transform door;

	public Vector3 positionClosed = new Vector3(0f, 0f, 0f);

	public Vector3 positionOpen = new Vector3(-1.5f, 0f, 0f);

	[AssertNotNull]
	public FMODAsset soundOpen;

	[AssertNotNull]
	public FMODAsset soundClose;

	private const float verticalThreshold = 1f;

	public float timeOpen = 0.2f;

	public float timeClose = 1f;

	private const float radiusIn = 1.5f;

	private const float radiusOut = 2.5f;

	private const float radiusInSqr = 2.25f;

	private const float radiusOutSqr = 6.25f;

	private bool open;

	private float start;

	private void OnEnable()
	{
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.LateUpdateAfterInput, OnUpdate);
		open = GetState();
		float time = Time.time;
		float num = (open ? timeOpen : timeClose);
		start = time - num;
		collider.enabled = !open;
		door.localPosition = (open ? positionOpen : positionClosed);
	}

	private void OnDisable()
	{
		ManagedUpdate.Unsubscribe(OnUpdate);
	}

	private void OnUpdate()
	{
		bool state = GetState();
		if (open != state)
		{
			float time = Time.time;
			float num = (open ? timeOpen : timeClose);
			float num2 = (state ? timeOpen : timeClose);
			start = time - Mathf.Max(0f, 1f - (time - start) / num) * num2;
			open = state;
			collider.enabled = !open;
			FMODUWE.PlayOneShot(open ? soundOpen : soundClose, base.transform.position);
		}
		float num3 = (open ? timeOpen : timeClose);
		float num4 = Mathf.Clamp01((Time.time - start) / num3);
		door.localPosition = Vector3.Lerp(positionClosed, positionOpen, MathExtensions.EaseOutSine(open ? num4 : (1f - num4)));
	}

	private bool GetState()
	{
		bool result = false;
		Player main = Player.main;
		if (main.currentSub != null && main.currentSub.isBase)
		{
			Vector3 vector = main.transform.position - base.transform.position;
			if (Mathf.Abs(vector.y) < 1f)
			{
				float num = vector.x * vector.x + vector.z * vector.z;
				result = ((!open) ? (num < 2.25f) : (num < 6.25f));
			}
		}
		return result;
	}
}
