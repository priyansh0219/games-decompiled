using System.Collections;
using System.Collections.Generic;
using Gendarme;
using UWE;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;

[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
public class MiniWorld : MonoBehaviour
{
	private struct Chunk
	{
		public AsyncOperationHandle<Mesh> handle;

		public GameObject gameObject;
	}

	[AssertNotNull]
	public Transform hologramHolder;

	[AssertNotNull]
	public Material hologramMaterial;

	public float hologramRadius = 1f;

	public float fadeRadius = 0.2f;

	public float fadeSharpness = 5f;

	public int mapWorldRadius = 20;

	private const int mapChunkSize = 32;

	private const int mapLOD = 2;

	public bool active = true;

	public bool updatePosition = true;

	private GameObject hologramObject;

	private float chunkScale;

	private Stack<GameObject> chunkPool = new Stack<GameObject>();

	private readonly string pooledName = "PooledHologramChunk";

	private readonly Dictionary<Int3, Chunk> loadedChunks = new Dictionary<Int3, Chunk>(Int3.equalityComparer);

	private Color mapColor;

	private Color mapColorNoAlpha;

	private HashSet<Int3> requestChunks = new HashSet<Int3>();

	private HashSet<Int3> disableChunks = new HashSet<Int3>();

	private Dictionary<Int3, string> chunkFilenameCache = new Dictionary<Int3, string>();

	private const int maxFilenameCacheSize = 500;

	public Material materialInstance { get; private set; }

	private float mapScale => hologramRadius / (float)mapWorldRadius;

	public void EnableMap()
	{
		active = true;
	}

	public void DisableMap()
	{
		active = false;
	}

	public void ToggleMap()
	{
		active = !active;
	}

	public void SetMapGlitchIntensity(float intensity)
	{
		materialInstance.SetFloat(ShaderPropertyID._GlitchIntensity, Mathf.Clamp01(intensity));
	}

	private void Start()
	{
		InitializeHologram();
	}

	private void InitializeHologram()
	{
		chunkScale = mapScale * 1f;
		materialInstance = Object.Instantiate(hologramMaterial);
		materialInstance.SetFloat(ShaderPropertyID._FadeRadius, fadeRadius);
		materialInstance.SetFloat(ShaderPropertyID._FadeSharpness, fadeSharpness);
		hologramObject = new GameObject("MiniWorld");
		hologramObject.transform.SetParent(hologramHolder, worldPositionStays: false);
		mapColor = materialInstance.GetColor(ShaderPropertyID._Color);
		mapColorNoAlpha = new Color(mapColor.r, mapColor.g, mapColor.b, 0f);
		CoroutineHost.StartCoroutine(RebuildHologram());
	}

	private void UpdatePosition()
	{
		hologramHolder.rotation = Quaternion.identity;
		materialInstance.SetVector(ShaderPropertyID._MapCenterWorldPos, base.transform.position);
		Vector3 vector = LargeWorldStreamer.main.land.transform.InverseTransformPoint(base.transform.position) / 4f;
		foreach (KeyValuePair<Int3, Chunk> loadedChunk in loadedChunks)
		{
			Chunk value = loadedChunk.Value;
			Vector3 vector2 = (loadedChunk.Key * 32).ToVector3() - vector;
			value.gameObject.transform.localPosition = vector2 * chunkScale;
		}
	}

	private void Update()
	{
		if (updatePosition)
		{
			UpdatePosition();
		}
		if (active)
		{
			SetColor(mapColor);
		}
		else
		{
			SetColor(mapColorNoAlpha);
		}
	}

	private void SetColor(Color goToColor)
	{
		Color value = Color.LerpUnclamped(materialInstance.GetColor(ShaderPropertyID._Color), goToColor, Time.deltaTime * 5f);
		materialInstance.SetColor(ShaderPropertyID._Color, value);
	}

	private void ClearUnusedChunks(HashSet<Int3> requestChunks)
	{
		foreach (KeyValuePair<Int3, Chunk> loadedChunk in loadedChunks)
		{
			if (!requestChunks.Contains(loadedChunk.Key))
			{
				disableChunks.Add(loadedChunk.Key);
			}
		}
		foreach (Int3 disableChunk in disableChunks)
		{
			DisableChunk(disableChunk);
		}
		disableChunks.Clear();
	}

	private void ClearAllChunks()
	{
		foreach (KeyValuePair<Int3, Chunk> loadedChunk in loadedChunks)
		{
			disableChunks.Add(loadedChunk.Key);
		}
		foreach (Int3 disableChunk in disableChunks)
		{
			DisableChunk(disableChunk);
		}
		disableChunks.Clear();
	}

	private void DisableChunk(Int3 chunkIndex)
	{
		Chunk chunk = loadedChunks[chunkIndex];
		GameObject gameObject = chunk.gameObject;
		gameObject.name = pooledName;
		gameObject.GetComponent<MeshRenderer>().enabled = false;
		MeshFilter component = gameObject.GetComponent<MeshFilter>();
		AddressablesUtility.QueueRelease(ref chunk.handle);
		component.sharedMesh = null;
		loadedChunks.Remove(chunkIndex);
		chunkPool.Push(gameObject);
	}

	private void GetOrMakeChunk(Int3 chunkId, AsyncOperationHandle<Mesh> handle, string chunkPath)
	{
		GameObject gameObject = null;
		MeshRenderer meshRenderer = null;
		MeshFilter meshFilter = null;
		if (chunkPool.Count > 0)
		{
			gameObject = chunkPool.Pop();
			gameObject.name = chunkPath;
			meshRenderer = gameObject.GetComponent<MeshRenderer>();
			meshRenderer.enabled = true;
			meshFilter = gameObject.GetComponent<MeshFilter>();
		}
		else
		{
			gameObject = new GameObject(chunkPath);
			gameObject.transform.SetParent(hologramObject.transform, worldPositionStays: false);
			gameObject.transform.localScale = new Vector3(chunkScale, chunkScale, chunkScale);
			meshFilter = gameObject.AddComponent<MeshFilter>();
			meshRenderer = gameObject.AddComponent<MeshRenderer>();
		}
		loadedChunks.Add(chunkId, new Chunk
		{
			handle = handle,
			gameObject = gameObject
		});
		meshFilter.sharedMesh = handle.Result;
		meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
		meshRenderer.sharedMaterial = materialInstance;
		meshRenderer.receiveShadows = false;
	}

	private bool GetChunkExists(Int3 requestChunk)
	{
		if (loadedChunks.ContainsKey(requestChunk))
		{
			return true;
		}
		return false;
	}

	private string GetChunkFilename(Int3 chunkId)
	{
		if (!chunkFilenameCache.TryGetValue(chunkId, out var value))
		{
			if (chunkFilenameCache.Count == 500)
			{
				chunkFilenameCache.Clear();
			}
			value = $"WorldMeshes/Mini2/Chunk-{chunkId.x}-{chunkId.y}-{chunkId.z}.asset";
			chunkFilenameCache.Add(chunkId, value);
		}
		return value;
	}

	private IEnumerator RebuildHologram()
	{
		MiniWorld instance = this;
		bool isPickupable = GetComponentInParent<Pickupable>() != null;
		while (!(instance == null))
		{
			if (!instance.gameObject.activeInHierarchy || (isPickupable && GetComponentInParent<Player>() == null))
			{
				ClearAllChunks();
			}
			else if (base.gameObject.activeInHierarchy)
			{
				Int3 block = LargeWorldStreamer.main.GetBlock(base.transform.position);
				Int3 @int = block - mapWorldRadius;
				Int3 int2 = block + mapWorldRadius;
				_ = block >> 2;
				Int3 mins = (@int >> 2) / 32;
				Int3 maxs = (int2 >> 2) / 32;
				bool chunkAdded = false;
				Int3.RangeEnumerator iter = Int3.Range(mins, maxs);
				while (iter.MoveNext())
				{
					Int3 chunkId = iter.Current;
					requestChunks.Add(chunkId);
					if (GetChunkExists(chunkId))
					{
						continue;
					}
					string chunkPath = GetChunkFilename(chunkId);
					if (AddressablesUtility.Exists<Mesh>(chunkPath))
					{
						AsyncOperationHandle<Mesh> request = AddressablesUtility.LoadAsync<Mesh>(chunkPath);
						yield return request;
						if (instance == null)
						{
							AddressablesUtility.QueueRelease(ref request);
							yield break;
						}
						if (request.Status != AsyncOperationStatus.Failed && !GetChunkExists(chunkId))
						{
							GetOrMakeChunk(chunkId, request, chunkPath);
							chunkAdded = true;
						}
					}
				}
				ClearUnusedChunks(requestChunks);
				requestChunks.Clear();
				if (chunkAdded)
				{
					UpdatePosition();
				}
			}
			yield return new WaitForSeconds(1f);
		}
	}

	private void OnDestroy()
	{
		if (GameApplication.isQuitting)
		{
			return;
		}
		foreach (KeyValuePair<Int3, Chunk> loadedChunk in loadedChunks)
		{
			Chunk value = loadedChunk.Value;
			AddressablesUtility.QueueRelease(ref value.handle);
		}
		loadedChunks.Clear();
		Object.Destroy(materialInstance);
	}
}
