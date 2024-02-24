using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class CrashHome : MonoBehaviour, IProtoEventListener
{
	private const double respawnDelay = 1200.0;

	[AssertNotNull]
	public GameObject crashPrefab;

	[AssertNotNull]
	public FMODAsset openSound;

	[AssertNotNull]
	public Animator animator;

	private Crash crash;

	private bool prevClosed = true;

	[NonSerialized]
	[ProtoMember(2)]
	public float spawnTime = -1f;

	private const int currentVersion = 3;

	[NonSerialized]
	[ProtoMember(3)]
	public int version = 3;

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		if (version < 3)
		{
			spawnTime = -1f;
			version = 3;
		}
	}

	private void Start()
	{
		if (spawnTime < 0f)
		{
			Spawn();
		}
	}

	private void Update()
	{
		DayNightCycle main = DayNightCycle.main;
		if (spawnTime >= 0f && main.timePassed >= (double)spawnTime)
		{
			Spawn();
		}
		bool flag = (bool)crash && crash.IsResting();
		if (flag != prevClosed)
		{
			if (!flag)
			{
				Utils.PlayFMODAsset(openSound, base.transform, 10f);
			}
			animator.SetBool(AnimatorHashID.attacking, !flag);
			prevClosed = flag;
		}
		if (!crash && spawnTime < 0f)
		{
			spawnTime = (float)(main.timePassed + 1200.0);
		}
	}

	private void OnDestroy()
	{
		if (crash != null)
		{
			UnityEngine.Object.Destroy(crash.gameObject);
		}
	}

	private void Spawn()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(crashPrefab, Vector3.zero, Quaternion.Euler(-90f, 0f, 0f));
		gameObject.transform.SetParent(base.transform, worldPositionStays: false);
		crash = gameObject.GetComponent<Crash>();
		spawnTime = -1f;
		if (LargeWorldStreamer.main != null)
		{
			LargeWorldStreamer.main.MakeEntityTransient(gameObject);
		}
	}
}
