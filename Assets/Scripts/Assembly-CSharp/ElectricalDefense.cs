using UWE;
using UnityEngine;

public class ElectricalDefense : MonoBehaviour
{
	public FMODAsset defenseSound;

	public GameObject[] fxElecSpheres;

	public float radius = 15f;

	public float chargeRadius = 1f;

	public float damage = 1f;

	public float chargeDamage = 2f;

	[HideInInspector]
	public float charge;

	[HideInInspector]
	public float chargeScalar;

	private void Start()
	{
		float num = radius + charge * chargeRadius;
		float originalDamage = damage + charge * chargeDamage;
		Utils.PlayFMODAsset(defenseSound, base.transform);
		int num2 = Mathf.Clamp((int)(chargeScalar * (float)fxElecSpheres.Length), 0, fxElecSpheres.Length - 1);
		Debug.Log("elecPrefabID = " + num2);
		if (MiscSettings.flashes)
		{
			Utils.SpawnZeroedAt(fxElecSpheres[num2], base.transform);
		}
		int num3 = UWE.Utils.OverlapSphereIntoSharedBuffer(base.transform.position, num);
		for (int i = 0; i < num3; i++)
		{
			Collider collider = UWE.Utils.sharedColliderBuffer[i];
			GameObject entityRoot = UWE.Utils.GetEntityRoot(collider.gameObject);
			if (entityRoot == null)
			{
				entityRoot = collider.gameObject;
			}
			Creature component = entityRoot.GetComponent<Creature>();
			LiveMixin component2 = entityRoot.GetComponent<LiveMixin>();
			if (component != null && component2 != null)
			{
				component2.TakeDamage(originalDamage, base.transform.position, DamageType.Electrical, base.gameObject);
			}
		}
		Object.Destroy(base.gameObject, 5f);
	}
}
