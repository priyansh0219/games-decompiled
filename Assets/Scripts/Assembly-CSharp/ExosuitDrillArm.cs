using UWE;
using UnityEngine;

public class ExosuitDrillArm : MonoBehaviour, IExosuitArm
{
	public Animator animator;

	public FMODAsset hitSound;

	public FMODAsset drillSound;

	public Transform front;

	public GameObject drillFXprefab;

	public Transform fxSpawnPoint;

	public VFXController fxControl;

	public VFXEventTypes vfxEventType;

	public FMOD_CustomLoopingEmitter loop;

	public FMOD_CustomLoopingEmitter loopHit;

	private const float attackDist = 5f;

	private const float damage = 4f;

	private const DamageType damageType = DamageType.Drill;

	private bool drilling;

	private bool drillAbleTarget;

	private VFXSurfaceTypes prevSurfaceType = VFXSurfaceTypes.fallback;

	private ParticleSystem drillFXinstance;

	private GameObject drillTarget;

	private Exosuit exosuit;

	private void Start()
	{
	}

	GameObject IExosuitArm.GetGameObject()
	{
		return base.gameObject;
	}

	GameObject IExosuitArm.GetInteractableRoot(GameObject target)
	{
		Drillable componentProfiled = target.GetComponentProfiled<Drillable>();
		if (componentProfiled != null)
		{
			return componentProfiled.gameObject;
		}
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
		drilling = true;
		loop.Play();
		cooldownDuration = 0f;
		drillTarget = null;
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
		drilling = false;
		StopEffects();
		cooldownDuration = 0f;
		return true;
	}

	bool IExosuitArm.OnAltDown()
	{
		return false;
	}

	void IExosuitArm.Update(ref Quaternion aimDirection)
	{
		if (drillTarget != null)
		{
			Quaternion quaternion = Quaternion.LookRotation(Vector3.Normalize(UWE.Utils.GetEncapsulatedAABB(drillTarget).center - MainCamera.camera.transform.position), Vector3.up);
			aimDirection = quaternion;
		}
	}

	void IExosuitArm.ResetArm()
	{
		animator.SetBool("use_tool", value: false);
		drilling = false;
		StopEffects();
	}

	public void OnHit()
	{
		if (!exosuit.CanPilot() || !exosuit.GetPilotingMode())
		{
			return;
		}
		Vector3 position = Vector3.zero;
		GameObject closestObj = null;
		drillTarget = null;
		UWE.Utils.TraceFPSTargetPosition(exosuit.gameObject, 5f, ref closestObj, ref position);
		if (closestObj == null)
		{
			InteractionVolumeUser component = Player.main.gameObject.GetComponent<InteractionVolumeUser>();
			if (component != null && component.GetMostRecent() != null)
			{
				closestObj = component.GetMostRecent().gameObject;
			}
		}
		if ((bool)closestObj && drilling)
		{
			Drillable drillable = closestObj.FindAncestor<Drillable>();
			loopHit.Play();
			if ((bool)drillable)
			{
				drillable.OnDrill(fxSpawnPoint.position, exosuit, out var hitObject);
				drillTarget = hitObject;
				if (fxControl.emitters[0].fxPS != null && !fxControl.emitters[0].fxPS.emission.enabled)
				{
					fxControl.Play(0);
				}
				return;
			}
			LiveMixin liveMixin = closestObj.FindAncestor<LiveMixin>();
			if ((bool)liveMixin)
			{
				liveMixin.IsAlive();
				liveMixin.TakeDamage(4f, position, DamageType.Drill);
				drillTarget = closestObj;
			}
			VFXSurface component2 = closestObj.GetComponent<VFXSurface>();
			if (drillFXinstance == null)
			{
				drillFXinstance = VFXSurfaceTypeManager.main.Play(component2, vfxEventType, fxSpawnPoint.position, fxSpawnPoint.rotation, fxSpawnPoint);
			}
			else if (component2 != null && prevSurfaceType != component2.surfaceType)
			{
				drillFXinstance.GetComponent<VFXLateTimeParticles>().Stop();
				Object.Destroy(drillFXinstance.gameObject, 1.6f);
				drillFXinstance = VFXSurfaceTypeManager.main.Play(component2, vfxEventType, fxSpawnPoint.position, fxSpawnPoint.rotation, fxSpawnPoint);
				prevSurfaceType = component2.surfaceType;
			}
			closestObj.SendMessage("BashHit", this, SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			StopEffects();
		}
	}

	private void StopEffects()
	{
		if (drillFXinstance != null)
		{
			drillFXinstance.GetComponent<VFXLateTimeParticles>().Stop();
			Object.Destroy(drillFXinstance.gameObject, 1.6f);
			drillFXinstance = null;
		}
		if (fxControl.emitters[0].fxPS != null && fxControl.emitters[0].fxPS.emission.enabled)
		{
			fxControl.Stop(0);
		}
		loop.Stop();
		loopHit.Stop();
	}
}
