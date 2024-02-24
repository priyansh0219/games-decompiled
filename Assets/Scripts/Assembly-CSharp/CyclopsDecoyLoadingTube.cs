using System;
using System.Collections.Generic;
using ProtoBuf;
using Story;
using UnityEngine;

[ProtoContract]
public class CyclopsDecoyLoadingTube : MonoBehaviour, IProtoEventListener, IProtoTreeEventListener
{
	private const int _currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version;

	[NonSerialized]
	[ProtoMember(2, OverwriteList = true)]
	public Dictionary<string, string> serializedDecoySlots;

	[AssertNotNull]
	public SubRoot subRoot;

	[AssertNotNull]
	public CyclopsDecoyManager decoyManager;

	[AssertNotNull]
	public GameObject storageRoot;

	public Equipment decoySlots { get; private set; }

	private void Awake()
	{
		Initialize();
	}

	private void Start()
	{
		RecalcDecoyTotals();
	}

	private void Initialize()
	{
		if (decoySlots == null)
		{
			decoySlots = new Equipment(base.gameObject, storageRoot.transform);
			decoySlots.SetLabel("DecoyTubeStorageLabel");
			decoySlots.onEquip += OnEquip;
			decoySlots.onUnequip += OnUnequip;
		}
	}

	private void UnlockDefaultModuleSlots()
	{
		string[] array = new string[5] { "DecoySlot1", "DecoySlot2", "DecoySlot3", "DecoySlot4", "DecoySlot5" };
		int num = 1;
		if (subRoot.decoyTubeSizeIncreaseUpgrade)
		{
			num = decoyManager.decoyMaxWithUpgrade;
		}
		else
		{
			for (int i = 1; i < array.Length; i++)
			{
				decoySlots.RemoveSlot(array[i]);
			}
		}
		for (int j = 0; j < num; j++)
		{
			decoySlots.AddSlot(array[j]);
		}
		decoyManager.decoyMax = num;
	}

	public void OnHover(HandTargetEventData eventData)
	{
		HandReticle main = HandReticle.main;
		main.SetText(HandReticle.TextType.Hand, "UseDecoyTube", translate: true, GameInput.Button.LeftHand);
		main.SetText(HandReticle.TextType.HandSubscript, "Tooltip_UseDecoyTube", translate: true);
		main.SetIcon(HandReticle.IconType.Hand);
	}

	public void OnUse(HandTargetEventData eventData)
	{
		PDA pDA = Player.main.GetPDA();
		Inventory.main.SetUsedStorage(decoySlots);
		pDA.Open(PDATab.Inventory);
	}

	public void OnEquip(string slot, InventoryItem item)
	{
		RecalcDecoyTotals();
	}

	public void OnUnequip(string slot, InventoryItem item)
	{
		RecalcDecoyTotals();
	}

	private void RecalcDecoyTotals()
	{
		int num = 0;
		int decoyMax = decoyManager.decoyMax;
		for (int i = 1; i <= decoyMax; i++)
		{
			string slot = $"DecoySlot{i}";
			if (decoySlots.GetTechTypeInSlot(slot) != 0)
			{
				num++;
			}
		}
		subRoot.BroadcastMessage("UpdateTotalDecoys", num, SendMessageOptions.DontRequireReceiver);
	}

	public void UpdateAbilities()
	{
		UnlockDefaultModuleSlots();
		RecalcDecoyTotals();
	}

	public void TryRemoveDecoyFromTube()
	{
		int num = 0;
		string slot = "";
		for (int i = 1; i <= 5; i++)
		{
			string text = "DecoySlot" + i;
			if (decoySlots.GetTechTypeInSlot(text) != 0)
			{
				slot = text;
				num++;
			}
		}
		if (num > 0)
		{
			num--;
			decoySlots.RemoveItem(slot, forced: true, verbose: true);
			subRoot.BroadcastMessage("UpdateTotalDecoys", num, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
		version = 1;
		serializedDecoySlots = decoySlots.SaveEquipment();
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		Initialize();
		if (serializedDecoySlots == null)
		{
			UnlockDefaultModuleSlots();
		}
	}

	public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
	{
		ItemGoalTracker.OnConstruct(TechType.Cyclops);
		if (serializedDecoySlots != null)
		{
			StorageHelper.TransferEquipment(storageRoot, serializedDecoySlots, decoySlots);
			serializedDecoySlots = null;
		}
	}
}
