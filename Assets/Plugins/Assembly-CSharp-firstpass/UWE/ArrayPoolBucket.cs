using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace UWE
{
	public class ArrayPoolBucket<T> : IArrayPoolBucketBase<T>
	{
		private readonly Stack<T[]> arrays = new Stack<T[]>();

		private readonly HashSet<T[]> pooledArrays = new HashSet<T[]>();

		public int numArraysPooled => arrays.Count;

		public int numArraysOutstanding { get; private set; }

		public int peakArraysOutstanding { get; private set; }

		public long totalBytesAllocated => (numArraysPooled + numArraysOutstanding) * arraySize;

		public long totalBytesWasted { get; private set; }

		public int arraySize { get; private set; }

		public ArrayPoolBucket(int arraySize)
		{
			this.arraySize = arraySize;
		}

		public T[] AddArray()
		{
			T[] array = new T[arraySize];
			arrays.Push(array);
			return array;
		}

		public T[] RemoveArray()
		{
			return arrays.Pop();
		}

		public bool TryGet(out T[] result)
		{
			if (arrays.Count < 1)
			{
				result = null;
				return false;
			}
			result = RemoveArray();
			numArraysOutstanding++;
			peakArraysOutstanding = Mathf.Max(peakArraysOutstanding, numArraysOutstanding);
			return true;
		}

		public void Return(T[] array)
		{
			arrays.Push(array);
			numArraysOutstanding--;
		}

		public T[] AllocateWasteArray(int minLength)
		{
			T[] result = new T[arraySize];
			totalBytesWasted += arraySize - minLength;
			numArraysOutstanding++;
			peakArraysOutstanding = Mathf.Max(peakArraysOutstanding, numArraysOutstanding);
			return result;
		}

		public void Clear()
		{
			arrays.Clear();
			pooledArrays.Clear();
			numArraysOutstanding = 0;
			totalBytesWasted = 0L;
			peakArraysOutstanding = 0;
		}

		[Conditional("DEBUG_UNITY_EDITOR")]
		private void AssertAdd(T[] array, string message)
		{
		}

		[Conditional("DEBUG_UNITY_EDITOR")]
		private void AssertRemove(T[] array, string message)
		{
		}
	}
}
