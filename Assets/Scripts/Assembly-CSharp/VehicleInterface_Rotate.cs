using UnityEngine;

public class VehicleInterface_Rotate : MonoBehaviour
{
	public float rotationSpeed = 1f;

	private void Update()
	{
		base.transform.Rotate(new Vector3(0f, 0f, rotationSpeed * Time.deltaTime), Space.Self);
	}
}
