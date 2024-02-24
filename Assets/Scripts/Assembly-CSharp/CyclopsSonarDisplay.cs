using System.Collections.Generic;
using UWE;
using UnityEngine;

public class CyclopsSonarDisplay : MonoBehaviour, ICompileTimeCheckable
{
	public enum EntityType
	{
		Decoy = 0,
		Creature = 1
	}

	public class EntityPing
	{
		public GameObject entity;

		public GameObject ping;
	}

	public CyclopsNoiseManager noiseManager;

	public Transform subTransform;

	[AssertNotNull]
	public GameObject noiseRangeObject;

	[AssertNotNull]
	public GameObject sonarPingObject;

	[AssertNotNull]
	public GameObject decoyPingObject;

	public BehaviourLOD LOD;

	private float localNoiseScalar;

	private ObjectPool<EntityPing> pingPool = ObjectPoolHelper.CreatePool<EntityPing>("CyclopsSonarDisplay::EntityPing", 32);

	private List<EntityPing> entitysOnSonar = new List<EntityPing>();

	private EntityPing GetEntityPing()
	{
		return pingPool.Get();
	}

	private void ReleaseEntityPing(EntityPing ping)
	{
		if (ping.ping != null)
		{
			Object.Destroy(ping.ping);
		}
		entitysOnSonar.Remove(ping);
		ping.entity = null;
		ping.ping = null;
		pingPool.Return(ping);
	}

	private void Start()
	{
		InvokeRepeating("DistanceCheck", 3f, 3f);
		if (LOD == null)
		{
			LOD = subTransform.GetComponent<BehaviourLOD>();
		}
	}

	private void Update()
	{
		if (LOD.IsFull())
		{
			float b = noiseManager.GetNoisePercent() * 100f;
			localNoiseScalar = Mathf.Lerp(localNoiseScalar, b, Time.deltaTime * 2f);
			if (localNoiseScalar > 0f)
			{
				noiseRangeObject.transform.localScale = new Vector3(localNoiseScalar, localNoiseScalar, localNoiseScalar);
			}
			bool flag = localNoiseScalar > 0f;
			if (noiseRangeObject.activeSelf != flag)
			{
				noiseRangeObject.SetActive(flag);
			}
		}
	}

	private void DistanceCheck()
	{
		if (!LOD.IsFull())
		{
			return;
		}
		foreach (EntityPing item in entitysOnSonar)
		{
			if (item.entity == null)
			{
				RemoveEntityOnSonar(item);
				break;
			}
			if ((subTransform.position - item.entity.transform.position).sqrMagnitude > 11025f)
			{
				RemoveEntityOnSonar(item);
				break;
			}
		}
	}

	public void NewEntityOnSonar(CyclopsSonarCreatureDetector.EntityData entityData)
	{
		GameObject gameObject = entityData.gameObject;
		bool flag = true;
		foreach (EntityPing item in entitysOnSonar)
		{
			if ((bool)item.entity && item.entity.Equals(gameObject))
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			EntityPing entityPing = GetEntityPing();
			GameObject gameObject2 = Object.Instantiate((entityData.entityType == EntityType.Creature) ? sonarPingObject : decoyPingObject);
			gameObject2.transform.SetParent(base.transform);
			entityPing.ping = gameObject2;
			entityPing.entity = gameObject;
			CyclopsHUDSonarPing component = gameObject2.GetComponent<CyclopsHUDSonarPing>();
			if ((bool)component)
			{
				component.entity = gameObject;
				component.attackCyclops = entityData.attackCyclops;
				component.cyclopsObject = subTransform.gameObject;
			}
			entitysOnSonar.Add(entityPing);
		}
	}

	public void RemoveEntityOnSonar(GameObject entity)
	{
		EntityPing entityPing = null;
		foreach (EntityPing item in entitysOnSonar)
		{
			if (item.entity.Equals(entity))
			{
				entityPing = item;
				break;
			}
		}
		if (entityPing != null)
		{
			ReleaseEntityPing(entityPing);
		}
	}

	public void RemoveEntityOnSonar(EntityPing ping)
	{
		if (entitysOnSonar.Contains(ping))
		{
			ReleaseEntityPing(ping);
		}
	}

	public string CompileTimeCheck()
	{
		if (base.transform.parent != null)
		{
			if (noiseManager == null)
			{
				return $"noiseManager field must not be null on CyclopsSonarDisplay {base.name}";
			}
			if (subTransform == null)
			{
				return $"subTransform field must not be null on CyclopsSonarDisplay {base.name}";
			}
		}
		return null;
	}
}
