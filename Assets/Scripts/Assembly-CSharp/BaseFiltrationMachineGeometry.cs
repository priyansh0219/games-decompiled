using System;
using UnityEngine;

public class BaseFiltrationMachineGeometry : MonoBehaviour, IBaseModuleGeometry, IObstacle
{
	[Serializable]
	public class Item
	{
		[AssertNotNull]
		public TechType techType;

		[AssertNotNull]
		public GameObject gameObject;

		[AssertNotNull]
		public FMODAsset spawnSound;

		[AssertNotNull]
		public VFXScan scanEffect;

		public float scanDuration = 1.5f;
	}

	[AssertNotNull]
	public Animator animator;

	[AssertNotNull]
	public GameObject sparksPrefab;

	[AssertNotNull]
	public GameObject fabLight;

	[AssertNotNull]
	public Transform[] beams;

	[AssertNotNull]
	public VFXController vfxController;

	[AssertNotNull]
	public FMOD_CustomLoopingEmitter workSound;

	[AssertNotNull]
	public Transform spawnPoint;

	[AssertNotNull]
	public Item[] items;

	private Transform[] sparks;

	private ParticleSystem[] sparksParticles;

	private bool cachedWorking;

	private bool cachedScanning;

	private bool isDirty = true;

	private bool initialized;

	private VFXScan itemVFXScan;

	private float timeItemSpawned = -1f;

	private TechType shownTechType;

	private int shownCount;

	private FiltrationMachine module;

	[AssertLocalization]
	private const string deconstructNonEmptyMessage = "DeconstructNonEmptyFiltrationMachineError";

	private Base.Face _geometryFace;

	public Base.Face geometryFace
	{
		get
		{
			return _geometryFace;
		}
		set
		{
			_geometryFace = value;
			module = null;
			isDirty = true;
			initialized = false;
		}
	}

	private void Awake()
	{
		int num = beams.Length;
		sparks = new Transform[num];
		sparksParticles = new ParticleSystem[num];
		for (int i = 0; i < num; i++)
		{
			Transform transform = beams[i];
			GameObject gameObject = Utils.SpawnPrefabAt(sparksPrefab, transform, transform.position);
			sparks[i] = gameObject.transform;
			sparksParticles[i] = gameObject.GetComponent<ParticleSystem>();
		}
	}

	private void Start()
	{
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.UpdateAfterInput, OnUpdate);
	}

	private void OnDestroy()
	{
		ManagedUpdate.Unsubscribe(OnUpdate);
	}

	private void OnUpdate()
	{
		UpdateVisuals();
		if (cachedScanning)
		{
			float num = 0f;
			if (itemVFXScan != null)
			{
				num = itemVFXScan.GetCurrentYPos();
			}
			Vector3 position = base.transform.position;
			position.y = num;
			Shader.SetGlobalFloat(ShaderPropertyID._FabricatorPosY, num + 0.03f);
			for (int i = 0; i < beams.Length; i++)
			{
				Transform transform = beams[i];
				sparks[i].position = GetBeamEnd(transform.position, transform.forward, position, Vector3.up);
			}
		}
	}

	private void UpdateVisuals()
	{
		if (initialized && (!isDirty || !Player.main.IsInBase() || (Player.main.transform.position - base.transform.position).sqrMagnitude > 36f))
		{
			return;
		}
		isDirty = false;
		FiltrationMachine filtrationMachine = GetModule();
		bool producingWater = filtrationMachine.producingWater;
		if (cachedWorking != producingWater)
		{
			cachedWorking = producingWater;
			animator.SetBool(AnimatorHashID.fabricating, producingWater);
			if (producingWater)
			{
				workSound.Play();
				vfxController.Play(1);
			}
			else
			{
				workSound.Stop();
				vfxController.Stop(1);
			}
		}
		int num = -1;
		Item item = null;
		TechType techType = TechType.None;
		int num2 = 0;
		ItemsContainer container = filtrationMachine.storageContainer.container;
		for (int i = 0; i < items.Length; i++)
		{
			Item item2 = items[i];
			int count = container.GetCount(item2.techType);
			if (count > 0)
			{
				num = i;
				item = item2;
				techType = item2.techType;
				num2 = count;
				break;
			}
		}
		if (initialized && techType != 0 && (shownTechType != techType || num2 < shownCount))
		{
			timeItemSpawned = Time.time;
		}
		shownTechType = techType;
		shownCount = num2;
		for (int j = 0; j < items.Length; j++)
		{
			items[j].gameObject.SetActive(j == num);
		}
		bool flag = false;
		if (item != null)
		{
			itemVFXScan = item.gameObject.GetComponent<VFXScan>();
			float time = Time.time;
			float num3 = 0f;
			if (timeItemSpawned >= 0f)
			{
				num3 = (time - timeItemSpawned) / item.scanDuration;
				flag = num3 >= 0f && num3 <= 1f;
			}
		}
		if (flag || shownCount == 0)
		{
			vfxController.Stop(0);
		}
		if (cachedScanning != flag)
		{
			cachedScanning = flag;
			fabLight.SetActive(flag && MiscSettings.flashes);
			for (int k = 0; k < sparks.Length; k++)
			{
				beams[k].gameObject.SetActive(flag);
				sparksParticles[k].SetPlaying(flag && MiscSettings.flashes);
			}
			if (flag)
			{
				if (item != null && !DayNightCycle.main.IsInSkipTimeMode())
				{
					Utils.PlayFMODAsset(item.spawnSound, spawnPoint);
					item.scanEffect.StartScan(item.scanDuration);
				}
			}
			else if (item != null && !DayNightCycle.main.IsInSkipTimeMode() && item.techType == TechType.BigFilteredWater)
			{
				vfxController.Play(0);
			}
		}
		initialized = true;
	}

	public void OnHover(HandTargetEventData eventData)
	{
		FiltrationMachine filtrationMachine = GetModule();
		if (filtrationMachine != null)
		{
			filtrationMachine.OnHover(eventData);
		}
	}

	public void OnUse(HandTargetEventData eventData)
	{
		FiltrationMachine filtrationMachine = GetModule();
		if (filtrationMachine != null)
		{
			filtrationMachine.OnUse(this);
		}
	}

	public void SetDirty()
	{
		isDirty = true;
	}

	private FiltrationMachine GetModule()
	{
		if (module == null)
		{
			Base componentInParent = GetComponentInParent<Base>();
			if (componentInParent != null)
			{
				IBaseModule baseModule = componentInParent.GetModule(_geometryFace);
				if (baseModule != null)
				{
					module = baseModule as FiltrationMachine;
				}
			}
		}
		return module;
	}

	private static Vector3 GetBeamEnd(Vector3 beamPos, Vector3 beamRot, Vector3 basePos, Vector3 baseRot)
	{
		return beamPos + Vector3.Normalize(beamRot) * (Vector3.Dot(basePos - beamPos, baseRot) / Vector3.Dot(beamRot, baseRot));
	}

	public bool IsDeconstructionObstacle()
	{
		return true;
	}

	public bool CanDeconstruct(out string reason)
	{
		FiltrationMachine filtrationMachine = GetModule();
		if (filtrationMachine != null && filtrationMachine.storageContainer.container.count > 0)
		{
			reason = Language.main.Get("DeconstructNonEmptyFiltrationMachineError");
			return false;
		}
		reason = null;
		return true;
	}
}
