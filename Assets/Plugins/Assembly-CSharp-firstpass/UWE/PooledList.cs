using System;
using System.Collections.Generic;

namespace UWE
{
	public struct PooledList<T> : IDisposable
	{
		private readonly ObjectPool<List<T>> pool;

		public List<T> Value { get; private set; }

		public PooledList(ObjectPool<List<T>> pool)
		{
			Value = pool.Get();
			this.pool = pool;
		}

		public void Dispose()
		{
			Value.Clear();
			pool.Return(Value);
			Value = null;
		}

		public static implicit operator List<T>(PooledList<T> pooledObject)
		{
			return pooledObject.Value;
		}
	}
}
