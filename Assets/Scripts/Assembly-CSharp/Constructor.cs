using System;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
public class Constructor : PlayerTool, IEquippable, IProtoEventListener
{
	[Serializable]
	public struct SpawnPoint
	{
		public TechType techType;

		public Transform transform;
	}

	[AssertNotNull]
	public Transform defaultSpawnPoint;

	[AssertNotNull]
	public List<SpawnPoint> spawnPoints;

	public GameObject equipped;

	public GameObject unequipped;

	public GameObject deployedOnly;

	[AssertNotNull]
	public List<Behaviour> deployedBehaviours;

	public GameObject undeployedOnly;

	public PlayerDistanceTracker playerDistanceTracker;

	public Animator animator;

	public FMOD_StudioEventEmitter releaseSound;

	public GameObject buildBotPrefab;

	public Transform[] deployedRestPositions;

	[AssertNotNull]
	public WorldForces worldForces;

	[Tooltip("Essentially how quickly this will float to the surface when deployed.")]
	[SerializeField]
	private float deployedUnderwaterGravity = -5f;

	[Tooltip("Essentially how quickly this will sink if dropped underwater.")]
	[SerializeField]
	private float undeployedUnderwaterGravity = 4f;

	[AssertNotNull]
	public GameObject climbTrigger;

	private bool buildBotsSpawned;

	private List<GameObject> buildBots = new List<GameObject>();

	private float timeDeployed;

	private GameObject buildTarget;

	private bool building;

	[NonSerialized]
	[ProtoMember(1)]
	public bool deployed;

	[AssertLocalization]
	private const string deployInWaterMessage = "DeployConstructorInWater";

	private const string deployInPrecursorMessage = "DeployConstructorInPrecursor";

	public bool IsDeployAnimationInProgress { get; private set; }

	public bool usingMenu
	{
		set
		{
			for (int i = 0; i < buildBots.Count; i++)
			{
				buildBots[i].GetComponent<ConstructorBuildBot>().usingMenu = value;
			}
		}
	}

	private void OnEnable()
	{
		Deploy(deployed);
		StartCoroutine(LoadVehiclesAsync());
	}

	private IEnumerator LoadVehiclesAsync()
	{
		yield return CraftData.GetPrefabForTechTypeAsync(TechType.Seamoth);
		yield return CraftData.GetPrefabForTechTypeAsync(TechType.Exosuit);
		yield return CraftData.GetPrefabForTechTypeAsync(TechType.RocketBase);
	}

	private void Update()
	{
		bool num = playerDistanceTracker.distanceToPlayer < 3f;
		bool flag = base.transform.position.y >= worldForces.waterDepth - 0.5f;
		bool flag2 = timeDeployed + 3f < Time.time;
		bool flag3 = (num && flag && flag2) || buildTarget != null;
		for (int i = 0; i < buildBots.Count; i++)
		{
			buildBots[i].GetComponent<ConstructorBuildBot>().launch = flag3 || buildBots[i].transform.localPosition != Vector3.zero;
		}
		if (building && buildTarget == null)
		{
			RecallBuildBots();
		}
		climbTrigger.SetActive(Ocean.GetOceanLevel() - base.transform.position.y < 0.8f && deployed && !IsDeployAnimationInProgress);
	}

	public override bool OnRightHandDown()
	{
		if (!Player.main.IsUnderwaterForSwimming() || Player.main.IsInSub() || Player.main.precursorOutOfWater)
		{
			ErrorMessage.AddMessage(Language.main.Get("DeployConstructorInWater"));
			return false;
		}
		if (PrecursorMoonPoolTrigger.inMoonpool || PrisonManager.IsInsideAquarium(base.transform.position))
		{
			ErrorMessage.AddMessage(Language.main.Get("DeployConstructorInPrecursor"));
			return false;
		}
		Vector3 forward = MainCamera.camera.transform.forward;
		pickupable.Drop(base.transform.position + forward * 0.7f + Vector3.down * 0.3f);
		GetComponent<Rigidbody>().AddForce(forward * 6.5f, ForceMode.VelocityChange);
		Deploy(value: true);
		OnDeployAnimationStart();
		LargeWorldEntity.Register(base.gameObject);
		Utils.PlayEnvSound(releaseSound, MainCamera.camera.transform.position);
		GoalManager.main.OnCustomGoalEvent("Release_Constructor");
		return true;
	}

	public override void OnDraw(Player p)
	{
		base.OnDraw(p);
		if (deployed)
		{
			Deploy(value: false);
		}
	}

	public override void OnHolster()
	{
		base.OnHolster();
		if (deployed)
		{
			Deploy(value: false);
		}
	}

	public Transform GetItemSpawnPoint(TechType techType)
	{
		for (int i = 0; i < spawnPoints.Count; i++)
		{
			SpawnPoint spawnPoint = spawnPoints[i];
			if (spawnPoint.techType == techType && spawnPoint.transform != null)
			{
				return spawnPoint.transform;
			}
		}
		return defaultSpawnPoint;
	}

	public void SendBuildBots(GameObject toBuild)
	{
		building = true;
		buildTarget = toBuild;
		BuildBotPath[] componentsInChildren = buildTarget.GetComponentsInChildren<BuildBotPath>();
		if (componentsInChildren.Length != 0)
		{
			for (int i = 0; i < buildBots.Count; i++)
			{
				int num = i % componentsInChildren.Length;
				buildBots[i].GetComponent<ConstructorBuildBot>().SetPath(componentsInChildren[num], buildTarget);
			}
			VFXConstructing componentInChildren = toBuild.GetComponentInChildren<VFXConstructing>();
			if (componentInChildren != null)
			{
				componentInChildren.informGameObject = base.gameObject;
			}
		}
		else
		{
			Debug.Log("found no build bot path for " + toBuild.name);
		}
		pickupable.isPickupable = false;
	}

	private void RecallBuildBots()
	{
		for (int i = 0; i < buildBots.Count; i++)
		{
			buildBots[i].GetComponent<ConstructorBuildBot>().FinishConstruction();
			buildBots[i].transform.parent = deployedRestPositions[i];
		}
		buildTarget = null;
		building = false;
		pickupable.isPickupable = true;
	}

	public void OnConstructionDone(GameObject constructedObject)
	{
		RecallBuildBots();
	}

	private void SetEquipped(bool state)
	{
		if (unequipped != null)
		{
			unequipped.SetActive(!state);
		}
		if (equipped != null)
		{
			equipped.SetActive(state);
		}
		if (state)
		{
			Deploy(value: false);
		}
	}

	private void Deploy(bool value)
	{
		deployed = value;
		deployedOnly.SetActive(value);
		undeployedOnly.SetActive(!value);
		worldForces.underwaterGravity = (value ? deployedUnderwaterGravity : undeployedUnderwaterGravity);
		pickupable.usePackUpIcon = value;
		for (int i = 0; i < deployedBehaviours.Count; i++)
		{
			Behaviour behaviour = deployedBehaviours[i];
			if (behaviour != null)
			{
				behaviour.enabled = value;
			}
		}
		if (animator.gameObject.activeInHierarchy)
		{
			animator.SetBool(AnimatorHashID.deployed, deployed);
		}
		UpdateBuildBots(value);
		if (value)
		{
			timeDeployed = Time.time;
		}
	}

	private void UpdateBuildBots(bool shouldSpawn)
	{
		if (buildBotsSpawned == shouldSpawn)
		{
			return;
		}
		buildBotsSpawned = shouldSpawn;
		if (buildBotsSpawned)
		{
			for (int i = 0; i < deployedRestPositions.Length; i++)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(buildBotPrefab);
				gameObject.transform.parent = deployedRestPositions[i];
				gameObject.GetComponent<ConstructorBuildBot>().botId = i + 1;
				UWE.Utils.ZeroTransform(gameObject.transform);
				buildBots.Add(gameObject);
			}
		}
		else
		{
			for (int j = 0; j < buildBots.Count; j++)
			{
				UnityEngine.Object.Destroy(buildBots[j]);
			}
			buildBots.Clear();
		}
	}

	public void OnDeployAnimationStart()
	{
		animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
		IsDeployAnimationInProgress = true;
	}

	public void OnDeployAnimationEnd()
	{
		animator.cullingMode = AnimatorCullingMode.CullCompletely;
		IsDeployAnimationInProgress = false;
	}

	public void OnEquip(GameObject sender, string slot)
	{
		SetEquipped(state: true);
	}

	public void OnUnequip(GameObject sender, string slot)
	{
		SetEquipped(state: false);
	}

	public void UpdateEquipped(GameObject sender, string slot)
	{
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		if (pickupable.attached)
		{
			deployed = false;
		}
		else
		{
			pickupable.isPickupable = true;
		}
		Deploy(deployed);
		if (deployed)
		{
			OnDeployAnimationStart();
		}
	}
}
