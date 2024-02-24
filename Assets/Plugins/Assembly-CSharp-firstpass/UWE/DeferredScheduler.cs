using System.Collections.Generic;
using UnityEngine;

namespace UWE
{
	public sealed class DeferredScheduler : MonoBehaviour
	{
		private readonly Queue<Task> tasks = new Queue<Task>(128);

		public static DeferredScheduler Instance { get; private set; }

		private void Awake()
		{
			Instance = this;
		}

		private void Update()
		{
			while (tasks.Count > 0)
			{
				tasks.Dequeue().Execute();
			}
		}

		public void Enqueue(Task.Function task, object owner, object state)
		{
			Task item = new Task(task, owner, state);
			tasks.Enqueue(item);
		}
	}
}
