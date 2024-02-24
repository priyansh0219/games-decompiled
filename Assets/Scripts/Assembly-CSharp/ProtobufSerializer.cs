using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ProtoBuf;
using ProtoBuf.Meta;
using UWE;
using UnityEngine;

public class ProtobufSerializer
{
	[ProtoContract]
	public class StreamHeader
	{
		[ProtoMember(1)]
		public int Signature { get; set; }

		[ProtoMember(2)]
		public int Version { get; set; }

		public void Reset()
		{
			Signature = 0;
			Version = 0;
		}

		public override string ToString()
		{
			return $"(UniqueIdentifier={Signature}, Version={Version})";
		}
	}

	[ProtoContract]
	public class LoopHeader
	{
		[ProtoMember(1)]
		public int Count { get; set; }

		public void Reset()
		{
			Count = 0;
		}

		public override string ToString()
		{
			return $"(Count={Count})";
		}
	}

	[ProtoContract]
	public class GameObjectData
	{
		[ProtoMember(1)]
		public bool CreateEmptyObject { get; set; }

		[ProtoMember(2)]
		public bool IsActive { get; set; }

		[ProtoMember(3)]
		public int Layer { get; set; }

		[ProtoMember(4)]
		public string Tag { get; set; }

		[ProtoMember(6)]
		public string Id { get; set; }

		[ProtoMember(7)]
		public string ClassId { get; set; }

		[ProtoMember(8)]
		public string Parent { get; set; }

		[ProtoMember(9)]
		public bool OverridePrefab { get; set; }

		[ProtoMember(10)]
		public bool MergeObject { get; set; }

		public void Reset()
		{
			CreateEmptyObject = false;
			IsActive = false;
			Layer = 0;
			Tag = null;
			Id = null;
			ClassId = null;
			Parent = null;
			OverridePrefab = false;
			MergeObject = false;
		}

		public override string ToString()
		{
			return $"(CreateEmptyObject={CreateEmptyObject}, IsActive={IsActive}, Layer={Layer}, Tag={Tag}, Id={Id}, ClassId={ClassId}, Parent={Parent}, OverridePrefab={OverridePrefab}, MergeObject={MergeObject})";
		}
	}

	[ProtoContract]
	public class ComponentHeader
	{
		[ProtoMember(1)]
		public string TypeName { get; set; }

		[ProtoMember(2)]
		public bool IsEnabled { get; set; }

		public void Reset()
		{
			TypeName = null;
			IsEnabled = false;
		}

		public override string ToString()
		{
			return $"(TypeName={TypeName}, IsEnabled={IsEnabled})";
		}
	}

	private static readonly ObjectPool<GameObjectData> gameObjectDataPool = ObjectPoolHelper.CreatePool<GameObjectData>();

	private static readonly ObjectPool<ComponentHeader> componentHeaderPool = ObjectPoolHelper.CreatePool<ComponentHeader>();

	private static readonly ObjectPool<LoopHeader> loopHeaderPool = ObjectPoolHelper.CreatePool<LoopHeader>();

	private static readonly ObjectPool<StreamHeader> streamHeaderPool = ObjectPoolHelper.CreatePool<StreamHeader>();

	private static readonly ArrayPool<int> componentIndicesPool = new ArrayPool<int>(4, 32);

	private static readonly ObjectPool<List<Component>> componentListPool = ObjectPoolHelper.CreatePool<List<Component>>();

	private static readonly ObjectPool<List<IProtoEventListener>> eventListenersPool = ObjectPoolHelper.CreatePool<List<IProtoEventListener>>();

	private static readonly ObjectPool<List<IProtoEventListenerAsync>> asyncEventListenersPool = ObjectPoolHelper.CreatePool<List<IProtoEventListenerAsync>>();

	private static readonly ObjectPool<List<IProtoTreeEventListener>> treeEventListenersPool = ObjectPoolHelper.CreatePool<List<IProtoTreeEventListener>>();

	private static readonly ObjectPool<List<UniqueIdentifier>> uniqueIdentifiersPool = ObjectPoolHelper.CreatePool<List<UniqueIdentifier>>();

	private static readonly ObjectPool<List<ChildObjectIdentifier>> childObjectIdentifiersPool = ObjectPoolHelper.CreatePool<List<ChildObjectIdentifier>>();

	private static readonly ObjectPool<Dictionary<Type, int>> componentCountersPool = ObjectPoolHelper.CreatePool<Dictionary<Type, int>>();

	private static readonly ObjectPool<TaskResult<GameObject>> taskResultPool = ObjectPoolHelper.CreatePool<TaskResult<GameObject>>();

	private static readonly Dictionary<string, Type> typeNameCache = new Dictionary<string, Type>();

	public static bool IsTestingPlayMode = false;

	private const int uniqueSignature = 1369164567;

	private const int currentVersion = 4;

	private static readonly StreamHeader currentStreamHeader = new StreamHeader
	{
		Signature = 1369164567,
		Version = 4
	};

	private readonly TypeModel model;

	private readonly Dictionary<Type, bool> canSerializeCache = new Dictionary<Type, bool>();

	private int lastDeserializedStreamHeaderVersion;

	private static GameObject emptyGameObjectPrefab;

	private static readonly HashSet<string> componentWhitelist = new HashSet<string> { "GrownPlant", "JointHelper", "Pickupable", "PickPrefab", "ReefbackPlant", "ReefbackCreature", "WaterParkCreature" };

	private static readonly HashSet<string> componentBlacklist = new HashSet<string>
	{
		"UnityEngine.BoxCollider", "UnityEngine.CapsuleCollider", "UnityEngine.SphereCollider", "Bioreactor", "Constructable", "CreatureBehaviour", "EnergyMixin", "LargeWorldEntity", "LiveMixin", "NuclearReactor",
		"PowerSource", "SwimToMeat", "Leviathan"
	};

	[Obsolete("Use serializerPool.Get() instead")]
	public ProtobufSerializer()
	{
		model = new ProtobufSerializerPrecompiled();
	}

	public void SerializeStreamHeader(Stream stream)
	{
		Serialize(stream, currentStreamHeader);
	}

	public bool TryDeserializeStreamHeader(Stream stream)
	{
		_ = StopwatchProfiler.Instance;
		int version;
		int signature;
		using (PooledObject<StreamHeader> pooledObject = streamHeaderPool.GetProxy())
		{
			StreamHeader value = pooledObject.Value;
			value.Reset();
			try
			{
				model.DeserializeWithLengthPrefix(stream, value, typeof(StreamHeader), PrefixStyle.Base128, 0);
				version = value.Version;
				signature = value.Signature;
			}
			catch (ProtoException)
			{
				return false;
			}
		}
		if (signature != 1369164567)
		{
			return false;
		}
		if (version < 0 || version > 4)
		{
			return false;
		}
		lastDeserializedStreamHeaderVersion = version;
		return true;
	}

	public IEnumerator SerializeObjectTreeAsync(Stream stream, GameObject root, bool beforeDestroy = false)
	{
		_ = StopwatchProfiler.Instance;
		using (PooledList<IProtoTreeEventListener> pooledList = treeEventListenersPool.GetListProxy())
		{
			List<IProtoTreeEventListener> value = pooledList.Value;
			root.GetComponentsInChildren(includeInactive: true, value);
			for (int i = 0; i < value.Count; i++)
			{
				IProtoTreeEventListener protoTreeEventListener = value[i];
				UnityEngine.Object @object = protoTreeEventListener as UnityEngine.Object;
				if ((bool)@object)
				{
					try
					{
						protoTreeEventListener.OnProtoSerializeObjectTree(this);
					}
					catch (Exception exception)
					{
						Debug.LogException(exception, @object);
					}
				}
			}
		}
		using (PooledList<UniqueIdentifier> uidsProxy = uniqueIdentifiersPool.GetListProxy())
		{
			List<UniqueIdentifier> uids = uidsProxy.Value;
			root.GetComponentsInChildren(includeInactive: true, uids);
			yield return SerializeObjectsAsync(stream, uids, storeParent: true);
			if (beforeDestroy)
			{
				for (int j = 0; j < uids.Count; j++)
				{
					uids[j].BeforeDestroy();
				}
			}
		}
	}

	private static bool IsChildObjectIdentifier(UniqueIdentifier uid)
	{
		return uid is ChildObjectIdentifier;
	}

	public IEnumerator SerializeObjectsAsync(Stream stream, IList<UniqueIdentifier> uids, bool storeParent)
	{
		_ = StopwatchProfiler.Instance;
		using (PooledObject<LoopHeader> pooledObject = loopHeaderPool.GetProxy())
		{
			LoopHeader value = pooledObject.Value;
			value.Reset();
			value.Count = uids.Count;
			Serialize(stream, value);
		}
		for (int i = 0; i < uids.Count; i++)
		{
			UniqueIdentifier uniqueIdentifier = uids[i];
			if (!uniqueIdentifier)
			{
				uniqueIdentifier = CreateTemporaryGameObject("DESTROYED OBJECT");
			}
			SerializeGameObject(stream, uniqueIdentifier, storeParent);
			yield return null;
		}
	}

	public void SerializeGameObject(Stream stream, UniqueIdentifier uid, bool storeParent)
	{
		_ = StopwatchProfiler.Instance;
		GameObject gameObject = uid.gameObject;
		if (string.IsNullOrEmpty(uid.Id))
		{
			Debug.LogErrorFormat(uid, "serializing object '{0}' with empty id", uid.name);
			uid.Id = Guid.NewGuid().ToString();
		}
		using (PooledList<IProtoEventListener> pooledList = eventListenersPool.GetListProxy())
		{
			List<IProtoEventListener> value = pooledList.Value;
			gameObject.GetComponents(value);
			for (int i = 0; i < value.Count; i++)
			{
				IProtoEventListener protoEventListener = value[i];
				try
				{
					protoEventListener.OnProtoSerialize(this);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception, protoEventListener as UnityEngine.Object);
				}
				finally
				{
				}
			}
		}
		using (PooledObject<GameObjectData> pooledObject = gameObjectDataPool.GetProxy())
		{
			GameObjectData value2 = pooledObject.Value;
			value2.Reset();
			value2.CreateEmptyObject = uid.ShouldCreateEmptyObject();
			value2.MergeObject = uid.ShouldMergeObject();
			value2.IsActive = gameObject.activeSelf;
			value2.Layer = gameObject.layer;
			value2.Tag = gameObject.tag;
			value2.Id = uid.Id;
			value2.ClassId = GetClassId(uid, uid.ShouldStoreClassId());
			value2.Parent = GetParentId(uid, storeParent);
			value2.OverridePrefab = uid.ShouldOverridePrefab();
			Serialize(stream, value2);
		}
		List<Component> list = componentListPool.Get();
		gameObject.GetComponents(list);
		int[] array = componentIndicesPool.Get(list.Count);
		int num = 0;
		for (int j = 0; j < list.Count; j++)
		{
			if (ShouldSerialize(uid, list[j]))
			{
				array[num] = j;
				num++;
			}
		}
		LoopHeader loopHeader = loopHeaderPool.Get();
		loopHeader.Reset();
		loopHeader.Count = num;
		Serialize(stream, loopHeader);
		loopHeaderPool.Return(loopHeader);
		for (int k = 0; k < num; k++)
		{
			int index = array[k];
			Component component = list[index];
			Type type = component.GetType();
			ComponentHeader componentHeader = componentHeaderPool.Get();
			componentHeader.Reset();
			componentHeader.TypeName = type.FullName;
			componentHeader.IsEnabled = GetIsEnabled(component);
			Serialize(stream, componentHeader);
			componentHeaderPool.Return(componentHeader);
			Serialize(stream, component, type);
		}
		list.Clear();
		componentListPool.Return(list);
		componentIndicesPool.Return(array);
	}

	public CoroutineTask<GameObject> DeserializeObjectTreeAsync(Stream stream, bool forceInactiveRoot, bool allowSpawnRestrictions, int verbose)
	{
		TaskResult<GameObject> result = new TaskResult<GameObject>();
		return new CoroutineTask<GameObject>(DeserializeObjectTreeAsync(stream, forceInactiveRoot, verbose, allowSpawnRestrictions, result), result);
	}

	private IEnumerator DeserializeObjectTreeAsync(Stream stream, bool forceInactiveRoot, int verbose, bool allowSpawnRestrictions, IOut<GameObject> result)
	{
		CoroutineTask<GameObject> task = DeserializeObjectsAsync(stream, null, forceInactiveRoot, forceParent: false, null, allowSpawnRestrictions, verbose);
		yield return task;
		try
		{
			GameObject result2 = task.GetResult();
			result.Set(result2);
			if (!result2)
			{
				yield break;
			}
			using (PooledList<IProtoTreeEventListener> pooledList = treeEventListenersPool.GetListProxy())
			{
				List<IProtoTreeEventListener> value = pooledList.Value;
				result2.GetComponentsInChildren(includeInactive: true, value);
				for (int i = 0; i < value.Count; i++)
				{
					IProtoTreeEventListener protoTreeEventListener = value[i];
					UnityEngine.Object @object = protoTreeEventListener as UnityEngine.Object;
					if ((bool)@object)
					{
						try
						{
							protoTreeEventListener.OnProtoDeserializeObjectTree(this);
						}
						catch (Exception exception)
						{
							Debug.LogException(exception, @object);
						}
					}
				}
			}
		}
		finally
		{
		}
	}

	public CoroutineTask<GameObject> DeserializeObjectsAsync(Stream stream, UniqueIdentifier rootUid, bool forceInactiveRoot, bool forceParent, Transform parent, bool allowSpawnRestrictions, int verbose)
	{
		TaskResult<GameObject> result = new TaskResult<GameObject>();
		return new CoroutineTask<GameObject>(DeserializeObjectsAsync(stream, rootUid, forceInactiveRoot, forceParent, parent, allowSpawnRestrictions, verbose, result), result);
	}

	private IEnumerator DeserializeObjectsAsync(Stream stream, UniqueIdentifier rootUid, bool forceInactiveRoot, bool forceParent, Transform parent, bool allowSpawnRestrictions, int verbose, IOut<GameObject> result)
	{
		_ = StopwatchProfiler.Instance;
		LoopHeader loopHeader = loopHeaderPool.Get();
		loopHeader.Reset();
		Deserialize(stream, loopHeader, verbose > 3);
		int gameObjectCount = loopHeader.Count;
		loopHeaderPool.Return(loopHeader);
		TaskResult<UniqueIdentifier> prefabInstantiateResult = new TaskResult<UniqueIdentifier>();
		GameObjectData gameObjectData = gameObjectDataPool.Get();
		for (int i = 0; i < gameObjectCount; i++)
		{
			gameObjectData.Reset();
			Deserialize(stream, gameObjectData, verbose > 0);
			if (allowSpawnRestrictions && !SpawnRestrictionEnforcer.ShouldSpawn(gameObjectData.ClassId))
			{
				SkipFullGameObjectDeserialization(stream, verbose);
				continue;
			}
			UniqueIdentifier uniqueIdentifier;
			if (i == 0 && rootUid != null)
			{
				uniqueIdentifier = rootUid;
			}
			else if (gameObjectData.CreateEmptyObject)
			{
				uniqueIdentifier = CreateEmptyGameObject("SerializerEmptyGameObject");
			}
			else if (gameObjectData.MergeObject)
			{
				uniqueIdentifier = FindChildObject(gameObjectData);
			}
			else
			{
				yield return InstantiatePrefabAsync(gameObjectData, prefabInstantiateResult);
				uniqueIdentifier = prefabInstantiateResult.Get();
			}
			if (i == 0)
			{
				GameObject gameObject = uniqueIdentifier.gameObject;
				if (forceInactiveRoot)
				{
					gameObject.SetActive(value: false);
				}
				result.Set(gameObject);
			}
			DeserializeIntoGameObject(stream, gameObjectData, uniqueIdentifier, forceInactiveRoot && i == 0, forceParent, parent, verbose);
			if (ShouldNotifyAsyncListeners(uniqueIdentifier))
			{
				yield return NotifyAsyncListenersAsync(uniqueIdentifier);
			}
		}
		gameObjectDataPool.Return(gameObjectData);
	}

	private void SkipFullGameObjectDeserialization(Stream stream, int verbose)
	{
		LoopHeader loopHeader = loopHeaderPool.Get();
		loopHeader.Reset();
		Deserialize(stream, loopHeader, verbose > 3);
		int count = loopHeader.Count;
		loopHeaderPool.Return(loopHeader);
		ComponentHeader obj = componentHeaderPool.Get();
		Dictionary<Type, int> dictionary = componentCountersPool.Get();
		dictionary.Clear();
		for (int i = 0; i < count; i++)
		{
			SkipDeserialize(stream);
			SkipDeserialize(stream);
		}
		dictionary.Clear();
		componentCountersPool.Return(dictionary);
		componentHeaderPool.Return(obj);
	}

	public IEnumerator DeserializeGameObjectAsync(Stream stream, UniqueIdentifier uid, bool forceParent, Transform parent, int verbose)
	{
		GameObjectData gameObjectData = gameObjectDataPool.Get();
		gameObjectData.Reset();
		Deserialize(stream, gameObjectData, verbose > 0);
		DeserializeIntoGameObject(stream, gameObjectData, uid, forceInactive: false, forceParent, parent, verbose);
		if (ShouldNotifyAsyncListeners(uid))
		{
			yield return NotifyAsyncListenersAsync(uid);
		}
		gameObjectDataPool.Return(gameObjectData);
	}

	private void DeserializeIntoGameObject(Stream stream, GameObjectData goData, UniqueIdentifier uid, bool forceInactive, bool forceParent, Transform parent, int verbose)
	{
		_ = StopwatchProfiler.Instance;
		GameObject gameObject = uid.gameObject;
		uid.Id = goData.Id;
		bool flag = goData.IsActive && !forceInactive;
		if (goData.CreateEmptyObject || goData.OverridePrefab)
		{
			gameObject.layer = goData.Layer;
			gameObject.tag = goData.Tag ?? "";
			if (!flag)
			{
				gameObject.SetActive(value: false);
			}
		}
		gameObject.transform.SetParent(forceParent ? parent : GetTransformById(goData.Parent), worldPositionStays: false);
		LoopHeader loopHeader = loopHeaderPool.Get();
		loopHeader.Reset();
		Deserialize(stream, loopHeader, verbose > 3);
		int count = loopHeader.Count;
		loopHeaderPool.Return(loopHeader);
		ComponentHeader componentHeader = componentHeaderPool.Get();
		Dictionary<Type, int> dictionary = componentCountersPool.Get();
		dictionary.Clear();
		for (int i = 0; i < count; i++)
		{
			componentHeader.Reset();
			Deserialize(stream, componentHeader, verbose > 1);
			bool isEnabled = componentHeader.IsEnabled;
			if (!string.IsNullOrEmpty(componentHeader.TypeName))
			{
				string typeName = componentHeader.TypeName;
				Type cachedType = GetCachedType(typeName);
				int id = IncrementComponentCounter(dictionary, cachedType);
				Component orAddComponent = GetOrAddComponent(gameObject, cachedType, typeName, id, goData.CreateEmptyObject);
				if (!orAddComponent)
				{
					Debug.LogWarningFormat(gameObject, "Serialized component '{0}' not found in game object", componentHeader.TypeName);
					SkipDeserialize(stream);
				}
				else
				{
					Deserialize(stream, orAddComponent, cachedType, verbose > 2);
				}
				SetIsEnabled(orAddComponent, isEnabled);
			}
		}
		dictionary.Clear();
		componentCountersPool.Return(dictionary);
		componentHeaderPool.Return(componentHeader);
		try
		{
			gameObject.SetActive(flag);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, gameObject);
		}
		using (PooledList<IProtoEventListener> pooledList = eventListenersPool.GetListProxy())
		{
			List<IProtoEventListener> value = pooledList.Value;
			gameObject.GetComponents(value);
			for (int j = 0; j < value.Count; j++)
			{
				IProtoEventListener protoEventListener = value[j];
				try
				{
					protoEventListener.OnProtoDeserialize(this);
				}
				catch (Exception exception2)
				{
					Debug.LogException(exception2, protoEventListener as UnityEngine.Object);
				}
				finally
				{
				}
			}
		}
	}

	private bool ShouldNotifyAsyncListeners(UniqueIdentifier uid)
	{
		IProtoEventListenerAsync component = null;
		return uid.gameObject.TryGetComponent<IProtoEventListenerAsync>(out component);
	}

	private IEnumerator NotifyAsyncListenersAsync(UniqueIdentifier uid)
	{
		_ = StopwatchProfiler.Instance;
		using (PooledList<IProtoEventListenerAsync> listenersProxy = asyncEventListenersPool.GetListProxy())
		{
			List<IProtoEventListenerAsync> listeners = listenersProxy.Value;
			uid.gameObject.GetComponents(listeners);
			TaskResult<Exception> exceptionResult = new TaskResult<Exception>();
			for (int i = 0; i < listeners.Count; i++)
			{
				IProtoEventListenerAsync listener = listeners[i];
				yield return CoroutineUtils.YieldSafe(listener.OnProtoDeserializeAsync(this), exceptionResult);
				Exception ex = exceptionResult.Get();
				exceptionResult.Set(null);
				if (ex != null)
				{
					Debug.LogException(ex, listener as UnityEngine.Object);
				}
			}
		}
	}

	private bool ShouldSerialize(UniqueIdentifier uid, Component comp)
	{
		if (!comp)
		{
			if (!PrefabDatabase.TryGetPrefabFilename(uid.ClassId, out var filename))
			{
				filename = uid.ClassId;
			}
			Debug.LogErrorFormat(uid, "A component referenced on prefab '{0}' is missing.", filename);
			return false;
		}
		if (comp is UniqueIdentifier)
		{
			return false;
		}
		Type type = comp.GetType();
		if (!CanSerialize(type))
		{
			return false;
		}
		if (!uid.ShouldSerialize(comp))
		{
			return false;
		}
		if (uid.ShouldCreateEmptyObject())
		{
			return true;
		}
		if (ProtobufSerializerPrecompiled.IsEmptyContract(type))
		{
			return false;
		}
		if (comp is IShouldSerialize shouldSerialize)
		{
			return shouldSerialize.ShouldSerialize();
		}
		return true;
	}

	public bool CanSerialize(Type type)
	{
		if (!canSerializeCache.TryGetValue(type, out var value))
		{
			value = model.CanSerialize(type);
			canSerializeCache[type] = value;
		}
		return value;
	}

	public void Serialize<T>(Stream stream, T source)
	{
		Serialize(stream, source, typeof(T));
	}

	private void Serialize(Stream stream, object source, Type type)
	{
		try
		{
			_ = StopwatchProfiler.Instance;
			model.SerializeWithLengthPrefix(stream, source, type, PrefixStyle.Base128, 0);
		}
		catch (Exception ex)
		{
			Debug.LogErrorFormat(source as UnityEngine.Object, "Exception while serializing '{0}' (on '{1}'): {2}", type.FullName, source, ex);
			Debug.LogException(ex, source as UnityEngine.Object);
		}
	}

	public void Deserialize<T>(Stream stream, T target, bool verbose)
	{
		Deserialize(stream, target, typeof(T), verbose);
	}

	private void Deserialize(Stream stream, object target, Type type, bool verbose)
	{
		_ = StopwatchProfiler.Instance;
		long position = stream.Position;
		try
		{
			model.DeserializeWithLengthPrefix(stream, target, type, PrefixStyle.Base128, 0);
			if (verbose)
			{
				try
				{
					Debug.LogFormat("deserialized {0}", target);
					return;
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
					return;
				}
			}
		}
		catch (Exception ex)
		{
			if (lastDeserializedStreamHeaderVersion == 1 && type.IsSubclassOf(typeof(Collider)))
			{
				RuntimeTypeModel runtimeTypeModel = TypeModel.Create();
				runtimeTypeModel.Add(typeof(Vector3), applyDefaultBehaviour: false).Add("x", "y", "z");
				runtimeTypeModel.Add(typeof(BoxCollider), applyDefaultBehaviour: false).Add("isTrigger", "center", "size");
				runtimeTypeModel.Add(typeof(SphereCollider), applyDefaultBehaviour: false).Add("isTrigger", "center", "radius");
				Debug.LogWarningFormat(target as UnityEngine.Object, "Trying to deserialize '{0}' with old model", type.FullName);
				stream.Seek(position, SeekOrigin.Begin);
				runtimeTypeModel.DeserializeWithLengthPrefix(stream, target, type, PrefixStyle.Base128, 0);
			}
			else
			{
				Debug.LogErrorFormat(target as UnityEngine.Object, "Exception while deserializing '{0}' (on '{1}'): {2}", type.FullName, target, ex);
				Debug.LogException(ex, target as UnityEngine.Object);
			}
		}
	}

	public void SkipDeserialize(Stream stream)
	{
		try
		{
			int fieldNumber;
			int bytesRead;
			int num = ProtoReader.ReadLengthPrefix(stream, expectHeader: false, PrefixStyle.Base128, out fieldNumber, out bytesRead);
			stream.Seek(num, SeekOrigin.Current);
		}
		finally
		{
		}
	}

	private UniqueIdentifier CreateEmptyGameObject(string name)
	{
		if (!emptyGameObjectPrefab)
		{
			emptyGameObjectPrefab = Resources.Load<GameObject>("SerializerEmptyGameObject");
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(emptyGameObjectPrefab);
		gameObject.name = name;
		return gameObject.GetComponent<StoreInformationIdentifier>();
	}

	private static UniqueIdentifier CreateTemporaryGameObject(string name)
	{
		GameObject gameObject = new GameObject(name);
		gameObject.SetActive(value: false);
		UnityEngine.Object.Destroy(gameObject, 60f);
		return gameObject.AddComponent<TemporaryObjectIdentifier>();
	}

	private static UniqueIdentifier FindChildObject(GameObjectData gameObjectData)
	{
		if (!UniqueIdentifier.TryGetIdentifier(gameObjectData.Parent, out var uid))
		{
			return CreateTemporaryGameObject("MISSING PARENT OBJECT");
		}
		using (PooledList<ChildObjectIdentifier> pooledList = childObjectIdentifiersPool.GetListProxy())
		{
			List<ChildObjectIdentifier> value = pooledList.Value;
			Transform transform = uid.transform;
			uid.GetComponentsInChildren(includeInactive: true, value);
			for (int i = 0; i < value.Count; i++)
			{
				ChildObjectIdentifier childObjectIdentifier = value[i];
				if (childObjectIdentifier.transform.parent == transform && childObjectIdentifier.ClassId == gameObjectData.ClassId)
				{
					return childObjectIdentifier;
				}
			}
		}
		UniqueIdentifier uniqueIdentifier = CreateTemporaryGameObject("MISSING CHILD OBJECT");
		uniqueIdentifier.transform.SetParent(uid.transform, worldPositionStays: false);
		return uniqueIdentifier;
	}

	private static IEnumerator InstantiatePrefabAsync(GameObjectData gameObjectData, IOut<UniqueIdentifier> result)
	{
		if (string.IsNullOrEmpty(gameObjectData.ClassId))
		{
			UniqueIdentifier uniqueIdentifier = CreateTemporaryGameObject("MISSING ENTITY PREFAB");
			Debug.LogErrorFormat(uniqueIdentifier, "Missing class id on prefab {0}", gameObjectData);
			result.Set(uniqueIdentifier);
			yield break;
		}
		DebugDisplayTimer.Start();
		GameObject gameObject = null;
		string filename;
		if (ScenePrefabDatabase.TryGetScenePrefab(gameObjectData.ClassId, out var prefab))
		{
			gameObject = UnityEngine.Object.Instantiate(prefab);
		}
		else if (PrefabDatabase.TryGetPrefabFilename(gameObjectData.ClassId, out filename))
		{
			TaskResult<GameObject> taskResult = taskResultPool.Get();
			yield return AddressablesUtility.InstantiateAsync(filename, taskResult, null, Vector3.zero, Quaternion.identity, awake: false);
			gameObject = taskResult.Get();
			taskResultPool.Return(taskResult);
		}
		DebugDisplayTimer.Start();
		if (gameObject == null)
		{
			UniqueIdentifier uniqueIdentifier2 = CreateTemporaryGameObject("FAILED TO LOAD PREFAB");
			Debug.LogErrorFormat(uniqueIdentifier2, "Failed to load prefab {0}", gameObjectData);
			result.Set(uniqueIdentifier2);
			yield break;
		}
		UniqueIdentifier component = gameObject.GetComponent<UniqueIdentifier>();
		if (!component)
		{
			gameObject.name = "MISSING IDENTIFIER ON ENTITY PREFAB";
			TemporaryObjectIdentifier temporaryObjectIdentifier = gameObject.AddComponent<TemporaryObjectIdentifier>();
			Debug.LogErrorFormat(temporaryObjectIdentifier, "Failed to find identifier on prefab {0}", gameObjectData);
			result.Set(temporaryObjectIdentifier);
		}
		else
		{
			result.Set(component);
		}
	}

	private static void Prewarm(TypeModel model, Type type)
	{
		if (!model.CanSerialize(type))
		{
			Debug.LogWarningFormat("ProtobufSerializer can not serialize {0}", type);
		}
	}

	private static string GetClassId(UniqueIdentifier uid, bool useClassId)
	{
		if (!useClassId)
		{
			return null;
		}
		return uid.ClassId;
	}

	private static string GetParentId(UniqueIdentifier uid, bool useParent)
	{
		if (!useParent)
		{
			return null;
		}
		Transform parent = uid.transform.parent;
		if (!parent)
		{
			return null;
		}
		UniqueIdentifier component = parent.GetComponent<UniqueIdentifier>();
		if (!component)
		{
			return null;
		}
		return component.Id;
	}

	private static Transform GetTransformById(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			return null;
		}
		if (!UniqueIdentifier.TryGetIdentifier(id, out var uid))
		{
			Debug.LogErrorFormat("GetTransformById could not find '{0}' among {1} ids", id, UniqueIdentifier.DebugIdentifiers().Count);
			return null;
		}
		if (!uid)
		{
			Debug.LogErrorFormat("GetTransformById found '{0}' but the object got destroyed (ref null? {1})", id, (object)uid == null);
			return null;
		}
		return uid.transform;
	}

	private static Component GetComponent(GameObject go, Type type)
	{
		if (type == null)
		{
			return null;
		}
		return go.GetComponent(type);
	}

	private static Component GetOrAddComponent(GameObject go, Type type, string typeName, int id, bool allowAddAny)
	{
		if (type == null)
		{
			return null;
		}
		List<Component> list = componentListPool.Get();
		try
		{
			go.GetComponents(type, list);
			if (id < list.Count)
			{
				return list[id];
			}
			if (!allowAddAny && !componentWhitelist.Contains(typeName))
			{
				if (componentBlacklist.Contains(typeName))
				{
					return CreateTemporaryGameObject("BLACKLISTED COMPONENT").gameObject.AddComponent(type);
				}
				Debug.LogWarningFormat(go, "Adding missing component '{0}' (#{1}) to '{2}'", type.FullName, id, go.name);
			}
			StopwatchProfiler.GetCachedProfilerTag("AddComponent-", typeName);
			return go.AddComponent(type);
		}
		finally
		{
			list.Clear();
			componentListPool.Return(list);
		}
	}

	private static bool GetIsEnabled(Component comp)
	{
		if (!(comp is Behaviour behaviour))
		{
			return true;
		}
		return behaviour.enabled;
	}

	private static void SetIsEnabled(Component comp, bool isEnabled)
	{
		if (comp is Behaviour behaviour)
		{
			behaviour.enabled = isEnabled;
		}
	}

	private static Type GetCachedType(string fullTypeName)
	{
		Type value = null;
		if (!typeNameCache.TryGetValue(fullTypeName, out value))
		{
			value = GetTypeEx(fullTypeName);
			typeNameCache.Add(fullTypeName, value);
		}
		return value;
	}

	private static Type GetTypeEx(string fullTypeName)
	{
		if (fullTypeName == "UnityEngine.Transform")
		{
			return typeof(Transform);
		}
		if (fullTypeName == "Oculus")
		{
			fullTypeName = "OculusFish";
		}
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		for (int i = 0; i < assemblies.Length; i++)
		{
			Type type = assemblies[i].GetType(fullTypeName);
			if (type != null)
			{
				return type;
			}
		}
		return null;
	}

	private static int IncrementComponentCounter(Dictionary<Type, int> componentCounters, Type type)
	{
		if (type == null)
		{
			return 0;
		}
		int orDefault = componentCounters.GetOrDefault(type, 0);
		componentCounters[type] = orDefault + 1;
		return orDefault;
	}
}
