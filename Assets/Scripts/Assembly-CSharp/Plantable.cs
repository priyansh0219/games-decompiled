using System;
using Gendarme;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class Plantable : MonoBehaviour, IProtoEventListener, IShouldSerialize
{
	public enum PlantSize
	{
		Small = 0,
		Large = 1
	}

	public bool aboveWater;

	public bool underwater;

	public bool isSeedling;

	public TechType plantTechType;

	public PlantSize size;

	public Eatable eatable;

	[AssertNotNull]
	public Pickupable pickupable;

	[AssertNotNull]
	public GameObject model;

	public Vector3 modelLocalPosition = Vector3.zero;

	public Vector3 modelEulerAngles = Vector3.zero;

	public Vector3 modelScale = Vector3.one;

	public Vector3 modelIndoorScale = Vector3.one;

	private const int defaultPlanterSlotId = -1;

	private const float defaultPlantAge = 0f;

	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public int planterSlotId = -1;

	[NonSerialized]
	[ProtoMember(3)]
	public float plantAge;

	public GrownPlant linkedGrownPlant;

	private GrowingPlant growingPlant;

	private Planter _currentPlanter;

	public Planter currentPlanter
	{
		get
		{
			return _currentPlanter;
		}
		set
		{
			_currentPlanter = value;
			if (_currentPlanter == null)
			{
				planterSlotId = -1;
			}
			else
			{
				planterSlotId = _currentPlanter.GetSlotID(this);
			}
		}
	}

	public GameObject Spawn(Transform parent, bool isIndoor)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(model);
		gameObject.transform.SetParent(parent, worldPositionStays: false);
		gameObject.transform.localScale = (isIndoor ? modelIndoorScale : modelScale);
		gameObject.transform.localPosition = modelLocalPosition;
		gameObject.transform.localRotation = Quaternion.Euler(modelEulerAngles);
		if (isIndoor)
		{
			UnityEngine.Object.Destroy(gameObject.GetComponent<Rigidbody>());
			UnityEngine.Object.Destroy(gameObject.GetComponent<WorldForces>());
		}
		growingPlant = gameObject.GetComponent<GrowingPlant>();
		if (growingPlant != null)
		{
			growingPlant.seed = this;
			if (isIndoor)
			{
				growingPlant.EnableIndoorState();
			}
			if (linkedGrownPlant != null)
			{
				growingPlant.enabled = false;
				growingPlant.SetProgress(1f);
				Transform transform = linkedGrownPlant.transform;
				growingPlant.SetScale(transform, 1f);
				growingPlant.SetPosition(transform);
			}
			else
			{
				growingPlant.SetProgress(plantAge);
			}
		}
		else
		{
			UnityEngine.Object.Destroy(gameObject.GetComponent<Pickupable>());
			TechType techType = CraftData.GetTechType(gameObject);
			gameObject.AddComponent<TechTag>().type = techType;
			UnityEngine.Object.Destroy(gameObject.GetComponent<UniqueIdentifier>());
			gameObject.AddComponent<GrownPlant>().seed = this;
		}
		return gameObject;
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
		if (planterSlotId >= 0 && growingPlant != null)
		{
			plantAge = growingPlant.GetProgress();
		}
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		pickupable.ResetTechTypeOverride();
	}

	public int GetSlotID()
	{
		return planterSlotId;
	}

	public void FreeSpot()
	{
		if (currentPlanter != null)
		{
			currentPlanter.RemoveItem(this);
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public bool ReplaceSeedByPlant(Plantable plant)
	{
		bool result = false;
		if (currentPlanter != null)
		{
			result = currentPlanter.ReplaceItem(this, plant);
		}
		UnityEngine.Object.Destroy(base.gameObject);
		return result;
	}

	public void SetMaxPlantHeight(float height)
	{
		if (growingPlant != null)
		{
			growingPlant.SetMaxHeight(height);
			if (linkedGrownPlant != null && growingPlant.GetProgress() < 1f)
			{
				UnityEngine.Object.Destroy(linkedGrownPlant.gameObject);
				growingPlant.enabled = true;
			}
		}
	}

	[SuppressMessage("Gendarme.Rules.Correctness", "AvoidFloatingPointEqualityRule")]
	public bool ShouldSerialize()
	{
		if (version == 1 && planterSlotId == -1)
		{
			return plantAge != 0f;
		}
		return true;
	}
}
