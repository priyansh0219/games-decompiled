using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class SpawnRandom : MonoBehaviour
{
	[AssertNotNull]
	public AssetReferenceGameObject[] assetReferences;

	private IEnumerator Start()
	{
		AssetReferenceGameObject random = assetReferences.GetRandom();
		yield return AddressablesUtility.InstantiateAsync(random.RuntimeKey as string, base.transform.parent, base.transform.localPosition, base.transform.localRotation);
		Object.Destroy(base.gameObject);
	}
}
