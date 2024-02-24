using System;

namespace UWE
{
	public static class ObjectPoolHelper
	{
		public static ObjectPool<T> CreatePool<T>() where T : class, new()
		{
			return CreatePool<T>(32);
		}

		public static ObjectPool<T> CreatePool<T>(int capacity) where T : class, new()
		{
			return CreatePool<T>($"ObjectPool-{typeof(T)}", capacity);
		}

		public static ObjectPool<T> CreatePool<T>(string name, int capacity) where T : class, new()
		{
			return CreatePool(name, capacity, Allocate<T>);
		}

		public static ObjectPool<T> CreatePool<T>(string name, int capacity, Func<T> allocate) where T : class
		{
			return new ObjectPool<T>(name, capacity, allocate);
		}

		public static ObjectPool<T[]> CreateFixedArrayPool<T>(string name, int capacity, int arrayLength)
		{
			return CreatePool(name, capacity, () => new T[arrayLength]);
		}

		public static T Allocate<T>() where T : new()
		{
			return new T();
		}
	}
}
