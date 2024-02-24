using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class BaseUpgradeConsole : MonoBehaviour, IBaseModule
{
	private const int _currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int _protoVersion = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public Base.Face _moduleFace;

	[NonSerialized]
	[ProtoMember(3)]
	public float _constructed = 1f;

	[AssertNotNull]
	public CrafterLogic crafterLogic;

	public Base.Face moduleFace
	{
		get
		{
			return _moduleFace;
		}
		set
		{
			_moduleFace = value;
		}
	}

	public float constructed
	{
		get
		{
			return _constructed;
		}
		set
		{
			value = Mathf.Clamp01(value);
			if (_constructed != value)
			{
				_constructed = value;
				if (!(_constructed >= 1f) && _constructed <= 0f)
				{
					UnityEngine.Object.Destroy(base.gameObject);
				}
			}
		}
	}
}
