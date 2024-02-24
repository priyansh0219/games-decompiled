using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
public class CreepvineSeed : Pickupable
{
	public float _maturity = 1f;

	private static float kMaxVelocityForPlanting = 3f;

	private static float kPlantChanceConstant = 0.2f;

	public GameObject creepvinePrefab;

	private Vector3 startScale;

	public float maturity
	{
		get
		{
			return _maturity;
		}
		set
		{
			if (value != _maturity)
			{
				_maturity = Mathf.Clamp01(value);
				base.transform.localScale = startScale * (0.1f + _maturity);
				if (_maturity >= 1f)
				{
					isPickupable = true;
				}
			}
		}
	}

	public override void Awake()
	{
		base.Awake();
		startScale = base.transform.localScale;
	}

	private void OnCollisionEnter(Collision col)
	{
		if (Utils.IsTerrain(col.gameObject))
		{
			PlantSeed(col.contacts[0].point);
		}
	}

	private void PlantSeed(Vector3 atPosition)
	{
		GameObject obj = Object.Instantiate(creepvinePrefab);
		UWE.Utils.ZeroTransform(obj.transform);
		obj.transform.position = atPosition;
		obj.GetComponent<Creepvine>().isPlanted = true;
		isPickupable = false;
		Object.Destroy(base.gameObject);
	}
}
