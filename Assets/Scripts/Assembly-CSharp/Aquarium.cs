using System.Collections.Generic;
using UnityEngine;

public class Aquarium : MonoBehaviour
{
	private class FishTrack
	{
		public GameObject track;

		public TechType fishType;

		public GameObject fish;

		public GameObject item;

		public InfectedMixin infectedMixin;

		public void Clear()
		{
			fishType = TechType.None;
			item = null;
			infectedMixin = null;
			Object.Destroy(fish);
		}

		public FishTrack(GameObject track)
		{
			this.track = track;
		}
	}

	public StorageContainer storageContainer;

	public GameObject fishRoot;

	public GameObject[] trackObjects;

	public float spreadInfectionInterval = 10f;

	private List<FishTrack> tracks;

	private bool subscribed;

	private double timeNextInfectionSpread = -1.0;

	private const float fishScale = 0.3f;

	private const float fishMaxSpeed = 0.5f;

	private void Start()
	{
		tracks = new List<FishTrack>();
		for (int i = 0; i < trackObjects.Length; i++)
		{
			tracks.Add(new FishTrack(trackObjects[i]));
		}
		Invoke("InitFishDelayed", 0f);
	}

	private void InitFishDelayed()
	{
		IEnumerable<InventoryItem> container = storageContainer.container;
		if (container == null)
		{
			return;
		}
		foreach (InventoryItem item in container)
		{
			AddItem(item);
		}
	}

	private void OnEnable()
	{
		storageContainer.enabled = true;
	}

	private void OnDisable()
	{
		Subscribe(state: false);
		storageContainer.enabled = false;
	}

	private void Update()
	{
		double timePassed = DayNightCycle.main.timePassed;
		if (timeNextInfectionSpread > 0.0 && timePassed > timeNextInfectionSpread)
		{
			if (InfectCreature())
			{
				timeNextInfectionSpread = timePassed + (double)spreadInfectionInterval;
			}
			else
			{
				timeNextInfectionSpread = -1.0;
			}
		}
	}

	private void LateUpdate()
	{
		Subscribe(state: true);
	}

	private void Subscribe(bool state)
	{
		if (subscribed == state)
		{
			return;
		}
		if (storageContainer.container == null)
		{
			Debug.LogWarning("Aquarium.Subscribe(): container null; will retry next frame");
			return;
		}
		if (subscribed)
		{
			storageContainer.container.onAddItem -= AddItem;
			storageContainer.container.onRemoveItem -= RemoveItem;
			storageContainer.container.isAllowedToAdd = null;
			fishRoot.SetActive(value: false);
		}
		else
		{
			storageContainer.container.onAddItem += AddItem;
			storageContainer.container.onRemoveItem += RemoveItem;
			storageContainer.container.isAllowedToAdd = IsAllowedToAdd;
			fishRoot.SetActive(value: true);
		}
		subscribed = state;
	}

	private bool IsAllowedToAdd(Pickupable pickupable, bool verbose)
	{
		return pickupable.GetComponent<AquariumFish>() != null;
	}

	private void AddItem(InventoryItem item)
	{
		GameObject gameObject = item.item.gameObject;
		AquariumFish component = gameObject.GetComponent<AquariumFish>();
		if (!(component == null))
		{
			TechType techType = item.item.GetTechType();
			FishTrack freeTrack = GetFreeTrack();
			GameObject gameObject2 = Object.Instantiate(component.model, Vector3.zero, Quaternion.identity);
			gameObject2.transform.SetParent(freeTrack.track.transform, worldPositionStays: false);
			gameObject2.transform.localScale *= 0.3f;
			SetupRenderers(gameObject2);
			AnimateByVelocity componentInChildren = gameObject2.GetComponentInChildren<AnimateByVelocity>();
			componentInChildren.rootGameObject = gameObject2;
			componentInChildren.animationMoveMaxSpeed = 0.5f;
			componentInChildren.levelOfDetail = gameObject2.EnsureComponent<BehaviourLOD>();
			freeTrack.fishType = techType;
			freeTrack.fish = gameObject2;
			freeTrack.item = gameObject;
			InfectedMixin component2 = gameObject.GetComponent<InfectedMixin>();
			if (component2 != null)
			{
				InfectedMixin infectedMixin = gameObject2.AddComponent<InfectedMixin>();
				infectedMixin.renderers = GetMarmosetRenderers(gameObject2).ToArray();
				infectedMixin.SetInfectedAmount(component2.GetInfectedAmount());
				freeTrack.infectedMixin = infectedMixin;
			}
			gameObject2.SetActive(value: true);
			UpdateInfectionSpreading();
		}
	}

	private void RemoveItem(InventoryItem item)
	{
		GameObject gameObject = item.item.gameObject;
		FishTrack trackByItem = GetTrackByItem(gameObject);
		if (trackByItem == null)
		{
			return;
		}
		InfectedMixin infectedMixin = trackByItem.infectedMixin;
		if (infectedMixin != null)
		{
			InfectedMixin component = gameObject.GetComponent<InfectedMixin>();
			if (component != null)
			{
				component.SetInfectedAmount(infectedMixin.GetInfectedAmount());
			}
		}
		trackByItem.Clear();
		UpdateInfectionSpreading();
	}

	private FishTrack GetFreeTrack()
	{
		return GetTrackByFishType(TechType.None);
	}

	private FishTrack GetTrackByFishType(TechType fishType)
	{
		for (int i = 0; i < tracks.Count; i++)
		{
			if (tracks[i].fishType == fishType)
			{
				return tracks[i];
			}
		}
		return null;
	}

	private FishTrack GetTrackByItem(GameObject item)
	{
		for (int i = 0; i < tracks.Count; i++)
		{
			if (tracks[i].item == item)
			{
				return tracks[i];
			}
		}
		return null;
	}

	private void SetupRenderers(GameObject gameObject)
	{
		int newLayer = LayerMask.NameToLayer("Viewmodel");
		Utils.SetLayerRecursively(gameObject, newLayer);
	}

	private bool ContainsHeroPeepers()
	{
		for (int i = 0; i < tracks.Count; i++)
		{
			if (tracks[i].fishType == TechType.Peeper)
			{
				Peeper component = tracks[i].item.GetComponent<Peeper>();
				if (component != null && component.isHero)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool ContainsInfectedCreature()
	{
		for (int i = 0; i < tracks.Count; i++)
		{
			if (tracks[i].infectedMixin != null && tracks[i].infectedMixin.GetInfectedAmount() > 0.25f)
			{
				return true;
			}
		}
		return false;
	}

	private bool InfectCreature()
	{
		bool result = false;
		for (int i = 0; i < tracks.Count; i++)
		{
			if (tracks[i].infectedMixin != null && tracks[i].infectedMixin.GetInfectedAmount() < 1f)
			{
				tracks[i].infectedMixin.SetInfectedAmount(1f);
				result = true;
				break;
			}
		}
		return result;
	}

	private void CureAllCreatures()
	{
		InfectedMixin infectedMixin = null;
		for (int i = 0; i < tracks.Count; i++)
		{
			infectedMixin = tracks[i].infectedMixin;
			if (infectedMixin != null && infectedMixin.GetInfectedAmount() > 0.1f)
			{
				infectedMixin.SetInfectedAmount(0.1f);
			}
		}
	}

	private void UpdateInfectionSpreading()
	{
		if (ContainsHeroPeepers())
		{
			CureAllCreatures();
			timeNextInfectionSpread = -1.0;
		}
		else if (timeNextInfectionSpread < 0.0 && ContainsInfectedCreature())
		{
			timeNextInfectionSpread = DayNightCycle.main.timePassed + (double)spreadInfectionInterval;
		}
	}

	private static List<Renderer> GetMarmosetRenderers(GameObject gameObject)
	{
		List<Renderer> list = new List<Renderer>();
		Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
		foreach (Renderer renderer in componentsInChildren)
		{
			bool flag = false;
			for (int j = 0; j < renderer.sharedMaterials.Length; j++)
			{
				Material material = renderer.sharedMaterials[j];
				if (material != null && material.shader != null && material.shader.name.Contains("Marmoset"))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				list.Add(renderer);
			}
		}
		return list;
	}
}
