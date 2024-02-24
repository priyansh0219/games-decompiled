using UnityEngine;

public class MagmaParticlesMover : MonoBehaviour
{
	public float minVelocity;

	public float maxVelocity;

	public float minAngleRate;

	public float maxAngleRate;

	private Vector3 eulerAngleRate;

	private float angleX;

	private float angleZ;

	private void Update()
	{
		angleX += Random.Range(minAngleRate, maxAngleRate);
		angleZ += Random.Range(minAngleRate, maxAngleRate);
		eulerAngleRate = new Vector3(Random.Range(minAngleRate, maxAngleRate), 0f, Random.Range(minAngleRate, maxAngleRate));
		base.transform.Translate(Vector3.up * Time.deltaTime * Random.Range(minVelocity, maxVelocity));
		base.transform.rotation = Quaternion.Euler(angleX, 0f, angleZ);
	}
}
