using System;
using System.Collections.Generic;
using System.IO;
using UWE;
using UnityEngine;

[Serializable]
public class Voxeland : MonoBehaviour, IVoxeland, ISerializationCallbackReceiver, ICompileTimeCheckable
{
	public enum BrushShape
	{
		Sphere = 0,
		Cube = 1,
		Mesh = 2,
		Count = 3
	}

	public interface DebugHandler
	{
		void BeginRebuild();

		void DrawPoint(Vector3 p, Color color);

		void DrawLine(Vector3 blockCenter, Vector3 isoPos, Color c);
	}

	public interface FaceCreator
	{
		void CreateFaces(IVoxelandChunk chunk);
	}

	[Serializable]
	public class UndoStep
	{
		public List<GameObject> props = new List<GameObject>();

		public bool hasCoords;

		public int sx;

		public int sy;

		public int sz;

		public int ex;

		public int ey;

		public int ez;

		public VoxelandUndoAction dataAction;

		public void Add(VoxelandCoords c)
		{
			if (!hasCoords)
			{
				sx = (ex = c.x);
				sy = (ey = c.y);
				sz = (ez = c.z);
				hasCoords = true;
			}
			else
			{
				sx = Mathf.Min(sx, c.x);
				sy = Mathf.Min(sy, c.y);
				sz = Mathf.Min(sz, c.z);
				ex = Mathf.Max(ex, c.x);
				ey = Mathf.Max(ey, c.y);
				ez = Mathf.Max(ez, c.z);
			}
		}

		public void RegisterProp(GameObject go)
		{
			props.Add(go);
		}

		public void PerformUndo()
		{
			for (int i = 0; i < props.Count; i++)
			{
				UnityEngine.Object.DestroyImmediate(props[i]);
			}
			props.Clear();
			if (dataAction != null)
			{
				dataAction.Perform();
			}
		}
	}

	[Serializable]
	public class EditBlock
	{
		public int x;

		public int y;

		public int z;

		public byte type;

		public byte density;

		public Int3 gridPos;

		public Array3<byte> typesGrid;

		public Array3<byte> densityGrid;
	}

	[Serializable]
	public struct RasterWorkspace
	{
		[NonSerialized]
		public Array3<byte> typesGrid;

		[NonSerialized]
		public Array3<byte> densityGrid;

		public Int3 size;

		public int EstimateBytes()
		{
			if (typesGrid == null)
			{
				return 0;
			}
			return typesGrid.Length + densityGrid.Length;
		}

		public void SetSize(int meshRes)
		{
			int num = meshRes + 6;
			SetSize(num, num, num);
		}

		public void SetSize(int sx, int sy, int sz)
		{
			size = new Int3(sx, sy, sz);
			if (typesGrid == null || typesGrid.sizeX < sx || typesGrid.sizeY < sy || typesGrid.sizeZ < sz)
			{
				typesGrid = new Array3<byte>(sx, sy, sz);
				densityGrid = new Array3<byte>(sx, sy, sz);
			}
			typesGrid.Clear();
			densityGrid.Clear();
		}
	}

	public enum PaintHeightMode
	{
		raise = 0,
		lower = 1,
		flatten = 2
	}

	[Serializable]
	public struct CloseChunk
	{
		public int cx;

		public int cy;

		public int cz;

		public float distSq;
	}

	[Serializable]
	public class BuildStats
	{
		public int numChunks;

		public int numLayers;

		public int numMeshVerts;

		public int numUniqueVerts;
	}

	public const int terrainLayer = 30;

	public static readonly Vector3 half3 = new Vector3(0.5f, 0.5f, 0.5f);

	public const int NumUndos = 30;

	[HideInInspector]
	public VoxelandData data;

	public bool localAO;

	public bool castShadows;

	public bool scaleToolToEdit;

	public Material opaqueMaterial;

	public string paletteResourceDir;

	public VoxelandBlockType[] types;

	public int selected;

	[NonSerialized]
	public Material[] typeSolidMaterials;

	public int chunkSize = 8;

	[NonSerialized]
	public Int3 meshMins = new Int3(-1);

	[NonSerialized]
	public Int3 meshMaxs = new Int3(-1);

	public int newChunkSize = 8;

	public int numChunksBuilt;

	public bool debugHiResColliders;

	public bool debugSkipRelax;

	public bool debugBlocky;

	public bool debugDensityPerpendicular;

	public bool debugCheckLevelSetInvariant;

	public float surfaceDensityValue;

	public bool debugAlwaysRebuildClosest;

	public bool debugVerbose;

	public bool debugSkipMeshUpload;

	public bool debugFreezeLOD;

	public int debugDownsampleLevels;

	public bool debugUploadNullGrass;

	public bool debugUseDummyMaterial;

	public Material debugDummyMaterial;

	public bool debugSolidColorMaterials;

	public bool debugOneType;

	public bool debugLogMeshing;

	public bool debugUseLQShader;

	public Shader debugLQShader;

	public bool skipHiRes;

	[NonSerialized]
	public ChunkState[] chunkWindow;

	public int chunkCountX;

	public int chunkCountY;

	public int chunkCountZ;

	public Transform chunkPrefab;

	public VoxelandHighlight highlight;

	public Material highlightMaterial;

	public int brushSize;

	public float noiseScale = 0.5f;

	public float densityAdd = 0.5f;

	public BrushShape brushShape;

	public VoxelandBrush brushAsset;

	public Vector3 brushRotationEuler;

	public float brushScaleY = 1f;

	public float brushFillRate = 1f;

	public bool displaceMode;

	public Int3 displaceOffset = new Int3(0, 1, 0);

	public bool replaceMode;

	public int replaceType;

	public bool grassMode;

	public bool grassPreciseMode = true;

	public VoxelandGrassType[] grassTypes = new VoxelandGrassType[0];

	public int selectedGrassType;

	public bool heightLockMode;

	public float cubeAngle;

	public int blurIterations = 5;

	public int legacyBlurIterations = 10;

	public bool smoothTweak;

	public bool freeze;

	public bool readOnly;

	public bool disableAutoSerialize;

	public bool playmodeEdit;

	public bool editingSelection;

	public bool disableGrass;

	public Material grassMaterial;

	public Color landAmbient = new Color(0.5f, 0.5f, 0.5f, 1f);

	public Color landSpecular = new Color(0f, 0f, 0f, 1f);

	public float landShininess;

	public float triplanarScale;

	public float landBakeAmbient;

	public bool ambient = true;

	public int ambientMargins = 5;

	public int ambientSpread = 4;

	public float normalsRandom;

	public VoxelandNormalsSmooth normalsSmooth = VoxelandNormalsSmooth.mesh;

	public bool guiData;

	public bool guiGenerate;

	public bool guiExport;

	public bool guiLod;

	public bool guiAmbient;

	public bool guiMaterials;

	public bool guiSettings;

	public bool guiRebuild = true;

	public float lightmapPadding = 0.1f;

	public bool saveMeshes;

	public bool hideChunks;

	public int lodDistance = 40;

	public bool dynamicRebuilding = true;

	public bool updateChunksEnabled = true;

	public bool generateCollider = true;

	[NonSerialized]
	public bool managedPalette;

	public bool visualizeNodes;

	public bool visualizeFill;

	public int ambientBlurDisplayLayer = 15;

	public int ambientBlurDisplayChunk = 5;

	public Ray oldAimRay;

	[NonSerialized]
	public const VoxelandBulkChunkBuilder overrideBuilder = null;

	[NonSerialized]
	public VoxelandRasterizer overrideRasterizer;

	[NonSerialized]
	public VoxelandEventHandler eventHandler;

	[NonSerialized]
	public int lastChunkFrame;

	public DebugHandler debugHandler;

	[NonSerialized]
	public FaceCreator faceCreator;

	[NonSerialized]
	public bool liveDataStale;

	[NonSerialized]
	private List<UndoStep> undoStack = new List<UndoStep>();

	[NonSerialized]
	private UndoStep currUndoStep;

	public static int[] offsetX = new int[7] { 0, 0, 0, 1, -1, 0, 0 };

	public static int[] offsetY = new int[7] { 0, 1, -1, 0, 0, 0, 0 };

	public static int[] offsetZ = new int[7] { 0, 0, 0, 0, 0, -1, 1 };

	private RasterWorkspace blurWS;

	public int lockedHeight;

	public float selectedHeight;

	private RaycastHit prevPostEditHit;

	private RasterWorkspace queryWS;

	[NonSerialized]
	public VoxelandCoords[] lastAimCoords;

	[NonSerialized]
	private VoxelandChunkWorkspace chunkWorkspace;

	[NonSerialized]
	public bool keepColliders;

	public bool antiAccumulating = true;

	public bool rebuildOnlyOnMouseUp = true;

	[NonSerialized]
	public List<CloseChunk> closestChunks = new List<CloseChunk>();

	public BuildStats stats = new BuildStats();

	private Int3 dataSize => new Int3(data.sizeX, data.sizeY, data.sizeZ);

	private Int3 maxBlock => dataSize - 1;

	bool IVoxeland.debugBlocky => debugBlocky;

	bool IVoxeland.debugLogMeshing => debugLogMeshing;

	bool IVoxeland.debugOneType => debugOneType;

	VoxelandNormalsSmooth IVoxeland.normalsSmooth => normalsSmooth;

	FaceCreator IVoxeland.faceCreator => faceCreator;

	VoxelandBlockType[] IVoxeland.types => types;

	Int3 IVoxeland.meshMins => meshMins;

	Int3 IVoxeland.meshMaxs => meshMaxs;

	VoxelandData IVoxeland.data => data;

	public static int GetTerrainLayerMask()
	{
		return 1073741824;
	}

	public bool IsUsingSharedPalette()
	{
		if (paletteResourceDir != null)
		{
			return paletteResourceDir != "";
		}
		return false;
	}

	public bool IsLimitedMeshing()
	{
		return meshMins.x != -1;
	}

	public static VoxelandBlockType[] LoadPaletteStatic(string path)
	{
		VoxelandBlockTypePrefab[] array = Resources.LoadAll<VoxelandBlockTypePrefab>(path);
		int num = 0;
		VoxelandBlockTypePrefab[] array2 = array;
		foreach (VoxelandBlockTypePrefab voxelandBlockTypePrefab in array2)
		{
			if (voxelandBlockTypePrefab.HasValidId())
			{
				num = Mathf.Max(num, voxelandBlockTypePrefab.globalId);
			}
		}
		VoxelandBlockType[] array3 = new VoxelandBlockType[num + 1];
		array3[0] = new VoxelandBlockType();
		array3[0].name = "Empty";
		array3[0].filled = false;
		array2 = array;
		foreach (VoxelandBlockTypePrefab voxelandBlockTypePrefab2 in array2)
		{
			if (voxelandBlockTypePrefab2.HasValidId())
			{
				int globalId = voxelandBlockTypePrefab2.globalId;
				if (array3[globalId] != null)
				{
					Debug.LogError("More than one block type assigned to a palette slot. Id = " + globalId + ". Aborting.");
					return null;
				}
				voxelandBlockTypePrefab2.blockType.name = voxelandBlockTypePrefab2.name;
				array3[globalId] = voxelandBlockTypePrefab2.blockType;
			}
		}
		return array3;
	}

	public void LoadPalette(string path)
	{
		types = LoadPaletteStatic(path);
	}

	public static byte[] CreateTypeConversionTable(VoxelandBlockType[] from, VoxelandBlockType[] to)
	{
		byte[] array = new byte[256];
		for (int i = 0; i < from.Length; i++)
		{
			array[i] = byte.MaxValue;
			if (from[i] == null)
			{
				continue;
			}
			VoxelandBlockType other = from[i];
			for (int j = 0; j < to.Length; j++)
			{
				if (to[j] != null && to[j].IsVisuallySame(other))
				{
					array[i] = (byte)j;
					break;
				}
			}
		}
		return array;
	}

	public float GetLocalBuildDistance()
	{
		return 10f / base.transform.localScale.x;
	}

	public int GetWindowRadius()
	{
		return Mathf.CeilToInt(GetLocalBuildDistance() / (float)chunkSize);
	}

	public int GetWindowLen()
	{
		return 2 * GetWindowRadius() + 1;
	}

	public void UpdateOpaqueMaterial()
	{
		if (!opaqueMaterial)
		{
			opaqueMaterial = new Material(ShaderManager.preloadedShaders.voxelandOpaque);
		}
	}

	public void OnEditorCreate()
	{
		UpdateOpaqueMaterial();
	}

	public VoxelandSerialData GetSerialData()
	{
		return GetComponent<VoxelandSerialData>();
	}

	public void UpdateData()
	{
		if (liveDataStale)
		{
			if (data == null)
			{
				data = ScriptableObject.CreateInstance<VoxelandData>();
			}
			if (GetSerialData() != null)
			{
				data.UnserializeFrom(GetSerialData(), this);
			}
			else
			{
				data.UnserializeOctrees();
			}
			liveDataStale = false;
		}
		else if (data == null)
		{
			Debug.LogError("Missing data on intialized Voxeland", base.gameObject);
		}
	}

	public void Awake()
	{
		if (grassMaterial != null)
		{
			grassMaterial = new Material(grassMaterial);
		}
		if (!Mathf.Approximately(base.transform.localScale.x, base.transform.localScale.y) || !Mathf.Approximately(base.transform.localScale.x, base.transform.localScale.z))
		{
			Debug.LogError("Scale is not uniform! Please keep it uniform.");
		}
	}

	public void Start()
	{
		if (MainCamera.camera == null)
		{
			Debug.Log("WARNING: no main camera..??");
		}
	}

	public bool CannotBuildDueToOverrideBuilder()
	{
		return false;
	}

	public void Update()
	{
		UpdateData();
		if (freeze || chunkWindow == null)
		{
			return;
		}
		Camera camera = MainCamera.camera;
		if (camera == null)
		{
			return;
		}
		if (updateChunksEnabled)
		{
			Vector3 position = MainCamera.camera.transform.position;
			Vector3 lsRef = base.transform.InverseTransformPoint(position);
			if (dynamicRebuilding)
			{
				Vector3 lsForward = base.transform.InverseTransformDirection(camera.transform.forward);
				UpdateChunks(lsRef, GetLocalBuildDistance(), (float)lodDistance / base.transform.localScale.x);
				BuildBestChunk(lsRef, lsForward);
			}
			else
			{
				UpdateChunks(lsRef, -1f, (float)lodDistance / base.transform.localScale.x);
			}
		}
		if (playmodeEdit)
		{
			Ray aimRay = MainCamera.camera.ScreenPointToRay(Input.mousePosition);
			bool num = Mathf.Approximately(aimRay.origin.x, oldAimRay.origin.x) && Mathf.Approximately(aimRay.origin.y, oldAimRay.origin.y) && Mathf.Approximately(aimRay.origin.z, oldAimRay.origin.z) && Mathf.Approximately(aimRay.direction.x, oldAimRay.direction.x) && Mathf.Approximately(aimRay.direction.y, oldAimRay.direction.y) && Mathf.Approximately(aimRay.direction.z, oldAimRay.direction.z);
			oldAimRay = aimRay;
			if (!num || Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))
			{
				Edit(aimRay, Input.GetMouseButtonDown(0), Input.GetMouseButton(0), Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift), Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl), Input.GetMouseButtonUp(0), blockyEdit: true);
			}
		}
	}

	public bool IsChunkUniform(int x, int y, int z)
	{
		return IsChunkUniform(new Int3(x, y, z), debugDownsampleLevels);
	}

	public bool IsChunkUniform(Int3 chunk, int downsamples)
	{
		return IsChunkUniform(chunk, downsamples, chunkSize, 3);
	}

	public Int3.Bounds GetChunkBounds(Int3 chunk, int downsamples, int size, int overlap)
	{
		overlap <<= downsamples;
		int num = size << downsamples;
		Int3 mins = chunk * num - overlap;
		Int3 maxs = (chunk + 1) * num + (overlap - 1);
		return new Int3.Bounds(mins, maxs).Clamp(Int3.zero, maxBlock);
	}

	public bool IsChunkDataReady(Int3 chunk, int downsamples, int size, int overlap)
	{
		if (overrideRasterizer != null)
		{
			Int3.Bounds chunkBounds = GetChunkBounds(chunk, downsamples, size, overlap);
			return overrideRasterizer.IsRangeLoaded(chunkBounds, downsamples);
		}
		return true;
	}

	public bool IsChunkUniform(Int3 chunk, int downsamples, int size, int overlap)
	{
		Int3.Bounds chunkBounds = GetChunkBounds(chunk, downsamples, size, overlap);
		if (overrideRasterizer != null)
		{
			return overrideRasterizer.IsRangeUniform(chunkBounds);
		}
		return data.IsRangeEmpty(chunkBounds.mins.x, chunkBounds.mins.y, chunkBounds.mins.z, chunkBounds.maxs.x, chunkBounds.maxs.y, chunkBounds.maxs.z);
	}

	public VoxelandChunk CreateChunk(int cx, int cy, int cz)
	{
		GameObject gameObject = new GameObject($"Chunk ({cx},{cy},{cz})");
		int num = 1 << debugDownsampleLevels;
		int num2 = chunkSize << debugDownsampleLevels;
		gameObject.transform.parent = base.transform;
		gameObject.transform.localPosition = new Vector3(cx * num2, cy * num2, cz * num2);
		gameObject.transform.localScale = new Vector3(num, num, num);
		gameObject.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		gameObject.layer = base.gameObject.layer;
		VoxelandChunk voxelandChunk = gameObject.AddComponent<VoxelandChunk>();
		chunkWindow[GetChunkWindowSlot(cx, cy, cz)].chunk = voxelandChunk;
		voxelandChunk.land = this;
		voxelandChunk.offsetX = cx * num2;
		voxelandChunk.offsetY = cy * num2;
		voxelandChunk.offsetZ = cz * num2;
		voxelandChunk.meshRes = chunkSize;
		voxelandChunk.downsamples = debugDownsampleLevels;
		voxelandChunk.surfaceDensityValue = surfaceDensityValue;
		voxelandChunk.generateCollider = generateCollider;
		voxelandChunk.cx = cx;
		voxelandChunk.cy = cy;
		voxelandChunk.cz = cz;
		if (hideChunks)
		{
			gameObject.hideFlags = HideFlags.HideInHierarchy;
			gameObject.transform.hideFlags = HideFlags.HideInHierarchy;
		}
		numChunksBuilt++;
		return voxelandChunk;
	}

	public void BeginUndoStep()
	{
	}

	public bool IsInUndoStep()
	{
		return currUndoStep != null;
	}

	public void EndUndoStep()
	{
	}

	public int GetNumUndos()
	{
		return undoStack.Count;
	}

	public void UndoSetBlocks()
	{
	}

	public void RegisterExternalUndoRange(int sx, int sy, int sz, int ex, int ey, int ez)
	{
	}

	public Int3.Bounds BoundsForCoords(VoxelandCoords[] coords)
	{
		Int3.Bounds result = new Int3.Bounds(coords[0].ToInt3(), coords[0].ToInt3());
		for (int i = 1; i < coords.Length; i++)
		{
			result = result.Union(coords[i].ToInt3());
		}
		return result;
	}

	public void StampMesh(Vector3 position, Vector3 scale, Vector3 eulerRotation, VoxelandBrush brush, bool addVoxels)
	{
		DistanceFieldGrid distanceFieldGrid = brush.CreateGrid(position, Quaternion.Euler(eulerRotation), scale, (byte)selected);
		Int3.Bounds bounds = distanceFieldGrid.GetBounds();
		VoxelandData.OctNode.BlendOp operation = ((!addVoxels) ? VoxelandData.OctNode.BlendOp.Subtraction : VoxelandData.OctNode.BlendOp.Union);
		VoxelandData.OctNode.BlendArgs blend = new VoxelandData.OctNode.BlendArgs(operation, replaceTypes: false, 0);
		RegisterExternalUndoRange(bounds.mins.x, bounds.mins.y, bounds.mins.z, bounds.maxs.x, bounds.maxs.y, bounds.maxs.z);
		data.SetForRange(distanceFieldGrid, bounds.mins.x, bounds.mins.y, bounds.mins.z, bounds.size.x, bounds.size.y, bounds.size.z, blend);
		data.CollapseRelevant(bounds.mins.x, bounds.mins.y, bounds.mins.z, bounds.maxs.x, bounds.maxs.y, bounds.maxs.z);
		OnRangeEditDone(bounds.mins.x, bounds.mins.y, bounds.mins.z, bounds.maxs.x, bounds.maxs.y, bounds.maxs.z);
	}

	public void SetSphere(Vector3 center, Vector3 radius, bool add)
	{
		int num = Mathf.FloorToInt(center.x - radius.x - 1f);
		int num2 = Mathf.FloorToInt(center.y - radius.y - 1f);
		int num3 = Mathf.FloorToInt(center.z - radius.z - 1f);
		int num4 = Mathf.CeilToInt(center.x + radius.x + 1f);
		int num5 = Mathf.CeilToInt(center.y + radius.y + 1f);
		int num6 = Mathf.CeilToInt(center.z + radius.z + 1f);
		if (num5 == num2)
		{
			num5++;
		}
		VoxelandData.OctNode.BlendArgs blend = new VoxelandData.OctNode.BlendArgs((!add) ? VoxelandData.OctNode.BlendOp.Subtraction : VoxelandData.OctNode.BlendOp.Union, add, 0);
		float num7 = (radius.x + radius.y + radius.z) / 3f;
		Vector3 b = new Vector3(num7 / radius.x, num7 / radius.y, num7 / radius.z);
		for (int i = num; i <= num4; i++)
		{
			for (int j = num2; j <= num5; j++)
			{
				for (int k = num3; k <= num6; k++)
				{
					if (data.CheckBounds(i, j, k))
					{
						float num8 = num7 - 0.01f - Vector3.Scale(new Vector3(i, j, k) + half3 - center, b).magnitude;
						VoxelandData.OctNode srcNode = new VoxelandData.OctNode((byte)((num8 >= 0f) ? ((uint)selected) : 0u), VoxelandData.OctNode.EncodeDensity(num8));
						data.BlendNode(i, j, k, srcNode, blend);
					}
				}
			}
		}
		data.CollapseRelevant(num, num2, num3, num4, num5, num6);
		OnRangeEditDone(num, num2, num3, num4, num5, num6);
	}

	private void SetCubeInternal(Vector3 center, Vector3 radius, VoxelandData.OctNode.BlendArgs blend, int blockType, int x0, int y0, int z0, int x1, int y1, int z1)
	{
		Bounds bounds = default(Bounds);
		bounds.SetMinMax(center - radius, center + radius);
		for (int i = x0; i <= x1; i++)
		{
			for (int j = y0; j <= y1; j++)
			{
				for (int k = z0; k <= z1; k++)
				{
					if (data.CheckBounds(i, j, k))
					{
						Vector3 vector = new Vector3(i, j, k) + half3;
						float num = VoxelandMisc.SignedDistToBox(bounds, center + Quaternion.AngleAxis(cubeAngle, Vector3.up) * (vector - center));
						VoxelandData.OctNode srcNode = new VoxelandData.OctNode((byte)((num >= 0f) ? ((uint)blockType) : 0u), VoxelandData.OctNode.EncodeDensity(num));
						data.BlendNode(i, j, k, srcNode, blend);
					}
				}
			}
		}
	}

	private void SetCubeInternal6(Vector3 center, Vector3 radius, VoxelandData.OctNode.BlendArgs blend, int blockType, int cx, int cy, int cz)
	{
		Bounds bounds = default(Bounds);
		bounds.SetMinMax(center - radius, center + radius);
		for (int i = 0; i < 7; i++)
		{
			int num = cx + offsetX[i];
			int num2 = cy + offsetY[i];
			int num3 = cz + offsetZ[i];
			if (data.CheckBounds(num, num2, num3))
			{
				Vector3 vector = new Vector3(num, num2, num3) + half3;
				float num4 = VoxelandMisc.SignedDistToBox(bounds, center + Quaternion.AngleAxis(cubeAngle, Vector3.up) * (vector - center));
				VoxelandData.OctNode srcNode = new VoxelandData.OctNode((byte)((num4 >= 0f) ? ((uint)blockType) : 0u), VoxelandData.OctNode.EncodeDensity(num4));
				data.BlendNode(num, num2, num3, srcNode, blend);
			}
		}
	}

	private void SetCubeInternal0(VoxelandData.OctNode.BlendArgs blend, int blockType, int x, int y, int z)
	{
		if (data.CheckBounds(x, y, z))
		{
			VoxelandData.OctNode srcNode = new VoxelandData.OctNode((byte)blockType, 0);
			data.BlendNode(x, y, z, srcNode, blend);
		}
	}

	public void SetCube(Vector3 center, Vector3 radius, bool add)
	{
		int num = Mathf.FloorToInt(center.x - radius.x - 1f);
		int num2 = Mathf.FloorToInt(center.y - radius.y - 1f);
		int num3 = Mathf.FloorToInt(center.z - radius.z - 1f);
		int num4 = Mathf.CeilToInt(center.x + radius.x + 1f);
		int num5 = Mathf.CeilToInt(center.y + radius.y + 1f);
		int num6 = Mathf.CeilToInt(center.z + radius.z + 1f);
		if (num5 == num2)
		{
			num5++;
		}
		VoxelandData.OctNode.BlendArgs blend = new VoxelandData.OctNode.BlendArgs((!add) ? VoxelandData.OctNode.BlendOp.Subtraction : VoxelandData.OctNode.BlendOp.Union, add, 0);
		SetCubeInternal(center, radius, blend, selected, num, num2, num3, num4, num5, num6);
		data.CollapseRelevant(num, num2, num3, num4, num5, num6);
		OnRangeEditDone(num, num2, num3, num4, num5, num6);
	}

	public void SetCubes(VoxelandCoords[] coords, int type, bool add)
	{
		if (coords == null || coords.Length == 0)
		{
			return;
		}
		Int3.Bounds bounds = BoundsForCoords(coords);
		bounds.Expand(1);
		VoxelandData.OctNode.BlendArgs blend = new VoxelandData.OctNode.BlendArgs((!add) ? VoxelandData.OctNode.BlendOp.Subtraction : VoxelandData.OctNode.BlendOp.Union, add, 0);
		foreach (VoxelandCoords voxelandCoords in coords)
		{
			if (voxelandCoords.dir >= 0)
			{
				Vector3 center = voxelandCoords.GetCenter();
				SetCubeInternal6(center, half3, blend, selected, voxelandCoords.x, voxelandCoords.y, voxelandCoords.z);
			}
			else
			{
				SetCubeInternal0(blend, selected, voxelandCoords.x, voxelandCoords.y, voxelandCoords.z);
			}
		}
		data.CollapseRelevant(bounds.mins.x, bounds.mins.y, bounds.mins.z, bounds.maxs.x, bounds.maxs.y, bounds.maxs.z);
		OnRangeEditDone(bounds.mins.x, bounds.mins.y, bounds.mins.z, bounds.maxs.x, bounds.maxs.y, bounds.maxs.z);
	}

	public void AddLayer(VoxelandCoords[] coords, int type)
	{
		VoxelandData.OctNode.BlendArgs blend = new VoxelandData.OctNode.BlendArgs(VoxelandData.OctNode.BlendOp.Union, replaceTypes: false, 0);
		foreach (EditBlock item in EditCoords(coords, 1))
		{
			if (item.type > 0)
			{
				continue;
			}
			bool flag = false;
			foreach (Int3 item2 in item.gridPos.Get26Neighbors())
			{
				if (item.typesGrid[item2.x, item2.y, item2.z] > 0)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				SetCubeInternal(new Vector3(item.x, item.y, item.z) + half3, half3, blend, selected, item.x - 1, item.y - 1, item.z - 1, item.x + 1, item.y + 1, item.z + 1);
			}
		}
	}

	public void RemoveLayer(VoxelandCoords[] coords)
	{
		VoxelandData.OctNode.BlendArgs blend = new VoxelandData.OctNode.BlendArgs(VoxelandData.OctNode.BlendOp.Subtraction, replaceTypes: false, 0);
		foreach (EditBlock item in EditCoords(coords, 1))
		{
			if (item.type == 0)
			{
				continue;
			}
			bool flag = false;
			foreach (Int3 item2 in item.gridPos.Get26Neighbors())
			{
				if (item.typesGrid[item2.x, item2.y, item2.z] == 0)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				SetCubeInternal(new Vector3(item.x, item.y, item.z) + half3, half3, blend, selected, item.x - 1, item.y - 1, item.z - 1, item.x + 1, item.y + 1, item.z + 1);
			}
		}
	}

	public IEnumerable<EditBlock> EditCoords(VoxelandCoords[] coords, int dataMargin)
	{
		if (coords == null || coords.Length == 0)
		{
			yield break;
		}
		Int3.Bounds bounds = BoundsForCoords(coords);
		bounds.Expand(dataMargin);
		queryWS.SetSize(bounds.size.x, bounds.size.y, bounds.size.z);
		RasterizeVoxels(queryWS, bounds.mins.x, bounds.mins.y, bounds.mins.z, 0);
		EditBlock rv = new EditBlock();
		for (int i = 0; i < coords.Length; i++)
		{
			Int3 @int = coords[i].ToInt3();
			if (data.CheckBounds(@int.x, @int.y, @int.z))
			{
				Int3 gridPos = @int - bounds.mins;
				byte density = queryWS.densityGrid[gridPos.x, gridPos.y, gridPos.z];
				byte type = queryWS.typesGrid[gridPos.x, gridPos.y, gridPos.z];
				rv.x = @int.x;
				rv.y = @int.y;
				rv.z = @int.z;
				rv.type = type;
				rv.density = density;
				rv.gridPos = gridPos;
				rv.typesGrid = queryWS.typesGrid;
				rv.densityGrid = queryWS.densityGrid;
				yield return rv;
			}
		}
		CollapseForCoords(coords, 0);
		OnCoordsEditsDone(coords, 0);
	}

	public void PaintGrassAt(Vector3 point, Vector3 normal, VoxelandGrassType type)
	{
		if (type != null && (bool)type.grass)
		{
			Quaternion identity = Quaternion.identity;
			identity.SetFromToRotation(Vector3.up, normal);
			if (type.grassRandomSpin)
			{
				identity *= Quaternion.AngleAxis(360f * UnityEngine.Random.value, Vector3.up);
			}
			if (type.grassZUp)
			{
				identity *= Quaternion.AngleAxis(-90f, Vector3.right);
			}
			float num = Mathf.Lerp(type.grassMinScale, type.grassMaxScale, UnityEngine.Random.value);
			Vector3 position = point + normal * type.grassOffset;
			GameObject gameObject = UnityEngine.Object.Instantiate(type.grass, position, identity);
			gameObject.transform.localScale *= num;
		}
	}

	public void PaintGrass(VoxelandGrassType type, Vector3 point, float radius, float scaleY, BrushShape shape)
	{
		if (type == null || !type.grass)
		{
			return;
		}
		float num = Mathf.Cos((float)type.grassMaxTilt * ((float)System.Math.PI / 180f));
		float num2 = Mathf.Cos((float)type.grassMinTilt * ((float)System.Math.PI / 180f));
		for (int i = 0; i < chunkWindow.Length; i++)
		{
			ChunkState chunkState = chunkWindow[i];
			if (!chunkState.chunk || !chunkState.chunk.collision || LocalDistMaxToChunk(base.transform.InverseTransformPoint(point), chunkState.chunk) > radius)
			{
				continue;
			}
			Mesh sharedMesh = chunkState.chunk.collision.sharedMesh;
			Transform transform = chunkState.chunk.collision.transform;
			int[] triangles = sharedMesh.triangles;
			Vector3[] vertices = sharedMesh.vertices;
			for (int j = 0; j < triangles.Length; j += 6)
			{
				Vector3 vector = transform.TransformPoint(vertices[triangles[j]]);
				Vector3 vector2 = transform.TransformPoint(vertices[triangles[j + 1]]);
				Vector3 vector3 = transform.TransformPoint(vertices[triangles[j + 2]]);
				Vector3 vector4 = transform.TransformPoint(vertices[triangles[j + 5]]);
				Vector3 vector5 = (vector + vector2 + vector3 + vector4) / 4f;
				Vector3 vector6 = vector5 - point;
				vector6.y /= scaleY;
				switch (shape)
				{
				case BrushShape.Sphere:
					if (vector6.sqrMagnitude > radius * radius)
					{
						continue;
					}
					break;
				case BrushShape.Cube:
					if (Mathf.Abs(vector6.x) > radius || Mathf.Abs(vector6.y) > radius || Mathf.Abs(vector6.z) > radius)
					{
						continue;
					}
					break;
				}
				Vector3 vector7 = vector - vector3;
				Vector3 vector8 = vector2 - vector4;
				Vector3 normalized = Vector3.Cross(vector7, vector8).normalized;
				if (normalized.y < num || normalized.y > num2)
				{
					continue;
				}
				float grassDensity = type.grassDensity;
				if (type.perlinGrass)
				{
					if (Mathf.PerlinNoise(vector5.x / type.perlinPeriod, vector5.z / type.perlinPeriod) > grassDensity)
					{
						continue;
					}
				}
				else if (UnityEngine.Random.value > grassDensity)
				{
					continue;
				}
				float grassJitter = type.grassJitter;
				Vector3 vector9 = vector7 * grassJitter * (UnityEngine.Random.value - 0.5f) + vector8 * grassJitter * (UnityEngine.Random.value - 0.5f);
				PaintGrassAt(vector5 + vector9, normalized, type);
			}
		}
	}

	public void PaintBlocks(VoxelandCoords[] coords, byte type)
	{
		foreach (EditBlock item in EditCoords(coords, 0))
		{
			if (item.type != 0 && (!replaceMode || item.type == replaceType) && (!(brushFillRate < 1f) || !(UnityEngine.Random.value > brushFillRate)))
			{
				data.SetBlock(item.x, item.y, item.z, type, item.density, skipCollapse: true, threaded: false);
			}
		}
	}

	public void ReplaceType(int sx, int sy, int sz, int ex, int ey, int ez, int oldType, int newType)
	{
		if (IsInUndoStep())
		{
			EndUndoStep();
		}
		bool flag = antiAccumulating;
		antiAccumulating = false;
		BeginUndoStep();
		data.ReplaceType(sx, sy, sz, ex, ey, ez, (byte)oldType, (byte)newType);
		data.CollapseRelevant(sx, sy, sz, ex, ey, ez);
		RebuildRelevantChunks(sx, sy, sz, ex, ey, ez);
		EndUndoStep();
		antiAccumulating = flag;
	}

	public void ResetSmoothing(VoxelandCoords[] coords)
	{
		foreach (EditBlock item in EditCoords(coords, 0))
		{
			if (item.density > 0)
			{
				if (item.type > 0)
				{
					data.SetBlock(item.x, item.y, item.z, item.type, VoxelandData.OctNode.EncodeDensity(0.5f), skipCollapse: true, threaded: false);
				}
				else
				{
					data.SetBlock(item.x, item.y, item.z, item.type, VoxelandData.OctNode.EncodeDensity(-0.5f), skipCollapse: true, threaded: false);
				}
			}
		}
	}

	public void AddDensityNoise(VoxelandCoords[] coords)
	{
		foreach (EditBlock item in EditCoords(coords, 0))
		{
			if (item.density != 0)
			{
				float num = VoxelandData.OctNode.DecodeNearDensity(item.density);
				num += Mathf.Lerp(0f - noiseScale, noiseScale, UnityEngine.Random.value);
				data.SetBlock(item.x, item.y, item.z, item.type, VoxelandData.OctNode.EncodeDensity(num), skipCollapse: true, threaded: false);
			}
		}
	}

	public void AddDensity(VoxelandCoords[] coords, float amt)
	{
		foreach (EditBlock item in EditCoords(coords, 0))
		{
			if (item.density != 0)
			{
				float num = VoxelandData.OctNode.DecodeNearDensity(item.density);
				num += amt;
				data.SetBlock(item.x, item.y, item.z, item.type, VoxelandData.OctNode.EncodeDensity(num), skipCollapse: true, threaded: false);
			}
		}
	}

	public void RasterizeVoxels(RasterWorkspace ws, int wx0, int wy0, int wz0, int downsampleLevels)
	{
		if (overrideRasterizer != null)
		{
			overrideRasterizer.Rasterize(this, ws.typesGrid, ws.densityGrid, ws.size, wx0, wy0, wz0, downsampleLevels);
		}
		else
		{
			data.Rasterize(ws.typesGrid, ws.densityGrid, ws.size, wx0, wy0, wz0, downsampleLevels);
		}
	}

	public static void FixFarDensities(Array3<float> densityMatrix, Int3 size)
	{
		for (int i = 1; i < size.x - 1; i++)
		{
			for (int j = 1; j < size.y - 1; j++)
			{
				for (int k = 1; k < size.z - 1; k++)
				{
					bool flag = densityMatrix[i, j, k] >= 0f;
					bool flag2 = false;
					for (int l = 0; l < 6; l++)
					{
						int x = i + VoxelandChunk.VoxelandFace.dirToPosX[l];
						int y = j + VoxelandChunk.VoxelandFace.dirToPosY[l];
						int z = k + VoxelandChunk.VoxelandFace.dirToPosZ[l];
						bool flag3 = densityMatrix[x, y, z] >= 0f;
						if (flag != flag3)
						{
							flag2 = true;
							break;
						}
					}
					if (flag2)
					{
						densityMatrix[i, j, k] = (flag ? 1f : (-1f));
					}
				}
			}
		}
	}

	public static void NeutralizeBorderDensities(Array3<float> densityMatrix, Int3 size)
	{
		for (int i = 0; i < size.x; i++)
		{
			for (int j = 0; j < size.y; j++)
			{
				for (int k = 0; k < size.z; k++)
				{
					if ((i < 1 || i >= size.x - 1 || j < 1 || j >= size.y - 1 || k < 1 || k >= size.z - 1) && densityMatrix[i, j, k] == -1f)
					{
						densityMatrix[i, j, k] = 0f;
					}
				}
			}
		}
	}

	public static void CopyDensities(Array3<float> dest, Array3<float> source, Int3 size)
	{
		for (int i = 0; i < size.x; i++)
		{
			for (int j = 0; j < size.y; j++)
			{
				for (int k = 0; k < size.z; k++)
				{
					dest[i, j, k] = source[i, j, k];
				}
			}
		}
	}

	public void PaintHeight(VoxelandCoords[] coords, PaintHeightMode mode)
	{
		Int3.Bounds bounds = BoundsForCoords(coords);
		Int3 size = bounds.size;
		blurWS.SetSize(size.x, size.y, size.z);
		RasterizeVoxels(blurWS, bounds.mins.x, bounds.mins.y, bounds.mins.z, 0);
		byte[,] array = new byte[size.x, size.z];
		for (int i = 0; i < size.x; i++)
		{
			for (int j = 0; j < size.z; j++)
			{
				array[i, j] = (byte)selected;
				for (int num = size.y - 1; num >= 0; num--)
				{
					byte b = blurWS.typesGrid[i, num, j];
					if (b > 0)
					{
						array[i, j] = b;
						break;
					}
				}
			}
		}
		VoxelandData.OctNode.BlendArgs blend = new VoxelandData.OctNode.BlendArgs(VoxelandData.OctNode.BlendOp.Union, replaceTypes: true, 0);
		VoxelandData.OctNode.BlendArgs blend2 = new VoxelandData.OctNode.BlendArgs(VoxelandData.OctNode.BlendOp.Subtraction, replaceTypes: false, 0);
		foreach (VoxelandCoords voxelandCoords in coords)
		{
			Int3 @int = voxelandCoords.ToInt3() - bounds.mins;
			byte b2 = blurWS.typesGrid[@int.x, @int.y, @int.z];
			if (b2 == 0)
			{
				b2 = array[@int.x, @int.z];
			}
			Vector3 center = voxelandCoords.GetCenter();
			if (center.y <= selectedHeight)
			{
				if (mode == PaintHeightMode.raise || mode == PaintHeightMode.flatten)
				{
					SetCubeInternal6(radius: new Vector3(0.5f, selectedHeight - center.y, 0.5f), center: center, blend: blend, blockType: b2, cx: voxelandCoords.x, cy: voxelandCoords.y, cz: voxelandCoords.z);
				}
			}
			else if (mode == PaintHeightMode.lower || mode == PaintHeightMode.flatten)
			{
				SetCubeInternal6(radius: new Vector3(0.5f, center.y - selectedHeight, 0.5f), center: center, blend: blend2, blockType: b2, cx: voxelandCoords.x, cy: voxelandCoords.y, cz: voxelandCoords.z);
			}
		}
		bounds.Expand(1);
		data.CollapseRelevant(bounds.mins.x, bounds.mins.y, bounds.mins.z, bounds.maxs.x, bounds.maxs.y, bounds.maxs.z);
		OnRangeEditDone(bounds.mins.x, bounds.mins.y, bounds.mins.z, bounds.maxs.x, bounds.maxs.y, bounds.maxs.z);
	}

	public void DisplaceBlocks(VoxelandCoords[] coords, Int3 offset)
	{
		Int3.Bounds bounds = BoundsForCoords(coords);
		Int3 size = bounds.size;
		blurWS.SetSize(size.x, size.y, size.z);
		RasterizeVoxels(blurWS, bounds.mins.x, bounds.mins.y, bounds.mins.z, 0);
		foreach (VoxelandCoords voxelandCoords in coords)
		{
			Int3 @int = voxelandCoords.ToInt3() - offset;
			if (bounds.Contains(@int))
			{
				Int3 int2 = @int - bounds.mins;
				byte type = blurWS.typesGrid[int2.x, int2.y, int2.z];
				byte density = blurWS.densityGrid[int2.x, int2.y, int2.z];
				data.SetBlock(voxelandCoords.x, voxelandCoords.y, voxelandCoords.z, type, density, skipCollapse: true, threaded: false);
			}
		}
		data.CollapseRelevant(bounds.mins.x, bounds.mins.y, bounds.mins.z, bounds.maxs.x, bounds.maxs.y, bounds.maxs.z);
		OnRangeEditDone(bounds.mins.x, bounds.mins.y, bounds.mins.z, bounds.maxs.x, bounds.maxs.y, bounds.maxs.z);
	}

	public void BlurBlocks(VoxelandCoords[] coords)
	{
		Int3.Bounds bounds = BoundsForCoords(coords);
		Int3.Bounds bounds2 = bounds.Expanded(1);
		Int3 size = bounds2.size;
		blurWS.SetSize(size.x, size.y, size.z);
		RasterizeVoxels(blurWS, bounds2.mins.x, bounds2.mins.y, bounds2.mins.z, 0);
		Array3<float> array = new Array3<float>(size.x, size.y, size.z);
		Array3<float> array2 = new Array3<float>(size.x, size.y, size.z);
		for (int i = 0; i < size.x; i++)
		{
			for (int j = 0; j < size.y; j++)
			{
				for (int k = 0; k < size.z; k++)
				{
					byte b = blurWS.densityGrid[i, j, k];
					float num = 0f;
					num = (array[i, j, k] = ((b != 0) ? VoxelandData.OctNode.DecodeNearDensity(b) : ((blurWS.typesGrid[i, j, k] == 0) ? (-1f) : 1f)));
					array2[i, j, k] = num;
				}
			}
		}
		if (smoothTweak)
		{
			FixFarDensities(array, size);
			CopyDensities(array2, array, size);
		}
		for (int l = 0; l < blurIterations; l++)
		{
			foreach (VoxelandCoords obj in coords)
			{
				int num3 = obj.x - bounds2.mins.x;
				int num4 = obj.y - bounds2.mins.y;
				int num5 = obj.z - bounds2.mins.z;
				array2[num3, num4, num5] = array[num3, num4, num5] * 0.4f + 0.1f * (array[num3 - 1, num4, num5] + array[num3 + 1, num4, num5] + array[num3, num4 - 1, num5] + array[num3, num4 + 1, num5] + array[num3, num4, num5 - 1] + array[num3, num4, num5 + 1]);
			}
			Array3<float> array3 = array;
			array = array2;
			array2 = array3;
		}
		foreach (VoxelandCoords voxelandCoords in coords)
		{
			int num6 = voxelandCoords.x - bounds2.mins.x;
			int num7 = voxelandCoords.y - bounds2.mins.y;
			int num8 = voxelandCoords.z - bounds2.mins.z;
			float num9 = array[num6, num7, num8];
			byte b2 = VoxelandData.OctNode.EncodeDensity(num9);
			byte b3 = 0;
			if (num9 >= 0f)
			{
				b3 = blurWS.typesGrid[num6, num7, num8];
				if (b3 == 0)
				{
					b3 = blurWS.typesGrid[num6, num7 - 1, num8];
				}
				if (b3 == 0)
				{
					b3 = blurWS.typesGrid[num6, num7 + 1, num8];
				}
				if (b3 == 0)
				{
					b3 = blurWS.typesGrid[num6 - 1, num7, num8];
				}
				if (b3 == 0)
				{
					b3 = blurWS.typesGrid[num6 + 1, num7, num8];
				}
				if (b3 == 0)
				{
					b3 = blurWS.typesGrid[num6, num7, num8 - 1];
				}
				if (b3 == 0)
				{
					b3 = blurWS.typesGrid[num6, num7, num8 + 1];
				}
				if (b3 == 0)
				{
					b3 = (byte)selected;
				}
			}
			else
			{
				b3 = 0;
			}
			if (b2 >= 126 && b3 == 0)
			{
				Debug.LogError("AHHHHHHHHHHHHHHHHHHHH");
			}
			data.SetBlock(voxelandCoords.x, voxelandCoords.y, voxelandCoords.z, b3, b2, skipCollapse: true, threaded: false);
		}
		data.CollapseRelevant(bounds.mins.x, bounds.mins.y, bounds.mins.z, bounds.maxs.x, bounds.maxs.y, bounds.maxs.z);
		OnRangeEditDone(bounds.mins.x, bounds.mins.y, bounds.mins.z, bounds.maxs.x, bounds.maxs.y, bounds.maxs.z);
	}

	public void LegacyBlurBlocks(VoxelandCoords[] coords)
	{
		Int3.Bounds bounds = BoundsForCoords(coords);
		Int3.Bounds bounds2 = bounds.Expanded(1);
		Int3 size = bounds2.size;
		blurWS.SetSize(size.x, size.y, size.z);
		RasterizeVoxels(blurWS, bounds2.mins.x, bounds2.mins.y, bounds2.mins.z, 0);
		Array3<float> array = new Array3<float>(size.x, size.y, size.z);
		Array3<float> array2 = new Array3<float>(size.x, size.y, size.z);
		for (int i = 0; i < size.x; i++)
		{
			for (int j = 0; j < size.y; j++)
			{
				for (int k = 0; k < size.z; k++)
				{
					float value = (array[i, j, k] = ((blurWS.typesGrid[i, j, k] > 0) ? 1f : 0f));
					array2[i, j, k] = value;
				}
			}
		}
		if (smoothTweak)
		{
			FixFarDensities(array, size);
			CopyDensities(array2, array, size);
		}
		for (int l = 0; l < legacyBlurIterations; l++)
		{
			foreach (VoxelandCoords obj in coords)
			{
				int num2 = obj.x - bounds2.mins.x;
				int num3 = obj.y - bounds2.mins.y;
				int num4 = obj.z - bounds2.mins.z;
				array2[num2, num3, num4] = array[num2, num3, num4] * 0.4f + 0.1f * (array[num2 - 1, num3, num4] + array[num2 + 1, num3, num4] + array[num2, num3 - 1, num4] + array[num2, num3 + 1, num4] + array[num2, num3, num4 - 1] + array[num2, num3, num4 + 1]);
			}
			Array3<float> array3 = array;
			array = array2;
			array2 = array3;
		}
		foreach (VoxelandCoords voxelandCoords in coords)
		{
			int num5 = voxelandCoords.x - bounds2.mins.x;
			int num6 = voxelandCoords.y - bounds2.mins.y;
			int num7 = voxelandCoords.z - bounds2.mins.z;
			byte b = 0;
			byte b2 = 0;
			bool flag = blurWS.densityGrid[num5, num6, num7] != 0;
			bool flag2 = blurWS.typesGrid[num5, num6, num7] > 0;
			bool flag3 = array[num5, num6, num7] > 0.5f;
			bool flag4 = false;
			for (int num8 = 0; num8 < 6; num8++)
			{
				int x = num5 + VoxelandChunk.VoxelandFace.dirToPosX[num8];
				int y = num6 + VoxelandChunk.VoxelandFace.dirToPosY[num8];
				int z = num7 + VoxelandChunk.VoxelandFace.dirToPosZ[num8];
				if (array[x, y, z] > 0.5f != flag3)
				{
					flag4 = true;
					break;
				}
			}
			if (flag == flag4 && flag3 == flag2)
			{
				b2 = blurWS.densityGrid[num5, num6, num7];
				b = blurWS.typesGrid[num5, num6, num7];
			}
			else
			{
				b2 = (byte)(flag4 ? VoxelandData.OctNode.EncodeDensity(flag3 ? 0.5f : (-0.5f)) : 0);
				if (!flag3)
				{
					b = 0;
				}
				else
				{
					b = blurWS.typesGrid[num5, num6, num7];
					if (b == 0)
					{
						b = blurWS.typesGrid[num5, num6 - 1, num7];
					}
					if (b == 0)
					{
						b = blurWS.typesGrid[num5, num6 + 1, num7];
					}
					if (b == 0)
					{
						b = blurWS.typesGrid[num5 - 1, num6, num7];
					}
					if (b == 0)
					{
						b = blurWS.typesGrid[num5 + 1, num6, num7];
					}
					if (b == 0)
					{
						b = blurWS.typesGrid[num5, num6, num7 - 1];
					}
					if (b == 0)
					{
						b = blurWS.typesGrid[num5, num6, num7 + 1];
					}
					if (b == 0)
					{
						b = (byte)selected;
					}
				}
			}
			data.SetBlock(voxelandCoords.x, voxelandCoords.y, voxelandCoords.z, b, b2, skipCollapse: true, threaded: false);
		}
		data.CollapseRelevant(bounds.mins.x, bounds.mins.y, bounds.mins.z, bounds.maxs.x, bounds.maxs.y, bounds.maxs.z);
		OnRangeEditDone(bounds.mins.x, bounds.mins.y, bounds.mins.z, bounds.maxs.x, bounds.maxs.y, bounds.maxs.z);
	}

	public void OnRangeEditDone(int sx, int sy, int sz, int ex, int ey, int ez)
	{
		if (!rebuildOnlyOnMouseUp)
		{
			RebuildRelevantChunks(sx, sy, sz, ex, ey, ez);
		}
	}

	public void RebuildForUndoStep(UndoStep undoStep)
	{
		if (undoStep.hasCoords)
		{
			RebuildRelevantChunks(undoStep.sx, undoStep.sy, undoStep.sz, undoStep.ex, undoStep.ey, undoStep.ez);
		}
	}

	public void RebuildRelevantChunks(int sx, int sy, int sz, int ex, int ey, int ez)
	{
		int num = debugDownsampleLevels;
		sx -= 3 << num;
		sy -= 3 << num;
		sz -= 3 << num;
		ex += 3 << num;
		ey += 3 << num;
		ez += 3 << num;
		int num2 = chunkSize << num;
		int num3 = Mathf.Max(0, sx / num2);
		int num4 = Mathf.Min(chunkCountX, ex / num2 + 1);
		int num5 = Mathf.Max(0, sy / num2);
		int num6 = Mathf.Min(chunkCountY, ey / num2 + 1);
		int num7 = Mathf.Max(0, sz / num2);
		int num8 = Mathf.Min(chunkCountZ, ez / num2 + 1);
		if (overrideRasterizer != null)
		{
			Int3 mins = ChunkMinUsedBlock(new Int3(num3, num5, num7));
			Int3 maxs = ChunkMaxUsedBlock(new Int3(num4, num6, num8));
			overrideRasterizer.OnPreBuildRange(new Int3.Bounds(mins, maxs));
		}
		int totalChunks = (num4 - num3) * (num6 - num5) * (num8 - num7);
		int num9 = 0;
		_ = DateTime.Now.Ticks;
		VoxelandBulkChunkBuilder voxelandBulkChunkBuilder = null;
		voxelandBulkChunkBuilder?.OnBeginBuildingChunks(this, totalChunks);
		for (int i = num3; i < num4; i++)
		{
			for (int j = num5; j < num6; j++)
			{
				for (int k = num7; k < num8; k++)
				{
					if (CheckChunkBlocksReady(i, j, k))
					{
						BuildChunk(i, j, k);
						num9++;
					}
				}
			}
		}
		voxelandBulkChunkBuilder?.OnEndBuildingChunks(this);
	}

	public void CollapseForCoords(VoxelandCoords[] coords, int margin)
	{
		int num = data.sizeX;
		int num2 = 0;
		int num3 = data.sizeY;
		int num4 = 0;
		int num5 = data.sizeZ;
		int num6 = 0;
		for (int i = 0; i < coords.Length; i++)
		{
			num = Mathf.Min(num, coords[i].x);
			num3 = Mathf.Min(num3, coords[i].y);
			num5 = Mathf.Min(num5, coords[i].z);
			num2 = Mathf.Max(num2, coords[i].x);
			num4 = Mathf.Max(num4, coords[i].y);
			num6 = Mathf.Max(num6, coords[i].z);
		}
		num -= margin;
		num3 -= margin;
		num5 -= margin;
		num2 += margin;
		num4 += margin;
		num6 += margin;
		data.CollapseRelevant(num, num3, num5, num2, num4, num6);
	}

	public void OnCoordsEditsDone(VoxelandCoords[] coords, int margin)
	{
		if (debugVerbose)
		{
			Debug.Log("rebuilding chunks for " + coords.Length + " changed blocks");
		}
		int num = data.sizeX;
		int num2 = 0;
		int num3 = data.sizeY;
		int num4 = 0;
		int num5 = data.sizeZ;
		int num6 = 0;
		for (int i = 0; i < coords.Length; i++)
		{
			num = Mathf.Min(num, coords[i].x);
			num3 = Mathf.Min(num3, coords[i].y);
			num5 = Mathf.Min(num5, coords[i].z);
			num2 = Mathf.Max(num2, coords[i].x);
			num4 = Mathf.Max(num4, coords[i].y);
			num6 = Mathf.Max(num6, coords[i].z);
		}
		num -= margin;
		num3 -= margin;
		num5 -= margin;
		num2 += margin;
		num4 += margin;
		num6 += margin;
		OnRangeEditDone(num, num3, num5, num2, num4, num6);
	}

	public VoxelandCoords GetHitCoords(RaycastHit hit)
	{
		if (hit.collider == null)
		{
			return null;
		}
		VoxelandChunk voxelandChunk = FindAncestorChunk(hit.collider.gameObject);
		if (voxelandChunk == null)
		{
			return null;
		}
		if (voxelandChunk.GetFaceMap() == null || voxelandChunk.GetFaceMap().Count == 0)
		{
			return null;
		}
		return voxelandChunk.GetFaceMap()[hit.triangleIndex / 2];
	}

	public bool HitSameBlock(RaycastHit a, RaycastHit b)
	{
		if (GetHitCoords(a) != null && GetHitCoords(b) != null)
		{
			return GetHitCoords(a).IsSameBlock(GetHitCoords(b));
		}
		return false;
	}

	public VoxelandChunk FindAncestorChunk(GameObject go)
	{
		Transform parent = go.transform;
		while (parent != null && parent.gameObject.GetComponent<VoxelandChunk>() == null)
		{
			parent = parent.parent;
		}
		if (parent != null)
		{
			return parent.GetComponent<VoxelandChunk>();
		}
		return null;
	}

	public void PrepareHighlight()
	{
		if (!highlight)
		{
			GameObject gameObject = new GameObject("Highlight");
			gameObject.transform.parent = base.transform;
			highlight = gameObject.AddComponent<VoxelandHighlight>();
			highlight.Init();
		}
	}

	public void PrepareHighlightMaterial()
	{
		if (!highlightMaterial || highlightMaterial.name != "Voxeland/Highlight")
		{
			highlightMaterial = new Material(ShaderManager.preloadedShaders.voxelandHighlight);
			highlightMaterial.color = new Color(0.6f, 0.73f, 1f, 0.353f);
		}
	}

	public void SelectTypeAt(VoxelandCoords c)
	{
		SelectTypeAt(c.x, c.y, c.z);
	}

	public byte QueryTypeAt(int x, int y, int z)
	{
		queryWS.SetSize(1, 1, 1);
		RasterizeVoxels(queryWS, x, y, z, 0);
		return queryWS.typesGrid[0, 0, 0];
	}

	public byte QueryDensityAt(int x, int y, int z)
	{
		queryWS.SetSize(1, 1, 1);
		RasterizeVoxels(queryWS, x, y, z, 0);
		return queryWS.densityGrid[0, 0, 0];
	}

	public void SelectTypeAt(int x, int y, int z)
	{
		selected = QueryTypeAt(x, y, z);
	}

	public bool Edit(Ray aimRay, bool mouseDown, bool mouseDragged, bool shift, bool control, bool mouseUp, bool blockyEdit)
	{
		if (data == null || data.roots == null)
		{
			return false;
		}
		if (debugDownsampleLevels > 0)
		{
			return false;
		}
		PrepareHighlight();
		PrepareHighlightMaterial();
		VoxelandCoords[] array = null;
		RaycastHit hitInfo;
		bool flag = Physics.Raycast(aimRay, out hitInfo, float.PositiveInfinity, 1073741824);
		bool flag2 = HitSameBlock(hitInfo, prevPostEditHit);
		bool flag3 = flag && hitInfo.collider.transform.IsChildOf(base.transform);
		Color color = (blockyEdit ? Color.blue : Color.green).ToAlpha(0.25f);
		Vector3 vector = Vector3.zero;
		Vector3 vector2 = Vector3.zero;
		if (flag3)
		{
			VoxelandChunk voxelandChunk = FindAncestorChunk(hitInfo.collider.gameObject);
			if (voxelandChunk != null && !voxelandChunk.NeedsRebuild())
			{
				array = new VoxelandCoords[1] { voxelandChunk.GetFaceMap()[hitInfo.triangleIndex / 2] };
				if (heightLockMode)
				{
					if (mouseDown)
					{
						lockedHeight = array[0].y;
					}
					if (mouseDragged)
					{
						array[0].y = lockedHeight;
					}
				}
				if (brushSize == 0)
				{
					highlight.transform.localPosition = Vector3.zero;
					highlight.transform.localScale = Vector3.one;
					highlight.DrawFace(voxelandChunk, hitInfo.triangleIndex / 2, color);
				}
				else
				{
					vector = array[0].GetCenter();
					vector2 = new Vector3(brushSize, (float)brushSize * brushScaleY, brushSize);
					if (displaceMode && !blockyEdit && !heightLockMode)
					{
						float num = selectedHeight;
						float y = base.transform.InverseTransformPoint(hitInfo.point).y;
						if (num > y)
						{
							y = Mathf.Floor(y - (float)brushSize);
						}
						else
						{
							float num2 = num;
							num = Mathf.Ceil(y + 0.5f);
							y = num2;
						}
						float y2 = (num - y) / 2f;
						float f = (vector.y = (num + y) / 2f);
						vector2.y = y2;
						highlight.transform.localPosition = vector;
						highlight.transform.localScale = vector2;
						VoxelandCoords voxelandCoords = new VoxelandCoords(array[0]);
						voxelandCoords.y = Mathf.FloorToInt(f);
						switch (brushShape)
						{
						case BrushShape.Sphere:
							highlight.DrawCylinder(color);
							array = voxelandCoords.GetCylinderNeigs(vector2, data);
							break;
						case BrushShape.Cube:
							highlight.DrawCube(color);
							array = voxelandCoords.GetNeigs(vector2, data);
							break;
						}
					}
					else
					{
						switch (brushShape)
						{
						case BrushShape.Sphere:
							highlight.transform.localPosition = vector;
							highlight.transform.localRotation = Quaternion.identity;
							highlight.transform.localScale = vector2;
							highlight.DrawSphere(color);
							array = array[0].GetSphereNeigs(vector2, data);
							break;
						case BrushShape.Cube:
							highlight.transform.localPosition = vector;
							highlight.transform.localRotation = Quaternion.identity;
							highlight.transform.localScale = vector2;
							highlight.DrawCube(color);
							array = array[0].GetNeigs(vector2, data);
							break;
						case BrushShape.Mesh:
						{
							Quaternion quaternion = Quaternion.Euler(brushRotationEuler);
							highlight.transform.localPosition = vector;
							highlight.transform.localRotation = quaternion;
							highlight.transform.localScale = vector2;
							highlight.DrawMesh(color, brushAsset.mesh);
							array = array[0].GetAABBNeigs(brushAsset.mesh.bounds, quaternion, vector2, data);
							break;
						}
						}
					}
				}
			}
		}
		else
		{
			highlight.Clear();
		}
		if ((mouseDown || (mouseDragged && IsInUndoStep())) && array != null && !flag2)
		{
			if (mouseDown)
			{
				if (IsInUndoStep())
				{
					EndUndoStep();
				}
				BeginUndoStep();
			}
			if (selected < 1 || selected > types.Length - 1)
			{
				selected = 1;
			}
			if (types[selected] == null)
			{
				selected = 1;
			}
			bool filled = types[selected].filled;
			if (Event.current.alt && !shift && !control)
			{
				SelectTypeAt(array[0]);
				selectedHeight = base.transform.InverseTransformPoint(hitInfo.point).y;
			}
			else if (filled && brushSize > 0)
			{
				if (shift && !control)
				{
					if (blockyEdit)
					{
						if (displaceMode)
						{
							DisplaceBlocks(array, Int3.zero - displaceOffset);
						}
						else if (Event.current.alt)
						{
							RemoveLayer(array);
						}
						else
						{
							SetCubes(array, selected, add: false);
						}
					}
					else if (displaceMode)
					{
						PaintHeight(array, PaintHeightMode.lower);
					}
					else
					{
						switch (brushShape)
						{
						case BrushShape.Sphere:
							SetSphere(vector, vector2, add: false);
							break;
						case BrushShape.Cube:
							SetCube(vector, vector2, add: false);
							break;
						case BrushShape.Mesh:
							StampMesh(vector, vector2, brushRotationEuler, brushAsset, addVoxels: false);
							break;
						}
					}
				}
				else if (control && !shift)
				{
					if (blockyEdit)
					{
						if (displaceMode)
						{
							DisplaceBlocks(array, displaceOffset);
						}
						else if (Event.current.alt)
						{
							AddLayer(array, selected);
						}
						else
						{
							SetCubes(array, selected, add: true);
						}
					}
					else if (displaceMode)
					{
						PaintHeight(array, PaintHeightMode.raise);
					}
					else
					{
						switch (brushShape)
						{
						case BrushShape.Sphere:
							SetSphere(vector, vector2, add: true);
							break;
						case BrushShape.Cube:
							SetCube(vector, vector2, add: true);
							break;
						case BrushShape.Mesh:
							StampMesh(vector, vector2, brushRotationEuler, brushAsset, addVoxels: true);
							break;
						}
					}
				}
				else if (control && shift)
				{
					if (blockyEdit)
					{
						LegacyBlurBlocks(array);
					}
					else if (displaceMode)
					{
						PaintHeight(array, PaintHeightMode.flatten);
					}
					else if (Event.current.alt)
					{
						ResetSmoothing(array);
					}
					else
					{
						BlurBlocks(array);
					}
				}
				else if (grassMode)
				{
					Vector3 point = base.transform.TransformPoint(array[0].GetCenter());
					PaintGrass(grassTypes[selectedGrassType], point, (float)brushSize - 0.4f, brushScaleY, brushShape);
				}
				else
				{
					PaintBlocks(array, (byte)selected);
				}
			}
			else if (shift && !control)
			{
				SetCube(array[0].GetCenter(), half3, add: false);
			}
			else if (control && !shift)
			{
				SetCube(array[0].GetOpposite().GetCenter(), half3, add: true);
			}
			else if (control && shift)
			{
				if (blockyEdit)
				{
					LegacyBlurBlocks(array);
				}
				else if (Event.current.alt)
				{
					ResetSmoothing(array);
				}
				else
				{
					BlurBlocks(array);
				}
			}
			else if (grassMode)
			{
				Mesh sharedMesh = (hitInfo.collider as MeshCollider).sharedMesh;
				Transform transform = hitInfo.collider.gameObject.transform;
				if (grassPreciseMode)
				{
					if (mouseDown && !mouseDragged)
					{
						Vector3 vector3 = sharedMesh.normals[sharedMesh.triangles[3 * hitInfo.triangleIndex]];
						Vector3 vector4 = sharedMesh.normals[sharedMesh.triangles[3 * hitInfo.triangleIndex + 1]];
						Vector3 vector5 = sharedMesh.normals[sharedMesh.triangles[3 * hitInfo.triangleIndex + 2]];
						Vector3 barycentricCoordinate = hitInfo.barycentricCoordinate;
						Vector3 normal = transform.TransformDirection(vector3 * barycentricCoordinate.x + vector4 * barycentricCoordinate.y + vector5 * barycentricCoordinate.z);
						PaintGrassAt(hitInfo.point, normal, grassTypes[selectedGrassType]);
					}
				}
				else
				{
					int num3 = hitInfo.triangleIndex / 2;
					Vector3 vector6 = sharedMesh.vertices[sharedMesh.triangles[6 * num3]];
					Vector3 vector7 = sharedMesh.vertices[sharedMesh.triangles[6 * num3 + 1]];
					Vector3 vector8 = sharedMesh.vertices[sharedMesh.triangles[6 * num3 + 2]];
					Vector3 vector9 = sharedMesh.vertices[sharedMesh.triangles[6 * num3 + 5]];
					Vector3 point2 = transform.TransformPoint((vector6 + vector7 + vector8 + vector9) / 4f);
					PaintGrass(grassTypes[selectedGrassType], point2, 0.1f, 1f, BrushShape.Cube);
				}
			}
			else
			{
				PaintBlocks(array, (byte)selected);
			}
			Physics.Raycast(aimRay, out prevPostEditHit, float.PositiveInfinity, 1073741824);
		}
		if (mouseUp || !flag)
		{
			prevPostEditHit = default(RaycastHit);
		}
		if (mouseUp && IsInUndoStep())
		{
			EndUndoStep();
		}
		lastAimCoords = array;
		return flag3;
	}

	public void CreateSolidMaterials()
	{
		_ = debugSolidColorMaterials;
	}

	public void InitBlockTypes()
	{
		if (paletteResourceDir != null && paletteResourceDir != "")
		{
			LoadPalette(paletteResourceDir);
		}
		for (int i = 0; i < types.Length; i++)
		{
			if (types[i] != null)
			{
				types[i].RuntimeInit(i);
			}
		}
	}

	public void Rebuild(bool buildChunksNow = false)
	{
		if (!data)
		{
			Debug.LogError("Voxeland had no data set!");
			return;
		}
		if (buildChunksNow && (data.roots == null || data.roots.Length == 0))
		{
			Debug.LogError("Data object had no octree roots!");
			return;
		}
		UpdateData();
		UpdateOpaqueMaterial();
		if (debugHandler != null)
		{
			debugHandler.BeginRebuild();
		}
		chunkCountX = Utils.CeilDiv(data.sizeX, chunkSize << debugDownsampleLevels);
		chunkCountY = Utils.CeilDiv(data.sizeY, chunkSize << debugDownsampleLevels);
		chunkCountZ = Utils.CeilDiv(data.sizeZ, chunkSize << debugDownsampleLevels);
		if (chunkWindow != null)
		{
			DestroyAllChunks();
			chunkWindow = null;
		}
		for (int num = base.transform.childCount - 1; num >= 0; num--)
		{
			VoxelandChunk component = base.transform.GetChild(num).gameObject.GetComponent<VoxelandChunk>();
			if (component != null)
			{
				component.DestroySelf();
			}
		}
		InitBlockTypes();
		CreateSolidMaterials();
		chunkWindow = new ChunkState[GetWindowLen() * GetWindowLen() * GetWindowLen()];
		for (int i = 0; i < chunkWindow.Length; i++)
		{
			chunkWindow[i] = new ChunkState();
			chunkWindow[i].Reset();
		}
		if (!buildChunksNow)
		{
			return;
		}
		ResetStats();
		int num2 = 0;
		int totalChunks = chunkCountX * chunkCountY * chunkCountZ;
		VoxelandBulkChunkBuilder voxelandBulkChunkBuilder = null;
		voxelandBulkChunkBuilder?.OnBeginBuildingChunks(this, totalChunks);
		for (int j = 0; j < chunkCountX; j++)
		{
			for (int k = 0; k < chunkCountY; k++)
			{
				for (int l = 0; l < chunkCountZ; l++)
				{
					if (CheckChunkBlocksReady(j, k, l))
					{
						BuildChunk(j, k, l);
						num2++;
					}
				}
			}
		}
		voxelandBulkChunkBuilder?.OnEndBuildingChunks(this);
	}

	public void HideAllChunkRenders()
	{
		for (int i = 0; i < chunkWindow.Length; i++)
		{
			if (chunkWindow[i].chunk != null)
			{
				chunkWindow[i].chunk.DisableAllMeshRenderers();
			}
		}
	}

	public bool CheckChunk(int cx, int cy, int cz)
	{
		if (cx >= 0 && cy >= 0 && cz >= 0 && cx < chunkCountX && cy < chunkCountY)
		{
			return cz < chunkCountZ;
		}
		return false;
	}

	public void DestroyRelevantChunks(int sx, int sy, int sz, int ex, int ey, int ez)
	{
		sx = Mathf.Max(0, sx - 3);
		sy = Mathf.Max(0, sy - 3);
		sz = Mathf.Max(0, sz - 3);
		ex = Mathf.Min(ex + 3, data.sizeX - 1);
		ey = Mathf.Min(ey + 3, data.sizeY - 1);
		ez = Mathf.Min(ez + 3, data.sizeZ - 1);
		int num = chunkSize << debugDownsampleLevels;
		for (int i = sx / num; i <= ex / num; i++)
		{
			for (int j = sy / num; j <= ey / num; j++)
			{
				for (int k = sz / num; k <= ez / num; k++)
				{
					ChunkState chunkState = chunkWindow[GetChunkWindowSlot(i, j, k)];
					if (chunkState.IsOccupiedByChunk(i, j, k))
					{
						chunkState.DestroyChunk(this);
					}
				}
			}
		}
	}

	public void DestroyAllChunks()
	{
		if (chunkWindow != null)
		{
			for (int i = 0; i < chunkWindow.Length; i++)
			{
				chunkWindow[i].DestroyChunk(this);
			}
		}
	}

	public int GetChunkX(Vector3 localPos)
	{
		return Mathf.FloorToInt(localPos.x / (float)chunkSize);
	}

	public int GetChunkY(Vector3 localPos)
	{
		return Mathf.FloorToInt(localPos.y / (float)chunkSize);
	}

	public int GetChunkZ(Vector3 localPos)
	{
		return Mathf.FloorToInt(localPos.z / (float)chunkSize);
	}

	public int GetChunkWindowSlot(int cx, int cy, int cz)
	{
		if (cx < 0 || cx >= chunkCountX || cy < 0 || cy >= chunkCountY || cz < 0 || cz >= chunkCountZ)
		{
			Debug.LogErrorFormat("Bad chunk coords: ({0}, {1}, {2}) out of range of (0, 0, 0) - ({3}, {4}, {5})", cx, cy, cz, chunkCountX, chunkCountY, chunkCountZ);
			return 0;
		}
		int windowLen = GetWindowLen();
		cx %= windowLen;
		cy %= windowLen;
		cz %= windowLen;
		int num = cx * windowLen * windowLen + cy * windowLen + cz;
		if (num >= chunkWindow.Length)
		{
			Debug.LogError(num + " >= " + chunkWindow.Length + ", W = " + windowLen);
			return 0;
		}
		return num;
	}

	public int GetChunkWindowSlot(Vector3 localPos)
	{
		return GetChunkWindowSlot(GetChunkX(localPos), GetChunkY(localPos), GetChunkZ(localPos));
	}

	public int ChunkMinUsedBlock(int coord)
	{
		return coord * (chunkSize << debugDownsampleLevels) - (3 << debugDownsampleLevels);
	}

	public int ChunkMaxUsedBlock(int coord)
	{
		return ChunkMinUsedBlock(coord) + (chunkSize << debugDownsampleLevels) + 2 * (3 << debugDownsampleLevels) - 1;
	}

	public Int3 ChunkMinUsedBlock(Int3 chunk)
	{
		return new Int3(ChunkMinUsedBlock(chunk.x), ChunkMinUsedBlock(chunk.y), ChunkMinUsedBlock(chunk.z));
	}

	public Int3 ChunkMaxUsedBlock(Int3 chunk)
	{
		return new Int3(ChunkMaxUsedBlock(chunk.x), ChunkMaxUsedBlock(chunk.y), ChunkMaxUsedBlock(chunk.z));
	}

	public bool IsRangeReady(Int3.Bounds range)
	{
		if (overrideRasterizer != null)
		{
			return overrideRasterizer.IsRangeLoaded(range, 0);
		}
		return true;
	}

	public bool CheckChunkBlocksReady(int cx, int cy, int cz)
	{
		Int3 chunk = new Int3(cx, cy, cz);
		Int3 mins = ChunkMinUsedBlock(chunk);
		Int3 maxs = ChunkMaxUsedBlock(chunk);
		return IsRangeReady(new Int3.Bounds(mins, maxs));
	}

	public bool IsChunkBuilt(Vector3 localPos)
	{
		int chunkX = GetChunkX(localPos);
		int chunkY = GetChunkY(localPos);
		int chunkZ = GetChunkZ(localPos);
		return chunkWindow[GetChunkWindowSlot(chunkX, chunkY, chunkZ)].IsOccupiedByChunk(chunkX, chunkY, chunkZ);
	}

	public bool IsChunkBuilt(int cx, int cy, int cz)
	{
		return chunkWindow[GetChunkWindowSlot(cx, cy, cz)].IsOccupiedByChunk(cx, cy, cz);
	}

	public static Mesh DuplicateMesh(Mesh mesh)
	{
		return new Mesh
		{
			vertices = mesh.vertices,
			colors = mesh.colors,
			normals = mesh.normals,
			tangents = mesh.tangents,
			uv = mesh.uv,
			uv2 = mesh.uv2,
			triangles = mesh.triangles
		};
	}

	public void BuildChunk(int cx, int cy, int cz)
	{
		if (data.roots == null)
		{
			Debug.LogError("No octree! Can't build chunk");
		}
		if (debugVerbose)
		{
			Debug.Log("BuildChunk(" + cx + ", " + cy + ", " + cz + ")");
		}
		bool flag = false;
		if (CannotBuildDueToOverrideBuilder())
		{
			return;
		}
		ChunkState chunkState = chunkWindow[GetChunkWindowSlot(cx, cy, cz)];
		MeshCollider meshCollider = null;
		List<VoxelandCoords> faceMap = null;
		if (chunkState.beingBuilt)
		{
			Debug.LogWarning("Skipping build of chunk (" + cx + ", " + cy + ", " + cz + ") because somebody is already building it.");
			return;
		}
		if (chunkState.chunk != null)
		{
			if (keepColliders && chunkState.chunk.collision != null)
			{
				meshCollider = chunkState.chunk.DetachCollision();
				faceMap = chunkState.chunk.GetFaceMap();
			}
			chunkState.DestroyChunk(this);
		}
		VoxelandChunk voxelandChunk = (chunkState.chunk = CreateChunk(cx, cy, cz));
		voxelandChunk.skipHiRes = skipHiRes;
		voxelandChunk.disableGrass = disableGrass;
		if (keepColliders)
		{
			chunkState.chunk.generateCollider = false;
		}
		if (IsChunkUniform(cx, cy, cz))
		{
			chunkState.beingBuilt = true;
			voxelandChunk.gameObject.name = voxelandChunk.gameObject.name + " (Empty)";
			OnChunkBuilt(chunkState);
		}
		else if (flag)
		{
			chunkState.beingBuilt = true;
			((VoxelandChunkBuilder)null).Build(chunkState);
		}
		else
		{
			chunkState.beingBuilt = true;
			if (chunkWorkspace == null)
			{
				chunkWorkspace = new VoxelandChunkWorkspace();
			}
			chunkWorkspace.SetSize(voxelandChunk.meshRes);
			voxelandChunk.ws = chunkWorkspace;
			voxelandChunk.BuildMesh(debugSkipRelax);
			if (voxelandChunk.ws.verts.Count > 65000)
			{
				Debug.LogError("Too many verts generated for chunk: " + voxelandChunk.ws.verts.Count);
			}
			else
			{
				voxelandChunk.BuildLayerObjects();
				if (ambient)
				{
					voxelandChunk.BuildAmbient();
				}
				if (!disableGrass)
				{
					voxelandChunk.BuildGrass();
				}
				voxelandChunk.ws = null;
				OnChunkBuilt(chunkState);
			}
		}
		if (keepColliders && meshCollider != null)
		{
			voxelandChunk.AttachCollision(meshCollider);
			voxelandChunk.SetFaceMap(faceMap);
			meshCollider.gameObject.SetActive(value: true);
		}
		lastChunkFrame = Time.frameCount;
	}

	public void OnChunkBuilt(ChunkState state)
	{
		if (!state.beingBuilt)
		{
			Debug.LogError("Something reported a chunk built, but its beingBuilt state was not set. Something went wrong..");
			return;
		}
		state.beingBuilt = false;
		if (state.destroyQueued)
		{
			state.DestroyChunk(this);
		}
		else if (eventHandler != null)
		{
			eventHandler.OnChunkBuilt(this, state.chunk.cx, state.chunk.cy, state.chunk.cz);
		}
	}

	public void UpdateChunks(Vector3 lsRef, float maxKeepDist, float maxHiResDist)
	{
		for (int i = 0; i < chunkWindow.Length; i++)
		{
			ChunkState chunkState = chunkWindow[i];
			if (chunkState.chunk != null)
			{
				float num = LocalDistSqToChunk(lsRef, chunkState.chunk);
				if (maxKeepDist > 0f && num > maxKeepDist * maxKeepDist)
				{
					chunkState.DestroyChunk(this);
				}
				else if (!debugFreezeLOD)
				{
					chunkState.SwitchToLOD(this, num < maxHiResDist * maxHiResDist);
				}
			}
		}
	}

	public bool ComputeClosestUnbuiltChunks(Vector3 lsRef, Vector3 lsForward, int count)
	{
		if (chunkWindow == null || chunkWindow.Length == 0)
		{
			Debug.LogError("WTF??");
			return false;
		}
		int num = Mathf.Clamp(Mathf.FloorToInt(lsRef.x / (float)chunkSize), 0, chunkCountX - 1);
		int num2 = Mathf.Clamp(Mathf.FloorToInt(lsRef.y / (float)chunkSize), 0, chunkCountY - 1);
		int num3 = Mathf.Clamp(Mathf.FloorToInt(lsRef.z / (float)chunkSize), 0, chunkCountZ - 1);
		ChunkState chunkState = null;
		int windowRadius = GetWindowRadius();
		float num4 = GetLocalBuildDistance() * GetLocalBuildDistance();
		float num5 = Mathf.Cos((float)System.Math.PI / 3f);
		closestChunks.Clear();
		int num6 = -1;
		for (int i = num - windowRadius; i <= num + windowRadius; i++)
		{
			for (int j = num2 - windowRadius; j <= num2 + windowRadius; j++)
			{
				for (int k = num3 - windowRadius; k <= num3 + windowRadius; k++)
				{
					if (!CheckChunk(i, j, k))
					{
						continue;
					}
					chunkState = chunkWindow[GetChunkWindowSlot(i, j, k)];
					if (chunkState.IsOccupiedByChunk(i, j, k) || chunkState.beingBuilt)
					{
						continue;
					}
					float num7 = LocalDistSqToChunk(lsRef, i, j, k);
					if (num7 > num4 || !CheckChunkBlocksReady(i, j, k))
					{
						continue;
					}
					if (Vector3.Dot((new Vector3(((float)i + 0.5f) * (float)chunkSize, ((float)j + 0.5f) * (float)chunkSize, ((float)k + 0.5f) * (float)chunkSize) - lsRef).normalized, lsForward) > num5)
					{
						num7 -= num4 * 0.8f;
					}
					CloseChunk closeChunk = default(CloseChunk);
					closeChunk.cx = i;
					closeChunk.cy = j;
					closeChunk.cz = k;
					closeChunk.distSq = num7;
					if (closestChunks.Count < count)
					{
						closestChunks.Add(closeChunk);
						if (num6 == -1 || num7 > closestChunks[num6].distSq)
						{
							num6 = closestChunks.Count - 1;
						}
					}
					else
					{
						if (!(num7 < closestChunks[num6].distSq))
						{
							continue;
						}
						closestChunks[num6] = closeChunk;
						num6 = -1;
						for (int l = 0; l < closestChunks.Count; l++)
						{
							if (num6 == -1 || closestChunks[l].distSq > closestChunks[num6].distSq)
							{
								num6 = l;
							}
						}
					}
				}
			}
		}
		return true;
	}

	public bool BuildBestChunk(Vector3 lsRef, Vector3 lsForward)
	{
		return BuildBestChunk(lsRef, lsForward, forceRebuild: false);
	}

	public bool BuildBestChunk(Vector3 lsRef, Vector3 lsForward, bool forceRebuild)
	{
		ComputeClosestUnbuiltChunks(lsRef, lsForward, 1);
		if (closestChunks.Count > 0)
		{
			CloseChunk closeChunk = closestChunks[0];
			BuildChunk(closeChunk.cx, closeChunk.cy, closeChunk.cz);
			return true;
		}
		return false;
	}

	public void ExportOBJ(string fname, bool includeNormals, bool useHi)
	{
		StringWriter stringWriter = new StringWriter();
		StringWriter stringWriter2 = new StringWriter();
		StringWriter stringWriter3 = new StringWriter();
		StringWriter stringWriter4 = new StringWriter();
		int num = 0;
		int num2 = 0;
		_ = chunkWindow.Length;
		int num3 = 0;
		ChunkState[] array = chunkWindow;
		foreach (ChunkState chunkState in array)
		{
			num3++;
			if (chunkState.chunk == null || chunkState.chunk.hiFilters.Count == 0)
			{
				continue;
			}
			Mesh sharedMesh = chunkState.chunk.hiFilters[0].sharedMesh;
			if (!useHi)
			{
				sharedMesh = chunkState.chunk.loFilters[0].sharedMesh;
			}
			Vector3[] vertices = sharedMesh.vertices;
			Vector3[] normals = sharedMesh.normals;
			int[] triangles = sharedMesh.triangles;
			Transform transform = chunkState.chunk.transform;
			int num4 = 0;
			int[] array2 = triangles;
			foreach (int b in array2)
			{
				num4 = Mathf.Max(num4, b);
			}
			int num5 = num4 + 1;
			int num6 = 0;
			for (num6 = 0; num6 < num5; num6++)
			{
				Vector3 vector = transform.TransformPoint(vertices[num6]);
				vector.x *= -1f;
				stringWriter.WriteLine($"v {vector.x:0.000000f} {vector.y:0.000000f} {vector.z:0.000000f}");
				if (includeNormals)
				{
					Vector3 vector2 = normals[num6];
					vector2.x *= -1f;
					stringWriter2.WriteLine($"vn {vector2.x:0.000000f} {vector2.y:0.000000f} {vector2.z:0.000000f}");
					float num7 = Mathf.Abs(vector2.x);
					float num8 = Mathf.Abs(vector2.y);
					float num9 = Mathf.Abs(vector2.z);
					Vector2 vector3 = ((num9 > num7 && num9 > num8) ? new Vector2(vector.x, vector.y) : ((!(num7 > num8) || !(num7 > num9)) ? new Vector2(vector.x, vector.z) : new Vector2(vector.y, vector.z)));
					vector3 *= 0.1f;
					stringWriter3.WriteLine($"vt {vector3.x:0.000000f} {vector3.y:0.000000f}");
				}
			}
			for (num = 0; num < triangles.Length / 3; num++)
			{
				int num10 = num2 + triangles[3 * num] + 1;
				int num11 = num2 + triangles[3 * num + 1] + 1;
				int num12 = num2 + triangles[3 * num + 2] + 1;
				if (num10 != num11 || num10 != num12)
				{
					if (includeNormals)
					{
						stringWriter4.WriteLine(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}", num12, num11, num10));
					}
					else
					{
						stringWriter4.WriteLine($"f {num12} {num11} {num10}");
					}
				}
			}
			num2 += num5;
		}
		StreamWriter streamWriter = FileUtils.CreateTextFile(fname);
		try
		{
			streamWriter.Write(stringWriter.ToString());
			streamWriter.Write(stringWriter2.ToString());
			streamWriter.Write(stringWriter3.ToString());
			streamWriter.Write(stringWriter4.ToString());
		}
		finally
		{
			streamWriter.Close();
		}
	}

	public void RefreshMaterials()
	{
		Debug.LogError("not implemented - bug steve");
	}

	public void EditorUpdate()
	{
	}

	public void OnDrawGizmos()
	{
		if (visualizeNodes || visualizeFill)
		{
			data.Visualize(visualizeNodes, visualizeFill);
		}
	}

	public void OnDestroy()
	{
		DestroyAllChunks();
	}

	public void WriteNumVisFacesStats()
	{
		int num = 0;
		StreamWriter streamWriter = FileUtils.CreateTextFile("DEBUG-num-vis-faces.txt");
		for (int i = 0; i < chunkWindow.Length; i++)
		{
			if (chunkWindow[i].chunk != null)
			{
				int count = chunkWindow[i].chunk.GetFaceMap().Count;
				streamWriter.Write(count + "\n");
				num += count;
			}
		}
		streamWriter.Close();
		Debug.Log("total num faces = " + num);
	}

	public void ResizeGrassTypes(int size)
	{
		VoxelandGrassType[] array = new VoxelandGrassType[size];
		for (int i = 0; i < size; i++)
		{
			if (i < grassTypes.Length)
			{
				array[i] = grassTypes[i];
			}
			else
			{
				array[i] = new VoxelandGrassType();
			}
		}
		grassTypes = array;
		if (selectedGrassType < 0)
		{
			selectedGrassType = 0;
		}
		if (selectedGrassType >= size)
		{
			selectedGrassType = size - 1;
		}
	}

	public void ClearTypes()
	{
		types = new VoxelandBlockType[1];
		types[0] = new VoxelandBlockType();
		types[0].name = "Empty";
		types[0].filled = false;
	}

	public void AddTypes(List<VoxelandBlockType> addTypes)
	{
		List<VoxelandBlockType> list = new List<VoxelandBlockType>();
		for (int i = 0; i < types.Length; i++)
		{
			list.Add(types[i]);
		}
		for (int j = 0; j < addTypes.Count; j++)
		{
			list.Add(addTypes[j]);
		}
		types = list.ToArray();
	}

	public int FindEquivalentType(VoxelandBlockType query)
	{
		if (query.material == null)
		{
			return 0;
		}
		for (int i = 0; i < types.Length; i++)
		{
			if (types[i] != null && types[i].IsVisuallySame(query))
			{
				return i;
			}
		}
		return -1;
	}

	public List<byte> MergeTypes(VoxelandBlockType[] otherTypes)
	{
		return MergeTypes(otherTypes, errorIfAnyNew: false);
	}

	public List<byte> MergeTypes(VoxelandBlockType[] otherTypes, bool errorIfAnyNew, UnityEngine.Object errorContext = null)
	{
		List<byte> list = new List<byte>();
		List<VoxelandBlockType> list2 = new List<VoxelandBlockType>();
		VoxelandBlockType[] array = types;
		foreach (VoxelandBlockType item in array)
		{
			list2.Add(item);
		}
		bool flag = false;
		int num = -1;
		for (int j = 0; j < otherTypes.Length; j++)
		{
			if (otherTypes[j].filled && otherTypes[j].material == null)
			{
				Debug.Log($"WARNING: While merging block types: filled block type {j} named {otherTypes[j].name} had no material assigned. Skipping");
				list.Add(0);
				continue;
			}
			bool flag2 = false;
			for (int k = 0; k < types.Length; k++)
			{
				if (types[k] != null && types[k].IsVisuallySame(otherTypes[j]))
				{
					list.Add((byte)k);
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				list.Add((byte)list2.Count);
				list2.Add(otherTypes[j]);
				flag = true;
				num = j;
			}
		}
		if (flag)
		{
			if (errorIfAnyNew)
			{
				Debug.LogError($"MergeTypes called, and new types were found. Undesired by caller. One new type: {num}. material = {otherTypes[num].material.name}", errorContext);
			}
			types = list2.ToArray();
		}
		return list;
	}

	public bool AreRelevantChunksBuilt(int bx0, int by0, int bz0, int bx1, int by1, int bz1)
	{
		bx0 = Mathf.Max(0, bx0 - 3);
		by0 = Mathf.Max(0, by0 - 3);
		bz0 = Mathf.Max(0, bz0 - 3);
		bx1 = Mathf.Min(bx1 + 3, data.sizeX - 1);
		by1 = Mathf.Min(by1 + 3, data.sizeX - 1);
		bz1 = Mathf.Min(bz1 + 3, data.sizeX - 1);
		for (int i = bx0 / chunkSize; i <= bx1 / chunkSize; i++)
		{
			for (int j = by0 / chunkSize; j <= by1 / chunkSize; j++)
			{
				for (int k = bz0 / chunkSize; k <= bz1 / chunkSize; k++)
				{
					ChunkState chunkState = chunkWindow[GetChunkWindowSlot(i, j, k)];
					if (!chunkState.IsOccupiedByChunk(i, j, k))
					{
						return false;
					}
					if (chunkState.beingBuilt)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public ChunkState GetChunkState(int cx, int cy, int cz)
	{
		if (!CheckChunk(cx, cy, cz))
		{
			return null;
		}
		ChunkState chunkState = chunkWindow[GetChunkWindowSlot(cx, cy, cz)];
		if (!chunkState.IsOccupiedByChunk(cx, cy, cz))
		{
			return null;
		}
		return chunkState;
	}

	public bool AreAnyChunksBeingBuilt(int sx, int sy, int sz, int ex, int ey, int ez)
	{
		sx = Mathf.Max(0, sx - 3);
		sy = Mathf.Max(0, sy - 3);
		sz = Mathf.Max(0, sz - 3);
		ex = Mathf.Min(ex + 3, data.sizeX - 1);
		ey = Mathf.Min(ey + 3, data.sizeX - 1);
		ez = Mathf.Min(ez + 3, data.sizeX - 1);
		for (int i = sx / chunkSize; i <= ex / chunkSize; i++)
		{
			for (int j = sy / chunkSize; j <= ey / chunkSize; j++)
			{
				for (int k = sz / chunkSize; k <= ez / chunkSize; k++)
				{
					ChunkState chunkState = chunkWindow[GetChunkWindowSlot(i, j, k)];
					if (chunkState.IsOccupiedByChunk(i, j, k) && chunkState.beingBuilt)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public Vector3 DiffLocalToChunk(Vector3 lsFrom, int cx, int cy, int cz)
	{
		return new Vector3(Mathf.Clamp(lsFrom.x, cx * chunkSize, (cx + 1) * chunkSize), Mathf.Clamp(lsFrom.y, cy * chunkSize, (cy + 1) * chunkSize), Mathf.Clamp(lsFrom.z, cz * chunkSize, (cz + 1) * chunkSize)) - lsFrom;
	}

	public float LocalDistSqToChunk(Vector3 lsFrom, int cx, int cy, int cz)
	{
		return DiffLocalToChunk(lsFrom, cx, cy, cz).sqrMagnitude;
	}

	public float LocalDistSqToChunk(Vector3 lsFrom, VoxelandChunk chunk)
	{
		return LocalDistSqToChunk(lsFrom, chunk.cx, chunk.cy, chunk.cz);
	}

	public float LocalDistMaxToChunk(Vector3 lsFrom, VoxelandChunk chunk)
	{
		Vector3 vector = DiffLocalToChunk(lsFrom, chunk.cx, chunk.cy, chunk.cz);
		return Mathf.Max(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
	}

	public void ResetStats()
	{
		stats = new BuildStats();
	}

	public void OnBeforeSerialize()
	{
		if (!disableAutoSerialize && !(data == null) && data.dirty)
		{
			VoxelandSerialData serialData = GetSerialData();
			if (serialData != null)
			{
				data.SerializeInto(serialData);
			}
			else
			{
				data.SerializeOctrees();
			}
			data.dirty = false;
		}
	}

	public void OnAfterDeserialize()
	{
		liveDataStale = true;
	}

	public void PrepareForPrefabbing()
	{
		dynamicRebuilding = false;
		if (GetSerialData() == null)
		{
			base.gameObject.AddComponent<VoxelandSerialData>();
		}
		if (data != null)
		{
			data.SerializeInto(GetSerialData());
		}
	}

	public void PrepareForPrefabApply()
	{
		dynamicRebuilding = false;
		DestroyAllChunks();
		data.SerializeInto(GetSerialData());
	}

	public string CompileTimeCheck()
	{
		if (types == null)
		{
			return "No block types assigned";
		}
		for (int i = 1; i < types.Length; i++)
		{
			string text = types[i].Check();
			if (!string.IsNullOrEmpty(text))
			{
				return $"Block type {i}: {text}";
			}
		}
		return null;
	}
}
