using System.Collections;
using UWE;
using UnityEngine;

public class ExosuitClawArm : MonoBehaviour, IExosuitArm
{
	public const float kGrabDistance = 6f;

	public Animator animator;

	public FMODAsset hitTerrainSound;

	public FMODAsset hitFishSound;

	public FMODAsset pickupSound;

	public Transform front;

	public VFXEventTypes vfxEventType;

	public VFXController fxControl;

	public float cooldownPunch = 1f;

	public float cooldownPickup = 1.533f;

	private const float attackDist = 6.5f;

	private const float damage = 50f;

	private const DamageType damageType = DamageType.Normal;

	[AssertLocalization]
	private const string noRoomNotification = "ContainerCantFit";

	private float timeUsed = float.NegativeInfinity;

	private float cooldownTime;

	private bool shownNoRoomNotification;

	private Exosuit exosuit;

	GameObject IExosuitArm.GetGameObject()
	{
		return base.gameObject;
	}

	GameObject IExosuitArm.GetInteractableRoot(GameObject target)
	{
		Pickupable componentInParent = target.GetComponentInParent<Pickupable>();
		if (componentInParent != null && componentInParent.isPickupable)
		{
			return componentInParent.gameObject;
		}
		PickPrefab componentProfiled = target.GetComponentProfiled<PickPrefab>();
		if (componentProfiled != null)
		{
			return componentProfiled.gameObject;
		}
		BreakableResource componentInParent2 = target.GetComponentInParent<BreakableResource>();
		if (componentInParent2 != null)
		{
			return componentInParent2.gameObject;
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
		return TryUse(out cooldownDuration);
	}

	bool IExosuitArm.OnUseHeld(out float cooldownDuration)
	{
		return TryUse(out cooldownDuration);
	}

	bool IExosuitArm.OnUseUp(out float cooldownDuration)
	{
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

	void IExosuitArm.ResetArm()
	{
	}

	private bool TryUse(out float cooldownDuration)
	{
		if (Time.time - timeUsed >= cooldownTime)
		{
			Pickupable pickupable = null;
			PickPrefab pickPrefab = null;
			if ((bool)exosuit.GetActiveTarget())
			{
				pickupable = exosuit.GetActiveTarget().GetComponent<Pickupable>();
				pickPrefab = exosuit.GetActiveTarget().GetComponent<PickPrefab>();
			}
			if (!(pickupable != null) || !pickupable.isPickupable)
			{
				if (pickPrefab != null)
				{
					animator.SetTrigger("use_tool");
					cooldownTime = (cooldownDuration = cooldownPickup);
					return true;
				}
				animator.SetTrigger("bash");
				cooldownTime = (cooldownDuration = cooldownPunch);
				fxControl.Play(0);
				return true;
			}
			if (exosuit.storageContainer.container.HasRoomFor(pickupable))
			{
				animator.SetTrigger("use_tool");
				cooldownTime = (cooldownDuration = cooldownPickup);
				shownNoRoomNotification = false;
				return true;
			}
			if (!shownNoRoomNotification)
			{
				ErrorMessage.AddMessage(Language.main.Get("ContainerCantFit"));
				shownNoRoomNotification = true;
			}
		}
		cooldownDuration = 0f;
		return false;
	}

	public void OnPickup()
	{
		Exosuit componentInParent = GetComponentInParent<Exosuit>();
		if ((bool)componentInParent.GetActiveTarget())
		{
			Pickupable component = componentInParent.GetActiveTarget().GetComponent<Pickupable>();
			PickPrefab component2 = componentInParent.GetActiveTarget().GetComponent<PickPrefab>();
			StartCoroutine(OnPickupAsync(component, component2, componentInParent));
		}
	}

	private IEnumerator OnPickupAsync(Pickupable pickupAble, PickPrefab pickPrefab, Exosuit exo)
	{
		ItemsContainer container = exo.storageContainer.container;
		if (pickupAble != null && pickupAble.isPickupable && container.HasRoomFor(pickupAble))
		{
			pickupAble.Initialize();
			InventoryItem item = new InventoryItem(pickupAble);
			container.UnsafeAdd(item);
			Utils.PlayFMODAsset(pickupSound, front, 5f);
		}
		else if (pickPrefab != null)
		{
			TaskResult<bool> result = new TaskResult<bool>();
			yield return pickPrefab.AddToContainerAsync(container, result);
			bool flag = result.Get();
			if (pickPrefab != null && flag)
			{
				pickPrefab.SetPickedUp();
			}
		}
	}

	public void OnHit()
	{
		Exosuit componentInParent = GetComponentInParent<Exosuit>();
		if (!componentInParent.CanPilot() || !componentInParent.GetPilotingMode())
		{
			return;
		}
		Vector3 position = default(Vector3);
		GameObject closestObj = null;
		UWE.Utils.TraceFPSTargetPosition(componentInParent.gameObject, 6.5f, ref closestObj, ref position, out var _);
		if (closestObj == null)
		{
			InteractionVolumeUser component = Player.main.gameObject.GetComponent<InteractionVolumeUser>();
			if (component != null && component.GetMostRecent() != null)
			{
				closestObj = component.GetMostRecent().gameObject;
			}
		}
		if ((bool)closestObj)
		{
			LiveMixin liveMixin = closestObj.FindAncestor<LiveMixin>();
			if ((bool)liveMixin)
			{
				liveMixin.TakeDamage(50f, position);
			}
			if ((bool)closestObj.FindAncestor<Creature>())
			{
				Utils.PlayFMODAsset(hitFishSound, front, 50f);
			}
			else
			{
				Utils.PlayFMODAsset(hitTerrainSound, front, 50f);
			}
			VFXSurface component2 = closestObj.GetComponent<VFXSurface>();
			Vector3 euler = MainCameraControl.main.transform.eulerAngles + new Vector3(300f, 90f, 0f);
			VFXSurfaceTypeManager.main.Play(component2, vfxEventType, position, Quaternion.Euler(euler), componentInParent.gameObject.transform);
			closestObj.SendMessage("BashHit", this, SendMessageOptions.DontRequireReceiver);
		}
	}
}
