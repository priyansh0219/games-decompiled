using System;
using System.Collections.Generic;
using ProtoBuf;
using UWE;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[ProtoContract]
[ProtoInclude(1000, typeof(BloomCreature))]
[ProtoInclude(1200, typeof(Boomerang))]
[ProtoInclude(1300, typeof(LavaLarva))]
[ProtoInclude(1400, typeof(OculusFish))]
[ProtoInclude(1500, typeof(Eyeye))]
[ProtoInclude(1600, typeof(Garryfish))]
[ProtoInclude(1700, typeof(GasoPod))]
[ProtoInclude(1800, typeof(Grabcrab))]
[ProtoInclude(1900, typeof(Grower))]
[ProtoInclude(2000, typeof(Holefish))]
[ProtoInclude(2100, typeof(Hoverfish))]
[ProtoInclude(2200, typeof(Jellyray))]
[ProtoInclude(2300, typeof(Jumper))]
[ProtoInclude(2400, typeof(Peeper))]
[ProtoInclude(2500, typeof(RabbitRay))]
[ProtoInclude(2600, typeof(Reefback))]
[ProtoInclude(2700, typeof(Reginald))]
[ProtoInclude(2800, typeof(SandShark))]
[ProtoInclude(2900, typeof(Spadefish))]
[ProtoInclude(3000, typeof(Stalker))]
[ProtoInclude(3100, typeof(Bladderfish))]
[ProtoInclude(3200, typeof(Hoopfish))]
[ProtoInclude(3300, typeof(Mesmer))]
[ProtoInclude(3400, typeof(Bleeder))]
[ProtoInclude(3500, typeof(Slime))]
[ProtoInclude(3600, typeof(Crash))]
[ProtoInclude(3700, typeof(BoneShark))]
[ProtoInclude(3800, typeof(CuteFish))]
[ProtoInclude(3900, typeof(Leviathan))]
[ProtoInclude(4000, typeof(ReaperLeviathan))]
[ProtoInclude(4100, typeof(CaveCrawler))]
[ProtoInclude(4200, typeof(BirdBehaviour))]
[ProtoInclude(4400, typeof(Biter))]
[ProtoInclude(4500, typeof(Shocker))]
[ProtoInclude(4600, typeof(CrabSnake))]
[ProtoInclude(4700, typeof(SpineEel))]
[ProtoInclude(4800, typeof(SeaTreader))]
[ProtoInclude(4900, typeof(CrabSquid))]
[ProtoInclude(4910, typeof(Warper))]
[ProtoInclude(4920, typeof(LavaLizard))]
[ProtoInclude(5000, typeof(SeaDragon))]
[ProtoInclude(5100, typeof(GhostRay))]
[ProtoInclude(5200, typeof(SeaEmperorBaby))]
[ProtoInclude(5300, typeof(GhostLeviathan))]
[ProtoInclude(5400, typeof(SeaEmperorJuvenile))]
[ProtoInclude(5500, typeof(GhostLeviatanVoid))]
[RequireComponent(typeof(CreatureUtils))]
[DisallowMultipleComponent]
public class Creature : Living, IProtoEventListener, IScheduledUpdateBehaviour, IManagedBehaviour, ICompileTimeCheckable, IMovementPlatform
{
	[SerializeField]
	private Animator traitsAnimator;

	[SerializeField]
	private CreatureFriend creatureFriend;

	[AssertNotNull]
	public LiveMixin liveMixin;

	[AssertNotNull]
	public AnimationCurve initialCuriosity;

	[AssertNotNull]
	public AnimationCurve initialFriendliness;

	[AssertNotNull]
	public AnimationCurve initialHunger;

	[AssertNotNull]
	public CreatureTrait Curiosity;

	[AssertNotNull]
	public CreatureTrait Friendliness;

	[AssertNotNull]
	public CreatureTrait Hunger;

	[AssertNotNull]
	public CreatureTrait Aggression;

	[AssertNotNull]
	public CreatureTrait Scared;

	[AssertNotNull]
	public CreatureTrait Tired;

	[AssertNotNull]
	public CreatureTrait Happy;

	[AssertNotNull]
	public AnimationCurve activity;

	public bool hasEyes = true;

	public float eyeFOV = 0.25f;

	public bool eyesOnTop;

	public float hearingSensitivity = 1f;

	public bool detectsMotion = true;

	private const int currentVersion = 3;

	[NonSerialized]
	[ProtoMember(1)]
	public Vector3 leashPosition = Vector3.zero;

	[NonSerialized]
	[ProtoMember(2)]
	public int version = 3;

	[NonSerialized]
	[ProtoMember(3)]
	public bool isInitialized;

	private readonly List<CreatureAction> actions = new List<CreatureAction>();

	private CreatureAction prevBestAction;

	private CreatureAction lastAction;

	public float babyScaleSize = 0.5f;

	[AssertNotNull]
	public AnimationCurve sizeDistribution;

	private float Size = -1f;

	public float seaLevelOffset;

	public bool cyclopsSonarDetectable;

	public bool debug;

	public string debugActionsString;

	private long techTypeHash;

	private static readonly int animAggressive = Animator.StringToHash("aggressive");

	private static readonly int animScared = Animator.StringToHash("scared");

	private static readonly int animTired = Animator.StringToHash("tired");

	private static readonly int animHappy = Animator.StringToHash("happy");

	protected const string kSafeShallows = "safeShallows";

	public static Bounds prisonAquriumBounds = new Bounds(new Vector3(325f, -1554.33f, -455f), new Vector3(550f, 200f, 550f));

	private const string kPrisonAquariumPrefix = "Prison_Aquarium";

	private int indexLastActionChecked;

	private float lastUpdateTime = -1f;

	private bool _friendlyToPlayer;

	public int scheduledUpdateIndex { get; set; }

	public virtual bool friendlyToPlayer
	{
		set
		{
			_friendlyToPlayer = value;
		}
	}

	public string GetProfileTag()
	{
		return "Creature";
	}

	public virtual void Start()
	{
		if (initialCuriosity != null && initialCuriosity.length > 0)
		{
			Curiosity.Value = initialCuriosity.Evaluate(UnityEngine.Random.value);
		}
		if (initialFriendliness != null && initialFriendliness.length > 0)
		{
			Friendliness.Value = initialFriendliness.Evaluate(UnityEngine.Random.value);
		}
		if (initialHunger != null && initialHunger.length > 0)
		{
			Hunger.Value = initialHunger.Evaluate(UnityEngine.Random.value);
		}
		bool flag = !isInitialized && Size < 0f;
		float magnitude = (base.transform.localScale - Vector3.one).magnitude;
		if (flag && !Utils.NearlyEqual(magnitude, 0f))
		{
			base.transform.localScale = Vector3.one;
		}
		GrowMixin component = base.gameObject.GetComponent<GrowMixin>();
		if ((bool)component)
		{
			component.growScalarChanged.AddHandler(base.gameObject, OnGrowChanged);
		}
		else if (flag && sizeDistribution != null)
		{
			float size = Mathf.Clamp01(sizeDistribution.Evaluate(UnityEngine.Random.value));
			SetSize(size);
		}
		TechType techType = CraftData.GetTechType(base.gameObject);
		if (techType != 0)
		{
			techTypeHash = UWE.Utils.SDBMHash(techType.AsString());
		}
		else
		{
			Debug.LogErrorFormat("Creature: Couldn't find tech type for creature name: {0}", base.gameObject.name);
		}
		ScanCreatureActions();
		if (isInitialized)
		{
			InitializeAgain();
		}
		else
		{
			InitializeOnce();
			isInitialized = true;
		}
		DeferredSchedulerUtils.Schedule(this);
	}

	protected virtual void InitializeOnce()
	{
		ProcessInfection();
		leashPosition = base.transform.position;
	}

	protected virtual void InitializeAgain()
	{
		InfectedMixin component = base.gameObject.GetComponent<InfectedMixin>();
		if ((bool)component && component.IsInfected() && UnityEngine.Random.value < 0.2f)
		{
			component.SetInfectedAmount(1f);
		}
	}

	private void ProcessInfection()
	{
		if (!LargeWorld.main)
		{
			return;
		}
		string biome = LargeWorld.main.GetBiome(base.transform.position);
		bool num = string.Equals("safeShallows", biome, StringComparison.OrdinalIgnoreCase);
		bool flag = prisonAquriumBounds.Contains(base.transform.position);
		if (!num && !flag)
		{
			InfectedMixin component = base.gameObject.GetComponent<InfectedMixin>();
			if ((bool)component && UnityEngine.Random.value < 0.05f)
			{
				component.SetInfectedAmount(1f);
			}
		}
	}

	private CreatureAction ChooseBestAction(float time)
	{
		if (actions.Count == 0)
		{
			return null;
		}
		float num = 0f;
		CreatureAction creatureAction = null;
		if (prevBestAction != null)
		{
			creatureAction = prevBestAction;
			num = creatureAction.Evaluate(this, time);
			creatureAction.timeLastChecked = time;
		}
		int num2 = indexLastActionChecked + 1;
		if (num2 >= actions.Count)
		{
			num2 = 0;
		}
		indexLastActionChecked = num2;
		_ = prevBestAction;
		for (int i = 0; i < actions.Count; i++)
		{
			CreatureAction creatureAction2 = actions[i];
			if (creatureAction2 == prevBestAction || (i != num2 && !creatureAction2.NeedsToBeChecked(time)))
			{
				continue;
			}
			creatureAction2.timeLastChecked = time;
			if (!(creatureAction2.GetMaxEvaluatePriority() <= num))
			{
				float num3 = creatureAction2.Evaluate(this, time);
				if (num3 > num)
				{
					num = num3;
					creatureAction = creatureAction2;
				}
				if (debug)
				{
					Debug.LogFormat("{0}.{1}.Evaluate() returned: {2}", base.gameObject.name, creatureAction2.GetType(), num3);
				}
			}
		}
		if (debug && creatureAction != null && creatureAction != prevBestAction)
		{
			Debug.LogFormat("     Found {0} evaluation with higher priority {1} then {2}", creatureAction.GetType(), num, (prevBestAction != null) ? prevBestAction.GetType().ToString() : "<none>");
		}
		_ = creatureAction != null;
		return creatureAction;
	}

	public void ScanCreatureActions()
	{
		actions.Clear();
		CreatureAction[] components = base.gameObject.GetComponents<CreatureAction>();
		foreach (CreatureAction creatureAction in components)
		{
			if (creatureAction.enabled)
			{
				actions.Add(creatureAction);
			}
		}
		if (prevBestAction != null && !actions.Contains(prevBestAction))
		{
			prevBestAction.StopPerform(this, Time.time);
			prevBestAction = null;
		}
		indexLastActionChecked = actions.Count - 1;
	}

	public virtual bool TryStartAction(CreatureAction action)
	{
		if (action == null || prevBestAction == action)
		{
			return false;
		}
		if ((float)actions.IndexOf(action) < 0f)
		{
			return false;
		}
		float time = Time.time;
		if (prevBestAction != null)
		{
			float num = prevBestAction.Evaluate(this, time);
			float num2 = action.Evaluate(this, time);
			prevBestAction.timeLastChecked = time;
			action.timeLastChecked = time;
			if (num > num2)
			{
				return false;
			}
			prevBestAction.StopPerform(this, time);
			if (debug)
			{
				Debug.LogFormat("     Force start {0} with higher priority {1} then {2}", action.GetType(), num2, prevBestAction.GetType().ToString());
			}
		}
		action.StartPerform(this, time);
		action.Perform(this, time, time - lastUpdateTime);
		prevBestAction = action;
		lastUpdateTime = time;
		lastAction = action;
		return true;
	}

	public CreatureAction GetBestAction()
	{
		return prevBestAction;
	}

	private void UpdateBehaviour(float time, float deltaTime)
	{
		_ = StopwatchProfiler.Instance;
		CreatureAction creatureAction = ChooseBestAction(time);
		if (prevBestAction != creatureAction)
		{
			if ((bool)prevBestAction)
			{
				prevBestAction.StopPerform(this, time);
			}
			if ((bool)creatureAction)
			{
				creatureAction.StartPerform(this, time);
			}
			prevBestAction = creatureAction;
		}
		if ((bool)creatureAction)
		{
			creatureAction.Perform(this, time, deltaTime);
			lastAction = creatureAction;
		}
		float num = DayNightUtils.Evaluate(1f, activity);
		Tired.Value = Mathf.Lerp(Tired.Value, 1f - num, 0.1f * deltaTime);
		Curiosity.UpdateTrait(deltaTime);
		Friendliness.UpdateTrait(deltaTime);
		Hunger.UpdateTrait(deltaTime);
		Aggression.UpdateTrait(deltaTime);
		Scared.UpdateTrait(deltaTime);
		Tired.UpdateTrait(deltaTime);
		Happy.UpdateTrait(deltaTime);
		if ((bool)traitsAnimator && traitsAnimator.isActiveAndEnabled)
		{
			traitsAnimator.SetFloat(animAggressive, Aggression.Value);
			traitsAnimator.SetFloat(animScared, Scared.Value);
			traitsAnimator.SetFloat(animTired, Tired.Value);
			traitsAnimator.SetFloat(animHappy, Happy.Value);
		}
	}

	public void ScheduledUpdate()
	{
		float deltaTime = Time.time - lastUpdateTime;
		UpdateBehaviour(Time.time, deltaTime);
		lastUpdateTime = Time.time;
	}

	private void OnGrowChanged(float growScalar)
	{
		SetSize(growScalar);
	}

	private void SetSize(float size)
	{
		float num = Mathf.Lerp(babyScaleSize, 1f, size);
		base.transform.localScale = new Vector3(num, num, num);
		Size = size;
	}

	public Animator GetAnimator()
	{
		return traitsAnimator;
	}

	public GameObject GetFriend()
	{
		if (!(creatureFriend != null))
		{
			return null;
		}
		return creatureFriend.GetFriend();
	}

	public void SetFriend(GameObject friend, float duration = float.PositiveInfinity)
	{
		if (creatureFriend == null)
		{
			creatureFriend = base.gameObject.EnsureComponent<CreatureFriend>();
		}
		if (friend != null)
		{
			creatureFriend.SetFriend(friend, duration);
		}
	}

	private bool IsPlayerOrPlayerProperty(GameObject gameObject)
	{
		if (!(gameObject == Player.main.gameObject))
		{
			return gameObject.GetComponent<Vehicle>() != null;
		}
		return true;
	}

	public virtual bool IsFriendlyTo(GameObject gameObject)
	{
		if (gameObject == null)
		{
			return false;
		}
		if (!(gameObject == GetFriend()))
		{
			if (_friendlyToPlayer)
			{
				return IsPlayerOrPlayerProperty(gameObject);
			}
			return false;
		}
		return true;
	}

	public float GetSize()
	{
		return Size;
	}

	public void SetScale(float scale)
	{
		base.transform.localScale = scale * Vector3.one;
		Size = Mathf.InverseLerp(babyScaleSize, 1f, scale);
	}

	public float GetSpeedScalar()
	{
		float num = 1f;
		SpeedGene component = base.gameObject.GetComponent<SpeedGene>();
		if ((bool)component)
		{
			num += component.GetSpeedScalar();
			Debug.LogFormat("{0}.GetSpeedScalar() gene affecting scalar is {1}", base.gameObject.name, num);
		}
		return num;
	}

	public virtual string GetActiveBehaviourName()
	{
		return GetType().ToString();
	}

	public virtual string GetDebugString()
	{
		if (lastAction == null)
		{
			return string.Empty;
		}
		return lastAction.GetDebugString();
	}

	public virtual void OnKill()
	{
		base.enabled = false;
		SwimBehaviour component = GetComponent<SwimBehaviour>();
		if ((bool)component)
		{
			component.Idle();
			component.enabled = false;
		}
	}

	public virtual void OnDrop()
	{
		leashPosition = base.transform.position;
	}

	public long GetTechTypeHash()
	{
		return techTypeHash;
	}

	public bool GetCanSeeObject(GameObject obj)
	{
		if (!hasEyes || !IsInFieldOfView(obj))
		{
			return false;
		}
		return true;
	}

	public bool IsInFieldOfView(GameObject go)
	{
		bool result = false;
		if (go != null)
		{
			Vector3 vector = go.transform.position - base.transform.position;
			Vector3 rhs = (eyesOnTop ? base.transform.up : base.transform.forward);
			Vector3 normalized = vector.normalized;
			if ((Mathf.Approximately(eyeFOV, -1f) || Vector3.Dot(normalized, rhs) >= eyeFOV) && !Physics.Linecast(base.transform.position, go.transform.position, Voxeland.GetTerrainLayerMask()))
			{
				result = true;
			}
		}
		return result;
	}

	public virtual void OnProtoSerialize(ProtobufSerializer serializer)
	{
	}

	public virtual void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		if (version < 3)
		{
			isInitialized = leashPosition != Vector3.zero && Vector3.Distance(leashPosition, base.transform.position) < 150f;
			version = 3;
		}
	}

	public virtual void OnEnable()
	{
		UpdateSchedulerUtils.Register(this);
	}

	public virtual void OnDisable()
	{
		UpdateSchedulerUtils.Deregister(this);
	}

	public virtual void OnDestroy()
	{
		UpdateSchedulerUtils.Deregister(this);
	}

	protected void AllowCreatureUpdates(bool allowed)
	{
		if (base.enabled)
		{
			if (allowed)
			{
				UpdateSchedulerUtils.Register(this);
			}
			else
			{
				UpdateSchedulerUtils.Deregister(this);
			}
		}
	}

	public string CompileTimeCheck()
	{
		bool flag = GetComponent<Pickupable>() != null;
		if (flag || GetComponent<WaterParkCreature>() != null)
		{
			TechType techType = CraftData.GetTechType(base.gameObject);
			if (!BaseBioReactor.CanAdd(techType) && GetComponent<CuteFish>() == null)
			{
				if (flag)
				{
					return $"Pickupable creature {techType} is missing its biorector charge value. Please add it to charge dictionary of BaseBioReactor class";
				}
				return $"Aquarium creature {techType} is missing its biorector charge value. Please add it to charge dictionary of BaseBioReactor class";
			}
		}
		return null;
	}

	bool IMovementPlatform.IsPlatform()
	{
		return false;
	}
}
