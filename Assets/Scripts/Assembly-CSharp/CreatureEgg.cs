using System;
using Gendarme;
using ProtoBuf;
using UnityEngine;
using UnityEngine.AddressableAssets;

[ProtoContract]
public class CreatureEgg : MonoBehaviour, IShouldSerialize, ILocalizationCheckable, ICompileTimeCheckable
{
	[SerializeField]
	[AssertNotNull]
	private LiveMixin liveMixin;

	[SerializeField]
	[AssertNotNull]
	private AssetReferenceGameObject creaturePrefab;

	[SerializeField]
	private TechType creatureType;

	[SerializeField]
	private TechType overrideEggType;

	[SerializeField]
	private float daysBeforeHatching = 1f;

	[SerializeField]
	[AssertNotNull]
	private Animator animator;

	private TechType eggType;

	private const float defaultProgress = 0f;

	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public float progress;

	private float timeStartHatching;

	private bool insideWaterPark;

	private bool subscribed;

	private bool isKnown;

	[AssertLocalization(1)]
	private const string eggDiscoveredFormat = "EggDiscovered";

	private const string encyclopediaEntryForUnknownEgg = "UnknownEgg";

	private void Awake()
	{
		eggType = CraftData.GetTechType(base.gameObject);
		Pickupable component = GetComponent<Pickupable>();
		if (overrideEggType != 0 && !KnownTech.Contains(eggType))
		{
			component.SetTechTypeOverride(overrideEggType);
			Subscribe(state: true);
			isKnown = false;
		}
		else
		{
			component.ResetTechTypeOverride();
			isKnown = true;
		}
	}

	private void Start()
	{
		animator.enabled = insideWaterPark;
		animator.SetFloat(AnimatorHashID.progress, insideWaterPark ? progress : 0f);
	}

	private void OnKnownTechChanged()
	{
		if (KnownTech.Contains(eggType))
		{
			GetComponent<Pickupable>().ResetTechTypeOverride();
			Subscribe(state: false);
			isKnown = true;
		}
	}

	private void Subscribe(bool state)
	{
		if (subscribed != state)
		{
			if (state)
			{
				KnownTech.onChanged += OnKnownTechChanged;
			}
			else
			{
				KnownTech.onChanged -= OnKnownTechChanged;
			}
			subscribed = state;
		}
	}

	private void OnAddToWaterPark()
	{
		insideWaterPark = true;
		base.transform.localScale = 0.6f * Vector3.one;
		animator.enabled = true;
		if (creaturePrefab != null && creaturePrefab.RuntimeKeyIsValid())
		{
			UpdateHatchingTime();
			InvokeRepeating("UpdateProgress", 0f, 1f);
		}
	}

	private void OnDisable()
	{
		insideWaterPark = false;
		CancelInvoke();
		animator.enabled = false;
		base.transform.localScale = Vector3.one;
	}

	private float GetHatchDuration()
	{
		float num = (NoCostConsoleCommand.main.fastHatchCheat ? 0.01f : 1f);
		return 1200f * daysBeforeHatching * num;
	}

	private void UpdateHatchingTime()
	{
		timeStartHatching = DayNightCycle.main.timePassedAsFloat - GetHatchDuration() * progress;
	}

	private void UpdateProgress()
	{
		float timePassedAsFloat = DayNightCycle.main.timePassedAsFloat;
		progress = Mathf.InverseLerp(timeStartHatching, timeStartHatching + GetHatchDuration(), timePassedAsFloat);
		animator.SetFloat(AnimatorHashID.progress, progress);
		if (progress >= 1f)
		{
			Hatch();
		}
	}

	private void Hatch()
	{
		CancelInvoke();
		WaterParkItem component = GetComponent<WaterParkItem>();
		if (component != null)
		{
			WaterPark waterPark = component.GetWaterPark();
			component.SetWaterPark(null);
			if (KnownTech.Add(eggType, verbose: false))
			{
				ErrorMessage.AddMessage(Language.main.GetFormat("EggDiscovered", Language.main.Get(eggType.AsString())));
			}
			WaterParkCreature.Born(creaturePrefab, waterPark, base.transform.position);
		}
		liveMixin.Kill();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public int GetCreatureSize()
	{
		return WaterParkCreature.GetCreatureSize(creatureType);
	}

	private void OnDestroy()
	{
		Subscribe(state: false);
	}

	[SuppressMessage("Gendarme.Rules.Correctness", "AvoidFloatingPointEqualityRule")]
	public bool ShouldSerialize()
	{
		if (version == 1)
		{
			return progress != 0f;
		}
		return true;
	}

	public string CompileTimeCheck(ILanguage language)
	{
		return language.CheckTechType(CraftData.GetTechType(base.gameObject)) ?? language.CheckTechType(overrideEggType);
	}

	public string CompileTimeCheck()
	{
		TechType techType = CraftData.GetTechType(base.gameObject);
		if (!BaseBioReactor.CanAdd(techType))
		{
			return $"Creature egg {techType} is missing its biorector charge value. Please add it to charge dictionary of BaseBioReactor class";
		}
		return null;
	}

	private void OnExamine()
	{
		if (!isKnown)
		{
			PDAEncyclopedia.Add("UnknownEgg", verbose: true);
		}
	}
}
