using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

[ProtoContract]
public class BaseDeconstructable : MonoBehaviour, ICompileTimeCheckable
{
	private static List<IObstacle> sObstacles = new List<IObstacle>();

	private static List<IObstacle> sMapObstacles = new List<IObstacle>();

	private static List<GameObject> sObstacleGameObjects = new List<GameObject>();

	private static List<OrientedBounds> sBounds = new List<OrientedBounds>();

	private static StringBuilder sb = new StringBuilder();

	private static AsyncOperationHandle<GameObject> baseDeconstructablePrefabRequest;

	private static GameObject baseDeconstructablePrefab;

	[AssertLocalization]
	private const string deconstructFailedKey = "DeconstructFailed";

	[AssertLocalization]
	private const string deconstructGhostErrorKey = "DeconstructGhostError";

	[AssertLocalization]
	private const string deconstructAttachedErrorKey = "DeconstructAttachedError";

	[AssertLocalization]
	private const string deconstructObstacleFormat = "DeconstructObstacle";

	public Base.Direction hintDirection = Base.Direction.Count;

	[NonSerialized]
	[ProtoMember(1)]
	public Int3.Bounds bounds;

	[NonSerialized]
	[ProtoMember(2)]
	public Base.Face? face;

	[NonSerialized]
	[ProtoMember(3)]
	public Base.FaceType faceType;

	public List<OrientedBounds> basePiecesBounds = new List<OrientedBounds>();

	private TechType recipe;

	private Base.Face? moduleFace;

	private Base deconstructedBase;

	private static Transform allowedTransform;

	public string Name => recipe.AsString();

	public void ShiftCell(Int3 offset)
	{
		bounds.mins += offset;
		bounds.maxs += offset;
		if (face.HasValue)
		{
			Base.Face value = face.Value;
			value.cell += offset;
			face = value;
		}
	}

	public static BaseDeconstructable MakeCellDeconstructable(Transform geometry, Int3.Bounds bounds, TechType recipe)
	{
		BaseDeconstructable baseDeconstructable = geometry.gameObject.EnsureComponent<BaseDeconstructable>();
		baseDeconstructable.recipe = recipe;
		baseDeconstructable.bounds = bounds;
		baseDeconstructable.face = null;
		baseDeconstructable.faceType = Base.FaceType.None;
		baseDeconstructable.basePiecesBounds.Clear();
		return baseDeconstructable;
	}

	public static BaseDeconstructable MakeFaceDeconstructable(Transform geometry, Int3.Bounds bounds, Base.Face face, Base.FaceType faceType, TechType recipe)
	{
		BaseDeconstructable baseDeconstructable = geometry.gameObject.EnsureComponent<BaseDeconstructable>();
		baseDeconstructable.Init(bounds, face, faceType, recipe);
		return baseDeconstructable;
	}

	public void Init(Int3.Bounds bounds, Base.Face face, Base.FaceType faceType, TechType recipe)
	{
		this.recipe = recipe;
		this.bounds = bounds;
		this.face = face;
		this.faceType = faceType;
	}

	public static IEnumerator InitializeAsync()
	{
		if (!baseDeconstructablePrefabRequest.IsValid())
		{
			baseDeconstructablePrefabRequest = AddressablesUtility.LoadAsync<GameObject>("Base/Ghosts/BaseDeconstructable.prefab");
			yield return baseDeconstructablePrefabRequest;
		}
		baseDeconstructablePrefab = baseDeconstructablePrefabRequest.Result;
	}

	private bool IsBulkheadConnected()
	{
		if (deconstructedBase == null)
		{
			return false;
		}
		if (deconstructedBase.IsBulkheadConnected(bounds))
		{
			return true;
		}
		if (deconstructedBase.IsGhostBulkheadConntected(bounds))
		{
			return true;
		}
		return false;
	}

	public void Awake()
	{
		deconstructedBase = GetComponentInParent<Base>();
	}

	public bool DeconstructionAllowed(out string reason)
	{
		reason = null;
		_ = Player.main;
		Language main = Language.main;
		if (!base.enabled)
		{
			return false;
		}
		sb.Length = 0;
		sb.Append(main.Get("DeconstructFailed"));
		int maxReasons = 1;
		GetComponentsInChildren(includeInactive: true, sObstacles);
		if (recipe == TechType.BaseMapRoom)
		{
			MapRoomFunctionality mapRoomFunctionalityForCell = deconstructedBase.GetMapRoomFunctionalityForCell(this.bounds.mins);
			if (mapRoomFunctionalityForCell != null)
			{
				mapRoomFunctionalityForCell.GetComponentsInChildren(includeInactive: true, sMapObstacles);
				sObstacles.AddRange(sMapObstacles);
				sMapObstacles.Clear();
			}
		}
		bool reasonDefined;
		bool num = CheckObstacles(sObstacles, ref maxReasons, out reasonDefined, sb);
		sObstacles.Clear();
		if (!num)
		{
			reason = sb.ToString();
			return false;
		}
		if (recipe == TechType.None)
		{
			return false;
		}
		if (deconstructedBase == null)
		{
			return false;
		}
		foreach (Int3 bound in this.bounds)
		{
			if (deconstructedBase.IsCellUnderConstruction(bound))
			{
				reason = main.Get("DeconstructGhostError");
				return false;
			}
		}
		if (!this.face.HasValue)
		{
			foreach (Int3 bound2 in this.bounds)
			{
				if (deconstructedBase.GetAreCellFacesUsed(bound2))
				{
					reason = main.Get("DeconstructAttachedError");
					return false;
				}
			}
			if (recipe == TechType.BaseFoundation && this.bounds.maxs.y < deconstructedBase.Bounds.maxs.y)
			{
				Int3.Bounds bounds = this.bounds;
				bounds.mins.y = bounds.maxs.y + 1;
				bounds.maxs.y = bounds.maxs.y + 1;
				foreach (Int3 item in bounds)
				{
					if (!deconstructedBase.IsCellEmpty(item))
					{
						reason = main.Get("DeconstructAttachedError");
						return false;
					}
				}
			}
			if (IsBulkheadConnected())
			{
				reason = main.Get("DeconstructAttachedError");
				return false;
			}
		}
		Builder.CacheBounds(base.transform, base.gameObject, sBounds);
		sBounds.AddRange(basePiecesBounds);
		if (!this.face.HasValue)
		{
			foreach (Int3 bound3 in this.bounds)
			{
				int cellConnections = deconstructedBase.GetCellConnections(bound3);
				if (cellConnections == 0)
				{
					continue;
				}
				Base.Direction[] allDirections = Base.AllDirections;
				foreach (Base.Direction direction in allDirections)
				{
					int num2 = 1 << (int)direction;
					if ((cellConnections & num2) == 0)
					{
						continue;
					}
					Base.Face face = new Base.Face(bound3, direction);
					Base.Face adjacentFace = Base.GetAdjacentFace(face);
					int cellConnections2 = deconstructedBase.GetCellConnections(adjacentFace.cell);
					Base.Direction direction2 = Base.ReverseDirection(direction);
					Base.FaceType faceType = deconstructedBase.GetFace(face);
					Base.FaceType faceType2 = deconstructedBase.GetFace(adjacentFace);
					if ((faceType == Base.FaceType.Solid || faceType == Base.FaceType.None || Base.IsBulkhead(faceType)) && (faceType2 == Base.FaceType.Solid || faceType2 == Base.FaceType.None || Base.IsBulkhead(faceType2)) && (cellConnections2 & (1 << (int)direction2)) != 0)
					{
						Transform transform = deconstructedBase.FindFaceObject(face);
						if (transform != null)
						{
							Builder.CacheBounds(base.transform, transform.gameObject, sBounds, append: true);
						}
						Transform transform2 = deconstructedBase.FindFaceObject(adjacentFace);
						if (transform2 != null)
						{
							Builder.CacheBounds(base.transform, transform2.gameObject, sBounds, append: true);
						}
					}
				}
			}
		}
		try
		{
			allowedTransform = base.transform;
			Builder.GetObstacles(base.transform.position, base.transform.rotation, sBounds, FilterDeconstructionObstacles, sObstacleGameObjects);
		}
		finally
		{
			allowedTransform = null;
		}
		bool flag = true;
		if (sObstacleGameObjects.Count > 0)
		{
			for (int j = 0; j < sObstacleGameObjects.Count; j++)
			{
				GameObject gameObject = sObstacleGameObjects[j];
				gameObject.GetComponentsInChildren(includeInactive: true, sObstacles);
				for (int num3 = sObstacles.Count - 1; num3 >= 0; num3--)
				{
					if (!sObstacles[num3].IsDeconstructionObstacle())
					{
						sObstacles.RemoveAt(num3);
					}
				}
				if (sObstacles.Count <= 0)
				{
					continue;
				}
				flag = false;
				CheckObstacles(sObstacles, ref maxReasons, out reasonDefined, sb);
				if (maxReasons <= 0)
				{
					break;
				}
				if (reasonDefined)
				{
					continue;
				}
				TechType techType = CraftData.GetTechType(gameObject);
				if (techType == TechType.None)
				{
					ConstructableBase component = gameObject.GetComponent<ConstructableBase>();
					if (component != null)
					{
						techType = component.techType;
					}
				}
				string arg = ((techType != 0) ? main.Get(techType) : gameObject.name);
				sb.Append(main.GetFormat("DeconstructObstacle", arg));
				maxReasons--;
			}
			sObstacleGameObjects.Clear();
			sObstacles.Clear();
			reason = sb.ToString();
			sb.Length = 0;
		}
		if (flag)
		{
			reason = null;
		}
		return flag;
	}

	private static bool FilterDeconstructionObstacles(Collider collider)
	{
		if (collider.gameObject.layer == LayerID.TerrainCollider)
		{
			return true;
		}
		if (!(allowedTransform == null))
		{
			return collider.transform.IsChildOf(allowedTransform);
		}
		return true;
	}

	private bool CheckObstacles(List<IObstacle> obstacles, ref int maxReasons, out bool reasonDefined, StringBuilder sb)
	{
		reasonDefined = false;
		bool result = true;
		for (int i = 0; i < obstacles.Count; i++)
		{
			if (obstacles[i].CanDeconstruct(out var reason))
			{
				continue;
			}
			result = false;
			if (!string.IsNullOrEmpty(reason))
			{
				reasonDefined = true;
				sb.Append(reason);
				maxReasons--;
				if (maxReasons <= 0)
				{
					break;
				}
			}
		}
		return result;
	}

	public void Deconstruct()
	{
		Base componentInParent = GetComponentInParent<Base>();
		if (componentInParent == null)
		{
			Debug.LogError("BaseDeconstructable without a Base");
			return;
		}
		Vector3 position = componentInParent.GridToWorld(bounds.mins);
		GameObject obj = UnityEngine.Object.Instantiate(baseDeconstructablePrefab, position, componentInParent.transform.rotation);
		ConstructableBase component = obj.GetComponent<ConstructableBase>();
		BaseGhost component2 = component.model.GetComponent<BaseGhost>();
		component2.Deconstruct(componentInParent, bounds, face, faceType);
		component2.GhostBase.isPlaced = true;
		obj.transform.position = componentInParent.GridToWorld(component2.TargetOffset);
		component.techType = recipe;
		component.SetState(value: false, setAmount: false);
		if (face.HasValue)
		{
			componentInParent.ClearFace(face.Value, faceType);
		}
		else
		{
			componentInParent.ClearCell(bounds.mins);
		}
		component.LinkModule(moduleFace);
		if (componentInParent.IsEmpty())
		{
			componentInParent.OnPreDestroy();
			UnityEngine.Object.Destroy(componentInParent.gameObject);
			component2.ClearTargetBase();
			if ((bool)LargeWorld.main)
			{
				LargeWorld.main.streamer.cellManager.RegisterEntity(component.gameObject);
			}
		}
		else
		{
			component.transform.parent = componentInParent.transform;
			componentInParent.RegisterBaseGhost(component2);
			componentInParent.FixRoomFloors();
			componentInParent.FixCorridorLinks();
			componentInParent.RebuildGeometry();
		}
	}

	public void LinkModule(Base.Face? moduleFace)
	{
		this.moduleFace = moduleFace;
	}

	public string CompileTimeCheck()
	{
		if (hintDirection == Base.Direction.Count)
		{
			return $"hintDirection is not assigned";
		}
		if (base.gameObject.layer != LayerID.Useable)
		{
			return $"Explicitly assigned BaseDeconstructable component is on a gameobject not in Useable layer.";
		}
		return null;
	}
}
