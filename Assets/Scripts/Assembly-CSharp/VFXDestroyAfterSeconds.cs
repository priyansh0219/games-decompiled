using UWE;
using UnityEngine;

public class VFXDestroyAfterSeconds : MonoBehaviour
{
	public float lifeTime;

	public bool destroyMaterials;

	private float timeleft;

	private void OnEnable()
	{
		timeleft = lifeTime;
	}

	private void LateUpdate()
	{
		timeleft -= Time.deltaTime;
		if (timeleft <= 0f)
		{
			UWE.Utils.DestroyWrap(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		if (destroyMaterials)
		{
			Object.DestroyImmediate(GetComponent<Renderer>().material);
		}
	}
}
