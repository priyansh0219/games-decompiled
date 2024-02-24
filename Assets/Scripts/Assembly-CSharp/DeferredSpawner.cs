using System.Collections;
using System.Collections.Generic;
using UWE;
using UnityEngine;

public class DeferredSpawner : MonoBehaviour
{
	public abstract class Task : IEnumerator
	{
		protected readonly Transform parent;

		protected readonly Vector3 position;

		protected readonly Quaternion rotation;

		protected readonly bool instantiateActivated;

		private readonly MonoBehaviour owner;

		private readonly bool hasParent;

		protected bool forceCancelled;

		public bool cancelled
		{
			get
			{
				if (!(owner == null) && (!hasParent || !(parent == null)))
				{
					return forceCancelled;
				}
				return true;
			}
		}

		public object Current => null;

		public abstract GameObject GetResult();

		public Task(MonoBehaviour owner, Transform parent, Vector3 position, Quaternion rotation, bool instantiateActivated)
		{
			this.owner = owner;
			this.parent = parent;
			hasParent = parent != null;
			this.position = position;
			this.rotation = rotation;
			this.instantiateActivated = instantiateActivated;
		}

		public abstract void Spawn();

		public virtual bool MoveNext()
		{
			if (!cancelled)
			{
				return GetResult() == null;
			}
			return false;
		}

		public override string ToString()
		{
			return $"owner: {owner}/parent: {parent}/hasParent: {hasParent}/position: {position}/rotation: {rotation}/instantiateActivated: {instantiateActivated}";
		}

		public void Reset()
		{
		}
	}

	public class ManualTask : Task
	{
		private readonly GameObject prefab;

		private GameObject result;

		public ManualTask(MonoBehaviour owner, Transform parent, Vector3 position, Quaternion rotation, bool instantiateActivated, GameObject prefab)
			: base(owner, parent, position, rotation, instantiateActivated)
		{
			this.prefab = prefab;
		}

		public override GameObject GetResult()
		{
			return result;
		}

		public override void Spawn()
		{
			if (!base.cancelled)
			{
				result = EditorModifications.Instantiate(prefab, parent, position, rotation, instantiateActivated);
			}
		}

		public override string ToString()
		{
			return $"ManualTask prefab: {prefab.name}/" + base.ToString();
		}
	}

	public class AddressablesTask : Task
	{
		private readonly string key;

		private GameObject spawnedObject;

		public AddressablesTask(MonoBehaviour owner, Transform parent, Vector3 position, Quaternion rotation, bool instantiateActivated, string key)
			: base(owner, parent, position, rotation, instantiateActivated)
		{
			this.key = key;
		}

		public override GameObject GetResult()
		{
			return spawnedObject;
		}

		public override void Spawn()
		{
			if (!base.cancelled)
			{
				CoroutineHost.StartCoroutine(SpawnAsync());
			}
		}

		private IEnumerator SpawnAsync()
		{
			CoroutineTask<GameObject> task = AddressablesUtility.InstantiateAsync(key, parent, position, rotation, instantiateActivated);
			yield return task;
			spawnedObject = task.GetResult();
			if (spawnedObject == null)
			{
				forceCancelled = true;
			}
			HandleLateCancelledSpawn();
		}

		public override string ToString()
		{
			return $"AddressablesTask key: {key}/" + base.ToString();
		}

		private void HandleLateCancelledSpawn()
		{
			if (base.cancelled && (bool)spawnedObject)
			{
				Object.Destroy(spawnedObject);
				spawnedObject = null;
			}
		}
	}

	private readonly Queue<Task> lowPriorityQueue = new Queue<Task>();

	private readonly Queue<Task> highPriorityQueue = new Queue<Task>();

	private readonly int maxActiveSpawns = 2;

	private readonly List<Task> activeSpawns = new List<Task>();

	public static DeferredSpawner instance { get; private set; }

	public int InstantiateQueueCount => lowPriorityQueue.Count + highPriorityQueue.Count;

	private void Awake()
	{
		instance = this;
	}

	public void Reset()
	{
		lowPriorityQueue.Clear();
		highPriorityQueue.Clear();
		activeSpawns.Clear();
	}

	private void OnDestroy()
	{
		Reset();
		instance = null;
	}

	public Task InstantiateAsync(GameObject prefab, MonoBehaviour owner, Transform parent = null, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion), bool instantiateActivated = true, bool highPriority = false)
	{
		ManualTask manualTask = new ManualTask(owner, parent, position, rotation, instantiateActivated, prefab);
		(highPriority ? highPriorityQueue : lowPriorityQueue).Enqueue(manualTask);
		return manualTask;
	}

	public Task InstantiateAsync(string key, MonoBehaviour owner, Transform parent = null, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion), bool instantiateActivated = true, bool highPriority = false)
	{
		AddressablesTask addressablesTask = new AddressablesTask(owner, parent, position, rotation, instantiateActivated, key);
		(highPriority ? highPriorityQueue : lowPriorityQueue).Enqueue(addressablesTask);
		return addressablesTask;
	}

	private void Update()
	{
		CleanupCompletedSpawns();
		Process(highPriorityQueue);
		Process(lowPriorityQueue);
	}

	private void Process(Queue<Task> queue)
	{
		while (activeSpawns.Count < maxActiveSpawns && queue.Count > 0)
		{
			Task task = queue.Dequeue();
			if (task != null && !task.cancelled)
			{
				activeSpawns.Add(task);
				task.Spawn();
			}
		}
	}

	private void CleanupCompletedSpawns()
	{
		activeSpawns.RemoveAll((Task i) => i == null || !i.MoveNext());
	}
}
