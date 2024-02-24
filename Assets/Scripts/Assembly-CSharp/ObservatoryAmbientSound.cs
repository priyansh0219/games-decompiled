using UWE;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ObservatoryAmbientSound : MonoBehaviour
{
	public SphereCollider trigger;

	private bool inObservatory;

	private static int insideObservatoriesCount;

	public static bool IsPlayerInObservatory()
	{
		return insideObservatoriesCount > 0;
	}

	private void OnEnable()
	{
		InvokeRepeating("CheckTrigger", Random.value, 2f);
	}

	private void OnDisable()
	{
		CancelInvoke("CheckTrigger");
		SetInObservatory(value: false);
	}

	private void CheckTrigger()
	{
		GameObject mainObject = Player.mainObject;
		if (mainObject != null)
		{
			Vector3 position = mainObject.transform.position;
			bool flag = UWE.Utils.IsInsideCollider(trigger, position);
			SetInObservatory(flag);
		}
		else
		{
			SetInObservatory(value: false);
		}
	}

	private void SetInObservatory(bool value)
	{
		if (inObservatory != value)
		{
			inObservatory = value;
			if (inObservatory)
			{
				insideObservatoriesCount++;
			}
			else
			{
				insideObservatoriesCount--;
			}
		}
	}
}
