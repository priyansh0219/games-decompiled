using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class GrownPlant : HandTarget, IHandTarget, IProtoEventListener
{
	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public string seedUID = "";

	private Plantable _seed;

	public Plantable seed
	{
		get
		{
			return _seed;
		}
		set
		{
			if (_seed != null)
			{
				_seed.linkedGrownPlant = null;
			}
			_seed = value;
			if (_seed != null)
			{
				_seed.linkedGrownPlant = this;
			}
		}
	}

	public override void Awake()
	{
		base.Awake();
		PickPrefab component = GetComponent<PickPrefab>();
		if (component != null && component.destroyOnPicked)
		{
			component.pickedEvent.AddHandler(this, OnPicked);
		}
		UnityEngine.Object.Destroy(base.gameObject.GetComponent<Rigidbody>());
		UnityEngine.Object.Destroy(base.gameObject.GetComponent<WorldForces>());
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
		if (seed != null)
		{
			seedUID = seed.gameObject.GetComponent<UniqueIdentifier>().Id;
		}
		else
		{
			seedUID = "";
		}
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		if (seed == null)
		{
			base.gameObject.SetActive(value: false);
		}
		if (string.IsNullOrEmpty(seedUID))
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void FindSeed()
	{
		if (seed != null)
		{
			return;
		}
		if (!UniqueIdentifier.TryGetIdentifier(seedUID, out var uid))
		{
			Debug.LogWarningFormat("Cannot find seedling game object linked to {0}", base.gameObject);
			return;
		}
		Plantable component = uid.GetComponent<Plantable>();
		if (component == null)
		{
			Debug.LogWarningFormat("Grown plant {0} is linked to a seed {1} which is missing Plantable component", base.gameObject, uid.name);
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else if (component.linkedGrownPlant != null && component.linkedGrownPlant != this)
		{
			Debug.LogWarningFormat("Two grown plants game objects refers to the same seedling object. {0}", base.gameObject);
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			seed = component;
			base.gameObject.SetActive(value: true);
		}
	}

	private void OnPicked(PickPrefab pickPrefab)
	{
		if ((bool)seed)
		{
			seed.FreeSpot();
		}
	}

	private void OnKill()
	{
		if ((bool)seed)
		{
			seed.FreeSpot();
		}
	}

	public void OnHandHover(GUIHand hand)
	{
		if (seed != null && !seed.isSeedling && seed.pickupable != null && seed.currentPlanter != null)
		{
			TechType techType = seed.pickupable.GetTechType();
			if (Inventory.Get().HasRoomFor(seed.pickupable))
			{
				string pickupText = LanguageCache.GetPickupText(techType);
				HandReticle.main.SetText(HandReticle.TextType.Hand, pickupText, translate: false, GameInput.Button.LeftHand);
				HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
				HandReticle.main.SetIcon(HandReticle.IconType.Hand);
			}
			else
			{
				HandReticle.main.SetText(HandReticle.TextType.Hand, techType.AsString(), translate: true);
				HandReticle.main.SetText(HandReticle.TextType.HandSubscript, "InventoryFull", translate: true);
			}
		}
	}

	public void OnHandClick(GUIHand hand)
	{
		if (seed != null && !seed.isSeedling && seed.pickupable != null && Inventory.Get().HasRoomFor(seed.pickupable) && seed.currentPlanter != null)
		{
			seed.currentPlanter.RemoveItem(seed);
			Inventory.Get().Pickup(seed.pickupable);
			hand.player.PlayGrab();
		}
	}
}
