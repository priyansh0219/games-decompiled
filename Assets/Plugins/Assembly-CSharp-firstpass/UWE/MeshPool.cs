using UnityEngine;

namespace UWE
{
	public class MeshPool
	{
		private ObjectPool<Mesh> pool;

		private bool _usePooling = true;

		public bool poolingEnabled
		{
			get
			{
				return _usePooling;
			}
			set
			{
				_usePooling = value;
			}
		}

		public Mesh Get()
		{
			Mesh mesh = null;
			if (poolingEnabled)
			{
				EnsurePool();
				return pool.Get();
			}
			return new Mesh();
		}

		public void Return(Mesh m)
		{
			if (poolingEnabled)
			{
				EnsurePool();
				m.Clear(keepVertexLayout: false);
				pool.Return(m);
			}
			else
			{
				Object.Destroy(m);
			}
		}

		private void EnsurePool()
		{
			if (pool == null)
			{
				pool = ObjectPoolHelper.CreatePool<Mesh>("MeshPool::Mesh", 4096);
			}
		}
	}
}
