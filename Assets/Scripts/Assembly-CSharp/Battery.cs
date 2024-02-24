using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[DisallowMultipleComponent]
public class Battery : MonoBehaviour, IBattery, ISerializationCallbackReceiver, IProtoEventListener
{
	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int protoVersion = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public float _charge;

	[ProtoMember(3)]
	public float _capacity = 100f;

	public float charge
	{
		get
		{
			return _charge;
		}
		set
		{
			_charge = Mathf.Min(value, _capacity);
		}
	}

	public float capacity => _capacity;

	public string GetChargeValueText()
	{
		float arg = _charge / _capacity;
		return Language.main.GetFormat("BatteryCharge", arg, Mathf.RoundToInt(_charge), _capacity);
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		_charge = _capacity;
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
	}
}
