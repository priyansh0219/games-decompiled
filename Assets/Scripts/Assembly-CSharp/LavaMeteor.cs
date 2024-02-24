using System.Collections.Generic;
using UWE;
using UnityEngine;

public class LavaMeteor : MonoBehaviour, IPropulsionCannonAmmo
{
	public float explodeRadius = 4f;

	public float damage = 10f;

	public float maxLiveTime = 5f;

	public FMODAsset detonateSound;

	[AssertNotNull]
	public Rigidbody rb;

	[AssertNotNull]
	public Transform fxSpawnPoint;

	[AssertNotNull]
	public VFXController fxControl;

	private float detonationTime;

	private bool grabbedByPropCannon;

	private bool wasShot;

	void IPropulsionCannonAmmo.OnGrab()
	{
		grabbedByPropCannon = true;
	}

	void IPropulsionCannonAmmo.OnShoot()
	{
		wasShot = true;
		grabbedByPropCannon = false;
		SetDetonationTime();
	}

	void IPropulsionCannonAmmo.OnImpact()
	{
	}

	void IPropulsionCannonAmmo.OnRelease()
	{
		if (!wasShot)
		{
			SetDetonationTime();
		}
		grabbedByPropCannon = false;
	}

	bool IPropulsionCannonAmmo.GetAllowedToGrab()
	{
		return true;
	}

	bool IPropulsionCannonAmmo.GetAllowedToShoot()
	{
		return true;
	}

	private void Start()
	{
		SetDetonationTime();
		fxControl.Play(0);
	}

	private void FixedUpdate()
	{
		Vector3 forward = Vector3.Lerp(Vector3.up, Vector3.Normalize(rb.velocity), Mathf.Clamp01(rb.velocity.magnitude * 0.1f));
		fxSpawnPoint.forward = forward;
	}

	private void Update()
	{
		if (!grabbedByPropCannon && Time.time >= detonationTime)
		{
			Detonate();
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!grabbedByPropCannon)
		{
			Detonate();
		}
	}

	private void Detonate()
	{
		fxControl.Play(1);
		if (detonateSound != null)
		{
			Utils.PlayFMODAsset(detonateSound, base.transform);
		}
		HashSet<LiveMixin> hashSet = new HashSet<LiveMixin>();
		int num = UWE.Utils.OverlapSphereIntoSharedBuffer(base.transform.position, explodeRadius);
		for (int i = 0; i < num; i++)
		{
			Collider collider = UWE.Utils.sharedColliderBuffer[i];
			if (collider.attachedRigidbody != null)
			{
				LiveMixin component = collider.attachedRigidbody.GetComponent<LiveMixin>();
				if (component != null && !hashSet.Contains(component))
				{
					component.TakeDamage(damage, base.transform.position, DamageType.Heat);
					hashSet.Add(component);
				}
			}
		}
		fxControl.emitters[0].instanceGO.transform.parent = null;
		fxControl.StopAndDestroy(0, 4f);
		Object.Destroy(base.gameObject);
	}

	private void SetDetonationTime()
	{
		detonationTime = Time.time + maxLiveTime;
	}
}
