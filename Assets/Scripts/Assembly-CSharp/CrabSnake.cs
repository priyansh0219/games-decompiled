using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
public class CrabSnake : Creature, IPropulsionCannonAmmo
{
	public enum State
	{
		InMushroom = 0,
		MushroomAttack = 1,
		Chasing = 2,
		Coil = 3
	}

	public CrabsnakeAnimationController animationController;

	public FMOD_CustomLoopingEmitter swimLoopSound;

	public FMOD_StudioEventEmitter alertSound;

	[AssertNotNull]
	public Rigidbody myRigidbody;

	[AssertNotNull]
	public Collider myCollider;

	[AssertNotNull]
	public Collider coilCollider;

	[AssertNotNull]
	public SwimBehaviour swimBehaviour;

	[AssertNotNull]
	public WaterParkCreature waterParkCreature;

	public GameObject mouthTrigger;

	public GameObject coilMouthTrigger;

	public float updateCurrentMushroomInterval = 2f;

	public bool cinematicMode;

	public float leaveMushroomTime;

	private const float mushroomSwimYOffset = 10f;

	private EcoRegion.TargetFilter isTargetValidFilter;

	private CrabsnakeMushroom _currentMushroom;

	public State state;

	public Vector3 mushroomPosition { get; private set; }

	public Vector3 mushroomUp
	{
		get
		{
			if (currentMushroom != null)
			{
				return currentMushroom.GetUpDirection();
			}
			return Vector3.up;
		}
	}

	public Quaternion mushroomRotation
	{
		get
		{
			if (currentMushroom != null)
			{
				return currentMushroom.GetCrabsnakeRotation();
			}
			return Quaternion.identity;
		}
	}

	private CrabsnakeMushroom currentMushroom
	{
		get
		{
			return _currentMushroom;
		}
		set
		{
			if (_currentMushroom != null)
			{
				_currentMushroom.occupied = false;
			}
			_currentMushroom = value;
			if (_currentMushroom != null)
			{
				mushroomPosition = _currentMushroom.GetCrabsnakePosition();
				leashPosition = mushroomPosition + 10f * Vector3.up;
				_currentMushroom.occupied = true;
			}
		}
	}

	void IPropulsionCannonAmmo.OnGrab()
	{
	}

	void IPropulsionCannonAmmo.OnShoot()
	{
	}

	void IPropulsionCannonAmmo.OnRelease()
	{
	}

	void IPropulsionCannonAmmo.OnImpact()
	{
	}

	bool IPropulsionCannonAmmo.GetAllowedToGrab()
	{
		return false;
	}

	bool IPropulsionCannonAmmo.GetAllowedToShoot()
	{
		return !IsInMushroom();
	}

	private void OnAddToWaterPark()
	{
		animationController.Initialize(isInMushroom: false);
	}

	public override void OnEnable()
	{
		base.OnEnable();
		if (isInitialized)
		{
			InitializeInMushroomState();
		}
	}

	public override void Start()
	{
		base.Start();
		isTargetValidFilter = IsFreeMushroom;
	}

	protected override void InitializeOnce()
	{
		base.InitializeOnce();
		InitializeInMushroomState();
	}

	private void InitializeInMushroomState()
	{
		bool flag = !IsWaterParkCreature() && TryFindCurrentMushroom();
		state = ((!flag) ? State.Chasing : State.InMushroom);
		UWE.Utils.SetIsKinematicAndUpdateInterpolation(myRigidbody, flag);
		animationController.Initialize(flag);
		if (!IsWaterParkCreature())
		{
			InvokeRepeating("FindNearestMushroom", updateCurrentMushroomInterval, updateCurrentMushroomInterval);
		}
	}

	public override void OnDisable()
	{
		base.OnDisable();
		CancelInvoke("FindNearestMushroom");
		currentMushroom = null;
	}

	public void Update()
	{
		AllowCreatureUpdates(!cinematicMode);
		if (state == State.InMushroom && currentMushroom == null)
		{
			ExitMushroom();
		}
	}

	private void FindNearestMushroom()
	{
		if (!IsInMushroom())
		{
			IEcoTarget ecoTarget = EcoRegionManager.main.FindNearestTarget(EcoTargetType.Mushroom, base.transform.position, isTargetValidFilter);
			if (ecoTarget != null)
			{
				currentMushroom = ecoTarget.GetGameObject().GetComponent<CrabsnakeMushroom>();
			}
		}
	}

	private bool IsFreeMushroom(IEcoTarget target)
	{
		GameObject gameObject = target.GetGameObject();
		if (gameObject == null)
		{
			return false;
		}
		CrabsnakeMushroom component = gameObject.GetComponent<CrabsnakeMushroom>();
		if (component != null)
		{
			if (component.occupied)
			{
				return component == currentMushroom;
			}
			return true;
		}
		return false;
	}

	private bool TryFindCurrentMushroom()
	{
		if (currentMushroom == null)
		{
			mushroomPosition = leashPosition - Vector3.up * 10f;
			int num = UWE.Utils.OverlapSphereIntoSharedBuffer(mushroomPosition, 4f);
			for (int i = 0; i < num; i++)
			{
				CrabsnakeMushroom componentInParent = UWE.Utils.sharedColliderBuffer[i].GetComponentInParent<CrabsnakeMushroom>();
				if (componentInParent != null && !componentInParent.occupied)
				{
					currentMushroom = componentInParent;
					break;
				}
			}
		}
		return currentMushroom != null;
	}

	public Vector3 GetSwimToMushroomPosition()
	{
		return mushroomPosition + Vector3.up * animationController.enterAnimPosOffset.y;
	}

	private bool GetPlayerIsInside()
	{
		float num = 7.5f;
		Vector3 a = mushroomPosition + num * Vector3.down;
		Vector3 position = Player.main.transform.position;
		return Vector3.Distance(a, position) < num;
	}

	public void EnterMushroom()
	{
		if (animationController.IsInTransition || GetPlayerIsInside())
		{
			leaveMushroomTime = Time.time;
		}
		else if (state == State.Chasing)
		{
			state = State.InMushroom;
			UWE.Utils.SetIsKinematicAndUpdateInterpolation(myRigidbody, isKinematic: true);
			animationController.EnterMushroom();
			swimLoopSound.Stop();
		}
	}

	public void ExitMushroom(Vector3 targetPos = default(Vector3))
	{
		if (!animationController.IsInTransition && state == State.InMushroom)
		{
			state = State.Chasing;
			leaveMushroomTime = Time.time;
			if (targetPos == default(Vector3))
			{
				targetPos = base.transform.TransformPoint(Vector3.forward);
			}
			animationController.ExitMushroom(FindExitPosition(targetPos));
			swimLoopSound.Play();
		}
	}

	public void EndExitMushroom()
	{
		UWE.Utils.SetIsKinematicAndUpdateInterpolation(myRigidbody, isKinematic: false);
		swimBehaviour.SwimTo(base.transform.position + Vector3.forward * 5f, 10f);
	}

	public void EnterCoil()
	{
		if (state == State.Chasing)
		{
			state = State.Coil;
			SetCoilColliders(state: true);
			animationController.EnterCoil(Player.main.transform);
		}
	}

	public void ExitCoil()
	{
		if (state == State.Coil)
		{
			state = State.Chasing;
			SetCoilColliders(state: false);
			animationController.ExitCoil();
		}
	}

	public void StartMushroomAttack(Vector3 targetPos = default(Vector3), bool isHigh = true)
	{
		if (state == State.InMushroom)
		{
			state = State.MushroomAttack;
			if (targetPos == default(Vector3))
			{
				targetPos = base.transform.TransformPoint(Vector3.forward);
			}
			animationController.StartMushroomAttack(targetPos, isHigh);
			Utils.PlayEnvSound(alertSound);
			swimLoopSound.Play();
		}
	}

	public void EndMushroomAttack()
	{
		if (state == State.MushroomAttack)
		{
			state = State.InMushroom;
			animationController.EndMushroomAttack();
			swimLoopSound.Stop();
		}
	}

	private void SetCoilColliders(bool state)
	{
		myCollider.enabled = !state;
		coilCollider.enabled = state;
		mouthTrigger.SetActive(!state);
		coilMouthTrigger.SetActive(state);
	}

	public override void OnKill()
	{
		UWE.Utils.SetIsKinematicAndUpdateInterpolation(myRigidbody, isKinematic: false);
		SetCoilColliders(state: false);
		animationController.enabled = false;
		base.OnKill();
	}

	private Vector3 FindExitPosition(Vector3 defaultPosition)
	{
		Vector3 vector = defaultPosition;
		Vector3 vector2 = mushroomPosition + animationController.exitAnimPosOffset.y * Vector3.up;
		float maxDistance = animationController.exitAnimPosOffset.z + 5f;
		Vector3 planeNormal = mushroomUp;
		for (int i = 0; i < 10; i++)
		{
			Vector3 vector3 = Vector3.ProjectOnPlane(vector - vector2, planeNormal);
			if (!Physics.Raycast(vector2, vector3, maxDistance, Voxeland.GetTerrainLayerMask()))
			{
				return vector;
			}
			vector = ((i != 0) ? (mushroomPosition + Random.onUnitSphere) : (vector2 - vector3));
		}
		return defaultPosition;
	}

	public bool IsInMushroom()
	{
		if (state != 0)
		{
			return state == State.MushroomAttack;
		}
		return true;
	}

	public bool IsWaterParkCreature()
	{
		return waterParkCreature.bornInside;
	}

	public bool HasMushroomToHide()
	{
		return currentMushroom != null;
	}
}
