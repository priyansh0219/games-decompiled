using UnityEngine;

public class ExosuitTorpedoArm : MonoBehaviour, IExosuitArm
{
	private const float cooldownTime = 5f;

	private const float cooldownInterval = 1f;

	[AssertNotNull]
	public Transform siloFirst;

	[AssertNotNull]
	public Transform siloSecond;

	[AssertNotNull]
	public GameObject visualTorpedoFirst;

	[AssertNotNull]
	public GameObject visualTorpedoSecond;

	[AssertNotNull]
	public GameObject visualTorpedoReload;

	public Animator animator;

	public FMODAsset fireSound;

	public FMODAsset torpedoDisarmed;

	private ItemsContainer container;

	private float timeFirstShot = float.NegativeInfinity;

	private float timeSecondShot = float.NegativeInfinity;

	private Exosuit exosuit;

	[AssertLocalization]
	private const string noAmmoMessage = "VehicleTorpedoNoAmmo";

	[AssertLocalization]
	private const string exosuitTorpedoStorageHandText = "ExosuitTorpedoStorage";

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
		if (container != null)
		{
			container.onAddItem -= OnAddItem;
			container.onRemoveItem -= OnRemoveItem;
		}
		if (arm == Exosuit.Arm.Right)
		{
			base.transform.localScale = new Vector3(-1f, 1f, 1f);
			container = exosuit.GetStorageInSlot(exosuit.GetSlotIndex("ExosuitArmRight"), TechType.ExosuitTorpedoArmModule);
		}
		else
		{
			base.transform.localScale = new Vector3(1f, 1f, 1f);
			container = exosuit.GetStorageInSlot(exosuit.GetSlotIndex("ExosuitArmLeft"), TechType.ExosuitTorpedoArmModule);
		}
		if (container != null)
		{
			container.onAddItem += OnAddItem;
			container.onRemoveItem += OnRemoveItem;
		}
		UpdateVisuals();
	}

	bool IExosuitArm.OnUseDown(out float cooldownDuration)
	{
		return TryShoot(out cooldownDuration, verbose: true);
	}

	bool IExosuitArm.OnUseHeld(out float cooldownDuration)
	{
		return TryShoot(out cooldownDuration, verbose: false);
	}

	bool IExosuitArm.OnUseUp(out float cooldownDuration)
	{
		animator.SetBool("use_tool", value: false);
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
		animator.SetBool("use_tool", value: false);
	}

	private bool TryShoot(out float cooldownDuration, bool verbose)
	{
		TorpedoType[] torpedoTypes = exosuit.torpedoTypes;
		TorpedoType torpedoType = null;
		for (int i = 0; i < torpedoTypes.Length; i++)
		{
			if (container.Contains(torpedoTypes[i].techType))
			{
				torpedoType = torpedoTypes[i];
				break;
			}
		}
		float num = Mathf.Clamp(Time.time - timeFirstShot, 0f, 5f);
		float num2 = Mathf.Clamp(Time.time - timeSecondShot, 0f, 5f);
		float b = 5f - num;
		float b2 = 5f - num2;
		if (Mathf.Min(num, num2) < 1f)
		{
			cooldownDuration = 0f;
			return false;
		}
		if (num >= 5f)
		{
			if (Shoot(torpedoType, siloFirst, verbose))
			{
				timeFirstShot = Time.time;
				cooldownDuration = Mathf.Max(1f, b2);
				return true;
			}
		}
		else
		{
			if (!(num2 >= 5f))
			{
				cooldownDuration = 0f;
				return false;
			}
			if (Shoot(torpedoType, siloSecond, verbose))
			{
				timeSecondShot = Time.time;
				cooldownDuration = Mathf.Max(1f, b);
				return true;
			}
		}
		animator.SetBool("use_tool", value: false);
		cooldownDuration = 0f;
		return false;
	}

	private bool Shoot(TorpedoType torpedoType, Transform siloTransform, bool verbose)
	{
		if (Vehicle.TorpedoShot(container, torpedoType, siloTransform))
		{
			Utils.PlayFMODAsset(fireSound, siloTransform);
			animator.SetBool("use_tool", value: true);
			if (container.count == 0)
			{
				Utils.PlayFMODAsset(torpedoDisarmed, base.transform, 1f);
			}
			return true;
		}
		if (verbose)
		{
			ErrorMessage.AddError(Language.main.Get("VehicleTorpedoNoAmmo"));
		}
		return false;
	}

	private void OnAddItem(InventoryItem item)
	{
		UpdateVisuals();
	}

	private void OnRemoveItem(InventoryItem item)
	{
		UpdateVisuals();
	}

	private void UpdateVisuals()
	{
		int num = 0;
		TorpedoType[] torpedoTypes = exosuit.torpedoTypes;
		for (int i = 0; i < torpedoTypes.Length; i++)
		{
			num += container.GetCount(torpedoTypes[i].techType);
		}
		visualTorpedoReload.SetActive(num >= 3);
		visualTorpedoSecond.SetActive(num >= 2);
		visualTorpedoFirst.SetActive(num >= 1);
	}

	public void OnHoverTorpedoStorage(HandTargetEventData eventData)
	{
		if (container != null)
		{
			HandReticle.main.SetText(HandReticle.TextType.Hand, "ExosuitTorpedoStorage", translate: true, GameInput.Button.LeftHand);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
			HandReticle.main.SetIcon(HandReticle.IconType.Hand);
		}
	}

	public void OnOpenTorpedoStorage(HandTargetEventData eventData)
	{
		OpenTorpedoStorageExternal(eventData.transform);
	}

	public void OpenTorpedoStorageExternal(Transform useTransform)
	{
		if (container != null)
		{
			Inventory.main.SetUsedStorage(container);
			Player.main.GetPDA().Open(PDATab.Inventory, useTransform);
		}
	}

	private void OnDestroy()
	{
		if (container != null)
		{
			container.onAddItem -= OnAddItem;
			container.onRemoveItem -= OnRemoveItem;
		}
	}
}
