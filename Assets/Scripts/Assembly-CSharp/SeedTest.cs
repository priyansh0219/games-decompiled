using UnityEngine;

public class SeedTest : MonoBehaviour
{
	public GameObject kSeedPrefab;

	public float kInitialSeedVelocity = 0.05f;

	public float kInitialSeedTorque = 0.1f;

	private bool firedSinceLastMouseUp;

	public void Trigger(Vector3 position, Vector3 forceDir)
	{
		GameObject obj = Object.Instantiate(kSeedPrefab, position, Utils.GetRandomYawQuat());
		forceDir = new Vector3(forceDir.x, -0.1f, forceDir.z);
		forceDir.Normalize();
		Vector3 force = forceDir * kInitialSeedVelocity;
		ConstantForce component = obj.GetComponent<ConstantForce>();
		component.force = force;
		component.relativeTorque = Vector3.up * Random.value * kInitialSeedTorque;
	}

	public void Update()
	{
		bool buttonHeld = GameInput.GetButtonHeld(GameInput.Button.Exit);
		if (buttonHeld && !firedSinceLastMouseUp)
		{
			Trigger(MainCamera.camera.transform.position + MainCamera.camera.transform.forward * 1f, MainCamera.camera.transform.forward);
			firedSinceLastMouseUp = true;
		}
		if (!buttonHeld)
		{
			firedSinceLastMouseUp = false;
		}
	}
}
