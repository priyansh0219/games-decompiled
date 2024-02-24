using UnityEngine;

public class AmbientParticles : MonoBehaviour
{
	private Player player;

	private Rigidbody playerRB;

	private Transform playerTrans;

	public float maxOffset = 10f;

	[AssertNotNull]
	public Transform particlesTrans;

	[AssertNotNull]
	public ParticleSystem particles;

	public int amount = 1000;

	public Color color1;

	public Color color2;

	private void StartPlaying()
	{
		if (!particles.isPlaying)
		{
			particles.Play();
		}
	}

	private void StopPlaying()
	{
		if (particles.isPlaying)
		{
			particles.Stop();
			particles.Clear();
		}
	}

	private void Init()
	{
		player = Utils.GetLocalPlayerComp();
		if (player != null)
		{
			playerTrans = player.transform;
			playerRB = player.GetComponent<Rigidbody>();
		}
	}

	private void Update()
	{
		if (!player)
		{
			Init();
			return;
		}
		if (!player.displaySurfaceWater || player.precursorOutOfWater)
		{
			StopPlaying();
			return;
		}
		StartPlaying();
		Vector3 velocity = playerRB.velocity;
		particlesTrans.position = playerTrans.position + Vector3.ClampMagnitude(velocity, maxOffset);
		Vector3 forward = playerTrans.position - particlesTrans.position;
		if (forward.magnitude > 0.001f)
		{
			Quaternion rotation = Quaternion.LookRotation(forward);
			particlesTrans.rotation = rotation;
		}
	}
}
