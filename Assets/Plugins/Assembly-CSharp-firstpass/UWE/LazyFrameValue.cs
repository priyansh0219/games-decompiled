using System;
using UnityEngine;

namespace UWE
{
	public class LazyFrameValue<T>
	{
		public delegate T EvalFunction();

		private T currValue;

		private int lastEvaledFrame = -1;

		private bool beingEvaled;

		public readonly EvalFunction eval;

		public LazyFrameValue(EvalFunction eval)
		{
			this.eval = eval;
		}

		public void Reset()
		{
			lastEvaledFrame = -1;
			beingEvaled = false;
		}

		public T Get()
		{
			if (beingEvaled)
			{
				Debug.LogError("LazyFrameValue cycle detected!");
				return currValue;
			}
			if (Time.frameCount != lastEvaledFrame)
			{
				beingEvaled = true;
				try
				{
					currValue = eval();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
				lastEvaledFrame = Time.frameCount;
				beingEvaled = false;
			}
			return currValue;
		}
	}
}
