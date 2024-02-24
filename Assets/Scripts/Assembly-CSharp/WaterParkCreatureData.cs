using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "WaterParkCreatureData.asset", menuName = "Subnautica/Create WaterParkCreature Data Asset")]
public class WaterParkCreatureData : ScriptableObject
{
	public float initialSize = 0.1f;

	public float maxSize = 0.6f;

	public float outsideSize = 1f;

	public float daysToGrow = 1f;

	public bool isPickupableOutside = true;

	public bool canBreed = true;

	public AssetReferenceGameObject eggOrChildPrefab;

	public AssetReferenceGameObject adultPrefab;

	public float growingPeriod => daysToGrow * 1200f;
}
