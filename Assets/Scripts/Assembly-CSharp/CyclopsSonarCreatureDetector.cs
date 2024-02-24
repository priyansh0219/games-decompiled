using System.Collections.Generic;
using UnityEngine;

public class CyclopsSonarCreatureDetector : MonoBehaviour
{
	public class EntityData
	{
		public CyclopsSonarDisplay.EntityType entityType;

		public GameObject gameObject;

		public AttackCyclops attackCyclops;
	}

	[AssertNotNull]
	public CyclopsSonarDisplay sonarDisplay;

	public float creatureDetectionRadius = 100f;

	private List<GameObject> entities = new List<GameObject>();

	private const float invokeIteration = 0.5f;

	private void OnEnable()
	{
		if (!IsInvoking("CheckForCreaturesInRange"))
		{
			InvokeRepeating("CheckForCreaturesInRange", 0.5f, 0.5f);
		}
	}

	private void OnDisable()
	{
		CancelInvoke();
	}

	private void CheckForCreaturesInRange()
	{
		ChekItemsOnHashSet(CyclopsDecoyManager.decoyList, CyclopsSonarDisplay.EntityType.Decoy);
		ChekItemsOnHashSet(AttackCyclops.attackCyclopsCreatureHashSet, CyclopsSonarDisplay.EntityType.Creature);
	}

	private void ChekItemsOnHashSet(HashSet<GameObject> checkList, CyclopsSonarDisplay.EntityType entityType)
	{
		if (checkList.Count == 0)
		{
			return;
		}
		HashSet<GameObject>.Enumerator enumerator = checkList.GetEnumerator();
		while (enumerator.MoveNext())
		{
			GameObject current = enumerator.Current;
			bool flag = (current.transform.position - base.transform.position).sqrMagnitude <= creatureDetectionRadius * creatureDetectionRadius;
			if (!entities.Contains(current) && flag)
			{
				EntityData entityData = new EntityData();
				entityData.entityType = entityType;
				entityData.gameObject = current;
				entityData.attackCyclops = current.GetComponent<AttackCyclops>();
				sonarDisplay.NewEntityOnSonar(entityData);
				entities.Add(current);
			}
			else if (entities.Contains(current) && !flag)
			{
				sonarDisplay.RemoveEntityOnSonar(current);
				entities.Remove(current);
			}
		}
	}
}
