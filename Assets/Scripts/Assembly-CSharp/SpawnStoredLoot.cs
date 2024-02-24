using System;
using System.Collections;
using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
[RequireComponent(typeof(StorageContainer))]
public class SpawnStoredLoot : MonoBehaviour
{
	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public bool lootSpawned;

	public int randomStartMinItems;

	public int randomStartMaxItems;

	public AnimationCurve distribution = new AnimationCurve();

	private IEnumerator Start()
	{
		if (lootSpawned)
		{
			yield break;
		}
		StorageContainer component = base.gameObject.GetComponent<StorageContainer>();
		ItemsContainer container = component.container;
		float num = UWE.Utils.Sample(distribution);
		int count = Mathf.RoundToInt((float)randomStartMinItems + num * (float)(randomStartMaxItems - randomStartMinItems));
		LootSpawner main = LootSpawner.main;
		TechType[] techTypes = main.GetSupplyTechTypes(count);
		foreach (TechType techType in techTypes)
		{
			TaskResult<GameObject> result = new TaskResult<GameObject>();
			yield return CraftData.InstantiateFromPrefabAsync(techType, result);
			Pickupable component2 = result.Get().GetComponent<Pickupable>();
			component2.Initialize();
			if (container.HasRoomFor(component2))
			{
				InventoryItem item = new InventoryItem(component2);
				container.UnsafeAdd(item);
			}
			else
			{
				UnityEngine.Object.Destroy(component2.gameObject);
			}
		}
		lootSpawned = true;
	}
}
