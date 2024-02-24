using System;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf;
using UWE;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

[ProtoContract]
[ProtoInclude(601, typeof(BaseAddBulkheadGhost))]
[ProtoInclude(602, typeof(BaseAddCellGhost))]
[ProtoInclude(603, typeof(BaseAddConnectorGhost))]
[ProtoInclude(604, typeof(BaseAddCorridorGhost))]
[ProtoInclude(605, typeof(BaseAddFaceGhost))]
[ProtoInclude(606, typeof(BaseAddLadderGhost))]
[ProtoInclude(607, typeof(BaseAddWaterPark))]
[ProtoInclude(608, typeof(BaseAddMapRoomGhost))]
[ProtoInclude(609, typeof(BaseAddModuleGhost))]
[ProtoInclude(611, typeof(BaseAddPartitionGhost))]
[ProtoInclude(612, typeof(BaseAddPartitionDoorGhost))]
public class BaseGhost : MonoBehaviour
{
	private static List<Collider> sColliders = new List<Collider>();

	private static List<MonoBehaviour> sMonoBehaviours = new List<MonoBehaviour>();

	private static readonly List<Base.Face> sConnectionFaces = new List<Base.Face>();

	protected static List<BaseExplicitFace> explicitFaces = new List<BaseExplicitFace>();

	[NonSerialized]
	[ProtoMember(1)]
	public Int3 targetOffset;

	protected Base ghostBase;

	protected Base targetBase;

	protected Base prevTargetBase;

	public bool allowedAboveWater = true;

	public const float maxGroundDistance = 25f;

	public const float groundCheckThreshhold = float.PositiveInfinity;

	private static AsyncOperationHandle<GameObject> basePrefabRequest;

	private static GameObject _basePrefab;

	public Base TargetBase => targetBase;

	protected static LayerMask placeLayerMask => ~((1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Trigger")));

	public Base GhostBase => ghostBase;

	public Int3 TargetOffset => targetOffset;

	public static GameObject GetBasePrefab()
	{
		return _basePrefab;
	}

	protected virtual Base.CellType GetCellType()
	{
		return ghostBase.GetCell(Int3.zero);
	}

	public static float GetDistanceToGround(Vector3 position)
	{
		float result = float.PositiveInfinity;
		int layerMask = 1 << LayerID.TerrainCollider;
		QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore;
		if (Physics.Raycast(position, Vector3.down, out var hitInfo, 25f, layerMask, queryTriggerInteraction))
		{
			result = hitInfo.distance;
		}
		return result;
	}

	public static IEnumerator InitializeAsync()
	{
		if (!basePrefabRequest.IsValid())
		{
			basePrefabRequest = AddressablesUtility.LoadAsync<GameObject>("WorldEntities/Structures/Base.prefab");
			yield return basePrefabRequest;
		}
		_basePrefab = basePrefabRequest.Result;
	}

	public void Start()
	{
		BuildBaseGhostModel();
		DisableGhostModelScripts();
		RecalculateBounds();
	}

	public void RecalculateTargetOffset()
	{
		Vector3 point = ghostBase.GridToWorld(Int3.zero);
		targetOffset = targetBase.WorldToGrid(point);
	}

	public virtual void SetupGhost()
	{
	}

	public virtual bool UpdatePlacement(Transform camera, float placeMaxDistance, out bool positionFound, out bool geometryChanged, ConstructableBase ghostModelParentConstructableBase)
	{
		positionFound = false;
		geometryChanged = false;
		ghostBase.isPlaced = false;
		return false;
	}

	public void Place()
	{
		OnPlace();
		ghostBase.FixCorridorLinks();
		ghostBase.RebuildGeometry();
		BuildBaseGhostModel();
		DisableGhostModelScripts();
		RecalculateBounds();
		ghostBase.isPlaced = true;
		ConstructableBase componentInParent = GetComponentInParent<ConstructableBase>();
		if (targetBase != null)
		{
			componentInParent.transform.parent = targetBase.transform;
			targetBase.RegisterBaseGhost(this);
		}
		else if ((bool)LargeWorld.main)
		{
			LargeWorldEntity largeWorldEntity = componentInParent.gameObject.AddComponent<LargeWorldEntity>();
			largeWorldEntity.cellLevel = LargeWorldEntity.CellLevel.Global;
			LargeWorld.main.streamer.cellManager.RegisterEntity(largeWorldEntity);
		}
	}

	public bool PlaceWithForwardCast(Vector3 origin, Vector3 forward, Vector3 halfExtents, float placeDefaultDistance, out Vector3 center)
	{
		bool flag = false;
		Quaternion orientation = Quaternion.LookRotation(new Vector3(forward.x, 0f, forward.z).normalized, Vector3.up);
		int layerMask = 1 << LayerID.TerrainCollider;
		center = origin;
		Vector3 vector = forward;
		if (Physics.BoxCast(center, halfExtents, vector, out var hitInfo, orientation, placeDefaultDistance, layerMask, QueryTriggerInteraction.Ignore))
		{
			SurfaceType surfaceType = Builder.GetSurfaceType(hitInfo.normal);
			float distance = hitInfo.distance;
			Vector3 vector2 = center + vector * distance;
			center = vector2;
			return surfaceType == SurfaceType.Wall;
		}
		center = origin + forward * placeDefaultDistance;
		return true;
	}

	public bool PlaceWithBoundsCast(Vector3 origin, Vector3 forward, Vector3 halfExtents, float placeDefaultDistance, float minHeight, float maxHeight, out Vector3 center)
	{
		bool result = false;
		Quaternion orientation = Quaternion.LookRotation(new Vector3(forward.x, 0f, forward.z).normalized, Vector3.up);
		int layerMask = 1 << LayerID.TerrainCollider;
		center = origin;
		Vector3 vector = forward;
		float maxDistance = placeDefaultDistance;
		if (Physics.BoxCast(center, halfExtents, vector, out var hitInfo, orientation, maxDistance, layerMask, QueryTriggerInteraction.Ignore))
		{
			SurfaceType surfaceType = Builder.GetSurfaceType(hitInfo.normal);
			float distance = hitInfo.distance;
			Vector3 vector2 = center + vector * distance;
			switch (surfaceType)
			{
			case SurfaceType.Ground:
				center = vector2 + Vector3.up * minHeight;
				break;
			case SurfaceType.Wall:
				center = vector2;
				break;
			case SurfaceType.Ceiling:
				center = vector2;
				break;
			}
			if (maxHeight >= 0f)
			{
				vector = Vector3.down;
				maxDistance = maxHeight;
				if (Physics.BoxCast(center, halfExtents, vector, out hitInfo, orientation, maxDistance, layerMask, QueryTriggerInteraction.Ignore))
				{
					result = true;
				}
			}
			else
			{
				result = true;
			}
		}
		else
		{
			center = origin + forward * placeDefaultDistance;
			vector = Vector3.down;
			maxDistance = maxHeight;
			if (Physics.BoxCast(center, halfExtents, vector, out hitInfo, orientation, maxDistance, layerMask, QueryTriggerInteraction.Ignore))
			{
				float distance2 = hitInfo.distance;
				Vector3 vector3 = center + vector * distance2;
				if (distance2 < minHeight)
				{
					center = vector3 + Vector3.up * minHeight;
				}
				result = true;
			}
		}
		if (!allowedAboveWater && center.y > 0f)
		{
			result = false;
		}
		return result;
	}

	public virtual void Finish()
	{
		ConstructableBase componentInParent = GetComponentInParent<ConstructableBase>();
		if (targetBase == null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(_basePrefab, componentInParent.transform.position, componentInParent.transform.rotation);
			if ((bool)LargeWorld.main)
			{
				LargeWorld.main.streamer.cellManager.RegisterEntity(gameObject);
			}
			targetBase = gameObject.GetComponent<Base>();
		}
		targetOffset = targetBase.WorldToGrid(base.transform.position);
		targetBase.DeregisterBaseGhost(this);
		targetBase.CopyFrom(ghostBase, ghostBase.Bounds, targetOffset);
	}

	protected virtual void OnPlace()
	{
	}

	private void RecalculateBounds()
	{
		_ = GetComponentInParent<ConstructableBase>() == null;
	}

	public void ClearTargetBase()
	{
		targetBase = null;
		targetOffset = Int3.zero;
	}

	protected bool IsBlockingHatch(Int3 min, Int3 max)
	{
		Base.Face face = default(Base.Face);
		for (int i = min.x; i <= max.x; i++)
		{
			face.cell = new Int3(i, min.y, min.z - 1);
			face.direction = Base.Direction.North;
			if (targetBase.GetFace(face) == Base.FaceType.Hatch)
			{
				return true;
			}
		}
		for (int j = min.z; j <= max.z; j++)
		{
			face.cell = new Int3(min.x - 1, min.y, j);
			face.direction = Base.Direction.East;
			if (targetBase.GetFace(face) == Base.FaceType.Hatch)
			{
				return true;
			}
		}
		for (int k = min.z; k <= max.z; k++)
		{
			face.cell = new Int3(max.x + 1, min.y, k);
			face.direction = Base.Direction.West;
			if (targetBase.GetFace(face) == Base.FaceType.Hatch)
			{
				return true;
			}
		}
		for (int l = min.x; l <= max.x; l++)
		{
			face.cell = new Int3(l, min.y, max.z + 1);
			face.direction = Base.Direction.South;
			if (targetBase.GetFace(face) == Base.FaceType.Hatch)
			{
				return true;
			}
		}
		for (int m = min.x; m <= max.x; m++)
		{
			for (int n = min.z; n <= max.z; n++)
			{
				face.cell = new Int3(m, max.y + 1, n);
				face.direction = Base.Direction.Below;
				if (targetBase.GetFace(face) == Base.FaceType.Hatch)
				{
					return true;
				}
				face.cell = new Int3(m, min.y - 1, n);
				face.direction = Base.Direction.Above;
				if (targetBase.GetFace(face) == Base.FaceType.Hatch)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void Deconstruct(Base targetBase, Int3.Bounds bounds, Base.Face? face, Base.FaceType faceType)
	{
		Base.CellType cell = targetBase.GetCell(bounds.mins);
		if (cell == Base.CellType.Empty)
		{
			Debug.LogError("Deconstructing empty cell");
		}
		this.targetBase = targetBase;
		if (face.HasValue)
		{
			Base.Face face2 = face.Value;
			Base.Face face3 = default(Base.Face);
			Int3 exit;
			if (faceType != Base.FaceType.Ladder)
			{
				face3 = new Base.Face(face2.cell - bounds.mins, face2.direction);
				ghostBase.SetSize(bounds.size);
				ghostBase.AllocateMasks();
				switch (faceType)
				{
				case Base.FaceType.Partition:
					ghostBase.SetFaceMask(face3, isMasked: true);
					break;
				case Base.FaceType.PartitionDoor:
				{
					Base.Direction[] allDirections = Base.HorizontalDirections;
					foreach (Base.Direction direction2 in allDirections)
					{
						Base.Face face5 = new Base.Face(face2.cell, direction2);
						Base.FaceType face6 = targetBase.GetFace(face5);
						if (face6 == Base.FaceType.Partition || face6 == Base.FaceType.PartitionDoor)
						{
							Base.Face face7 = new Base.Face(face2.cell - bounds.mins, direction2);
							ghostBase.SetFaceMask(face7, isMasked: true);
						}
					}
					break;
				}
				case Base.FaceType.WaterPark:
				{
					int num = ((cell != Base.CellType.LargeRoom && cell != Base.CellType.LargeRoomRotated) ? 1 : 2);
					int index = ((cell == Base.CellType.LargeRoomRotated) ? 2 : 0);
					for (int i = 0; i < num; i++)
					{
						Base.Direction[] allDirections = Base.AllDirections;
						foreach (Base.Direction direction in allDirections)
						{
							Base.Face face4 = new Base.Face(face2.cell - bounds.mins, direction);
							face4.cell[index] += i;
							ghostBase.SetFaceMask(face4, isMasked: true);
						}
					}
					break;
				}
				default:
					ghostBase.SetFaceMask(face3, isMasked: true);
					ghostBase.SetFaceMask(Base.GetAdjacentFace(face3), isMasked: true);
					break;
				}
			}
			else if (targetBase.GetLadderExitCell(face2.cell, face2.direction, out exit))
			{
				Base.Face face8 = new Base.Face(exit, Base.ReverseDirection(face2.direction));
				bounds = bounds.Union(exit);
				if (face2.direction == Base.Direction.Below)
				{
					Base.Face face9 = face2;
					face2 = face8;
					face8 = face9;
				}
				face3 = new Base.Face(face2.cell - bounds.mins, face2.direction);
				Base.Face face10 = new Base.Face(face8.cell - bounds.mins, face8.direction);
				ghostBase.SetSize(bounds.size);
				ghostBase.AllocateMasks();
				ghostBase.SetFaceMask(face3, isMasked: true);
				ghostBase.SetFaceMask(face10, isMasked: true);
				for (int k = 1; k < face10.cell.y; k++)
				{
					Int3 cell2 = face10.cell;
					cell2.y = k;
					Base.Face face11 = new Base.Face(cell2, BaseAddLadderGhost.ladderFaceDir);
					ghostBase.SetFaceMask(face11, isMasked: true);
				}
			}
			else
			{
				Debug.LogError("Could not find ladder exit");
			}
		}
		targetOffset = bounds.mins;
		ghostBase.CopyFrom(targetBase, bounds, targetOffset * -1);
		BuildBaseGhostModel();
		DisableGhostModelScripts();
		RecalculateBounds();
	}

	protected bool ObservatoryAcceptsCorridors(Int3 cell)
	{
		Base.Direction[] horizontalDirections = Base.HorizontalDirections;
		foreach (Base.Direction direction in horizontalDirections)
		{
			if (targetBase.GetFace(new Base.Face(cell, direction)) != Base.FaceType.Solid)
			{
				return false;
			}
		}
		return true;
	}

	private bool IsMoonpoolObstructingType(Base.CellType cellType)
	{
		if (cellType == Base.CellType.Room || cellType == Base.CellType.LargeRoom || cellType == Base.CellType.LargeRoomRotated || cellType == Base.CellType.Moonpool || cellType == Base.CellType.MoonpoolRotated)
		{
			return true;
		}
		return false;
	}

	protected bool WouldCreateMoonpoolObstructions(Base.CellType cellType, Int3 cell)
	{
		Int3 @int = Base.CellSize[(uint)cellType];
		Int3 int2 = cell + @int - 1;
		if (IsMoonpoolObstructingType(cellType))
		{
			foreach (Int3 item in new Int3.Bounds(new Int3(cell.x, cell.y + 1, cell.z), new Int3(int2.x, cell.y + 2, int2.z)))
			{
				Base.CellType cell2 = targetBase.GetCell(item);
				if (cell2 == Base.CellType.Moonpool || cell2 == Base.CellType.MoonpoolRotated)
				{
					return true;
				}
			}
		}
		if (cellType == Base.CellType.Moonpool || cellType == Base.CellType.MoonpoolRotated)
		{
			foreach (Int3 item2 in new Int3.Bounds(new Int3(cell.x, cell.y - 2, cell.z), new Int3(int2.x, cell.y - 1, int2.z)))
			{
				Base.CellType cell3 = targetBase.GetCell(item2);
				if (IsMoonpoolObstructingType(cell3))
				{
					return true;
				}
			}
		}
		return false;
	}

	protected bool WouldHaveProhibitedCorridors()
	{
		GetCellType();
		Int3 zero = Int3.zero;
		sConnectionFaces.Clear();
		ghostBase.GetAllCellConnections(zero, sConnectionFaces);
		for (int i = 0; i < sConnectionFaces.Count; i++)
		{
			Base.Face face = sConnectionFaces[i];
			Base.Face face2 = new Base.Face(targetOffset + Base.GetAdjacent(face.cell - zero, face.direction), Base.ReverseDirection(face.direction));
			if (targetBase.GetCell(face2.cell) == Base.CellType.Observatory && !ObservatoryAcceptsCorridors(face2.cell))
			{
				return true;
			}
		}
		return false;
	}

	public virtual void AttachCorridorConnectors(bool disableColliders = true)
	{
		Base.CellType cellType = GetCellType();
		Int3 zero = Int3.zero;
		sConnectionFaces.Clear();
		if (cellType != Base.CellType.Observatory)
		{
			ghostBase.GetAllCellConnections(zero, sConnectionFaces);
		}
		else
		{
			Base.Direction[] horizontalDirections = Base.HorizontalDirections;
			foreach (Base.Direction direction in horizontalDirections)
			{
				if (targetBase.IsValidObsConnection(Base.GetAdjacent(targetOffset, direction), Base.ReverseDirection(direction)))
				{
					sConnectionFaces.Add(new Base.Face(zero, direction));
					break;
				}
			}
		}
		for (int j = 0; j < sConnectionFaces.Count; j++)
		{
			Base.Face face = sConnectionFaces[j];
			Base.Face face2 = new Base.Face(targetOffset + Base.GetAdjacent(face.cell - zero, face.direction), Base.ReverseDirection(face.direction));
			if ((targetBase.GetCellConnections(face2.cell) & (1 << (int)face2.direction)) == 0)
			{
				continue;
			}
			Base.FaceType face3 = ghostBase.GetFace(face);
			Base.FaceType face4 = targetBase.GetFace(face2);
			bool flag = targetBase.HasGhostFace(face2);
			if ((face3 == Base.FaceType.Solid || face3 == Base.FaceType.None || Base.IsBulkhead(face3)) && (face4 == Base.FaceType.Solid || face4 == Base.FaceType.None || Base.IsBulkhead(face4)) && !flag)
			{
				Base.Piece piece = ghostBase.GetPiece(face, Base.FaceType.None);
				Transform transform = ghostBase.SpawnCorridorConnector(piece, face, face.cell);
				if (transform != null && disableColliders)
				{
					DisableColliders(transform);
				}
				Base.Piece piece2 = targetBase.GetPiece(face2, Base.FaceType.None);
				Transform transform2 = ghostBase.SpawnCorridorConnector(piece2, new Base.Face(face2.cell - targetOffset, face2.direction), face.cell);
				if (transform2 != null && disableColliders)
				{
					DisableColliders(transform2);
				}
			}
		}
		sConnectionFaces.Clear();
	}

	public void RebuildGhostGeometry(bool disableCollison = true)
	{
		ghostBase.RebuildGeometry();
		if (disableCollison)
		{
			DisableColliders(base.transform);
		}
		BuildBaseGhostModel();
		DisableGhostModelScripts();
		RecalculateBounds();
	}

	private void BuildBaseGhostModel()
	{
		IBaseGhostModel[] componentsInChildren = GetComponentsInChildren<IBaseGhostModel>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].BuildModel(isGhost: true);
		}
	}

	private static void DisableColliders(Transform target)
	{
		target.GetComponentsInChildren(includeInactive: true, sColliders);
		for (int i = 0; i < sColliders.Count; i++)
		{
			sColliders[i].enabled = false;
		}
		sColliders.Clear();
	}

	private void DisableGhostModelScripts()
	{
		GetComponentsInChildren(includeInactive: false, sMonoBehaviours);
		for (int i = 0; i < sMonoBehaviours.Count; i++)
		{
			MonoBehaviour monoBehaviour = sMonoBehaviours[i];
			if (monoBehaviour.gameObject != base.gameObject)
			{
				monoBehaviour.enabled = false;
			}
		}
		sMonoBehaviours.Clear();
	}

	protected static Base FindBase(Transform camera, float searchDistance = 20f)
	{
		if (Physics.SphereCast(new Ray(camera.position, camera.forward), 0.5f, out var hitInfo, searchDistance, placeLayerMask.value))
		{
			Base componentInParent = hitInfo.collider.GetComponentInParent<Base>();
			if (componentInParent != null && componentInParent.GetComponent<BaseGhost>() == null)
			{
				return componentInParent;
			}
		}
		int num = UWE.Utils.OverlapSphereIntoSharedBuffer(camera.position + camera.forward * searchDistance * 0.5f, searchDistance * 0.5f, placeLayerMask.value);
		for (int i = 0; i < num; i++)
		{
			Base componentInParent2 = UWE.Utils.sharedColliderBuffer[i].GetComponentInParent<Base>();
			if (componentInParent2 != null && componentInParent2.GetComponent<BaseGhost>() == null)
			{
				return componentInParent2;
			}
		}
		return null;
	}

	public virtual void Awake()
	{
		targetBase = base.transform.parent.GetComponentInParent<Base>();
		ghostBase = GetComponent<Base>();
		ghostBase.onPostRebuildGeometry += OnPostRebuildGeometry;
		ghostBase.isGhost = true;
		if ((bool)targetBase)
		{
			targetBase.RegisterBaseGhost(this);
		}
	}

	private void OnDestroy()
	{
		if (targetBase != null)
		{
			targetBase.DeregisterBaseGhost(this);
			targetBase.RebuildGhostBases();
		}
		if ((bool)ghostBase)
		{
			ghostBase.onPostRebuildGeometry -= OnPostRebuildGeometry;
		}
	}

	private void OnPostRebuildGeometry(Base b)
	{
		if ((bool)targetBase && ghostBase.GetCellMask(Int3.zero))
		{
			AttachCorridorConnectors(disableColliders: false);
		}
	}

	public virtual void FindOverlappedObjects(List<GameObject> overlappedObjects)
	{
	}
}
