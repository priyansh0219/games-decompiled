using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using UnityEngine;

namespace UWE
{
	public class NativeArrayPoolBucket<T> : IArrayPoolBucketBase<T> where T : struct
	{
		private readonly Stack<NativeArray<T>> arrays = new Stack<NativeArray<T>>();

		private readonly HashSet<NativeArray<T>> pooledArrays = new HashSet<NativeArray<T>>();

		public int numArraysPooled => arrays.Count;

		public int numArraysOutstanding { get; private set; }

		public int peakArraysOutstanding { get; private set; }

		public long totalBytesAllocated => (numArraysPooled + numArraysOutstanding) * arraySize;

		public long totalBytesWasted { get; private set; }

		public int arraySize { get; private set; }

		public NativeArrayPoolBucket(int arraySize)
		{
			this.arraySize = arraySize;
		}

		public NativeArray<T> AddArray()
		{
			NativeArray<T> nativeArray = new NativeArray<T>(arraySize, Allocator.Persistent);
			arrays.Push(nativeArray);
			return nativeArray;
		}

		public NativeArray<T> RemoveArray()
		{
			return arrays.Pop();
		}

		public bool TryGet(out NativeArray<T> result)
		{
			if (arrays.Count < 1)
			{
				result = NativeArrayPool<T>.emptyArray;
				return false;
			}
			result = RemoveArray();
			numArraysOutstanding++;
			peakArraysOutstanding = Mathf.Max(peakArraysOutstanding, numArraysOutstanding);
			return true;
		}

		public void Return(NativeArray<T> array)
		{
			arrays.Push(array);
			numArraysOutstanding--;
		}

		public NativeArray<T> AllocateWasteArray(int minLength)
		{
			NativeArray<T> result = new NativeArray<T>(arraySize, Allocator.Persistent);
			totalBytesWasted += arraySize - minLength;
			numArraysOutstanding++;
			peakArraysOutstanding = Mathf.Max(peakArraysOutstanding, numArraysOutstanding);
			return result;
		}

		public void Clear()
		{
			foreach (NativeArray<T> array in arrays)
			{
				array.Dispose();
			}
			arrays.Clear();
			arrays.TrimExcess();
			pooledArrays.Clear();
			pooledArrays.TrimExcess();
			numArraysOutstanding = 0;
			totalBytesWasted = 0L;
			peakArraysOutstanding = 0;
		}

		[Conditional("DEBUG_UNITY_EDITOR")]
		private void AssertAdd(NativeArray<T> array, string message)
		{
		}

		[Conditional("DEBUG_UNITY_EDITOR")]
		private void AssertRemove(NativeArray<T> array, string message)
		{
		}
	}
}
