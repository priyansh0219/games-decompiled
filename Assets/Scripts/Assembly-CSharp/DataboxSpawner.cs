using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class DataboxSpawner : MonoBehaviour
{
	[AssertNotNull]
	public AssetReferenceGameObject databoxPrefabReference;

	public TechType spawnTechType;

	private IEnumerator Start()
	{
		if (spawnTechType != 0 && !KnownTech.Contains(spawnTechType))
		{
			yield return AddressablesUtility.InstantiateAsync(databoxPrefabReference.RuntimeKey as string, base.transform.parent, base.transform.localPosition, base.transform.localRotation);
		}
		Object.Destroy(base.gameObject);
	}
}
