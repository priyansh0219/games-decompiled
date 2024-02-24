using System;
using System.Collections.Generic;
using System.Text;
using UWE;
using UnityEngine;

namespace ProtoBuf
{
	public class ProtoReaderStringCache
	{
		private class StringDataWrapper : IEquatable<StringDataWrapper>
		{
			private IAlloc<byte> sourceBytesAlloc;

			private byte[] sourceBytesRef;

			private int startIndex;

			private int stringLength;

			public void InitWithData(byte[] buffer, int ioIndex, int numBytes)
			{
				sourceBytesRef = buffer;
				startIndex = ioIndex;
				stringLength = numBytes;
				if (sourceBytesAlloc != null)
				{
					CommonByteArrayAllocator.Free(sourceBytesAlloc);
					sourceBytesAlloc = null;
				}
			}

			public void CopyIntoPooledData(byte[] buffer, int ioIndex, int numBytes)
			{
				if (sourceBytesAlloc == null)
				{
					sourceBytesAlloc = CommonByteArrayAllocator.Allocate(numBytes);
				}
				else if (sourceBytesAlloc.Length < numBytes)
				{
					CommonByteArrayAllocator.Free(sourceBytesAlloc);
					sourceBytesAlloc = CommonByteArrayAllocator.Allocate(numBytes);
				}
				Buffer.BlockCopy(buffer, ioIndex, sourceBytesAlloc.Array, sourceBytesAlloc.Offset, numBytes);
				ioIndex = 0;
				stringLength = numBytes;
				sourceBytesRef = null;
			}

			public void ReturnByteArrayToPool()
			{
				if (sourceBytesAlloc != null)
				{
					CommonByteArrayAllocator.Free(sourceBytesAlloc);
				}
			}

			public override int GetHashCode()
			{
				int num = -2128831035;
				for (int i = 0; i < stringLength; i++)
				{
					num = (num ^ GetByte(i + startIndex)) * 16777619;
				}
				num += num << 13;
				num ^= num >> 7;
				num += num << 3;
				num ^= num >> 17;
				return num + (num << 5);
			}

			public bool Equals(StringDataWrapper other)
			{
				if (other.stringLength != stringLength)
				{
					return false;
				}
				for (int i = 0; i < stringLength; i++)
				{
					if (GetByte(startIndex + i) != other.GetByte(other.startIndex + i))
					{
						return false;
					}
				}
				return true;
			}

			public long EstimateBytes()
			{
				return ((sourceBytesRef != null) ? sourceBytesRef.Length : sourceBytesAlloc.Length) + 8;
			}

			private byte GetByte(int index)
			{
				if (sourceBytesRef == null)
				{
					return sourceBytesAlloc[index];
				}
				return sourceBytesRef[index];
			}
		}

		private const int maxStringCacheSize = 5000;

		private static readonly Dictionary<StringDataWrapper, string> stringDataDictionary = new Dictionary<StringDataWrapper, string>();

		private static readonly Queue<StringDataWrapper> stringDataLRUQueue = new Queue<StringDataWrapper>();

		private static StringDataWrapper comparisonStringData = new StringDataWrapper();

		private static int cacheHits = 0;

		private static int cacheMisses = 0;

		public static void CleanupCache()
		{
			while (stringDataLRUQueue.Count > 5000)
			{
				StringDataWrapper stringDataWrapper = stringDataLRUQueue.Dequeue();
				stringDataDictionary.Remove(stringDataWrapper);
				stringDataWrapper.ReturnByteArrayToPool();
			}
		}

		public static string FindOrAdd(UTF8Encoding encoding, byte[] buffer, int ioIndex, int numBytes)
		{
			if (numBytes == 36 && buffer[ioIndex + 8] == 45)
			{
				return encoding.GetString(buffer, ioIndex, numBytes);
			}
			string value = null;
			comparisonStringData.InitWithData(buffer, ioIndex, numBytes);
			if (!stringDataDictionary.TryGetValue(comparisonStringData, out value))
			{
				value = encoding.GetString(buffer, ioIndex, numBytes);
				StringDataWrapper stringDataWrapper = new StringDataWrapper();
				stringDataWrapper.CopyIntoPooledData(buffer, ioIndex, numBytes);
				stringDataDictionary.Add(stringDataWrapper, value);
				cacheMisses++;
				stringDataLRUQueue.Enqueue(stringDataWrapper);
			}
			else
			{
				cacheHits++;
			}
			return value;
		}

		public static void PrintCacheReport()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("CACHED STRING DICTIONARY");
			stringBuilder.AppendFormat("HITS {0} MISSES {1}\n", cacheHits, cacheMisses);
			_ = stringDataDictionary.Count;
			long num = 0L;
			foreach (KeyValuePair<StringDataWrapper, string> item in stringDataDictionary)
			{
				num += item.Key.EstimateBytes();
				num += Encoding.ASCII.GetByteCount(item.Value);
			}
			stringBuilder.AppendFormat("ESTIMATED CACHE SIZE {0}\n", num);
			stringBuilder.AppendFormat("CURRENT POOL SIZE {0}\n", CommonByteArrayAllocator.EstimateBytes());
			stringBuilder.AppendFormat("QUEUE SIZE {0}  DICT SIZE {1} \n", stringDataLRUQueue.Count, stringDataDictionary.Count);
			Debug.Log(stringBuilder.ToString());
		}
	}
}
