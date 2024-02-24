using System.Collections;
using UnityEngine;

namespace UWE
{
	public interface IPrefabRequest : IEnumerator
	{
		bool TryGetPrefab(out GameObject prefab);

		void Release();
	}
}
