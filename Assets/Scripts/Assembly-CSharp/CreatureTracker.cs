using System.Collections;
using System.Collections.Generic;
using UWE;
using UnityEngine;

public class CreatureTracker : MonoBehaviour
{
	public float radius = 1f;

	public float updateInterval = 1f;

	public TechType techTypeFilter;

	private Coroutine routine;

	private readonly HashSet<GameObject> creatureSet = new HashSet<GameObject>();

	private void OnEnable()
	{
		routine = StartCoroutine(UpdateRoutine());
	}

	private void OnDisable()
	{
		if (routine != null)
		{
			StopCoroutine(routine);
			routine = null;
		}
	}

	private IEnumerator UpdateRoutine()
	{
		while (true)
		{
			creatureSet.Clear();
			int num = UWE.Utils.OverlapSphereIntoSharedBuffer(base.transform.position, radius, -5, QueryTriggerInteraction.Ignore);
			for (int i = 0; i < num; i++)
			{
				Rigidbody attachedRigidbody = UWE.Utils.sharedColliderBuffer[i].attachedRigidbody;
				if (!(attachedRigidbody == null))
				{
					GameObject gameObject = attachedRigidbody.gameObject;
					if (!(gameObject.GetComponent<Creature>() == null) && (techTypeFilter == TechType.None || techTypeFilter == CraftData.GetTechType(gameObject)))
					{
						creatureSet.Add(gameObject);
					}
				}
			}
			yield return new WaitForSeconds(updateInterval);
		}
	}

	public HashSet<GameObject> Get()
	{
		return creatureSet;
	}
}
