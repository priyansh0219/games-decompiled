using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class PrecursorPrisonVent : PrecursorVentBase
{
	[AssertNotNull]
	public GameObject prisonPeeperPrefab;

	public float minEmitInterval = 5f;

	public float maxEmitInterval = 30f;

	public float leashDistance = 20f;

	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public int numStoredPeepers = 20;

	private void OnEnable()
	{
		ScheduleEmitPeeper();
	}

	private void OnDisable()
	{
		CancelEmitPeeper();
	}

	private void ScheduleEmitPeeper()
	{
		Invoke("TryEmitPeeper", UnityEngine.Random.Range(minEmitInterval, maxEmitInterval));
	}

	private void CancelEmitPeeper()
	{
		CancelInvoke("TryEmitPeeper");
	}

	private void TryEmitPeeper()
	{
		if (numStoredPeepers > 0)
		{
			EmitPeeper();
		}
		ScheduleEmitPeeper();
	}

	protected override void StorePeeper(Peeper peeper)
	{
		numStoredPeepers++;
		UnityEngine.Object.Destroy(peeper.gameObject);
	}

	protected override Peeper RetrievePeeper()
	{
		if (numStoredPeepers < 1)
		{
			Debug.LogWarningFormat(this, "Precursor prison vent can not emit a peeper because none are stored anymore");
			return null;
		}
		numStoredPeepers--;
		Peeper component = UnityEngine.Object.Instantiate(prisonPeeperPrefab).GetComponent<Peeper>();
		component.leashPosition = base.transform.TransformPoint(new Vector3(0f, leashDistance, 0f));
		component.isInitialized = true;
		component.InitializeInPrison();
		return component;
	}
}
