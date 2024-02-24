using UnityEngine;

public class SandPile : MonoBehaviour
{
	public float obscureAmount = 1f;

	public float removeMin = 0.3f;

	public float removeMax = 0.5f;

	public float initialY;

	public GameObject digParticleTemplate;

	private void Start()
	{
		initialY = base.transform.position.y;
	}

	public void OnInteract(Vector3 position)
	{
		Dig(position);
	}

	public void Dig(Vector3 hitPosition)
	{
		if (!Utils.NearlyEqual(obscureAmount, 0f))
		{
			float num = removeMin + (removeMax - removeMin) * Random.value;
			obscureAmount = Mathf.Clamp01(obscureAmount - num);
			Utils.PlayOneShotPS(digParticleTemplate, hitPosition, MainCamera.camera.transform.rotation);
			if (Utils.NearlyEqual(obscureAmount, 0f))
			{
				Uncover();
			}
		}
	}

	private void Uncover()
	{
		Object.Destroy(base.gameObject);
	}

	private void Update()
	{
	}
}
