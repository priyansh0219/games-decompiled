using System.Collections;
using System.Collections.Generic;
using UWE;
using UnityEngine;

public class PooledMonoBehaviour : MonoBehaviour, GameObjectPool.IPooledObject
{
	private int m_poolQueueID = -1;

	private List<GameObjectPool.IPooledObject> m_pooledObjectCache = new List<GameObjectPool.IPooledObject>();

	private Coroutine m_timerCoroutine;

	private Rigidbody m_rigidBody;

	public int PoolQueueID
	{
		get
		{
			return m_poolQueueID;
		}
		set
		{
			m_poolQueueID = value;
		}
	}

	public List<GameObjectPool.IPooledObject> PooledObjectCache => m_pooledObjectCache;

	public virtual bool AlwaysPool => false;

	public void CacheComponents()
	{
		GetComponents(m_pooledObjectCache);
		m_rigidBody = GetComponent<Rigidbody>();
	}

	public virtual void Despawn(float time = 0f)
	{
		if (time <= 0f)
		{
			if (m_rigidBody != null)
			{
				m_rigidBody.velocity = Vector3.zero;
				m_rigidBody.angularVelocity = Vector3.zero;
			}
			base.gameObject.SetActive(value: false);
		}
		else if (time > 0f)
		{
			if (m_timerCoroutine == null && (bool)base.gameObject)
			{
				m_timerCoroutine = StartCoroutine(DespawnTimer(time));
				return;
			}
			StopCoroutine(m_timerCoroutine);
			m_timerCoroutine = StartCoroutine(DespawnTimer(time));
		}
	}

	public virtual void Spawn(float time = 0f, bool active = true)
	{
		if (active)
		{
			base.gameObject.SetActive(value: true);
		}
		if (time > 0f)
		{
			m_timerCoroutine = StartCoroutine(DespawnTimer(time));
		}
	}

	protected IEnumerator DespawnTimer(float time)
	{
		float startTime = Time.timeSinceLevelLoad;
		while (Time.timeSinceLevelLoad - startTime < time)
		{
			yield return null;
		}
		Utils.DestroyWrap(base.gameObject);
		m_timerCoroutine = null;
	}
}
