using System.Collections.Generic;
using UWE;
using UnityEngine;

public class RepulsionCannon : PlayerTool, IEquippable
{
	private const float shootForce = 70f;

	private const float massScalingFactor = 0.005f;

	private const float maxDistance = 35f;

	private const float maxMass = 1300f;

	private const float maxAABBVolume = 400f;

	private const float energyPerShot = 4f;

	private const float sphereTraceRadius = 1f;

	[AssertNotNull]
	public Animator animator;

	[AssertNotNull]
	public VFXController fxControl;

	[AssertNotNull]
	public GameObject bubblesFX;

	[AssertNotNull]
	public Transform muzzle;

	[AssertNotNull]
	public FMODAsset shootSound;

	private bool callBubblesFX;

	private List<IPropulsionCannonAmmo> iammo = new List<IPropulsionCannonAmmo>();

	private bool firstUse;

	private void Update()
	{
		if (callBubblesFX)
		{
			Utils.PlayOneShotPS(bubblesFX, muzzle.position, muzzle.rotation);
			callBubblesFX = false;
		}
	}

	public override void OnDraw(Player p)
	{
		TechType techType = pickupable.GetTechType();
		firstUse = !p.IsToolUsed(techType) || PlayerToolConsoleCommands.debugFirstUse;
		base.OnDraw(p);
	}

	public override void OnToolBleederHitAnim(GUIHand guiHand)
	{
		if (usingPlayer != null)
		{
			Bleeder bleeder = usingPlayer.GetComponentInChildren<BleederAttachTarget>().bleeder;
			if (bleeder != null)
			{
				bleeder.attachAndSuck.SetDetached();
				ShootObject(bleeder.GetComponent<Rigidbody>(), -MainCamera.camera.transform.right * 6f);
				energyMixin.ConsumeEnergy(4f);
				fxControl.Play();
				callBubblesFX = true;
				Utils.PlayFMODAsset(shootSound, base.transform);
			}
		}
	}

	public override FMODAsset GetBleederHitSound(FMODAsset defaultSound)
	{
		return null;
	}

	private void ShootObject(Rigidbody rb, Vector3 velocity)
	{
		UWE.Utils.SetIsKinematicAndUpdateInterpolation(rb, isKinematic: false);
		rb.velocity = velocity;
		PropulseCannonAmmoHandler propulseCannonAmmoHandler = rb.gameObject.GetComponent<PropulseCannonAmmoHandler>();
		if (propulseCannonAmmoHandler == null)
		{
			propulseCannonAmmoHandler = rb.gameObject.AddComponent<PropulseCannonAmmoHandler>();
		}
		propulseCannonAmmoHandler.ResetHandler();
		propulseCannonAmmoHandler.OnShot();
	}

	public override void OnToolUseAnim(GUIHand guiHand)
	{
		base.OnToolUseAnim(guiHand);
		if (!(energyMixin.charge > 0f))
		{
			return;
		}
		float num = Mathf.Clamp01(energyMixin.charge / 4f);
		Vector3 forward = MainCamera.camera.transform.forward;
		Vector3 position = MainCamera.camera.transform.position;
		int num2 = UWE.Utils.SpherecastIntoSharedBuffer(position, 1f, forward, 35f, ~(1 << LayerMask.NameToLayer("Player")));
		float num3 = 0f;
		for (int i = 0; i < num2; i++)
		{
			RaycastHit raycastHit = UWE.Utils.sharedHitBuffer[i];
			Vector3 point = raycastHit.point;
			float magnitude = (position - point).magnitude;
			float num4 = 1f - Mathf.Clamp01((magnitude - 1f) / 35f);
			GameObject entityRoot = UWE.Utils.GetEntityRoot(raycastHit.collider.gameObject);
			if (entityRoot == null)
			{
				entityRoot = raycastHit.collider.gameObject;
			}
			Rigidbody component = entityRoot.GetComponent<Rigidbody>();
			if (!(component != null))
			{
				continue;
			}
			num3 += component.mass;
			bool flag = true;
			entityRoot.GetComponents(iammo);
			for (int j = 0; j < iammo.Count; j++)
			{
				if (!iammo[j].GetAllowedToShoot())
				{
					flag = false;
					break;
				}
			}
			iammo.Clear();
			if (flag && !(raycastHit.collider is MeshCollider) && (entityRoot.GetComponent<Pickupable>() != null || entityRoot.GetComponent<Living>() != null || (component.mass <= 1300f && UWE.Utils.GetAABBVolume(entityRoot) <= 400f)))
			{
				float num5 = 1f + component.mass * 0.005f;
				Vector3 velocity = forward * num4 * num * 70f / num5;
				ShootObject(component, velocity);
			}
		}
		energyMixin.ConsumeEnergy(4f);
		fxControl.Play();
		callBubblesFX = true;
		Utils.PlayFMODAsset(shootSound, base.transform);
		float num6 = Mathf.Clamp(num3 / 100f, 0f, 15f);
		Player.main.GetComponent<Rigidbody>().AddForce(-forward * num6, ForceMode.VelocityChange);
	}

	public void OnEquip(GameObject sender, string slot)
	{
		if (base.isDrawn && firstUse)
		{
			animator.SetBool("using_tool_first", value: true);
		}
	}

	public void OnUnequip(GameObject sender, string slot)
	{
		animator.SetBool("using_tool_first", value: false);
	}

	public void UpdateEquipped(GameObject sender, string slot)
	{
	}
}
