using System.Collections.Generic;
using UnityEngine;

namespace UWE
{
	public static class GameObjectPool
	{
		public interface IPooledObject
		{
			void Spawn(float time = 0f, bool active = true);

			void Despawn(float time = 0f);
		}

		public class QueueInfo
		{
			public Transform parent;

			public Queue<GameObject> queue;

			public string name;

			public int id;

			public int instanceCount;

			public int hitCount;

			public float lastHitTime;

			public string prefabId;

			public LinkedListNode<QueueInfo> listNode;
		}

		public const string ObjectPoolAssetPath = "Assets/AddressableResources/GameObjectPools.asset";

		private const int skMinObjectForPool = 5;

		private const float skStalePoolTime = 120f;

		private static Dictionary<int, QueueInfo> sPoolMap = new Dictionary<int, QueueInfo>();

		private static int sHitCount;

		private static int sInstanceCount;

		private static bool sDumpEnabled = true;

		private static GameObject sPoolParentObject;

		private static int sCurrentObjectCount = 0;

		private static List<IPooledObject> sTempObjCache = new List<IPooledObject>();

		private static List<PooledMonoBehaviour> sTempComponentCache = new List<PooledMonoBehaviour>();

		private static LinkedList<QueueInfo> sQueueList = new LinkedList<QueueInfo>();

		public static Dictionary<int, QueueInfo> PoolMap => sPoolMap;

		public static int HitCount
		{
			get
			{
				return sHitCount;
			}
			set
			{
				sHitCount = value;
			}
		}

		public static int InstanceCount => sInstanceCount;

		public static bool DumpEnabled
		{
			get
			{
				return sDumpEnabled;
			}
			set
			{
				sDumpEnabled = value;
			}
		}

		public static GameObject Instantiate(GameObject pooledInstancePrefab, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion), bool active = true)
		{
			GameObject gameObject = null;
			CheckPoolValid();
			PooledMonoBehaviour component = pooledInstancePrefab.GetComponent<PooledMonoBehaviour>();
			int num = Animator.StringToHash(pooledInstancePrefab.name);
			if (component != null)
			{
				PooledMonoBehaviour pooledMonoBehaviour = null;
				QueueInfo value = null;
				if (sPoolMap.TryGetValue(num, out value) && value.queue.Count > 0)
				{
					gameObject = value.queue.Dequeue();
					while (gameObject == null && value.queue.Count > 0)
					{
						gameObject = value.queue.Dequeue();
					}
					if (gameObject != null)
					{
						sHitCount++;
						value.hitCount++;
						value.lastHitTime = Time.timeSinceLevelLoad;
						sQueueList.Remove(value.listNode);
						sQueueList.AddFirst(value);
						value.listNode = sQueueList.First;
						pooledMonoBehaviour = gameObject.GetComponent<PooledMonoBehaviour>();
					}
				}
				if (gameObject == null)
				{
					gameObject = CreateNewInstance(pooledInstancePrefab, position, rotation, active);
					pooledMonoBehaviour = gameObject.GetComponent<PooledMonoBehaviour>();
					if (pooledMonoBehaviour != null && (pooledMonoBehaviour.AlwaysPool || GameObjectPoolPrefabMap.Map.Count == 0 || GameObjectPoolPrefabMap.Map.ContainsKey(num)))
					{
						sInstanceCount++;
						pooledMonoBehaviour.PoolQueueID = num;
						pooledMonoBehaviour.CacheComponents();
						if (value != null)
						{
							value.instanceCount++;
							value.lastHitTime = Time.timeSinceLevelLoad;
							value.parent.SetAsFirstSibling();
							sQueueList.Remove(value.listNode);
							sQueueList.AddFirst(value);
							value.listNode = sQueueList.First;
						}
						else
						{
							string name = pooledMonoBehaviour.gameObject.name.Replace("(Clone)", string.Empty);
							PrefabIdentifier component2 = pooledInstancePrefab.GetComponent<PrefabIdentifier>();
							value = new QueueInfo();
							value.instanceCount = 1;
							value.name = name;
							value.id = num;
							value.queue = new Queue<GameObject>();
							value.parent = new GameObject(value.name).transform;
							value.parent.gameObject.SetActive(value: false);
							value.parent.parent = sPoolParentObject.transform;
							value.lastHitTime = Time.timeSinceLevelLoad;
							value.prefabId = ((component2 != null) ? component2.ClassId : value.name);
							sPoolMap[num] = value;
							sQueueList.AddFirst(value);
							value.listNode = sQueueList.First;
						}
					}
				}
				gameObject.transform.SetParent(null, worldPositionStays: false);
				gameObject.transform.position = position;
				gameObject.transform.rotation = rotation;
				if (pooledMonoBehaviour != null && pooledMonoBehaviour.PooledObjectCache != null)
				{
					for (int i = 0; i < pooledMonoBehaviour.PooledObjectCache.Count; i++)
					{
						pooledMonoBehaviour.PooledObjectCache[i].Spawn(0f, active);
					}
				}
			}
			else
			{
				gameObject = CreateNewInstance(pooledInstancePrefab, position, rotation, active);
			}
			if (sQueueList.Last != null && sDumpEnabled && Time.timeSinceLevelLoad - sQueueList.Last.Value.lastHitTime > 120f)
			{
				QueueInfo value2 = sQueueList.Last.Value;
				sPoolMap.Remove(value2.id);
				sQueueList.Remove(value2.listNode);
				Object.Destroy(value2.parent.gameObject);
			}
			return gameObject;
		}

		private static GameObject CreateNewInstance(GameObject prefab, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion), bool active = true)
		{
			return EditorModifications.Instantiate(prefab, null, position, rotation, active);
		}

		public static void Return(GameObject Instance, float time = 0f)
		{
			PooledMonoBehaviour component = Instance.GetComponent<PooledMonoBehaviour>();
			Instance.GetComponentsInChildren(sTempComponentCache);
			for (int i = 0; i < sTempComponentCache.Count; i++)
			{
				Return(sTempComponentCache[i], time);
			}
			if (component == null)
			{
				Object.Destroy(Instance, time);
			}
		}

		public static void Return(PooledMonoBehaviour Instance, float time = 0f)
		{
			CheckPoolValid();
			if (Instance != null)
			{
				Instance.GetComponents(sTempObjCache);
				for (int i = 0; i < sTempObjCache.Count; i++)
				{
					sTempObjCache[i].Despawn(time);
				}
				if (time <= 0f)
				{
					if (Instance.PoolQueueID == -1 || !sPoolMap.TryGetValue(Instance.PoolQueueID, out var value))
					{
						Object.Destroy(Instance.gameObject);
						return;
					}
					value.queue.Enqueue(Instance.gameObject);
					value.lastHitTime = Time.timeSinceLevelLoad;
					Instance.transform.parent = value.parent;
				}
			}
			else
			{
				Debug.LogWarning("[GameObjectPool::Return] trying to return null component!");
			}
		}

		public static void ClearPools()
		{
			foreach (KeyValuePair<int, QueueInfo> item in sPoolMap)
			{
				while (item.Value.queue != null && item.Value.queue.Count > 0)
				{
					GameObject gameObject = item.Value.queue.Dequeue();
					if (gameObject != null && (bool)gameObject)
					{
						Object.Destroy(gameObject);
					}
				}
				if (item.Value.parent != null)
				{
					Object.Destroy(item.Value.parent.gameObject);
				}
			}
			sPoolMap.Clear();
			sQueueList.Clear();
			sInstanceCount = 0;
			Object.Destroy(sPoolParentObject);
			sPoolParentObject = null;
		}

		private static void CheckPoolValid()
		{
			if (sPoolParentObject == null)
			{
				sPoolParentObject = new GameObject("[ObjectPool]");
			}
		}
	}
}
