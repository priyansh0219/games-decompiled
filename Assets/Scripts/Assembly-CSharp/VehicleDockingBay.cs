using UWE;
using UnityEngine;

public class VehicleDockingBay : MonoBehaviour, IAnimParamReceiver
{
	public delegate void OnDockedChanged();

	public OnDockedChanged onDockedChanged;

	public Animator animator;

	public Transform dockingEndPos;

	public Transform dockingEndPosExo;

	public Transform exit;

	public SubRoot subRoot;

	public PlayerCinematicController dockPlayerCinematic;

	public PlayerCinematicController exosuitDockPlayerCinematic;

	public float interpolationTime = 1f;

	[SerializeField]
	[Tooltip("Whether the animator needs to have a particular state before docking can take place")]
	private bool requiresAnimationStateForDocking;

	[SerializeField]
	private string animationStateNameForDocking = string.Empty;

	[SerializeField]
	private bool requiresUndockingClearance;

	[SerializeField]
	private float undockingClearanceCastDistance = 4f;

	[SerializeField]
	private BoxCollider castBoxCollider;

	[AssertNotNull]
	public GameObject dockedCollision;

	public FMOD_CustomEmitter bayDoorsOpenSFX;

	public FMOD_CustomEmitter bayDoorsCloseSFX;

	private PowerRelay powerRelay;

	private float timeDockingStarted;

	private Vehicle interpolatingVehicle;

	private Vehicle _dockedVehicle;

	private Vehicle nearbyVehicle;

	private bool vehicle_docked_param;

	private Vector3 startPosition;

	private Quaternion startRotation;

	private bool soundReset = true;

	private Vehicle dockedVehicle
	{
		get
		{
			return _dockedVehicle;
		}
		set
		{
			_dockedVehicle = value;
			dockedCollision.SetActive(value != null);
			if (onDockedChanged != null)
			{
				onDockedChanged();
			}
		}
	}

	private bool IsPowered()
	{
		if (powerRelay != null)
		{
			return powerRelay.IsPowered();
		}
		return false;
	}

	private void OnDestroy()
	{
		if ((bool)dockedVehicle)
		{
			dockedVehicle.ReAttach(exit.position);
		}
	}

	private void Start()
	{
		powerRelay = GetComponentInParent<PowerRelay>();
		if (!subRoot)
		{
			subRoot = GetComponentInParent<SubRoot>();
		}
		dockedCollision.SetActive(dockedVehicle != null);
	}

	void IAnimParamReceiver.ForwardAnimationParameterBool(string parameterName, bool value)
	{
		IAnimParamReceiver animParamReceiver = dockedVehicle;
		if (animParamReceiver != null && !animParamReceiver.Equals(null))
		{
			animParamReceiver.ForwardAnimationParameterBool(parameterName, value);
		}
	}

	public Vehicle GetDockedVehicle()
	{
		return dockedVehicle;
	}

	public bool HasUndockingClearance()
	{
		if (requiresUndockingClearance)
		{
			OrientedBounds orientedBounds = OrientedBounds.FromCollider(castBoxCollider);
			return !Physics.BoxCast(orientedBounds.position, orientedBounds.extents, -castBoxCollider.transform.up, orientedBounds.rotation, undockingClearanceCastDistance, -5, QueryTriggerInteraction.Ignore);
		}
		return true;
	}

	private void OnDrawGizmosSelected()
	{
		if (requiresUndockingClearance)
		{
			Color cyan = Color.cyan;
			Vector3 vector = -castBoxCollider.transform.up;
			OrientedBounds orientedBounds = OrientedBounds.FromCollider(castBoxCollider);
			Vector3 position = orientedBounds.position;
			Debug.DrawRay(position, vector * undockingClearanceCastDistance, cyan);
			Gizmos.color = cyan;
			for (float num = 0f; num < undockingClearanceCastDistance; num += 0.1f)
			{
				orientedBounds.position = position + vector * num;
			}
		}
	}

	public void OnUndockingComplete(Player player)
	{
		player.SetCurrentSub(null);
		if (dockedVehicle != null)
		{
			StartCoroutine(dockedVehicle.Undock(player, base.transform.position.y));
			SkyEnvironmentChanged.Broadcast(dockedVehicle.gameObject, (GameObject)null);
		}
		else
		{
			player.inExosuit = false;
			player.inSeamoth = false;
		}
		dockedVehicle = null;
	}

	private void OnTriggerEnter(Collider other)
	{
		Vehicle componentInHierarchy = UWE.Utils.GetComponentInHierarchy<Vehicle>(other.gameObject);
		if (!(componentInHierarchy == null) && !componentInHierarchy.docked && !componentInHierarchy.GetRecentlyUndocked() && !GetDockedVehicle() && (!GameModeUtils.RequiresPower() || IsPowered()) && (!requiresAnimationStateForDocking || animator.GetCurrentAnimatorStateInfo(0).IsName(animationStateNameForDocking)) && !(interpolatingVehicle != null))
		{
			timeDockingStarted = Time.time;
			interpolatingVehicle = componentInHierarchy;
			startPosition = interpolatingVehicle.transform.position;
			startRotation = interpolatingVehicle.transform.rotation;
		}
	}

	public SubRoot GetSubRoot()
	{
		if (!subRoot)
		{
			subRoot = GetComponentInParent<SubRoot>();
		}
		return subRoot;
	}

	private void LaunchbayAreaEnter(GameObject nearby)
	{
		Vehicle componentInHierarchy = UWE.Utils.GetComponentInHierarchy<Vehicle>(nearby.gameObject);
		if (!(componentInHierarchy != null) || componentInHierarchy.docked)
		{
			return;
		}
		if (IsPowered())
		{
			SubRoot obj = GetSubRoot();
			obj.BroadcastMessage("OnLaunchBayOpening", SendMessageOptions.DontRequireReceiver);
			obj.BroadcastMessage("LockDoors", SendMessageOptions.DontRequireReceiver);
			if (soundReset)
			{
				if (bayDoorsOpenSFX != null)
				{
					bayDoorsOpenSFX.Play();
				}
				soundReset = false;
				Invoke("SoundReset", 1f);
			}
		}
		nearbyVehicle = componentInHierarchy;
	}

	private void LaunchbayAreaExit(GameObject nearby)
	{
		if (!(UWE.Utils.GetComponentInHierarchy<Vehicle>(nearby.gameObject) != null))
		{
			return;
		}
		nearbyVehicle = null;
		GetSubRoot().BroadcastMessage("UnlockDoors", SendMessageOptions.DontRequireReceiver);
		if (soundReset)
		{
			if (bayDoorsCloseSFX != null)
			{
				bayDoorsCloseSFX.Play();
			}
			soundReset = false;
			Invoke("SoundReset", 1f);
		}
	}

	public void DockVehicle(Vehicle vehicle, bool rebuildBase = false)
	{
		dockedVehicle = vehicle;
		LargeWorldStreamer.main.cellManager.UnregisterEntity(dockedVehicle.gameObject);
		dockedVehicle.transform.parent = GetSubRoot().transform;
		vehicle.docked = true;
		vehicle_docked_param = true;
		if (Player.main.currentMountedVehicle == vehicle)
		{
			Player.main.currentMountedVehicle = null;
		}
		SkyEnvironmentChanged.Broadcast(dockedVehicle.gameObject, subRoot);
		if (!rebuildBase)
		{
			CancelInvoke("RepairVehicle");
			InvokeRepeating("RepairVehicle", 0f, 5f);
		}
		GoalManager.main.OnCustomGoalEvent("Dock_Seamoth");
		GetSubRoot().BroadcastMessage("UnlockDoors", SendMessageOptions.DontRequireReceiver);
	}

	public void SetVehicleDocked(Vehicle vehicle)
	{
		if (dockedVehicle == null)
		{
			vehicle.docked = true;
			dockedVehicle = vehicle;
			vehicle_docked_param = true;
			LargeWorldStreamer.main.cellManager.UnregisterEntity(dockedVehicle.gameObject);
			startPosition = dockedVehicle.transform.position;
			startRotation = dockedVehicle.transform.rotation;
		}
	}

	public void SetVehicleUndocked()
	{
		if (dockedVehicle != null)
		{
			if (exit != null)
			{
				dockedVehicle.transform.position = exit.position;
			}
			dockedVehicle.docked = false;
		}
		CancelInvoke("RepairVehicle");
	}

	public void OnUndockingStart()
	{
		vehicle_docked_param = false;
		dockedVehicle.SetPlayerInside(inside: true);
	}

	private void RepairVehicle()
	{
		bool flag = true;
		if (dockedVehicle == null)
		{
			flag = false;
		}
		else if (dockedVehicle.liveMixin.IsFullHealth())
		{
			flag = false;
		}
		if (!subRoot.vehicleRepairUpgrade)
		{
			flag = false;
		}
		if (flag)
		{
			dockedVehicle.liveMixin.AddHealth(25f);
		}
	}

	private void UpdateDockedPosition(Vehicle vehicle, float interpfraction)
	{
		Transform transform = dockingEndPos;
		if (vehicle is Exosuit)
		{
			transform = dockingEndPosExo;
		}
		vehicle.transform.position = Vector3.Lerp(startPosition, transform.position, interpfraction);
		vehicle.transform.rotation = Quaternion.Lerp(startRotation, transform.rotation, interpfraction);
	}

	private void LateUpdate()
	{
		Vehicle vehicle = dockedVehicle;
		if (interpolatingVehicle != null)
		{
			vehicle = interpolatingVehicle;
		}
		if (vehicle != null)
		{
			float num = 1f;
			if (interpolationTime > 0f)
			{
				num = Mathf.Clamp01((Time.time - timeDockingStarted) / interpolationTime);
			}
			UpdateDockedPosition(vehicle, num);
			if (interpolatingVehicle != null && num == 1f)
			{
				DockVehicle(interpolatingVehicle);
				interpolatingVehicle = null;
				Player player = null;
				if (vehicle.GetPilotingMode())
				{
					player = Player.main;
					player.transform.parent = null;
					player.SetCurrentSub(GetSubRoot());
					player.ToNormalMode(findNewPosition: false);
				}
				if (vehicle is Exosuit)
				{
					exosuitDockPlayerCinematic.StartCinematicMode(player);
				}
				else
				{
					dockPlayerCinematic.StartCinematicMode(player);
				}
			}
		}
		bool value = nearbyVehicle != null && IsPowered();
		SafeAnimator.SetBool(animator, "vehicle_nearby", value);
		SafeAnimator.SetBool(animator, "seamoth_docked", vehicle_docked_param && dockedVehicle != null && dockedVehicle is SeaMoth);
		SafeAnimator.SetBool(animator, "exosuit_docked", vehicle_docked_param && dockedVehicle != null && dockedVehicle is Exosuit);
	}

	private void SoundReset()
	{
		soundReset = true;
	}
}
