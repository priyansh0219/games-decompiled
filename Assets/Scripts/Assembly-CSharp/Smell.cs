using UnityEngine;

public class Smell : MonoBehaviour
{
	public GameObject owner;

	public float strength;

	public float falloff;

	public void Update()
	{
		strength -= Time.deltaTime * falloff;
		if (strength < 0f)
		{
			Object.Destroy(this);
		}
	}
}
