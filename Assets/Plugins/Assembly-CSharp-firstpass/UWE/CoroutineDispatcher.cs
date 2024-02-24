using System.Collections;
using System.Collections.Generic;
using Gendarme;
using UnityEngine;

namespace UWE
{
	public class CoroutineDispatcher : MonoBehaviour
	{
		private static CoroutineDispatcher main;

		private readonly Queue<IEnumerator> coroutines = new Queue<IEnumerator>();

		private readonly object coroutinesLock = new object();

		public int numDispatched { get; private set; }

		public int GetQueueLength()
		{
			lock (coroutinesLock)
			{
				return coroutines.Count;
			}
		}

		[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule")]
		private void Start()
		{
			main = this;
		}

		private void Update()
		{
			IEnumerator enumerator = null;
			lock (coroutinesLock)
			{
				if (coroutines.Count > 0)
				{
					enumerator = coroutines.Dequeue();
				}
			}
			if (enumerator != null)
			{
				numDispatched++;
				StartCoroutine(enumerator);
			}
		}

		private void DispatchImpl(IEnumerator coroutine)
		{
			lock (coroutinesLock)
			{
				coroutines.Enqueue(coroutine);
			}
		}

		public static void Dispatch(IEnumerator coroutine)
		{
			main.DispatchImpl(coroutine);
		}
	}
}
