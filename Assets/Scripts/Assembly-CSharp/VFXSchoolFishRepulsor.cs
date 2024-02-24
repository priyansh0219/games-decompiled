using UnityEngine;

public class VFXSchoolFishRepulsor : MonoBehaviour
{
	private VFXSchoolFishManager mng;

	private bool isRepulsor;

	private void AddToRepulsorsList()
	{
		if (!isRepulsor)
		{
			mng = VFXSchoolFishManager.main;
			if (mng != null)
			{
				mng.AddRepulsor(base.transform);
				isRepulsor = true;
			}
		}
	}

	private void RemoveFromRepulsorsList()
	{
		if (isRepulsor && mng != null)
		{
			mng.RemoveRepulsor(base.transform);
			isRepulsor = false;
		}
	}

	private void Start()
	{
		AddToRepulsorsList();
	}

	private void OnEnable()
	{
		AddToRepulsorsList();
	}

	private void OnDisable()
	{
		RemoveFromRepulsorsList();
	}

	private void OnDestroy()
	{
		RemoveFromRepulsorsList();
	}
}
