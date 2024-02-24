using System.Collections;
using UWE;
using UnityEngine;

public class SpawnEscapePodSupplies : MonoBehaviour
{
	public StorageContainer storageContainer;

	private void OnNewBorn()
	{
		CoroutineHost.StartCoroutine(OnNewBornAsync());
	}

	private IEnumerator OnNewBornAsync()
	{
		ItemsContainer container = storageContainer.container;
		LootSpawner main = LootSpawner.main;
		TechType[] techTypes = main.GetEscapePodStorageTechTypes();
		foreach (TechType techType in techTypes)
		{
			CoroutineTask<GameObject> prefabRequest = CraftData.GetPrefabForTechTypeAsync(techType);
			yield return prefabRequest;
			Pickupable component = CraftData.InstantiateFromPrefab(prefabRequest.GetResult(), techType).GetComponent<Pickupable>();
			component.Initialize();
			if (container.HasRoomFor(component))
			{
				InventoryItem item = new InventoryItem(component);
				container.UnsafeAdd(item);
			}
			else
			{
				Object.Destroy(component.gameObject);
			}
		}
	}
}
