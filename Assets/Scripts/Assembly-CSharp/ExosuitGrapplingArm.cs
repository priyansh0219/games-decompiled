using UWE;
using UnityEngine;

public class ExosuitGrapplingArm : MonoBehaviour, IExosuitArm
{
	public Animator animator;

	public Transform front;

	public GameObject hookPrefab;

	public VFXGrapplingRope rope;

	public FMOD_CustomLoopingEmitter grapplingLoopSound;

	public FMODAsset shootSound;

	private const float maxDistance = 35f;

	private const float damage = 5f;

	private const DamageType damageType = DamageType.Collide;

	private GrapplingHook hook;

	private const float exosuitGrapplingAccel = 15f;

	private const float targetGrapplingAccel = 400f;

	private Vector3 grapplingStartPos = Vector3.zero;

	private Exosuit exosuit;

	GameObject IExosuitArm.GetGameObject()
	{
		return base.gameObject;
	}

	GameObject IExosuitArm.GetInteractableRoot(GameObject target)
	{
		return null;
	}

	void IExosuitArm.SetSide(Exosuit.Arm arm)
	{
		exosuit = GetComponentInParent<Exosuit>();
		if (arm == Exosuit.Arm.Right)
		{
			base.transform.localScale = new Vector3(-1f, 1f, 1f);
		}
		else
		{
			base.transform.localScale = new Vector3(1f, 1f, 1f);
		}
	}

	bool IExosuitArm.OnUseDown(out float cooldownDuration)
	{
		animator.SetBool("use_tool", value: true);
		if (!rope.isLaunching)
		{
			rope.LaunchHook(35f);
		}
		cooldownDuration = 2f;
		return true;
	}

	bool IExosuitArm.OnUseHeld(out float cooldownDuration)
	{
		cooldownDuration = 0f;
		return false;
	}

	bool IExosuitArm.OnUseUp(out float cooldownDuration)
	{
		animator.SetBool("use_tool", value: false);
		ResetHook();
		cooldownDuration = 0f;
		return true;
	}

	bool IExosuitArm.OnAltDown()
	{
		return false;
	}

	void IExosuitArm.Update(ref Quaternion aimDirection)
	{
	}

	public void SetArmEffects(Exosuit.Arm arm)
	{
	}

	void IExosuitArm.ResetArm()
	{
		animator.SetBool("use_tool", value: false);
		ResetHook();
	}

	private void Start()
	{
		GameObject gameObject = Object.Instantiate(hookPrefab);
		hook = gameObject.GetComponent<GrapplingHook>();
		hook.transform.parent = front;
		hook.transform.localPosition = Vector3.zero;
		hook.transform.localRotation = Quaternion.identity;
		hook.transform.localScale = new Vector3(1f, 1f, 1f);
		rope.attachPoint = hook.transform;
	}

	private void OnDestroy()
	{
		if ((bool)hook)
		{
			Object.Destroy(hook.gameObject);
		}
		if (rope != null)
		{
			Object.Destroy(rope.gameObject);
		}
	}

	private void ResetHook()
	{
		rope.Release();
		hook.Release();
		hook.SetFlying(isFlying: false);
		hook.transform.parent = front;
		hook.transform.localScale = new Vector3(1f, 1f, 1f);
	}

	public void OnHit()
	{
		hook.transform.parent = null;
		hook.transform.position = front.transform.position;
		hook.SetFlying(isFlying: true);
		GameObject closestObj = null;
		Vector3 position = default(Vector3);
		UWE.Utils.TraceFPSTargetPosition(exosuit.gameObject, 100f, ref closestObj, ref position, out var _, includeUseableTriggers: false);
		if (closestObj == null || closestObj == hook.gameObject)
		{
			position = MainCamera.camera.transform.position + MainCamera.camera.transform.forward * 25f;
		}
		Vector3 vector = Vector3.Normalize(position - hook.transform.position);
		hook.rb.velocity = vector * 25f;
		Utils.PlayFMODAsset(shootSound, front, 15f);
		grapplingStartPos = exosuit.transform.position;
	}

	public void FixedUpdate()
	{
		if (hook.attached)
		{
			grapplingLoopSound.Play();
			Vector3 value = hook.transform.position - front.position;
			Vector3 vector = Vector3.Normalize(value);
			if (value.magnitude > 1f)
			{
				if (!exosuit.IsUnderwater() && exosuit.transform.position.y + 0.2f >= grapplingStartPos.y)
				{
					vector.y = Mathf.Min(vector.y, 0f);
				}
				exosuit.GetComponent<Rigidbody>().AddForce(vector * 15f, ForceMode.Acceleration);
				hook.GetComponent<Rigidbody>().AddForce(-vector * 400f, ForceMode.Force);
			}
			rope.SetIsHooked();
		}
		else if ((bool)rope && rope.isHooked)
		{
			ResetHook();
			grapplingLoopSound.Play();
		}
		else if (hook.flying)
		{
			if ((hook.transform.position - front.position).magnitude > 35f)
			{
				ResetHook();
			}
			grapplingLoopSound.Play();
		}
		else
		{
			grapplingLoopSound.Stop();
		}
	}

	public bool GetIsGrappling()
	{
		if (hook != null)
		{
			return hook.attached;
		}
		return false;
	}
}
