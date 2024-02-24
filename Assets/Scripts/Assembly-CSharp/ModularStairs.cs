using System.Collections.Generic;
using UnityEngine;

public class ModularStairs : MonoBehaviour, IBaseAccessoryGeometry
{
	public Transform traceStart;

	public GameObject stepPrefab;

	public GameObject walkWay;

	public ConstructableBounds constructableBounds;

	private const float stepLength = 0.5f;

	private const float maxLength = 10f;

	private GameObject stairsParent;

	private readonly List<GameObject> cachedSteps = new List<GameObject>();

	void IBaseAccessoryGeometry.BuildGeometry(Base baseComp, bool disableColliders)
	{
		if ((bool)stairsParent)
		{
			cachedSteps.Clear();
			foreach (Transform item in stairsParent.transform)
			{
				cachedSteps.Add(item.gameObject);
			}
		}
		else
		{
			stairsParent = new GameObject();
			stairsParent.transform.parent = base.transform;
		}
		Vector3 vector = traceStart.transform.up * -0.15f;
		float floorDistance = BaseUtils.GetFloorDistance(vector + traceStart.position - traceStart.right * 0.7f, traceStart.forward, 10f, base.gameObject);
		float floorDistance2 = BaseUtils.GetFloorDistance(vector + traceStart.position + traceStart.right * 0.7f, traceStart.forward, 10f, base.gameObject);
		float num = 0f;
		float num2 = 0f;
		if (floorDistance > floorDistance2)
		{
			num = floorDistance;
			num2 = floorDistance2;
		}
		else
		{
			num = floorDistance2;
			num2 = floorDistance;
		}
		if (num > 0f)
		{
			BuildStairs(num, baseComp.isGhost, disableColliders);
			constructableBounds.bounds.extents = new Vector3(1.4f, 0.01f, (num2 - 0.5f) / 2f);
			constructableBounds.bounds.position = vector + new Vector3(0f, 0.01f, (num2 - 0.5f) / 2f);
		}
		else
		{
			constructableBounds.bounds.extents = Vector3.zero;
			constructableBounds.bounds.position = Vector3.zero;
		}
	}

	private GameObject CreateStep()
	{
		if (cachedSteps.Count > 0)
		{
			GameObject gameObject = cachedSteps[0];
			cachedSteps.Remove(gameObject);
			return gameObject;
		}
		return Object.Instantiate(stepPrefab, stairsParent.transform);
	}

	private void BuildStairs(float length, bool isGhost, bool disableColliders)
	{
		int num = Mathf.RoundToInt(length / 0.5f + 1.5f);
		for (int i = 0; i < num; i++)
		{
			GameObject obj = CreateStep();
			obj.transform.position = traceStart.position + (float)i * 0.5f * traceStart.forward;
			obj.transform.rotation = base.transform.rotation;
			obj.SetActive(value: true);
		}
		Collider[] componentsInChildren = stairsParent.GetComponentsInChildren<Collider>();
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			componentsInChildren[j].enabled = !disableColliders;
		}
	}
}
