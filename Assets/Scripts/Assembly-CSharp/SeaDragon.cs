using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
public class SeaDragon : Creature
{
	public Transform exosuitAttachPoint;

	public float exosuitDamagePerSecond = 15f;

	public float exosuitDamageInterval = 1f;

	public FMODAsset exosuitGrabSound;

	private Exosuit holdingExosuit;

	private float timeExosuitGrabbed;

	private float timeExosuitReleased;

	private bool exosuitReleased = true;

	private Quaternion exosuitInitialRotation;

	private Vector3 exosuitInitialPosition;

	private float timeLastBurningChunkDamage;

	private float cumulativeBurningChunkDamage;

	public bool IsHoldingExosuit()
	{
		return holdingExosuit != null;
	}

	public float GetTimeExoReleased()
	{
		return timeExosuitReleased;
	}

	public void GrabExosuit(Exosuit exosuit)
	{
		SafeAnimator.SetBool(exosuit.mainAnimator, "reaper_attack", value: true);
		if (exosuitAttachPoint.InverseTransformPoint(exosuit.transform.position).y < -3f)
		{
			SafeAnimator.SetBool(GetAnimator(), "exo_ground_attack", value: true);
		}
		else
		{
			SafeAnimator.SetBool(GetAnimator(), "exo_attack", value: true);
		}
		exosuit.cinematicMode = true;
		UWE.Utils.SetIsKinematicAndUpdateInterpolation(exosuit.GetComponent<Rigidbody>(), isKinematic: true);
		exosuit.collisionModel.SetActive(value: false);
		holdingExosuit = exosuit;
		Aggression.Value = 0f;
		timeExosuitGrabbed = Time.time;
		exosuitReleased = false;
		exosuitInitialRotation = exosuit.transform.rotation;
		exosuitInitialPosition = exosuit.transform.position;
		if (exosuitGrabSound != null)
		{
			Utils.PlayFMODAsset(exosuitGrabSound, base.transform);
		}
		InvokeRepeating("DamageExosuit", exosuitDamageInterval, exosuitDamageInterval);
		Invoke("StartThrowExosuit", 1f);
	}

	public bool GetCanGrabExosuit()
	{
		if (timeExosuitReleased + 10f < Time.time)
		{
			return !IsHoldingExosuit();
		}
		return false;
	}

	public void ReleaseExosuit()
	{
		SafeAnimator.SetBool(GetAnimator(), "exo_attack", value: false);
		SafeAnimator.SetBool(GetAnimator(), "exo_ground_attack", value: false);
		if (holdingExosuit != null)
		{
			SafeAnimator.SetBool(holdingExosuit.mainAnimator, "reaper_attack", value: false);
			holdingExosuit.cinematicMode = false;
			UWE.Utils.SetIsKinematicAndUpdateInterpolation(holdingExosuit.GetComponent<Rigidbody>(), isKinematic: false);
			holdingExosuit.collisionModel.SetActive(value: true);
			holdingExosuit = null;
		}
		CancelInvoke("DamageExosuit");
		exosuitReleased = true;
		timeExosuitReleased = Time.time;
	}

	private void StartThrowExosuit()
	{
		if (!(holdingExosuit == null))
		{
			SafeAnimator.SetBool(GetAnimator(), "exo_throw", value: true);
			Invoke("ThrowExosuit", 2f);
		}
	}

	private void ThrowExosuit()
	{
		SafeAnimator.SetBool(GetAnimator(), "exo_throw", value: false);
		Exosuit exosuit = holdingExosuit;
		ReleaseExosuit();
		if (exosuit != null)
		{
			base.gameObject.SendMessage("OnThrowExosuit", exosuit, SendMessageOptions.RequireReceiver);
		}
	}

	private void DamageExosuit()
	{
		if (holdingExosuit != null)
		{
			holdingExosuit.liveMixin.TakeDamage(exosuitDamagePerSecond * exosuitDamageInterval);
		}
	}

	public void Update()
	{
		if (holdingExosuit == null && !exosuitReleased)
		{
			ReleaseExosuit();
		}
	}

	private void LateUpdate()
	{
		if (holdingExosuit != null)
		{
			float num = Mathf.InverseLerp(0f, 0.25f, Time.time - timeExosuitGrabbed);
			if (num >= 1f)
			{
				holdingExosuit.transform.position = exosuitAttachPoint.position;
				holdingExosuit.transform.rotation = exosuitAttachPoint.transform.rotation;
			}
			else
			{
				holdingExosuit.transform.position = (exosuitAttachPoint.position - exosuitInitialPosition) * num + exosuitInitialPosition;
				holdingExosuit.transform.rotation = Quaternion.Lerp(exosuitInitialRotation, exosuitAttachPoint.transform.rotation, num);
			}
		}
	}

	public override void OnDisable()
	{
		base.OnDisable();
		if (!exosuitReleased)
		{
			ReleaseExosuit();
		}
	}

	public float GetBurningChunkDamage(float originalDamage)
	{
		if (Time.time > timeLastBurningChunkDamage + 5f)
		{
			cumulativeBurningChunkDamage = 0f;
		}
		cumulativeBurningChunkDamage += originalDamage;
		timeLastBurningChunkDamage = Time.time;
		if (cumulativeBurningChunkDamage > 500f)
		{
			return 0f;
		}
		return originalDamage;
	}
}
