using System;

namespace UWE
{
	public struct PooledObject<T> : IDisposable where T : class, new()
	{
		private readonly ObjectPool<T> pool;

		public T Value { get; private set; }

		public PooledObject(ObjectPool<T> pool)
		{
			Value = pool.Get();
			this.pool = pool;
		}

		public void Dispose()
		{
			pool.Return(Value);
			Value = null;
		}

		public static implicit operator T(PooledObject<T> pooledObject)
		{
			return pooledObject.Value;
		}
	}
}
