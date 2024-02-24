using UnityEngine;

namespace UWE
{
	public static class DeferredSchedulerUtils
	{
		private static readonly Task.Function UpdateBehaviourDelegate = UpdateBehaviour;

		public static void Schedule(IScheduledUpdateBehaviour behaviour)
		{
			DeferredScheduler instance = DeferredScheduler.Instance;
			if (!instance)
			{
				Debug.LogError("DeferredScheduler not instantiated", behaviour as Object);
			}
			else
			{
				instance.Enqueue(UpdateBehaviourDelegate, behaviour, null);
			}
		}

		private static void UpdateBehaviour(object owner, object state)
		{
			((IScheduledUpdateBehaviour)owner).ScheduledUpdate();
		}
	}
}
