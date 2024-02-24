using System.Collections;
using System.Threading;

namespace UWE
{
	public class CallbackAwaiter
	{
		private const int STATUS_NONE = 0;

		private const int STATUS_INITIALIZED = 1;

		private const int STATUS_CALLED = 2;

		private IThread thread;

		private IEnumerator coroutine;

		private volatile int status;

		public void Initialize(IThread thread, IEnumerator coroutine)
		{
			this.thread = thread;
			this.coroutine = coroutine;
			if (Interlocked.CompareExchange(ref status, 1, 0) == 2)
			{
				ResumeCoroutine();
			}
		}

		public void Call()
		{
			if (Interlocked.CompareExchange(ref status, 2, 0) == 1)
			{
				ResumeCoroutine();
			}
		}

		private void ResumeCoroutine()
		{
			thread.Enqueue(ThreadUtils.StepCoroutineDelegate, thread, coroutine);
		}
	}
	public sealed class CallbackAwaiter<T1> : CallbackAwaiter
	{
		public T1 arg1 { get; private set; }

		public void Call(T1 arg1)
		{
			this.arg1 = arg1;
			Call();
		}
	}
	public sealed class CallbackAwaiter<T1, T2> : CallbackAwaiter
	{
		public T1 arg1 { get; private set; }

		public T2 arg2 { get; private set; }

		public void Call(T1 arg1, T2 arg2)
		{
			this.arg1 = arg1;
			this.arg2 = arg2;
			Call();
		}
	}
	public sealed class CallbackAwaiter<T1, T2, T3> : CallbackAwaiter
	{
		public T1 arg1 { get; private set; }

		public T2 arg2 { get; private set; }

		public T3 arg3 { get; private set; }

		public void Call(T1 arg1, T2 arg2, T3 arg3)
		{
			this.arg1 = arg1;
			this.arg2 = arg2;
			this.arg3 = arg3;
			Call();
		}
	}
}
