using System;
using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
public class CreatureFriend : MonoBehaviour, IProtoEventListener, IOnTakeDamage
{
	private const int currentVersion = 0;

	[NonSerialized]
	[ProtoMember(1)]
	public int version;

	[NonSerialized]
	[ProtoMember(2)]
	public double timeFriendshipEnd;

	[NonSerialized]
	[ProtoMember(3)]
	public string currentFriendUID;

	public bool unfriendOnDamage;

	[NonSerialized]
	public readonly Event<GameObject> friendSetEvent = new Event<GameObject>();

	private GameObject _friend;

	private void Update()
	{
		RestoreFriendIfNeeded();
		if (_friend != null && DayNightCycle.main.timePassed > timeFriendshipEnd)
		{
			Unfriend();
		}
	}

	private void RestoreFriendIfNeeded()
	{
		if (!string.IsNullOrEmpty(currentFriendUID))
		{
			UniqueIdentifier uid;
			if (_friend != null)
			{
				currentFriendUID = null;
			}
			else if (UniqueIdentifier.TryGetIdentifier(currentFriendUID, out uid))
			{
				_friend = uid.gameObject;
				friendSetEvent.Trigger(_friend);
				currentFriendUID = null;
			}
		}
	}

	public GameObject GetFriend()
	{
		return _friend;
	}

	public void SetFriend(GameObject friend, float duration)
	{
		double num = DayNightCycle.main.timePassed + (double)duration;
		if (_friend != friend || num > timeFriendshipEnd)
		{
			timeFriendshipEnd = num;
		}
		_friend = friend;
		friendSetEvent.Trigger(_friend);
	}

	public void Unfriend()
	{
		SetFriend(null, 0f);
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
		if (_friend != null)
		{
			UniqueIdentifier component = _friend.GetComponent<UniqueIdentifier>();
			currentFriendUID = ((component != null) ? component.Id : null);
		}
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
	}

	public void OnTakeDamage(DamageInfo damageInfo)
	{
		if (unfriendOnDamage && _friend != null && damageInfo.dealer == _friend)
		{
			Unfriend();
		}
	}
}
