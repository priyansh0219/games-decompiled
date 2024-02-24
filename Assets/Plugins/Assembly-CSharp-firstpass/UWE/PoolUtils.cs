using System;
using System.Collections.Generic;

namespace UWE
{
	public static class PoolUtils
	{
		public static T Get<T>(this Stack<T> pool) where T : new()
		{
			return pool.Get(Allocate<T>);
		}

		public static T Get<T>(this Stack<T> pool, Func<T> allocate)
		{
			if (pool.Count <= 0)
			{
				return allocate();
			}
			return pool.Pop();
		}

		public static void Return<T>(this Stack<T> pool, T item)
		{
			pool.Push(item);
		}

		public static T GetThreaded<T>(this Stack<T> pool) where T : new()
		{
			return pool.GetThreaded(Allocate<T>);
		}

		public static T GetThreaded<T>(this Stack<T> pool, Func<T> allocate)
		{
			lock (pool)
			{
				return (pool.Count > 0) ? pool.Pop() : allocate();
			}
		}

		public static void ReturnThreaded<T>(this Stack<T> pool, T item)
		{
			lock (pool)
			{
				pool.Push(item);
			}
		}

		private static T Allocate<T>() where T : new()
		{
			return new T();
		}
	}
}
