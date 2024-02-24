using System;
using UnityEngine;
using UnityEngine.XR;

public class PlayerCinematicController : MonoBehaviour, IManagedUpdateBehaviour, IManagedBehaviour, IManagedLateUpdateBehaviour
{
	private enum State
	{
		None = 0,
		In = 1,
		Update = 2,
		Out = 3
	}

	[AssertNotNull]
	public Transform animatedTransform;

	public GameObject informGameObject;

	public Transform endTransform;

	public bool onlyUseEndTransformInVr;

	public bool playInVr;

	public bool interruptAutoMove = true;

	public string playerViewAnimationName = "";

	public string playerViewInterpolateAnimParam = "";

	public string animParam = "cinematicMode";

	public string interpolateAnimParam = "";

	public float interpolationTime = 0.25f;

	public float interpolationTimeOut = 0.25f;

	public string receiversAnimParam = "";

	public GameObject[] animParamReceivers;

	public bool interpolateDuringAnimation;

	public bool debug;

	public Animator animator;

	public FMODAsset sound;

	public bool enforceCinematicModeEnd;

	[NonSerialized]
	public bool cinematicModeActive;

	private Vector3 playerFromPosition = Vector3.zero;

	private Quaternion playerFromRotation = Quaternion.identity;

	private Vector3 cameraPosition = Vector3.zero;

	private Quaternion cameraFromRotation = Quaternion.identity;

	private bool onCinematicModeEndCall;

	private float timeStateChanged;

	private State _state;

	private Player player;

	private float timeCinematicModeStarted = -1f;

	private bool subscribed;

	private bool _animState;

	public static int cinematicModeCount { get; private set; }

	public static float cinematicActivityExpireTime { get; private set; }

	private State state
	{
		get
		{
			return _state;
		}
		set
		{
			timeStateChanged = Time.time;
			_state = value;
		}
	}

	public bool animState
	{
		get
		{
			return _animState;
		}
		private set
		{
			if (value == _animState)
			{
				return;
			}
			if (debug)
			{
				Debug.Log("setting cinematic controller " + base.gameObject.name + " to: " + value);
			}
			_animState = value;
			if (animParam.Length > 0)
			{
				SafeAnimator.SetBool(animator, animParam, value);
			}
			if (receiversAnimParam.Length > 0)
			{
				for (int i = 0; i < animParamReceivers.Length; i++)
				{
					animParamReceivers[i].GetComponent<IAnimParamReceiver>()?.ForwardAnimationParameterBool(receiversAnimParam, value);
				}
			}
			if (playerViewAnimationName.Length > 0 && (bool)player)
			{
				Animator componentInChildren = player.GetComponentInChildren<Animator>();
				if (componentInChildren != null && componentInChildren.gameObject.activeInHierarchy)
				{
					SafeAnimator.SetBool(componentInChildren, playerViewAnimationName, value);
				}
			}
			SetVrActiveParam();
			if ((bool)sound && value)
			{
				Utils.PlayFMODAsset(sound, base.transform, 0f);
			}
		}
	}

	public int managedUpdateIndex { get; set; }

	public int managedLateUpdateIndex { get; set; }

	public string GetProfileTag()
	{
		return "PlayerCinematicController";
	}

	public void SetPlayer(Player setplayer)
	{
		if (subscribed && player != setplayer)
		{
			Subscribe(player, state: false);
			Subscribe(setplayer, state: true);
		}
		player = setplayer;
	}

	public Player GetPlayer()
	{
		return player;
	}

	private void AddToUpdateManager()
	{
		BehaviourUpdateUtils.Register((IManagedUpdateBehaviour)this);
		BehaviourUpdateUtils.Register((IManagedLateUpdateBehaviour)this);
	}

	private void RemoveFromUpdateManager()
	{
		BehaviourUpdateUtils.Deregister((IManagedUpdateBehaviour)this);
		BehaviourUpdateUtils.Deregister((IManagedLateUpdateBehaviour)this);
	}

	private void OnEnable()
	{
		AddToUpdateManager();
	}

	private void OnDestroy()
	{
		RemoveFromUpdateManager();
	}

	private void Start()
	{
		if (animator == null)
		{
			animator = GetComponent<Animator>();
		}
		SetVrActiveParam();
	}

	private void SetVrActiveParam()
	{
		string paramaterName = "vr_active";
		bool vrAnimationMode = GameOptions.GetVrAnimationMode();
		if (animator != null)
		{
			animator.SetBool(paramaterName, vrAnimationMode);
		}
		for (int i = 0; i < animParamReceivers.Length; i++)
		{
			animParamReceivers[i].GetComponent<IAnimParamReceiver>()?.ForwardAnimationParameterBool(paramaterName, vrAnimationMode);
		}
	}

	private bool UseEndTransform()
	{
		if (endTransform == null)
		{
			return false;
		}
		if (onlyUseEndTransformInVr)
		{
			return GameOptions.GetVrAnimationMode();
		}
		return true;
	}

	private void SkipCinematic(Player player)
	{
		if (interruptAutoMove)
		{
			GameInput.AutoMove = false;
		}
		this.player = player;
		if ((bool)player)
		{
			Transform component = player.GetComponent<Transform>();
			Transform component2 = MainCameraControl.main.GetComponent<Transform>();
			if (UseEndTransform())
			{
				player.playerController.SetEnabled(enabled: false);
				if (XRSettings.enabled)
				{
					MainCameraControl.main.ResetCamera();
					VRUtil.Recenter();
				}
				component.position = endTransform.position;
				component.rotation = endTransform.rotation;
				component2.rotation = component.rotation;
			}
			player.playerController.SetEnabled(enabled: true);
			player.cinematicModeActive = false;
		}
		if (informGameObject != null)
		{
			informGameObject.SendMessage("OnPlayerCinematicModeEnd", this, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void StartCinematicMode(Player setplayer)
	{
		if (debug)
		{
			Debug.Log(base.gameObject.name + ".StartCinematicMode");
		}
		if (!cinematicModeActive)
		{
			player = null;
			if (!playInVr && GameOptions.GetVrAnimationMode())
			{
				if (debug)
				{
					Debug.Log(base.gameObject.name + " skip cinematic");
				}
				SkipCinematic(setplayer);
				return;
			}
			animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
			cinematicModeActive = true;
			timeCinematicModeStarted = Time.time;
			if ((bool)setplayer)
			{
				SetPlayer(setplayer);
				Subscribe(player, state: true);
			}
			state = State.In;
			if (informGameObject != null)
			{
				informGameObject.SendMessage("OnPlayerCinematicModeStart", this, SendMessageOptions.DontRequireReceiver);
			}
			if ((bool)player)
			{
				Transform component = player.GetComponent<Transform>();
				Transform component2 = MainCameraControl.main.GetComponent<Transform>();
				cameraPosition = player.camAnchor.InverseTransformPoint(component2.position);
				cameraFromRotation = component2.rotation;
				player.cinematicModeActive = true;
				player.FreezeStats();
				player.playerController.SetEnabled(enabled: false);
				playerFromPosition = component.position;
				playerFromRotation = component.rotation;
				if (playerViewInterpolateAnimParam.Length > 0)
				{
					SafeAnimator.SetBool(player.GetComponentInChildren<Animator>(), playerViewInterpolateAnimParam, value: true);
				}
			}
			if (interpolateAnimParam.Length > 0)
			{
				SafeAnimator.SetBool(animator, interpolateAnimParam, value: true);
			}
			if (interpolateDuringAnimation)
			{
				animState = true;
			}
			if (debug)
			{
				Debug.Log(base.gameObject.name + " successfully started cinematic");
			}
			float num = 60f;
			float num2 = Time.time + num;
			cinematicActivityExpireTime = ((cinematicModeCount == 0) ? num2 : Mathf.Max(cinematicActivityExpireTime, num2));
			cinematicModeCount++;
		}
		else if (debug)
		{
			Debug.Log(base.gameObject.name + " cinematic already active!");
		}
	}

	private void EndCinematicMode()
	{
		if (cinematicModeActive)
		{
			if (interruptAutoMove)
			{
				GameInput.AutoMove = false;
			}
			animator.cullingMode = AnimatorCullingMode.CullCompletely;
			animState = false;
			state = State.None;
			if ((bool)player)
			{
				player.playerController.SetEnabled(enabled: true);
				player.cinematicModeActive = false;
				player.UnfreezeStats();
			}
			cinematicModeActive = false;
			cinematicModeCount--;
		}
	}

	public void OnPlayerCinematicModeEnd()
	{
		if (!cinematicModeActive || onCinematicModeEndCall)
		{
			return;
		}
		if ((bool)player)
		{
			UpdatePlayerPosition();
		}
		animState = false;
		if (UseEndTransform())
		{
			state = State.Out;
			if ((bool)player)
			{
				Transform component = player.GetComponent<Transform>();
				playerFromPosition = component.position;
				playerFromRotation = component.rotation;
			}
		}
		else
		{
			EndCinematicMode();
		}
		if (informGameObject != null)
		{
			onCinematicModeEndCall = true;
			informGameObject.SendMessage("OnPlayerCinematicModeEnd", this, SendMessageOptions.DontRequireReceiver);
			onCinematicModeEndCall = false;
		}
		if ((bool)player)
		{
			player.playerController.ForceControllerSize();
		}
	}

	private void UpdatePlayerPosition()
	{
		Transform component = player.GetComponent<Transform>();
		Transform component2 = MainCameraControl.main.GetComponent<Transform>();
		component.position = animatedTransform.position;
		component.rotation = animatedTransform.rotation;
		component2.position = player.camAnchor.position;
		component2.rotation = animatedTransform.rotation;
	}

	public void ManagedLateUpdate()
	{
		if (!cinematicModeActive)
		{
			return;
		}
		float num = Time.time - timeStateChanged;
		float num2 = 0f;
		Transform transform = null;
		Transform transform2 = null;
		if ((bool)player)
		{
			transform = player.GetComponent<Transform>();
			transform2 = MainCameraControl.main.GetComponent<Transform>();
		}
		bool flag = !GameOptions.GetVrAnimationMode();
		switch (state)
		{
		case State.In:
			num2 = ((interpolationTime != 0f && flag) ? Mathf.Clamp01(num / interpolationTime) : 1f);
			if ((bool)player)
			{
				transform.position = Vector3.Lerp(playerFromPosition, animatedTransform.position, num2);
				transform.rotation = Quaternion.Slerp(playerFromRotation, animatedTransform.rotation, num2);
				transform2.position = player.camAnchor.position + cameraPosition * (1f - num2);
				transform2.rotation = Quaternion.Slerp(cameraFromRotation, animatedTransform.rotation, num2);
			}
			if (num2 == 1f)
			{
				state = State.Update;
				animState = true;
				if (interpolateAnimParam.Length > 0)
				{
					SafeAnimator.SetBool(animator, interpolateAnimParam, value: false);
				}
				if (playerViewInterpolateAnimParam.Length > 0 && (bool)player)
				{
					SafeAnimator.SetBool(player.GetComponentInChildren<Animator>(), playerViewInterpolateAnimParam, value: false);
				}
			}
			break;
		case State.Update:
			if ((bool)player)
			{
				UpdatePlayerPosition();
			}
			if (enforceCinematicModeEnd && !animator.IsInTransition(0) && num >= animator.GetCurrentAnimatorStateInfo(0).length)
			{
				OnPlayerCinematicModeEnd();
				if (debug)
				{
					Debug.Log(base.gameObject.name + " has exceeded current animation time, forcing OnPlayerCinematicModeEnd");
				}
			}
			break;
		case State.Out:
			num2 = ((interpolationTimeOut != 0f && flag) ? Mathf.Clamp01(num / interpolationTimeOut) : 1f);
			if ((bool)player)
			{
				transform.position = Vector3.Lerp(playerFromPosition, endTransform.position, num2);
				transform.rotation = Quaternion.Slerp(playerFromRotation, endTransform.rotation, num2);
				transform2.rotation = transform.rotation;
			}
			if (num2 == 1f)
			{
				EndCinematicMode();
			}
			break;
		}
	}

	public bool IsCinematicModeActive()
	{
		return cinematicModeActive;
	}

	private void OnDisable()
	{
		RemoveFromUpdateManager();
		if (subscribed)
		{
			Subscribe(player, state: false);
		}
		EndCinematicMode();
	}

	private void OnPlayerDeath(Player player)
	{
		EndCinematicMode();
		animator.Rebind();
	}

	private void Subscribe(Player player, bool state)
	{
		if (player == null)
		{
			subscribed = false;
		}
		else if (subscribed != state)
		{
			if (state)
			{
				player.playerDeathEvent.AddHandler(base.gameObject, OnPlayerDeath);
			}
			else
			{
				player.playerDeathEvent.RemoveHandler(base.gameObject, OnPlayerDeath);
			}
			subscribed = state;
		}
	}

	public void ManagedUpdate()
	{
		if (!cinematicModeActive && subscribed)
		{
			Subscribe(player, state: false);
		}
	}
}
