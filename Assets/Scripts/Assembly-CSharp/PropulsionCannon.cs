using System;
using System.Collections.Generic;
using UWE;
using UnityEngine;

public class PropulsionCannon : MonoBehaviour, IItemSelectorManager
{
	private static readonly HashSet<TechType> bannedTech = new HashSet<TechType>
	{
		TechType.PowerCell,
		TechType.ReactorRod,
		TechType.DepletedReactorRod,
		TechType.DiveSuit,
		TechType.Fins,
		TechType.Tank,
		TechType.DoubleTank,
		TechType.Battery,
		TechType.Knife,
		TechType.Drill,
		TechType.Flashlight,
		TechType.Builder,
		TechType.Compass,
		TechType.AirBladder,
		TechType.Terraformer,
		TechType.Pipe,
		TechType.PipeSurfaceFloater,
		TechType.Thermometer,
		TechType.DiveReel,
		TechType.Rebreather,
		TechType.RadiationSuit,
		TechType.RadiationHelmet,
		TechType.RadiationGloves,
		TechType.ReinforcedDiveSuit,
		TechType.ReinforcedGloves,
		TechType.Scanner,
		TechType.FireExtinguisher,
		TechType.MapRoomHUDChip,
		TechType.Welder,
		TechType.Seaglide,
		TechType.Constructor,
		TechType.Transfuser,
		TechType.StasisRifle,
		TechType.PropulsionCannon,
		TechType.Gravsphere,
		TechType.SmallStorage,
		TechType.StasisSphere,
		TechType.LaserCutter,
		TechType.LEDLight,
		TechType.DiamondBlade,
		TechType.HeatBlade,
		TechType.LithiumIonBattery,
		TechType.PlasteelTank,
		TechType.HighCapacityTank,
		TechType.UltraGlideFins,
		TechType.SwimChargeFins,
		TechType.RepulsionCannon,
		TechType.WaterFiltrationSuit,
		TechType.PowerGlide,
		TechType.HullReinforcementModule,
		TechType.HullReinforcementModule2,
		TechType.HullReinforcementModule3,
		TechType.CurrentGenerator,
		TechType.DevTestItem,
		TechType.PowerUpgradeModule,
		TechType.SmallLocker,
		TechType.CyclopsHullModule1,
		TechType.CyclopsHullModule2,
		TechType.CyclopsHullModule3,
		TechType.CyclopsShieldModule,
		TechType.CyclopsSonarModule,
		TechType.CyclopsSeamothRepairModule,
		TechType.CyclopsFireSuppressionModule,
		TechType.CyclopsDecoyModule,
		TechType.CyclopsThermalReactorModule,
		TechType.Audiolog,
		TechType.Signal,
		TechType.SeamothReinforcementModule,
		TechType.VehiclePowerUpgradeModule,
		TechType.SeamothSolarCharge,
		TechType.VehicleStorageModule,
		TechType.SeamothElectricalDefense,
		TechType.VehicleArmorPlating,
		TechType.LootSensorMetal,
		TechType.LootSensorLithium,
		TechType.LootSensorFragment,
		TechType.SeamothTorpedoModule,
		TechType.SeamothSonarModule,
		TechType.VehicleHullModule1,
		TechType.VehicleHullModule2,
		TechType.VehicleHullModule3,
		TechType.ExoHullModule1,
		TechType.ExoHullModule2,
		TechType.ExosuitJetUpgradeModule,
		TechType.ExosuitDrillArmModule,
		TechType.ExosuitThermalReactorModule,
		TechType.ExosuitClawArmModule,
		TechType.ExosuitPropulsionArmModule,
		TechType.ExosuitGrapplingArmModule,
		TechType.ExosuitTorpedoArmModule,
		TechType.MapRoomUpgradeScanRange,
		TechType.MapRoomUpgradeScanSpeed,
		TechType.StalkerTooth,
		TechType.FilteredWater,
		TechType.DisinfectedWater,
		TechType.FirstAidKit,
		TechType.BigFilteredWater,
		TechType.Poster,
		TechType.PrecursorIonBattery,
		TechType.PrecursorIonPowerCell
	};

	[AssertLocalization(1)]
	private const string unloadFormat = "PropulsionCannonUnload";

	[AssertLocalization]
	private const string unloadedKey = "PropulsionCannonUnloaded";

	private const bool debug = false;

	private const float energyPerSecond = 0.7f;

	private const float energyPerShot = 4f;

	private const float cameraDistance = 2.5f;

	private const float grabAnimationDuration = 1f;

	private const float grabTraceRadius = 1.2f;

	private GameObject lastValidTarget;

	private GameObject _grabbedObject;

	private GameObject _fistUsedGrabbedObject;

	private HashSet<GameObject> launchedObjects = new HashSet<GameObject>();

	private Vector3 targetPosition = Vector3.zero;

	private Vector3 grabbedObjectCenter = Vector3.zero;

	private VFXElectricLine[] elecLines;

	private float cannonGlow;

	private int shaderParamID;

	private float timeGrabbed;

	private float timeLastValidTargetSoundPlayed;

	private List<IPropulsionCannonAmmo> iammo = new List<IPropulsionCannonAmmo>();

	private StorageSlot storageSlot = new StorageSlot(null);

	private List<IItemsContainer> srcContainers;

	private static List<GameObject> checkedObjects = new List<GameObject>();

	public VFXController fxControl;

	public GameObject grabbedEffect;

	public GameObject fxTrailPrefab;

	public GameObject fxBeam;

	public Transform muzzle;

	public Renderer fpCannonModelRenderer;

	public float firstUseGrabDelay = 2.5f;

	public float firstUseShootDelay = 4f;

	public GameObject _debugGrabbedObject;

	public FMODAsset validTargetSound;

	public EnergyInterface energyInterface;

	public bool usingCannon;

	public FMOD_CustomLoopingEmitter grabbingSound;

	public FMODAsset grabFailSound;

	public FMODAsset shootSound;

	public Animator animator;

	[NonSerialized]
	[HideInInspector]
	public Vector3 localObjectOffset = Vector3.zero;

	public float shootForce = 50f;

	public float attractionForce = 140f;

	public float massScalingFactor = 0.02f;

	public float pickupDistance = 18f;

	public float maxMass = 1200f;

	public float maxAABBVolume = 120f;

	protected static Dictionary<TechType, List<InventoryItem>> sGroups = new Dictionary<TechType, List<InventoryItem>>(TechTypeExtensions.sTechTypeComparer);

	public bool canGrab => lastValidTarget != null;

	public GameObject grabbedObject
	{
		get
		{
			return _grabbedObject;
		}
		set
		{
			_grabbedObject = value;
			Pickupable pickupable = storageSlot.storedItem?.item;
			Pickupable pickupable2 = ((_grabbedObject == null) ? null : _grabbedObject.GetComponent<Pickupable>());
			if (pickupable != null)
			{
				if (pickupable2 != null)
				{
					if (pickupable != pickupable2)
					{
						storageSlot.RemoveItem();
						InventoryItem item = new InventoryItem(pickupable2);
						storageSlot.AddItem(item);
					}
				}
				else
				{
					storageSlot.RemoveItem();
				}
			}
			else if (pickupable2 != null)
			{
				InventoryItem item2 = new InventoryItem(pickupable2);
				storageSlot.AddItem(item2);
			}
			if (_grabbedObject != null)
			{
				grabbingSound.Play();
				grabbedEffect.SetActive(value: true);
				fxBeam.SetActive(value: true);
				grabbedEffect.transform.parent = null;
				grabbedEffect.transform.position = _grabbedObject.transform.position;
				UpdateTargetPosition();
				timeGrabbed = Time.time;
			}
			else
			{
				grabbingSound.Stop();
				grabbedEffect.SetActive(value: false);
				fxBeam.SetActive(value: false);
				grabbedEffect.transform.parent = base.transform;
			}
			if (MainGameController.Instance != null)
			{
				if (_grabbedObject != null)
				{
					MainGameController.Instance.RegisterHighFixedTimestepBehavior(this);
				}
				else
				{
					MainGameController.Instance.DeregisterHighFixedTimestepBehavior(this);
				}
			}
		}
	}

	public GameObject firstUseGrabbedObject
	{
		get
		{
			return _fistUsedGrabbedObject;
		}
		set
		{
			if (_fistUsedGrabbedObject != value)
			{
				_fistUsedGrabbedObject = value;
				if (_fistUsedGrabbedObject != null)
				{
					grabbingSound.Play();
					grabbedEffect.transform.localScale = Vector3.one * 0.25f;
					grabbedEffect.SetActive(value: true);
					fxBeam.SetActive(value: true);
					grabbedEffect.transform.parent = null;
					grabbedEffect.transform.position = _fistUsedGrabbedObject.transform.position;
				}
				else
				{
					grabbingSound.Stop();
					grabbedEffect.transform.localScale = Vector3.one;
					grabbedEffect.SetActive(value: false);
					fxBeam.SetActive(value: false);
					grabbedEffect.transform.parent = base.transform;
				}
			}
		}
	}

	private void Start()
	{
		elecLines = fxBeam.GetComponentsInChildren<VFXElectricLine>(includeInactive: true);
		shaderParamID = Shader.PropertyToID("_OverlayStrength");
	}

	public bool Filter(InventoryItem item)
	{
		if (item == null || item.item == null || base.transform.IsChildOf(item.item.GetComponent<Transform>()))
		{
			return false;
		}
		TechType techType = item.item.GetTechType();
		if (bannedTech.Contains(techType))
		{
			return false;
		}
		if (!IsAllowedToGrab(item.item.gameObject))
		{
			return false;
		}
		return true;
	}

	public int Sort(List<InventoryItem> items)
	{
		InventoryItem storedItem = storageSlot.storedItem;
		bool flag = storedItem != null && items.Remove(storedItem);
		sGroups.Clear();
		int i = 0;
		for (int count = items.Count; i < count; i++)
		{
			InventoryItem inventoryItem = items[i];
			TechType techType = inventoryItem.item.GetTechType();
			if (!sGroups.TryGetValue(techType, out var value))
			{
				value = new List<InventoryItem>();
				sGroups.Add(techType, value);
			}
			value.Add(inventoryItem);
		}
		items.Clear();
		if (flag)
		{
			items.Insert(0, storedItem);
		}
		Dictionary<TechType, List<InventoryItem>>.Enumerator enumerator = sGroups.GetEnumerator();
		while (enumerator.MoveNext())
		{
			List<InventoryItem> value2 = enumerator.Current.Value;
			items.AddRange(value2);
		}
		sGroups.Clear();
		if (!flag)
		{
			return -1;
		}
		return 0;
	}

	public string GetText(InventoryItem item)
	{
		if (item != null)
		{
			Pickupable item2 = item.item;
			return Language.main.Get(item2.GetTechName());
		}
		if (grabbedObject != null)
		{
			Pickupable component = grabbedObject.GetComponent<Pickupable>();
			if (component != null)
			{
				string arg = Language.main.Get(component.GetTechName());
				return Language.main.GetFormat("PropulsionCannonUnload", arg);
			}
		}
		return Language.main.Get("PropulsionCannonUnloaded");
	}

	public void Select(InventoryItem newItem)
	{
		bool flag = false;
		InventoryItem storedItem = storageSlot.storedItem;
		if (storedItem != null)
		{
			if (newItem != null)
			{
				IItemsContainer container = newItem.container;
				if (storedItem != newItem)
				{
					storageSlot.RemoveItem();
					if (storageSlot.AddItem(newItem))
					{
						if (AddToAnyContainer(srcContainers, storedItem))
						{
							flag = true;
						}
						else
						{
							storageSlot.RemoveItem();
							container?.AddItem(newItem);
							storageSlot.AddItem(storedItem);
						}
					}
					else
					{
						container?.AddItem(newItem);
						storageSlot.AddItem(storedItem);
					}
				}
			}
			else
			{
				storageSlot.RemoveItem();
				if (AddToAnyContainer(srcContainers, storedItem))
				{
					grabbedObject = null;
				}
				else
				{
					storageSlot.AddItem(storedItem);
				}
			}
		}
		else if (newItem != null)
		{
			IItemsContainer container2 = newItem.container;
			if (storageSlot.AddItem(newItem))
			{
				flag = true;
			}
			else
			{
				container2?.AddItem(newItem);
			}
		}
		if (flag)
		{
			Pickupable item = newItem.item;
			GameObject gameObject = item.gameObject;
			gameObject.SetActive(value: true);
			targetPosition = GetObjectPosition(gameObject);
			item.Drop(targetPosition, Vector3.zero);
			GrabObject(gameObject);
		}
		srcContainers = null;
	}

	private bool AddToAnyContainer(List<IItemsContainer> containers, InventoryItem item)
	{
		int num = containers?.Count ?? 0;
		for (int i = 0; i < num; i++)
		{
			IItemsContainer itemsContainer = containers[i];
			if (itemsContainer != null && itemsContainer.AddItem(item))
			{
				return true;
			}
		}
		return false;
	}

	private Bounds GetAABB(GameObject target)
	{
		FixedBounds component = target.GetComponent<FixedBounds>();
		if (component != null)
		{
			return component.bounds;
		}
		return UWE.Utils.GetEncapsulatedAABB(target, 20);
	}

	private Vector3 GetObjectPosition(GameObject go)
	{
		Camera camera = MainCamera.camera;
		Vector3 vector = Vector3.zero;
		float num = 0f;
		if (go != null)
		{
			Bounds aABB = GetAABB(go);
			vector = go.transform.position - aABB.center;
			Ray ray = new Ray(aABB.center, camera.transform.forward);
			float distance = 0f;
			if (aABB.IntersectRay(ray, out distance))
			{
				num = Mathf.Abs(distance);
			}
			grabbedObjectCenter = aABB.center;
		}
		Vector3 position = Vector3.forward * (2.5f + num) + localObjectOffset;
		return camera.transform.TransformPoint(position) + vector;
	}

	private void UpdateTargetPosition()
	{
		targetPosition = GetObjectPosition(grabbedObject);
	}

	public void OnAmmoHandlerDestroyed(GameObject AmmoHandlerGO)
	{
		launchedObjects.Remove(AmmoHandlerGO);
		if (AmmoHandlerGO == grabbedObject)
		{
			grabbedObject = null;
		}
	}

	public void GrabObject(GameObject target)
	{
		grabbedObject = target;
		UWE.Utils.SetIsKinematicAndUpdateInterpolation(grabbedObject.GetComponent<Rigidbody>(), isKinematic: false);
		PropulseCannonAmmoHandler propulseCannonAmmoHandler = target.GetComponent<PropulseCannonAmmoHandler>();
		if (propulseCannonAmmoHandler == null)
		{
			propulseCannonAmmoHandler = target.AddComponent<PropulseCannonAmmoHandler>();
			propulseCannonAmmoHandler.fxTrailPrefab = fxTrailPrefab;
		}
		propulseCannonAmmoHandler.ResetHandler();
		propulseCannonAmmoHandler.SetCannon(this);
	}

	public void ReleaseGrabbedObject()
	{
		if (grabbedObject != null)
		{
			PropulseCannonAmmoHandler component = grabbedObject.GetComponent<PropulseCannonAmmoHandler>();
			if (component != null && component.IsGrabbedBy(this))
			{
				component.UndoChanges();
				UnityEngine.Object.Destroy(component);
			}
			grabbedObject = null;
		}
	}

	public GameObject GetInteractableGrabbedObject()
	{
		if (grabbedObject != null && (grabbedObject.transform.position - targetPosition).magnitude < 1.4f)
		{
			PickupableStorage componentInChildren = grabbedObject.GetComponentInChildren<PickupableStorage>();
			if (!(componentInChildren != null))
			{
				return grabbedObject;
			}
			return componentInChildren.gameObject;
		}
		return null;
	}

	public bool IsGrabbingObject()
	{
		return grabbedObject != null;
	}

	private bool CheckLineOfSight(GameObject obj, Vector3 a, Vector3 b)
	{
		bool result = true;
		int num = UWE.Utils.RaycastIntoSharedBuffer(a, Vector3.Normalize(b - a), (b - a).magnitude, ~(1 << LayerMask.NameToLayer("Player")), QueryTriggerInteraction.Ignore);
		bool flag = false;
		for (int i = 0; i < num; i++)
		{
			if (flag)
			{
				break;
			}
			GameObject entityRoot = UWE.Utils.GetEntityRoot(UWE.Utils.sharedHitBuffer[i].collider.gameObject);
			if (!entityRoot)
			{
				entityRoot = UWE.Utils.sharedHitBuffer[i].collider.gameObject;
			}
			if (entityRoot.GetComponentInChildren<Player>() == null && entityRoot != obj)
			{
				result = false;
				flag = true;
			}
		}
		return result;
	}

	private bool IsAllowedToGrab(GameObject go)
	{
		bool result = true;
		go.GetComponents(iammo);
		for (int i = 0; i < iammo.Count; i++)
		{
			if (!iammo[i].GetAllowedToGrab())
			{
				result = false;
				break;
			}
		}
		iammo.Clear();
		return result;
	}

	public bool ValidateObject(GameObject go)
	{
		if (!go.activeSelf || !go.activeInHierarchy)
		{
			Debug.Log("object is inactive");
			return false;
		}
		Rigidbody component = go.GetComponent<Rigidbody>();
		if (component == null || component.mass > maxMass)
		{
			return false;
		}
		Pickupable component2 = go.GetComponent<Pickupable>();
		bool flag = false;
		if (component2 != null)
		{
			flag = component2.attached;
		}
		if (IsAllowedToGrab(go) && energyInterface.hasCharge)
		{
			return !flag;
		}
		return false;
	}

	public bool ValidateNewObject(GameObject go, Vector3 hitPos, bool checkLineOfSight = true)
	{
		if (go.GetComponent<PropulseCannonAmmoHandler>() != null)
		{
			return false;
		}
		if (!ValidateObject(go))
		{
			return false;
		}
		if (checkLineOfSight && !CheckLineOfSight(go, MainCamera.camera.transform.position, hitPos))
		{
			return false;
		}
		if (go.GetComponent<Pickupable>() != null)
		{
			return true;
		}
		Bounds aABB = GetAABB(go);
		return aABB.size.x * aABB.size.y * aABB.size.z <= maxAABBVolume;
	}

	public GameObject TraceForGrabTarget()
	{
		Vector3 position = MainCamera.camera.transform.position;
		int layerMask = ~(1 << LayerMask.NameToLayer("Player"));
		int num = UWE.Utils.SpherecastIntoSharedBuffer(position, 1.2f, MainCamera.camera.transform.forward, pickupDistance, layerMask);
		GameObject result = null;
		float num2 = float.PositiveInfinity;
		checkedObjects.Clear();
		for (int i = 0; i < num; i++)
		{
			RaycastHit raycastHit = UWE.Utils.sharedHitBuffer[i];
			if (raycastHit.collider.isTrigger && raycastHit.collider.gameObject.layer != LayerMask.NameToLayer("Useable"))
			{
				continue;
			}
			GameObject entityRoot = UWE.Utils.GetEntityRoot(raycastHit.collider.gameObject);
			if (!(entityRoot != null) || checkedObjects.Contains(entityRoot))
			{
				continue;
			}
			if (!launchedObjects.Contains(entityRoot))
			{
				float sqrMagnitude = (raycastHit.point - position).sqrMagnitude;
				if (sqrMagnitude < num2 && ValidateNewObject(entityRoot, raycastHit.point))
				{
					result = entityRoot;
					num2 = sqrMagnitude;
				}
			}
			checkedObjects.Add(entityRoot);
		}
		return result;
	}

	public bool HasChargeForShot()
	{
		int sourceCount;
		return energyInterface.TotalCanProvide(out sourceCount) > 4f;
	}

	public bool OnShoot()
	{
		if (grabbedObject != null)
		{
			energyInterface.GetValues(out var charge, out var _);
			float num = Mathf.Min(1f, charge / 4f);
			Rigidbody component = grabbedObject.GetComponent<Rigidbody>();
			float num2 = 1f + component.mass * massScalingFactor;
			Vector3 vector = MainCamera.camera.transform.forward * shootForce * num / num2;
			Vector3 velocity = component.velocity;
			if (Vector3.Dot(velocity, vector) < 0f)
			{
				component.velocity = vector;
			}
			else
			{
				component.velocity = velocity * 0.3f + vector;
			}
			grabbedObject.GetComponent<PropulseCannonAmmoHandler>().OnShot();
			launchedObjects.Add(grabbedObject);
			grabbedObject = null;
			energyInterface.ConsumeEnergy(4f);
			Utils.PlayFMODAsset(shootSound, base.transform);
			fxControl.Play(0);
		}
		else
		{
			GameObject gameObject = TraceForGrabTarget();
			if (gameObject != null)
			{
				GrabObject(gameObject);
			}
			else
			{
				Utils.PlayFMODAsset(grabFailSound, base.transform);
			}
		}
		return true;
	}

	public bool OnReload(List<IItemsContainer> containers)
	{
		List<IItemsContainer> list = new List<IItemsContainer>(containers);
		list.Add(storageSlot);
		if (uGUI_ItemSelector.HasCompatibleItems(this, list))
		{
			srcContainers = containers;
			uGUI.main.itemSelector.Initialize(this, SpriteManager.Get(SpriteManager.Group.Item, "nobattery"), list);
			return true;
		}
		return false;
	}

	private float EaseOutElastic(float t)
	{
		return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - 0.075f) * ((float)System.Math.PI * 2f) / 0.3f) + 1f;
	}

	public void PlayShootFxAndSound()
	{
		Utils.PlayFMODAsset(shootSound, base.transform);
		fxControl.Play(0);
		firstUseGrabbedObject = null;
	}

	public void PlayGrabFxAndSound()
	{
		firstUseGrabbedObject = Player.main.GetLeftHandBone().gameObject;
	}

	public void PlayFirstUseFxAndSound()
	{
		Invoke("PlayGrabFxAndSound", firstUseGrabDelay);
		Invoke("PlayShootFxAndSound", firstUseShootDelay);
	}

	public void StopFirstUseFxAndSound()
	{
		CancelInvoke("PlayGrabFxAndSound");
		CancelInvoke("PlayShootFxAndSound");
		firstUseGrabbedObject = null;
		fxControl.Stop();
	}

	private void FixedUpdate()
	{
		if (grabbedObject != null)
		{
			if (!ValidateObject(grabbedObject) || pickupDistance * 1.5f < (grabbedObject.transform.position - MainCamera.camera.transform.position).magnitude)
			{
				ReleaseGrabbedObject();
			}
			else
			{
				Rigidbody component = grabbedObject.GetComponent<Rigidbody>();
				Vector3 value = targetPosition - grabbedObject.transform.position;
				float magnitude = value.magnitude;
				float num = Mathf.Clamp(magnitude, 1f, 4f);
				Vector3 vector = component.velocity + Vector3.Normalize(value) * attractionForce * num * Time.deltaTime / (1f + component.mass * massScalingFactor);
				Vector3 amount = vector * (10f + Mathf.Pow(Mathf.Clamp01(1f - magnitude), 1.75f) * 40f) * Time.deltaTime;
				vector = UWE.Utils.SlerpVector(vector, Vector3.zero, amount);
				component.velocity = vector;
			}
		}
		if (firstUseGrabbedObject != null)
		{
			grabbedEffect.transform.position = firstUseGrabbedObject.transform.position;
		}
	}

	private void OnDisable()
	{
		MainGameController.Instance?.DeregisterHighFixedTimestepBehavior(this);
	}

	private void Update()
	{
		if (grabbedObject != null)
		{
			if (grabbedObject.GetComponent<Rigidbody>() != null)
			{
				for (int i = 0; i < elecLines.Length; i++)
				{
					VFXElectricLine obj = elecLines[i];
					obj.origin = muzzle.position;
					obj.target = grabbedObjectCenter;
					obj.originVector = muzzle.forward;
				}
			}
			energyInterface.ConsumeEnergy(Time.deltaTime * 0.7f);
			UpdateTargetPosition();
			if ((bool)grabbedEffect)
			{
				grabbedEffect.transform.position = grabbedObjectCenter;
			}
		}
		if (firstUseGrabbedObject != null)
		{
			for (int j = 0; j < elecLines.Length; j++)
			{
				VFXElectricLine obj2 = elecLines[j];
				obj2.origin = muzzle.position;
				obj2.target = firstUseGrabbedObject.transform.position;
				obj2.originVector = muzzle.forward;
			}
		}
	}

	public void UpdateActive()
	{
		if (grabbedObject == null)
		{
			GameObject gameObject = TraceForGrabTarget();
			if (lastValidTarget != gameObject && gameObject != null && timeLastValidTargetSoundPlayed + 2f <= Time.time)
			{
				Utils.PlayFMODAsset(validTargetSound, base.transform);
				timeLastValidTargetSoundPlayed = Time.time;
			}
			lastValidTarget = gameObject;
		}
		if (fpCannonModelRenderer != null)
		{
			if (grabbedObject != null)
			{
				cannonGlow = 1f;
			}
			else
			{
				cannonGlow -= Time.deltaTime;
			}
			fpCannonModelRenderer.material.SetFloat(shaderParamID, Mathf.Clamp01(cannonGlow));
		}
		animator.SetBool("use_tool", usingCannon);
		animator.SetBool("cangrab_propulsioncannon", canGrab || grabbedObject != null);
		HandReticle.main.SetIcon(HandReticle.IconType.Default, (canGrab && grabbedObject == null) ? 1.5f : 1f);
	}
}
