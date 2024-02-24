using System.Collections.Generic;
using UWE;

public static class ObjectPoolExtensions
{
	public static PooledObject<T> GetProxy<T>(this ObjectPool<T> pool) where T : class, new()
	{
		return new PooledObject<T>(pool);
	}

	public static PooledList<T> GetListProxy<T>(this ObjectPool<List<T>> pool)
	{
		return new PooledList<T>(pool);
	}
}
