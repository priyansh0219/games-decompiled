using System;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
public class TimeCapsule : MonoBehaviour, IHandTarget
{
	[AssertNotNull]
	public GameObject content;

	[AssertNotNull]
	public GameObject inspectPrefab;

	public FMODAsset useSound;

	public string animParam;

	public string viewAnimParam;

	public float viewAnimDuration = 2f;

	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int protoVersion = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public bool spawned;

	[NonSerialized]
	[ProtoMember(3)]
	public string instanceId;

	[NonSerialized]
	[ProtoMember(4)]
	public string id;

	private bool visible = true;

	private GameObject inspectObject;

	[AssertLocalization]
	private const string timeCapsuleOpenHandText = "TimeCapsuleOpen";

	[AssertLocalization]
	private const string timeCapsuleInvalidHandText = "TimeCapsuleInvalid";

	private void Start()
	{
		if (spawned)
		{
			if (!TimeCapsuleContentProvider.GetIsActive(id))
			{
				spawned = false;
				instanceId = null;
				id = null;
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
		else
		{
			PlayerTimeCapsule.main.RegisterSpawn(this);
		}
		UpdateVisibility();
	}

	private void OnDestroy()
	{
		PlayerTimeCapsule.main.UnregisterSpawn(this);
		if (inspectObject != null)
		{
			UnityEngine.Object.Destroy(inspectObject);
			Inventory.main.quickSlots.SetIgnoreHotkeyInput(ignore: false);
			Player.main.GetPDA().SetIgnorePDAInput(ignore: false);
		}
	}

	public void Spawn(string instanceId, string id)
	{
		this.instanceId = instanceId;
		this.id = id;
		spawned = true;
		UpdateVisibility();
	}

	public void DoNotSpawn()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void Collect()
	{
		try
		{
			PDAEncyclopedia.AddTimeCapsule(id, verbose: true);
			PlayerTimeCapsule.main.RegisterOpen(instanceId);
			List<TimeCapsuleItem> items = TimeCapsuleContentProvider.GetItems(id);
			if (items != null)
			{
				CoroutineHost.StartCoroutine(PickupItemsAsync(items));
			}
		}
		finally
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private IEnumerator PickupItemsAsync(List<TimeCapsuleItem> items)
	{
		TaskResult<Pickupable> result = new TaskResult<Pickupable>();
		for (int i = 0; i < items.Count; i++)
		{
			TimeCapsuleItem timeCapsuleItem = items[i];
			result.Set(null);
			yield return timeCapsuleItem.SpawnAsync(result);
			Pickupable pickupable = result.Get();
			if (pickupable != null)
			{
				Inventory.main.ForcePickup(pickupable);
			}
		}
	}

	private void UpdateVisibility()
	{
		content.SetActive(spawned && visible);
	}

	private IEnumerator Open()
	{
		bool hasViewAnim = !string.IsNullOrEmpty(viewAnimParam);
		float seconds = 0f;
		if (hasViewAnim)
		{
			seconds = Player.main.armsController.StartHolsterTime(viewAnimDuration);
		}
		yield return new WaitForSeconds(seconds);
		if (hasViewAnim)
		{
			ArmsController armsController = Player.main.armsController;
			armsController.TriggerAnimParam(viewAnimParam, viewAnimDuration);
			if (inspectPrefab != null)
			{
				inspectObject = UnityEngine.Object.Instantiate(inspectPrefab);
				Transform transform = inspectObject.transform;
				transform.SetParent(armsController.leftHandAttach, worldPositionStays: false);
				transform.localPosition = Vector3.zero;
				transform.localRotation = Quaternion.identity;
				if (!string.IsNullOrEmpty(animParam))
				{
					Animator component = inspectObject.GetComponent<Animator>();
					if (component != null)
					{
						component.SetTrigger(animParam);
					}
				}
				if (useSound != null)
				{
					Utils.PlayFMODAsset(useSound, transform);
				}
			}
			yield return new WaitForSeconds(viewAnimDuration);
			if (inspectObject != null)
			{
				UnityEngine.Object.Destroy(inspectObject);
			}
		}
		Collect();
	}

	public void OnHandHover(GUIHand hand)
	{
		PlatformServices services = PlatformUtils.main.GetServices();
		if (services == null || services.CanAccessUGC())
		{
			if (spawned)
			{
				HandReticle.main.SetIcon(HandReticle.IconType.Hand);
				HandReticle.main.SetText(HandReticle.TextType.Hand, "TimeCapsuleOpen", translate: true, GameInput.Button.LeftHand);
				HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
			}
			else
			{
				HandReticle.main.SetText(HandReticle.TextType.Hand, "TimeCapsuleInvalid", translate: true, GameInput.Button.LeftHand);
				HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
			}
		}
	}

	public void OnHandClick(GUIHand hand)
	{
		PlatformServices services = PlatformUtils.main.GetServices();
		if ((services == null || services.CanAccessUGC()) && spawned)
		{
			visible = false;
			UpdateVisibility();
			StartCoroutine(Open());
		}
	}
}
