using UnityEngine;

public class OutOfBoundsWarp : MonoBehaviour
{
	private static readonly Bounds bounds = new Bounds(new Vector3(0f, 0f, 0f), new Vector3(16384f, 16384f, 16384f));

	private static readonly Bounds target = new Bounds(new Vector3(0f, 10f, 0f), new Vector3(500f, 0f, 500f));

	private Rigidbody rigidBody;

	private PlayerMotor playerMotor;

	public void Start()
	{
		rigidBody = GetComponent<Rigidbody>();
		playerMotor = GetComponent<PlayerMotor>();
	}

	public void FixedUpdate()
	{
		if (!bounds.Contains(base.transform.position) && (!base.transform.parent || !(base.transform.parent.GetComponentInParent<OutOfBoundsWarp>() != null)) && PlayerCinematicController.cinematicModeCount <= 0)
		{
			Debug.LogError($"object went out of bounds, warping; {base.gameObject}");
			Warp();
		}
	}

	public static Vector3 GetRandomWarpPoint()
	{
		Vector3 center = target.center;
		center.x += Random.Range(0f - target.extents.x, target.extents.x);
		center.y += Random.Range(0f - target.extents.y, target.extents.y);
		center.z += Random.Range(0f - target.extents.z, target.extents.z);
		return center;
	}

	public void Warp()
	{
		base.transform.position = GetRandomWarpPoint();
		base.transform.rotation = Quaternion.identity;
		rigidBody.velocity = Vector3.zero;
		rigidBody.angularVelocity = Vector3.zero;
		if ((bool)playerMotor)
		{
			playerMotor.SetVelocity(Vector3.zero);
		}
		GroundMotor[] array = Object.FindObjectsOfType<GroundMotor>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnTeleport();
		}
	}
}
