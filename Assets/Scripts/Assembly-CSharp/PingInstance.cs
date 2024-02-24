using System;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Serialization;

[ProtoContract]
public class PingInstance : MonoBehaviour, INotificationTarget
{
	public PingType pingType;

	[AssertNotNull]
	public Transform origin;

	[Tooltip("Don't show in Ping Manager PDA Tab if set to false.")]
	public bool displayPingInManager = true;

	public float minDist = 15f;

	[FormerlySerializedAs("maxDist")]
	public float range = 10f;

	[Space]
	[Tooltip("Add 'not visited' notification for PDA (only to be used as initial value defined in prefab. At runtime - use AddNotification())")]
	public bool visitable;

	[Tooltip("Represents distance from GetPosition() which will mark this ping as visited.")]
	public float visitDistance = 10f;

	[Tooltip("Duration of player stay within visitDistance radius from GetPosition() to mark this instance as visited.")]
	public float visitDuration = 1f;

	private const int version = 2;

	[NonSerialized]
	[ProtoMember(1)]
	public int currentVersion = 2;

	[NonSerialized]
	[ProtoMember(2)]
	public bool visible = true;

	[NonSerialized]
	[ProtoMember(3)]
	public int colorIndex;

	[NonSerialized]
	[ProtoMember(4)]
	public string _id;

	private bool initialized;

	private string _label;

	private bool notify;

	public bool Initialized => initialized;

	public string Id
	{
		get
		{
			if (!initialized)
			{
				throw new InvalidOperationException("Attempt to access id before backing _id value is initialized/deserialized");
			}
			return _id;
		}
	}

	private void OnEnable()
	{
		if (initialized)
		{
			PingManager.Register(this);
		}
	}

	private void Start()
	{
		if (!IsSceneObject())
		{
			Initialize();
		}
	}

	private void OnDisable()
	{
		if (initialized)
		{
			PingManager.Unregister(this);
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (visitDistance > 0f)
		{
			Gizmos.color = new Color(1f, 0.65f, 0f, 0.75f);
			Gizmos.DrawWireSphere(GetPosition(), visitDistance);
		}
	}

	private void Initialize()
	{
		bool flag = false;
		if (string.IsNullOrEmpty(_id))
		{
			_id = Guid.NewGuid().ToString();
			flag = visitable && currentVersion >= 2;
		}
		currentVersion = 2;
		flag |= notify;
		initialized = true;
		PingManager.Register(this);
		if (flag)
		{
			AddNotificationInternal();
		}
	}

	private bool IsSceneObject()
	{
		return GetComponent<UniqueIdentifier>() is SceneObjectIdentifier;
	}

	public void SetVisible(bool value)
	{
		if (visible != value)
		{
			visible = value;
			if (initialized)
			{
				PingManager.NotifyVisible(this);
			}
		}
	}

	public void AddNotification()
	{
		if (initialized)
		{
			AddNotificationInternal();
		}
		else
		{
			notify = true;
		}
	}

	public void RemoveNotification()
	{
		if (initialized)
		{
			RemoveNotificationInternal();
		}
	}

	public void SetType(PingType value)
	{
		if (pingType != value)
		{
			pingType = value;
			if (initialized)
			{
				PingManager.NotifyIconChange(this);
				PingManager.NotifyRename(this);
			}
		}
	}

	public string GetLabel()
	{
		return _label;
	}

	public void SetLabel(string value)
	{
		if (!(_label == value))
		{
			_label = value;
			if (initialized)
			{
				PingManager.NotifyRename(this);
			}
		}
	}

	public void SetColor(int index)
	{
		if (index >= PingManager.colorOptions.Length)
		{
			index = 0;
		}
		colorIndex = index;
		if (initialized)
		{
			PingManager.NotifyColor(this);
		}
	}

	public Vector3 GetPosition()
	{
		return origin.position;
	}

	private void AddNotificationInternal()
	{
		NotificationManager.main.Add(NotificationManager.Group.Pings, Id, visitDuration);
	}

	private void RemoveNotificationInternal()
	{
		NotificationManager.main.Remove(NotificationManager.Group.Pings, Id);
	}

	private void OnSceneObjectsLoaded()
	{
		if (IsSceneObject())
		{
			Initialize();
		}
	}

	bool INotificationTarget.IsVisible()
	{
		if (visitDistance > 0f)
		{
			Player main = Player.main;
			if (main != null && (GetPosition() - main.GetLastPosition()).sqrMagnitude < visitDistance * visitDistance)
			{
				return true;
			}
		}
		return false;
	}

	bool INotificationTarget.IsDestroyed()
	{
		return this == null;
	}

	void INotificationTarget.Progress(float progress)
	{
		if (initialized)
		{
			PingManager.NotifyVisit(this, progress);
		}
	}
}
