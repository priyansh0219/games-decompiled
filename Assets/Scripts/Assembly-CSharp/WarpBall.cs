using System.Collections;
using System.Collections.Generic;
using UWE;
using UnityEngine;

public class WarpBall : MonoBehaviour, IPropulsionCannonAmmo
{
	public float warpRadius = 4f;

	public float warpDistance = 10f;

	public float damage = 10f;

	public float maxLifeTime = 5f;

	public float warpedInCreatureLifeTime = 20f;

	public float minDistanceToWarper = 10f;

	[AssertNotNull]
	public GameObject warpOutEffectPrefab;

	[AssertNotNull]
	public GameObject warpInEffectPrefab;

	[AssertNotNull]
	public Material warpedMaterial;

	public float overlayFXduration = 4f;

	public FMOD_StudioEventEmitter warpOutSound;

	public FMOD_StudioEventEmitter warpInSound;

	private GameObject warperGO;

	private float detonationTime;

	private bool grabbedByPropCannon;

	private bool wasShot;

	private bool wasDetonate;

	public WarperData warperData;

	void IPropulsionCannonAmmo.OnGrab()
	{
		grabbedByPropCannon = true;
	}

	void IPropulsionCannonAmmo.OnShoot()
	{
		wasShot = true;
		grabbedByPropCannon = false;
		SetDetonationTime();
	}

	void IPropulsionCannonAmmo.OnImpact()
	{
	}

	void IPropulsionCannonAmmo.OnRelease()
	{
		if (!wasShot)
		{
			SetDetonationTime();
		}
		grabbedByPropCannon = false;
	}

	bool IPropulsionCannonAmmo.GetAllowedToGrab()
	{
		return true;
	}

	bool IPropulsionCannonAmmo.GetAllowedToShoot()
	{
		return true;
	}

	private void Start()
	{
		SetDetonationTime();
	}

	private void Update()
	{
		if (!grabbedByPropCannon && Time.time >= detonationTime)
		{
			Detonate();
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!grabbedByPropCannon)
		{
			Detonate();
		}
	}

	private void Detonate()
	{
		if (!wasDetonate)
		{
			wasDetonate = true;
			if (Time.time >= detonationTime && warperGO != null && (warperGO.transform.position - base.transform.position).sqrMagnitude > minDistanceToWarper * minDistanceToWarper)
			{
				StartCoroutine(WarpIn());
				return;
			}
			WarpOut();
			Object.Destroy(base.gameObject);
		}
	}

	private void WarpOut()
	{
		Vector3 positionBeforeWarper = GetPositionBeforeWarper();
		HashSet<GameObject> hashSet = new HashSet<GameObject>();
		int num = UWE.Utils.OverlapSphereIntoSharedBuffer(base.transform.position, warpRadius);
		for (int i = 0; i < num; i++)
		{
			Collider collider = UWE.Utils.sharedColliderBuffer[i];
			GameObject gameObjectToWarp = GetGameObjectToWarp(collider);
			if (gameObjectToWarp != null)
			{
				hashSet.Add(gameObjectToWarp);
			}
		}
		foreach (GameObject item in hashSet)
		{
			Warp(item, positionBeforeWarper);
		}
		Utils.SpawnPrefabAt(warpOutEffectPrefab, null, base.transform.position);
		if (warpOutSound != null)
		{
			Utils.PlayEnvSound(warpOutSound, base.transform.position);
		}
		if (hashSet.Count > 0)
		{
			Utils.SpawnPrefabAt(warpInEffectPrefab, null, positionBeforeWarper);
			if (warpInSound != null)
			{
				Utils.PlayEnvSound(warpInSound, positionBeforeWarper);
			}
		}
	}

	private IEnumerator WarpIn()
	{
		string biomeName = "";
		if ((bool)LargeWorld.main)
		{
			biomeName = LargeWorld.main.GetBiome(base.transform.position);
		}
		WarperData.WarpInCreature creatureToWarp = warperData.GetRandomCreature(biomeName);
		if (creatureToWarp == null)
		{
			WarpOut();
		}
		else
		{
			if (warpInEffectPrefab != null)
			{
				Utils.SpawnPrefabAt(warpInEffectPrefab, null, base.transform.position);
			}
			if (warpInSound != null)
			{
				Utils.PlayEnvSound(warpInSound, base.transform.position);
			}
			int creaturesNum = Random.Range(creatureToWarp.minNum, creatureToWarp.maxNum + 1);
			for (int i = 0; i < creaturesNum; i++)
			{
				if (creatureToWarp.techType != 0)
				{
					yield return WarpInCreatureAsync(creatureToWarp.techType);
				}
			}
		}
		Object.Destroy(base.gameObject);
	}

	private IEnumerator WarpInCreatureAsync(TechType techType)
	{
		TaskResult<GameObject> result = new TaskResult<GameObject>();
		yield return CraftData.InstantiateFromPrefabAsync(techType, result);
		GameObject gameObject = result.Get();
		gameObject.transform.position = base.transform.position + Random.insideUnitSphere * 0.5f;
		WarpedInCreature warpedInCreature = gameObject.AddComponent<WarpedInCreature>();
		warpedInCreature.SetLifeTime(warpedInCreatureLifeTime + Random.Range(-2f, 2f));
		warpedInCreature.warpOutEffectPrefab = warpOutEffectPrefab;
		warpedInCreature.warpedMaterial = warpedMaterial;
		warpedInCreature.overlayFXduration = overlayFXduration;
		if (warpOutSound != null)
		{
			warpedInCreature.warpOutSound = warpOutSound.asset;
		}
		Creature component = gameObject.GetComponent<Creature>();
		if (component != null)
		{
			component.SetFriend(warperGO);
		}
		Object.Destroy(GetComponent<InfectedMixin>());
		if (LargeWorld.main != null && LargeWorld.main.streamer != null && LargeWorld.main.streamer.cellManager != null)
		{
			base.transform.parent = null;
			LargeWorld.main.streamer.cellManager.UnregisterEntity(gameObject);
		}
	}

	private void SetDetonationTime()
	{
		detonationTime = Time.time + maxLifeTime;
	}

	private Vector3 GetPositionBeforeWarper()
	{
		if (warperGO == null || !warperGO.activeInHierarchy)
		{
			return GetRandomNearPosition();
		}
		Transform transform = warperGO.transform;
		if (Physics.Raycast(transform.position, transform.forward, out var hitInfo, 8f))
		{
			return hitInfo.point;
		}
		return transform.position + transform.forward * 8f;
	}

	private Vector3 GetRandomNearPosition()
	{
		Vector3 vector = Vector3.zero;
		float maxDistance = 0f;
		for (int i = 0; i < 10; i++)
		{
			vector = Random.onUnitSphere;
			if (!Physics.Raycast(base.transform.position, vector, warpDistance))
			{
				return base.transform.position + vector * warpDistance;
			}
		}
		Physics.Raycast(base.transform.position, vector, out var hitInfo, maxDistance);
		return hitInfo.point - vector.normalized;
	}

	private GameObject GetGameObjectToWarp(Collider collider)
	{
		if (collider.gameObject == base.gameObject)
		{
			return null;
		}
		Rigidbody attachedRigidbody = collider.attachedRigidbody;
		if (attachedRigidbody == null || attachedRigidbody.gameObject == null)
		{
			return null;
		}
		GameObject gameObject = attachedRigidbody.gameObject;
		Vehicle component = gameObject.GetComponent<Vehicle>();
		if (component != null)
		{
			Player componentInChildren = component.playerPosition.GetComponentInChildren<Player>();
			if (componentInChildren != null)
			{
				return componentInChildren.gameObject;
			}
			return null;
		}
		if (Player.main.gameObject == gameObject)
		{
			if (!Player.main.IsInside())
			{
				return gameObject;
			}
			return null;
		}
		if (attachedRigidbody.mass > 500f || attachedRigidbody.isKinematic)
		{
			return null;
		}
		WaterParkItem component2 = gameObject.GetComponent<WaterParkItem>();
		if (component2 != null && component2.IsInsideWaterPark())
		{
			return null;
		}
		if (gameObject.GetComponent<Creature>() != null || gameObject.GetComponent<Pickupable>() != null)
		{
			return gameObject;
		}
		return null;
	}

	private void Warp(GameObject target, Vector3 position)
	{
		Player component = target.GetComponent<Player>();
		if (component != null && component.GetMode() == Player.Mode.LockedPiloting && component.GetVehicle() != null)
		{
			component.ExitLockedMode();
		}
		if (component != null)
		{
			MainCamera.camera.GetComponent<WarpScreenFXController>().StartWarp();
		}
		ApplyAndForgetOverlayFX(target);
		target.transform.position = position + Random.insideUnitSphere * 0.5f;
		LiveMixin component2 = target.GetComponent<LiveMixin>();
		if (component2 != null)
		{
			component2.TakeDamage(damage, base.transform.position);
		}
		if (warperGO != null && (component != null || target.CompareTag("Creature")))
		{
			LastTarget component3 = warperGO.GetComponent<LastTarget>();
			if (component3 != null)
			{
				component3.SetTarget(target);
			}
		}
	}

	private void OnProjectileCasted(Creature creature)
	{
		if (creature != null)
		{
			warperGO = creature.gameObject;
		}
	}

	private void ApplyAndForgetOverlayFX(GameObject targetObj)
	{
		targetObj.AddComponent<VFXOverlayMaterial>().ApplyAndForgetOverlay(warpedMaterial, "VFXOverlay: Warped", Color.clear, overlayFXduration);
	}
}
