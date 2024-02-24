using System.Collections;
using UWE;
using UnityEngine;

public class CyclopsDestructionEvent : MonoBehaviour
{
	[AssertNotNull]
	public SubRoot subRoot;

	[AssertNotNull]
	public LiveMixin subLiveMixin;

	[AssertNotNull]
	public Transform lootSpawnPoints;

	[AssertNotNull]
	public FMOD_CustomEmitter explodeSFX;

	[AssertNotNull]
	public GameObject[] intact;

	[AssertNotNull]
	public GameObject[] destroyed;

	[AssertNotNull]
	public VFXController fxControl;

	[AssertNotNull]
	public GameObject interiorPlayerFx;

	[AssertNotNull]
	public PingInstance pingInstance;

	[AssertNotNull]
	public GameObject beaconPrefab;

	public float interiorFxStartOffset = 18f;

	public float interiorFxEndOffset = -25f;

	public float interiorFxDuration = 5f;

	public float swapModelsDelay = 2.5f;

	private GameObject interiorFxGo;

	private float animTime;

	private GameObject playerFxInstance;

	[AssertLocalization]
	private const string damageLabelKey = "CyclopsDamageLabel";

	private void Start()
	{
		Player.main.playerRespawnEvent.AddHandler(base.gameObject, OnRespawn);
		DevConsole.RegisterConsoleCommand(this, "destroycyclops");
		DevConsole.RegisterConsoleCommand(this, "restorecyclops");
		if (subLiveMixin.health <= 0f)
		{
			SwapToDamagedModels();
			ToggleSinking(isSinking: true);
		}
	}

	private void RestoreCyclops()
	{
		fxControl.Stop();
		Object.Destroy(playerFxInstance);
		RestoreModels();
		ToggleSinking(isSinking: false);
	}

	public void DestroyCyclops()
	{
		explodeSFX.Play();
		animTime = 0f;
		if (Player.main.currentSub == subRoot)
		{
			fxControl.Play(0);
		}
		else
		{
			fxControl.Play(1);
		}
		Player.main.TryEject();
		interiorFxGo = fxControl.emitters[0].instanceGO;
		Invoke("SwapToDamagedModels", swapModelsDelay);
		StartCoroutine(SpawnLootAsync());
		subRoot.BroadcastMessage("OnKill");
		float amountConsumed = 0f;
		subRoot.powerRelay.ConsumeEnergy(99999f, out amountConsumed);
		subRoot.BroadcastMessage("CyclopsDeathEvent", null, SendMessageOptions.DontRequireReceiver);
		Vector3 position = subRoot.transform.position + new Vector3(0f, 15f, 0f);
		Beacon component = Object.Instantiate(beaconPrefab, position, Quaternion.identity).GetComponent<Beacon>();
		if ((bool)component)
		{
			string subName = subRoot.GetSubName();
			string text = Language.main.Get("CyclopsDamageLabel");
			string label = ((!(subName == "")) ? $"{text}: {subName}" : text);
			component.label = label;
		}
	}

	private void Update()
	{
		if (Player.main.currentSub != subRoot)
		{
			if (!(fxControl.emitters[0].fxPS == null) && fxControl.emitters[0].fxPS.isPlaying)
			{
				fxControl.Stop(0);
			}
		}
		else if (interiorFxGo != null)
		{
			animTime += Time.deltaTime;
			Vector3 localPosition = interiorFxGo.transform.localPosition;
			localPosition.z = Mathf.Lerp(interiorFxStartOffset, interiorFxEndOffset, Mathf.Clamp01(animTime / interiorFxDuration));
			interiorFxGo.transform.localPosition = localPosition;
			if (animTime >= interiorFxDuration)
			{
				fxControl.Stop(0);
			}
			Vector3 localPlayerPos = Utils.GetLocalPlayerPos();
			if (interiorFxGo.transform.InverseTransformPoint(localPlayerPos).z > 0.5f)
			{
				playerFxInstance = Utils.SpawnPrefabAt(interiorPlayerFx, SNCameraRoot.main.transform, SNCameraRoot.main.transform.position);
				playerFxInstance.transform.localPosition = new Vector3(0f, 0f, 4f);
				fxControl.Stop(0);
				interiorFxGo = null;
				Player.main.liveMixin.Kill();
			}
		}
	}

	private void ToggleSinking(bool isSinking)
	{
		subRoot.worldForces.underwaterGravity = (isSinking ? 3 : 0);
	}

	private void SwapToDamagedModels()
	{
		for (int i = 0; i < intact.Length; i++)
		{
			intact[i].SetActive(value: false);
		}
		for (int j = 0; j < destroyed.Length; j++)
		{
			destroyed[j].SetActive(value: true);
		}
		ToggleSinking(isSinking: true);
		subRoot.subWarning = false;
		subRoot.BroadcastMessage("NewAlarmState", null, SendMessageOptions.DontRequireReceiver);
		subRoot.subDestroyed = true;
		pingInstance.enabled = false;
	}

	private void RestoreModels()
	{
		for (int i = 0; i < intact.Length; i++)
		{
			intact[i].SetActive(value: true);
		}
		for (int j = 0; j < destroyed.Length; j++)
		{
			destroyed[j].SetActive(value: false);
		}
	}

	private IEnumerator SpawnLootAsync()
	{
		float t = 0f;
		float duration = swapModelsDelay + 0.5f;
		while (t < duration)
		{
			t += Time.deltaTime;
			yield return null;
		}
		CoroutineTask<GameObject> request2 = CraftData.GetPrefabForTechTypeAsync(TechType.ScrapMetal);
		yield return request2;
		GameObject result = request2.GetResult();
		for (int i = 0; i < 15; i++)
		{
			Vector3 lootSpawnPoint = GetLootSpawnPoint();
			Vector3 vector = new Vector3(Random.Range(-15f, 15f), Random.Range(-15f, 15f), Random.Range(-15f, 15f));
			UWE.Utils.SetIsKinematic(Object.Instantiate(result, lootSpawnPoint + vector, Random.rotation).GetComponent<Rigidbody>(), state: false);
		}
		request2 = CraftData.GetPrefabForTechTypeAsync(TechType.ComputerChip);
		yield return request2;
		result = request2.GetResult();
		for (int j = 0; j < 5; j++)
		{
			Vector3 lootSpawnPoint2 = GetLootSpawnPoint();
			Vector3 vector2 = new Vector3(Random.Range(-15f, 15f), Random.Range(-15f, 15f), Random.Range(-15f, 15f));
			UWE.Utils.SetIsKinematic(Object.Instantiate(result, lootSpawnPoint2 + vector2, Random.rotation).GetComponent<Rigidbody>(), state: false);
		}
	}

	private Vector3 GetLootSpawnPoint()
	{
		int childCount = lootSpawnPoints.childCount;
		int index = Random.Range(0, childCount);
		return lootSpawnPoints.GetChild(index).position;
	}

	public void OnRespawn(Player p)
	{
		fxControl.StopAndDestroy(0, 0f);
		Object.Destroy(playerFxInstance);
	}

	private void OnConsoleCommand_destroycyclops(NotificationCenter.Notification n)
	{
		DestroyCyclops();
	}

	private void OnConsoleCommand_restorecyclops(NotificationCenter.Notification n)
	{
		RestoreCyclops();
	}
}
