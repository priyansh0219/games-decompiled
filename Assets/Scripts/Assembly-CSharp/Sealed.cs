using System;
using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
public class Sealed : MonoBehaviour
{
	[NonSerialized]
	public Event<Sealed> openedEvent = new Event<Sealed>();

	private const int currentVersion = 0;

	[NonSerialized]
	[ProtoMember(1)]
	public int version;

	[ProtoMember(2)]
	public float openedAmount;

	[ProtoMember(3)]
	public float maxOpenedAmount = 100f;

	[NonSerialized]
	[ProtoMember(4)]
	public bool _sealed = true;

	public bool requireOpenFromFront;

	public bool IsSealed()
	{
		return _sealed;
	}

	public void Weld(float amount)
	{
		if (_sealed)
		{
			openedAmount = Mathf.Min(openedAmount + amount, maxOpenedAmount);
			if (Mathf.Approximately(openedAmount, maxOpenedAmount))
			{
				Debug.Log("Trigger opened event");
				openedEvent.Trigger(this);
				_sealed = false;
			}
		}
	}

	public int GetSealedPercent()
	{
		return (int)(Mathf.Clamp01(openedAmount / maxOpenedAmount) * 100f);
	}

	public float GetSealedPercentNormalized()
	{
		return openedAmount / maxOpenedAmount;
	}
}
