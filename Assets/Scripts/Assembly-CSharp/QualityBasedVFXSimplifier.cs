using System.Collections.Generic;
using UnityEngine;

public class QualityBasedVFXSimplifier : MonoBehaviour
{
	[SerializeField]
	private List<GameObject> objectsToDisable;

	[SerializeField]
	private List<GameObject> objectsToDistribute;

	[SerializeField]
	private float probabilityToDisableDistributed = 0.5f;

	private bool ShouldSimplify()
	{
		return QualitySettings.GetQualityLevel() == 0;
	}

	private void Start()
	{
		if (ShouldSimplify())
		{
			Simplify();
		}
	}

	private void Simplify()
	{
		foreach (GameObject item in objectsToDisable)
		{
			item.SetActive(value: false);
		}
		foreach (GameObject item2 in objectsToDistribute)
		{
			if (Random.Range(0f, 1f) <= probabilityToDisableDistributed)
			{
				item2.SetActive(value: false);
			}
		}
	}
}
