using System.Collections.Generic;
using UnityEngine;

public class PrecursorVentEntryTrigger : MonoBehaviour
{
	private static readonly HashSet<PrecursorVentEntryTrigger> entries = new HashSet<PrecursorVentEntryTrigger>();

	[AssertNotNull]
	public PrecursorVentBase vent;

	public bool isPrisonVent;

	private Peeper authorizedPeeper;

	private float timeRevokeAuthorization = -1f;

	public static PrecursorVentEntryTrigger GetNearestVentEntry(float maxRange, Peeper peeper)
	{
		if (entries.Count < 1)
		{
			return null;
		}
		Vector3 position = peeper.transform.position;
		PrecursorVentEntryTrigger result = null;
		float num = maxRange;
		HashSet<PrecursorVentEntryTrigger>.Enumerator enumerator = entries.GetEnumerator();
		while (enumerator.MoveNext())
		{
			PrecursorVentEntryTrigger current = enumerator.Current;
			if (current.Accepts(peeper) && current.vent.CanSuckIn() && (!current.authorizedPeeper || !(current.authorizedPeeper != peeper)))
			{
				float num2 = Vector3.Distance(position, current.transform.position);
				if (num2 < num)
				{
					result = current;
					num = num2;
				}
			}
		}
		return result;
	}

	public bool Accepts(Peeper peeper)
	{
		if (!isPrisonVent)
		{
			return !peeper.isHero;
		}
		return peeper.isHero;
	}

	public void AcquireExclusiveAccess(Peeper peeper)
	{
		if (!isPrisonVent)
		{
			authorizedPeeper = peeper;
			timeRevokeAuthorization = Time.time + 30f;
		}
	}

	public void ReleaseExclusiveAccess(Peeper peeper)
	{
		if (authorizedPeeper == peeper)
		{
			authorizedPeeper = null;
			timeRevokeAuthorization = -1f;
		}
	}

	private void Start()
	{
		entries.Add(this);
		InvokeRepeating("UpdateAuthorization", Random.value, 1f);
	}

	private void UpdateAuthorization()
	{
		if (timeRevokeAuthorization > 0f && Time.time > timeRevokeAuthorization)
		{
			authorizedPeeper = null;
			timeRevokeAuthorization = -1f;
		}
	}

	private void OnDestroy()
	{
		entries.Remove(this);
	}

	private void OnTriggerEnter(Collider other)
	{
		Peeper component = other.GetComponent<Peeper>();
		if (!component)
		{
			return;
		}
		if (!vent.CanSuckIn())
		{
			SwimToVent component2 = component.GetComponent<SwimToVent>();
			if ((bool)component2)
			{
				component2.OnReachBlockedVentEntry(this);
			}
		}
		else
		{
			ReleaseExclusiveAccess(component);
			vent.SuckInPeeper(component);
		}
	}
}
