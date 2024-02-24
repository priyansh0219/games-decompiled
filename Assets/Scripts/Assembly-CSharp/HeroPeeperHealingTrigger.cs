using System.Collections.Generic;
using UnityEngine;

public class HeroPeeperHealingTrigger : MonoBehaviour
{
	[AssertNotNull]
	public GameObject root;

	public float healingSpeed = 0.5f;

	private static readonly HashSet<HeroPeeperHealingTrigger> heroPeepers = new HashSet<HeroPeeperHealingTrigger>();

	public static HeroPeeperHealingTrigger GetNearestHeroPeeper(Vector3 position)
	{
		if (heroPeepers.Count < 1)
		{
			return null;
		}
		HeroPeeperHealingTrigger result = null;
		float num = float.MaxValue;
		HashSet<HeroPeeperHealingTrigger>.Enumerator enumerator = heroPeepers.GetEnumerator();
		while (enumerator.MoveNext())
		{
			HeroPeeperHealingTrigger current = enumerator.Current;
			float num2 = Vector3.Distance(position, current.transform.position);
			if (num2 < num)
			{
				result = current;
				num = num2;
			}
		}
		return result;
	}

	private void OnEnable()
	{
		heroPeepers.Add(this);
	}

	private void OnDisable()
	{
		heroPeepers.Remove(this);
	}

	private void OnTriggerStay(Collider other)
	{
		if (!base.enabled)
		{
			return;
		}
		GameObject gameObject = ((other.attachedRigidbody != null) ? other.attachedRigidbody.gameObject : other.gameObject);
		if (!(gameObject == root) && gameObject.CompareTag("Creature"))
		{
			InfectedMixin component = gameObject.GetComponent<InfectedMixin>();
			if (!(component == null))
			{
				component.Heal(healingSpeed * Time.fixedDeltaTime);
			}
		}
	}
}
