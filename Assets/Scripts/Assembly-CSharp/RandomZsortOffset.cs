using UnityEngine;

public class RandomZsortOffset : MonoBehaviour
{
	public int sortOffsetFrom;

	public int sortOffsetTo;

	public bool destroyMaterial;

	private void Start()
	{
		int renderQueue = GetComponent<Renderer>().material.renderQueue;
		GetComponent<Renderer>().material.renderQueue = renderQueue + Random.Range(sortOffsetFrom, sortOffsetTo);
	}

	private void OnDestroy()
	{
		if (destroyMaterial)
		{
			Object.DestroyImmediate(GetComponent<Renderer>().material);
		}
	}
}
