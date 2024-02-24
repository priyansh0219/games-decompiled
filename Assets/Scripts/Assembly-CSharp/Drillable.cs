using System;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
public class Drillable : MonoBehaviour, IManagedUpdateBehaviour, IManagedBehaviour, ILocalizationCheckable
{
	[Serializable]
	public struct ResourceType
	{
		public TechType techType;

		public float chance;
	}

	public delegate void OnDrilled(Drillable drillable);

	public ResourceType[] resources;

	public GameObject breakFX;

	public GameObject breakAllFX;

	[AssertLocalization(AssertLocalizationAttribute.Options.AllowEmptyString)]
	public string primaryTooltip;

	[AssertLocalization(AssertLocalizationAttribute.Options.AllowEmptyString)]
	public string secondaryTooltip;

	public bool deleteWhenDrilled = true;

	[AssertNotNull]
	public GameObject modelRoot;

	public int minResourcesToSpawn = 1;

	public int maxResourcesToSpawn = 3;

	public bool lootPinataOnSpawn = true;

	private MeshRenderer[] renderers;

	private const float drillDamage = 5f;

	private const float maxHealth = 200f;

	private float timeLastDrilled;

	private List<GameObject> lootPinataObjects = new List<GameObject>();

	private Exosuit drillingExo;

	private bool addedToUpdateManager;

	private Vector3 modelRootOffset;

	[AssertLocalization]
	private const string cantFitContainerMessage = "ContainerCantFit";

	[AssertLocalization]
	private const string addedToStorageMessage = "VehicleAddedToStorage";

	[AssertLocalization(2)]
	private const string tooltipFormat = "DrillResourceTooltipFormat";

	[AssertLocalization(1)]
	private const string handFormat = "DrillResource";

	[AssertLocalization]
	private const string handNeedExo = "NeedExoToMine";

	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 1;

	[NonSerialized]
	[ProtoMember(2, OverwriteList = true)]
	public float[] health;

	public int managedUpdateIndex { get; set; }

	public event OnDrilled onDrilled;

	public string GetProfileTag()
	{
		return "Drillable";
	}

	private void OnDisable()
	{
		BehaviourUpdateUtils.Deregister(this);
	}

	private void OnDestroy()
	{
		BehaviourUpdateUtils.Deregister(this);
	}

	private void Start()
	{
		renderers = GetComponentsInChildren<MeshRenderer>();
		if (health == null)
		{
			health = new float[renderers.Length];
			for (int i = 0; i < health.Length; i++)
			{
				health[i] = 200f;
			}
		}
		else
		{
			if (health.Length != renderers.Length)
			{
				float[] array = (float[])health.Clone();
				health = new float[renderers.Length];
				for (int j = 0; j < health.Length; j++)
				{
					if (j < array.Length)
					{
						health[j] = array[j];
					}
					else
					{
						health[j] = 200f;
					}
				}
			}
			for (int k = 0; k < health.Length; k++)
			{
				renderers[k].gameObject.SetActive(health[k] > 0f);
			}
		}
		TechType dominantResourceType = GetDominantResourceType();
		if (string.IsNullOrEmpty(primaryTooltip))
		{
			primaryTooltip = dominantResourceType.AsString();
		}
		if (string.IsNullOrEmpty(secondaryTooltip))
		{
			string arg = Language.main.Get(dominantResourceType);
			string arg2 = Language.main.Get(TooltipFactory.techTypeTooltipStrings.Get(dominantResourceType));
			secondaryTooltip = Language.main.GetFormat("DrillResourceTooltipFormat", arg, arg2);
		}
		modelRootOffset = modelRoot.transform.position - base.transform.position;
	}

	public void HoverDrillable()
	{
		Exosuit exosuit = Player.main.GetVehicle() as Exosuit;
		if ((bool)exosuit && exosuit.HasDrill())
		{
			GameInput.Button button = ((exosuit.leftArmType != TechType.ExosuitDrillArmModule) ? GameInput.Button.RightHand : GameInput.Button.LeftHand);
			HandReticle.main.SetText(HandReticle.TextType.Hand, Language.main.GetFormat("DrillResource", Language.main.Get(primaryTooltip)), translate: false, button);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, secondaryTooltip, translate: true);
			HandReticle.main.SetIcon(HandReticle.IconType.Drill);
		}
		else
		{
			HandReticle.main.SetText(HandReticle.TextType.Hand, primaryTooltip, translate: true);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, "NeedExoToMine", translate: true);
		}
	}

	private TechType GetDominantResourceType()
	{
		TechType result = TechType.None;
		float num = 0f;
		for (int i = 0; i < resources.Length; i++)
		{
			if (resources[i].chance > num)
			{
				num = resources[i].chance;
				result = resources[i].techType;
			}
		}
		return result;
	}

	public void Restore()
	{
		for (int i = 0; i < health.Length; i++)
		{
			health[i] = 200f;
			renderers[i].gameObject.SetActive(value: true);
		}
	}

	public void OnDrill(Vector3 position, Exosuit exo, out GameObject hitObject)
	{
		float num = 0f;
		for (int i = 0; i < health.Length; i++)
		{
			num += health[i];
		}
		drillingExo = exo;
		Vector3 center = Vector3.zero;
		int num2 = FindClosestMesh(position, out center);
		hitObject = renderers[num2].gameObject;
		timeLastDrilled = Time.time;
		if (num > 0f)
		{
			float num3 = health[num2];
			health[num2] = Mathf.Max(0f, health[num2] - 5f);
			num -= num3 - health[num2];
			if (num3 > 0f && health[num2] <= 0f)
			{
				renderers[num2].gameObject.SetActive(value: false);
				SpawnFX(breakFX, center);
				if (resources.Length != 0)
				{
					StartCoroutine(SpawnLootAsync(center));
				}
			}
			if (num <= 0f)
			{
				SpawnFX(breakAllFX, center);
				if (this.onDrilled != null)
				{
					this.onDrilled(this);
				}
				if (deleteWhenDrilled)
				{
					ResourceTracker component = GetComponent<ResourceTracker>();
					if ((bool)component)
					{
						component.OnBreakResource();
					}
					float time = (lootPinataOnSpawn ? 6f : 0f);
					Invoke("DestroySelf", time);
				}
			}
		}
		BehaviourUpdateUtils.Register(this);
	}

	private void DestroySelf()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void ClipWithTerrain(ref Vector3 position)
	{
		Vector3 origin = position;
		origin.y = base.transform.position.y + 5f;
		if (Physics.Raycast(new Ray(origin, Vector3.down), out var hitInfo, 10f, Voxeland.GetTerrainLayerMask(), QueryTriggerInteraction.Ignore))
		{
			position.y = Mathf.Max(position.y, hitInfo.point.y + 0.3f);
		}
	}

	private IEnumerator SpawnLootAsync(Vector3 position)
	{
		int numResources = UnityEngine.Random.Range(minResourcesToSpawn, maxResourcesToSpawn);
		TaskResult<GameObject> prefabResult = new TaskResult<GameObject>();
		for (int i = 0; i < numResources; i++)
		{
			yield return ChooseRandomResourceAsync(prefabResult);
			GameObject gameObject = prefabResult.Get();
			if ((bool)gameObject)
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject);
				Vector3 position2 = position;
				float num = 1f;
				position2.x += UnityEngine.Random.Range(0f - num, num);
				position2.z += UnityEngine.Random.Range(0f - num, num);
				position2.y += UnityEngine.Random.Range(0f - num, num);
				ClipWithTerrain(ref position2);
				gameObject2.transform.position = position2;
				Vector3 onUnitSphere = UnityEngine.Random.onUnitSphere;
				onUnitSphere.y = 0f;
				onUnitSphere = Vector3.Normalize(onUnitSphere);
				onUnitSphere.y = 1f;
				gameObject2.GetComponent<Rigidbody>().isKinematic = false;
				gameObject2.GetComponent<Rigidbody>().AddForce(onUnitSphere);
				gameObject2.GetComponent<Rigidbody>().AddTorque(Vector3.right * UnityEngine.Random.Range(3f, 6f));
				if (lootPinataOnSpawn)
				{
					StartCoroutine(AddResourceToPinata(gameObject2));
				}
			}
		}
	}

	private IEnumerator AddResourceToPinata(GameObject resource)
	{
		yield return new WaitForSeconds(1.5f);
		lootPinataObjects.Add(resource);
	}

	private int FindClosestMesh(Vector3 position, out Vector3 center)
	{
		int result = 0;
		float num = float.PositiveInfinity;
		center = Vector3.zero;
		for (int i = 0; i < renderers.Length; i++)
		{
			if (!renderers[i].gameObject.activeInHierarchy)
			{
				continue;
			}
			Bounds encapsulatedAABB = UWE.Utils.GetEncapsulatedAABB(renderers[i].gameObject);
			float sqrMagnitude = (encapsulatedAABB.center - position).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				result = i;
				center = encapsulatedAABB.center;
				if (sqrMagnitude <= 0.5f)
				{
					break;
				}
			}
		}
		return result;
	}

	private IEnumerator ChooseRandomResourceAsync(IOut<GameObject> result)
	{
		for (int i = 0; i < resources.Length; i++)
		{
			ResourceType resourceType = resources[i];
			if (resourceType.chance >= 1f)
			{
				CoroutineTask<GameObject> task = CraftData.GetPrefabForTechTypeAsync(resourceType.techType);
				yield return task;
				result.Set(task.GetResult());
				break;
			}
			if (Player.main.gameObject.GetComponent<PlayerEntropy>().CheckChance(resourceType.techType, resourceType.chance))
			{
				CoroutineTask<GameObject> task = CraftData.GetPrefabForTechTypeAsync(resourceType.techType);
				yield return task;
				result.Set(task.GetResult());
				break;
			}
		}
	}

	private void SpawnFX(GameObject fx, Vector3 position)
	{
		UnityEngine.Object.Instantiate(fx).transform.position = position;
	}

	public void ManagedUpdate()
	{
		if (timeLastDrilled + 0.5f > Time.time)
		{
			modelRoot.transform.position = base.transform.position + modelRootOffset + new Vector3(Mathf.Sin(Time.time * 60f), Mathf.Cos(Time.time * 58f + 0.5f), Mathf.Cos(Time.time * 64f + 2f)) * 0.011f;
		}
		if (lootPinataObjects.Count <= 0 || !drillingExo)
		{
			return;
		}
		List<GameObject> list = new List<GameObject>();
		foreach (GameObject lootPinataObject in lootPinataObjects)
		{
			if (lootPinataObject == null)
			{
				list.Add(lootPinataObject);
				continue;
			}
			Vector3 b = drillingExo.transform.position + new Vector3(0f, 0.8f, 0f);
			lootPinataObject.transform.position = Vector3.Lerp(lootPinataObject.transform.position, b, Time.deltaTime * 5f);
			if (!(Vector3.Distance(lootPinataObject.transform.position, b) < 3f))
			{
				continue;
			}
			Pickupable componentInChildren = lootPinataObject.GetComponentInChildren<Pickupable>();
			if (!componentInChildren)
			{
				continue;
			}
			if (!drillingExo.storageContainer.container.HasRoomFor(componentInChildren))
			{
				if (Player.main.GetVehicle() == drillingExo)
				{
					ErrorMessage.AddMessage(Language.main.Get("ContainerCantFit"));
				}
			}
			else
			{
				string arg = Language.main.Get(componentInChildren.GetTechName());
				ErrorMessage.AddMessage(Language.main.GetFormat("VehicleAddedToStorage", arg));
				uGUI_IconNotifier.main.Play(componentInChildren.GetTechType(), uGUI_IconNotifier.AnimationType.From);
				componentInChildren.Initialize();
				InventoryItem item = new InventoryItem(componentInChildren);
				drillingExo.storageContainer.container.UnsafeAdd(item);
				componentInChildren.PlayPickupSound();
			}
			list.Add(lootPinataObject);
		}
		if (list.Count <= 0)
		{
			return;
		}
		foreach (GameObject item2 in list)
		{
			lootPinataObjects.Remove(item2);
		}
	}

	public string CompileTimeCheck(ILanguage language)
	{
		TechType dominantResourceType = GetDominantResourceType();
		if (string.IsNullOrEmpty(primaryTooltip))
		{
			return language.CheckTechType(dominantResourceType);
		}
		if (string.IsNullOrEmpty(secondaryTooltip))
		{
			return language.CheckTechType(dominantResourceType) ?? language.CheckTechTypeTooltip(dominantResourceType);
		}
		return null;
	}
}
