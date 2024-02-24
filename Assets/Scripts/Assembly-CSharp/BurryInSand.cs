using UWE;
using UnityEngine;

[RequireComponent(typeof(SwimBehaviour))]
public class BurryInSand : CreatureAction
{
	private bool burried;

	private Vector3 startPoint;

	private Vector3 burryPosition;

	private bool foundPosition;

	private float burryDistanceSq = 16f;

	private Quaternion floorAlign;

	private bool coverInSandEffectStarted;

	public float sandCoverAmount;

	public FMOD_StudioEventEmitter burrowSound;

	public GameObject dunePrefab;

	public VFXController sandTrailFX;

	private GameObject burrowFXinstance;

	private VFXSandCover sandCoverFX;

	private Animator[] animators;

	private float disableTime;

	public ConstantForce descendForce;

	public float swimVelocity = 5f;

	public float swimInterval = 3f;

	private float timeNextSwim;

	public void Disable(float forTime)
	{
		disableTime = Time.time + forTime;
	}

	private void Start()
	{
		animators = GetComponentsInChildren<Animator>();
		startPoint = base.transform.position;
	}

	public override void OnEnable()
	{
		base.OnEnable();
		startPoint = base.transform.position;
	}

	private void PlaySandTrailFX()
	{
		if (sandTrailFX != null)
		{
			sandTrailFX.Play();
		}
	}

	private void StopSandTrailFX()
	{
		if (sandTrailFX != null)
		{
			sandTrailFX.Stop();
		}
	}

	public override float Evaluate(Creature creature, float time)
	{
		if (time > disableTime)
		{
			return GetEvaluatePriority();
		}
		return 0f;
	}

	private bool FindBurryPosition(out Vector3 position, out Quaternion align)
	{
		position = Vector3.zero;
		align = Quaternion.identity;
		bool flag = false;
		Vector3 origin = startPoint + Random.onUnitSphere + Vector3.up;
		if (UWE.Utils.TraceForTerrain(new Ray(origin, Vector3.down), 3000f, out var hitInfo))
		{
			position = hitInfo.point;
			floorAlign = Quaternion.LookRotation(Vector3.Cross(hitInfo.normal, -base.transform.right));
			return true;
		}
		return false;
	}

	public bool IsBurried()
	{
		return burried;
	}

	private bool UpdateAlign()
	{
		Vector3 eulerAngles = base.transform.eulerAngles;
		base.transform.rotation = Quaternion.Lerp(base.transform.rotation, floorAlign, 2f * Time.deltaTime);
		return (eulerAngles - base.transform.eulerAngles).sqrMagnitude < 0.01f;
	}

	private void TriggerSandCoverEffect()
	{
		coverInSandEffectStarted = true;
		if (burrowSound != null)
		{
			Utils.PlayEnvSound(burrowSound, base.transform.position);
		}
		PlaySandTrailFX();
		if (dunePrefab != null)
		{
			burrowFXinstance = Utils.SpawnPrefabAt(dunePrefab, null, burryPosition);
			burrowFXinstance.transform.eulerAngles = new Vector3(0f, base.transform.eulerAngles.y, 0f);
			burrowFXinstance.transform.localScale = base.transform.localScale;
			sandCoverFX = burrowFXinstance.GetComponent<VFXSandCover>();
			sandCoverFX.Invoke("StartBury", 0.01f);
		}
	}

	public override void Perform(Creature creature, float time, float deltaTime)
	{
		if (burried)
		{
			return;
		}
		if (!foundPosition)
		{
			foundPosition = FindBurryPosition(out burryPosition, out floorAlign);
		}
		if (!foundPosition)
		{
			return;
		}
		Vector3 vector = burryPosition - base.transform.position;
		float sqrMagnitude = vector.sqrMagnitude;
		if (sqrMagnitude <= 1.5f && !coverInSandEffectStarted)
		{
			TriggerSandCoverEffect();
		}
		if (sqrMagnitude <= 0.001f)
		{
			if (UpdateAlign())
			{
				Burry();
			}
			return;
		}
		if (sqrMagnitude <= burryDistanceSq)
		{
			GetComponent<Rigidbody>().isKinematic = true;
			base.transform.position = base.transform.position + vector * deltaTime;
			UpdateAlign();
			return;
		}
		GetComponent<Rigidbody>().isKinematic = false;
		if (time > timeNextSwim)
		{
			timeNextSwim = time + swimInterval;
			base.swimBehaviour.SwimTo(burryPosition, floorAlign * Vector3.forward, swimVelocity);
		}
	}

	public override void StartPerform(Creature creature, float time)
	{
		burried = false;
		foundPosition = false;
		coverInSandEffectStarted = false;
		descendForce.enabled = true;
		descendForce.force = new Vector3(0f, -10f, 0f);
	}

	public override void StopPerform(Creature creature, float time)
	{
		descendForce.enabled = false;
		burried = false;
		foundPosition = false;
		coverInSandEffectStarted = false;
	}

	private void Update()
	{
		if (!GetComponent<Rigidbody>().isKinematic)
		{
			if (sandCoverFX != null && !sandCoverFX.isDiggingUp)
			{
				PlaySandTrailFX();
				Invoke("StopSandTrailFX", 4f);
				sandCoverFX.DigUp();
			}
			for (int i = 0; i < animators.Length; i++)
			{
				SafeAnimator.SetBool(animators[i], "in_ground", value: false);
			}
		}
		float num = -1f;
		if (coverInSandEffectStarted)
		{
			num = 1f;
		}
		sandCoverAmount = Mathf.Clamp01(sandCoverAmount + Time.deltaTime * num);
	}

	private void Burry()
	{
		GetComponent<Rigidbody>().isKinematic = true;
		burried = true;
		base.transform.position = burryPosition;
		if (sandCoverFX != null)
		{
			sandCoverFX.StopBury();
		}
		StopSandTrailFX();
		for (int i = 0; i < animators.Length; i++)
		{
			SafeAnimator.SetBool(animators[i], "in_ground", value: true);
		}
	}
}
