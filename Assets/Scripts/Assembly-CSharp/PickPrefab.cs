using System;
using System.Collections;
using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
public class PickPrefab : HandTarget, IProtoEventListener, IHandTarget, ICompileTimeCheckable
{
	public TechType pickTech;

	public bool destroyOnPicked;

	private bool isAddingToInventory;

	[NonSerialized]
	[ProtoMember(1)]
	public bool pickedState;

	[NonSerialized]
	public readonly Event<PickPrefab> pickedEvent = new Event<PickPrefab>();

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		SetPickedState(pickedState);
	}

	public void Start()
	{
	}

	private bool AllowedToPickUp()
	{
		Vector2int itemSize = TechData.GetItemSize(pickTech);
		return Player.main.HasInventoryRoom(itemSize.x, itemSize.y);
	}

	public void OnHandHover(GUIHand hand)
	{
		if (base.gameObject.activeInHierarchy)
		{
			if (AllowedToPickUp())
			{
				string pickupText = LanguageCache.GetPickupText(pickTech);
				HandReticle.main.SetText(HandReticle.TextType.Hand, pickupText, translate: false, GameInput.Button.LeftHand);
				HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
				HandReticle.main.SetIcon(HandReticle.IconType.Hand);
			}
			else
			{
				HandReticle.main.SetText(HandReticle.TextType.Hand, pickTech.AsString(), translate: true);
				HandReticle.main.SetText(HandReticle.TextType.HandSubscript, "InventoryFull", translate: true);
			}
		}
	}

	public void OnHandClick(GUIHand hand)
	{
		if (base.gameObject.activeInHierarchy && AllowedToPickUp() && !isAddingToInventory)
		{
			isAddingToInventory = true;
			StartCoroutine(AddToInventoryRoutine());
		}
	}

	public void SetPickedUp()
	{
		pickedEvent.Trigger(this);
		SetPickedState(newPickedState: true);
	}

	public IEnumerator AddToContainerAsync(ItemsContainer container, TaskResult<bool> result)
	{
		TaskResult<GameObject> prefabResult = new TaskResult<GameObject>();
		yield return CraftData.InstantiateFromPrefabAsync(pickTech, prefabResult);
		GameObject gameObject = prefabResult.Get();
		result.Set(value: true);
		if ((bool)gameObject)
		{
			Pickupable component = gameObject.GetComponent<Pickupable>();
			if (!component)
			{
				UnityEngine.Object.Destroy(gameObject);
				result.Set(value: false);
				yield break;
			}
			if (!container.HasRoomFor(component))
			{
				UnityEngine.Object.Destroy(gameObject);
				result.Set(value: false);
				yield break;
			}
			component.Initialize();
			InventoryItem item = new InventoryItem(component);
			container.UnsafeAdd(item);
			result.Set(value: true);
		}
	}

	public void SetPickedState(bool newPickedState)
	{
		pickedState = newPickedState;
		base.gameObject.SetActive(!newPickedState);
		if (newPickedState && destroyOnPicked)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public bool GetPickedState()
	{
		return pickedState;
	}

	public string CompileTimeCheck()
	{
		if (pickTech != 0)
		{
			return null;
		}
		return "Tech type not set";
	}

	private IEnumerator AddToInventoryRoutine()
	{
		TaskResult<GameObject> result = new TaskResult<GameObject>();
		yield return CraftData.AddToInventoryAsync(pickTech, result, 1, noMessage: false, spawnIfCantAdd: false);
		if ((bool)result.Get())
		{
			SetPickedUp();
		}
		isAddingToInventory = false;
	}
}
