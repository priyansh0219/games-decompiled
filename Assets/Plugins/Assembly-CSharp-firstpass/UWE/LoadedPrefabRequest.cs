using System.Collections;
using UnityEngine;

namespace UWE
{
	public class LoadedPrefabRequest : IPrefabRequest, IEnumerator
	{
		private readonly GameObject prefab;

		public object Current => null;

		public LoadedPrefabRequest(GameObject prefab)
		{
			this.prefab = prefab;
		}

		public bool TryGetPrefab(out GameObject result)
		{
			result = prefab;
			return prefab != null;
		}

		public void Release()
		{
		}

		public bool MoveNext()
		{
			return false;
		}

		public void Reset()
		{
		}
	}
}
