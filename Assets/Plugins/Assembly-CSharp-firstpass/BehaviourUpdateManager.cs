#define PROFILER_MARKERS
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class BehaviourUpdateManager : MonoBehaviour
{
	public interface IManagedPolicy<T>
	{
		int GetIndex(T item);

		void SetIndex(T item, int index);

		void CallUpdate(T item);

		string GetProfileTag();
	}

	public class UpdatePolicy : IManagedPolicy<IManagedUpdateBehaviour>
	{
		public int GetIndex(IManagedUpdateBehaviour item)
		{
			return item.managedUpdateIndex;
		}

		public void SetIndex(IManagedUpdateBehaviour item, int index)
		{
			item.managedUpdateIndex = index;
		}

		public void CallUpdate(IManagedUpdateBehaviour item)
		{
			item.ManagedUpdate();
		}

		public string GetProfileTag()
		{
			return "Update";
		}
	}

	public class LateUpdatePolicy : IManagedPolicy<IManagedLateUpdateBehaviour>
	{
		public int GetIndex(IManagedLateUpdateBehaviour item)
		{
			return item.managedLateUpdateIndex;
		}

		public void SetIndex(IManagedLateUpdateBehaviour item, int index)
		{
			item.managedLateUpdateIndex = index;
		}

		public void CallUpdate(IManagedLateUpdateBehaviour item)
		{
			item.ManagedLateUpdate();
		}

		public string GetProfileTag()
		{
			return "LateUpdate";
		}
	}

	public class FixedUpdatePolicy : IManagedPolicy<IManagedFixedUpdateBehaviour>
	{
		public int GetIndex(IManagedFixedUpdateBehaviour item)
		{
			return item.managedFixedUpdateIndex;
		}

		public void SetIndex(IManagedFixedUpdateBehaviour item, int index)
		{
			item.managedFixedUpdateIndex = index;
		}

		public void CallUpdate(IManagedFixedUpdateBehaviour item)
		{
			item.ManagedFixedUpdate();
		}

		public string GetProfileTag()
		{
			return "FixedUpdate";
		}
	}

	public interface IBehaviourSet
	{
		int Count { get; }
	}

	public class BehaviourSet<T> : IBehaviourSet where T : IManagedBehaviour
	{
		private readonly List<T> behaviours = new List<T>();

		private readonly List<T> toAdd = new List<T>();

		private readonly List<T> toRemove = new List<T>();

		private bool iterating;

		private readonly IManagedPolicy<T> policy;

		public int Count => behaviours.Count;

		public BehaviourSet(IManagedPolicy<T> policy)
		{
			this.policy = policy;
		}

		public bool Add(T behaviour)
		{
			if (policy.GetIndex(behaviour) > 0)
			{
				return false;
			}
			if (!iterating)
			{
				policy.SetIndex(behaviour, behaviours.Count + 1);
				behaviours.Add(behaviour);
			}
			else
			{
				toAdd.Add(behaviour);
			}
			return true;
		}

		public bool Remove(T behaviour)
		{
			int index = policy.GetIndex(behaviour);
			if (index <= 0)
			{
				return false;
			}
			if (!iterating)
			{
				T val = behaviours[behaviours.Count - 1];
				behaviours[index - 1] = val;
				policy.SetIndex(val, index);
				behaviours.RemoveAt(behaviours.Count - 1);
				policy.SetIndex(behaviour, 0);
			}
			else
			{
				toRemove.Add(behaviour);
			}
			return true;
		}

		public void Print()
		{
			foreach (T behaviour in behaviours)
			{
				UnityEngine.Debug.Log(behaviour.GetProfileTag(), behaviour as UnityEngine.Object);
			}
		}

		public void CallUpdate()
		{
			StartIterating();
			try
			{
				ProfilingUtils.BeginSample(policy.GetProfileTag());
				for (int i = 0; i < behaviours.Count; i++)
				{
					T val = behaviours[i];
					StartProfiling(val);
					try
					{
						policy.CallUpdate(val);
					}
					catch (Exception exception)
					{
						UnityEngine.Debug.LogException(exception, val as UnityEngine.Object);
					}
					StopProfiling();
				}
				ProfilingUtils.EndSample();
			}
			finally
			{
				StopIterating();
			}
		}

		private void StartIterating()
		{
			iterating = true;
		}

		private void StopIterating()
		{
			iterating = false;
			ProfilingUtils.BeginSample("PendingOperations");
			foreach (T item in toAdd)
			{
				Add(item);
			}
			foreach (T item2 in toRemove)
			{
				Remove(item2);
			}
			toAdd.Clear();
			toRemove.Clear();
			ProfilingUtils.EndSample();
		}

		[Conditional("PROFILER_MARKERS")]
		private static void StartProfiling(IManagedBehaviour beh)
		{
			ProfilingUtils.BeginSample(beh.GetProfileTag());
		}

		[Conditional("PROFILER_MARKERS")]
		private static void StopProfiling()
		{
			ProfilingUtils.EndSample();
		}
	}

	public readonly BehaviourSet<IManagedUpdateBehaviour> updateSet = new BehaviourSet<IManagedUpdateBehaviour>(new UpdatePolicy());

	public readonly BehaviourSet<IManagedLateUpdateBehaviour> lateUpdateSet = new BehaviourSet<IManagedLateUpdateBehaviour>(new LateUpdatePolicy());

	public readonly BehaviourSet<IManagedFixedUpdateBehaviour> fixedUpdateSet = new BehaviourSet<IManagedFixedUpdateBehaviour>(new FixedUpdatePolicy());

	public static BehaviourUpdateManager Instance { get; private set; }

	private void OnEnable()
	{
		Instance = this;
	}

	private void OnDisable()
	{
		Instance = null;
	}

	private void Update()
	{
		updateSet.CallUpdate();
	}

	private void FixedUpdate()
	{
		fixedUpdateSet.CallUpdate();
	}

	private void LateUpdate()
	{
		lateUpdateSet.CallUpdate();
	}

	public void DebugGUI()
	{
		DebugGUI("Update", updateSet);
		DebugGUI("Late Update", lateUpdateSet);
		DebugGUI("Fixed Update", fixedUpdateSet);
	}

	public static void DebugGUI(string label, IBehaviourSet set)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(label, GUILayout.Width(120f));
		GUILayout.TextField(set.Count.ToString(), GUILayout.Width(60f));
		GUILayout.EndHorizontal();
	}
}
