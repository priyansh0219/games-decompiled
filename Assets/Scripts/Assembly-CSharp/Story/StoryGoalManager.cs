using System;
using System.Collections.Generic;
using ProtoBuf;
using UWE;
using UnityEngine;

namespace Story
{
	[ProtoContract]
	public class StoryGoalManager : MonoBehaviour
	{
		public delegate void OnRadioMessagePending(bool newMessages);

		[AssertNotNull]
		public ItemGoalTracker itemGoalTracker;

		[AssertNotNull]
		public BiomeGoalTracker biomeGoalTracker;

		[AssertNotNull]
		public LocationGoalTracker locationGoalTracker;

		[AssertNotNull]
		public CompoundGoalTracker compoundGoalTracker;

		[AssertNotNull]
		public OnGoalUnlockTracker onGoalUnlockTracker;

		[AssertNotNull]
		public StoryGoalCustomEventHandler customEventHandler;

		private readonly HashSet<IStoryGoalListener> listeners = new HashSet<IStoryGoalListener>();

		private const int currentVersion = 3;

		[NonSerialized]
		[ProtoMember(1)]
		public int version = 3;

		[NonSerialized]
		[ProtoMember(2)]
		public readonly HashSet<string> completedGoals = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		[NonSerialized]
		[ProtoMember(4)]
		public readonly List<string> pendingRadioMessages = new List<string>();

		private bool initialized;

		public static StoryGoalManager main { get; private set; }

		public static event OnRadioMessagePending PendingMessageEvent;

		public void AddListener(IStoryGoalListener listener)
		{
			listeners.Add(listener);
		}

		public void RemoveListener(IStoryGoalListener listener)
		{
			listeners.Remove(listener);
		}

		public bool IsGoalComplete(string key)
		{
			return completedGoals.Contains(key);
		}

		public bool OnGoalComplete(string key)
		{
			if (completedGoals.Add(key))
			{
				SendGoalComplete(key);
				compoundGoalTracker.NotifyGoalComplete(completedGoals);
				onGoalUnlockTracker.NotifyGoalComplete(key);
				customEventHandler.NotifyGoalComplete(key);
				try
				{
					HashSet<IStoryGoalListener>.Enumerator enumerator = listeners.GetEnumerator();
					while (enumerator.MoveNext())
					{
						enumerator.Current.NotifyGoalComplete(key);
					}
				}
				catch (Exception exception)
				{
					Debug.LogException(exception, this);
				}
				return true;
			}
			return false;
		}

		public void AddPendingRadioMessage(string key)
		{
			pendingRadioMessages.Add(key);
			PulsePendingMessages();
		}

		[ContextMenu("Pulse pending radio messages")]
		public void PulsePendingMessages()
		{
			CancelInvoke("PulsePendingMessages");
			InvokePendingMessageEvent(pendingRadioMessages.Count > 0);
		}

		private void InvokePendingMessageEvent(bool pending)
		{
			if (StoryGoalManager.PendingMessageEvent != null)
			{
				StoryGoalManager.PendingMessageEvent(pending);
			}
		}

		[ContextMenu("Execute pending radio message")]
		public void ExecutePendingRadioMessage()
		{
			if (pendingRadioMessages.Count != 0)
			{
				string key = pendingRadioMessages[0];
				pendingRadioMessages.RemoveAt(0);
				PDALog.EntryData entryData = PDALog.Add(key);
				if (entryData != null)
				{
					OnGoalComplete("OnPlay" + entryData.key);
				}
				InvokePendingMessageEvent(pending: false);
				Invoke("PulsePendingMessages", 600f);
			}
		}

		private void Awake()
		{
			main = this;
			DevConsole.RegisterConsoleCommand(this, "goal");
			DevConsole.RegisterConsoleCommand(this, "pulseradio");
		}

		private void OnConsoleCommand_goal(NotificationCenter.Notification n)
		{
			if (n.data != null && n.data.Count == 2)
			{
				string val = (string)n.data[0];
				string key = (string)n.data[1];
				if (UWE.Utils.TryParseEnum<GoalType>(val, out var result))
				{
					StoryGoal.Execute(key, result);
					return;
				}
			}
			ErrorMessage.AddDebug("Usage: goal <pda|radio|encyclopedia> <key>");
		}

		private void OnConsoleCommand_pulseradio(NotificationCenter.Notification n)
		{
			PulsePendingMessages();
		}

		private void OnSceneObjectsLoaded()
		{
			if (!initialized)
			{
				compoundGoalTracker.Initialize(completedGoals);
				onGoalUnlockTracker.Initialize(completedGoals);
				initialized = true;
			}
		}

		private void SendGoalComplete(string key)
		{
			try
			{
				Vector3 position = Vector3.zero;
				Player player = Player.main;
				if ((bool)player)
				{
					position = player.transform.position;
				}
				GameAnalytics.LegacyEvent(GameAnalytics.Event.LegacyGoal, key);
				using (GameAnalytics.EventData eventData = GameAnalytics.CustomEvent(GameAnalytics.Event.Goal))
				{
					eventData.Add("goal", key);
					eventData.AddPosition(position);
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception, this);
			}
		}
	}
}
