using System;
using System.Collections;
using ProtoBuf;
using UnityEngine;
using UnityEngine.AddressableAssets;

[ProtoContract]
[SkipProtoContractCheck]
public class GrowingPlant : HandTarget, IHandTarget, ILocalizationCheckable, ICompileTimeCheckable, IManagedUpdateBehaviour, IManagedBehaviour
{
	[SerializeField]
	private float growthDuration = 1200f;

	[SerializeField]
	[AssertNotNull]
	private AnimationCurve growthWidth;

	[SerializeField]
	[AssertNotNull]
	private AnimationCurve growthHeight;

	[SerializeField]
	[AssertNotNull]
	private AnimationCurve growthWidthIndoor;

	[SerializeField]
	[AssertNotNull]
	private AnimationCurve growthHeightIndoor;

	[SerializeField]
	private Vector3 positionOffset = Vector3.zero;

	[SerializeField]
	public float heightProgressFactor;

	[SerializeField]
	private Transform growingTransform;

	[SerializeField]
	private AssetReferenceGameObject grownModelPrefab;

	[SerializeField]
	private TechType plantTechType;

	[NonSerialized]
	public Plantable seed;

	private float timeStartGrowth = -1f;

	private float maxProgress = 1f;

	private bool isIndoor;

	private VFXPassYboundsToMat passYbounds;

	private VFXScaleWaving wavingScaler;

	[AssertLocalization(1)]
	private const string growingPlantFormat = "GrowingPlant";

	public int managedUpdateIndex { get; set; }

	public string GetProfileTag()
	{
		return "GrowingPlant";
	}

	private void Start()
	{
		BehaviourUpdateUtils.RegisterForUpdate(this);
	}

	private void OnDestroy()
	{
		BehaviourUpdateUtils.DeregisterFromUpdate(this);
	}

	private void OnEnable()
	{
		ShowGrowingTransform();
	}

	private void OnDisable()
	{
		growingTransform.gameObject.SetActive(value: false);
	}

	public void ManagedUpdate()
	{
		float progress = GetProgress();
		SetScale(growingTransform, progress);
		SetPosition(growingTransform);
		if (progress == 1f)
		{
			SpawnGrownModel();
		}
	}

	private void SpawnGrownModel()
	{
		StartCoroutine(SpawnGrownModelAsync());
	}

	private IEnumerator SpawnGrownModelAsync()
	{
		BehaviourUpdateUtils.DeregisterFromUpdate(this);
		CoroutineTask<GameObject> task = AddressablesUtility.InstantiateAsync(grownModelPrefab.RuntimeKey as string, null, growingTransform.position, growingTransform.rotation, awake: false);
		yield return task;
		GameObject result = task.GetResult();
		growingTransform.gameObject.SetActive(value: false);
		SetScale(result.transform, 1f);
		if (result.GetComponent<Pickupable>() != null)
		{
			Plantable component = result.GetComponent<Plantable>();
			if (component != null && seed.ReplaceSeedByPlant(component))
			{
				yield break;
			}
		}
		result.SetActive(value: true);
		GrownPlant grownPlant = result.AddComponent<GrownPlant>();
		grownPlant.seed = seed;
		grownPlant.SendMessage("OnGrown", SendMessageOptions.DontRequireReceiver);
		if (seed != null)
		{
			result.transform.parent = seed.currentPlanter.grownPlantsRoot;
			seed.currentPlanter.SetupRenderers(result, interior: true);
			seed.currentPlanter.SetupLighting(result);
			seed.currentPlanter.TryToExcludeFromSubParentRigidbody();
		}
		base.enabled = false;
	}

	private void ShowGrowingTransform()
	{
		if (!growingTransform.gameObject.activeSelf)
		{
			passYbounds = growingTransform.GetComponent<VFXPassYboundsToMat>();
			if (passYbounds == null)
			{
				wavingScaler = growingTransform.gameObject.EnsureComponent<VFXScaleWaving>();
			}
			growingTransform.gameObject.SetActive(value: true);
		}
	}

	public void SetScale(Transform tr, float progress)
	{
		float num = (isIndoor ? growthWidthIndoor.Evaluate(progress) : growthWidth.Evaluate(progress));
		float y = (isIndoor ? growthHeightIndoor.Evaluate(progress) : growthHeight.Evaluate(progress));
		tr.localScale = new Vector3(num, y, num);
		if (passYbounds != null)
		{
			passYbounds.UpdateWavingScale(tr.localScale);
		}
		else if (wavingScaler != null)
		{
			wavingScaler.UpdateWavingScale(tr.localScale);
		}
	}

	public void SetPosition(Transform tr)
	{
		Vector3 localScale = tr.localScale;
		Vector3 position = new Vector3(localScale.x * positionOffset.x, localScale.y * positionOffset.y, localScale.z * positionOffset.z);
		tr.position = base.transform.TransformPoint(position);
	}

	public void EnableIndoorState()
	{
		isIndoor = true;
	}

	private float GetGrowthDuration()
	{
		float num = (NoCostConsoleCommand.main.fastGrowCheat ? 0.01f : 1f);
		return growthDuration * num;
	}

	public float GetProgress()
	{
		if (timeStartGrowth == -1f)
		{
			SetProgress(0f);
			return 0f;
		}
		return Mathf.Clamp((float)(DayNightCycle.main.timePassed - (double)timeStartGrowth) / GetGrowthDuration(), 0f, maxProgress);
	}

	public void SetProgress(float progress)
	{
		progress = Mathf.Clamp(progress, 0f, maxProgress);
		SetScale(growingTransform, progress);
		timeStartGrowth = DayNightCycle.main.timePassedAsFloat - GetGrowthDuration() * progress;
	}

	public void SetMaxHeight(float height)
	{
		if (!(heightProgressFactor <= 0f))
		{
			float progress = GetProgress();
			maxProgress = Mathf.Clamp01(height * heightProgressFactor);
			if (progress > maxProgress)
			{
				SetProgress(maxProgress);
			}
			else if (progress < GetProgress())
			{
				SetProgress(progress);
			}
		}
	}

	public void OnHandHover(GUIHand hand)
	{
		if (base.enabled)
		{
			string format = Language.main.GetFormat("GrowingPlant", Language.main.Get(plantTechType.AsString()));
			HandReticle.main.SetText(HandReticle.TextType.Hand, format, translate: false);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
			HandReticle.main.SetProgress(GetProgress());
			HandReticle.main.SetIcon(HandReticle.IconType.Progress, 1.5f);
		}
	}

	public void OnHandClick(GUIHand hand)
	{
	}

	public string CompileTimeCheck(ILanguage language)
	{
		if (!(seed != null))
		{
			return null;
		}
		return language.CheckTechType(plantTechType);
	}

	public string CompileTimeCheck()
	{
		if (grownModelPrefab == null || !grownModelPrefab.RuntimeKeyIsValid())
		{
			return $"Script 'GrowingPlant' field 'grownModelPrefab' is broken";
		}
		return null;
	}
}
