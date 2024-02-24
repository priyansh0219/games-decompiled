using UWE;
using UnityEngine;

public class PlayerTool : ModelPlug
{
	public enum ReloadMode
	{
		None = 0,
		Direct = 1,
		Animation = 2
	}

	public enum Socket
	{
		RightHand = 0,
		Camera = 1
	}

	public ReloadMode reloadMode = ReloadMode.Direct;

	public FMODAsset reloadSound;

	public Collider mainCollider;

	protected Player usingPlayer;

	public FMODAsset drawSound;

	public FMODAsset drawSoundUnderwater;

	public FMOD_CustomEmitter firstUseSound;

	public FMODAsset holsterSoundAboveWater;

	public FMODAsset holsterSoundUnderwater;

	public FMODAsset hitBleederSound;

	public float bleederDamage = 3f;

	public Socket socket;

	public bool ikAimRightArm;

	public bool ikAimLeftArm;

	public Transform rightHandIKTarget;

	public bool useLeftAimTargetOnPlayer;

	public Transform leftHandIKTarget;

	public bool hasAnimations = true;

	public float drawTime = 0.5f;

	public float holsterTime = 0.35f;

	public float dropTime = 1f;

	public float bashTime = 0.7f;

	private bool forceConfigureIK;

	protected EnergyMixin energyMixin;

	protected bool _isInUse;

	public Pickupable pickupable;

	public bool hasFirstUseAnimation;

	public bool hasBashAnimation;

	[SerializeField]
	private bool waitForAnimDrawEvent;

	[SerializeField]
	public Renderer[] renderers;

	protected bool firstUseAnimationStarted;

	private Transform savedRightHandIKTarget;

	private Transform savedLeftHandIKTarget;

	private bool savedIkAimRightArm;

	private bool savedIkAimLeftArm;

	public bool isInUse => _isInUse;

	protected bool isDrawn { get; private set; }

	public virtual string animToolName => GetComponent<Pickupable>().GetTechType().AsString(lowercase: true);

	public virtual void Awake()
	{
		energyMixin = GetComponent<EnergyMixin>();
		savedRightHandIKTarget = rightHandIKTarget;
		savedLeftHandIKTarget = leftHandIKTarget;
		savedIkAimRightArm = ikAimRightArm;
		savedIkAimLeftArm = ikAimLeftArm;
	}

	protected virtual void OnDestroy()
	{
		if (usingPlayer != null)
		{
			OnHolster();
		}
	}

	public virtual void OnDraw(Player p)
	{
		usingPlayer = p;
		SetHandIKTargetsEnabled(enabled: true);
		LargeWorldEntity component = GetComponent<LargeWorldEntity>();
		if (component != null && LargeWorldStreamer.main != null && LargeWorldStreamer.main.IsReady())
		{
			LargeWorldStreamer.main.cellManager.UnregisterEntity(component);
		}
		isDrawn = true;
		firstUseAnimationStarted = false;
		if (hasFirstUseAnimation && (bool)pickupable)
		{
			TechType techType = pickupable.GetTechType();
			bool flag = ShouldPlayFirstUseAnimation();
			if (flag)
			{
				Player.main.AddUsedTool(techType);
			}
			Player.main.playerAnimator.SetBool("using_tool_first", flag);
			firstUseAnimationStarted = flag;
		}
		if (firstUseAnimationStarted && (bool)firstUseSound)
		{
			firstUseSound.Play();
		}
		else if ((bool)drawSound)
		{
			FMODAsset fMODAsset = ((Player.main.IsUnderwater() && drawSoundUnderwater != null) ? drawSoundUnderwater : drawSound);
			if ((bool)fMODAsset)
			{
				Utils.PlayFMODAsset(fMODAsset, base.transform);
			}
		}
	}

	public virtual void OnHolster()
	{
		usingPlayer = null;
		Player.main.playerAnimator.SetBool("using_tool_first", value: false);
		isDrawn = false;
		if (firstUseAnimationStarted)
		{
			OnFirstUseAnimationStop();
		}
	}

	public virtual string GetCustomUseText()
	{
		return "";
	}

	private void SetRenderersEnabled(bool state)
	{
		if (renderers == null)
		{
			return;
		}
		Renderer[] array = renderers;
		foreach (Renderer renderer in array)
		{
			if (renderer != null)
			{
				renderer.enabled = state;
			}
		}
	}

	protected void SetHandIKTargetsEnabled(bool enabled)
	{
		if (enabled)
		{
			rightHandIKTarget = savedRightHandIKTarget;
			leftHandIKTarget = savedLeftHandIKTarget;
			ikAimRightArm = savedIkAimRightArm;
			ikAimLeftArm = savedIkAimLeftArm;
		}
		else
		{
			rightHandIKTarget = null;
			leftHandIKTarget = null;
			ikAimRightArm = false;
			ikAimLeftArm = false;
		}
		forceConfigureIK = true;
	}

	public bool PollForceConfigureIK()
	{
		bool result = forceConfigureIK;
		forceConfigureIK = false;
		return result;
	}

	public bool ShouldPlayFirstUseAnimation()
	{
		if (!hasFirstUseAnimation)
		{
			return false;
		}
		if (GameOptions.GetVrAnimationMode())
		{
			return false;
		}
		if (pickupable == null)
		{
			return false;
		}
		if (Player.main.IsToolUsed(pickupable.GetTechType()))
		{
			return PlayerToolConsoleCommands.debugFirstUse;
		}
		return true;
	}

	public virtual bool OnLeftHandDown()
	{
		return false;
	}

	public virtual bool OnLeftHandHeld()
	{
		return false;
	}

	public virtual bool OnLeftHandUp()
	{
		return false;
	}

	public virtual bool OnAltDown()
	{
		return false;
	}

	public virtual bool OnAltHeld()
	{
		return false;
	}

	public virtual bool OnAltUp()
	{
		return false;
	}

	public virtual bool OnRightHandDown()
	{
		return true;
	}

	public virtual bool OnRightHandHeld()
	{
		return false;
	}

	public virtual bool OnRightHandUp()
	{
		return false;
	}

	public virtual bool OnReloadDown()
	{
		if (energyMixin == null)
		{
			energyMixin = GetComponent<EnergyMixin>();
		}
		if (energyMixin != null)
		{
			energyMixin.InitiateReload();
			return true;
		}
		return false;
	}

	public virtual bool OnExitDown()
	{
		return false;
	}

	public virtual bool GetUsedToolThisFrame()
	{
		return false;
	}

	public virtual bool GetAltUsedToolThisFrame()
	{
		return false;
	}

	public virtual bool DoesOverrideHand()
	{
		return false;
	}

	public virtual FMODAsset GetBleederHitSound(FMODAsset defaultSound)
	{
		if (!(hitBleederSound != null))
		{
			return defaultSound;
		}
		return hitBleederSound;
	}

	public virtual void OnToolBleederHitAnim(GUIHand guiHand)
	{
	}

	public virtual void OnToolUseAnim(GUIHand guiHand)
	{
	}

	public virtual void OnToolReloadBeginAnim(GUIHand guiHand)
	{
	}

	public virtual void OnToolReloadEndAnim(GUIHand guiHand)
	{
	}

	public virtual void OnToolActionStart()
	{
		if (firstUseAnimationStarted)
		{
			OnFirstUseAnimationStop();
		}
	}

	public virtual void OnToolAnimHolster()
	{
		FMODAsset fMODAsset = (Player.main.IsUnderwater() ? holsterSoundUnderwater : holsterSoundAboveWater);
		if ((bool)fMODAsset)
		{
			Utils.PlayFMODAsset(fMODAsset, base.transform);
		}
	}

	public virtual void OnToolAnimDraw()
	{
		if (waitForAnimDrawEvent && isDrawn)
		{
			SetRenderersEnabled(state: true);
			SetHandIKTargetsEnabled(enabled: true);
		}
	}

	protected virtual void OnFirstUseAnimationStop()
	{
		if ((bool)firstUseSound)
		{
			firstUseSound.Stop();
		}
	}

	public static GameObject TraceForTarget(float distance, float sphereRadius = 0.2f, bool preferSphereHits = false)
	{
		if (UWE.Utils.TraceForFPSTarget(Player.main.gameObject, distance, sphereRadius, out var closestObj, out var _, preferSphereHits))
		{
			return UWE.Utils.GetEntityRoot(closestObj);
		}
		return null;
	}

	public static LiveMixin TraceForLiveTarget(float distance)
	{
		LiveMixin liveMixin = null;
		if (UWE.Utils.TraceForFPSTarget(Player.main.gameObject, distance, distance, out var closestObj, out var _))
		{
			liveMixin = closestObj.GetComponent<LiveMixin>();
			if (!liveMixin)
			{
				Debug.Log("Trace for Target hit game obj not Livemixin: " + closestObj.name);
			}
			else
			{
				Debug.Log("Trace for Target hitLivemixin: " + liveMixin.gameObject.name);
			}
		}
		return liveMixin;
	}

	protected void UpdateControllerLightbarToToolBarValue()
	{
	}
}
