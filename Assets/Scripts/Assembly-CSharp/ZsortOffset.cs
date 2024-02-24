using UnityEngine;

public class ZsortOffset : MonoBehaviour
{
	public bool onlyAtStart = true;

	public int sortOffset;

	private int startSort;

	public bool destroyMaterial;

	private void ApplyOffset()
	{
		GetComponent<Renderer>().material.renderQueue = startSort + sortOffset;
	}

	private void Start()
	{
		startSort = GetComponent<Renderer>().material.renderQueue;
		if (onlyAtStart)
		{
			ApplyOffset();
		}
		else
		{
			InvokeRepeating("ApplyOffset", 0f, 0.05f);
		}
	}

	private void OnDestroy()
	{
		if (destroyMaterial)
		{
			Object.DestroyImmediate(GetComponent<Renderer>().material);
		}
	}
}
