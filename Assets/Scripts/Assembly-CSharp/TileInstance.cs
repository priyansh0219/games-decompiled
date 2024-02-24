using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
public class TileInstance : MonoBehaviour
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct CompareByLayer : IComparer<TileInstance>
	{
		public int Compare(TileInstance x, TileInstance y)
		{
			if (x.layer < y.layer)
			{
				return -1;
			}
			if (x.layer == y.layer)
			{
				return 0;
			}
			return 1;
		}
	}

	public static bool outputSpawnTimes = false;

	public static int ShowGizmosLayerFilter = -1;

	public static bool boxGizmos = true;

	public static bool eyedropperGizmos = false;

	[ProtoMember(1)]
	public string resourcePath;

	[ProtoMember(2)]
	public Int3 origin = Int3.zero;

	[ProtoMember(3)]
	public Int3 gridOffset = Int3.zero;

	[ProtoMember(4)]
	public int gridSize = 16;

	[ProtoMember(5)]
	public int turns;

	[ProtoMember(6)]
	public VoxelBlendMode blendMode;

	[ProtoMember(7)]
	public byte multiplyPassiveType;

	[ProtoMember(8)]
	public int layer;

	[ProtoMember(9)]
	public bool clearHeightmap;

	[DoNotSerialize]
	public LargeWorld world;

	[DoNotSerialize]
	[HideInInspector]
	public GameObject previewObjectsRoot;

	[DoNotSerialize]
	private byte[] typeMap;

	private TempCollisionDisabler colDisabler = new TempCollisionDisabler();

	private UniqueIdentifier[] _entities;

	private bool preparedPlayModeSpawns;

	private Dictionary<UniqueIdentifier, PureTransform> ent2transform = new Dictionary<UniqueIdentifier, PureTransform>();

	public Vector3 wsBlockOrigin => world.land.transform.TransformPoint(origin.ToVector3());

	[DoNotSerialize]
	[HideInInspector]
	public bool prepared { get; private set; }

	[DoNotSerialize]
	[HideInInspector]
	public GameObject prefab { get; private set; }

	public VoxelandData srcData { get; private set; }

	public Voxeland srcLand { get; private set; }

	public Int3.Bounds blockBounds
	{
		get
		{
			if (!prepared)
			{
				Debug.LogError("blockBounds requested when instance was not prepared.");
			}
			return new Int3.Bounds(origin, origin + sizeBlocks - 1);
		}
	}

	public Int3 sizeBlocks => srcData.GetSize().RotateXZ(turns).Abs();

	public Bounds wsBounds
	{
		get
		{
			Int3.Bounds bounds = blockBounds;
			Vector3 vector = world.land.transform.TransformPoint(bounds.mins.ToVector3());
			Vector3 vector2 = world.land.transform.TransformPoint((bounds.maxs + 1).ToVector3());
			return new Bounds((vector + vector2) / 2f, vector2 - vector);
		}
	}

	public Vector3 cornerCorrection
	{
		get
		{
			Vector3 vector = srcData.GetSize().ToVector3();
			Vector3 rhs = Quaternion.AngleAxis((float)turns * -90f, Vector3.up) * vector;
			return -Vector3.Min(Vector3.zero, rhs);
		}
	}

	private UniqueIdentifier[] Entities
	{
		get
		{
			if (_entities == null)
			{
				_entities = (from p in prefab.GetComponentsInChildren<UniqueIdentifier>(includeInactive: true)
					where (object)p.gameObject.GetComponent<Voxeland>() == null
					select p).ToArray();
			}
			return _entities;
		}
	}

	public static event EventHandler onSpawn;

	public void SetOrigin(Int3 newOrigin)
	{
		origin = newOrigin;
		PositionEditorEntityPreview();
	}

	private void Awake()
	{
	}

	public void Rasterize(byte[,,] windowOut, byte[,,] bulgeOut, Int3 globalWindowOrigin, int downsamples)
	{
		throw new Exception("Function deprecated - do not use or edit. (steve@unknownworlds.com) 7/16/2014 8:59:47 PM");
	}

	public byte GetTypeForDest(byte type)
	{
		if (typeMap == null)
		{
			return type;
		}
		return typeMap[type];
	}

	public void CopyIntoRoot(VoxelandData dest, Int3 root)
	{
		if (!prepared)
		{
			return;
		}
		if (blendMode == VoxelBlendMode.None)
		{
			ClearRoot(dest, root);
		}
		VoxelandData.OctNode.BlendArgs args = new VoxelandData.OctNode.BlendArgs((blendMode != 0 && blendMode != VoxelBlendMode.Additive && (blendMode == VoxelBlendMode.Multiply || blendMode == VoxelBlendMode.MultiplyKeepSource)) ? VoxelandData.OctNode.BlendOp.Intersection : VoxelandData.OctNode.BlendOp.Union, blendMode == VoxelBlendMode.Multiply, (blendMode == VoxelBlendMode.Multiply) ? GetTypeForDest(multiplyPassiveType) : Convert.ToByte(0));
		Int3.Bounds bounds = blockBounds;
		Int3.Bounds bounds2 = new Int3.Bounds(root * dest.biggestNode, (root + 1) * dest.biggestNode - 1);
		if (!bounds.Intersects(bounds2))
		{
			return;
		}
		try
		{
			foreach (Int3 item in bounds.IntersectionWith(bounds2))
			{
				Int3 @int = Int3.InverseTileTransform(item, srcData.GetSize(), origin, turns);
				VoxelandData.OctNode node = dest.GetNode(item.x, item.y, item.z);
				VoxelandData.OctNode node2 = srcData.GetNode(@int.x, @int.y, @int.z);
				node2.type = GetTypeForDest(node2.type);
				dest.SetNodeFast(item.x, item.y, item.z, VoxelandData.OctNode.Blend(node, node2, args));
			}
		}
		catch (Exception exception)
		{
			Debug.Log(string.Concat("Exception while copying tile into range ", bounds2, ". tile range = ", bounds));
			Debug.LogException(exception, this);
		}
	}

	public void ClearRoot(VoxelandData dest, Int3 root)
	{
		if (!prepared)
		{
			return;
		}
		Int3.Bounds bounds = blockBounds.Expanded(1);
		Int3.Bounds other = new Int3.Bounds(root * dest.biggestNode, (root + 1) * dest.biggestNode - 1);
		if (!bounds.Intersects(other))
		{
			return;
		}
		Bounds bounds2 = default(Bounds);
		bounds2.SetMinMax(blockBounds.mins.ToVector3(), (blockBounds.maxs + 1).ToVector3());
		VoxelandData.OctNode.BlendArgs args = new VoxelandData.OctNode.BlendArgs(VoxelandData.OctNode.BlendOp.Subtraction, replaceTypes: false, 0);
		foreach (Int3 item in bounds.IntersectionWith(other))
		{
			Vector3 p = item.ToVector3() + UWE.Utils.half3;
			VoxelandData.OctNode node = dest.GetNode(item.x, item.y, item.z);
			VoxelandData.OctNode n = default(VoxelandData.OctNode);
			n.type = 1;
			n.density = VoxelandData.OctNode.EncodeDensity(VoxelandMisc.SignedDistToBox(bounds2, p));
			dest.SetNodeFast(item.x, item.y, item.z, VoxelandData.OctNode.Blend(node, n, args));
		}
	}

	public HashSet<VoxelandBlockType> FindMissingTypes(GameObject prefab, Voxeland destLand)
	{
		HashSet<VoxelandBlockType> hashSet = new HashSet<VoxelandBlockType>();
		Voxeland[] componentsInChildren = prefab.GetComponentsInChildren<Voxeland>();
		foreach (Voxeland voxeland in componentsInChildren)
		{
			for (int j = 1; j < voxeland.types.Length; j++)
			{
				VoxelandBlockType query = voxeland.types[j];
				if (destLand.FindEquivalentType(query) != -1)
				{
					continue;
				}
				bool flag = true;
				foreach (VoxelandBlockType item in hashSet)
				{
					if (item.IsVisuallySame(voxeland.types[j]))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					hashSet.Add(voxeland.types[j]);
				}
			}
		}
		return hashSet;
	}

	private Vector3 TileToGlobalPoint(Vector3 p)
	{
		Vector3 position = srcLand.transform.InverseTransformPoint(p);
		return previewObjectsRoot.transform.TransformPoint(position);
	}

	private GameObject CreateObjectPreview(GameObject prefabObj, Transform trans)
	{
		using (new StreamTiming.Block("CreateObjectPreview"))
		{
			Timer.Begin("CreateObjectPreview", 1000f);
			colDisabler.DisableColliders(prefabObj);
			GameObject gameObject = UnityEngine.Object.Instantiate(prefabObj);
			colDisabler.RestoreColliders();
			gameObject.name += "(preview - do not edit)";
			gameObject.SetHideFlagRecursive(HideFlags.NotEditable);
			Vector3 origPoint = srcLand.transform.InverseTransformPoint(trans.position);
			origPoint = UWE.Utils.CornerBoxRotatePoint(Vector3.zero, srcData.GetSize().ToVector3(), turns, origPoint);
			gameObject.transform.parent = previewObjectsRoot.transform;
			gameObject.transform.localPosition = origPoint;
			gameObject.transform.localRotation = Quaternion.AngleAxis((float)turns * -90f, Vector3.up) * trans.localRotation;
			gameObject.transform.localScale = trans.localScale;
			float num = Timer.End();
			if (outputSpawnTimes)
			{
				using (StreamWriter streamWriter = File.AppendText("previewCreateTimes.csv"))
				{
					streamWriter.WriteLine(gameObject.gameObject.name + "," + num);
				}
			}
			if (TileInstance.onSpawn != null)
			{
				TileInstance.onSpawn(this, null);
			}
			return gameObject;
		}
	}

	private void InEditorPreviewSpawn(GameObject prefab, bool bounded, Bounds bounds)
	{
		Voxeland component = prefab.GetComponent<Voxeland>();
		PrefabIdentifier component2 = prefab.GetComponent<PrefabIdentifier>();
		StoreInformationIdentifier component3 = prefab.GetComponent<StoreInformationIdentifier>();
		if (component != null)
		{
			return;
		}
		if (component2 != null)
		{
			if (!bounded || bounds.Contains(TileToGlobalPoint(prefab.transform.position)))
			{
				if (PrefabDatabase.GetPrefabAsync(component2.ClassId).TryGetPrefab(out var prefabObj))
				{
					UnityEngine.Object.DestroyImmediate(CreateObjectPreview(prefabObj, prefab.transform).GetComponent<PrefabIdentifier>());
					return;
				}
				Debug.LogError("Could not find world entity prefab referenced by '" + prefab.name + "' inside tile '" + resourcePath + "'. ClassId = " + component2.ClassId);
			}
			return;
		}
		if (component3 != null)
		{
			if (!bounded || bounds.Contains(TileToGlobalPoint(prefab.transform.position)))
			{
				UnityEngine.Object.DestroyImmediate(CreateObjectPreview(prefab, prefab.transform).GetComponent<StoreInformationIdentifier>());
			}
			return;
		}
		foreach (Transform item in prefab.transform)
		{
			InEditorPreviewSpawn(item.gameObject, bounded, bounds);
		}
	}

	public void ClearEditorEntityPreview()
	{
	}

	public void SetPreviewRenderersEnabled(bool val)
	{
		Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = val;
		}
	}

	public void Prepare(bool showPreviewEnts = true, bool bounded = false, Bounds entPreviewBounds = default(Bounds))
	{
		Debug.LogError("TileInstance.Prepare called in play-mode! This is bad - we shouldn't rely on loading tiles in play mode, since that can cause hitches. Everything should be cached in the world folder.");
		if (prepared || !(resourcePath != ""))
		{
			return;
		}
		typeMap = null;
		if (resourcePath.IndexOf("WorldTiles") == -1)
		{
			Debug.LogError("This tile (" + base.gameObject.GetFullHierarchyPath() + ") refers to a tile prefab that is not in the WorldTiles folder. Res path = " + resourcePath);
		}
		if (prefab == null)
		{
			Debug.LogError("could not find prefab with given resource path: " + resourcePath);
			return;
		}
		srcLand = null;
		Voxeland[] componentsInChildren = prefab.GetComponentsInChildren<Voxeland>(includeInactive: true);
		foreach (Voxeland voxeland in componentsInChildren)
		{
			if (voxeland.gameObject != prefab)
			{
				if ((double)voxeland.transform.localEulerAngles.magnitude > 0.0001)
				{
					Debug.LogError("The voxeland child " + voxeland.gameObject.name + " inside the tile prefab " + prefab.name + " has some local rotation - this is not allowed for tiles used in instancing!");
					return;
				}
				if ((double)(voxeland.transform.localScale - new Vector3(1f, 1f, 1f)).magnitude > 0.0001)
				{
					Debug.LogError("The voxeland child " + voxeland.gameObject.name + " inside the tile prefab " + prefab.name + " has some local non-1 scale - this is not allowed for tiles used in instancing!");
					return;
				}
			}
			if (srcLand != null)
			{
				Debug.LogError("Found more than one voxeland inside tile! Not allowed. Prefab path = " + resourcePath);
				break;
			}
			if (voxeland.paletteResourceDir != null && voxeland.paletteResourceDir != "" && voxeland.paletteResourceDir == world.land.paletteResourceDir)
			{
				typeMap = null;
			}
			else
			{
				List<byte> list = world.land.MergeTypes(voxeland.types, errorIfAnyNew: true, prefab);
				if (list == null)
				{
					Debug.LogError("Tile " + resourcePath + " had types that were not in the global list!");
					srcLand = null;
					break;
				}
				typeMap = list.ToArray();
			}
			srcLand = voxeland;
			if (srcLand.GetComponent<VoxelandSerialData>() == null)
			{
				Debug.LogError("TileInstance referred to prefab " + resourcePath + ", but the prefab's voxeland had no VoxelandSerialData component - was it prepared as a prefab?");
				return;
			}
			if (!srcLand.liveDataStale && (srcLand.data == null || srcLand.data.roots == null))
			{
				srcLand.liveDataStale = true;
			}
			srcLand.UpdateData();
			srcData = srcLand.data;
		}
		if (srcLand == null)
		{
			Debug.LogError("No Voxeland found in world tile or a block type mismatch occurred! (Click this error to select the prefab in the Project)", prefab);
			typeMap = null;
			prepared = false;
		}
		else
		{
			prepared = true;
		}
	}

	public void PreparePlayModeSpawns()
	{
		if (!(prefab == null))
		{
			Vector3 vector = world.land.transform.TransformPoint(origin.ToVector3());
			Quaternion quaternion = Quaternion.AngleAxis((float)turns * -90f, Vector3.up);
			Vector3 vector2 = cornerCorrection;
			Quaternion quaternion2 = Quaternion.Inverse(prefab.transform.rotation);
			UniqueIdentifier[] entities = Entities;
			foreach (UniqueIdentifier uniqueIdentifier in entities)
			{
				Transform transform = uniqueIdentifier.transform;
				Vector3 vector3 = srcLand.transform.InverseTransformPoint(transform.position);
				Vector3 vector4 = quaternion * vector3;
				Vector3 pos = vector + vector4 + vector2;
				Quaternion rot = quaternion * quaternion2 * transform.rotation;
				ent2transform[uniqueIdentifier] = new PureTransform(pos, rot, transform.localScale);
			}
			preparedPlayModeSpawns = true;
		}
	}

	private void PositionEditorEntityPreview()
	{
		if (previewObjectsRoot != null)
		{
			previewObjectsRoot.transform.position = world.land.transform.TransformPoint(origin.ToVector3());
		}
	}

	public void CreateEditorEntityPreview(bool bounded, Bounds bounds)
	{
		ClearEditorEntityPreview();
		base.transform.position = wsBlockOrigin;
		previewObjectsRoot = new GameObject("preview root");
		previewObjectsRoot.hideFlags |= HideFlags.NotEditable;
		previewObjectsRoot.transform.parent = base.transform;
		PositionEditorEntityPreview();
		InEditorPreviewSpawn(prefab, bounded, bounds);
	}

	public void ResetInstance()
	{
		ClearEditorEntityPreview();
		prepared = false;
	}

	private void OnDrawGizmos()
	{
		if ((!Event.current.shift || !Event.current.control) && boxGizmos && (ShowGizmosLayerFilter == -1 || ShowGizmosLayerFilter == layer))
		{
			if (!prepared || world == null || world.land == null)
			{
				Gizmos.color = Color.red.ToAlpha(0.5f);
				Vector3 vector = new Vector3(16f, 16f, 16f);
				Gizmos.DrawCube(base.transform.position + vector / 2f, vector);
			}
			else
			{
				Bounds bounds = wsBounds;
				Gizmos.color = ((blendMode == VoxelBlendMode.None) ? Color.blue : ((blendMode == VoxelBlendMode.Additive) ? Color.green : Color.yellow)).ToAlpha(0.2f);
				Gizmos.DrawCube(bounds.center, bounds.size);
			}
		}
	}
}
