using System.Collections.Generic;
using UnityEngine;

public class EnzymeCloud : MonoBehaviour
{
	private static readonly HashSet<EnzymeCloud> enzymeClouds = new HashSet<EnzymeCloud>();

	public float duration = 10f;

	public static EnzymeCloud GetNearestEnzymeCloud(Vector3 position)
	{
		if (enzymeClouds.Count < 1)
		{
			return null;
		}
		EnzymeCloud result = null;
		float num = float.MaxValue;
		HashSet<EnzymeCloud>.Enumerator enumerator = enzymeClouds.GetEnumerator();
		while (enumerator.MoveNext())
		{
			EnzymeCloud current = enumerator.Current;
			float num2 = Vector3.Distance(position, current.transform.position);
			if (num2 < num)
			{
				result = current;
				num = num2;
			}
		}
		return result;
	}

	private void Start()
	{
		enzymeClouds.Add(this);
		Object.Destroy(base.gameObject, duration);
	}

	private void OnDestroy()
	{
		enzymeClouds.Remove(this);
	}

	private void OnTriggerEnter(Collider other)
	{
		Peeper componentInParent = other.GetComponentInParent<Peeper>();
		if ((bool)componentInParent)
		{
			componentInParent.BecomeHero();
		}
	}
}
