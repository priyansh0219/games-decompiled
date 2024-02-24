using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
	public enum Group
	{
		Undefined = 0,
		Inventory = 1,
		Blueprints = 2,
		Builder = 3,
		CraftTree = 4,
		Log = 5,
		Gallery = 6,
		Encyclopedia = 7,
		Pings = 8
	}

	[ProtoContract]
	public struct NotificationId : IEquatable<NotificationId>
	{
		public class EqualityComparer : IEqualityComparer<NotificationId>
		{
			public bool Equals(NotificationId x, NotificationId y)
			{
				return x.Equals(y);
			}

			public int GetHashCode(NotificationId obj)
			{
				return obj.GetHashCode();
			}
		}

		public static readonly EqualityComparer equalityComparer = new EqualityComparer();

		[ProtoMember(1)]
		public Group group;

		[ProtoMember(2)]
		public string key;

		public NotificationId(Group group, string key)
		{
			this.group = group;
			this.key = key;
		}

		public override int GetHashCode()
		{
			int num = 1357;
			num = (int)(31 * num + group);
			return 31 * num + ((key != null) ? key.GetHashCode() : 0);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is NotificationId))
			{
				return false;
			}
			return Equals((NotificationId)obj);
		}

		public bool Equals(NotificationId other)
		{
			if (group == other.group)
			{
				return key == other.key;
			}
			return false;
		}
	}

	[ProtoContract]
	public class NotificationData
	{
		[ProtoMember(1)]
		public float duration;

		[ProtoMember(2)]
		public float timeLeft;

		public NotificationData Clone()
		{
			return new NotificationData
			{
				duration = duration,
				timeLeft = timeLeft
			};
		}
	}

	[ProtoContract]
	public class SerializedData
	{
		private const int currentVersion = 1;

		[NonSerialized]
		[ProtoMember(1)]
		public int protoVersion = 1;

		[NonSerialized]
		[ProtoMember(2)]
		public readonly Dictionary<NotificationId, NotificationData> notifications = new Dictionary<NotificationId, NotificationData>(NotificationId.equalityComparer);
	}

	public static readonly Color notificationColor = new Color(1f, 1f, 1f, 1f);

	public const string symbol = "+";

	private const float minDuration = 0.1f;

	private const float defaultDuration = 2f;

	private const float thresholdDuration = 1f;

	private static NotificationManager _main;

	private readonly Dictionary<NotificationId, NotificationData> notifications = new Dictionary<NotificationId, NotificationData>(NotificationId.equalityComparer);

	private readonly Dictionary<NotificationId, INotificationTarget> targets = new Dictionary<NotificationId, INotificationTarget>(NotificationId.equalityComparer);

	private readonly Dictionary<INotificationListener, HashSet<NotificationId>> listeners = new Dictionary<INotificationListener, HashSet<NotificationId>>();

	private Vector2 scrollPos = new Vector2(0f, 0f);

	private Group targetsGroupFilter;

	private Group listenersGroupFilter;

	private StringBuilder sb = new StringBuilder();

	private Dictionary<Group, List<string>> sorted = new Dictionary<Group, List<string>>();

	private List<Group> _groupsOrder;

	public static NotificationManager main
	{
		get
		{
			if (_main == null)
			{
				new GameObject("NotificationManager").AddComponent<NotificationManager>();
			}
			return _main;
		}
	}

	private List<Group> groupsOrder
	{
		get
		{
			if (_groupsOrder == null)
			{
				_groupsOrder = new List<Group>();
				Array values = Enum.GetValues(typeof(Group));
				for (int i = 0; i < values.Length; i++)
				{
					Group item = (Group)values.GetValue(i);
					_groupsOrder.Add(item);
				}
			}
			return _groupsOrder;
		}
	}

	private void Awake()
	{
		if (_main != null)
		{
			Debug.LogError("Multiple NotificationManagers found in scene. This is not allowed.");
			UnityEngine.Object.Destroy(this);
		}
		else
		{
			_main = this;
		}
	}

	private void LateUpdate()
	{
		float deltaTime = PDA.deltaTime;
		using (ListPool<NotificationId> listPool = Pool<ListPool<NotificationId>>.Get())
		{
			List<NotificationId> list = listPool.list;
			foreach (KeyValuePair<NotificationId, INotificationTarget> target in targets)
			{
				NotificationId key = target.Key;
				INotificationTarget value = target.Value;
				if (value == null || value.IsDestroyed())
				{
					list.Add(key);
				}
				else
				{
					if (!notifications.TryGetValue(key, out var value2))
					{
						continue;
					}
					if (value.IsVisible())
					{
						value2.timeLeft -= deltaTime;
						if (value2.timeLeft > 0f)
						{
							value.Progress(value2.timeLeft / value2.duration);
							continue;
						}
						notifications.Remove(key);
						NotifyRemove(key);
					}
					else if (value2.duration - value2.timeLeft > 1f)
					{
						notifications.Remove(key);
						NotifyRemove(key);
					}
					else
					{
						value.Progress(1f);
						value2.timeLeft = value2.duration;
					}
				}
			}
			for (int i = 0; i < list.Count; i++)
			{
				targets.Remove(list[i]);
			}
		}
	}

	public int GetCount(Group group)
	{
		int num = 0;
		foreach (KeyValuePair<NotificationId, NotificationData> notification in notifications)
		{
			if (notification.Key.group == group)
			{
				num++;
			}
		}
		return num;
	}

	public bool Contains(Group group, string key)
	{
		return notifications.ContainsKey(new NotificationId(group, key));
	}

	public void Add(Group group, string key, float duration = 2f)
	{
		float num = Mathf.Max(0.1f, duration);
		Add(group, key, num, num);
	}

	public void Remove(Group group, string key)
	{
		NotificationId notificationId = new NotificationId(group, key);
		if (notifications.Remove(notificationId))
		{
			NotifyRemove(notificationId);
		}
	}

	public void RegisterTarget(Group group, string key, INotificationTarget target)
	{
		if (target != null && !target.IsDestroyed())
		{
			NotificationId key2 = new NotificationId(group, key);
			if (targets.TryGetValue(key2, out var value) && value != null && !value.IsDestroyed() && value.IsVisible())
			{
				value.Progress(0f);
			}
			targets[key2] = target;
			if (notifications.TryGetValue(key2, out var value2) && target.IsVisible())
			{
				target.Progress(value2.timeLeft / value2.duration);
			}
		}
	}

	public void UnregisterTarget(INotificationTarget target)
	{
		if (target == null)
		{
			return;
		}
		using (ListPool<NotificationId> listPool = Pool<ListPool<NotificationId>>.Get())
		{
			List<NotificationId> list = listPool.list;
			foreach (KeyValuePair<NotificationId, INotificationTarget> target2 in targets)
			{
				INotificationTarget value = target2.Value;
				if (target == value)
				{
					list.Add(target2.Key);
				}
			}
			for (int i = 0; i < list.Count; i++)
			{
				NotificationId notificationId = list[i];
				targets.Remove(notificationId);
				if (notifications.TryGetValue(notificationId, out var value2))
				{
					if (value2.duration - value2.timeLeft > 1f)
					{
						notifications.Remove(notificationId);
						NotifyRemove(notificationId);
					}
					else
					{
						value2.timeLeft = value2.duration;
					}
				}
			}
		}
		if (!target.IsDestroyed())
		{
			target.Progress(0f);
		}
	}

	public void Subscribe(INotificationListener listener, List<NotificationId> newIds)
	{
		if (listener == null)
		{
			return;
		}
		if (!listeners.TryGetValue(listener, out var value))
		{
			value = new HashSet<NotificationId>(NotificationId.equalityComparer);
			listeners.Add(listener, value);
		}
		for (int i = 0; i < newIds.Count; i++)
		{
			NotificationId notificationId = newIds[i];
			if (!value.Add(notificationId))
			{
				continue;
			}
			if (notificationId.key == string.Empty)
			{
				foreach (KeyValuePair<NotificationId, NotificationData> notification in notifications)
				{
					if (notification.Key.group == notificationId.group)
					{
						listener.OnAdd(notificationId.group, notification.Key.key);
					}
				}
			}
			else if (notifications.ContainsKey(notificationId))
			{
				listener.OnAdd(notificationId.group, notificationId.key);
			}
		}
	}

	public void Subscribe(INotificationListener listener, Group group, string key)
	{
		if (listener == null)
		{
			return;
		}
		NotificationId notificationId = new NotificationId(group, key);
		if (!listeners.TryGetValue(listener, out var value))
		{
			value = new HashSet<NotificationId>(NotificationId.equalityComparer);
			listeners.Add(listener, value);
		}
		value.Add(notificationId);
		if (key == string.Empty)
		{
			foreach (KeyValuePair<NotificationId, NotificationData> notification in notifications)
			{
				if (notification.Key.group == group)
				{
					listener.OnAdd(group, notification.Key.key);
				}
			}
			return;
		}
		if (notifications.ContainsKey(notificationId))
		{
			listener.OnAdd(group, key);
		}
	}

	public void Unsubscribe(INotificationListener listener)
	{
		if (listener != null)
		{
			listeners.Remove(listener);
		}
	}

	public void Add(Group group, string key, float duration, float timeLeft)
	{
		if (!string.IsNullOrEmpty(key))
		{
			NotificationId notificationId = new NotificationId(group, key);
			bool flag = false;
			if (!notifications.TryGetValue(notificationId, out var value))
			{
				flag = true;
				value = new NotificationData();
				notifications.Add(notificationId, value);
			}
			value.duration = duration;
			value.timeLeft = timeLeft;
			if (flag)
			{
				NotifyAdd(notificationId, value);
			}
		}
	}

	private void NotifyAdd(NotificationId id, NotificationData notification)
	{
		if (targets.TryGetValue(id, out var value))
		{
			value.Progress(notification.timeLeft / notification.duration);
		}
		foreach (KeyValuePair<INotificationListener, HashSet<NotificationId>> listener in listeners)
		{
			INotificationListener key = listener.Key;
			HashSet<NotificationId> value2 = listener.Value;
			if (value2.Contains(id) || value2.Contains(new NotificationId(id.group, string.Empty)))
			{
				key.OnAdd(id.group, id.key);
			}
		}
	}

	private void NotifyRemove(NotificationId id)
	{
		if (targets.TryGetValue(id, out var value))
		{
			value.Progress(0f);
		}
		foreach (KeyValuePair<INotificationListener, HashSet<NotificationId>> listener in listeners)
		{
			INotificationListener key = listener.Key;
			HashSet<NotificationId> value2 = listener.Value;
			if (value2.Contains(id) || value2.Contains(new NotificationId(id.group, string.Empty)))
			{
				key.OnRemove(id.group, id.key);
			}
		}
	}

	public void RegisterItemTarget(InventoryItem item, INotificationTarget target)
	{
		if (item == null || target == null)
		{
			return;
		}
		Pickupable item2 = item.item;
		if (item2 == null)
		{
			return;
		}
		UniqueIdentifier component = item2.GetComponent<UniqueIdentifier>();
		if (!(component == null))
		{
			string id = component.Id;
			if (!string.IsNullOrEmpty(id))
			{
				RegisterTarget(Group.Inventory, id, target);
			}
		}
	}

	public SerializedData Serialize()
	{
		SerializedData serializedData = new SerializedData();
		foreach (KeyValuePair<NotificationId, NotificationData> notification in notifications)
		{
			serializedData.notifications.Add(notification.Key, notification.Value.Clone());
		}
		return serializedData;
	}

	public void Deserialize(SerializedData serializedData)
	{
		if (serializedData == null)
		{
			return;
		}
		foreach (KeyValuePair<NotificationId, NotificationData> notification in serializedData.notifications)
		{
			NotificationId key = notification.Key;
			NotificationData value = notification.Value;
			Group group = key.group;
			string key2 = key.key;
			switch (group)
			{
			case Group.Inventory:
				Add(group, key2, value.duration, value.timeLeft);
				break;
			case Group.Blueprints:
				if (key2.DecodeKey() != 0)
				{
					Add(group, key2, value.duration, value.timeLeft);
				}
				break;
			case Group.Builder:
			{
				TechType techType = key2.DecodeKey();
				if (techType != 0 && TechData.GetBuildable(techType))
				{
					Add(group, key2, value.duration, value.timeLeft);
				}
				break;
			}
			case Group.CraftTree:
			{
				TechType techType = key2.DecodeKey();
				if (techType != 0 && CraftTree.IsCraftable(techType))
				{
					Add(group, key2, value.duration, value.timeLeft);
				}
				break;
			}
			case Group.Log:
				Add(group, key2, value.duration, value.timeLeft);
				break;
			case Group.Gallery:
				if (ScreenshotManager.HasScreenshotForFile(key2))
				{
					Add(group, key2, value.duration, value.timeLeft);
				}
				break;
			case Group.Encyclopedia:
				if (PDAEncyclopedia.HasEntryData(key2))
				{
					Add(group, key2, value.duration, value.timeLeft);
				}
				break;
			default:
				Add(group, key2, value.duration, value.timeLeft);
				break;
			}
		}
		serializedData.notifications.Clear();
	}

	public void LayoutDebugGUI()
	{
		scrollPos = GUILayout.BeginScrollView(scrollPos, false, true, GUILayout.ExpandWidth(expand: true));
		string text = ToString();
		GUILayout.Label(text);
		GUILayout.EndScrollView();
		if (GUILayout.Button("Copy", GUILayout.ExpandWidth(expand: true), GUILayout.Height(20f)))
		{
			GUIUtility.systemCopyBuffer = text;
		}
	}

	private string KeyLogWrapper(NotificationId id)
	{
		if (id.group == Group.Blueprints || id.group == Group.Builder || id.group == Group.CraftTree)
		{
			TechType techType = id.key.DecodeKey();
			if (techType != 0)
			{
				return $"{id.key} ({techType})";
			}
			return id.key;
		}
		return id.key;
	}

	public override string ToString()
	{
		sb.Length = 0;
		sorted.Clear();
		foreach (KeyValuePair<NotificationId, NotificationData> notification in notifications)
		{
			NotificationId key = notification.Key;
			if (!sorted.TryGetValue(key.group, out var value))
			{
				value = new List<string>();
				sorted.Add(key.group, value);
			}
			value.Add(key.key);
		}
		sb.AppendFormat("NOTIFICATIONS ({0} entries):\n", notifications.Count);
		for (int i = 0; i < groupsOrder.Count; i++)
		{
			Group group = groupsOrder[i];
			if (!sorted.TryGetValue(group, out var value2))
			{
				continue;
			}
			sb.AppendFormat("    {0} ({1} entries):\n", group, value2.Count);
			for (int j = 0; j < value2.Count; j++)
			{
				string key2 = value2[j];
				NotificationId notificationId = new NotificationId(group, key2);
				if (notifications.TryGetValue(notificationId, out var value3))
				{
					sb.AppendFormat("        {0}, {1}/{2}\n", KeyLogWrapper(notificationId), value3.timeLeft, value3.duration);
				}
			}
		}
		sorted.Clear();
		foreach (KeyValuePair<NotificationId, INotificationTarget> target in targets)
		{
			NotificationId key3 = target.Key;
			if (targetsGroupFilter == Group.Undefined || key3.group == targetsGroupFilter)
			{
				if (!sorted.TryGetValue(key3.group, out var value4))
				{
					value4 = new List<string>();
					sorted.Add(key3.group, value4);
				}
				value4.Add(key3.key);
			}
		}
		sb.AppendFormat("TARGETS ({0} entries, filter:{1}):\n", targets.Count, targetsGroupFilter);
		for (int k = 0; k < groupsOrder.Count; k++)
		{
			Group group2 = groupsOrder[k];
			if (!sorted.TryGetValue(group2, out var value5))
			{
				continue;
			}
			sb.AppendFormat("    {0} ({1} entries):\n", group2, value5.Count);
			for (int l = 0; l < value5.Count; l++)
			{
				string key4 = value5[l];
				NotificationId notificationId2 = new NotificationId(group2, key4);
				if (targets.TryGetValue(notificationId2, out var value6))
				{
					sb.AppendFormat("        {0} isDestroyed={1} isVisible={2}\n", KeyLogWrapper(notificationId2), value6.IsDestroyed(), value6.IsVisible());
				}
			}
		}
		sorted.Clear();
		sb.AppendFormat("LISTENERS ({0} entries, filter:{1}):\n", listeners.Count, listenersGroupFilter);
		foreach (KeyValuePair<INotificationListener, HashSet<NotificationId>> listener in listeners)
		{
			_ = listener.Key;
			HashSet<NotificationId> value7 = listener.Value;
			sb.AppendFormat("    - ({0} entries):\n", value7.Count);
			foreach (NotificationId item in value7)
			{
				if (listenersGroupFilter == Group.Undefined || item.group == listenersGroupFilter)
				{
					sb.AppendFormat("        - {0} {1}\n", item.group, string.IsNullOrEmpty(item.key) ? "ALL" : item.key);
				}
			}
		}
		sorted.Clear();
		return sb.ToString();
	}
}
