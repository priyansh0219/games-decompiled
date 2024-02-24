using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class BaseSpotLight : Constructable
{
	public GameObject light;

	public Transform foundationPivot;

	public Transform lightPivot;

	public VFXSpotlight vfxSpotLight;

	public Creature trackTarget;

	private EcoRegion.TargetFilter isTargetValidFilter;

	private static float powerPerSecond = 0.2f;

	private static float updateInterval = 3f;

	private static float trackingDistance = 20f;

	private static float maxPitch = 16f;

	private static float yawAnimMin = -90f;

	private static float yawAnimMax = 90f;

	private static float pitchAnimMin = 320f;

	private static float pitchAnimMax = 360f + maxPitch;

	private static float animDuration = 15f;

	private float randomTime;

	private PowerRelay powerRelay;

	private bool _powered;

	private float targetPitch;

	private float targetYaw;

	private float currentPitch;

	private float currentYaw;

	private bool powered
	{
		get
		{
			return _powered;
		}
		set
		{
			if (_powered != value)
			{
				light.SetActive(value);
				vfxSpotLight.SetLightActive(value);
				if (!value)
				{
					trackTarget = null;
				}
			}
			_powered = value;
		}
	}

	protected override void Start()
	{
		base.Start();
		isTargetValidFilter = IsValidEcoTarget;
		randomTime = UnityEngine.Random.Range(0f, 1f) * animDuration;
		powerRelay = base.gameObject.GetComponentInParent<PowerRelay>();
		light.SetActive(value: false);
		vfxSpotLight.SetLightActive(active: false);
		InvokeRepeating("UpdatePower", 0f, updateInterval);
		InvokeRepeating("UpdateTarget", 4f, 2f);
	}

	private bool GetLightsActive()
	{
		if (GameModeUtils.RequiresPower())
		{
			if ((bool)powerRelay && powerRelay.IsPowered() && base.constructed)
			{
				return powerRelay.GetPower() >= powerPerSecond * updateInterval;
			}
			return false;
		}
		return true;
	}

	private void UpdatePower()
	{
		powered = GetLightsActive();
		if (powered && powerRelay != null)
		{
			powerRelay.ConsumeEnergy(powerPerSecond * updateInterval, out var _);
		}
	}

	public void LookAt(Vector3 point)
	{
		Vector3 eulerAngles = Quaternion.LookRotation(base.transform.InverseTransformDirection(Vector3.Normalize(point - base.transform.position))).eulerAngles;
		float num = eulerAngles.x;
		if (num < 270f)
		{
			num += 360f;
			num = Mathf.Clamp(num, 270f, 360f + maxPitch);
			num -= 360f;
		}
		targetPitch = num;
		targetYaw = eulerAngles.y;
	}

	public void UpdateSweepAnimation()
	{
		float num = (Time.time - randomTime) % animDuration / animDuration;
		num = Mathf.Abs((1f + Mathf.Cos(num * (float)Math.PI * 2f)) / 2f);
		targetYaw = yawAnimMin + (yawAnimMax - yawAnimMin) * num;
		float num2 = (Time.time - randomTime) % (animDuration * 1.5f) / (animDuration * 1.5f);
		num2 = Mathf.Abs((1f + Mathf.Cos(num2 * (float)Math.PI * 2f)) / 2f);
		targetPitch = pitchAnimMin + (pitchAnimMax - pitchAnimMin) * num2;
	}

	private bool IsValidTarget(Creature target)
	{
		if (target == null)
		{
			return false;
		}
		if (CreatureData.GetBehaviourType(target.gameObject) != BehaviourType.Shark)
		{
			return false;
		}
		Vector3 value = target.transform.position - base.transform.position;
		float x = Quaternion.LookRotation(base.transform.InverseTransformDirection(Vector3.Normalize(value))).eulerAngles.x;
		if (x < 270f && x + 360f > 360f + maxPitch)
		{
			return false;
		}
		if (value.magnitude > trackingDistance)
		{
			return false;
		}
		return true;
	}

	private bool IsValidTarget(GameObject target)
	{
		if (target != null)
		{
			return IsValidTarget(target.GetComponent<Creature>());
		}
		return false;
	}

	private bool IsValidEcoTarget(IEcoTarget ecoTarget)
	{
		return IsValidTarget(ecoTarget.GetGameObject());
	}

	private void FindTarget()
	{
		if (EcoRegionManager.main != null)
		{
			IEcoTarget ecoTarget = EcoRegionManager.main.FindNearestTarget(EcoTargetType.Shark, base.transform.position, isTargetValidFilter);
			if (ecoTarget != null)
			{
				trackTarget = ecoTarget.GetGameObject().GetComponent<Creature>();
				Debug.DrawLine(base.transform.position, ecoTarget.GetPosition(), Color.red, 10f);
			}
		}
	}

	private void UpdateTarget()
	{
		if ((bool)trackTarget && !IsValidTarget(trackTarget))
		{
			trackTarget = null;
		}
		if (trackTarget == null)
		{
			FindTarget();
		}
	}

	public void Update()
	{
		if (powered)
		{
			if ((bool)trackTarget)
			{
				LookAt(trackTarget.transform.position);
			}
			else
			{
				UpdateSweepAnimation();
			}
			currentYaw = Mathf.LerpAngle(currentYaw, targetYaw, Time.deltaTime * 2f);
			currentPitch = Mathf.LerpAngle(currentPitch, targetPitch, Time.deltaTime * 2f);
			foundationPivot.localEulerAngles = new Vector3(0f, currentYaw, 0f);
			lightPivot.localEulerAngles = new Vector3(currentPitch, 0f, 0f);
		}
	}

	public override bool UpdateGhostModel(Transform aimTransform, GameObject ghostModel, RaycastHit hit, out bool geometryChanged, ConstructableBase ghostModelParentConstructableBase)
	{
		bool result = false;
		geometryChanged = false;
		if ((bool)hit.collider && (bool)hit.collider.gameObject)
		{
			ghostModel.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(aimTransform.forward, hit.normal), hit.normal);
			result = Constructable.CheckFlags(allowedInBase, allowedInSub, allowedOutside, allowedUnderwater, hit.point) && hit.collider.gameObject.GetComponentInParent<Base>() != null;
		}
		return result;
	}
}
