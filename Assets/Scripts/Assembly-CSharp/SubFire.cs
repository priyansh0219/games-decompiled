using System;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class SubFire : MonoBehaviour, IOnTakeDamage
{
	public class RoomFire
	{
		public Transform root;

		public Transform[] spawnNodes;

		public float smokeValue;

		public int fireValue;

		public RoomLinks roomLinks;

		public RoomFire(Transform root)
		{
			this.root = root;
			roomLinks = root.GetComponent<RoomLinks>();
			spawnNodes = new Transform[root.childCount];
			int num = 0;
			foreach (Transform item in root)
			{
				spawnNodes[num] = item;
				num++;
			}
		}
	}

	[AssertNotNull]
	public Transform fireSpawnsRoot;

	[AssertNotNull]
	public LiveMixin liveMixin;

	[AssertNotNull]
	public GameObject firePrefab;

	[AssertNotNull]
	public SubRoot subRoot;

	[AssertNotNull]
	public CyclopsExternalCams cyclopsExternalCams;

	[AssertNotNull]
	public Renderer smokeImpostorRenderer;

	[AssertNotNull]
	public AnimationCurve smokeImpostorRemap;

	[AssertNotNull]
	public FMOD_CustomEmitter fireMusic;

	[AssertNotNull]
	public CyclopsMotorMode cyclopsMotorMode;

	[AssertNotNull]
	public SubControl subControl;

	[AssertNotNull]
	public BehaviourLOD LOD;

	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public int fireCount;

	[NonSerialized]
	[ProtoMember(3)]
	public float curSmokeVal;

	public float smokePerTick = 0.01f;

	public float smokeFalloffPerRoom = 2f;

	public float fireSuppressionSystemDuration = 30f;

	private int oldFireCount;

	private bool fireSuppressionActive;

	private int engineOverheatValue;

	private CyclopsSmokeScreenFXController smokeController;

	private CyclopsRooms playerCurrentRoom = CyclopsRooms.LowerStorageRoom;

	private Dictionary<CyclopsRooms, RoomFire> roomFires = new Dictionary<CyclopsRooms, RoomFire>(RoomLinks.RoomsComparer);

	private List<CyclopsRooms> cycledRoomList = new List<CyclopsRooms>();

	private List<Transform> availableNodes = new List<Transform>();

	private void Start()
	{
		roomFires.Clear();
		foreach (Transform item in fireSpawnsRoot)
		{
			RoomLinks component = item.GetComponent<RoomLinks>();
			if ((bool)component)
			{
				RoomFire value = new RoomFire(item);
				roomFires.Add(component.room, value);
			}
		}
		smokeController = MainCamera.camera.GetComponent<CyclopsSmokeScreenFXController>();
		smokeController.intensity = curSmokeVal;
		Color value2 = new Color(0.2f, 0.2f, 0.2f, smokeImpostorRemap.Evaluate(curSmokeVal));
		smokeImpostorRenderer.material.SetColor(ShaderPropertyID._Color, value2);
		if (fireCount > 0)
		{
			int max = Enum.GetNames(typeof(CyclopsRooms)).Length;
			for (int i = 0; i < fireCount; i++)
			{
				CyclopsRooms key = (CyclopsRooms)UnityEngine.Random.Range(0, max);
				CreateFire(roomFires[key]);
			}
		}
		InvokeRepeating("SmokeSimulation", 3f, 3f);
		InvokeRepeating("FireSimulation", 10f, 10f);
		InvokeRepeating("EngineOverheatSimulation", 5f, 5f);
	}

	private void Update()
	{
		if (LOD.IsMinimal())
		{
			return;
		}
		float smokeValue = roomFires[playerCurrentRoom].smokeValue;
		curSmokeVal = Mathf.Lerp(curSmokeVal, smokeValue, Time.deltaTime / 2f);
		Color value = new Color(0.2f, 0.2f, 0.2f, smokeImpostorRemap.Evaluate(curSmokeVal));
		smokeImpostorRenderer.material.SetColor(ShaderPropertyID._Color, value);
		if (Player.main.currentSub == null)
		{
			smokeImpostorRenderer.enabled = true;
			if (smokeController != null)
			{
				smokeController.intensity = 0f;
			}
		}
		else if (Player.main.currentSub != subRoot)
		{
			smokeImpostorRenderer.enabled = true;
		}
		else if (smokeController != null)
		{
			if (cyclopsExternalCams.GetActive())
			{
				smokeController.intensity = 0f;
				smokeImpostorRenderer.enabled = true;
			}
			else
			{
				smokeController.intensity = curSmokeVal;
				smokeImpostorRenderer.enabled = false;
			}
		}
	}

	private void EngineOverheatSimulation()
	{
		if (!LOD.IsFull())
		{
			return;
		}
		if (cyclopsMotorMode.cyclopsMotorMode == CyclopsMotorMode.CyclopsMotorModes.Flank && subControl.appliedThrottle && cyclopsMotorMode.engineOn)
		{
			engineOverheatValue = Mathf.Min(engineOverheatValue + 1, 10);
			int num = 0;
			if (engineOverheatValue > 5)
			{
				num = UnityEngine.Random.Range(1, 4);
				subRoot.voiceNotificationManager.PlayVoiceNotification(subRoot.engineOverheatCriticalNotification);
			}
			else if (engineOverheatValue > 3)
			{
				num = UnityEngine.Random.Range(1, 6);
				subRoot.voiceNotificationManager.PlayVoiceNotification(subRoot.engineOverheatNotification);
			}
			if (num == 1)
			{
				CreateFire(roomFires[CyclopsRooms.EngineRoom]);
			}
		}
		else if (cyclopsMotorMode.cyclopsMotorMode == CyclopsMotorMode.CyclopsMotorModes.Flank)
		{
			engineOverheatValue = Mathf.Max(1, engineOverheatValue - 1);
		}
		else
		{
			engineOverheatValue = Mathf.Max(0, engineOverheatValue - 1);
		}
	}

	private void SmokeSimulation()
	{
		if (LOD.IsMinimal())
		{
			return;
		}
		int num = RecalcFireValues();
		foreach (KeyValuePair<CyclopsRooms, RoomFire> roomFire in roomFires)
		{
			RoomFire value = roomFire.Value;
			if (num == 0 || fireSuppressionActive)
			{
				float num2 = (fireSuppressionActive ? 45f : 15f);
				value.smokeValue = Mathf.Lerp(value.smokeValue, 0f, Time.deltaTime * num2);
				continue;
			}
			_ = value.roomLinks.room;
			cycledRoomList.Clear();
			RecursiveIterateSmoke(cycledRoomList, value.roomLinks.room, 0, value.fireValue);
			if (Player.main.currentSub == subRoot && value.smokeValue > 0.5f)
			{
				Player.main.GetComponent<LiveMixin>().TakeDamage(0.2f, base.transform.position, DamageType.Smoke);
			}
		}
	}

	private void FireSimulation()
	{
		if (LOD.IsMinimal() || fireSuppressionActive)
		{
			return;
		}
		int num = RecalcFireValues();
		if (num > 0 && Player.main.currentSub == subRoot)
		{
			if (!fireMusic.playing)
			{
				fireMusic.Play();
			}
		}
		else
		{
			fireMusic.Stop();
		}
		if (num == 0)
		{
			if (oldFireCount > 0)
			{
				subRoot.voiceNotificationManager.PlayVoiceNotification(subRoot.fireExtinguishedNotification);
			}
			oldFireCount = 0;
		}
		else
		{
			float originalDamage = (float)num * 15f;
			liveMixin.TakeDamage(originalDamage, default(Vector3), DamageType.Fire);
			BroadcastMessage("OnTakeFireDamage", null, SendMessageOptions.DontRequireReceiver);
			oldFireCount = num;
		}
	}

	private void RecursiveIterateSmoke(List<CyclopsRooms> cycledRooms, CyclopsRooms roomLink, int roomsAwayFromRoot, int baseFireValue)
	{
		if (cycledRooms.Contains(roomLink))
		{
			return;
		}
		foreach (KeyValuePair<CyclopsRooms, RoomFire> roomFire in roomFires)
		{
			RoomFire value = roomFire.Value;
			if (value.roomLinks.room == roomLink)
			{
				float num = ((smokeFalloffPerRoom * (float)roomsAwayFromRoot == 0f) ? 1f : (smokeFalloffPerRoom * (float)roomsAwayFromRoot));
				value.smokeValue += smokePerTick * (float)baseFireValue / num;
				value.smokeValue = Mathf.Clamp(value.smokeValue, 0f, 1f);
				roomsAwayFromRoot++;
				cycledRooms.Add(roomLink);
				CyclopsRooms[] roomLinks = value.roomLinks.roomLinks;
				foreach (CyclopsRooms roomLink2 in roomLinks)
				{
					RecursiveIterateSmoke(cycledRooms, roomLink2, roomsAwayFromRoot, baseFireValue);
				}
				break;
			}
		}
	}

	public void ActivateFireSuppressionSystem()
	{
		StartCoroutine(StartSystem());
	}

	private IEnumerator StartSystem()
	{
		subRoot.voiceNotificationManager.PlayVoiceNotification(subRoot.fireSupressionNotification, addToQueue: false, forcePlay: true);
		yield return new WaitForSeconds(3f);
		fireSuppressionActive = true;
		subRoot.fireSuppressionState = true;
		subRoot.BroadcastMessage("NewAlarmState", null, SendMessageOptions.DontRequireReceiver);
		InvokeRepeating("FireSuppressionIteration", 0f, 2f);
		Invoke("CancelFireSuppression", fireSuppressionSystemDuration);
		float num = 30f;
		base.gameObject.BroadcastMessage("TemporaryClose", num, SendMessageOptions.DontRequireReceiver);
		base.gameObject.BroadcastMessage("TemporaryLock", num, SendMessageOptions.DontRequireReceiver);
	}

	private void FireSuppressionIteration()
	{
		if (RecalcFireValues() == 0)
		{
			return;
		}
		Fire[] componentsInChildren = fireSpawnsRoot.GetComponentsInChildren<Fire>();
		foreach (Fire fire in componentsInChildren)
		{
			if (fire != null)
			{
				fire.Douse(20f);
			}
		}
	}

	private void CancelFireSuppression()
	{
		fireSuppressionActive = false;
		subRoot.fireSuppressionState = false;
		subRoot.BroadcastMessage("NewAlarmState", null, SendMessageOptions.DontRequireReceiver);
		CancelInvoke("FireSuppressionIteration");
	}

	public int RecalcFireValues()
	{
		foreach (KeyValuePair<CyclopsRooms, RoomFire> roomFire in roomFires)
		{
			RoomFire value = roomFire.Value;
			fireCount = 0;
			Transform[] spawnNodes = value.spawnNodes;
			for (int i = 0; i < spawnNodes.Length; i++)
			{
				if (spawnNodes[i].childCount != 0)
				{
					fireCount++;
				}
			}
			value.fireValue = fireCount;
		}
		int num = 0;
		foreach (KeyValuePair<CyclopsRooms, RoomFire> roomFire2 in roomFires)
		{
			RoomFire value2 = roomFire2.Value;
			num += value2.fireValue;
		}
		if (num == 0)
		{
			BroadcastMessage("ClearFireWarning", null, SendMessageOptions.DontRequireReceiver);
		}
		return num;
	}

	public void SetPlayerRoom(CyclopsRooms room)
	{
		playerCurrentRoom = room;
	}

	public void CreateFire(RoomFire startInRoom)
	{
		availableNodes.Clear();
		Transform[] spawnNodes = startInRoom.spawnNodes;
		foreach (Transform transform in spawnNodes)
		{
			if (transform.childCount == 0)
			{
				availableNodes.Add(transform);
			}
		}
		if (availableNodes.Count == 0)
		{
			return;
		}
		int index = UnityEngine.Random.Range(0, availableNodes.Count);
		Transform obj = availableNodes[index];
		startInRoom.fireValue++;
		PrefabSpawnBase component = obj.GetComponent<PrefabSpawnBase>();
		if (component == null)
		{
			return;
		}
		component.SpawnManual(delegate(GameObject fireGO)
		{
			Fire componentInChildren = fireGO.GetComponentInChildren<Fire>();
			if ((bool)componentInChildren)
			{
				componentInChildren.fireSubRoot = subRoot;
			}
		});
	}

	public void OnTakeDamage(DamageInfo damageInfo)
	{
		if (!(damageInfo.damage <= 0f))
		{
			float chance = 9f;
			if (damageInfo.type == DamageType.Normal || damageInfo.type == DamageType.Electrical)
			{
				BroadcastMessage("OnTakeCreatureDamage", null, SendMessageOptions.DontRequireReceiver);
			}
			else if (damageInfo.type == DamageType.Collide)
			{
				BroadcastMessage("OnTakeCollisionDamage", damageInfo.damage, SendMessageOptions.DontRequireReceiver);
			}
			else if (damageInfo.type == DamageType.Fire)
			{
				chance = 2f;
			}
			if (CreateFireChance(chance))
			{
				int max = Enum.GetNames(typeof(CyclopsRooms)).Length;
				CyclopsRooms key = (CyclopsRooms)UnityEngine.Random.Range(0, max);
				CreateFire(roomFires[key]);
			}
		}
	}

	public int GetFireCount()
	{
		fireCount = 0;
		foreach (KeyValuePair<CyclopsRooms, RoomFire> roomFire in roomFires)
		{
			Transform[] spawnNodes = roomFire.Value.spawnNodes;
			for (int i = 0; i < spawnNodes.Length; i++)
			{
				if (spawnNodes[i].childCount != 0)
				{
					fireCount++;
				}
			}
		}
		return fireCount;
	}

	public List<GameObject> GetAllFires()
	{
		List<GameObject> list = new List<GameObject>();
		foreach (KeyValuePair<CyclopsRooms, RoomFire> roomFire in roomFires)
		{
			Transform[] spawnNodes = roomFire.Value.spawnNodes;
			foreach (Transform transform in spawnNodes)
			{
				if (transform.childCount <= 0)
				{
					continue;
				}
				foreach (Transform item in transform)
				{
					list.Add(item.gameObject);
				}
			}
		}
		return list;
	}

	private bool CreateFireChance(float chance)
	{
		if (liveMixin.GetHealthFraction() > 0.8f)
		{
			return false;
		}
		float max = liveMixin.GetHealthFraction() * 100f;
		if (UnityEngine.Random.Range(0f, max) <= chance)
		{
			return true;
		}
		return false;
	}

	public void CyclopsDeathEvent()
	{
		smokeImpostorRenderer.gameObject.SetActive(value: false);
		fireMusic.Stop();
		CancelInvoke();
	}

	private void OnDestroy()
	{
		UnityEngine.Object.Destroy(smokeImpostorRenderer.material);
	}
}
