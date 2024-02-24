using System;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
public class ReefbackLife : MonoBehaviour, IProtoTreeEventListener, GameObjectPool.IPooledObject
{
	private const int maxSpawnsPerFrame = 1;

	private const int currentVersion = 3;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 3;

	[NonSerialized]
	[ProtoMember(2)]
	public bool initialized;

	[NonSerialized]
	[ProtoMember(3)]
	public bool hasCorals;

	[NonSerialized]
	[ProtoMember(4)]
	public int grassIndex = -1;

	[AssertNotNull]
	public ReefbackSlotsData reefbackSlotsData;

	[AssertNotNull]
	public Transform plantSlotsRoot;

	[AssertNotNull]
	public Transform[] plantSlots;

	[AssertNotNull]
	public Transform[] creatureSlots;

	[AssertNotNull]
	public GameObject corals;

	[AssertNotNull]
	public GameObject islands;

	[AssertNotNull]
	public GameObject[] grassVariants;

	private bool needToRemovePlantPhysics = true;

	public void Spawn(float time = 0f, bool active = true)
	{
	}

	public void Despawn(float time = 0f)
	{
		for (int i = 0; i < grassVariants.Length; i++)
		{
			grassVariants[i].SetActive(value: false);
		}
	}

	private void OnEnable()
	{
		StartCoroutine(CoSpawn());
	}

	private IEnumerator CoSpawn()
	{
		if (!initialized)
		{
			yield return Initialize();
		}
		else if (needToRemovePlantPhysics)
		{
			AddReefbackPlantToNonSlotChildren();
		}
		needToRemovePlantPhysics = false;
		corals.SetActive(hasCorals);
		islands.SetActive(hasCorals);
		if (grassIndex >= 0 && grassIndex < grassVariants.Length)
		{
			grassVariants[grassIndex].SetActive(value: true);
		}
	}

	private IEnumerator Initialize()
	{
		initialized = true;
		hasCorals = base.transform.localScale.x > 0.8f;
		if (hasCorals)
		{
			yield return SpawnPlants();
			yield return SpawnCreatures();
			string text = null;
			LargeWorld main = LargeWorld.main;
			if ((bool)main)
			{
				text = main.GetBiome(base.transform.position);
			}
			if (!string.IsNullOrEmpty(text) && text.StartsWith("grassyplateaus", StringComparison.OrdinalIgnoreCase))
			{
				grassIndex = 0;
			}
			else
			{
				grassIndex = UnityEngine.Random.Range(1, grassVariants.Length);
			}
		}
	}

	private IEnumerator SpawnPlants()
	{
		float sumProb = 0f;
		for (int j = 0; j < reefbackSlotsData.plants.Length; j++)
		{
			sumProb += reefbackSlotsData.plants[j].probability;
		}
		int num = 0;
		for (int i = 0; i < plantSlots.Length; i++)
		{
			Transform transform = plantSlots[i];
			float num2 = UnityEngine.Random.value * sumProb;
			float num3 = 0f;
			int num4 = 0;
			for (int k = 0; k < reefbackSlotsData.plants.Length; k++)
			{
				num3 += reefbackSlotsData.plants[k].probability;
				if (num2 <= num3)
				{
					num4 = k;
					break;
				}
			}
			ReefbackSlotsData.ReefbackSlotPlant reefbackSlotPlant = reefbackSlotsData.plants[num4];
			GameObject random = reefbackSlotPlant.prefabVariants.GetRandom();
			if (SpawnRestrictionEnforcer.ShouldSpawn(random))
			{
				GameObject obj = UWE.Utils.InstantiateWrap(random, transform.position, Quaternion.Euler(reefbackSlotPlant.startRotation));
				obj.transform.SetParent(transform.parent, worldPositionStays: true);
				obj.AddComponent<ReefbackPlant>();
				if (++num >= 1)
				{
					yield return CoroutineUtils.waitForNextFrame;
					num = 0;
				}
			}
		}
	}

	private IEnumerator SpawnCreatures()
	{
		float sumProb = 0f;
		for (int k = 0; k < reefbackSlotsData.creatures.Length; k++)
		{
			sumProb += reefbackSlotsData.creatures[k].probability;
		}
		int num2 = 0;
		for (int j = 0; j < creatureSlots.Length; j++)
		{
			Transform creatureSlot = creatureSlots[j];
			float num3 = UnityEngine.Random.value * sumProb;
			float num4 = 0f;
			int num5 = 0;
			for (int l = 0; l < reefbackSlotsData.creatures.Length; l++)
			{
				num4 += reefbackSlotsData.creatures[l].probability;
				if (num3 <= num4)
				{
					num5 = l;
					break;
				}
			}
			ReefbackSlotsData.ReefbackSlotCreature reefbackSlotCreature = reefbackSlotsData.creatures[num5];
			GameObject prefabToSpawn = reefbackSlotCreature.prefab;
			if (!SpawnRestrictionEnforcer.ShouldSpawn(prefabToSpawn))
			{
				continue;
			}
			int num = UnityEngine.Random.Range(reefbackSlotCreature.minNumber, reefbackSlotCreature.maxNumber + 1);
			for (int i = 0; i < num; i++)
			{
				Vector3 position = creatureSlot.localPosition + UnityEngine.Random.insideUnitSphere * 5f;
				Quaternion localRotation = creatureSlot.localRotation;
				GameObject obj = UnityEngine.Object.Instantiate(prefabToSpawn, position, localRotation);
				obj.transform.SetParent(creatureSlot.parent, worldPositionStays: false);
				obj.AddComponent<ReefbackCreature>();
				if (++num2 >= 1)
				{
					yield return CoroutineUtils.waitForNextFrame;
					num2 = 0;
				}
			}
		}
	}

	public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
	{
		if (version < 3)
		{
			initialized = false;
			version = 3;
		}
		needToRemovePlantPhysics = true;
	}

	private void AddReefbackPlantToNonSlotChildren()
	{
		List<Transform> list = new List<Transform>(plantSlots);
		for (int i = 0; i < plantSlotsRoot.childCount; i++)
		{
			Transform child = plantSlotsRoot.GetChild(i);
			if (list.IndexOf(child) != -1)
			{
				list.Remove(child);
			}
			else
			{
				child.gameObject.AddComponent<ReefbackPlant>();
			}
		}
	}
}
