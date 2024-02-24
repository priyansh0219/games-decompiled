using System;
using UnityEngine;

namespace UWE
{
	public struct Task
	{
		public delegate void Function(object owner, object state);

		private readonly Function task;

		private readonly object owner;

		private readonly object state;

		public Task(Function task, object owner, object state)
		{
			this.task = task;
			this.owner = owner;
			this.state = state;
		}

		public void Execute()
		{
			try
			{
				task(owner, state);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}
}
