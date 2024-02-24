using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class EntitySlotData : IEntitySlot
{
	[Flags]
	public enum EntitySlotType
	{
		Small = 1,
		Medium = 2,
		Large = 4,
		Tall = 8,
		Creature = 0x10
	}

	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public BiomeType biomeType;

	[NonSerialized]
	[ProtoMember(3)]
	public EntitySlotType allowedTypes;

	[NonSerialized]
	[ProtoMember(4)]
	public float density = 1f;

	[NonSerialized]
	[ProtoMember(5)]
	public Vector3 localPosition;

	[NonSerialized]
	[ProtoMember(6)]
	public Quaternion localRotation;

	public static EntitySlotData Create(Transform virtualParent, EntitySlot source)
	{
		return new EntitySlotData
		{
			biomeType = source.biomeType,
			density = source.density,
			allowedTypes = ConvertAllowedTypes(source.allowedTypes),
			localPosition = virtualParent.InverseTransformPoint(source.transform.position),
			localRotation = Quaternion.Inverse(virtualParent.rotation) * source.transform.rotation
		};
	}

	public BiomeType GetBiomeType()
	{
		return biomeType;
	}

	public bool IsTypeAllowed(EntitySlot.Type slotType)
	{
		return IsTypeAllowed(allowedTypes, slotType);
	}

	public static bool IsTypeAllowed(EntitySlotType allowedTypes, EntitySlot.Type slotType)
	{
		switch (slotType)
		{
		case EntitySlot.Type.Small:
			return HasFlag(allowedTypes, EntitySlotType.Small);
		case EntitySlot.Type.Medium:
			return HasFlag(allowedTypes, EntitySlotType.Medium);
		case EntitySlot.Type.Large:
			return HasFlag(allowedTypes, EntitySlotType.Large);
		case EntitySlot.Type.Tall:
			return HasFlag(allowedTypes, EntitySlotType.Tall);
		case EntitySlot.Type.Creature:
			return HasFlag(allowedTypes, EntitySlotType.Creature);
		default:
			Debug.LogErrorFormat("Unexpected entity slot type {0} in EntitySlotData.IsTypeAllowed", slotType);
			return false;
		}
	}

	public float GetDensity()
	{
		return density;
	}

	public bool IsCreatureSlot()
	{
		return IsCreatureSlot(allowedTypes);
	}

	public static bool IsCreatureSlot(EntitySlotType allowedTypes)
	{
		return HasFlag(allowedTypes, EntitySlotType.Creature);
	}

	private static bool HasFlag(EntitySlotType flags, EntitySlotType flag)
	{
		return (flags & flag) != 0;
	}

	public static EntitySlotType ConvertAllowedTypes(IEnumerable<EntitySlot.Type> allowedTypes)
	{
		EntitySlotType entitySlotType = (EntitySlotType)0;
		foreach (EntitySlot.Type allowedType in allowedTypes)
		{
			switch (allowedType)
			{
			case EntitySlot.Type.Small:
				entitySlotType |= EntitySlotType.Small;
				break;
			case EntitySlot.Type.Medium:
				entitySlotType |= EntitySlotType.Medium;
				break;
			case EntitySlot.Type.Large:
				entitySlotType |= EntitySlotType.Large;
				break;
			case EntitySlot.Type.Tall:
				entitySlotType |= EntitySlotType.Tall;
				break;
			case EntitySlot.Type.Creature:
				entitySlotType |= EntitySlotType.Creature;
				break;
			default:
				Debug.LogErrorFormat("Unexpected entity slot type {0} in EntitySlotData.ConvertAllowedTypes", allowedType);
				break;
			}
		}
		return entitySlotType;
	}
}
