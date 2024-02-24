using System.Collections.Generic;
using UnityEngine;

public class SeamothTorpedo : Bullet
{
	private struct PossibleTarget
	{
		public EcoTargetType type;

		public GameObject go;
	}

	public GameObject explosionPrefab;

	public GameObject trailPrefab;

	public float speed = 10f;

	public float minExplosionDistanceFromPlayer = 10f;

	public float lifeTime = 3f;

	public FMODAsset fireSound;

	public bool homingTorpedo;

	public float homingAcuity = 1f;

	public float homingRange = 50f;

	private GameObject trailGO;

	private GameObject homingTarget;

	private readonly HashSet<EcoTargetType> targetPreferences = new HashSet<EcoTargetType>(EcoTarget.EcoTargetTypeComparer)
	{
		EcoTargetType.Leviathan,
		EcoTargetType.Shark
	};

	private static List<PossibleTarget> possibleTargets = new List<PossibleTarget>();

	protected override bool speedDependsOnEnergy => false;

	protected override float shellRadius => base.shellRadius;

	protected override void Awake()
	{
		base.Awake();
		if (explosionPrefab == null)
		{
			Debug.LogError("SeamothTorpedo : Awake() : explosionPrefab is not assigned");
		}
		if (trailPrefab == null)
		{
			Debug.LogError("SeamothTorpedo : Awake() : trailPrefab is not assigned");
		}
		else
		{
			trailGO = Utils.SpawnZeroedAt(trailPrefab, base.transform);
		}
	}

	private void Start()
	{
		if (fireSound != null)
		{
			FMODUWE.PlayOneShot(fireSound, base.tr.position);
		}
		if (homingTorpedo)
		{
			InvokeRepeating("RepeatingTargeting", 0.01f, 0.5f);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (base.tr.position.y >= 0f)
		{
			Explode();
		}
		if (homingTorpedo && (bool)homingTarget)
		{
			Quaternion b = Quaternion.LookRotation((homingTarget.transform.position - base.transform.position).normalized);
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, b, Time.deltaTime * homingAcuity);
		}
	}

	public override void Shoot(Vector3 position, Quaternion rotation, float speed, float lifeTime)
	{
		base.Shoot(position, rotation, this.speed + speed, this.lifeTime);
	}

	protected override void OnHit(RaycastHit hitInfo)
	{
		Explode();
	}

	protected override void OnEnergyDepleted()
	{
		Explode();
	}

	private void RepeatingTargeting()
	{
		homingTarget = TryAcquireTarget();
		if ((bool)homingTarget)
		{
			CancelInvoke();
		}
	}

	private GameObject TryAcquireTarget()
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("Creature");
		GameObject result = null;
		float num = homingRange * homingRange;
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			EcoTarget component = gameObject.GetComponent<EcoTarget>();
			if ((bool)component && targetPreferences.Contains(component.type) && (gameObject.transform.position - base.transform.position).sqrMagnitude <= num)
			{
				PossibleTarget item = default(PossibleTarget);
				item.type = component.type;
				item.go = gameObject;
				possibleTargets.Add(item);
			}
		}
		if (possibleTargets.Count == 0)
		{
			return null;
		}
		int num2 = 999;
		foreach (PossibleTarget possibleTarget in possibleTargets)
		{
			Vector3 lhs = possibleTarget.go.transform.position - base.transform.position;
			lhs.Normalize();
			if (Vector3.Dot(lhs, base.transform.forward) < 0.25f)
			{
				continue;
			}
			int num3 = 0;
			foreach (EcoTargetType targetPreference in targetPreferences)
			{
				if (possibleTarget.type == targetPreference && num3 < num2)
				{
					num2 = num3;
					result = possibleTarget.go;
				}
				num3++;
			}
		}
		possibleTargets.Clear();
		return result;
	}

	private bool CheckDistance()
	{
		Player main = Player.main;
		if (main != null)
		{
			SNCameraRoot camRoot = main.camRoot;
			if (camRoot != null)
			{
				Transform aimingTransform = camRoot.GetAimingTransform();
				if (aimingTransform != null)
				{
					if ((aimingTransform.position - base.tr.position).sqrMagnitude > minExplosionDistanceFromPlayer * minExplosionDistanceFromPlayer)
					{
						return true;
					}
				}
				else
				{
					Debug.LogError("SeamothTorpedo : Explode() : Player.main.camRoot.GetAimingTransform() returned null");
				}
			}
			else
			{
				Debug.LogError("SeamothTorpedo : Explode() : Player.main.camRoot is null");
			}
		}
		else
		{
			Debug.LogError("SeamothTorpedo : Explode() : Player.main is null");
		}
		return false;
	}

	private void Explode()
	{
		Transform component = Object.Instantiate(explosionPrefab).GetComponent<Transform>();
		component.position = base.tr.position;
		component.rotation = base.tr.rotation;
		if (trailGO != null)
		{
			trailGO.transform.parent = null;
			trailGO.GetComponent<ParticleSystem>().Stop();
		}
		Object.Destroy(base.go);
		CancelInvoke();
	}
}
