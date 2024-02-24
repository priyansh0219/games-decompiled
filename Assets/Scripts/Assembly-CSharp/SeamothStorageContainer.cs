using System;
using System.Collections;
using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
public class SeamothStorageContainer : MonoBehaviour, IProtoEventListenerAsync, IProtoTreeEventListener, ICraftTarget
{
	public string storageLabel = "VehicleStorageLabel";

	public int width = 4;

	public int height = 4;

	public TechType[] allowedTech = new TechType[0];

	[AssertNotNull]
	public ChildObjectIdentifier storageRoot;

	private const int currentVersion = 3;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 3;

	[NonSerialized]
	[Obsolete("Obsolete since v2")]
	[ProtoMember(2, OverwriteList = true)]
	public byte[] serializedStorage;

	public ItemsContainer container { get; private set; }

	private void Awake()
	{
		Init();
	}

	private void Init()
	{
		if (container == null)
		{
			container = new ItemsContainer(width, height, storageRoot.transform, storageLabel, null);
			container.SetAllowedTechTypes(allowedTech);
		}
	}

	public IEnumerator OnProtoDeserializeAsync(ProtobufSerializer serializer)
	{
		Init();
		container.Clear();
		if (serializedStorage != null)
		{
			yield return StorageHelper.RestoreItemsAsync(serializer, serializedStorage, container);
			serializedStorage = null;
		}
	}

	public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
	{
		if (version < 2)
		{
			StoreInformationIdentifier[] componentsInChildren = base.gameObject.GetComponentsInChildren<StoreInformationIdentifier>(includeInactive: true);
			foreach (StoreInformationIdentifier storeInformationIdentifier in componentsInChildren)
			{
				if ((bool)storeInformationIdentifier && storeInformationIdentifier.transform.parent == base.transform)
				{
					UnityEngine.Object.Destroy(storeInformationIdentifier.gameObject);
				}
			}
			version = 2;
		}
		else
		{
			StorageHelper.TransferItems(storageRoot.gameObject, container);
		}
		if (version < 3)
		{
			CoroutineHost.StartCoroutine(CleanUpDuplicatedStorage());
		}
	}

	private IEnumerator CleanUpDuplicatedStorage()
	{
		yield return StorageHelper.DestroyDuplicatedItems(base.gameObject);
		version = Mathf.Max(version, 3);
	}

	public void OnCraftEnd(TechType techType)
	{
		Init();
		if (techType == TechType.SeamothTorpedoModule || techType == TechType.ExosuitTorpedoArmModule)
		{
			StartCoroutine(OnCraftEndAsync());
		}
	}

	private IEnumerator OnCraftEndAsync()
	{
		TaskResult<GameObject> taskResult = new TaskResult<GameObject>();
		for (int i = 0; i < 2; i++)
		{
			yield return CraftData.InstantiateFromPrefabAsync(TechType.WhirlpoolTorpedo, taskResult);
			GameObject gameObject = taskResult.Get();
			if (!(gameObject != null))
			{
				continue;
			}
			Pickupable component = gameObject.GetComponent<Pickupable>();
			if (component != null)
			{
				component.Pickup(events: false);
				if (container.AddItem(component) == null)
				{
					UnityEngine.Object.Destroy(component.gameObject);
				}
			}
		}
	}
}
