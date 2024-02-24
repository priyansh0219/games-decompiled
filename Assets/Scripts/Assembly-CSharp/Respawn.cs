using System;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
public class Respawn : MonoBehaviour
{
	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public float spawnTime = -1f;

	[NonSerialized]
	[ProtoMember(3)]
	public TechType techType;

	[NonSerialized]
	[ProtoMember(4)]
	public readonly List<string> addComponents = new List<string>();

	private IEnumerator Start()
	{
		if (!(DayNightCycle.main.timePassed >= (double)spawnTime) || !(spawnTime >= 0f))
		{
			yield break;
		}
		int num = UWE.Utils.OverlapSphereIntoSharedBuffer(base.transform.position, 1.5f);
		for (int i = 0; i < num; i++)
		{
			if (UWE.Utils.sharedColliderBuffer[i].GetComponentInParent<Base>() != null)
			{
				UnityEngine.Object.Destroy(base.gameObject);
				yield break;
			}
		}
		TaskResult<GameObject> result = new TaskResult<GameObject>();
		yield return CraftData.InstantiateFromPrefabAsync(techType, result);
		GameObject gameObject = result.Get();
		gameObject.transform.SetPositionAndRotation(base.transform.position, base.transform.rotation);
		for (int j = 0; j < addComponents.Count; j++)
		{
			Type type = Type.GetType(addComponents[j]);
			if (type != null)
			{
				gameObject.AddComponent(type);
			}
		}
		gameObject.SetActive(value: true);
		if (base.transform.parent == null || base.transform.parent.GetComponentInParent<LargeWorldEntity>() == null)
		{
			if ((bool)LargeWorldStreamer.main)
			{
				LargeWorldStreamer.main.cellManager.RegisterEntity(gameObject);
			}
		}
		else
		{
			if ((bool)LargeWorldStreamer.main)
			{
				LargeWorldStreamer.main.cellManager.UnregisterEntity(gameObject);
			}
			gameObject.transform.parent = base.transform.parent;
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
