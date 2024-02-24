using System.Runtime.CompilerServices;
using FMOD.Studio;
using UnityEngine;

public class AirBladder : PlayerTool, IOxygenSource, ISecondaryTooltip, IEquippable, ICompileTimeCheckable
{
	[Header("Settings")]
	[Tooltip("The amount of oxygen the bladder can hold when full (ie how many seconds of oxygen the player will get if they consume the oxygen).")]
	[SerializeField]
	private float oxygenCapacity = 5f;

	[Tooltip("The strength of the upward force applied when using the air bladder to ascend.")]
	[SerializeField]
	private float buoyancyForce = 0.8f;

	[Header("Feedback")]
	[SerializeField]
	[AssertNotNull]
	private FMOD_CustomEmitter inflate;

	[SerializeField]
	[AssertNotNull]
	private FMOD_CustomEmitter deflate;

	[SerializeField]
	[AssertNotNull]
	private FMOD_CustomEmitter deflateAboveWater;

	[SerializeField]
	[AssertNotNull]
	private GameObject firstPersonBubbleParticlesPrefab;

	[SerializeField]
	[AssertNotNull]
	private Animator animator;

	[SerializeField]
	[AssertNotNull]
	private GameObject bubblesExitPoint;

	private float oxygen;

	private bool firstUse;

	private bool deflating;

	private bool reclaimOxygen;

	private bool applyBuoyancy;

	private float inflateStartTime;

	private const float kTransferPerSecond = 2f;

	private PARAMETER_ID inflateDepthParam;

	private const string depthParamName = "depth";

	private static readonly int kAnimFirstState = Animator.StringToHash("first_use_check");

	private static readonly int kAnimAltUseTrigger = Animator.StringToHash("use_alt");

	private static readonly int kAnimInflate = Animator.StringToHash("inflate");

	[AssertLocalization]
	private const string useToolButtonFormat = "AirBladderUseTool";

	[AssertLocalization]
	private const string consumeOxygenButtonFormat = "AirBladderConsumeOxygen";

	private bool isUnderwater
	{
		get
		{
			if (usingPlayer.IsUnderwaterForSwimming())
			{
				return base.transform.position.y < Ocean.GetOceanLevel() - 1f;
			}
			return false;
		}
	}

	public override void Awake()
	{
		base.Awake();
		oxygen = oxygenCapacity;
	}

	private void Start()
	{
		Inventory inventory = Inventory.Get();
		if (inventory.Contains(pickupable))
		{
			RegisterOxygen();
		}
		inventory.container.onAddItem += OnAdded;
		inventory.container.onRemoveItem += OnRemoved;
		inflateDepthParam = inflate.GetParameterIndex("depth");
	}

	protected override void OnDestroy()
	{
		UnregisterOxygen();
		Inventory inventory = Inventory.Get();
		if ((bool)inventory)
		{
			inventory.container.onAddItem -= OnAdded;
			inventory.container.onRemoveItem -= OnRemoved;
		}
	}

	public override bool GetUsedToolThisFrame()
	{
		return deflating;
	}

	public override bool GetAltUsedToolThisFrame()
	{
		return reclaimOxygen;
	}

	public override string GetCustomUseText()
	{
		string result = base.GetCustomUseText();
		if (!usingPlayer.IsBleederAttached() && !deflating && oxygen > 0f && isUnderwater)
		{
			result = LanguageCache.GetButtonFormat("AirBladderUseTool", GameInput.Button.RightHand);
			if (!Mathf.Approximately(usingPlayer.GetOxygenAvailable(), usingPlayer.GetOxygenCapacity()))
			{
				string buttonFormat = LanguageCache.GetButtonFormat("AirBladderConsumeOxygen", GameInput.Button.AltTool);
				HandReticle.main.SetTextRaw(HandReticle.TextType.UseSubscript, buttonFormat);
			}
		}
		return result;
	}

	public override bool OnRightHandDown()
	{
		if (usingPlayer.IsBleederAttached())
		{
			return true;
		}
		if (!deflating && oxygen > 0f && isUnderwater)
		{
			deflate.Play();
			deflating = true;
			inflateStartTime = Time.time + 0.25f;
			return true;
		}
		return false;
	}

	public override bool OnAltDown()
	{
		if (usingPlayer.IsBleederAttached())
		{
			return true;
		}
		if (!deflating && oxygen > 0f && isUnderwater && !Mathf.Approximately(usingPlayer.GetOxygenAvailable(), usingPlayer.GetOxygenCapacity()))
		{
			reclaimOxygen = true;
			usingPlayer.GetComponent<OxygenManager>().AddOxygen(oxygen);
			oxygen = 0f;
			deflateAboveWater.Play();
			animator.SetTrigger(kAnimAltUseTrigger);
			inflateStartTime = Time.time + 0.1f;
			return true;
		}
		return false;
	}

	private void Update()
	{
		if (usingPlayer != null)
		{
			UpdateInflateState();
			float depth = usingPlayer.GetDepth();
			if (FMODUWE.IsValidParameterId(inflateDepthParam))
			{
				inflate.SetParameterValue(inflateDepthParam, depth);
			}
		}
		UpdateControllerLightbarToToolBarValue();
	}

	private void OnEnable()
	{
		animator.Play(kAnimFirstState);
		animator.ResetTrigger(kAnimAltUseTrigger);
		animator.SetFloat(kAnimInflate, oxygen / oxygenCapacity);
		inflateStartTime = 0f;
	}

	private void RegisterOxygen()
	{
		OxygenManager oxygenMgr = Player.main.oxygenMgr;
		if (oxygenMgr != null)
		{
			oxygenMgr.RegisterSource(this);
		}
	}

	private void UnregisterOxygen()
	{
		OxygenManager oxygenMgr = Player.main.oxygenMgr;
		if (oxygenMgr != null)
		{
			oxygenMgr.UnregisterSource(this);
		}
	}

	private void OnAdded(InventoryItem item)
	{
		if (item.item == pickupable)
		{
			RegisterOxygen();
		}
	}

	private void OnRemoved(InventoryItem item)
	{
		if (item.item == pickupable)
		{
			UnregisterOxygen();
		}
	}

	public override void OnDraw(Player p)
	{
		TechType techType = pickupable.GetTechType();
		firstUse = !p.IsToolUsed(techType) || PlayerToolConsoleCommands.debugFirstUse;
		base.OnDraw(p);
	}

	public override void OnHolster()
	{
		base.OnHolster();
		deflating = false;
		reclaimOxygen = false;
		applyBuoyancy = false;
		deflate.Stop();
	}

	private void UpdateInflateState()
	{
		if (Time.time - inflateStartTime < 0f)
		{
			return;
		}
		if (reclaimOxygen)
		{
			animator.SetFloat(kAnimInflate, 0f);
			deflate.Stop();
			reclaimOxygen = false;
		}
		else if (deflating)
		{
			float amount = Time.deltaTime * 2f;
			if (RemoveOxygen(amount) > 0f && isUnderwater)
			{
				Utils.PlayOneShotPS(firstPersonBubbleParticlesPrefab, bubblesExitPoint.transform.position, Quaternion.identity, bubblesExitPoint.transform);
				animator.SetFloat(kAnimInflate, oxygen / oxygenCapacity);
				applyBuoyancy = true;
			}
			else
			{
				deflate.Stop();
				deflating = false;
				applyBuoyancy = false;
			}
		}
		else if (oxygen < oxygenCapacity && !isUnderwater)
		{
			inflate.Stop();
			inflate.Play();
			deflating = false;
			applyBuoyancy = false;
			oxygen = oxygenCapacity;
			animator.SetFloat(kAnimInflate, 1f);
		}
	}

	private int GetSecondsLeft()
	{
		if (oxygen > 0.5f)
		{
			return Mathf.RoundToInt(oxygen);
		}
		return 0;
	}

	public void ApplyBuoyancyForce()
	{
		if (Mathf.Approximately(oxygen, 0f) || !applyBuoyancy || !isUnderwater)
		{
			return;
		}
		GameObject gameObject = (Inventory.main.Contains(pickupable) ? usingPlayer.gameObject : base.gameObject);
		float num = Ocean.GetOceanLevel() - 1f - base.transform.position.y;
		if (num > 0f)
		{
			Rigidbody component = gameObject.GetComponent<Rigidbody>();
			if (component.velocity.y * Time.fixedDeltaTime < num)
			{
				Vector3 force = Vector3.up * buoyancyForce;
				component.AddForce(force, ForceMode.Acceleration);
			}
		}
	}

	private void FixedUpdate()
	{
		ApplyBuoyancyForce();
	}

	public void OnEquip(GameObject sender, string slot)
	{
		if (base.isDrawn && firstUse)
		{
			animator.SetBool("using_tool_first", value: true);
		}
	}

	public void OnUnequip(GameObject sender, string slot)
	{
		if (firstUse)
		{
			animator.SetBool("using_tool_first", value: false);
		}
	}

	public void UpdateEquipped(GameObject sender, string slot)
	{
	}

	public bool IsPlayer()
	{
		return false;
	}

	public bool IsBreathable()
	{
		return false;
	}

	public float GetOxygenCapacity()
	{
		return oxygenCapacity;
	}

	public float GetOxygenAvailable()
	{
		return oxygen;
	}

	public float AddOxygen(float amount)
	{
		if (!base.isDrawn && oxygen < oxygenCapacity)
		{
			float num = Mathf.Min(amount, oxygenCapacity - oxygen);
			oxygen += num;
			return num;
		}
		return 0f;
	}

	public float RemoveOxygen(float amount)
	{
		float num = Mathf.Min(amount, oxygen);
		oxygen = Mathf.Max(0f, oxygen - num);
		return num;
	}

	public string GetSecondaryTooltip()
	{
		return LanguageCache.GetOxygenText(GetSecondsLeft());
	}

	public string CompileTimeCheck()
	{
		if (pickupable == null)
		{
			return $"AirBladder pickupable field must not be null";
		}
		return null;
	}

	[SpecialName]
	GameObject IOxygenSource.get_gameObject()
	{
		return base.gameObject;
	}
}
