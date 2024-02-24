using UnityEngine;

public class VFXEnableAfterCall : MonoBehaviour
{
	[AssertNotNull]
	public VFXConstructing vfxConstructing;

	public GameObject[] enableObjects;

	private void Start()
	{
		TryEnableVFX();
	}

	public void OnConstructionDone()
	{
		TryEnableVFX();
	}

	private void TryEnableVFX()
	{
		if (vfxConstructing.isDone)
		{
			GameObject[] array = enableObjects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: true);
			}
		}
	}
}
