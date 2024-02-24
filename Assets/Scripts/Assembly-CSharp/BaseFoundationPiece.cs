using System;
using UnityEngine;

public class BaseFoundationPiece : MonoBehaviour, IBaseAccessoryGeometry
{
	[Serializable]
	public struct Pillar
	{
		public GameObject root;

		public Transform adjustable;

		public Transform bottom;
	}

	public Pillar[] pillars;

	public float maxPillarHeight = 8f;

	public float extraHeight = 0.1f;

	public float minHeight;

	private void Start()
	{
		Pillar[] array = pillars;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].root.SetActive(value: false);
		}
	}

	void IBaseAccessoryGeometry.BuildGeometry(Base baseComp, bool disableColliders)
	{
		for (int i = 0; i < pillars.Length; i++)
		{
			Pillar pillar = pillars[i];
			pillar.root.SetActive(value: false);
			Transform adjustable = pillar.adjustable;
			if (!adjustable)
			{
				continue;
			}
			float floorDistance = BaseUtils.GetFloorDistance(adjustable.position, adjustable.forward, maxPillarHeight, base.gameObject);
			if (!(floorDistance > -1f))
			{
				continue;
			}
			float num = floorDistance + 0.01f + extraHeight;
			if (num >= minHeight)
			{
				adjustable.localScale = new Vector3(1f, 1f, num);
				if ((bool)pillar.bottom)
				{
					pillar.bottom.position = adjustable.position + adjustable.forward * num;
				}
				pillar.root.SetActive(value: true);
			}
		}
	}
}
