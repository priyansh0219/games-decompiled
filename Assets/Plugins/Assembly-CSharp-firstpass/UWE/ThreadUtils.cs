using System;
using System.Collections;
using System.Threading;
using UnityEngine;

namespace UWE
{
	public static class ThreadUtils
	{
		private sealed class WaitHandle : CustomYieldInstruction
		{
			private bool executed;

			public static readonly Task.Function ExecuteDelegate = Execute;

			public override bool keepWaiting => !executed;

			public static void Execute(object owner, object state)
			{
				((WaitHandle)owner).executed = true;
			}
		}

		private sealed class Throttle
		{
			private readonly IThread thread;

			private readonly int timeoutMilliseconds;

			public static readonly Task.Function StepDelegate = Step;

			public Throttle(IThread thread, float frequency)
			{
				this.thread = thread;
				timeoutMilliseconds = (int)(1000f / frequency);
			}

			public void Execute()
			{
				Thread.Sleep(timeoutMilliseconds);
				thread.Enqueue(StepDelegate, this, null);
			}

			public static void Step(object owner, object state)
			{
				((Throttle)owner).Execute();
			}
		}

		public static readonly Task.Function StepCoroutineDelegate = StepCoroutine;

		private static void StepCoroutine(object owner, object state)
		{
			IThread thread = (IThread)owner;
			IEnumerator enumerator = (IEnumerator)state;
			if (enumerator.MoveNext())
			{
				if (enumerator.Current is CallbackAwaiter callbackAwaiter)
				{
					callbackAwaiter.Initialize(thread, enumerator);
				}
				else
				{
					thread.Enqueue(StepCoroutineDelegate, thread, enumerator);
				}
			}
		}

		private static void ExecuteAction(object owner, object state)
		{
			((Action)owner)();
		}

		private static void ExecuteAction<T>(object owner, object state)
		{
			Action<T> obj = (Action<T>)owner;
			T obj2 = (T)state;
			obj(obj2);
		}

		private static void ExecuteAction<T1, T2>(object owner, object state)
		{
			Action<T1, T2> obj = (Action<T1, T2>)owner;
			Tuple<T1, T2> tuple = (Tuple<T1, T2>)state;
			obj(tuple.Item1, tuple.Item2);
		}

		private static void ExecuteAction<T1, T2, T3>(object owner, object state)
		{
			Action<T1, T2, T3> obj = (Action<T1, T2, T3>)owner;
			Tuple<T1, T2, T3> tuple = (Tuple<T1, T2, T3>)state;
			obj(tuple.Item1, tuple.Item2, tuple.Item3);
		}

		private static void ExecuteAction<T1, T2, T3, T4>(object owner, object state)
		{
			Action<T1, T2, T3, T4> obj = (Action<T1, T2, T3, T4>)owner;
			Tuple<T1, T2, T3, T4> tuple = (Tuple<T1, T2, T3, T4>)state;
			obj(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
		}

		private static void ExecuteAction<T1, T2, T3, T4, T5>(object owner, object state)
		{
			Action<T1, T2, T3, T4, T5> obj = (Action<T1, T2, T3, T4, T5>)owner;
			Tuple<T1, T2, T3, T4, T5> tuple = (Tuple<T1, T2, T3, T4, T5>)state;
			obj(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5);
		}

		public static void Enqueue(this IThread thread, Action action)
		{
			thread.Enqueue(ExecuteAction, action, null);
		}

		public static void Enqueue<T>(this IThread thread, Action<T> action, T arg)
		{
			thread.Enqueue(ExecuteAction<T>, action, arg);
		}

		public static void Enqueue<T1, T2>(this IThread thread, Action<T1, T2> action, T1 arg1, T2 arg2)
		{
			thread.Enqueue(ExecuteAction<T1, T2>, action, Tuple.Create(arg1, arg2));
		}

		public static void Enqueue<T1, T2, T3>(this IThread thread, Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3)
		{
			thread.Enqueue(ExecuteAction<T1, T2, T3>, action, Tuple.Create(arg1, arg2, arg3));
		}

		public static void Enqueue<T1, T2, T3, T4>(this IThread thread, Action<T1, T2, T3, T4> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			thread.Enqueue(ExecuteAction<T1, T2, T3, T4>, action, Tuple.Create(arg1, arg2, arg3, arg4));
		}

		public static void Enqueue<T1, T2, T3, T4, T5>(this IThread thread, Action<T1, T2, T3, T4, T5> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
		{
			thread.Enqueue(ExecuteAction<T1, T2, T3, T4, T5>, action, Tuple.Create(arg1, arg2, arg3, arg4, arg5));
		}

		public static void ThrottleThread(this IThread thread, float frequency)
		{
			Throttle owner = new Throttle(thread, frequency);
			thread.Enqueue(Throttle.StepDelegate, owner, null);
		}

		public static CustomYieldInstruction Wait(this IThread thread)
		{
			WaitHandle waitHandle = new WaitHandle();
			thread.Enqueue(WaitHandle.ExecuteDelegate, waitHandle, null);
			return waitHandle;
		}

		public static void StartCoroutine(this IThread thread, IEnumerator coroutine)
		{
			thread.Enqueue(StepCoroutineDelegate, thread, coroutine);
		}

		public static WorkerThread StartThrottledThread(string group, string name, System.Threading.ThreadPriority priority, int coreAffinityMask, int initialCapacity, float frequency)
		{
			WorkerThread workerThread = StartWorkerThread(group, name, priority, coreAffinityMask, initialCapacity);
			workerThread.ThrottleThread(frequency);
			return workerThread;
		}

		public static WorkerThread StartWorkerThread(string group, string name, System.Threading.ThreadPriority priority, int coreAffinityMask, int initialCapacity)
		{
			WorkerThread workerThread = new WorkerThread(group, name, priority, coreAffinityMask, initialCapacity);
			workerThread.Start();
			return workerThread;
		}

		public static UnityThread StartUnityThread(string name, int initialCapacity, MonoBehaviour host)
		{
			UnityThread unityThread = new UnityThread(name, initialCapacity);
			host.StartCoroutine(unityThread.Pump());
			return unityThread;
		}
	}
}
