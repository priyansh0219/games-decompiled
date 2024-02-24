using System;
using System.Collections;
using System.Collections.Generic;
using Gendarme;
using ProtoBuf;
using UWE;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

[ProtoContract]
public class Base : MonoBehaviour, IProtoEventListener
{
	public delegate void BaseEventHandler(Base b);

	public delegate void BaseFaceEventHandler(Base b, Face face);

	public delegate void BaseResizeEventHandler(Base b, Int3 offset);

	public enum CellType : byte
	{
		Empty = 0,
		Room = 1,
		Foundation = 2,
		OccupiedByOtherCell = 3,
		Corridor = 4,
		Observatory = 5,
		Connector = 6,
		Moonpool = 7,
		MapRoom = 8,
		MapRoomRotated = 9,
		RechargePlatform = 10,
		ControlRoom = 11,
		ControlRoomRotated = 12,
		WallFoundationN = 13,
		WallFoundationW = 14,
		WallFoundationS = 15,
		WallFoundationE = 16,
		LargeRoom = 17,
		LargeRoomRotated = 18,
		MoonpoolRotated = 19,
		Count = 20
	}

	public class CellTypeComparer : IEqualityComparer<CellType>
	{
		public bool Equals(CellType x, CellType y)
		{
			int num = (int)x;
			return num.Equals((int)y);
		}

		public int GetHashCode(CellType obj)
		{
			return (int)obj;
		}
	}

	public enum FaceType : byte
	{
		None = 0,
		Solid = 1,
		Window = 2,
		Hatch = 3,
		ObsoleteDoor = 4,
		Ladder = 5,
		Reinforcement = 6,
		BulkheadClosed = 7,
		BulkheadOpened = 8,
		Hole = 9,
		UpgradeConsole = 10,
		Planter = 11,
		FiltrationMachine = 12,
		WaterPark = 13,
		BioReactor = 14,
		NuclearReactor = 15,
		Partition = 16,
		PartitionDoor = 17,
		GlassDome = 18,
		LargeGlassDome = 19,
		ControlRoomModule = 20,
		Count = 21,
		OccupiedByOtherFace = 128,
		OccupiedByNorthFace = 128,
		OccupiedByEastFace = 130,
		OccupiedBySouthFace = 129,
		OccupiedByWestFace = 131,
		OccupiedByAboveFace = 132,
		OccupiedByBelowFace = 133
	}

	public class FaceTypeComparer : IEqualityComparer<FaceType>
	{
		public bool Equals(FaceType x, FaceType y)
		{
			int num = (int)x;
			return num.Equals((int)y);
		}

		public int GetHashCode(FaceType obj)
		{
			return (int)obj;
		}
	}

	private struct PieceDef
	{
		public Transform prefab;

		public Int3 extraCells;

		public Vector3 offset;

		public Quaternion rotation;

		public PieceDef(GameObject prefab, Int3 extraCells, Quaternion rotation)
		{
			this.prefab = prefab.transform;
			this.extraCells = extraCells;
			this.rotation = rotation;
			offset = Int3.Scale(extraCells, halfCellSize);
		}
	}

	private struct CorridorFace
	{
		public Piece piece;

		public Quaternion rotation;

		public CorridorFace(Piece piece, Vector3 angles)
		{
			this.piece = piece;
			rotation = Quaternion.Euler(angles);
		}
	}

	private struct CorridorDef
	{
		public Piece piece;

		public Piece supportPiece;

		public Piece adjustableSupportPiece;

		public TechType techType;

		public Quaternion rotation;

		public CorridorFace[,] faces;

		public Direction[] worldToLocal;

		public CorridorDef(Piece piece, Piece supportPiece, Piece adjustableSupportPiece, TechType techType)
		{
			this.piece = piece;
			this.supportPiece = supportPiece;
			this.adjustableSupportPiece = adjustableSupportPiece;
			this.techType = techType;
			rotation = Quaternion.identity;
			faces = new CorridorFace[6, 21];
			worldToLocal = AllDirections;
		}

		public void SetFace(Direction side, FaceType faceType, Piece piece, Vector3 angles)
		{
			faces[(int)side, (uint)faceType] = new CorridorFace(piece, angles);
		}

		public CorridorDef GetRotated(float yRotation)
		{
			CorridorDef result = this;
			result.rotation *= Quaternion.Euler(0f, yRotation, 0f);
			Quaternion inverse = result.rotation.GetInverse();
			result.worldToLocal = new Direction[6];
			Direction[] allDirections = AllDirections;
			foreach (Direction direction in allDirections)
			{
				Vector3 normal = inverse * DirectionOffset[(int)direction].ToVector3();
				result.worldToLocal[(int)direction] = NormalToDirection(normal);
			}
			return result;
		}

		public TechType GetTechType()
		{
			switch (piece)
			{
			case Piece.CorridorIShapeGlass:
				return TechType.BaseCorridorGlassI;
			case Piece.CorridorLShapeGlass:
				return TechType.BaseCorridorGlassL;
			case Piece.CorridorIShape:
				return TechType.BaseCorridorI;
			case Piece.CorridorLShape:
				return TechType.BaseCorridorL;
			case Piece.CorridorTShape:
				return TechType.BaseCorridorT;
			case Piece.CorridorXShape:
				return TechType.BaseCorridorX;
			default:
				return TechType.BaseCorridor;
			}
		}
	}

	private struct RoomFace
	{
		public Int3 offset;

		public Direction direction;

		public Quaternion rotation;

		public Vector3 localOffset;

		public RoomFace(int x, int z, Direction direction, float yAngle, Vector3 localOffset = default(Vector3))
		{
			offset = new Int3(x, 0, z);
			this.direction = direction;
			rotation = Quaternion.Euler(0f, yAngle, 0f);
			this.localOffset = localOffset;
		}
	}

	private struct Partition
	{
		public Piece piece;

		public Quaternion pieceRotation;

		public Vector3 doorOffset;

		public Quaternion doorRotation;

		public Partition(Piece piece, Quaternion pieceRotation, Vector3 doorOffset, Quaternion doorRotation)
		{
			this.piece = piece;
			this.pieceRotation = pieceRotation;
			this.doorOffset = doorOffset;
			this.doorRotation = doorRotation;
		}
	}

	public enum Piece
	{
		Invalid = 0,
		Foundation = 1,
		CorridorCap = 2,
		CorridorWindow = 3,
		CorridorHatch = 4,
		CorridorBulkhead = 5,
		CorridorIShapeGlass = 6,
		CorridorLShapeGlass = 7,
		CorridorIShape = 8,
		CorridorLShape = 9,
		CorridorTShape = 10,
		CorridorXShape = 11,
		CorridorIShapeGlassSupport = 12,
		CorridorLShapeGlassSupport = 13,
		CorridorIShapeSupport = 14,
		CorridorLShapeSupport = 15,
		CorridorTShapeSupport = 16,
		CorridorIShapeGlassAdjustableSupport = 17,
		CorridorLShapeGlassAdjustableSupport = 18,
		CorridorIShapeAdjustableSupport = 19,
		CorridorLShapeAdjustableSupport = 20,
		CorridorTShapeAdjustableSupport = 21,
		CorridorXShapeAdjustableSupport = 22,
		CorridorIShapeCoverSide = 23,
		CorridorIShapeWindowSide = 24,
		CorridorIShapeWindowTop = 25,
		CorridorIShapeWindowBottom = 26,
		CorridorIShapeReinforcementSide = 27,
		CorridorIShapeHatchSide = 28,
		CorridorIShapeHatchTop = 29,
		CorridorIShapeHatchBottom = 30,
		CorridorIShapeLadderTop = 31,
		CorridorIShapeLadderBottom = 32,
		CorridorIShapePlanterSide = 33,
		CorridorTShapeWindowTop = 34,
		CorridorTShapeWindowBottom = 35,
		CorridorTShapeHatchTop = 36,
		CorridorTShapeHatchBottom = 37,
		CorridorTShapeLadderTop = 38,
		CorridorTShapeLadderBottom = 39,
		CorridorXShapeWindowTop = 40,
		CorridorXShapeWindowBottom = 41,
		CorridorXShapeHatchTop = 42,
		CorridorXShapeHatchBottom = 43,
		CorridorXShapeLadderTop = 44,
		CorridorXShapeLadderBottom = 45,
		CorridorCoverIShapeBottomExtClosed = 46,
		CorridorCoverIShapeBottomExtOpened = 47,
		CorridorCoverIShapeBottomIntClosed = 48,
		CorridorCoverIShapeBottomIntOpened = 49,
		CorridorCoverIShapeTopExtClosed = 50,
		CorridorCoverIShapeTopExtOpened = 51,
		CorridorCoverIShapeTopIntClosed = 52,
		CorridorCoverIShapeTopIntOpened = 53,
		CorridorCoverTShapeBottomExtClosed = 54,
		CorridorCoverTShapeBottomExtOpened = 55,
		CorridorCoverTShapeBottomIntClosed = 56,
		CorridorCoverTShapeBottomIntOpened = 57,
		CorridorCoverTShapeTopExtClosed = 58,
		CorridorCoverTShapeTopExtOpened = 59,
		CorridorCoverTShapeTopIntClosed = 60,
		CorridorCoverTShapeTopIntOpened = 61,
		CorridorCoverXShapeBottomExtClosed = 62,
		CorridorCoverXShapeBottomExtOpened = 63,
		CorridorCoverXShapeBottomIntClosed = 64,
		CorridorCoverXShapeBottomIntOpened = 65,
		CorridorCoverXShapeTopExtClosed = 66,
		CorridorCoverXShapeTopExtOpened = 67,
		CorridorCoverXShapeTopIntClosed = 68,
		CorridorCoverXShapeTopIntOpened = 69,
		ConnectorTube = 70,
		ConnectorTubeWindow = 71,
		ConnectorCap = 72,
		ConnectorLadder = 73,
		Room = 74,
		RoomCorridorConnector = 75,
		RoomCoverSide = 76,
		RoomCoverSideVariant = 77,
		RoomExteriorBottom = 78,
		RoomExteriorFoundationBottom = 79,
		RoomExteriorTop = 80,
		RoomExteriorTopGlass = 81,
		RoomReinforcementSide = 82,
		RoomWindowSide = 83,
		RoomCoverBottom = 84,
		RoomCoverTop = 85,
		RoomLadderBottom = 86,
		RoomLadderTop = 87,
		RoomAdjustableSupport = 88,
		RoomHatch = 89,
		RoomPlanterSide = 90,
		RoomFiltrationMachine = 91,
		RoomInteriorBottom = 92,
		RoomInteriorTop = 93,
		RoomInteriorTopGlass = 94,
		RoomInteriorBottomHole = 95,
		RoomInteriorTopHole = 96,
		RoomBioReactor = 97,
		RoomBioReactorUnderDome = 98,
		RoomNuclearReactor = 99,
		RoomNuclearReactorUnderDome = 100,
		RoomWaterParkTop = 101,
		RoomWaterParkBottom = 102,
		RoomWaterParkHatch = 103,
		RoomWaterParkSide = 104,
		RoomWaterParkCeilingGlass = 105,
		RoomWaterParkCeilingGlassDome = 106,
		RoomWaterParkCeilingMiddle = 107,
		RoomWaterParkCeilingTop = 108,
		RoomWaterParkFloorBottom = 109,
		RoomWaterParkFloorMiddle = 110,
		Observatory = 111,
		ObservatoryCoverSide = 112,
		ObservatoryCorridorConnector = 113,
		ObservatoryHatch = 114,
		Moonpool = 115,
		MoonpoolCoverSide = 116,
		MoonpoolCoverSideShort = 117,
		MoonpoolReinforcementSide = 118,
		MoonpoolReinforcementSideShort = 119,
		MoonpoolWindowSide = 120,
		MoonpoolWindowSideShort = 121,
		MoonpoolUpgradeConsole = 122,
		MoonpoolUpgradeConsoleShort = 123,
		MoonpoolAdjustableSupport = 124,
		MoonpoolHatch = 125,
		MoonpoolHatchShort = 126,
		MoonpoolCorridorConnector = 127,
		MoonpoolCorridorConnectorShort = 128,
		MoonpoolPlanterSide = 129,
		MoonpoolPlanterSideShort = 130,
		MapRoom = 131,
		MapRoomCoverSide = 132,
		MapRoomCorridorConnector = 133,
		MapRoomHatch = 134,
		MapRoomWindowSide = 135,
		MapRoomPlanterSide = 136,
		MapRoomReinforcementSide = 137,
		ControlRoom = 138,
		ControlRoomCoverSide = 139,
		ControlRoomCorridorConnector = 140,
		ControlRoomHatch = 141,
		ControlRoomWindowSide = 142,
		ControlRoomPlanterSide = 143,
		ControlRoomReinforcementSide = 144,
		ControlRoomModuleGeometry = 145,
		RechargePlatform = 146,
		WallFoundationN = 147,
		WallFoundationW = 148,
		WallFoundationS = 149,
		WallFoundationE = 150,
		LargeRoom = 151,
		LargeRoomExteriorTop = 152,
		LargeRoomExteriorTopGlass = 153,
		LargeRoomInteriorTop = 154,
		LargeRoomInteriorTopGlass = 155,
		LargeRoomInteriorTopHole1 = 156,
		LargeRoomInteriorTopHole2 = 157,
		LargeRoomInteriorTopHole3 = 158,
		LargeRoomInteriorTopHole4 = 159,
		LargeRoomInteriorBottom = 160,
		LargeRoomInteriorBottomHole1 = 161,
		LargeRoomInteriorBottomHole2 = 162,
		LargeRoomInteriorBottomHole3 = 163,
		LargeRoomInteriorBottomHole4 = 164,
		LargeRoomExteriorBottom = 165,
		LargeRoomExteriorFoundationBottom = 166,
		LargeRoomAdjustableSupport = 167,
		LargeRoomCoverSide = 168,
		LargeRoomCoverSideShort = 169,
		LargeRoomReinforcementSide = 170,
		LargeRoomReinforcementSideShort = 171,
		LargeRoomWindowSide = 172,
		LargeRoomWindowSideShort = 173,
		LargeRoomHatch = 174,
		LargeRoomHatchShort = 175,
		LargeRoomCorridorConnector = 176,
		LargeRoomCorridorConnectorShort = 177,
		LargeRoomPlanterSide = 178,
		LargeRoomPlanterSideShort = 179,
		LargeRoomFiltrationMachine = 180,
		LargeRoomFiltrationMachineShort = 181,
		LargeRoomLadderBottom = 182,
		LargeRoomLadderTop = 183,
		LargeRoomCoverTop = 184,
		LargeRoomCoverBottom = 185,
		LargeRoomWaterParkWalls = 186,
		LargeRoomWaterParkCeilingGlass = 187,
		LargeRoomWaterParkCeilingGlassDome = 188,
		LargeRoomWaterParkCeilingMiddle = 189,
		LargeRoomWaterParkCeilingTop = 190,
		LargeRoomWaterParkCeilingTopMiddle = 191,
		LargeRoomWaterParkCeilingMiddleTop = 192,
		LargeRoomWaterParkCeilingMiddleMiddle = 193,
		LargeRoomWaterParkFloorBottom = 194,
		LargeRoomWaterParkFloorMiddle = 195,
		LargeRoomWaterParkFloorBottomMiddle = 196,
		LargeRoomWaterParkFloorMiddleBottom = 197,
		LargeRoomWaterParkFloorMiddleMiddle = 198,
		LargeRoomWaterParkHatch = 199,
		LargeRoomWaterParkHatchShort = 200,
		LargeRoomWaterParkSide = 201,
		Partition = 202,
		PartitionDoor = 203,
		PartitionCentralIHalf = 204,
		PartitionCentralIHalf90 = 205,
		PartitionCentralIHalf180 = 206,
		PartitionCentralIHalf270 = 207,
		PartitionCentralI = 208,
		PartitionCentralI90 = 209,
		PartitionDoorwayCentralI = 210,
		PartitionDoorwayCentralI90 = 211,
		PartitionCentralL = 212,
		PartitionCentralL90 = 213,
		PartitionCentralL180 = 214,
		PartitionCentralL270 = 215,
		PartitionCentralT = 216,
		PartitionCentralT90 = 217,
		PartitionCentralT180 = 218,
		PartitionCentralT270 = 219,
		PartitionCentralX = 220,
		PartitionSideIHalf = 221,
		PartitionSideIHalf90 = 222,
		PartitionSideIHalf180 = 223,
		PartitionSideIHalf270 = 224,
		PartitionSideI = 225,
		PartitionSideI90 = 226,
		PartitionDoorwaySideI = 227,
		PartitionDoorwaySideI90 = 228,
		PartitionSideL = 229,
		PartitionSideL90 = 230,
		PartitionSideL180 = 231,
		PartitionSideL270 = 232,
		PartitionSideT = 233,
		PartitionSideT90 = 234,
		PartitionSideT180 = 235,
		PartitionSideT270 = 236,
		PartitionSideX = 237,
		PartitionSideShortIHalf = 238,
		PartitionSideShortIHalf90 = 239,
		PartitionSideShortIHalf180 = 240,
		PartitionSideShortIHalf270 = 241,
		PartitionSideShortI = 242,
		PartitionSideShortI90 = 243,
		PartitionDoorwaySideShortI = 244,
		PartitionDoorwaySideShortI90 = 245,
		PartitionSideShortL = 246,
		PartitionSideShortL90 = 247,
		PartitionSideShortL180 = 248,
		PartitionSideShortL270 = 249,
		PartitionSideShortT = 250,
		PartitionSideShortT90 = 251,
		PartitionSideShortT180 = 252,
		PartitionSideShortT270 = 253,
		PartitionSideShortX = 254,
		PartitionCornerIHalf = 255,
		PartitionCornerIHalf90 = 256,
		PartitionCornerIHalf180 = 257,
		PartitionCornerIHalf270 = 258,
		PartitionCornerI = 259,
		PartitionCornerI90 = 260,
		PartitionDoorwayCornerI = 261,
		PartitionDoorwayCornerI90 = 262,
		PartitionCornerL = 263,
		PartitionCornerL90 = 264,
		PartitionCornerL180 = 265,
		PartitionCornerL270 = 266,
		PartitionCornerT = 267,
		PartitionCornerT90 = 268,
		PartitionCornerT180 = 269,
		PartitionCornerT270 = 270,
		PartitionCornerX = 271,
		Count = 272
	}

	public enum Direction
	{
		North = 0,
		South = 1,
		East = 2,
		West = 3,
		Above = 4,
		Below = 5,
		Count = 6
	}

	[ProtoContract]
	public struct Face : IEquatable<Face>
	{
		[ProtoMember(1)]
		public Int3 cell;

		[ProtoMember(2)]
		public Direction direction;

		public Face(Int3 cell, Direction direction)
		{
			this.cell = cell;
			this.direction = direction;
		}

		public override int GetHashCode()
		{
			int num = 923;
			num = 31 * num + cell.GetHashCode();
			return 31 * num + direction.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj is Face)
			{
				return Equals((Face)obj);
			}
			return false;
		}

		public bool Equals(Face other)
		{
			if (cell == other.cell)
			{
				return direction == other.direction;
			}
			return false;
		}

		public static bool operator ==(Face lhs, Face rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(Face lhs, Face rhs)
		{
			return !lhs.Equals(rhs);
		}

		public override string ToString()
		{
			return $"Face ({cell}) {direction}";
		}
	}

	private struct FaceDef
	{
		public Face face;

		public FaceType faceType;

		public FaceDef(int x, int y, int z, Direction direction, FaceType faceType)
		{
			face = new Face(new Int3(x, y, z), direction);
			this.faceType = faceType;
		}
	}

	private struct PieceData
	{
		public Piece piece;

		public string name;

		public Int3 extraCells;

		public AsyncOperationHandle<GameObject> request;

		public PieceData(Piece _piece, string _name)
		{
			piece = _piece;
			name = _name;
			extraCells = Int3.zero;
			request = default(AsyncOperationHandle<GameObject>);
		}

		public PieceData(Piece _piece, string _name, Int3 _extraCells)
		{
			piece = _piece;
			name = _name;
			extraCells = _extraCells;
			request = default(AsyncOperationHandle<GameObject>);
		}
	}

	private struct RebuildAccessoryGeometry
	{
		public GameObject obj;

		public IBaseAccessoryGeometry accessory;

		public RebuildAccessoryGeometry(GameObject gameObject, IBaseAccessoryGeometry accessory)
		{
			obj = gameObject;
			this.accessory = accessory;
		}
	}

	private struct CachedPiece
	{
		public GameObject obj;

		public Int3 cell;
	}

	public const string kMainPieceGeometry = "MainPieceGeometry";

	private readonly List<BaseGhost> ghosts = new List<BaseGhost>();

	private BaseGhost placementGhost;

	private float timeLastPowerSample;

	private const float powerSampleRate = 2f;

	private BasePowerRelay powerRelay;

	private float _consumptionRate;

	private float _chargeRate;

	public bool isPlaced;

	private bool waitingForWorld;

	private float nextWorldPollTime;

	private static bool initialized;

	private static Transform cellPrefab;

	private static PieceDef[] pieces;

	private static CorridorDef[] corridors;

	private static CorridorDef[] glassCorridors;

	private static GameObject mapRoomFunctionalityPrefab;

	private static AsyncOperationHandle<GameObject> mapRoomFunctionalityPrefabRequest;

	private static AsyncOperationHandle<GameObject> cellLoadRequest;

	private PowerSystem.Status powerStatus;

	public static readonly Vector3 cellSize = new Vector3(5f, 3.5f, 5f);

	public static readonly Vector3 halfCellSize = cellSize * 0.5f;

	public static readonly CellTypeComparer sCellTypeComparer = new CellTypeComparer();

	public static readonly Int3[] CellSize = new Int3[20]
	{
		Int3.zero,
		new Int3(3, 1, 3),
		new Int3(2, 1, 2),
		Int3.zero,
		new Int3(1),
		new Int3(1),
		new Int3(1),
		new Int3(4, 1, 3),
		new Int3(3, 1, 3),
		new Int3(3, 1, 3),
		new Int3(2, 1, 2),
		new Int3(3, 1, 3),
		new Int3(3, 1, 3),
		new Int3(2, 1, 1),
		new Int3(1, 1, 2),
		new Int3(2, 1, 1),
		new Int3(1, 1, 2),
		new Int3(6, 1, 3),
		new Int3(3, 1, 6),
		new Int3(3, 1, 4)
	};

	public static readonly float[] CellPowerConsumption = new float[20]
	{
		0f,
		5f / 6f,
		0f,
		0f,
		1f / 12f,
		0f,
		0f,
		1.6666666f,
		5f / 12f,
		5f / 12f,
		0f,
		5f / 12f,
		5f / 12f,
		0f,
		0f,
		0f,
		0f,
		1.6666666f,
		1.6666666f,
		1.6666666f
	};

	public static readonly HashSet<CellType> sFoundationCheckTypes = new HashSet<CellType>
	{
		CellType.Foundation,
		CellType.WallFoundationN,
		CellType.WallFoundationW,
		CellType.WallFoundationS,
		CellType.WallFoundationE,
		CellType.RechargePlatform
	};

	public static readonly FaceTypeComparer sFaceTypeComparer = new FaceTypeComparer();

	public static readonly TechType[] FaceToRecipe = new TechType[21]
	{
		TechType.None,
		TechType.BaseWall,
		TechType.BaseWindow,
		TechType.BaseHatch,
		TechType.BaseDoor,
		TechType.BaseLadder,
		TechType.BaseReinforcement,
		TechType.BaseBulkhead,
		TechType.BaseBulkhead,
		TechType.None,
		TechType.BaseUpgradeConsole,
		TechType.BasePlanter,
		TechType.BaseFiltrationMachine,
		TechType.BaseWaterPark,
		TechType.BaseBioReactor,
		TechType.BaseNuclearReactor,
		TechType.BasePartition,
		TechType.BasePartitionDoor,
		TechType.BaseGlassDome,
		TechType.BaseLargeGlassDome,
		TechType.None
	};

	private static readonly float[] CellHullStrength = new float[20]
	{
		0f, -1.25f, 2f, 0f, -1f, -3f, -0.5f, -5f, -1f, -1f,
		0f, -1f, -1f, 2f, 2f, 2f, 2f, -4f, -4f, -5f
	};

	private static readonly float[] FaceHullStrength = new float[21]
	{
		0f, 0f, -1f, -1f, 0f, 0f, 7f, 3f, 3f, 0f,
		0f, 0f, -1f, 0f, 0f, 0f, 0.2f, 0f, -2f, -4f,
		0f
	};

	private static readonly int[] rotationCCWRemap = new int[16]
	{
		0, 1, 2, 3, 2, 3, 1, 0, 1, 0,
		3, 2, 3, 2, 0, 1
	};

	private static readonly int[] rotationCWRemap = new int[16]
	{
		0, 1, 2, 3, 3, 2, 0, 1, 1, 0,
		3, 2, 2, 3, 1, 0
	};

	private Dictionary<Piece, Piece> exteriorToInteriorPiece = new Dictionary<Piece, Piece>
	{
		{
			Piece.CorridorCoverIShapeBottomExtClosed,
			Piece.CorridorCoverIShapeBottomIntClosed
		},
		{
			Piece.CorridorCoverIShapeBottomExtOpened,
			Piece.CorridorCoverIShapeBottomIntOpened
		},
		{
			Piece.CorridorCoverIShapeTopExtClosed,
			Piece.CorridorCoverIShapeTopIntClosed
		},
		{
			Piece.CorridorCoverIShapeTopExtOpened,
			Piece.CorridorCoverIShapeTopIntOpened
		},
		{
			Piece.CorridorCoverTShapeBottomExtClosed,
			Piece.CorridorCoverTShapeBottomIntClosed
		},
		{
			Piece.CorridorCoverTShapeBottomExtOpened,
			Piece.CorridorCoverTShapeBottomIntOpened
		},
		{
			Piece.CorridorCoverTShapeTopExtClosed,
			Piece.CorridorCoverTShapeTopIntClosed
		},
		{
			Piece.CorridorCoverTShapeTopExtOpened,
			Piece.CorridorCoverTShapeTopIntOpened
		},
		{
			Piece.CorridorCoverXShapeBottomExtClosed,
			Piece.CorridorCoverXShapeBottomIntClosed
		},
		{
			Piece.CorridorCoverXShapeBottomExtOpened,
			Piece.CorridorCoverXShapeBottomIntOpened
		},
		{
			Piece.CorridorCoverXShapeTopExtClosed,
			Piece.CorridorCoverXShapeTopIntClosed
		},
		{
			Piece.CorridorCoverXShapeTopExtOpened,
			Piece.CorridorCoverXShapeTopIntOpened
		}
	};

	private const float kCoverOffset = -1.577f;

	private const float kCoverOffset2 = -1.6f;

	private static readonly float kRoomDiagonalOffsetX = cellSize.x * 0.29289323f;

	private static readonly float kRoomDiagonalOffsetZ = cellSize.z * 0.29289323f;

	private static readonly RoomFace[] roomFaces = new RoomFace[23]
	{
		new RoomFace(2, 1, Direction.East, 0f),
		new RoomFace(2, 0, Direction.South, 45f, new Vector3(0f - kRoomDiagonalOffsetX, 0f, kRoomDiagonalOffsetZ)),
		new RoomFace(1, 0, Direction.South, 90f),
		new RoomFace(0, 0, Direction.West, 135f, new Vector3(kRoomDiagonalOffsetX, 0f, kRoomDiagonalOffsetZ)),
		new RoomFace(0, 1, Direction.West, 180f),
		new RoomFace(0, 2, Direction.North, 225f, new Vector3(kRoomDiagonalOffsetX, 0f, 0f - kRoomDiagonalOffsetZ)),
		new RoomFace(1, 2, Direction.North, 270f),
		new RoomFace(2, 2, Direction.East, 315f, new Vector3(0f - kRoomDiagonalOffsetX, 0f, 0f - kRoomDiagonalOffsetZ)),
		new RoomFace(1, 0, Direction.Below, 0f, new Vector3(0f, 0f, 1.577f)),
		new RoomFace(0, 1, Direction.Below, 90f, new Vector3(1.577f, 0f, 0f)),
		new RoomFace(1, 1, Direction.Below, 0f),
		new RoomFace(2, 1, Direction.Below, 270f, new Vector3(-1.577f, 0f, 0f)),
		new RoomFace(1, 2, Direction.Below, 180f, new Vector3(0f, 0f, -1.577f)),
		new RoomFace(0, 0, Direction.Above, 0f, Int3.Scale(CellSize[1] - Int3.one, halfCellSize)),
		new RoomFace(1, 0, Direction.Above, 0f, new Vector3(0f, 0f, 1.577f)),
		new RoomFace(0, 1, Direction.Above, 90f, new Vector3(1.577f, 0f, 0f)),
		new RoomFace(1, 1, Direction.Above, 0f),
		new RoomFace(2, 1, Direction.Above, 270f, new Vector3(-1.577f, 0f, 0f)),
		new RoomFace(1, 2, Direction.Above, 180f, new Vector3(0f, 0f, -1.577f)),
		new RoomFace(1, 1, Direction.East, 0f),
		new RoomFace(1, 1, Direction.South, 90f),
		new RoomFace(1, 1, Direction.West, 180f),
		new RoomFace(1, 1, Direction.North, 270f)
	};

	private static readonly RoomFace[] moonpoolFaces = new RoomFace[6]
	{
		new RoomFace(1, 0, Direction.South, 90f),
		new RoomFace(2, 0, Direction.South, 90f),
		new RoomFace(1, 2, Direction.North, 270f),
		new RoomFace(2, 2, Direction.North, 270f),
		new RoomFace(3, 1, Direction.East, 0f),
		new RoomFace(0, 1, Direction.West, 180f)
	};

	private static readonly RoomFace[] moonpoolRotatedFaces = new RoomFace[6]
	{
		new RoomFace(0, 1, Direction.West, 180f),
		new RoomFace(0, 2, Direction.West, 180f),
		new RoomFace(2, 1, Direction.East, 0f),
		new RoomFace(2, 2, Direction.East, 0f),
		new RoomFace(1, 3, Direction.North, 270f),
		new RoomFace(1, 0, Direction.South, 90f)
	};

	private static readonly RoomFace[] largeRoomFaces = new RoomFace[55]
	{
		new RoomFace(1, 0, Direction.South, 90f),
		new RoomFace(2, 0, Direction.South, 90f),
		new RoomFace(3, 0, Direction.South, 90f),
		new RoomFace(4, 0, Direction.South, 90f),
		new RoomFace(0, 1, Direction.West, 180f),
		new RoomFace(5, 1, Direction.East, 0f),
		new RoomFace(1, 2, Direction.North, 270f),
		new RoomFace(2, 2, Direction.North, 270f),
		new RoomFace(3, 2, Direction.North, 270f),
		new RoomFace(4, 2, Direction.North, 270f),
		new RoomFace(1, 0, Direction.Below, 0f, new Vector3(0f, 0f, 1.6f)),
		new RoomFace(2, 0, Direction.Below, 0f, new Vector3(0f, 0f, 1.6f)),
		new RoomFace(3, 0, Direction.Below, 0f, new Vector3(0f, 0f, 1.6f)),
		new RoomFace(4, 0, Direction.Below, 0f, new Vector3(0f, 0f, 1.6f)),
		new RoomFace(0, 1, Direction.Below, 90f, new Vector3(1.6f, 0f, 0f)),
		new RoomFace(1, 1, Direction.Below, 0f),
		new RoomFace(2, 1, Direction.Below, 0f),
		new RoomFace(3, 1, Direction.Below, 0f),
		new RoomFace(4, 1, Direction.Below, 0f),
		new RoomFace(5, 1, Direction.Below, 270f, new Vector3(-1.6f, 0f, 0f)),
		new RoomFace(1, 2, Direction.Below, 180f, new Vector3(0f, 0f, -1.6f)),
		new RoomFace(2, 2, Direction.Below, 180f, new Vector3(0f, 0f, -1.6f)),
		new RoomFace(3, 2, Direction.Below, 180f, new Vector3(0f, 0f, -1.6f)),
		new RoomFace(4, 2, Direction.Below, 180f, new Vector3(0f, 0f, -1.6f)),
		new RoomFace(0, 0, Direction.Above, 0f, Int3.Scale(CellSize[17] - Int3.one, halfCellSize)),
		new RoomFace(1, 0, Direction.Above, 0f, new Vector3(0f, 0f, 1.6f)),
		new RoomFace(2, 0, Direction.Above, 0f, new Vector3(0f, 0f, 1.6f)),
		new RoomFace(3, 0, Direction.Above, 0f, new Vector3(0f, 0f, 1.6f)),
		new RoomFace(4, 0, Direction.Above, 0f, new Vector3(0f, 0f, 1.6f)),
		new RoomFace(0, 1, Direction.Above, 90f, new Vector3(1.6f, 0f, 0f)),
		new RoomFace(1, 1, Direction.Above, 0f),
		new RoomFace(2, 1, Direction.Above, 0f),
		new RoomFace(3, 1, Direction.Above, 0f),
		new RoomFace(4, 1, Direction.Above, 0f),
		new RoomFace(5, 1, Direction.Above, 270f, new Vector3(-1.6f, 0f, 0f)),
		new RoomFace(1, 2, Direction.Above, 180f, new Vector3(0f, 0f, -1.6f)),
		new RoomFace(2, 2, Direction.Above, 180f, new Vector3(0f, 0f, -1.6f)),
		new RoomFace(3, 2, Direction.Above, 180f, new Vector3(0f, 0f, -1.6f)),
		new RoomFace(4, 2, Direction.Above, 180f, new Vector3(0f, 0f, -1.6f)),
		new RoomFace(1, 1, Direction.East, 0f),
		new RoomFace(1, 1, Direction.South, 90f),
		new RoomFace(1, 1, Direction.West, 180f),
		new RoomFace(1, 1, Direction.North, 270f),
		new RoomFace(2, 1, Direction.East, 0f),
		new RoomFace(2, 1, Direction.South, 90f),
		new RoomFace(2, 1, Direction.West, 180f),
		new RoomFace(2, 1, Direction.North, 270f),
		new RoomFace(3, 1, Direction.East, 0f),
		new RoomFace(3, 1, Direction.South, 90f),
		new RoomFace(3, 1, Direction.West, 180f),
		new RoomFace(3, 1, Direction.North, 270f),
		new RoomFace(4, 1, Direction.East, 0f),
		new RoomFace(4, 1, Direction.South, 90f),
		new RoomFace(4, 1, Direction.West, 180f),
		new RoomFace(4, 1, Direction.North, 270f)
	};

	private static readonly RoomFace[] largeRoomRotatedFaces = new RoomFace[55]
	{
		new RoomFace(1, 0, Direction.South, 90f),
		new RoomFace(0, 1, Direction.West, 180f),
		new RoomFace(0, 2, Direction.West, 180f),
		new RoomFace(0, 3, Direction.West, 180f),
		new RoomFace(0, 4, Direction.West, 180f),
		new RoomFace(2, 1, Direction.East, 0f),
		new RoomFace(2, 2, Direction.East, 0f),
		new RoomFace(2, 3, Direction.East, 0f),
		new RoomFace(2, 4, Direction.East, 0f),
		new RoomFace(1, 5, Direction.North, 270f),
		new RoomFace(1, 0, Direction.Below, 0f, new Vector3(0f, 0f, 1.6f)),
		new RoomFace(0, 1, Direction.Below, 90f, new Vector3(1.6f, 0f, 0f)),
		new RoomFace(0, 2, Direction.Below, 90f, new Vector3(1.6f, 0f, 0f)),
		new RoomFace(0, 3, Direction.Below, 90f, new Vector3(1.6f, 0f, 0f)),
		new RoomFace(0, 4, Direction.Below, 90f, new Vector3(1.6f, 0f, 0f)),
		new RoomFace(1, 1, Direction.Below, 270f),
		new RoomFace(1, 2, Direction.Below, 270f),
		new RoomFace(1, 3, Direction.Below, 270f),
		new RoomFace(1, 4, Direction.Below, 270f),
		new RoomFace(2, 1, Direction.Below, 270f, new Vector3(-1.6f, 0f, 0f)),
		new RoomFace(2, 2, Direction.Below, 270f, new Vector3(-1.6f, 0f, 0f)),
		new RoomFace(2, 3, Direction.Below, 270f, new Vector3(-1.6f, 0f, 0f)),
		new RoomFace(2, 4, Direction.Below, 270f, new Vector3(-1.6f, 0f, 0f)),
		new RoomFace(1, 5, Direction.Below, 180f, new Vector3(0f, 0f, -1.6f)),
		new RoomFace(0, 0, Direction.Above, 270f, Int3.Scale(CellSize[18] - Int3.one, halfCellSize)),
		new RoomFace(1, 0, Direction.Above, 0f, new Vector3(0f, 0f, 1.6f)),
		new RoomFace(0, 1, Direction.Above, 90f, new Vector3(1.6f, 0f, 0f)),
		new RoomFace(0, 2, Direction.Above, 90f, new Vector3(1.6f, 0f, 0f)),
		new RoomFace(0, 3, Direction.Above, 90f, new Vector3(1.6f, 0f, 0f)),
		new RoomFace(0, 4, Direction.Above, 90f, new Vector3(1.6f, 0f, 0f)),
		new RoomFace(1, 1, Direction.Above, 0f),
		new RoomFace(1, 2, Direction.Above, 0f),
		new RoomFace(1, 3, Direction.Above, 0f),
		new RoomFace(1, 4, Direction.Above, 0f),
		new RoomFace(2, 1, Direction.Above, 270f, new Vector3(-1.6f, 0f, 0f)),
		new RoomFace(2, 2, Direction.Above, 270f, new Vector3(-1.6f, 0f, 0f)),
		new RoomFace(2, 3, Direction.Above, 270f, new Vector3(-1.6f, 0f, 0f)),
		new RoomFace(2, 4, Direction.Above, 270f, new Vector3(-1.6f, 0f, 0f)),
		new RoomFace(1, 5, Direction.Above, 180f, new Vector3(0f, 0f, -1.6f)),
		new RoomFace(1, 1, Direction.East, 0f),
		new RoomFace(1, 1, Direction.South, 90f),
		new RoomFace(1, 1, Direction.West, 180f),
		new RoomFace(1, 1, Direction.North, 270f),
		new RoomFace(1, 2, Direction.East, 0f),
		new RoomFace(1, 2, Direction.South, 90f),
		new RoomFace(1, 2, Direction.West, 180f),
		new RoomFace(1, 2, Direction.North, 270f),
		new RoomFace(1, 3, Direction.East, 0f),
		new RoomFace(1, 3, Direction.South, 90f),
		new RoomFace(1, 3, Direction.West, 180f),
		new RoomFace(1, 3, Direction.North, 270f),
		new RoomFace(1, 4, Direction.East, 0f),
		new RoomFace(1, 4, Direction.South, 90f),
		new RoomFace(1, 4, Direction.West, 180f),
		new RoomFace(1, 4, Direction.North, 270f)
	};

	private static readonly FaceType[] constructFaceTypes = new FaceType[21]
	{
		FaceType.None,
		FaceType.Solid,
		FaceType.Solid,
		FaceType.Solid,
		FaceType.Solid,
		FaceType.Solid,
		FaceType.Solid,
		FaceType.None,
		FaceType.None,
		FaceType.Solid,
		FaceType.Solid,
		FaceType.Solid,
		FaceType.Solid,
		FaceType.Solid,
		FaceType.None,
		FaceType.None,
		FaceType.None,
		FaceType.None,
		FaceType.Solid,
		FaceType.Solid,
		FaceType.None
	};

	private static readonly FaceType[] deconstructFaceTypes = new FaceType[21]
	{
		FaceType.None,
		FaceType.Solid,
		FaceType.Solid,
		FaceType.Solid,
		FaceType.Solid,
		FaceType.Solid,
		FaceType.Solid,
		FaceType.None,
		FaceType.None,
		FaceType.Solid,
		FaceType.Solid,
		FaceType.Solid,
		FaceType.Solid,
		FaceType.Solid,
		FaceType.None,
		FaceType.None,
		FaceType.None,
		FaceType.None,
		FaceType.Solid,
		FaceType.Solid,
		FaceType.None
	};

	private static readonly Piece[] observatoryFacePieces = new Piece[21]
	{
		Piece.ObservatoryCorridorConnector,
		Piece.ObservatoryCoverSide,
		Piece.Invalid,
		Piece.ObservatoryHatch,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid
	};

	private static readonly Piece[] mapRoomFacePieces = new Piece[21]
	{
		Piece.MapRoomCorridorConnector,
		Piece.MapRoomCoverSide,
		Piece.MapRoomWindowSide,
		Piece.MapRoomHatch,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid
	};

	private static readonly Piece[] controlRoomFacePieces = new Piece[21]
	{
		Piece.ControlRoomCorridorConnector,
		Piece.ControlRoomCoverSide,
		Piece.ControlRoomWindowSide,
		Piece.ControlRoomHatch,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.Invalid,
		Piece.ControlRoomModuleGeometry
	};

	private static readonly bool[,] roomLadderPlaces = new bool[3, 3]
	{
		{ false, true, false },
		{ true, true, true },
		{ false, true, false }
	};

	private static readonly bool[,] largeRoomLadderPlaces = new bool[6, 3]
	{
		{ false, true, false },
		{ true, true, true },
		{ true, true, true },
		{ true, true, true },
		{ true, true, true },
		{ false, true, false }
	};

	private static readonly bool[,] largeRoomRotatedLadderPlaces = new bool[3, 6]
	{
		{ false, true, true, true, true, false },
		{ true, true, true, true, true, true },
		{ false, true, true, true, true, false }
	};

	private static readonly Vector3[,] roomLadderExits = new Vector3[3, 3]
	{
		{
			Vector3.zero,
			new Vector3(2f, 0.3f, 5f),
			Vector3.zero
		},
		{
			new Vector3(5f, 0.3f, 2f),
			new Vector3(5f, 0.3f, 5.3f),
			new Vector3(5f, 0.3f, 8f)
		},
		{
			Vector3.zero,
			new Vector3(8f, 0.3f, 5f),
			Vector3.zero
		}
	};

	private static readonly Vector3 corridorLadderExit = new Vector3(0f, 0.7f, 0.3f);

	private static readonly Piece[,] roomFacePieces = new Piece[6, 21]
	{
		{
			Piece.RoomCorridorConnector,
			Piece.RoomCoverSide,
			Piece.RoomWindowSide,
			Piece.RoomHatch,
			Piece.Invalid,
			Piece.Invalid,
			Piece.RoomReinforcementSide,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.RoomPlanterSide,
			Piece.RoomFiltrationMachine,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.RoomCorridorConnector,
			Piece.RoomCoverSide,
			Piece.RoomWindowSide,
			Piece.RoomHatch,
			Piece.Invalid,
			Piece.Invalid,
			Piece.RoomReinforcementSide,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.RoomPlanterSide,
			Piece.RoomFiltrationMachine,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.RoomCorridorConnector,
			Piece.RoomCoverSide,
			Piece.RoomWindowSide,
			Piece.RoomHatch,
			Piece.Invalid,
			Piece.Invalid,
			Piece.RoomReinforcementSide,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.RoomPlanterSide,
			Piece.RoomFiltrationMachine,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.RoomCorridorConnector,
			Piece.RoomCoverSide,
			Piece.RoomWindowSide,
			Piece.RoomHatch,
			Piece.Invalid,
			Piece.Invalid,
			Piece.RoomReinforcementSide,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.RoomPlanterSide,
			Piece.RoomFiltrationMachine,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.Invalid,
			Piece.RoomCoverTop,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.RoomLadderTop,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.Invalid,
			Piece.RoomCoverBottom,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.RoomLadderBottom,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		}
	};

	private static readonly Piece[,] roomFaceCentralPieces = new Piece[6, 21]
	{
		{
			Piece.Invalid,
			Piece.RoomWaterParkSide,
			Piece.Invalid,
			Piece.RoomWaterParkHatch,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.RoomBioReactor,
			Piece.RoomNuclearReactor,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.Invalid,
			Piece.RoomWaterParkSide,
			Piece.Invalid,
			Piece.RoomWaterParkHatch,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.RoomBioReactor,
			Piece.RoomNuclearReactor,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.Invalid,
			Piece.RoomWaterParkSide,
			Piece.Invalid,
			Piece.RoomWaterParkHatch,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.RoomBioReactor,
			Piece.RoomNuclearReactor,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.Invalid,
			Piece.RoomWaterParkSide,
			Piece.Invalid,
			Piece.RoomWaterParkHatch,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.RoomBioReactor,
			Piece.RoomNuclearReactor,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.Invalid,
			Piece.RoomCoverTop,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.RoomLadderTop,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.RoomWaterParkTop,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.Invalid,
			Piece.RoomCoverBottom,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.RoomLadderBottom,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.RoomWaterParkBottom,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		}
	};

	private static readonly Piece[,] moonpoolFacePieces = new Piece[4, 21]
	{
		{
			Piece.MoonpoolCorridorConnector,
			Piece.MoonpoolCoverSide,
			Piece.MoonpoolWindowSide,
			Piece.MoonpoolHatch,
			Piece.Invalid,
			Piece.Invalid,
			Piece.MoonpoolReinforcementSide,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.MoonpoolUpgradeConsole,
			Piece.MoonpoolPlanterSide,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.MoonpoolCorridorConnector,
			Piece.MoonpoolCoverSide,
			Piece.MoonpoolWindowSide,
			Piece.MoonpoolHatch,
			Piece.Invalid,
			Piece.Invalid,
			Piece.MoonpoolReinforcementSide,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.MoonpoolUpgradeConsole,
			Piece.MoonpoolPlanterSide,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.MoonpoolCorridorConnectorShort,
			Piece.MoonpoolCoverSideShort,
			Piece.MoonpoolWindowSideShort,
			Piece.MoonpoolHatchShort,
			Piece.Invalid,
			Piece.Invalid,
			Piece.MoonpoolReinforcementSideShort,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.MoonpoolUpgradeConsoleShort,
			Piece.MoonpoolPlanterSideShort,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.MoonpoolCorridorConnectorShort,
			Piece.MoonpoolCoverSideShort,
			Piece.MoonpoolWindowSideShort,
			Piece.MoonpoolHatchShort,
			Piece.Invalid,
			Piece.Invalid,
			Piece.MoonpoolReinforcementSideShort,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.MoonpoolUpgradeConsoleShort,
			Piece.MoonpoolPlanterSideShort,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		}
	};

	private static readonly Piece[,] moonpoolRotatedFacePieces = new Piece[4, 21]
	{
		{
			Piece.MoonpoolCorridorConnectorShort,
			Piece.MoonpoolCoverSideShort,
			Piece.MoonpoolWindowSideShort,
			Piece.MoonpoolHatchShort,
			Piece.Invalid,
			Piece.Invalid,
			Piece.MoonpoolReinforcementSideShort,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.MoonpoolUpgradeConsoleShort,
			Piece.MoonpoolPlanterSideShort,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.MoonpoolCorridorConnectorShort,
			Piece.MoonpoolCoverSideShort,
			Piece.MoonpoolWindowSideShort,
			Piece.MoonpoolHatchShort,
			Piece.Invalid,
			Piece.Invalid,
			Piece.MoonpoolReinforcementSideShort,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.MoonpoolUpgradeConsoleShort,
			Piece.MoonpoolPlanterSideShort,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.MoonpoolCorridorConnector,
			Piece.MoonpoolCoverSide,
			Piece.MoonpoolWindowSide,
			Piece.MoonpoolHatch,
			Piece.Invalid,
			Piece.Invalid,
			Piece.MoonpoolReinforcementSide,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.MoonpoolUpgradeConsole,
			Piece.MoonpoolPlanterSide,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.MoonpoolCorridorConnector,
			Piece.MoonpoolCoverSide,
			Piece.MoonpoolWindowSide,
			Piece.MoonpoolHatch,
			Piece.Invalid,
			Piece.Invalid,
			Piece.MoonpoolReinforcementSide,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.MoonpoolUpgradeConsole,
			Piece.MoonpoolPlanterSide,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		}
	};

	private static readonly Piece[,] largeRoomFacePieces = new Piece[6, 21]
	{
		{
			Piece.LargeRoomCorridorConnector,
			Piece.LargeRoomCoverSide,
			Piece.LargeRoomWindowSide,
			Piece.LargeRoomHatch,
			Piece.Invalid,
			Piece.Invalid,
			Piece.LargeRoomReinforcementSide,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.LargeRoomPlanterSide,
			Piece.LargeRoomFiltrationMachine,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.LargeRoomCorridorConnector,
			Piece.LargeRoomCoverSide,
			Piece.LargeRoomWindowSide,
			Piece.LargeRoomHatch,
			Piece.Invalid,
			Piece.Invalid,
			Piece.LargeRoomReinforcementSide,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.LargeRoomPlanterSide,
			Piece.LargeRoomFiltrationMachine,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.LargeRoomCorridorConnectorShort,
			Piece.LargeRoomCoverSideShort,
			Piece.LargeRoomWindowSideShort,
			Piece.LargeRoomHatchShort,
			Piece.Invalid,
			Piece.Invalid,
			Piece.LargeRoomReinforcementSideShort,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.LargeRoomPlanterSideShort,
			Piece.LargeRoomFiltrationMachineShort,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.LargeRoomCorridorConnectorShort,
			Piece.LargeRoomCoverSideShort,
			Piece.LargeRoomWindowSideShort,
			Piece.LargeRoomHatchShort,
			Piece.Invalid,
			Piece.Invalid,
			Piece.LargeRoomReinforcementSideShort,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.LargeRoomPlanterSideShort,
			Piece.LargeRoomFiltrationMachineShort,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.Invalid,
			Piece.LargeRoomCoverTop,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.LargeRoomLadderTop,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.Invalid,
			Piece.LargeRoomCoverBottom,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.LargeRoomLadderBottom,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		}
	};

	private static readonly Piece[,] largeRoomRotatedFacePieces = new Piece[6, 21]
	{
		{
			Piece.LargeRoomCorridorConnectorShort,
			Piece.LargeRoomCoverSideShort,
			Piece.LargeRoomWindowSideShort,
			Piece.LargeRoomHatchShort,
			Piece.Invalid,
			Piece.Invalid,
			Piece.LargeRoomReinforcementSideShort,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.LargeRoomPlanterSideShort,
			Piece.LargeRoomFiltrationMachineShort,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.LargeRoomCorridorConnectorShort,
			Piece.LargeRoomCoverSideShort,
			Piece.LargeRoomWindowSideShort,
			Piece.LargeRoomHatchShort,
			Piece.Invalid,
			Piece.Invalid,
			Piece.LargeRoomReinforcementSideShort,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.LargeRoomPlanterSideShort,
			Piece.LargeRoomFiltrationMachineShort,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.LargeRoomCorridorConnector,
			Piece.LargeRoomCoverSide,
			Piece.LargeRoomWindowSide,
			Piece.LargeRoomHatch,
			Piece.Invalid,
			Piece.Invalid,
			Piece.LargeRoomReinforcementSide,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.LargeRoomPlanterSide,
			Piece.LargeRoomFiltrationMachine,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.LargeRoomCorridorConnector,
			Piece.LargeRoomCoverSide,
			Piece.LargeRoomWindowSide,
			Piece.LargeRoomHatch,
			Piece.Invalid,
			Piece.Invalid,
			Piece.LargeRoomReinforcementSide,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.LargeRoomPlanterSide,
			Piece.LargeRoomFiltrationMachine,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.Invalid,
			Piece.LargeRoomCoverTop,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.LargeRoomLadderTop,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.Invalid,
			Piece.LargeRoomCoverBottom,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.LargeRoomLadderBottom,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		}
	};

	private static readonly Piece[,] largeRoomFaceCentralPieces = new Piece[6, 21]
	{
		{
			Piece.Invalid,
			Piece.LargeRoomWaterParkSide,
			Piece.Invalid,
			Piece.LargeRoomWaterParkHatch,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.RoomBioReactor,
			Piece.RoomNuclearReactor,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.Invalid,
			Piece.LargeRoomWaterParkSide,
			Piece.Invalid,
			Piece.LargeRoomWaterParkHatch,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.RoomBioReactor,
			Piece.RoomNuclearReactor,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.Invalid,
			Piece.LargeRoomWaterParkSide,
			Piece.Invalid,
			Piece.LargeRoomWaterParkHatchShort,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.RoomBioReactor,
			Piece.RoomNuclearReactor,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.Invalid,
			Piece.LargeRoomWaterParkSide,
			Piece.Invalid,
			Piece.LargeRoomWaterParkHatchShort,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.RoomBioReactor,
			Piece.RoomNuclearReactor,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.Invalid,
			Piece.LargeRoomCoverTop,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.LargeRoomLadderTop,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.Invalid,
			Piece.LargeRoomCoverBottom,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.LargeRoomLadderBottom,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		}
	};

	private static readonly Piece[,] largeRoomRotatedFaceCentralPieces = new Piece[6, 21]
	{
		{
			Piece.Invalid,
			Piece.LargeRoomWaterParkSide,
			Piece.Invalid,
			Piece.LargeRoomWaterParkHatchShort,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.RoomBioReactor,
			Piece.RoomNuclearReactor,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.Invalid,
			Piece.LargeRoomWaterParkSide,
			Piece.Invalid,
			Piece.LargeRoomWaterParkHatchShort,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.RoomBioReactor,
			Piece.RoomNuclearReactor,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.Invalid,
			Piece.LargeRoomWaterParkSide,
			Piece.Invalid,
			Piece.LargeRoomWaterParkHatch,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.RoomBioReactor,
			Piece.RoomNuclearReactor,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.Invalid,
			Piece.LargeRoomWaterParkSide,
			Piece.Invalid,
			Piece.LargeRoomWaterParkHatch,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.RoomBioReactor,
			Piece.RoomNuclearReactor,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.Invalid,
			Piece.LargeRoomCoverTop,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.LargeRoomLadderTop,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		},
		{
			Piece.Invalid,
			Piece.LargeRoomCoverBottom,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.LargeRoomLadderBottom,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid,
			Piece.Invalid
		}
	};

	public static readonly Direction[] HorizontalDirections = new Direction[4]
	{
		Direction.North,
		Direction.East,
		Direction.South,
		Direction.West
	};

	public static readonly Direction[] VerticalDirections = new Direction[2]
	{
		Direction.Above,
		Direction.Below
	};

	public static readonly Direction[] AllDirections = new Direction[6]
	{
		Direction.North,
		Direction.South,
		Direction.East,
		Direction.West,
		Direction.Above,
		Direction.Below
	};

	public static readonly Direction[] OppositeDirections = new Direction[6]
	{
		Direction.South,
		Direction.North,
		Direction.West,
		Direction.East,
		Direction.Below,
		Direction.Above
	};

	private static readonly Vector3[] DirectionNormals = new Vector3[6]
	{
		Vector3.forward,
		Vector3.back,
		Vector3.right,
		Vector3.left,
		Vector3.up,
		Vector3.down
	};

	private static readonly Vector3[] FaceNormals = new Vector3[6]
	{
		Vector3.back,
		Vector3.forward,
		Vector3.left,
		Vector3.right,
		Vector3.down,
		Vector3.up
	};

	public static readonly Int3[] DirectionOffset = new Int3[6]
	{
		new Int3(0, 0, 1),
		new Int3(0, 0, -1),
		new Int3(1, 0, 0),
		new Int3(-1, 0, 0),
		new Int3(0, 1, 0),
		new Int3(0, -1, 0)
	};

	private static readonly Quaternion[] FaceRotation = new Quaternion[6]
	{
		Quaternion.Euler(0f, -90f, 0f),
		Quaternion.Euler(0f, 90f, 0f),
		Quaternion.Euler(0f, 0f, 0f),
		Quaternion.Euler(0f, -180f, 0f),
		Quaternion.Euler(-90f, 0f, 0f),
		Quaternion.Euler(90f, 0f, 0f)
	};

	public const byte NorthMask = 1;

	public const byte SouthMask = 2;

	public const byte EastMask = 4;

	public const byte WestMask = 8;

	public const byte AboveMask = 16;

	public const byte BelowMask = 32;

	public const byte CellUsedMask = 64;

	public const byte HorizontalMask = 15;

	private static readonly FaceDef[][] faceDefs = new FaceDef[20][]
	{
		null,
		new FaceDef[34]
		{
			new FaceDef(0, 0, 0, Direction.South, OccupiedFaceType(Direction.West)),
			new FaceDef(0, 0, 2, Direction.West, OccupiedFaceType(Direction.North)),
			new FaceDef(2, 0, 2, Direction.North, OccupiedFaceType(Direction.East)),
			new FaceDef(2, 0, 0, Direction.East, OccupiedFaceType(Direction.South)),
			new FaceDef(0, 0, 0, Direction.West, FaceType.Solid),
			new FaceDef(0, 0, 1, Direction.West, FaceType.Solid),
			new FaceDef(0, 0, 2, Direction.North, FaceType.Solid),
			new FaceDef(1, 0, 2, Direction.North, FaceType.Solid),
			new FaceDef(2, 0, 2, Direction.East, FaceType.Solid),
			new FaceDef(2, 0, 1, Direction.East, FaceType.Solid),
			new FaceDef(2, 0, 0, Direction.South, FaceType.Solid),
			new FaceDef(1, 0, 0, Direction.South, FaceType.Solid),
			new FaceDef(0, 0, 0, Direction.Above, FaceType.Solid),
			new FaceDef(1, 0, 0, Direction.Above, FaceType.Solid),
			new FaceDef(2, 0, 0, Direction.Above, FaceType.Solid),
			new FaceDef(0, 0, 1, Direction.Above, FaceType.Solid),
			new FaceDef(1, 0, 1, Direction.Above, FaceType.Solid),
			new FaceDef(2, 0, 1, Direction.Above, FaceType.Solid),
			new FaceDef(0, 0, 2, Direction.Above, FaceType.Solid),
			new FaceDef(1, 0, 2, Direction.Above, FaceType.Solid),
			new FaceDef(2, 0, 2, Direction.Above, FaceType.Solid),
			new FaceDef(0, 0, 0, Direction.Below, FaceType.Solid),
			new FaceDef(1, 0, 0, Direction.Below, FaceType.Solid),
			new FaceDef(2, 0, 0, Direction.Below, FaceType.Solid),
			new FaceDef(0, 0, 1, Direction.Below, FaceType.Solid),
			new FaceDef(1, 0, 1, Direction.Below, FaceType.Solid),
			new FaceDef(2, 0, 1, Direction.Below, FaceType.Solid),
			new FaceDef(0, 0, 2, Direction.Below, FaceType.Solid),
			new FaceDef(1, 0, 2, Direction.Below, FaceType.Solid),
			new FaceDef(2, 0, 2, Direction.Below, FaceType.Solid),
			new FaceDef(1, 0, 1, Direction.West, FaceType.None),
			new FaceDef(1, 0, 1, Direction.North, FaceType.None),
			new FaceDef(1, 0, 1, Direction.East, FaceType.None),
			new FaceDef(1, 0, 1, Direction.South, FaceType.None)
		},
		null,
		null,
		new FaceDef[6]
		{
			new FaceDef(0, 0, 0, Direction.North, FaceType.Solid),
			new FaceDef(0, 0, 0, Direction.East, FaceType.Solid),
			new FaceDef(0, 0, 0, Direction.South, FaceType.Solid),
			new FaceDef(0, 0, 0, Direction.West, FaceType.Solid),
			new FaceDef(0, 0, 0, Direction.Above, FaceType.Solid),
			new FaceDef(0, 0, 0, Direction.Below, FaceType.Solid)
		},
		new FaceDef[6]
		{
			new FaceDef(0, 0, 0, Direction.North, FaceType.Solid),
			new FaceDef(0, 0, 0, Direction.East, FaceType.Solid),
			new FaceDef(0, 0, 0, Direction.South, FaceType.Solid),
			new FaceDef(0, 0, 0, Direction.West, FaceType.Solid),
			new FaceDef(0, 0, 0, Direction.Above, FaceType.Solid),
			new FaceDef(0, 0, 0, Direction.Below, FaceType.Solid)
		},
		new FaceDef[0],
		new FaceDef[6]
		{
			new FaceDef(1, 0, 0, Direction.South, FaceType.Solid),
			new FaceDef(2, 0, 0, Direction.South, FaceType.Solid),
			new FaceDef(1, 0, 2, Direction.North, FaceType.Solid),
			new FaceDef(2, 0, 2, Direction.North, FaceType.Solid),
			new FaceDef(3, 0, 1, Direction.East, FaceType.Solid),
			new FaceDef(0, 0, 1, Direction.West, FaceType.Solid)
		},
		new FaceDef[2]
		{
			new FaceDef(0, 0, 1, Direction.West, FaceType.Solid),
			new FaceDef(2, 0, 1, Direction.East, FaceType.Solid)
		},
		new FaceDef[2]
		{
			new FaceDef(1, 0, 2, Direction.North, FaceType.Solid),
			new FaceDef(1, 0, 0, Direction.South, FaceType.Solid)
		},
		null,
		new FaceDef[3]
		{
			new FaceDef(0, 0, 1, Direction.West, FaceType.Solid),
			new FaceDef(2, 0, 1, Direction.East, FaceType.Solid),
			new FaceDef(1, 0, 1, Direction.Below, FaceType.ControlRoomModule)
		},
		new FaceDef[3]
		{
			new FaceDef(1, 0, 2, Direction.North, FaceType.Solid),
			new FaceDef(1, 0, 0, Direction.South, FaceType.Solid),
			new FaceDef(1, 0, 1, Direction.Below, FaceType.ControlRoomModule)
		},
		null,
		null,
		null,
		null,
		new FaceDef[62]
		{
			new FaceDef(1, 0, 0, Direction.South, FaceType.Solid),
			new FaceDef(2, 0, 0, Direction.South, FaceType.Solid),
			new FaceDef(3, 0, 0, Direction.South, FaceType.Solid),
			new FaceDef(4, 0, 0, Direction.South, FaceType.Solid),
			new FaceDef(0, 0, 1, Direction.West, FaceType.Solid),
			new FaceDef(5, 0, 1, Direction.East, FaceType.Solid),
			new FaceDef(1, 0, 2, Direction.North, FaceType.Solid),
			new FaceDef(2, 0, 2, Direction.North, FaceType.Solid),
			new FaceDef(3, 0, 2, Direction.North, FaceType.Solid),
			new FaceDef(4, 0, 2, Direction.North, FaceType.Solid),
			new FaceDef(0, 0, 0, Direction.Above, FaceType.Solid),
			new FaceDef(1, 0, 0, Direction.Above, FaceType.Solid),
			new FaceDef(2, 0, 0, Direction.Above, FaceType.Solid),
			new FaceDef(3, 0, 0, Direction.Above, FaceType.Solid),
			new FaceDef(4, 0, 0, Direction.Above, FaceType.Solid),
			new FaceDef(5, 0, 0, Direction.Above, FaceType.Solid),
			new FaceDef(0, 0, 1, Direction.Above, FaceType.Solid),
			new FaceDef(1, 0, 1, Direction.Above, FaceType.Solid),
			new FaceDef(2, 0, 1, Direction.Above, FaceType.Solid),
			new FaceDef(3, 0, 1, Direction.Above, FaceType.Solid),
			new FaceDef(4, 0, 1, Direction.Above, FaceType.Solid),
			new FaceDef(5, 0, 1, Direction.Above, FaceType.Solid),
			new FaceDef(0, 0, 2, Direction.Above, FaceType.Solid),
			new FaceDef(1, 0, 2, Direction.Above, FaceType.Solid),
			new FaceDef(2, 0, 2, Direction.Above, FaceType.Solid),
			new FaceDef(3, 0, 2, Direction.Above, FaceType.Solid),
			new FaceDef(4, 0, 2, Direction.Above, FaceType.Solid),
			new FaceDef(5, 0, 2, Direction.Above, FaceType.Solid),
			new FaceDef(0, 0, 0, Direction.Below, FaceType.Solid),
			new FaceDef(1, 0, 0, Direction.Below, FaceType.Solid),
			new FaceDef(2, 0, 0, Direction.Below, FaceType.Solid),
			new FaceDef(3, 0, 0, Direction.Below, FaceType.Solid),
			new FaceDef(4, 0, 0, Direction.Below, FaceType.Solid),
			new FaceDef(5, 0, 0, Direction.Below, FaceType.Solid),
			new FaceDef(0, 0, 1, Direction.Below, FaceType.Solid),
			new FaceDef(1, 0, 1, Direction.Below, FaceType.Solid),
			new FaceDef(2, 0, 1, Direction.Below, FaceType.Solid),
			new FaceDef(3, 0, 1, Direction.Below, FaceType.Solid),
			new FaceDef(4, 0, 1, Direction.Below, FaceType.Solid),
			new FaceDef(5, 0, 1, Direction.Below, FaceType.Solid),
			new FaceDef(0, 0, 2, Direction.Below, FaceType.Solid),
			new FaceDef(1, 0, 2, Direction.Below, FaceType.Solid),
			new FaceDef(2, 0, 2, Direction.Below, FaceType.Solid),
			new FaceDef(3, 0, 2, Direction.Below, FaceType.Solid),
			new FaceDef(4, 0, 2, Direction.Below, FaceType.Solid),
			new FaceDef(5, 0, 2, Direction.Below, FaceType.Solid),
			new FaceDef(1, 0, 1, Direction.North, FaceType.None),
			new FaceDef(1, 0, 1, Direction.South, FaceType.None),
			new FaceDef(1, 0, 1, Direction.East, FaceType.None),
			new FaceDef(1, 0, 1, Direction.West, FaceType.None),
			new FaceDef(2, 0, 1, Direction.North, FaceType.None),
			new FaceDef(2, 0, 1, Direction.South, FaceType.None),
			new FaceDef(2, 0, 1, Direction.East, FaceType.None),
			new FaceDef(2, 0, 1, Direction.West, FaceType.None),
			new FaceDef(3, 0, 1, Direction.North, FaceType.None),
			new FaceDef(3, 0, 1, Direction.South, FaceType.None),
			new FaceDef(3, 0, 1, Direction.East, FaceType.None),
			new FaceDef(3, 0, 1, Direction.West, FaceType.None),
			new FaceDef(4, 0, 1, Direction.North, FaceType.None),
			new FaceDef(4, 0, 1, Direction.South, FaceType.None),
			new FaceDef(4, 0, 1, Direction.East, FaceType.None),
			new FaceDef(4, 0, 1, Direction.West, FaceType.None)
		},
		new FaceDef[46]
		{
			new FaceDef(1, 0, 0, Direction.South, FaceType.Solid),
			new FaceDef(0, 0, 1, Direction.West, FaceType.Solid),
			new FaceDef(0, 0, 2, Direction.West, FaceType.Solid),
			new FaceDef(0, 0, 3, Direction.West, FaceType.Solid),
			new FaceDef(0, 0, 4, Direction.West, FaceType.Solid),
			new FaceDef(2, 0, 1, Direction.East, FaceType.Solid),
			new FaceDef(2, 0, 2, Direction.East, FaceType.Solid),
			new FaceDef(2, 0, 3, Direction.East, FaceType.Solid),
			new FaceDef(2, 0, 4, Direction.East, FaceType.Solid),
			new FaceDef(1, 0, 5, Direction.North, FaceType.Solid),
			new FaceDef(0, 0, 0, Direction.Above, FaceType.Solid),
			new FaceDef(1, 0, 0, Direction.Above, FaceType.Solid),
			new FaceDef(2, 0, 0, Direction.Above, FaceType.Solid),
			new FaceDef(0, 0, 1, Direction.Above, FaceType.Solid),
			new FaceDef(1, 0, 1, Direction.Above, FaceType.Solid),
			new FaceDef(2, 0, 1, Direction.Above, FaceType.Solid),
			new FaceDef(0, 0, 2, Direction.Above, FaceType.Solid),
			new FaceDef(1, 0, 2, Direction.Above, FaceType.Solid),
			new FaceDef(2, 0, 2, Direction.Above, FaceType.Solid),
			new FaceDef(0, 0, 3, Direction.Above, FaceType.Solid),
			new FaceDef(1, 0, 3, Direction.Above, FaceType.Solid),
			new FaceDef(2, 0, 3, Direction.Above, FaceType.Solid),
			new FaceDef(0, 0, 4, Direction.Above, FaceType.Solid),
			new FaceDef(1, 0, 4, Direction.Above, FaceType.Solid),
			new FaceDef(2, 0, 4, Direction.Above, FaceType.Solid),
			new FaceDef(0, 0, 5, Direction.Above, FaceType.Solid),
			new FaceDef(1, 0, 5, Direction.Above, FaceType.Solid),
			new FaceDef(2, 0, 5, Direction.Above, FaceType.Solid),
			new FaceDef(0, 0, 0, Direction.Below, FaceType.Solid),
			new FaceDef(1, 0, 0, Direction.Below, FaceType.Solid),
			new FaceDef(2, 0, 0, Direction.Below, FaceType.Solid),
			new FaceDef(0, 0, 1, Direction.Below, FaceType.Solid),
			new FaceDef(1, 0, 1, Direction.Below, FaceType.Solid),
			new FaceDef(2, 0, 1, Direction.Below, FaceType.Solid),
			new FaceDef(0, 0, 2, Direction.Below, FaceType.Solid),
			new FaceDef(1, 0, 2, Direction.Below, FaceType.Solid),
			new FaceDef(2, 0, 2, Direction.Below, FaceType.Solid),
			new FaceDef(0, 0, 3, Direction.Below, FaceType.Solid),
			new FaceDef(1, 0, 3, Direction.Below, FaceType.Solid),
			new FaceDef(2, 0, 3, Direction.Below, FaceType.Solid),
			new FaceDef(0, 0, 4, Direction.Below, FaceType.Solid),
			new FaceDef(1, 0, 4, Direction.Below, FaceType.Solid),
			new FaceDef(2, 0, 4, Direction.Below, FaceType.Solid),
			new FaceDef(0, 0, 5, Direction.Below, FaceType.Solid),
			new FaceDef(1, 0, 5, Direction.Below, FaceType.Solid),
			new FaceDef(2, 0, 5, Direction.Below, FaceType.Solid)
		},
		new FaceDef[6]
		{
			new FaceDef(0, 0, 1, Direction.West, FaceType.Solid),
			new FaceDef(0, 0, 2, Direction.West, FaceType.Solid),
			new FaceDef(2, 0, 1, Direction.East, FaceType.Solid),
			new FaceDef(2, 0, 2, Direction.East, FaceType.Solid),
			new FaceDef(1, 0, 3, Direction.North, FaceType.Solid),
			new FaceDef(1, 0, 0, Direction.South, FaceType.Solid)
		}
	};

	[HideInInspector]
	public bool isGhost;

	private FaceType[] previousfaces;

	[NonSerialized]
	[ProtoMember(1)]
	public Grid3Shape baseShape;

	[NonSerialized]
	[ProtoMember(2, OverwriteList = true)]
	public FaceType[] faces;

	[NonSerialized]
	[ProtoMember(3, OverwriteList = true)]
	public CellType[] cells;

	[NonSerialized]
	[ProtoMember(4, OverwriteList = true)]
	public byte[] links;

	[NonSerialized]
	[ProtoMember(5)]
	public Int3 cellOffset;

	[NonSerialized]
	[ProtoMember(6, OverwriteList = true)]
	public byte[] masks;

	[NonSerialized]
	[ProtoMember(7, OverwriteList = true)]
	public bool[] isGlass;

	[NonSerialized]
	[ProtoMember(8)]
	public Int3 anchor = Int3.zero;

	[NonSerialized]
	public byte[] flowData;

	private Transform[] cellObjects;

	private List<int> occupiedCellIndexes;

	private Bounds occupiedBounds;

	private Transform[] oldCellObjects;

	private CellType[] oldCells;

	private FaceType[] oldFaces;

	private byte[] oldLinks;

	private bool[] oldIsGlass;

	private List<Int3> touchedCells = new List<Int3>();

	private bool autobuilding;

	private Base autoBuildBase;

	private GameObject autoBuildBaseGo;

	private float timeLastAutoBuild;

	private static List<BaseDeconstructable> sDeconstructables = new List<BaseDeconstructable>();

	private static List<IBaseModule> sBaseModules = new List<IBaseModule>();

	private static List<IBaseModuleGeometry> sBaseModulesGeometry = new List<IBaseModuleGeometry>();

	private static readonly Dictionary<int, Piece> largeRoomInteriorTopPieces = new Dictionary<int, Piece>
	{
		{
			3,
			Piece.LargeRoomInteriorTopHole1
		},
		{
			6,
			Piece.LargeRoomInteriorTopHole2
		},
		{
			12,
			Piece.LargeRoomInteriorTopHole3
		},
		{
			15,
			Piece.LargeRoomInteriorTopHole4
		}
	};

	private static readonly Dictionary<int, Piece> largeRoomInteriorBottomPieces = new Dictionary<int, Piece>
	{
		{
			3,
			Piece.LargeRoomInteriorBottomHole1
		},
		{
			6,
			Piece.LargeRoomInteriorBottomHole2
		},
		{
			12,
			Piece.LargeRoomInteriorBottomHole3
		},
		{
			15,
			Piece.LargeRoomInteriorBottomHole4
		}
	};

	private static readonly Piece[] waterParkCeilingPieces = new Piece[4]
	{
		Piece.LargeRoomWaterParkCeilingGlass,
		Piece.LargeRoomWaterParkCeilingMiddleTop,
		Piece.LargeRoomWaterParkCeilingTopMiddle,
		Piece.LargeRoomWaterParkCeilingMiddleMiddle
	};

	private static readonly Piece[] waterParkFloorPieces = new Piece[4]
	{
		Piece.LargeRoomWaterParkFloorBottom,
		Piece.LargeRoomWaterParkFloorMiddleBottom,
		Piece.LargeRoomWaterParkFloorBottomMiddle,
		Piece.LargeRoomWaterParkFloorMiddleMiddle
	};

	private static readonly Dictionary<int, Piece> partitionCentralPieces = new Dictionary<int, Piece>
	{
		{
			3,
			Piece.PartitionCentralI
		},
		{
			12,
			Piece.PartitionCentralI90
		},
		{
			5,
			Piece.PartitionCentralL
		},
		{
			6,
			Piece.PartitionCentralL90
		},
		{
			10,
			Piece.PartitionCentralL180
		},
		{
			9,
			Piece.PartitionCentralL270
		},
		{
			13,
			Piece.PartitionCentralT
		},
		{
			7,
			Piece.PartitionCentralT90
		},
		{
			14,
			Piece.PartitionCentralT180
		},
		{
			11,
			Piece.PartitionCentralT270
		},
		{
			15,
			Piece.PartitionCentralX
		}
	};

	private bool deserializationFinished;

	private int currentAccessoryGeoIndex;

	private static readonly List<GameObject> overlappedObjects = new List<GameObject>();

	private readonly List<Renderer> hiddenRenderers = new List<Renderer>();

	private readonly List<GameObject> hiddenObjects = new List<GameObject>();

	private List<RebuildAccessoryGeometry> rebuildAccessoryGeometry = new List<RebuildAccessoryGeometry>();

	private List<CachedPiece> cachedPieces = new List<CachedPiece>();

	public Grid3Shape Shape => baseShape;

	public Int3.RangeEnumerator AllCells => Int3.Range(baseShape.ToInt3());

	public List<int> OccupiedCellIndexes => occupiedCellIndexes;

	public Int3.Bounds Bounds => new Int3.Bounds(Int3.zero, baseShape.ToInt3() - 1);

	public event BaseEventHandler onPostRebuildGeometry;

	public event BaseResizeEventHandler onBaseResize;

	public event BaseFaceEventHandler onBulkheadFaceChanged;

	private static float ApplyDepthScaling(float str, float y)
	{
		if (str >= 0f)
		{
			return str;
		}
		float num = Ocean.GetOceanLevel() - y;
		return Mathf.Max(1f, 1f + (num - 100f) / 1000f) * str;
	}

	[SuppressMessage("Gendarme.Rules.Maintainability", "AvoidComplexMethodsRule")]
	private static Partition GetPartition(CellType cellType, Int3 offset, int partitionMask, bool hasDoor, out int maskRotation)
	{
		Piece piece = Piece.Invalid;
		int num = -1;
		maskRotation = 0;
		Int3 @int = CellSize[(uint)cellType];
		switch (cellType)
		{
		case CellType.LargeRoom:
			if (offset.z == 0)
			{
				if (offset.x == 0)
				{
					num = 3;
					maskRotation = 1;
				}
				else if (offset.x > 0 && offset.x < 5)
				{
					num = 1;
					maskRotation = 1;
				}
				else if (offset.x == 5)
				{
					num = 3;
					maskRotation = 0;
				}
			}
			else if (offset.z == 1)
			{
				if (offset.x == 0)
				{
					num = 2;
					maskRotation = 2;
				}
				else if (offset.x > 0 && offset.x < 5)
				{
					num = 0;
					maskRotation = 0;
				}
				else if (offset.x == 5)
				{
					num = 2;
					maskRotation = 0;
				}
			}
			else if (offset.z == 2)
			{
				if (offset.x == 0)
				{
					num = 3;
					maskRotation = 2;
				}
				else if (offset.x > 0 && offset.x < 5)
				{
					num = 1;
					maskRotation = 3;
				}
				else if (offset.x == 5)
				{
					num = 3;
					maskRotation = 3;
				}
			}
			break;
		case CellType.LargeRoomRotated:
			if (offset.x == 0)
			{
				if (offset.z == 0)
				{
					num = 3;
					maskRotation = 1;
				}
				else if (offset.z > 0 && offset.z < 5)
				{
					num = 1;
					maskRotation = 2;
				}
				else if (offset.z == 5)
				{
					num = 3;
					maskRotation = 2;
				}
			}
			else if (offset.x == 1)
			{
				if (offset.z == 0)
				{
					num = 2;
					maskRotation = 1;
				}
				else if (offset.z > 0 && offset.z < 5)
				{
					num = 0;
					maskRotation = 0;
				}
				else if (offset.z == 5)
				{
					num = 2;
					maskRotation = 3;
				}
			}
			else if (offset.x == 2)
			{
				if (offset.z == 0)
				{
					num = 3;
					maskRotation = 0;
				}
				else if (offset.z > 0 && offset.z < 5)
				{
					num = 1;
					maskRotation = 0;
				}
				else if (offset.z == 5)
				{
					num = 3;
					maskRotation = 3;
				}
			}
			break;
		}
		int num2 = 0;
		for (int i = 0; i < 4; i++)
		{
			if ((partitionMask & (1 << rotationCCWRemap[maskRotation * 4 + i])) != 0)
			{
				num2 |= 1 << i;
			}
		}
		Vector3 vector = new Vector3(0f, 0f, 0f);
		int num3 = 0;
		switch (partitionMask)
		{
		case 3:
		{
			int num5 = (@int.z + 1) / 2;
			num3 = ((offset.z < num5) ? 2 : 0);
			break;
		}
		case 12:
		{
			int num4 = (@int.x + 1) / 2;
			num3 = ((offset.x >= num4) ? 1 : 3);
			break;
		}
		}
		switch (num)
		{
		case 0:
			switch (num2)
			{
			case 1:
				piece = Piece.PartitionCentralIHalf;
				break;
			case 4:
				piece = Piece.PartitionCentralIHalf90;
				break;
			case 2:
				piece = Piece.PartitionCentralIHalf180;
				break;
			case 8:
				piece = Piece.PartitionCentralIHalf270;
				break;
			case 3:
				piece = (hasDoor ? Piece.PartitionDoorwayCentralI : Piece.PartitionCentralI);
				break;
			case 12:
				piece = (hasDoor ? Piece.PartitionDoorwayCentralI90 : Piece.PartitionCentralI90);
				break;
			case 5:
				piece = Piece.PartitionCentralL;
				break;
			case 6:
				piece = Piece.PartitionCentralL90;
				break;
			case 10:
				piece = Piece.PartitionCentralL180;
				break;
			case 9:
				piece = Piece.PartitionCentralL270;
				break;
			case 13:
				piece = Piece.PartitionCentralT;
				break;
			case 7:
				piece = Piece.PartitionCentralT90;
				break;
			case 14:
				piece = Piece.PartitionCentralT180;
				break;
			case 11:
				piece = Piece.PartitionCentralT270;
				break;
			case 15:
				piece = Piece.PartitionCentralX;
				break;
			}
			break;
		case 1:
			switch (num2)
			{
			case 1:
				piece = Piece.PartitionSideIHalf;
				break;
			case 4:
				piece = Piece.PartitionSideIHalf90;
				break;
			case 2:
				piece = Piece.PartitionSideIHalf180;
				break;
			case 8:
				piece = Piece.PartitionSideIHalf270;
				break;
			case 3:
				piece = (hasDoor ? Piece.PartitionDoorwaySideI : Piece.PartitionSideI);
				vector = new Vector3(-1f, 0f, 0f);
				break;
			case 12:
				piece = (hasDoor ? Piece.PartitionDoorwaySideI90 : Piece.PartitionSideI90);
				vector = new Vector3(-1.6277f, 0f, 0f);
				break;
			case 5:
				piece = Piece.PartitionSideL;
				break;
			case 6:
				piece = Piece.PartitionSideL90;
				break;
			case 10:
				piece = Piece.PartitionSideL180;
				break;
			case 9:
				piece = Piece.PartitionSideL270;
				break;
			case 13:
				piece = Piece.PartitionSideT;
				break;
			case 7:
				piece = Piece.PartitionSideT90;
				break;
			case 14:
				piece = Piece.PartitionSideT180;
				break;
			case 11:
				piece = Piece.PartitionSideT270;
				break;
			case 15:
				piece = Piece.PartitionSideX;
				break;
			}
			break;
		case 2:
			switch (num2)
			{
			case 1:
				piece = Piece.PartitionSideShortIHalf;
				break;
			case 4:
				piece = Piece.PartitionSideShortIHalf90;
				break;
			case 2:
				piece = Piece.PartitionSideShortIHalf180;
				break;
			case 8:
				piece = Piece.PartitionSideShortIHalf270;
				break;
			case 3:
				piece = (hasDoor ? Piece.PartitionDoorwaySideShortI : Piece.PartitionSideShortI);
				vector = new Vector3(-1f, 0f, 0f);
				break;
			case 12:
				piece = (hasDoor ? Piece.PartitionDoorwaySideShortI90 : Piece.PartitionSideShortI90);
				vector = new Vector3(-1.631f, 0f, 0f);
				break;
			case 5:
				piece = Piece.PartitionSideShortL;
				break;
			case 6:
				piece = Piece.PartitionSideShortL90;
				break;
			case 10:
				piece = Piece.PartitionSideShortL180;
				break;
			case 9:
				piece = Piece.PartitionSideShortL270;
				break;
			case 13:
				piece = Piece.PartitionSideShortT;
				break;
			case 7:
				piece = Piece.PartitionSideShortT90;
				break;
			case 14:
				piece = Piece.PartitionSideShortT180;
				break;
			case 11:
				piece = Piece.PartitionSideShortT270;
				break;
			case 15:
				piece = Piece.PartitionSideShortX;
				break;
			}
			break;
		case 3:
			switch (num2)
			{
			case 1:
				piece = Piece.PartitionCornerIHalf;
				break;
			case 4:
				piece = Piece.PartitionCornerIHalf90;
				break;
			case 2:
				piece = Piece.PartitionCornerIHalf180;
				break;
			case 8:
				piece = Piece.PartitionCornerIHalf270;
				break;
			case 3:
				piece = (hasDoor ? Piece.PartitionDoorwayCornerI : Piece.PartitionCornerI);
				vector = new Vector3(-1f, 0f, 1.631f);
				break;
			case 12:
				piece = (hasDoor ? Piece.PartitionDoorwayCornerI90 : Piece.PartitionCornerI90);
				vector = new Vector3(-1.631f, 0f, 1f);
				break;
			case 5:
				piece = Piece.PartitionCornerL;
				break;
			case 6:
				piece = Piece.PartitionCornerL90;
				break;
			case 10:
				piece = Piece.PartitionCornerL180;
				break;
			case 9:
				piece = Piece.PartitionCornerL270;
				break;
			case 13:
				piece = Piece.PartitionCornerT;
				break;
			case 7:
				piece = Piece.PartitionCornerT90;
				break;
			case 14:
				piece = Piece.PartitionCornerT180;
				break;
			case 11:
				piece = Piece.PartitionCornerT270;
				break;
			case 15:
				piece = Piece.PartitionCornerX;
				break;
			}
			break;
		}
		Quaternion quaternion = Quaternion.Euler(0f, 90f * (float)maskRotation, 0f);
		vector = quaternion * vector;
		return new Partition(piece, quaternion, vector, Quaternion.Euler(0f, 90f * (float)num3, 0f));
	}

	private bool ExteriorToInteriorPiece(Piece exterior, out Piece interior)
	{
		interior = Piece.Invalid;
		return exteriorToInteriorPiece.TryGetValue(exterior, out interior);
	}

	private CorridorDef GetCorridorDef(int index)
	{
		byte b = links[index];
		if (!isGlass[index])
		{
			return corridors[b];
		}
		return glassCorridors[b];
	}

	public bool IsCorridorCorner(Int3 cell)
	{
		int index = baseShape.GetIndex(cell);
		byte b = links[index];
		CorridorDef corridorDef = (isGlass[index] ? glassCorridors[b] : corridors[b]);
		if (corridorDef.piece != Piece.CorridorLShape)
		{
			return corridorDef.piece == Piece.CorridorLShapeGlass;
		}
		return true;
	}

	public bool IsCorridorT(Int3 cell)
	{
		int index = baseShape.GetIndex(cell);
		byte b = links[index];
		CorridorDef obj = (isGlass[index] ? glassCorridors[b] : corridors[b]);
		return obj.piece == Piece.CorridorTShape;
	}

	public bool IsCorridorX(Int3 cell)
	{
		int index = baseShape.GetIndex(cell);
		byte b = links[index];
		CorridorDef obj = (isGlass[index] ? glassCorridors[b] : corridors[b]);
		return obj.piece == Piece.CorridorXShape;
	}

	public Quaternion GetCorridorRotation(Int3 cell)
	{
		int index = baseShape.GetIndex(cell);
		byte b = links[index];
		CorridorDef obj = (isGlass[index] ? glassCorridors[b] : corridors[b]);
		return obj.rotation;
	}

	private static FaceType OccupiedFaceType(Direction occupyingDirection)
	{
		return FaceType.OccupiedByOtherFace | (FaceType)occupyingDirection;
	}

	public static void Initialize()
	{
		if (!initialized)
		{
			CoroutineUtils.PumpCoroutine(InitializeAsync());
		}
	}

	public static IEnumerator InitializeAsync()
	{
		if (!initialized)
		{
			yield return RegisterPiecesAsync();
			RegisterCorridors();
			if (!mapRoomFunctionalityPrefabRequest.IsValid())
			{
				mapRoomFunctionalityPrefabRequest = AddressablesUtility.LoadAsync<GameObject>("Submarine/Build/MapRoomFunctionality.prefab");
				yield return mapRoomFunctionalityPrefabRequest;
			}
			mapRoomFunctionalityPrefab = mapRoomFunctionalityPrefabRequest.Result;
			if (!cellLoadRequest.IsValid())
			{
				cellLoadRequest = AddressablesUtility.LoadAsync<GameObject>("Base/Ghosts/BaseCell.prefab");
				yield return cellLoadRequest;
			}
			if (cellLoadRequest.Status == AsyncOperationStatus.Succeeded)
			{
				cellPrefab = cellLoadRequest.Result.transform;
			}
			else
			{
				Debug.LogError("Failed to load basepiece: BaseCell");
			}
			initialized = true;
		}
	}

	public static void Deinitialize()
	{
		if (initialized)
		{
			pieces = null;
			corridors = null;
			glassCorridors = null;
			mapRoomFunctionalityPrefab = null;
			AddressablesUtility.QueueRelease(ref mapRoomFunctionalityPrefabRequest);
			cellPrefab = null;
			AddressablesUtility.QueueRelease(ref cellLoadRequest);
			initialized = false;
		}
	}

	private void ClearArrays()
	{
		if (flowData != null)
		{
			Array.Clear(flowData, 0, flowData.Length);
		}
		if (cellObjects != null)
		{
			Array.Clear(cellObjects, 0, cellObjects.Length);
		}
		if (occupiedCellIndexes != null)
		{
			occupiedCellIndexes.Clear();
		}
		if (cells != null)
		{
			Array.Clear(cells, 0, cells.Length);
		}
		if (faces != null)
		{
			Array.Clear(faces, 0, faces.Length);
		}
		if (links != null)
		{
			Array.Clear(links, 0, links.Length);
		}
		if (masks != null)
		{
			Array.Clear(masks, 0, masks.Length);
		}
		if (isGlass != null)
		{
			Array.Clear(isGlass, 0, isGlass.Length);
		}
	}

	private void AllocateArrays()
	{
		if (baseShape.Size != 0)
		{
			UWE.Utils.EnsureArraySize(ref flowData, baseShape.Size);
			UWE.Utils.EnsureArraySize(ref cellObjects, baseShape.Size);
			UWE.Utils.EnsureArraySize(ref cells, baseShape.Size);
			UWE.Utils.EnsureArraySize(ref faces, baseShape.Size * 6);
			UWE.Utils.EnsureArraySize(ref links, baseShape.Size);
			UWE.Utils.EnsureArraySize(ref isGlass, baseShape.Size);
			if (occupiedCellIndexes == null)
			{
				occupiedCellIndexes = new List<int>();
			}
		}
	}

	public Int3 GetSize()
	{
		return baseShape.ToInt3();
	}

	public void SetSize(Int3 size)
	{
		if (!(GetSize() == size))
		{
			baseShape = new Grid3Shape(size);
			ClearArrays();
			AllocateArrays();
			if (this.onBaseResize != null)
			{
				this.onBaseResize(this, Int3.zero);
			}
		}
	}

	private Int3 EnsureSize(Int3.Bounds region)
	{
		if (region.mins >= 0 && region.maxs < baseShape.ToInt3())
		{
			return Int3.zero;
		}
		Int3 zero = Int3.zero;
		if (region.mins.x < 0)
		{
			zero.x = -region.mins.x;
		}
		if (region.mins.y < 0)
		{
			zero.y = -region.mins.y;
		}
		if (region.mins.z < 0)
		{
			zero.z = -region.mins.z;
		}
		Int3.Bounds bounds = Int3.Bounds.Union(region, Bounds);
		Grid3Shape grid3Shape = baseShape;
		UWE.Utils.CopyArray(cellObjects, ref oldCellObjects);
		UWE.Utils.CopyArray(cells, ref oldCells);
		UWE.Utils.CopyArray(faces, ref oldFaces);
		UWE.Utils.CopyArray(links, ref oldLinks);
		UWE.Utils.CopyArray(isGlass, ref oldIsGlass);
		baseShape = new Grid3Shape(bounds.size);
		ClearArrays();
		AllocateArrays();
		if (oldCells != null)
		{
			foreach (Int3 allCell in AllCells)
			{
				int index = baseShape.GetIndex(allCell);
				int index2 = grid3Shape.GetIndex(allCell - zero);
				if (index2 != -1)
				{
					cellObjects[index] = oldCellObjects[index2];
					cells[index] = oldCells[index2];
					links[index] = oldLinks[index2];
					isGlass[index] = oldIsGlass[index2];
					Direction[] allDirections = AllDirections;
					foreach (Direction direction in allDirections)
					{
						SetFace(index, direction, oldFaces[(int)(index2 * 6 + direction)]);
					}
				}
			}
		}
		cellOffset -= zero;
		RecalculateFlowData();
		if (this.onBaseResize != null)
		{
			this.onBaseResize(this, zero);
		}
		return zero;
	}

	public void CopyFrom(Base sourceBase, Int3.Bounds sourceRange, Int3 offset)
	{
		Int3.Bounds region = sourceRange;
		region.Move(offset);
		Int3 @int = EnsureSize(region);
		anchor += @int;
		foreach (Int3 item in region)
		{
			int index = sourceBase.baseShape.GetIndex(item - offset);
			Int3 int2 = item + @int;
			int index2 = baseShape.GetIndex(int2);
			if (index == -1 || index2 == -1)
			{
				continue;
			}
			if (sourceBase.IsCellUsed(index))
			{
				cells[index2] = sourceBase.cells[index];
				links[index2] = sourceBase.links[index];
				isGlass[index2] = sourceBase.isGlass[index];
				TouchCell(int2);
				TouchAdjacentVertical(int2);
			}
			Direction[] allDirections = AllDirections;
			foreach (Direction direction in allDirections)
			{
				if (sourceBase.IsFaceUsed(index, direction))
				{
					TouchCell(int2);
					TouchAdjacentVertical(int2);
					int faceIndex = GetFaceIndex(index, direction);
					int faceIndex2 = GetFaceIndex(index2, direction);
					faces[faceIndex2] = sourceBase.faces[faceIndex];
					if (faces[faceIndex2] == FaceType.WaterPark)
					{
						TouchBottomWaterParkCell(int2);
					}
				}
			}
		}
		StorePreviousFaces();
		FixCorridorLinks();
		RecalculateFlowData();
		RebuildGeometry();
		if (!isGhost)
		{
			for (int j = 0; j < ghosts.Count; j++)
			{
				ghosts[j].RecalculateTargetOffset();
			}
		}
	}

	private void Start()
	{
		DevConsole.RegisterConsoleCommand(this, "rebuildbase");
		DevConsole.RegisterConsoleCommand(this, "autobuildbase");
		timeLastPowerSample = Time.time;
	}

	private void OnConsoleCommand_rebuildbase(NotificationCenter.Notification n)
	{
		touchedCells.Clear();
		RebuildGeometry();
	}

	private void OnConsoleCommand_autobuildbase(NotificationCenter.Notification n)
	{
		autobuilding = !autobuilding;
		ErrorMessage.AddDebug("autobuilding = " + autobuilding);
	}

	private void AutoBuild()
	{
		if (autoBuildBase == null)
		{
			GameObject basePrefab = BaseGhost.GetBasePrefab();
			if ((bool)basePrefab)
			{
				autoBuildBaseGo = UnityEngine.Object.Instantiate(basePrefab);
				autoBuildBase = autoBuildBaseGo.GetComponent<Base>();
				autoBuildBase.isGhost = true;
				autoBuildBase.CopyFrom(this, Bounds, Int3.zero);
				autoBuildBaseGo.SetActive(value: false);
			}
		}
		if (timeLastAutoBuild + 0.2f < Time.time)
		{
			timeLastAutoBuild = Time.time;
			CopyFrom(autoBuildBase, autoBuildBase.Bounds, Bounds.maxs);
			Debug.Log("base size: " + GetSize());
		}
	}

	private void TouchCell(Int3 cell)
	{
		if (!isGhost)
		{
			cell = NormalizeCell(cell);
			if (!touchedCells.Contains(cell))
			{
				touchedCells.Add(cell);
			}
		}
	}

	private void TouchAdjacentVertical(Int3 cell)
	{
		cell = NormalizeCell(cell);
		CellType cell2 = GetCell(cell);
		if (cell2 != CellType.Room && cell2 - 17 > CellType.Room)
		{
			return;
		}
		Direction[] verticalDirections = VerticalDirections;
		foreach (Direction direction in verticalDirections)
		{
			Int3 adjacent = GetAdjacent(cell, direction);
			if (GetCell(adjacent) == cell2)
			{
				TouchCell(adjacent);
			}
		}
	}

	private void TouchBottomWaterParkCell(Int3 cell)
	{
		if (GetCell(NormalizeCell(cell)) == CellType.Room)
		{
			cell = GetCentralRoomCell(cell);
			Face face = new Face(cell, Direction.Below);
			Int3 cell2;
			do
			{
				cell2 = face.cell;
				face.cell.y--;
			}
			while (GetFace(face) == FaceType.WaterPark);
			if (cell2 != cell)
			{
				TouchCell(cell2);
			}
		}
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		UpgradeDeserializedFaces();
		StorePreviousFaces();
		isPlaced = true;
	}

	private void StorePreviousFaces()
	{
		UWE.Utils.CopyArray(faces, ref previousfaces);
	}

	private static IEnumerator RegisterPiecesAsync()
	{
		if (pieces != null)
		{
			yield break;
		}
		pieces = new PieceDef[272];
		Int3 extraCells = new Int3(2, 0, 2);
		PieceData[] piecesToLoad = new PieceData[257]
		{
			new PieceData(Piece.Foundation, "BaseFoundationPiece", new Int3(1, 0, 1)),
			new PieceData(Piece.CorridorCap, "BaseCorridorCap"),
			new PieceData(Piece.CorridorWindow, "BaseCorridorWindow"),
			new PieceData(Piece.CorridorHatch, "BaseCorridorHatch"),
			new PieceData(Piece.CorridorBulkhead, "BaseCorridorBulkhead"),
			new PieceData(Piece.CorridorIShapeGlass, "BaseCorridorIShapeGlass"),
			new PieceData(Piece.CorridorLShapeGlass, "BaseCorridorLShapeGlass"),
			new PieceData(Piece.CorridorIShape, "BaseCorridorIShape"),
			new PieceData(Piece.CorridorLShape, "BaseCorridorLShape"),
			new PieceData(Piece.CorridorTShape, "BaseCorridorTShape"),
			new PieceData(Piece.CorridorXShape, "BaseCorridorXShape"),
			new PieceData(Piece.CorridorIShapeGlassSupport, "BaseCorridorIShapeSupport"),
			new PieceData(Piece.CorridorLShapeGlassSupport, "BaseCorridorLShapeSupport"),
			new PieceData(Piece.CorridorIShapeSupport, "BaseCorridorIShapeSupport"),
			new PieceData(Piece.CorridorLShapeSupport, "BaseCorridorLShapeSupport"),
			new PieceData(Piece.CorridorTShapeSupport, "BaseCorridorTShapeSupport"),
			new PieceData(Piece.CorridorIShapeGlassAdjustableSupport, "BaseCorridorIShapeAdjustableSupport"),
			new PieceData(Piece.CorridorLShapeGlassAdjustableSupport, "BaseCorridorLShapeAdjustableSupport"),
			new PieceData(Piece.CorridorIShapeAdjustableSupport, "BaseCorridorIShapeAdjustableSupport"),
			new PieceData(Piece.CorridorLShapeAdjustableSupport, "BaseCorridorLShapeAdjustableSupport"),
			new PieceData(Piece.CorridorTShapeAdjustableSupport, "BaseCorridorTShapeAdjustableSupport"),
			new PieceData(Piece.CorridorXShapeAdjustableSupport, "BaseCorridorXShapeAdjustableSupport"),
			new PieceData(Piece.CorridorIShapeCoverSide, "BaseCorridorIShapeCoverSide"),
			new PieceData(Piece.CorridorIShapeWindowSide, "BaseCorridorIShapeWindowSide"),
			new PieceData(Piece.CorridorIShapeWindowTop, "BaseCorridorIShapeWindowTop"),
			new PieceData(Piece.CorridorIShapeWindowBottom, "BaseCorridorIShapeWindowBottom"),
			new PieceData(Piece.CorridorIShapeReinforcementSide, "BaseCorridorIShapeReinforcementSide"),
			new PieceData(Piece.CorridorIShapeHatchSide, "BaseCorridorIShapeHatchSide"),
			new PieceData(Piece.CorridorIShapeHatchTop, "BaseCorridorIShapeHatchTop"),
			new PieceData(Piece.CorridorIShapeHatchBottom, "BaseCorridorIShapeHatchBottom"),
			new PieceData(Piece.CorridorIShapePlanterSide, "BaseCorridorIShapeInteriorPlanterSide"),
			new PieceData(Piece.CorridorIShapeLadderTop, "BaseCorridorLadderTop"),
			new PieceData(Piece.CorridorIShapeLadderBottom, "BaseCorridorLadderBottom"),
			new PieceData(Piece.CorridorTShapeWindowTop, "BaseCorridorTShapeWindowTop"),
			new PieceData(Piece.CorridorTShapeWindowBottom, "BaseCorridorTShapeWindowBottom"),
			new PieceData(Piece.CorridorTShapeHatchTop, "BaseCorridorTShapeHatchTop"),
			new PieceData(Piece.CorridorTShapeHatchBottom, "BaseCorridorTShapeHatchBottom"),
			new PieceData(Piece.CorridorTShapeLadderTop, "BaseCorridorLadderTop"),
			new PieceData(Piece.CorridorTShapeLadderBottom, "BaseCorridorLadderBottom"),
			new PieceData(Piece.CorridorXShapeWindowTop, "BaseCorridorXShapeWindowTop"),
			new PieceData(Piece.CorridorXShapeWindowBottom, "BaseCorridorXShapeWindowBottom"),
			new PieceData(Piece.CorridorXShapeHatchTop, "BaseCorridorXShapeHatchTop"),
			new PieceData(Piece.CorridorXShapeHatchBottom, "BaseCorridorXShapeHatchBottom"),
			new PieceData(Piece.CorridorXShapeLadderTop, "BaseCorridorLadderTop"),
			new PieceData(Piece.CorridorXShapeLadderBottom, "BaseCorridorLadderBottom"),
			new PieceData(Piece.CorridorCoverIShapeBottomExtClosed, "BaseCorridorCoverIShapeBottomExtClosed"),
			new PieceData(Piece.CorridorCoverIShapeBottomExtOpened, "BaseCorridorCoverIShapeBottomExtOpened"),
			new PieceData(Piece.CorridorCoverIShapeBottomIntClosed, "BaseCorridorCoverIShapeBottomIntClosed"),
			new PieceData(Piece.CorridorCoverIShapeBottomIntOpened, "BaseCorridorCoverIShapeBottomIntOpened"),
			new PieceData(Piece.CorridorCoverIShapeTopExtClosed, "BaseCorridorCoverIShapeTopExtClosed"),
			new PieceData(Piece.CorridorCoverIShapeTopExtOpened, "BaseCorridorCoverIShapeTopExtOpened"),
			new PieceData(Piece.CorridorCoverIShapeTopIntClosed, "BaseCorridorCoverIShapeTopIntClosed"),
			new PieceData(Piece.CorridorCoverIShapeTopIntOpened, "BaseCorridorCoverIShapeTopIntOpened"),
			new PieceData(Piece.CorridorCoverTShapeBottomExtClosed, "BaseCorridorCoverTShapeBottomExtClosed"),
			new PieceData(Piece.CorridorCoverTShapeBottomExtOpened, "BaseCorridorCoverTShapeBottomExtOpened"),
			new PieceData(Piece.CorridorCoverTShapeBottomIntClosed, "BaseCorridorCoverTShapeBottomIntClosed"),
			new PieceData(Piece.CorridorCoverTShapeBottomIntOpened, "BaseCorridorCoverTShapeBottomIntOpened"),
			new PieceData(Piece.CorridorCoverTShapeTopExtClosed, "BaseCorridorCoverTShapeTopExtClosed"),
			new PieceData(Piece.CorridorCoverTShapeTopExtOpened, "BaseCorridorCoverTShapeTopExtOpened"),
			new PieceData(Piece.CorridorCoverTShapeTopIntClosed, "BaseCorridorCoverTShapeTopIntClosed"),
			new PieceData(Piece.CorridorCoverTShapeTopIntOpened, "BaseCorridorCoverTShapeTopIntOpened"),
			new PieceData(Piece.CorridorCoverXShapeBottomExtClosed, "BaseCorridorCoverXShapeBottomExtClosed"),
			new PieceData(Piece.CorridorCoverXShapeBottomExtOpened, "BaseCorridorCoverXShapeBottomExtOpened"),
			new PieceData(Piece.CorridorCoverXShapeBottomIntClosed, "BaseCorridorCoverXShapeBottomIntClosed"),
			new PieceData(Piece.CorridorCoverXShapeBottomIntOpened, "BaseCorridorCoverXShapeBottomIntOpened"),
			new PieceData(Piece.CorridorCoverXShapeTopExtClosed, "BaseCorridorCoverXShapeTopExtClosed"),
			new PieceData(Piece.CorridorCoverXShapeTopExtOpened, "BaseCorridorCoverXShapeTopExtOpened"),
			new PieceData(Piece.CorridorCoverXShapeTopIntClosed, "BaseCorridorCoverXShapeTopIntClosed"),
			new PieceData(Piece.CorridorCoverXShapeTopIntOpened, "BaseCorridorCoverXShapeTopIntOpened"),
			new PieceData(Piece.ConnectorTube, "BaseConnectorTube"),
			new PieceData(Piece.ConnectorTubeWindow, "BaseConnectorTubeWindow"),
			new PieceData(Piece.ConnectorCap, "BaseConnectorCap"),
			new PieceData(Piece.ConnectorLadder, "BaseConnectorLadder"),
			new PieceData(Piece.Room, "BaseRoom", extraCells),
			new PieceData(Piece.RoomExteriorBottom, "BaseRoomExteriorBottom", extraCells),
			new PieceData(Piece.RoomExteriorFoundationBottom, "BaseRoomExteriorFoundationBottom", extraCells),
			new PieceData(Piece.RoomExteriorTop, "BaseRoomExteriorTop", extraCells),
			new PieceData(Piece.RoomExteriorTopGlass, "BaseRoomExteriorTopGlass", extraCells),
			new PieceData(Piece.RoomAdjustableSupport, "BaseRoomAdjustableSupport", extraCells),
			new PieceData(Piece.RoomInteriorBottom, "BaseRoomInteriorBottom", extraCells),
			new PieceData(Piece.RoomInteriorTop, "BaseRoomInteriorTop", extraCells),
			new PieceData(Piece.RoomInteriorTopGlass, "BaseRoomInteriorTopGlass", extraCells),
			new PieceData(Piece.RoomInteriorBottomHole, "BaseRoomInteriorBottomHole", extraCells),
			new PieceData(Piece.RoomInteriorTopHole, "BaseRoomInteriorTopHole", extraCells),
			new PieceData(Piece.RoomCorridorConnector, "BaseRoomCorridorConnector", extraCells),
			new PieceData(Piece.RoomCoverSide, "BaseRoomCoverSide", extraCells),
			new PieceData(Piece.RoomCoverSideVariant, "BaseRoomCoverSideVariant", extraCells),
			new PieceData(Piece.RoomReinforcementSide, "BaseRoomReinforcementSide", extraCells),
			new PieceData(Piece.RoomWindowSide, "BaseRoomWindowSide", extraCells),
			new PieceData(Piece.RoomPlanterSide, "BaseRoomPlanterSide", extraCells),
			new PieceData(Piece.RoomFiltrationMachine, "BaseRoomFiltrationMachine", extraCells),
			new PieceData(Piece.RoomCoverBottom, "BaseRoomCoverBottom", extraCells),
			new PieceData(Piece.RoomCoverTop, "BaseRoomCoverTop", extraCells),
			new PieceData(Piece.RoomLadderBottom, "BaseRoomLadderBottom", extraCells),
			new PieceData(Piece.RoomLadderTop, "BaseRoomLadderTop", extraCells),
			new PieceData(Piece.RoomHatch, "BaseRoomHatch", extraCells),
			new PieceData(Piece.RoomBioReactor, "BaseRoomBioReactor", extraCells),
			new PieceData(Piece.RoomBioReactorUnderDome, "BaseRoomBioReactorUnderGlassDome", extraCells),
			new PieceData(Piece.RoomNuclearReactor, "BaseRoomNuclearReactor", extraCells),
			new PieceData(Piece.RoomNuclearReactorUnderDome, "BaseRoomNuclearReactorUnderGlassDome", extraCells),
			new PieceData(Piece.RoomWaterParkTop, "BaseWaterParkTop"),
			new PieceData(Piece.RoomWaterParkBottom, "BaseWaterParkBottom"),
			new PieceData(Piece.RoomWaterParkHatch, "BaseWaterParkHatch"),
			new PieceData(Piece.RoomWaterParkSide, "BaseWaterParkSide"),
			new PieceData(Piece.RoomWaterParkCeilingGlass, "BaseWaterParkCeilingGlass"),
			new PieceData(Piece.RoomWaterParkCeilingGlassDome, "BaseWaterParkCeilingGlassDome"),
			new PieceData(Piece.RoomWaterParkCeilingMiddle, "BaseWaterParkCeilingMiddle"),
			new PieceData(Piece.RoomWaterParkCeilingTop, "BaseWaterParkCeilingTop"),
			new PieceData(Piece.RoomWaterParkFloorBottom, "BaseWaterParkFloorBottom"),
			new PieceData(Piece.RoomWaterParkFloorMiddle, "BaseWaterParkFloorMiddle"),
			new PieceData(Piece.Moonpool, "BaseMoonpool"),
			new PieceData(Piece.MoonpoolAdjustableSupport, "BaseMoonpoolAdjustableSupport"),
			new PieceData(Piece.MoonpoolCoverSide, "BaseMoonpoolCoverSide"),
			new PieceData(Piece.MoonpoolCoverSideShort, "BaseMoonpoolCoverSideShort"),
			new PieceData(Piece.MoonpoolReinforcementSide, "BaseMoonpoolReinforcementSide"),
			new PieceData(Piece.MoonpoolReinforcementSideShort, "BaseMoonpoolReinforcementSideShort"),
			new PieceData(Piece.MoonpoolWindowSide, "BaseMoonpoolWindowSide"),
			new PieceData(Piece.MoonpoolWindowSideShort, "BaseMoonpoolWindowSideShort"),
			new PieceData(Piece.MoonpoolUpgradeConsole, "BaseMoonpoolUpgradeConsole"),
			new PieceData(Piece.MoonpoolUpgradeConsoleShort, "BaseMoonpoolUpgradeConsoleShort"),
			new PieceData(Piece.MoonpoolHatch, "BaseMoonpoolHatch"),
			new PieceData(Piece.MoonpoolHatchShort, "BaseMoonpoolHatchShort"),
			new PieceData(Piece.MoonpoolPlanterSide, "BaseMoonpoolPlanterSide"),
			new PieceData(Piece.MoonpoolPlanterSideShort, "BaseMoonpoolPlanterSideShort"),
			new PieceData(Piece.MoonpoolCorridorConnector, "BaseMoonpoolCorridorConnector"),
			new PieceData(Piece.MoonpoolCorridorConnectorShort, "BaseMoonpoolCorridorConnectorShort"),
			new PieceData(Piece.Observatory, "BaseObservatory"),
			new PieceData(Piece.ObservatoryCorridorConnector, "BaseObservatoryCorridorConnector"),
			new PieceData(Piece.ObservatoryCoverSide, "BaseObservatoryCoverSide"),
			new PieceData(Piece.ObservatoryHatch, "BaseObservatoryHatch"),
			new PieceData(Piece.MapRoom, "BaseMapRoom", extraCells),
			new PieceData(Piece.MapRoomCorridorConnector, "BaseMapRoomCorridorConnector", extraCells),
			new PieceData(Piece.MapRoomCoverSide, "BaseMapRoomCoverSide", extraCells),
			new PieceData(Piece.MapRoomHatch, "BaseMapRoomHatch", extraCells),
			new PieceData(Piece.MapRoomWindowSide, "BaseMapRoomWindowSide", extraCells),
			new PieceData(Piece.MapRoomPlanterSide, "BaseMapRoomPlanterSide", extraCells),
			new PieceData(Piece.MapRoomReinforcementSide, "BaseMapRoomReinforcementSide", extraCells),
			new PieceData(Piece.LargeRoom, "BaseLargeRoom"),
			new PieceData(Piece.LargeRoomExteriorTop, "BaseLargeRoomExteriorTop"),
			new PieceData(Piece.LargeRoomExteriorTopGlass, "BaseLargeRoomExteriorTopGlass"),
			new PieceData(Piece.LargeRoomExteriorBottom, "BaseLargeRoomExteriorBottom"),
			new PieceData(Piece.LargeRoomExteriorFoundationBottom, "BaseLargeRoomExteriorFoundationBottom"),
			new PieceData(Piece.LargeRoomInteriorTop, "BaseLargeRoomInteriorTop"),
			new PieceData(Piece.LargeRoomInteriorTopGlass, "BaseLargeRoomInteriorTopGlass"),
			new PieceData(Piece.LargeRoomInteriorTopHole1, "BaseLargeRoomInteriorTopHole1"),
			new PieceData(Piece.LargeRoomInteriorTopHole2, "BaseLargeRoomInteriorTopHole2"),
			new PieceData(Piece.LargeRoomInteriorTopHole3, "BaseLargeRoomInteriorTopHole3"),
			new PieceData(Piece.LargeRoomInteriorTopHole4, "BaseLargeRoomInteriorTopHole4"),
			new PieceData(Piece.LargeRoomInteriorBottom, "BaseLargeRoomInteriorBottom"),
			new PieceData(Piece.LargeRoomInteriorBottomHole1, "BaseLargeRoomInteriorBottomHole1"),
			new PieceData(Piece.LargeRoomInteriorBottomHole2, "BaseLargeRoomInteriorBottomHole2"),
			new PieceData(Piece.LargeRoomInteriorBottomHole3, "BaseLargeRoomInteriorBottomHole3"),
			new PieceData(Piece.LargeRoomInteriorBottomHole4, "BaseLargeRoomInteriorBottomHole4"),
			new PieceData(Piece.LargeRoomAdjustableSupport, "BaseLargeRoomAdjustableSupport"),
			new PieceData(Piece.LargeRoomCoverSide, "BaseLargeRoomCoverSide"),
			new PieceData(Piece.LargeRoomCoverSideShort, "BaseLargeRoomCoverSideShort"),
			new PieceData(Piece.LargeRoomReinforcementSide, "BaseLargeRoomReinforcementSide"),
			new PieceData(Piece.LargeRoomReinforcementSideShort, "BaseLargeRoomReinforcementSideShort"),
			new PieceData(Piece.LargeRoomWindowSide, "BaseLargeRoomWindowSide"),
			new PieceData(Piece.LargeRoomWindowSideShort, "BaseLargeRoomWindowSideShort"),
			new PieceData(Piece.LargeRoomHatch, "BaseLargeRoomHatch"),
			new PieceData(Piece.LargeRoomHatchShort, "BaseLargeRoomHatchShort"),
			new PieceData(Piece.LargeRoomCorridorConnector, "BaseLargeRoomCorridorConnector"),
			new PieceData(Piece.LargeRoomCorridorConnectorShort, "BaseLargeRoomCorridorConnectorShort"),
			new PieceData(Piece.LargeRoomPlanterSide, "BaseLargeRoomPlanterSide"),
			new PieceData(Piece.LargeRoomPlanterSideShort, "BaseLargeRoomPlanterSideShort"),
			new PieceData(Piece.LargeRoomFiltrationMachine, "BaseLargeRoomFiltrationMachine"),
			new PieceData(Piece.LargeRoomFiltrationMachineShort, "BaseLargeRoomFiltrationMachineShort"),
			new PieceData(Piece.LargeRoomLadderBottom, "BaseLargeRoomLadderBottom"),
			new PieceData(Piece.LargeRoomLadderTop, "BaseLargeRoomLadderTop"),
			new PieceData(Piece.LargeRoomCoverTop, "BaseLargeRoomCoverTop"),
			new PieceData(Piece.LargeRoomCoverBottom, "BaseLargeRoomCoverBottom"),
			new PieceData(Piece.LargeRoomWaterParkWalls, "BaseLargeWaterParkWalls"),
			new PieceData(Piece.LargeRoomWaterParkCeilingGlass, "BaseLargeWaterParkCeilingGlass"),
			new PieceData(Piece.LargeRoomWaterParkCeilingGlassDome, "BaseLargeWaterParkCeilingGlassDome"),
			new PieceData(Piece.LargeRoomWaterParkCeilingMiddle, "BaseLargeWaterParkCeilingMiddle"),
			new PieceData(Piece.LargeRoomWaterParkCeilingTop, "BaseLargeWaterParkCeilingTop"),
			new PieceData(Piece.LargeRoomWaterParkCeilingTopMiddle, "BaseLargeWaterParkCeilingTopMiddle"),
			new PieceData(Piece.LargeRoomWaterParkCeilingMiddleTop, "BaseLargeWaterParkCeilingMiddleTop"),
			new PieceData(Piece.LargeRoomWaterParkCeilingMiddleMiddle, "BaseLargeWaterParkCeilingMiddleMiddle"),
			new PieceData(Piece.LargeRoomWaterParkFloorBottom, "BaseLargeWaterParkFloorBottom"),
			new PieceData(Piece.LargeRoomWaterParkFloorMiddle, "BaseLargeWaterParkFloorMiddle"),
			new PieceData(Piece.LargeRoomWaterParkFloorBottomMiddle, "BaseLargeWaterParkFloorBottomMiddle"),
			new PieceData(Piece.LargeRoomWaterParkFloorMiddleBottom, "BaseLargeWaterParkFloorMiddleBottom"),
			new PieceData(Piece.LargeRoomWaterParkFloorMiddleMiddle, "BaseLargeWaterParkFloorMiddleMiddle"),
			new PieceData(Piece.LargeRoomWaterParkHatch, "BaseLargeRoomWaterParkHatch"),
			new PieceData(Piece.LargeRoomWaterParkHatchShort, "BaseLargeRoomWaterParkHatchShort"),
			new PieceData(Piece.LargeRoomWaterParkSide, "BaseWaterParkSide"),
			new PieceData(Piece.PartitionDoor, "BasePartitionDoor"),
			new PieceData(Piece.PartitionCentralIHalf, "BasePartitionCentralIHalf"),
			new PieceData(Piece.PartitionCentralIHalf90, "BasePartitionCentralIHalf90"),
			new PieceData(Piece.PartitionCentralIHalf180, "BasePartitionCentralIHalf180"),
			new PieceData(Piece.PartitionCentralIHalf270, "BasePartitionCentralIHalf270"),
			new PieceData(Piece.PartitionCentralI, "BasePartitionCentralI"),
			new PieceData(Piece.PartitionCentralI90, "BasePartitionCentralI90"),
			new PieceData(Piece.PartitionDoorwayCentralI, "BasePartitionDoorwayCentralI"),
			new PieceData(Piece.PartitionDoorwayCentralI90, "BasePartitionDoorwayCentralI90"),
			new PieceData(Piece.PartitionCentralL, "BasePartitionCentralL"),
			new PieceData(Piece.PartitionCentralL90, "BasePartitionCentralL90"),
			new PieceData(Piece.PartitionCentralL180, "BasePartitionCentralL180"),
			new PieceData(Piece.PartitionCentralL270, "BasePartitionCentralL270"),
			new PieceData(Piece.PartitionCentralT, "BasePartitionCentralT"),
			new PieceData(Piece.PartitionCentralT90, "BasePartitionCentralT90"),
			new PieceData(Piece.PartitionCentralT180, "BasePartitionCentralT180"),
			new PieceData(Piece.PartitionCentralT270, "BasePartitionCentralT270"),
			new PieceData(Piece.PartitionCentralX, "BasePartitionCentralX"),
			new PieceData(Piece.PartitionSideIHalf, "BasePartitionSideIHalf"),
			new PieceData(Piece.PartitionSideIHalf90, "BasePartitionSideIHalf90"),
			new PieceData(Piece.PartitionSideIHalf180, "BasePartitionSideIHalf180"),
			new PieceData(Piece.PartitionSideIHalf270, "BasePartitionSideIHalf270"),
			new PieceData(Piece.PartitionSideI, "BasePartitionSideI"),
			new PieceData(Piece.PartitionSideI90, "BasePartitionSideI90"),
			new PieceData(Piece.PartitionDoorwaySideI, "BasePartitionDoorwaySideI"),
			new PieceData(Piece.PartitionDoorwaySideI90, "BasePartitionDoorwaySideI90"),
			new PieceData(Piece.PartitionSideL, "BasePartitionSideL"),
			new PieceData(Piece.PartitionSideL90, "BasePartitionSideL90"),
			new PieceData(Piece.PartitionSideL180, "BasePartitionSideL180"),
			new PieceData(Piece.PartitionSideL270, "BasePartitionSideL270"),
			new PieceData(Piece.PartitionSideT, "BasePartitionSideT"),
			new PieceData(Piece.PartitionSideT90, "BasePartitionSideT90"),
			new PieceData(Piece.PartitionSideT180, "BasePartitionSideT180"),
			new PieceData(Piece.PartitionSideT270, "BasePartitionSideT270"),
			new PieceData(Piece.PartitionSideX, "BasePartitionSideX"),
			new PieceData(Piece.PartitionSideShortIHalf, "BasePartitionSideShortIHalf"),
			new PieceData(Piece.PartitionSideShortIHalf90, "BasePartitionSideShortIHalf90"),
			new PieceData(Piece.PartitionSideShortIHalf180, "BasePartitionSideShortIHalf180"),
			new PieceData(Piece.PartitionSideShortIHalf270, "BasePartitionSideShortIHalf270"),
			new PieceData(Piece.PartitionSideShortI, "BasePartitionSideShortI"),
			new PieceData(Piece.PartitionSideShortI90, "BasePartitionSideShortI90"),
			new PieceData(Piece.PartitionDoorwaySideShortI, "BasePartitionDoorwaySideShortI"),
			new PieceData(Piece.PartitionDoorwaySideShortI90, "BasePartitionDoorwaySideShortI90"),
			new PieceData(Piece.PartitionSideShortL, "BasePartitionSideShortL"),
			new PieceData(Piece.PartitionSideShortL90, "BasePartitionSideShortL90"),
			new PieceData(Piece.PartitionSideShortL180, "BasePartitionSideShortL180"),
			new PieceData(Piece.PartitionSideShortL270, "BasePartitionSideShortL270"),
			new PieceData(Piece.PartitionSideShortT, "BasePartitionSideShortT"),
			new PieceData(Piece.PartitionSideShortT90, "BasePartitionSideShortT90"),
			new PieceData(Piece.PartitionSideShortT180, "BasePartitionSideShortT180"),
			new PieceData(Piece.PartitionSideShortT270, "BasePartitionSideShortT270"),
			new PieceData(Piece.PartitionSideShortX, "BasePartitionSideShortX"),
			new PieceData(Piece.PartitionCornerIHalf, "BasePartitionCornerIHalf"),
			new PieceData(Piece.PartitionCornerIHalf90, "BasePartitionCornerIHalf90"),
			new PieceData(Piece.PartitionCornerIHalf180, "BasePartitionCornerIHalf180"),
			new PieceData(Piece.PartitionCornerIHalf270, "BasePartitionCornerIHalf270"),
			new PieceData(Piece.PartitionCornerI, "BasePartitionCornerI"),
			new PieceData(Piece.PartitionCornerI90, "BasePartitionCornerI90"),
			new PieceData(Piece.PartitionDoorwayCornerI, "BasePartitionDoorwayCornerI"),
			new PieceData(Piece.PartitionDoorwayCornerI90, "BasePartitionDoorwayCornerI90"),
			new PieceData(Piece.PartitionCornerL, "BasePartitionCornerL"),
			new PieceData(Piece.PartitionCornerL90, "BasePartitionCornerL90"),
			new PieceData(Piece.PartitionCornerL180, "BasePartitionCornerL180"),
			new PieceData(Piece.PartitionCornerL270, "BasePartitionCornerL270"),
			new PieceData(Piece.PartitionCornerT, "BasePartitionCornerT"),
			new PieceData(Piece.PartitionCornerT90, "BasePartitionCornerT90"),
			new PieceData(Piece.PartitionCornerT180, "BasePartitionCornerT180"),
			new PieceData(Piece.PartitionCornerT270, "BasePartitionCornerT270"),
			new PieceData(Piece.PartitionCornerX, "BasePartitionCornerX")
		};
		int nextPieceToLoad = 0;
		List<PieceData> activeLoads = new List<PieceData>(32);
		while (nextPieceToLoad < piecesToLoad.Length || activeLoads.Count > 0)
		{
			for (int i = activeLoads.Count - 1; i >= 0; i--)
			{
				PieceData pieceData = activeLoads[i];
				yield return pieceData.request;
				if (pieceData.request.IsDone)
				{
					activeLoads.RemoveAt(i);
					if (pieceData.request.Status == AsyncOperationStatus.Succeeded && pieceData.request.Result != null)
					{
						GameObject result = pieceData.request.Result;
						result.SetActive(value: false);
						pieces[(int)pieceData.piece] = new PieceDef(result, pieceData.extraCells, Quaternion.identity);
					}
					else
					{
						Debug.LogErrorFormat("Failed to load base piece '{0}'", pieceData.name);
					}
				}
			}
			while (activeLoads.Count < 32 && nextPieceToLoad < piecesToLoad.Length)
			{
				PieceData item = piecesToLoad[nextPieceToLoad++];
				string key = $"Assets/Prefabs/Base/GeneratorPieces/{item.name}.prefab";
				item.request = AddressablesUtility.LoadAsync<GameObject>(key);
				activeLoads.Add(item);
			}
			yield return null;
		}
	}

	private static void RegisterCorridors()
	{
		if (corridors == null)
		{
			CorridorDef corridorDef = new CorridorDef(Piece.CorridorIShapeGlass, Piece.CorridorIShapeGlassSupport, Piece.CorridorIShapeGlassAdjustableSupport, TechType.BaseCorridorGlassI);
			corridorDef.SetFace(Direction.North, FaceType.Solid, Piece.CorridorCap, Vector3.zero);
			corridorDef.SetFace(Direction.South, FaceType.Solid, Piece.CorridorCap, new Vector3(0f, 180f, 0f));
			corridorDef.SetFace(Direction.North, FaceType.Hatch, Piece.CorridorHatch, Vector3.zero);
			corridorDef.SetFace(Direction.South, FaceType.Hatch, Piece.CorridorHatch, new Vector3(0f, 180f, 0f));
			corridorDef.SetFace(Direction.North, FaceType.Window, Piece.CorridorWindow, Vector3.zero);
			corridorDef.SetFace(Direction.South, FaceType.Window, Piece.CorridorWindow, new Vector3(0f, 180f, 0f));
			corridorDef.SetFace(Direction.Below, FaceType.Solid, Piece.CorridorCoverIShapeBottomExtClosed, Vector3.zero);
			corridorDef.SetFace(Direction.Below, FaceType.Hole, Piece.CorridorCoverIShapeBottomExtOpened, Vector3.zero);
			corridorDef.SetFace(Direction.Below, FaceType.Ladder, Piece.CorridorIShapeLadderBottom, Vector3.zero);
			corridorDef.SetFace(Direction.Below, FaceType.Hatch, Piece.CorridorIShapeHatchBottom, Vector3.zero);
			corridorDef.SetFace(Direction.North, FaceType.BulkheadClosed, Piece.CorridorBulkhead, Vector3.zero);
			corridorDef.SetFace(Direction.North, FaceType.BulkheadOpened, Piece.CorridorBulkhead, Vector3.zero);
			corridorDef.SetFace(Direction.South, FaceType.BulkheadClosed, Piece.CorridorBulkhead, new Vector3(0f, 180f, 0f));
			corridorDef.SetFace(Direction.South, FaceType.BulkheadOpened, Piece.CorridorBulkhead, new Vector3(0f, 180f, 0f));
			CorridorDef corridorDef2 = new CorridorDef(Piece.CorridorLShapeGlass, Piece.CorridorLShapeGlassSupport, Piece.CorridorLShapeGlassAdjustableSupport, TechType.BaseCorridorGlassL);
			corridorDef2.SetFace(Direction.East, FaceType.Solid, Piece.CorridorCap, new Vector3(0f, 90f, 0f));
			corridorDef2.SetFace(Direction.North, FaceType.Solid, Piece.CorridorCap, Vector3.zero);
			corridorDef2.SetFace(Direction.East, FaceType.Hatch, Piece.CorridorHatch, new Vector3(0f, 90f, 0f));
			corridorDef2.SetFace(Direction.North, FaceType.Hatch, Piece.CorridorHatch, Vector3.zero);
			corridorDef2.SetFace(Direction.East, FaceType.Window, Piece.CorridorWindow, new Vector3(0f, 90f, 0f));
			corridorDef2.SetFace(Direction.North, FaceType.Window, Piece.CorridorWindow, Vector3.zero);
			corridorDef2.SetFace(Direction.East, FaceType.BulkheadClosed, Piece.CorridorBulkhead, new Vector3(0f, 90f, 0f));
			corridorDef2.SetFace(Direction.East, FaceType.BulkheadOpened, Piece.CorridorBulkhead, new Vector3(0f, 90f, 0f));
			corridorDef2.SetFace(Direction.North, FaceType.BulkheadClosed, Piece.CorridorBulkhead, Vector3.zero);
			corridorDef2.SetFace(Direction.North, FaceType.BulkheadOpened, Piece.CorridorBulkhead, Vector3.zero);
			CorridorDef corridorDef3 = new CorridorDef(Piece.CorridorIShape, Piece.CorridorIShapeSupport, Piece.CorridorIShapeAdjustableSupport, TechType.BaseCorridorI);
			corridorDef3.SetFace(Direction.East, FaceType.Solid, Piece.CorridorIShapeCoverSide, Vector3.zero);
			corridorDef3.SetFace(Direction.West, FaceType.Solid, Piece.CorridorIShapeCoverSide, new Vector3(0f, 180f, 0f));
			corridorDef3.SetFace(Direction.North, FaceType.Solid, Piece.CorridorCap, Vector3.zero);
			corridorDef3.SetFace(Direction.South, FaceType.Solid, Piece.CorridorCap, new Vector3(0f, 180f, 0f));
			corridorDef3.SetFace(Direction.Above, FaceType.Solid, Piece.CorridorCoverIShapeTopExtClosed, Vector3.zero);
			corridorDef3.SetFace(Direction.Below, FaceType.Solid, Piece.CorridorCoverIShapeBottomExtClosed, Vector3.zero);
			corridorDef3.SetFace(Direction.Above, FaceType.Hole, Piece.CorridorCoverIShapeTopExtOpened, Vector3.zero);
			corridorDef3.SetFace(Direction.Below, FaceType.Hole, Piece.CorridorCoverIShapeBottomExtOpened, Vector3.zero);
			corridorDef3.SetFace(Direction.Above, FaceType.Ladder, Piece.CorridorIShapeLadderTop, Vector3.zero);
			corridorDef3.SetFace(Direction.Below, FaceType.Ladder, Piece.CorridorIShapeLadderBottom, Vector3.zero);
			corridorDef3.SetFace(Direction.East, FaceType.Window, Piece.CorridorIShapeWindowSide, Vector3.zero);
			corridorDef3.SetFace(Direction.West, FaceType.Window, Piece.CorridorIShapeWindowSide, new Vector3(0f, 180f, 0f));
			corridorDef3.SetFace(Direction.North, FaceType.Window, Piece.CorridorWindow, Vector3.zero);
			corridorDef3.SetFace(Direction.South, FaceType.Window, Piece.CorridorWindow, new Vector3(0f, 180f, 0f));
			corridorDef3.SetFace(Direction.Above, FaceType.Window, Piece.CorridorIShapeWindowTop, Vector3.zero);
			corridorDef3.SetFace(Direction.North, FaceType.BulkheadClosed, Piece.CorridorBulkhead, Vector3.zero);
			corridorDef3.SetFace(Direction.North, FaceType.BulkheadOpened, Piece.CorridorBulkhead, Vector3.zero);
			corridorDef3.SetFace(Direction.South, FaceType.BulkheadClosed, Piece.CorridorBulkhead, new Vector3(0f, 180f, 0f));
			corridorDef3.SetFace(Direction.South, FaceType.BulkheadOpened, Piece.CorridorBulkhead, new Vector3(0f, 180f, 0f));
			corridorDef3.SetFace(Direction.East, FaceType.Reinforcement, Piece.CorridorIShapeReinforcementSide, Vector3.zero);
			corridorDef3.SetFace(Direction.West, FaceType.Reinforcement, Piece.CorridorIShapeReinforcementSide, new Vector3(0f, 180f, 0f));
			corridorDef3.SetFace(Direction.East, FaceType.Hatch, Piece.CorridorIShapeHatchSide, Vector3.zero);
			corridorDef3.SetFace(Direction.West, FaceType.Hatch, Piece.CorridorIShapeHatchSide, new Vector3(0f, 180f, 0f));
			corridorDef3.SetFace(Direction.Above, FaceType.Hatch, Piece.CorridorIShapeHatchTop, Vector3.zero);
			corridorDef3.SetFace(Direction.Below, FaceType.Hatch, Piece.CorridorIShapeHatchBottom, Vector3.zero);
			corridorDef3.SetFace(Direction.North, FaceType.Hatch, Piece.CorridorHatch, Vector3.zero);
			corridorDef3.SetFace(Direction.South, FaceType.Hatch, Piece.CorridorHatch, new Vector3(0f, 180f, 0f));
			corridorDef3.SetFace(Direction.East, FaceType.Planter, Piece.CorridorIShapePlanterSide, Vector3.zero);
			corridorDef3.SetFace(Direction.West, FaceType.Planter, Piece.CorridorIShapePlanterSide, new Vector3(0f, 180f, 0f));
			CorridorDef corridorDef4 = new CorridorDef(Piece.CorridorLShape, Piece.CorridorLShapeSupport, Piece.CorridorLShapeAdjustableSupport, TechType.BaseCorridorL);
			corridorDef4.SetFace(Direction.East, FaceType.Solid, Piece.CorridorCap, new Vector3(0f, 90f, 0f));
			corridorDef4.SetFace(Direction.North, FaceType.Solid, Piece.CorridorCap, Vector3.zero);
			corridorDef4.SetFace(Direction.East, FaceType.Window, Piece.CorridorWindow, new Vector3(0f, 90f, 0f));
			corridorDef4.SetFace(Direction.North, FaceType.Window, Piece.CorridorWindow, Vector3.zero);
			corridorDef4.SetFace(Direction.East, FaceType.Hatch, Piece.CorridorHatch, new Vector3(0f, 90f, 0f));
			corridorDef4.SetFace(Direction.North, FaceType.Hatch, Piece.CorridorHatch, Vector3.zero);
			corridorDef4.SetFace(Direction.East, FaceType.BulkheadClosed, Piece.CorridorBulkhead, new Vector3(0f, 90f, 0f));
			corridorDef4.SetFace(Direction.East, FaceType.BulkheadOpened, Piece.CorridorBulkhead, new Vector3(0f, 90f, 0f));
			corridorDef4.SetFace(Direction.North, FaceType.BulkheadClosed, Piece.CorridorBulkhead, Vector3.zero);
			corridorDef4.SetFace(Direction.North, FaceType.BulkheadOpened, Piece.CorridorBulkhead, Vector3.zero);
			CorridorDef corridorDef5 = new CorridorDef(Piece.CorridorTShape, Piece.CorridorTShapeSupport, Piece.CorridorTShapeAdjustableSupport, TechType.BaseCorridorT);
			corridorDef5.SetFace(Direction.East, FaceType.Solid, Piece.CorridorCap, new Vector3(0f, 90f, 0f));
			corridorDef5.SetFace(Direction.West, FaceType.Solid, Piece.CorridorCap, new Vector3(0f, -90f, 0f));
			corridorDef5.SetFace(Direction.North, FaceType.Solid, Piece.CorridorCap, Vector3.zero);
			corridorDef5.SetFace(Direction.South, FaceType.Solid, Piece.CorridorIShapeCoverSide, new Vector3(0f, 90f, 0f));
			corridorDef5.SetFace(Direction.Above, FaceType.Solid, Piece.CorridorCoverTShapeTopExtClosed, Vector3.zero);
			corridorDef5.SetFace(Direction.Below, FaceType.Solid, Piece.CorridorCoverTShapeBottomExtClosed, Vector3.zero);
			corridorDef5.SetFace(Direction.Above, FaceType.Hole, Piece.CorridorCoverTShapeTopExtOpened, Vector3.zero);
			corridorDef5.SetFace(Direction.Below, FaceType.Hole, Piece.CorridorCoverTShapeBottomExtOpened, Vector3.zero);
			corridorDef5.SetFace(Direction.Above, FaceType.Ladder, Piece.CorridorTShapeLadderTop, Vector3.zero);
			corridorDef5.SetFace(Direction.Below, FaceType.Ladder, Piece.CorridorTShapeLadderBottom, Vector3.zero);
			corridorDef5.SetFace(Direction.South, FaceType.Window, Piece.CorridorIShapeWindowSide, new Vector3(0f, 90f, 0f));
			corridorDef5.SetFace(Direction.East, FaceType.Window, Piece.CorridorWindow, new Vector3(0f, 90f, 0f));
			corridorDef5.SetFace(Direction.West, FaceType.Window, Piece.CorridorWindow, new Vector3(0f, -90f, 0f));
			corridorDef5.SetFace(Direction.North, FaceType.Window, Piece.CorridorWindow, Vector3.zero);
			corridorDef5.SetFace(Direction.Above, FaceType.Window, Piece.CorridorTShapeWindowTop, Vector3.zero);
			corridorDef5.SetFace(Direction.East, FaceType.BulkheadClosed, Piece.CorridorBulkhead, new Vector3(0f, 90f, 0f));
			corridorDef5.SetFace(Direction.East, FaceType.BulkheadOpened, Piece.CorridorBulkhead, new Vector3(0f, 90f, 0f));
			corridorDef5.SetFace(Direction.West, FaceType.BulkheadClosed, Piece.CorridorBulkhead, new Vector3(0f, -90f, 0f));
			corridorDef5.SetFace(Direction.West, FaceType.BulkheadOpened, Piece.CorridorBulkhead, new Vector3(0f, -90f, 0f));
			corridorDef5.SetFace(Direction.North, FaceType.BulkheadClosed, Piece.CorridorBulkhead, Vector3.zero);
			corridorDef5.SetFace(Direction.North, FaceType.BulkheadOpened, Piece.CorridorBulkhead, Vector3.zero);
			corridorDef5.SetFace(Direction.South, FaceType.Reinforcement, Piece.CorridorIShapeReinforcementSide, new Vector3(0f, 90f, 0f));
			corridorDef5.SetFace(Direction.South, FaceType.Hatch, Piece.CorridorIShapeHatchSide, new Vector3(0f, 90f, 0f));
			corridorDef5.SetFace(Direction.Above, FaceType.Hatch, Piece.CorridorTShapeHatchTop, Vector3.zero);
			corridorDef5.SetFace(Direction.Below, FaceType.Hatch, Piece.CorridorTShapeHatchBottom, Vector3.zero);
			corridorDef5.SetFace(Direction.East, FaceType.Hatch, Piece.CorridorHatch, new Vector3(0f, 90f, 0f));
			corridorDef5.SetFace(Direction.West, FaceType.Hatch, Piece.CorridorHatch, new Vector3(0f, -90f, 0f));
			corridorDef5.SetFace(Direction.North, FaceType.Hatch, Piece.CorridorHatch, Vector3.zero);
			corridorDef5.SetFace(Direction.South, FaceType.Planter, Piece.CorridorIShapePlanterSide, new Vector3(0f, 90f, 0f));
			CorridorDef corridorDef6 = new CorridorDef(Piece.CorridorXShape, Piece.Invalid, Piece.CorridorXShapeAdjustableSupport, TechType.BaseCorridorX);
			corridorDef6.SetFace(Direction.East, FaceType.Solid, Piece.CorridorCap, new Vector3(0f, 90f, 0f));
			corridorDef6.SetFace(Direction.West, FaceType.Solid, Piece.CorridorCap, new Vector3(0f, -90f, 0f));
			corridorDef6.SetFace(Direction.North, FaceType.Solid, Piece.CorridorCap, Vector3.zero);
			corridorDef6.SetFace(Direction.South, FaceType.Solid, Piece.CorridorCap, new Vector3(0f, 180f, 0f));
			corridorDef6.SetFace(Direction.Above, FaceType.Solid, Piece.CorridorCoverXShapeTopExtClosed, Vector3.zero);
			corridorDef6.SetFace(Direction.Below, FaceType.Solid, Piece.CorridorCoverXShapeBottomExtClosed, Vector3.zero);
			corridorDef6.SetFace(Direction.Above, FaceType.Hole, Piece.CorridorCoverXShapeTopExtOpened, Vector3.zero);
			corridorDef6.SetFace(Direction.Below, FaceType.Hole, Piece.CorridorCoverXShapeBottomExtOpened, Vector3.zero);
			corridorDef6.SetFace(Direction.Above, FaceType.Ladder, Piece.CorridorXShapeLadderTop, Vector3.zero);
			corridorDef6.SetFace(Direction.Below, FaceType.Ladder, Piece.CorridorXShapeLadderBottom, Vector3.zero);
			corridorDef6.SetFace(Direction.Above, FaceType.Hatch, Piece.CorridorXShapeHatchTop, Vector3.zero);
			corridorDef6.SetFace(Direction.Below, FaceType.Hatch, Piece.CorridorXShapeHatchBottom, Vector3.zero);
			corridorDef6.SetFace(Direction.East, FaceType.Hatch, Piece.CorridorHatch, new Vector3(0f, 90f, 0f));
			corridorDef6.SetFace(Direction.West, FaceType.Hatch, Piece.CorridorHatch, new Vector3(0f, -90f, 0f));
			corridorDef6.SetFace(Direction.North, FaceType.Hatch, Piece.CorridorHatch, Vector3.zero);
			corridorDef6.SetFace(Direction.South, FaceType.Hatch, Piece.CorridorHatch, new Vector3(0f, 180f, 0f));
			corridorDef6.SetFace(Direction.East, FaceType.Window, Piece.CorridorWindow, new Vector3(0f, 90f, 0f));
			corridorDef6.SetFace(Direction.West, FaceType.Window, Piece.CorridorWindow, new Vector3(0f, -90f, 0f));
			corridorDef6.SetFace(Direction.North, FaceType.Window, Piece.CorridorWindow, Vector3.zero);
			corridorDef6.SetFace(Direction.South, FaceType.Window, Piece.CorridorWindow, new Vector3(0f, 180f, 0f));
			corridorDef6.SetFace(Direction.Above, FaceType.Window, Piece.CorridorXShapeWindowTop, Vector3.zero);
			corridorDef6.SetFace(Direction.East, FaceType.BulkheadClosed, Piece.CorridorBulkhead, new Vector3(0f, 90f, 0f));
			corridorDef6.SetFace(Direction.East, FaceType.BulkheadOpened, Piece.CorridorBulkhead, new Vector3(0f, 90f, 0f));
			corridorDef6.SetFace(Direction.West, FaceType.BulkheadClosed, Piece.CorridorBulkhead, new Vector3(0f, -90f, 0f));
			corridorDef6.SetFace(Direction.West, FaceType.BulkheadOpened, Piece.CorridorBulkhead, new Vector3(0f, -90f, 0f));
			corridorDef6.SetFace(Direction.North, FaceType.BulkheadClosed, Piece.CorridorBulkhead, Vector3.zero);
			corridorDef6.SetFace(Direction.North, FaceType.BulkheadOpened, Piece.CorridorBulkhead, Vector3.zero);
			corridorDef6.SetFace(Direction.South, FaceType.BulkheadClosed, Piece.CorridorBulkhead, new Vector3(0f, 180f, 0f));
			corridorDef6.SetFace(Direction.South, FaceType.BulkheadOpened, Piece.CorridorBulkhead, new Vector3(0f, 180f, 0f));
			corridors = new CorridorDef[16];
			corridors[3] = corridorDef3;
			corridors[12] = corridorDef3.GetRotated(90f);
			corridors[5] = corridorDef4;
			corridors[6] = corridorDef4.GetRotated(90f);
			corridors[10] = corridorDef4.GetRotated(180f);
			corridors[9] = corridorDef4.GetRotated(-90f);
			corridors[13] = corridorDef5;
			corridors[7] = corridorDef5.GetRotated(90f);
			corridors[14] = corridorDef5.GetRotated(180f);
			corridors[11] = corridorDef5.GetRotated(-90f);
			corridors[15] = corridorDef6;
			glassCorridors = new CorridorDef[16];
			glassCorridors[3] = corridorDef;
			glassCorridors[12] = corridorDef.GetRotated(90f);
			glassCorridors[5] = corridorDef2;
			glassCorridors[6] = corridorDef2.GetRotated(90f);
			glassCorridors[10] = corridorDef2.GetRotated(180f);
			glassCorridors[9] = corridorDef2.GetRotated(-90f);
		}
	}

	public bool IsCellEmpty(Int3 cell)
	{
		int cellIndex = GetCellIndex(cell);
		if (cellIndex != -1)
		{
			return cells[cellIndex] == CellType.Empty;
		}
		return true;
	}

	private bool IsFoundation(int index)
	{
		CellType cellType = cells[index];
		if (cellType != CellType.Foundation && cellType != CellType.WallFoundationN && cellType != CellType.WallFoundationW && cellType != CellType.WallFoundationS)
		{
			return cellType == CellType.WallFoundationE;
		}
		return true;
	}

	private bool IsRechargePlatform(int index)
	{
		return cells[index] == CellType.RechargePlatform;
	}

	public bool IsInterior(int index)
	{
		return IsInterior(cells[index]);
	}

	public bool CompareCellTypes(Int3 startCell, Int3 size, ICollection<CellType> compareTypes, bool hasAny = false)
	{
		bool result = !hasAny;
		foreach (Int3 item in new Int3.RangeEnumerator(startCell, startCell + size - 1))
		{
			CellType cell = GetCell(item);
			if (compareTypes.Contains(cell) == hasAny)
			{
				result = hasAny;
				break;
			}
		}
		return result;
	}

	public bool CompareCellTypes(Int3 startCell, Int3 size, CellType compareType, bool hasAny = false, bool includeGhosts = false)
	{
		bool result = !hasAny;
		foreach (Int3 item in new Int3.RangeEnumerator(startCell, startCell + size - 1))
		{
			if (GetCell(item) == compareType == hasAny)
			{
				result = hasAny;
				break;
			}
		}
		if (includeGhosts)
		{
			foreach (BaseGhost ghost in ghosts)
			{
				if (ghost.GhostBase.CompareCellTypes(startCell - ghost.TargetOffset, size, compareType, hasAny) == hasAny)
				{
					result = hasAny;
					break;
				}
			}
		}
		return result;
	}

	private bool IsInterior(CellType cellType)
	{
		if (cellType != CellType.Room && cellType != CellType.Corridor && cellType != CellType.Observatory && cellType != CellType.Moonpool && cellType != CellType.MoonpoolRotated && cellType != CellType.MapRoom && cellType != CellType.MapRoomRotated && cellType != CellType.ControlRoom && cellType != CellType.ControlRoomRotated && cellType != CellType.LargeRoom)
		{
			return cellType == CellType.LargeRoomRotated;
		}
		return true;
	}

	public bool HasSpaceFor(Int3 cell, Int3 size)
	{
		return HasSpaceFor(cell, size, ghosts);
	}

	private bool HasSpaceFor(Int3 cell, Int3 size, List<BaseGhost> ghosts)
	{
		Int3.RangeEnumerator rangeEnumerator = Int3.Range(cell, cell + size - 1);
		while (rangeEnumerator.MoveNext())
		{
			Int3 current = rangeEnumerator.Current;
			int index = baseShape.GetIndex(current);
			if (index != -1 && cells[index] != 0)
			{
				return false;
			}
			if (IsCellUnderConstruction(current, ghosts))
			{
				return false;
			}
		}
		return true;
	}

	private bool HasSpaceFor(Int3 cell, Piece piece)
	{
		if (piece == Piece.Invalid)
		{
			return false;
		}
		PieceDef pieceDef = pieces[(int)piece];
		return HasSpaceFor(cell, pieceDef.extraCells + 1);
	}

	private bool HasFoundation(Int3 point)
	{
		Int3 maxs = point - new Int3(0, 1, 0);
		foreach (Int3 item in Int3.Range(new Int3(maxs.x, 0, maxs.z), maxs))
		{
			int cellIndex = GetCellIndex(item);
			if (cellIndex == -1)
			{
				break;
			}
			if (IsFoundation(cellIndex))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsHorizontal(Direction direction)
	{
		Direction[] horizontalDirections = HorizontalDirections;
		for (int i = 0; i < horizontalDirections.Length; i++)
		{
			if (horizontalDirections[i] == direction)
			{
				return true;
			}
		}
		return false;
	}

	public Transform CreateCellObject(Int3 cell)
	{
		int cellIndex = GetCellIndex(cell);
		if (cellIndex == -1)
		{
			return null;
		}
		Transform transform = UnityEngine.Object.Instantiate(cellPrefab, GridToLocal(NormalizeCell(cell)), Quaternion.identity);
		cellObjects[cellIndex] = transform;
		transform.GetComponent<BaseCell>().cell = cell;
		transform.SetParent(base.transform, worldPositionStays: false);
		transform.tag = "Generated";
		return transform;
	}

	public Transform FindFaceObject(Face face)
	{
		Transform result = null;
		Transform cellObject = GetCellObject(face.cell);
		if (cellObject != null)
		{
			cellObject.GetComponentsInChildren(includeInactive: false, sDeconstructables);
			for (int i = 0; i < sDeconstructables.Count; i++)
			{
				BaseDeconstructable baseDeconstructable = sDeconstructables[i];
				if (baseDeconstructable != null && baseDeconstructable.face.HasValue && baseDeconstructable.face.Value == face)
				{
					result = baseDeconstructable.transform;
					break;
				}
			}
		}
		sDeconstructables.Clear();
		return result;
	}

	public Transform GetCellObject(Int3 cell)
	{
		int cellIndex = GetCellIndex(cell);
		if (cellIndex == -1)
		{
			return null;
		}
		return cellObjects[cellIndex];
	}

	public Int3? FindCellObject(Transform cellObject)
	{
		foreach (Int3 allCell in AllCells)
		{
			if (GetCellObject(allCell) == cellObject)
			{
				return allCell;
			}
		}
		return null;
	}

	private void BindCellObjects()
	{
		BaseCell[] componentsInChildren = GetComponentsInChildren<BaseCell>();
		foreach (BaseCell baseCell in componentsInChildren)
		{
			if (baseCell.transform.parent != base.transform)
			{
				continue;
			}
			Int3 @int = WorldToGrid(baseCell.transform.position);
			int index = baseShape.GetIndex(@int);
			if (index == -1)
			{
				Debug.LogError("Base contains invalid cell object at: " + @int);
			}
			else if (cellObjects[index] != null)
			{
				if (cellObjects[index] != baseCell)
				{
					Debug.LogError("Cell object already bound: " + @int);
				}
			}
			else
			{
				cellObjects[index] = baseCell.transform;
			}
		}
	}

	private Transform SpawnPiece(Piece piece, Int3 cell, BaseDeconstructable sourceBaseDeconstructable = null)
	{
		return SpawnPiece(piece, cell, Quaternion.identity, null, sourceBaseDeconstructable);
	}

	private Transform SpawnPiece(Piece piece, Int3 cell, Quaternion rotation, Direction? faceDirection = null, BaseDeconstructable sourceBaseDeconstructable = null)
	{
		return SpawnPiece(piece, cell, Vector3.zero, rotation, faceDirection, sourceBaseDeconstructable);
	}

	private GameObject InstantiateOrReuse(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation, Int3 cell)
	{
		for (int i = 0; i < cachedPieces.Count; i++)
		{
			CachedPiece cachedPiece = cachedPieces[i];
			if (cell == cachedPiece.cell && rotation == cachedPiece.obj.transform.localRotation && string.Equals(prefab.name, cachedPiece.obj.name))
			{
				cachedPiece.obj.transform.parent = parent;
				cachedPieces.RemoveAt(i);
				return cachedPiece.obj;
			}
		}
		return UWE.Utils.InstantiateDeactivated(prefab, parent, position, rotation);
	}

	private Transform SpawnPiece(Piece piece, Int3 cell, Vector3 position, Quaternion rotation, Direction? faceDirection = null, BaseDeconstructable sourceBaseDeconstructable = null)
	{
		if (piece == Piece.Invalid)
		{
			return null;
		}
		Transform transform = GetCellObject(cell);
		if (transform == null)
		{
			transform = CreateCellObject(cell);
		}
		PieceDef pieceDef = pieces[(int)piece];
		GameObject gameObject = InstantiateOrReuse(pieceDef.prefab.gameObject, transform, position, rotation, cell);
		if (faceDirection.HasValue && piece == Piece.CorridorBulkhead)
		{
			BaseWaterTransition[] componentsInChildren = gameObject.GetComponentsInChildren<BaseWaterTransition>();
			foreach (BaseWaterTransition obj in componentsInChildren)
			{
				obj.face.cell = cell;
				obj.face.direction = faceDirection.Value;
			}
		}
		gameObject.SetActive(value: true);
		gameObject.BroadcastMessage("OnAddedToBase", this, SendMessageOptions.DontRequireReceiver);
		if (sourceBaseDeconstructable != null)
		{
			ConstructableBounds[] componentsInChildren2 = gameObject.transform.GetComponentsInChildren<ConstructableBounds>();
			foreach (ConstructableBounds constructableBounds in componentsInChildren2)
			{
				sourceBaseDeconstructable.basePiecesBounds.Add(constructableBounds.bounds);
			}
		}
		return gameObject.transform;
	}

	public static Direction ReverseDirection(Direction direction)
	{
		return OppositeDirections[(int)direction];
	}

	private static byte PackOffset(Int3 offset)
	{
		return (byte)((uint)(((offset.x & 7) << 5) | ((offset.y & 3) << 3)) | ((uint)offset.z & 7u));
	}

	private static Int3 UnpackOffset(byte packedOffset)
	{
		return new Int3((packedOffset >> 5) & 7, (packedOffset >> 3) & 3, packedOffset & 7);
	}

	public bool IsCellValid(Int3 cell)
	{
		return baseShape.GetIndex(cell) != -1;
	}

	public Int3 NormalizeCell(Int3 cell)
	{
		int index = baseShape.GetIndex(cell);
		if (index != -1 && cells[index] == CellType.OccupiedByOtherCell)
		{
			return cell - UnpackOffset(links[index]);
		}
		return cell;
	}

	public CellType GetRawCellType(Int3 cell)
	{
		int index = baseShape.GetIndex(cell);
		if (index != -1)
		{
			return cells[index];
		}
		return CellType.Empty;
	}

	public int GetCellIndex(Int3 cell)
	{
		return baseShape.GetIndex(NormalizeCell(cell));
	}

	public Int3 GetCellPointFromIndex(int cellIndex)
	{
		return baseShape.GetPoint(cellIndex).ToInt3();
	}

	public CellType GetCell(Int3 cell)
	{
		int cellIndex = GetCellIndex(cell);
		if (cellIndex == -1)
		{
			return CellType.Empty;
		}
		return cells[cellIndex];
	}

	public CellType GetCell(int cellIndex)
	{
		return GetCell(baseShape.GetPoint(cellIndex).ToInt3());
	}

	public float GetPowerConsumptionRate()
	{
		return _consumptionRate;
	}

	public float GetPowerChargeRate()
	{
		return _chargeRate;
	}

	public float GetCellStructualIntegrity(Int3 cell)
	{
		return GetHullStrength(NormalizeCell(cell));
	}

	public Int3 GetAnchor()
	{
		return anchor;
	}

	private static int GetFaceIndex(int cellIndex, Direction direction)
	{
		return (int)(cellIndex * 6 + direction);
	}

	private Direction NormalizeFaceDirection(int cellIndex, Direction direction)
	{
		int faceIndex = GetFaceIndex(cellIndex, direction);
		FaceType faceType = faces[faceIndex];
		if ((faceType & FaceType.OccupiedByOtherFace) != 0)
		{
			direction = (Direction)(faceType & (FaceType)127);
		}
		return direction;
	}

	private int GetNormalizedFaceIndex(int cellIndex, Direction direction)
	{
		return GetFaceIndex(cellIndex, NormalizeFaceDirection(cellIndex, direction));
	}

	private FaceType GetFace(int index, Direction direction)
	{
		return faces[GetNormalizedFaceIndex(index, direction)];
	}

	private void SetFace(int index, Direction direction, FaceType faceType)
	{
		faces[GetNormalizedFaceIndex(index, direction)] = faceType;
	}

	private void SetFaceOccupiedBy(Face face, Direction occupyingDirection)
	{
		int index = baseShape.GetIndex(face.cell);
		faces[GetFaceIndex(index, face.direction)] = FaceType.OccupiedByOtherFace | (FaceType)occupyingDirection;
	}

	public bool GetAreCellFacesUsed(Int3 cell)
	{
		int index = baseShape.GetIndex(cell);
		switch (GetCell(cell))
		{
		case CellType.Empty:
		case CellType.Foundation:
		case CellType.RechargePlatform:
		case CellType.WallFoundationN:
		case CellType.WallFoundationW:
		case CellType.WallFoundationS:
		case CellType.WallFoundationE:
			return false;
		case CellType.Corridor:
		{
			CorridorDef corridorDef = GetCorridorDef(index);
			int num = 7;
			Direction[] allDirections = AllDirections;
			foreach (Direction direction2 in allDirections)
			{
				if (IsCellFaceUsed(index, direction2))
				{
					return true;
				}
				Direction direction3 = corridorDef.worldToLocal[(int)direction2];
				if (corridorDef.faces[(int)direction3, num].piece == Piece.Invalid)
				{
					continue;
				}
				Int3 adjacent = GetAdjacent(cell, direction2);
				int cellIndex = GetCellIndex(adjacent);
				if (cellIndex != -1)
				{
					int faceIndex = GetFaceIndex(cellIndex, ReverseDirection(direction2));
					if (IsBulkhead(faces[faceIndex]))
					{
						return true;
					}
				}
			}
			break;
		}
		default:
		{
			Direction[] allDirections = AllDirections;
			foreach (Direction direction in allDirections)
			{
				if (IsCellFaceUsed(index, direction))
				{
					return true;
				}
			}
			break;
		}
		}
		return false;
	}

	private bool IsCellFaceUsed(int cellIndex, Direction direction)
	{
		int faceIndex = GetFaceIndex(cellIndex, direction);
		FaceType faceType = faces[faceIndex];
		if ((faceType & FaceType.OccupiedByOtherFace) != 0)
		{
			Direction direction2 = (Direction)(faceType & (FaceType)127);
			int faceIndex2 = GetFaceIndex(cellIndex, direction2);
			faceType = faces[faceIndex2];
		}
		if (faceType != 0 && faceType != FaceType.Solid && faceType != FaceType.Hole)
		{
			return faceType != FaceType.ControlRoomModule;
		}
		return false;
	}

	public FaceType GetFace(Face face)
	{
		int index = baseShape.GetIndex(face.cell);
		if (index == -1)
		{
			return FaceType.None;
		}
		return GetFace(index, face.direction);
	}

	public FaceType GetFaceRaw(Face face)
	{
		int index = baseShape.GetIndex(face.cell);
		if (index == -1)
		{
			return FaceType.None;
		}
		int faceIndex = GetFaceIndex(index, face.direction);
		if (faceIndex < 0 || faceIndex >= faces.Length)
		{
			return FaceType.None;
		}
		return faces[faceIndex];
	}

	private bool CanSetCorridorFace(Face face, FaceType faceType)
	{
		int cellIndex = GetCellIndex(face.cell);
		if (GetFace(cellIndex, face.direction) != constructFaceTypes[(uint)faceType])
		{
			return false;
		}
		CorridorDef corridorDef = GetCorridorDef(cellIndex);
		Direction direction = corridorDef.worldToLocal[(int)face.direction];
		if (corridorDef.faces[(int)direction, (uint)faceType].piece == Piece.Invalid)
		{
			return false;
		}
		bool flag = IsInterior(cellIndex);
		bool result = false;
		switch (faceType)
		{
		case FaceType.Window:
		case FaceType.Hatch:
		case FaceType.ObsoleteDoor:
		case FaceType.Reinforcement:
		case FaceType.BulkheadClosed:
		case FaceType.BulkheadOpened:
		case FaceType.Planter:
			result = flag;
			break;
		}
		if (faceType == FaceType.Hatch && GetCell(GetAdjacent(face)) != 0)
		{
			result = false;
		}
		return result;
	}

	private bool IsRootRoomCell(Int3 cell)
	{
		return cell - NormalizeCell(cell) == new Int3(0, 0, 0);
	}

	private bool IsCentralRoomCell(Int3 cell)
	{
		return cell - NormalizeCell(cell) == new Int3(1, 0, 1);
	}

	private Int3 GetCentralRoomCell(Int3 cell)
	{
		return NormalizeCell(cell) + new Int3(1, 0, 1);
	}

	private Piece GetRoomPiece(Face face, FaceType faceType, bool aboveOccupied = false)
	{
		if (IsCentralRoomCell(face.cell))
		{
			return roomFaceCentralPieces[(int)face.direction, (uint)faceType];
		}
		if (IsRootRoomCell(face.cell) && face.direction == Direction.Above)
		{
			if (faceType == FaceType.GlassDome)
			{
				return Piece.RoomExteriorTopGlass;
			}
			if (!aboveOccupied && faceType == FaceType.Solid)
			{
				return Piece.RoomExteriorTop;
			}
			return Piece.Invalid;
		}
		return roomFacePieces[(int)face.direction, (uint)faceType];
	}

	private Piece GetMoonpoolPiece(Face face, FaceType faceType)
	{
		Int3 cell = face.cell;
		NormalizeCell(cell);
		int direction = (int)face.direction;
		if (direction >= 0 && direction < moonpoolFacePieces.GetLength(0) && (int)faceType >= 0 && (int)faceType < moonpoolFacePieces.GetLength(1))
		{
			return moonpoolFacePieces[direction, (uint)faceType];
		}
		return Piece.Invalid;
	}

	private Piece GetMoonpoolRotatedPiece(Face face, FaceType faceType)
	{
		Int3 cell = face.cell;
		NormalizeCell(cell);
		int direction = (int)face.direction;
		if (direction >= 0 && direction < moonpoolRotatedFacePieces.GetLength(0) && (int)faceType >= 0 && (int)faceType < moonpoolRotatedFacePieces.GetLength(1))
		{
			return moonpoolRotatedFacePieces[direction, (uint)faceType];
		}
		return Piece.Invalid;
	}

	private bool IsCentralLargeRoomCell(Int3 cell)
	{
		Int3 @int = cell - NormalizeCell(cell);
		if (@int.z == 1 && @int.x > 0)
		{
			return @int.x < 5;
		}
		return false;
	}

	private Piece GetLargeRoomPiece(Face face, FaceType faceType, bool aboveOccupied = false)
	{
		if (IsCentralLargeRoomCell(face.cell))
		{
			return largeRoomFaceCentralPieces[(int)face.direction, (uint)faceType];
		}
		if (IsRootRoomCell(face.cell) && face.direction == Direction.Above)
		{
			if (faceType == FaceType.LargeGlassDome)
			{
				return Piece.LargeRoomExteriorTopGlass;
			}
			if (!aboveOccupied && faceType == FaceType.Solid)
			{
				return Piece.LargeRoomExteriorTop;
			}
			return Piece.Invalid;
		}
		return largeRoomFacePieces[(int)face.direction, (uint)faceType];
	}

	private bool IsCentralLargeRoomRotatedCell(Int3 cell)
	{
		Int3 @int = cell - NormalizeCell(cell);
		if (@int.x == 1 && @int.z > 0)
		{
			return @int.z < 5;
		}
		return false;
	}

	private Piece GetLargeRoomRotatedPiece(Face face, FaceType faceType, bool aboveOccupied = false)
	{
		if (IsCentralLargeRoomRotatedCell(face.cell))
		{
			return largeRoomRotatedFaceCentralPieces[(int)face.direction, (uint)faceType];
		}
		if (IsRootRoomCell(face.cell) && face.direction == Direction.Above)
		{
			if (faceType == FaceType.LargeGlassDome)
			{
				return Piece.LargeRoomExteriorTopGlass;
			}
			if (!aboveOccupied && faceType == FaceType.Solid)
			{
				return Piece.LargeRoomExteriorTop;
			}
			return Piece.Invalid;
		}
		return largeRoomRotatedFacePieces[(int)face.direction, (uint)faceType];
	}

	public bool CanSetWaterPark(Int3 cell)
	{
		switch (GetCell(cell))
		{
		case CellType.Room:
			return CanSetRoomWaterPark(cell);
		case CellType.LargeRoom:
		case CellType.LargeRoomRotated:
			return CanSetLargeRoomWaterPark(cell);
		default:
			return false;
		}
	}

	private bool CanSetRoomWaterPark(Int3 cell)
	{
		if (!IsCentralRoomCell(cell))
		{
			return false;
		}
		int index = baseShape.GetIndex(cell);
		if (index == -1)
		{
			return false;
		}
		Direction[] verticalDirections = VerticalDirections;
		foreach (Direction direction in verticalDirections)
		{
			if (GetFace(index, direction) != FaceType.Solid)
			{
				return false;
			}
		}
		verticalDirections = HorizontalDirections;
		foreach (Direction direction2 in verticalDirections)
		{
			if (GetFace(index, direction2) != 0)
			{
				return false;
			}
		}
		return true;
	}

	public bool CanSetLargeRoomWaterPark(Int3 cell)
	{
		int index;
		int index2;
		if (GetCell(cell) == CellType.LargeRoomRotated)
		{
			index = 2;
			index2 = 0;
		}
		else
		{
			index = 0;
			index2 = 2;
		}
		Int3 @int = NormalizeCell(cell);
		Int3 int2 = cell - @int;
		if (int2[index2] != 1)
		{
			return false;
		}
		if (int2[index] < 1 || int2[index] > 3)
		{
			return false;
		}
		int i = int2[index];
		for (int num = int2[index] + 2; i < num; i++)
		{
			cell[index] = @int[index] + i;
			int index3 = baseShape.GetIndex(cell);
			if (index3 == -1)
			{
				return false;
			}
			Direction[] verticalDirections = VerticalDirections;
			foreach (Direction direction in verticalDirections)
			{
				if (GetFace(index3, direction) != FaceType.Solid)
				{
					return false;
				}
			}
			verticalDirections = HorizontalDirections;
			foreach (Direction direction2 in verticalDirections)
			{
				if (GetFace(index3, direction2) != 0)
				{
					return false;
				}
			}
		}
		return true;
	}

	private int GetLargeRoomWaterParks(Int3 cell)
	{
		cell = NormalizeCell(cell);
		int index;
		int index2;
		switch (GetCell(cell))
		{
		case CellType.LargeRoom:
			index = 0;
			index2 = 2;
			break;
		case CellType.LargeRoomRotated:
			index = 2;
			index2 = 0;
			break;
		default:
			return 0;
		}
		Face face = new Face(cell, Direction.Below);
		face.cell[index2] = cell[index2] + 1;
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			face.cell[index] = cell[index] + 1 + i;
			if (GetFaceMask(face) && GetFace(face) == FaceType.WaterPark)
			{
				num |= 1 << i;
			}
		}
		return num;
	}

	private bool AreAlignedWaterParks(int waterParks1, int waterParks2)
	{
		if (waterParks1 != 0 && waterParks2 != 0)
		{
			return waterParks1 == 6 == (waterParks2 == 6);
		}
		return true;
	}

	public bool CanSetLadder(Face faceStart, out Face faceEnd)
	{
		faceEnd = faceStart;
		int cellIndex = GetCellIndex(faceStart.cell);
		if (cellIndex == -1)
		{
			return false;
		}
		switch (cells[cellIndex])
		{
		case CellType.Room:
			return CanSetRoomLadder(cellIndex, faceStart, out faceEnd);
		case CellType.LargeRoom:
			return CanSetLargeRoomLadder(cellIndex, faceStart, out faceEnd);
		case CellType.LargeRoomRotated:
			return CanSetLargeRoomRotatedLadder(cellIndex, faceStart, out faceEnd);
		case CellType.Corridor:
		{
			FaceType face = GetFace(cellIndex, faceStart.direction);
			if (face != FaceType.Solid && face != FaceType.Hole)
			{
				return false;
			}
			CorridorDef corridorDef = GetCorridorDef(cellIndex);
			Direction direction = corridorDef.worldToLocal[(int)faceStart.direction];
			if (corridorDef.faces[(int)direction, (uint)face].piece == Piece.Invalid)
			{
				return false;
			}
			Int3 exit = default(Int3);
			if (GetLadderExitCell(faceStart, out exit))
			{
				faceEnd = new Face(exit, ReverseDirection(faceStart.direction));
				return true;
			}
			break;
		}
		}
		return false;
	}

	private bool CanSetRoomLadder(int index, Face faceStart, out Face faceEnd)
	{
		faceEnd = GetAdjacentFace(faceStart);
		if (GetRoomPiece(faceStart, FaceType.Ladder) == Piece.Invalid)
		{
			return false;
		}
		if (GetCell(faceEnd.cell) != CellType.Room)
		{
			return false;
		}
		Int3 @int = NormalizeCell(faceStart.cell);
		Int3 int2 = faceStart.cell - @int;
		if (int2 < Int3.zero || int2 >= CellSize[1])
		{
			return false;
		}
		if (!roomLadderPlaces[int2.x, int2.z])
		{
			return false;
		}
		int index2 = baseShape.GetIndex(faceStart.cell);
		FaceType face = GetFace(index2, faceStart.direction);
		if (face != FaceType.Solid)
		{
			return false;
		}
		int index3 = baseShape.GetIndex(faceEnd.cell);
		face = GetFace(index3, faceEnd.direction);
		if (face != FaceType.Solid)
		{
			return false;
		}
		for (int i = 0; i < 2; i++)
		{
			int index4 = ((i == 0) ? index2 : index3);
			Direction[] horizontalDirections = HorizontalDirections;
			foreach (Direction direction in horizontalDirections)
			{
				face = GetFace(index4, direction);
				if (face == FaceType.FiltrationMachine || face - 14 <= FaceType.Solid)
				{
					return false;
				}
			}
		}
		return true;
	}

	private bool CanSetLargeRoomLadder(int index, Face faceStart, out Face faceEnd)
	{
		faceEnd = GetAdjacentFace(faceStart);
		if (GetLargeRoomPiece(faceStart, FaceType.Ladder) == Piece.Invalid)
		{
			return false;
		}
		if (GetCell(faceEnd.cell) != CellType.LargeRoom)
		{
			return false;
		}
		Int3 @int = NormalizeCell(faceStart.cell);
		Int3 int2 = faceStart.cell - @int;
		if (int2 < Int3.zero || int2 >= CellSize[17])
		{
			return false;
		}
		if (!largeRoomLadderPlaces[int2.x, int2.z])
		{
			return false;
		}
		int index2 = baseShape.GetIndex(faceStart.cell);
		FaceType face = GetFace(index2, faceStart.direction);
		if (face != FaceType.Solid)
		{
			return false;
		}
		int index3 = baseShape.GetIndex(faceEnd.cell);
		face = GetFace(index3, faceEnd.direction);
		if (face != FaceType.Solid)
		{
			return false;
		}
		for (int i = 0; i < 2; i++)
		{
			int index4 = ((i == 0) ? index2 : index3);
			Direction[] horizontalDirections = HorizontalDirections;
			foreach (Direction direction in horizontalDirections)
			{
				face = GetFace(index4, direction);
				if (face == FaceType.FiltrationMachine || face - 14 <= FaceType.Hatch)
				{
					return false;
				}
			}
		}
		return true;
	}

	private bool CanSetLargeRoomRotatedLadder(int index, Face faceStart, out Face faceEnd)
	{
		faceEnd = GetAdjacentFace(faceStart);
		if (GetLargeRoomRotatedPiece(faceStart, FaceType.Ladder) == Piece.Invalid)
		{
			return false;
		}
		if (GetCell(faceEnd.cell) != CellType.LargeRoomRotated)
		{
			return false;
		}
		Int3 @int = NormalizeCell(faceStart.cell);
		Int3 int2 = faceStart.cell - @int;
		if (int2 < Int3.zero || int2 >= CellSize[18])
		{
			return false;
		}
		if (!largeRoomRotatedLadderPlaces[int2.x, int2.z])
		{
			return false;
		}
		int index2 = baseShape.GetIndex(faceStart.cell);
		FaceType face = GetFace(index2, faceStart.direction);
		if (face != FaceType.Solid)
		{
			return false;
		}
		int index3 = baseShape.GetIndex(faceEnd.cell);
		face = GetFace(index3, faceEnd.direction);
		if (face != FaceType.Solid)
		{
			return false;
		}
		for (int i = 0; i < 2; i++)
		{
			int index4 = ((i == 0) ? index2 : index3);
			Direction[] horizontalDirections = HorizontalDirections;
			foreach (Direction direction in horizontalDirections)
			{
				face = GetFace(index4, direction);
				if (face == FaceType.FiltrationMachine || face - 14 <= FaceType.Hatch)
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool GetLadderExitPosition(Face face, out Vector3 position)
	{
		position = Vector3.zero;
		if (GetLadderExitCell(face, out var exit))
		{
			int cellIndex = GetCellIndex(exit);
			if (cellIndex == -1)
			{
				Debug.LogErrorFormat(this, "Could not find cell index for ladder exit {0}", exit);
				return false;
			}
			Int3 cell = exit;
			Vector3 vector = corridorLadderExit;
			if (cells[cellIndex] == CellType.Room)
			{
				Int3 @int = NormalizeCell(exit);
				Int3 int2 = exit - @int;
				if (int2 < Int3.zero || int2 >= CellSize[1])
				{
					Debug.LogErrorFormat(this, "Exit offset {0} is out of room bounds", int2);
					return false;
				}
				cell = @int;
				vector = roomLadderExits[int2.x, int2.z];
			}
			Vector3 vector2 = GridToLocal(cell);
			position = base.transform.TransformPoint(vector2 + vector);
			return true;
		}
		return false;
	}

	private bool GetLadderExitCell(Face face, out Int3 exit)
	{
		return GetLadderExitCell(face.cell, face.direction, out exit);
	}

	public bool GetLadderExitCell(Int3 cell, Direction direction, out Int3 exit)
	{
		exit = Int3.zero;
		if (direction != Direction.Above && direction != Direction.Below)
		{
			return false;
		}
		int cellIndex = GetCellIndex(cell);
		if (cellIndex == -1)
		{
			return false;
		}
		CellType cellType = cells[cellIndex];
		if (!IsInterior(cellType))
		{
			return false;
		}
		do
		{
			cell += DirectionOffset[(int)direction];
			cellIndex = GetCellIndex(cell);
			if (cellIndex == -1)
			{
				return false;
			}
			cellType = cells[cellIndex];
		}
		while (cellType == CellType.Connector);
		if (IsInterior(cellType))
		{
			CorridorDef corridorDef = GetCorridorDef(cellIndex);
			if (corridorDef.piece == Piece.CorridorLShape || cellType == CellType.Observatory || cellType == CellType.MapRoom || cellType == CellType.MapRoomRotated || (corridorDef.piece == Piece.CorridorIShapeGlass && direction == Direction.Below) || cellType == CellType.ControlRoom || cellType == CellType.ControlRoomRotated)
			{
				return false;
			}
			exit = cell;
			return true;
		}
		return false;
	}

	public static bool IsBulkhead(FaceType faceType)
	{
		if (faceType != FaceType.BulkheadClosed)
		{
			return faceType == FaceType.BulkheadOpened;
		}
		return true;
	}

	public bool CanSetBulkhead(Face fromCell)
	{
		Face adjacentFace = GetAdjacentFace(fromCell);
		if (!CanSetFace(fromCell, FaceType.BulkheadClosed))
		{
			return false;
		}
		if (!CanSetFace(adjacentFace, FaceType.BulkheadClosed))
		{
			return false;
		}
		return true;
	}

	public bool CanSetConnector(Int3 cell)
	{
		if (GetCell(cell) != 0)
		{
			return false;
		}
		Int3 adjacent = GetAdjacent(cell, Direction.Above);
		CellType cell2 = GetCell(adjacent);
		Int3 adjacent2 = GetAdjacent(cell, Direction.Below);
		CellType cell3 = GetCell(adjacent2);
		if (cell2 == CellType.Empty)
		{
			if (cell3 == CellType.Empty)
			{
				return false;
			}
			return CanConnectToCell(adjacent2, Direction.Above);
		}
		if (cell3 == CellType.Empty)
		{
			return CanConnectToCell(adjacent, Direction.Below);
		}
		if (CanConnectToCell(adjacent, Direction.Below))
		{
			return CanConnectToCell(adjacent2, Direction.Above);
		}
		return false;
	}

	private bool CanConnectToCell(Int3 cell, Direction direction)
	{
		switch (GetCell(cell))
		{
		case CellType.Connector:
			return true;
		case CellType.Corridor:
		{
			int cellIndex = GetCellIndex(cell);
			if (isGlass[cellIndex] && direction == Direction.Above)
			{
				return false;
			}
			CorridorDef corridorDef = GetCorridorDef(cellIndex);
			if (corridorDef.piece == Piece.CorridorLShape || corridorDef.piece == Piece.CorridorLShapeGlass)
			{
				return false;
			}
			FaceType face = GetFace(cellIndex, direction);
			if (face != FaceType.Solid)
			{
				return face == FaceType.Hole;
			}
			return true;
		}
		default:
			return false;
		}
	}

	private bool CanSetRoomFace(Face face, FaceType faceType)
	{
		int index = baseShape.GetIndex(face.cell);
		if (GetFace(index, face.direction) != constructFaceTypes[(uint)faceType])
		{
			return false;
		}
		bool flag = GetCell(GetAdjacent(face)) == CellType.Room;
		bool flag2 = face.direction == Direction.Above || face.direction == Direction.Below;
		bool result = false;
		switch (faceType)
		{
		case FaceType.BulkheadClosed:
		case FaceType.BulkheadOpened:
			result = !flag2;
			break;
		case FaceType.Ladder:
			result = flag && flag2;
			break;
		case FaceType.Window:
		case FaceType.Hatch:
		case FaceType.Reinforcement:
		case FaceType.Planter:
			result = !flag2;
			break;
		case FaceType.FiltrationMachine:
		{
			if (flag2)
			{
				break;
			}
			result = true;
			Direction[] verticalDirections = VerticalDirections;
			foreach (Direction direction in verticalDirections)
			{
				if (GetFace(index, direction) == FaceType.Ladder)
				{
					result = false;
					break;
				}
			}
			break;
		}
		case FaceType.GlassDome:
			result = face.direction == Direction.Above && !flag && IsRootRoomCell(face.cell) && HasSpaceFor(NormalizeCell(face.cell) + new Int3(0, 1, 0), CellSize[1]);
			break;
		}
		return result;
	}

	public bool CanSetModule(ref Face face, FaceType faceType)
	{
		int cellIndex = GetCellIndex(face.cell);
		if (cellIndex == -1)
		{
			return false;
		}
		Piece piece = Piece.Invalid;
		switch (cells[cellIndex])
		{
		case CellType.Room:
			face.cell = NormalizeCell(face.cell) + new Int3(1, 0, 1);
			piece = GetRoomPiece(face, faceType);
			break;
		case CellType.LargeRoom:
			piece = GetLargeRoomPiece(face, faceType);
			break;
		case CellType.LargeRoomRotated:
			piece = GetLargeRoomRotatedPiece(face, faceType);
			break;
		}
		if (piece == Piece.Invalid)
		{
			return false;
		}
		int index = baseShape.GetIndex(face.cell);
		Direction[] horizontalDirections = HorizontalDirections;
		foreach (Direction direction in horizontalDirections)
		{
			FaceType face2 = GetFace(index, direction);
			if (face2 != 0 && face2 != FaceType.Solid)
			{
				return false;
			}
		}
		horizontalDirections = VerticalDirections;
		foreach (Direction direction2 in horizontalDirections)
		{
			if (GetFace(index, direction2) != FaceType.Solid)
			{
				return false;
			}
		}
		return true;
	}

	public IBaseModule GetModule(Face face)
	{
		Int3 @int = face.cell - anchor;
		GetComponentsInChildren(includeInactive: true, sBaseModules);
		int i = 0;
		for (int count = sBaseModules.Count; i < count; i++)
		{
			IBaseModule baseModule = sBaseModules[i];
			Face moduleFace = baseModule.moduleFace;
			if (moduleFace.cell == @int && moduleFace.direction == face.direction)
			{
				sBaseModules.Clear();
				return baseModule;
			}
		}
		sBaseModules.Clear();
		return null;
	}

	public IBaseModuleGeometry GetModuleGeometry(Face face)
	{
		if (cells != null && cellObjects != null)
		{
			Int3 @int = anchor + face.cell;
			Transform cellObject = GetCellObject(@int);
			if (cellObject != null)
			{
				cellObject.GetComponentsInChildren(includeInactive: true, sBaseModulesGeometry);
				int i = 0;
				for (int count = sBaseModulesGeometry.Count; i < count; i++)
				{
					IBaseModuleGeometry baseModuleGeometry = sBaseModulesGeometry[i];
					Face geometryFace = baseModuleGeometry.geometryFace;
					if (geometryFace.cell == @int && geometryFace.direction == face.direction)
					{
						sBaseModulesGeometry.Clear();
						return baseModuleGeometry;
					}
				}
				sBaseModulesGeometry.Clear();
			}
		}
		return null;
	}

	private bool CanSetObservatoryFace(Face face, FaceType faceType)
	{
		if (faceType != FaceType.Hatch || GetFace(baseShape.GetIndex(face.cell), face.direction) != FaceType.Solid)
		{
			return false;
		}
		return true;
	}

	private bool CanSetMapRoomFace(Face face, FaceType faceType)
	{
		int index = baseShape.GetIndex(face.cell);
		if (GetFace(index, face.direction) != constructFaceTypes[(uint)faceType])
		{
			return false;
		}
		return mapRoomFacePieces[(uint)faceType] != Piece.Invalid;
	}

	private bool CanSetControlRoomFace(Face face, FaceType faceType)
	{
		int index = baseShape.GetIndex(face.cell);
		if (GetFace(index, face.direction) != constructFaceTypes[(uint)faceType])
		{
			return false;
		}
		return controlRoomFacePieces[(uint)faceType] != Piece.Invalid;
	}

	private bool CanSetMoonpoolFace(Face face, FaceType faceType, CellType cellType)
	{
		int index = baseShape.GetIndex(face.cell);
		if (GetFace(index, face.direction) != constructFaceTypes[(uint)faceType])
		{
			return false;
		}
		bool result = false;
		switch (faceType)
		{
		case FaceType.Window:
		case FaceType.Hatch:
		case FaceType.Reinforcement:
		case FaceType.BulkheadClosed:
		case FaceType.BulkheadOpened:
		case FaceType.Planter:
			result = true;
			break;
		case FaceType.UpgradeConsole:
		{
			result = true;
			if (cellType == CellType.Moonpool)
			{
				int i = 0;
				for (int num = moonpoolFaces.Length; i < num; i++)
				{
					RoomFace roomFace = moonpoolFaces[i];
					Face face2 = new Face(NormalizeCell(face.cell) + roomFace.offset, roomFace.direction);
					if (GetFaceMask(face2) && GetFace(face2) == FaceType.UpgradeConsole)
					{
						result = false;
						break;
					}
				}
				break;
			}
			int j = 0;
			for (int num2 = moonpoolRotatedFaces.Length; j < num2; j++)
			{
				RoomFace roomFace2 = moonpoolRotatedFaces[j];
				Face face3 = new Face(NormalizeCell(face.cell) + roomFace2.offset, roomFace2.direction);
				if (GetFaceMask(face3) && GetFace(face3) == FaceType.UpgradeConsole)
				{
					result = false;
					break;
				}
			}
			break;
		}
		}
		return result;
	}

	private bool GetEdgeMask(Int3 cell, out int mask)
	{
		mask = 0;
		Int3 @int = NormalizeCell(cell);
		Int3 int2 = cell - @int;
		int index = baseShape.GetIndex(@int);
		if (index == -1)
		{
			return false;
		}
		CellType cellType = cells[index];
		Int3 int3 = CellSize[(uint)cellType] - Int3.one;
		if (int2.x < 0 || int2.x > int3.x)
		{
			return false;
		}
		if (int2.y < 0 || int2.y > int3.y)
		{
			return false;
		}
		if (int2.z < 0 || int2.z > int3.z)
		{
			return false;
		}
		if (int2.z == int3.z)
		{
			mask |= 1;
		}
		if (int2.z == 0)
		{
			mask |= 2;
		}
		if (int2.x == int3.x)
		{
			mask |= 4;
		}
		if (int2.x == 0)
		{
			mask |= 8;
		}
		if (int2.y == int3.y)
		{
			mask |= 16;
		}
		if (int2.y == 0)
		{
			mask |= 32;
		}
		return true;
	}

	private static bool IsZeroOrPOT(int mask)
	{
		if (mask != 0)
		{
			return (mask & (mask - 1)) == 0;
		}
		return true;
	}

	public bool CanSetPartition(Int3 cell, Direction partitionDirection)
	{
		int index = baseShape.GetIndex(cell);
		if (index == -1)
		{
			return false;
		}
		CellType cell2 = GetCell(cell);
		if (cell2 != CellType.LargeRoom && cell2 != CellType.LargeRoomRotated)
		{
			return false;
		}
		if (!GetEdgeMask(cell, out var mask))
		{
			return false;
		}
		int num = 1 << (int)partitionDirection;
		FaceType face = GetFace(index, partitionDirection);
		bool flag = (mask & num) != 0;
		FaceType faceType = ((IsZeroOrPOT(mask & 0xF) && flag) ? FaceType.Solid : FaceType.None);
		if (face != faceType)
		{
			return false;
		}
		int num2 = 0;
		bool flag2 = false;
		Direction[] horizontalDirections = HorizontalDirections;
		foreach (Direction direction in horizontalDirections)
		{
			int num3 = 1 << (int)direction;
			face = GetFace(index, direction);
			switch (face)
			{
			case FaceType.Partition:
				num2 |= num3;
				continue;
			case FaceType.PartitionDoor:
				flag2 = true;
				continue;
			}
			if ((mask & num3) != 0)
			{
				if (face == FaceType.FiltrationMachine)
				{
					return false;
				}
			}
			else if (face != 0)
			{
				return false;
			}
		}
		horizontalDirections = VerticalDirections;
		foreach (Direction direction2 in horizontalDirections)
		{
			face = GetFace(index, direction2);
			if (face != FaceType.Solid)
			{
				return false;
			}
		}
		if (flag2 && (num2 == 3 || num2 == 12))
		{
			return false;
		}
		num2 |= num;
		Int3 @int = NormalizeCell(cell);
		Int3 offset = cell - @int;
		int maskRotation;
		return GetPartition(cell2, offset, num2, flag2, out maskRotation).piece != Piece.Invalid;
	}

	public bool CanSetPartitionDoor(Int3 cell, out Direction doorFaceDirection)
	{
		doorFaceDirection = Direction.Count;
		int index = baseShape.GetIndex(cell);
		if (index == -1)
		{
			return false;
		}
		CellType cell2 = GetCell(cell);
		if (cell2 != CellType.LargeRoom && cell2 != CellType.LargeRoomRotated)
		{
			return false;
		}
		if (!GetEdgeMask(cell, out var mask))
		{
			return false;
		}
		int num = 0;
		Direction[] horizontalDirections = HorizontalDirections;
		foreach (Direction direction in horizontalDirections)
		{
			switch (GetFace(index, direction))
			{
			case FaceType.Partition:
				num |= 1 << (int)direction;
				continue;
			case FaceType.None:
				continue;
			}
			int num2 = 1 << (int)direction;
			if ((mask & num2) == 0)
			{
				return false;
			}
		}
		switch (num)
		{
		case 3:
			doorFaceDirection = Direction.East;
			break;
		case 12:
			doorFaceDirection = Direction.North;
			break;
		}
		if (doorFaceDirection != Direction.Count)
		{
			if ((mask & (1 << (int)doorFaceDirection)) != 0)
			{
				doorFaceDirection = OppositeDirections[(int)doorFaceDirection];
			}
			return true;
		}
		return false;
	}

	private bool CanSetLargeRoomFace(Face face, FaceType faceType, CellType cellType)
	{
		int index = baseShape.GetIndex(face.cell);
		if (GetFace(index, face.direction) != constructFaceTypes[(uint)faceType])
		{
			return false;
		}
		bool flag = GetCell(GetAdjacent(face)) == cellType;
		bool flag2 = face.direction == Direction.Above || face.direction == Direction.Below;
		bool result = false;
		switch (faceType)
		{
		case FaceType.Window:
		case FaceType.Hatch:
		case FaceType.Reinforcement:
		case FaceType.BulkheadClosed:
		case FaceType.BulkheadOpened:
		case FaceType.Planter:
			result = !flag2;
			break;
		case FaceType.FiltrationMachine:
		{
			if (flag2)
			{
				break;
			}
			result = true;
			Direction[] allDirections = AllDirections;
			foreach (Direction direction in allDirections)
			{
				if (direction != face.direction)
				{
					FaceType face2 = GetFace(index, direction);
					if (face2 == FaceType.Partition || face2 == FaceType.PartitionDoor || face2 == FaceType.Ladder)
					{
						result = false;
						break;
					}
				}
			}
			break;
		}
		case FaceType.Ladder:
			result = flag && flag2;
			break;
		case FaceType.LargeGlassDome:
			result = face.direction == Direction.Above && !flag && IsRootRoomCell(face.cell) && HasSpaceFor(NormalizeCell(face.cell) + new Int3(0, 1, 0), CellSize[(uint)cellType]);
			break;
		}
		return result;
	}

	private bool CanSetWaterParkFace(Face face, FaceType faceType)
	{
		if (faceType != FaceType.Hatch || face.direction == Direction.Above || face.direction == Direction.Below)
		{
			return false;
		}
		baseShape.GetIndex(face.cell);
		return GetFace(face) == FaceType.Solid;
	}

	public bool CanSetFace(Face srcStart, FaceType faceType)
	{
		CellType cell = GetCell(srcStart.cell);
		bool result = false;
		switch (cell)
		{
		case CellType.Corridor:
			result = CanSetCorridorFace(srcStart, faceType);
			break;
		case CellType.Room:
		{
			Face face = new Face(srcStart.cell, Direction.Above);
			result = ((GetFace(face) != FaceType.WaterPark) ? CanSetRoomFace(srcStart, faceType) : CanSetWaterParkFace(srcStart, faceType));
			break;
		}
		case CellType.Observatory:
			result = CanSetObservatoryFace(srcStart, faceType);
			break;
		case CellType.MapRoom:
		case CellType.MapRoomRotated:
			result = CanSetMapRoomFace(srcStart, faceType);
			break;
		case CellType.Moonpool:
		case CellType.MoonpoolRotated:
			result = CanSetMoonpoolFace(srcStart, faceType, cell);
			break;
		case CellType.ControlRoom:
		case CellType.ControlRoomRotated:
			result = CanSetControlRoomFace(srcStart, faceType);
			break;
		case CellType.LargeRoom:
		case CellType.LargeRoomRotated:
		{
			Face face = new Face(srcStart.cell, Direction.Above);
			result = ((GetFace(face) != FaceType.WaterPark) ? CanSetLargeRoomFace(srcStart, faceType, cell) : CanSetWaterParkFace(srcStart, faceType));
			break;
		}
		}
		return result;
	}

	public void SetFaceType(Face face, FaceType faceType)
	{
		int index = baseShape.GetIndex(face.cell);
		if (index != -1)
		{
			SetFace(index, face.direction, faceType);
		}
	}

	private void UpdateFlowData(Int3 cell)
	{
		int cellIndex = GetCellIndex(cell);
		if (cellIndex == -1)
		{
			return;
		}
		byte b = 0;
		if (IsInterior(cellIndex))
		{
			b = (byte)(b | 0x40u);
		}
		Face face = default(Face);
		face.cell = cell;
		Direction[] allDirections = AllDirections;
		foreach (Direction direction in allDirections)
		{
			int cellIndex2 = GetCellIndex(cell + DirectionOffset[(int)direction]);
			if (cellIndex2 == -1 || !IsInterior(cellIndex2))
			{
				continue;
			}
			face.direction = direction;
			FaceType faceType = GetFace(face);
			if (faceType == FaceType.None)
			{
				FaceType face2 = GetFace(GetAdjacentFace(face));
				if (IsBulkhead(face2))
				{
					faceType = face2;
				}
			}
			if (faceType == FaceType.None || faceType == FaceType.ObsoleteDoor || faceType == FaceType.Ladder || faceType == FaceType.BulkheadOpened || faceType == FaceType.Partition || faceType == FaceType.PartitionDoor)
			{
				b |= (byte)(1 << (int)direction);
			}
		}
		int index = baseShape.GetIndex(cell);
		flowData[index] = b;
	}

	private void RecalculateFlowData()
	{
		foreach (Int3 allCell in AllCells)
		{
			UpdateFlowData(allCell);
		}
	}

	private void RecomputeOccupiedCells()
	{
		occupiedCellIndexes.Clear();
		for (int i = 0; i < baseShape.Size; i++)
		{
			if (cells[i] != 0)
			{
				occupiedCellIndexes.Add(i);
			}
		}
	}

	private void UpdateFlowDataForCellAndNeighbors(Int3 cell)
	{
		UpdateFlowData(cell);
		Face face = default(Face);
		face.cell = cell;
		Direction[] allDirections = AllDirections;
		foreach (Direction direction in allDirections)
		{
			face.direction = direction;
			Int3 adjacent = GetAdjacent(face);
			UpdateFlowData(adjacent);
		}
	}

	private void SetFaceAndUpdateFlow(Face face, FaceType faceType)
	{
		SetFaceType(face, faceType);
		UpdateFlowData(face.cell);
		Int3 adjacent = GetAdjacent(face);
		UpdateFlowData(adjacent);
	}

	public static Int3 GetAdjacent(Int3 cell, Direction direction)
	{
		return cell + DirectionOffset[(int)direction];
	}

	public static Int3 GetAdjacent(Face face)
	{
		return GetAdjacent(face.cell, face.direction);
	}

	public static Face GetAdjacentFace(Face face)
	{
		return new Face(GetAdjacent(face), ReverseDirection(face.direction));
	}

	private void BuildFoundationGeometry(Int3 cell)
	{
		Int3 @int = CellSize[2];
		Vector3 position = Int3.Scale(@int - Int3.one, halfCellSize);
		Transform obj = SpawnPiece(Piece.Foundation, cell, position, Quaternion.identity);
		Int3.Bounds bounds = new Int3.Bounds(cell, cell + @int - 1);
		BaseDeconstructable.MakeCellDeconstructable(obj, bounds, TechType.BaseFoundation);
		obj.tag = "MainPieceGeometry";
	}

	private void BuildWallFoundationGeometry(Int3 cell, CellType cellType)
	{
		Piece piece = Piece.WallFoundationN;
		switch (cellType)
		{
		case CellType.WallFoundationN:
			piece = Piece.WallFoundationN;
			break;
		case CellType.WallFoundationW:
			piece = Piece.WallFoundationW;
			break;
		case CellType.WallFoundationS:
			piece = Piece.WallFoundationS;
			break;
		case CellType.WallFoundationE:
			piece = Piece.WallFoundationE;
			break;
		}
		Int3 @int = CellSize[(uint)cellType];
		Vector3 position = Int3.Scale(@int - Int3.one, halfCellSize);
		Transform obj = SpawnPiece(piece, cell, position, Quaternion.identity);
		Int3.Bounds bounds = new Int3.Bounds(cell, cell + @int - 1);
		BaseDeconstructable.MakeCellDeconstructable(obj, bounds, TechType.BaseWallFoundation);
		obj.tag = "MainPieceGeometry";
	}

	private void BuildRechargePlatformGeometry(Int3 cell)
	{
		Int3 @int = CellSize[10];
		Vector3 position = Int3.Scale(@int - Int3.one, halfCellSize);
		Transform obj = SpawnPiece(Piece.RechargePlatform, cell, position, Quaternion.identity);
		Int3.Bounds bounds = new Int3.Bounds(cell, cell + @int - 1);
		BaseDeconstructable.MakeCellDeconstructable(obj, bounds, TechType.BaseRechargePlatform);
		obj.tag = "MainPieceGeometry";
	}

	private bool IsCellUsed(int index)
	{
		if (masks != null)
		{
			return (masks[index] & 0x40) != 0;
		}
		return true;
	}

	public bool IsFaceUsed(int index, Direction direction)
	{
		if (masks != null)
		{
			return (masks[index] & (1 << (int)direction)) != 0;
		}
		return true;
	}

	private void BuildCorridorGeometry(Int3 cell, int index)
	{
		CorridorDef corridorDef = GetCorridorDef(index);
		Int3.Bounds bounds = new Int3.Bounds(cell, cell);
		BaseDeconstructable parent = null;
		if (IsCellUsed(index))
		{
			TechType techType = corridorDef.techType;
			Transform obj = SpawnPiece(corridorDef.piece, cell, corridorDef.rotation);
			parent = BaseDeconstructable.MakeCellDeconstructable(obj, bounds, techType);
			obj.tag = "MainPieceGeometry";
			Piece piece = corridorDef.adjustableSupportPiece;
			Int3.Bounds bounds2 = Bounds;
			Int3 cell2 = cell;
			for (int num = cell.y - 1; num >= bounds2.mins.y; num--)
			{
				cell2.y = num;
				CellType cell3 = GetCell(cell2);
				if ((cell3 == CellType.Foundation || cell3 == CellType.WallFoundationN || cell3 == CellType.WallFoundationW || cell3 == CellType.WallFoundationS || cell3 == CellType.WallFoundationE) && num == cell.y - 1)
				{
					piece = corridorDef.supportPiece;
					break;
				}
				if (cell3 != 0)
				{
					piece = Piece.Invalid;
					break;
				}
			}
			SpawnPiece(piece, cell, corridorDef.rotation);
		}
		Direction[] allDirections = AllDirections;
		foreach (Direction direction in allDirections)
		{
			if (!IsFaceUsed(index, direction))
			{
				continue;
			}
			FaceType face2 = GetFace(index, direction);
			Direction direction2 = corridorDef.worldToLocal[(int)direction];
			CorridorFace corridorFace = corridorDef.faces[(int)direction2, (uint)face2];
			Quaternion rotation = corridorDef.rotation * corridorFace.rotation;
			if (direction == Direction.Above || direction == Direction.Below)
			{
				switch (face2)
				{
				case FaceType.Solid:
				{
					if (ExteriorToInteriorPiece(corridorFace.piece, out var interior3))
					{
						SpawnPiece(interior3, cell, rotation, direction);
					}
					break;
				}
				case FaceType.Hole:
				{
					CorridorFace corridorFace3 = corridorDef.faces[(int)direction2, 1];
					if (ExteriorToInteriorPiece(corridorFace3.piece, out var interior2))
					{
						SpawnPiece(interior2, cell, rotation, direction);
					}
					break;
				}
				case FaceType.Ladder:
					if (!isGhost)
					{
						CorridorFace corridorFace2 = corridorDef.faces[(int)direction2, 9];
						SpawnPiece(corridorFace2.piece, cell, rotation);
						if (ExteriorToInteriorPiece(corridorFace2.piece, out var interior))
						{
							SpawnPiece(interior, cell, rotation, direction);
						}
					}
					rotation = Quaternion.identity;
					break;
				}
			}
			Transform facePiece = SpawnPiece(corridorFace.piece, cell, rotation, direction);
			if (face2 == FaceType.None)
			{
				continue;
			}
			Face face = new Face(cell, direction);
			TechType recipe = FaceToRecipe[(uint)face2];
			if (IsBulkhead(face2))
			{
				BulkheadDoor componentInChildren = facePiece.GetComponentInChildren<BulkheadDoor>();
				if (componentInChildren != null)
				{
					Direction bulkheadDirection = direction;
					componentInChildren.SetState(face2 == FaceType.BulkheadOpened);
					componentInChildren.onStateChange = (BulkheadDoor.OnStateChange)Delegate.Combine(componentInChildren.onStateChange, (BulkheadDoor.OnStateChange)delegate(bool open)
					{
						FaceType faceType = (open ? FaceType.BulkheadOpened : FaceType.BulkheadClosed);
						int index2 = cellObjects.IndexOf(facePiece.parent);
						Grid3Point point = baseShape.GetPoint(index2);
						if (point.Valid)
						{
							SetFace(index2, bulkheadDirection, faceType);
							UpdateFlowDataForCellAndNeighbors(point.ToInt3());
							if (this.onBulkheadFaceChanged != null)
							{
								this.onBulkheadFaceChanged(this, face);
							}
						}
						else
						{
							Debug.LogError("Bulkhead door state changed but doesn't seem to be part of a base anymore");
						}
					});
				}
				else
				{
					Debug.LogError("Face tagged as bulkhead but piece missing BulkheadDoor component");
				}
			}
			switch (face2)
			{
			case FaceType.Ladder:
			{
				if (GetLadderExitCell(face, out var exit))
				{
					Int3.Bounds bounds3 = bounds.Union(exit);
					BaseDeconstructable.MakeFaceDeconstructable(facePiece, bounds3, face, FaceType.Ladder, recipe);
				}
				else
				{
					Debug.LogError("Face tagged as ladder but could not find exit cell");
				}
				break;
			}
			case FaceType.Solid:
				if (!isGhost)
				{
					BaseExplicitFace.MakeFaceDeconstructable(facePiece, face, parent);
				}
				break;
			default:
				if (!isGhost)
				{
					BaseDeconstructable.MakeFaceDeconstructable(facePiece, bounds, face, face2, recipe);
				}
				break;
			case FaceType.Hole:
				break;
			}
		}
	}

	public Direction GetObservatoryRotation(Int3 cell, out float yaw)
	{
		Direction result = Direction.East;
		yaw = 0f;
		if (IsValidObsConnection(GetAdjacent(cell, Direction.South), Direction.North))
		{
			yaw = 90f;
			result = Direction.South;
		}
		else if (IsValidObsConnection(GetAdjacent(cell, Direction.West), Direction.East))
		{
			yaw = 180f;
			result = Direction.West;
		}
		else if (IsValidObsConnection(GetAdjacent(cell, Direction.North), Direction.South))
		{
			yaw = 270f;
			result = Direction.North;
		}
		return result;
	}

	private void BuildObservatoryGeometry(Int3 cell)
	{
		_ = ref CellSize[5];
		Int3.Bounds bounds = new Int3.Bounds(cell, cell);
		float yaw = 0f;
		Direction observatoryRotation = GetObservatoryRotation(cell, out yaw);
		Quaternion rotation = Quaternion.Euler(0f, yaw, 0f);
		if (GetCellMask(cell))
		{
			Transform obj = SpawnPiece(Piece.Observatory, cell, rotation);
			BaseDeconstructable.MakeCellDeconstructable(obj, bounds, TechType.BaseObservatory);
			obj.tag = "MainPieceGeometry";
		}
		Face face = new Face(cell, observatoryRotation);
		if (GetFaceMask(face))
		{
			FaceType face2 = GetFace(face);
			Piece piece = observatoryFacePieces[(uint)face2];
			Transform geometry = SpawnPiece(piece, cell, rotation);
			if (face2 != FaceType.Solid)
			{
				TechType recipe = FaceToRecipe[(uint)face2];
				BaseDeconstructable.MakeFaceDeconstructable(geometry, bounds, face, face2, recipe);
			}
		}
	}

	private void BuildMapRoomGeometry(Int3 cell, int index, CellType cellType)
	{
		Int3 @int = CellSize[(uint)cellType];
		Int3.Bounds bounds = new Int3.Bounds(cell, cell + @int - 1);
		Transform transform = null;
		BaseDeconstructable parent = null;
		float y = ((cellType == CellType.MapRoomRotated) ? 90 : 0);
		Vector3 position = Int3.Scale(@int - Int3.one, halfCellSize);
		Quaternion rotation = Quaternion.Euler(0f, y, 0f);
		if (GetCellMask(cell))
		{
			transform = SpawnPiece(Piece.MapRoom, cell, position, rotation);
			parent = BaseDeconstructable.MakeCellDeconstructable(transform, bounds, TechType.BaseMapRoom);
			transform.tag = "MainPieceGeometry";
		}
		FaceDef[] array = faceDefs[(uint)cellType];
		for (int i = 0; i < array.Length; i++)
		{
			FaceDef faceDef = array[i];
			Face face = new Face(cell + faceDef.face.cell, faceDef.face.direction);
			if (GetFaceMask(face))
			{
				FaceType face2 = GetFace(face);
				Piece piece = mapRoomFacePieces[(uint)face2];
				Transform transform2 = SpawnPiece(piece, cell, FaceRotation[(int)faceDef.face.direction]);
				transform2.localPosition = Int3.Scale(faceDef.face.cell, cellSize);
				if (face2 != FaceType.Solid)
				{
					TechType recipe = FaceToRecipe[(uint)face2];
					BaseDeconstructable.MakeFaceDeconstructable(transform2, bounds, face, face2, recipe);
				}
				else if (!isGhost)
				{
					BaseExplicitFace.MakeFaceDeconstructable(transform2, face, parent);
				}
			}
		}
		if (!isGhost)
		{
			if (GetMapRoomFunctionalityForCell(cell) != null)
			{
				Debug.Log("found existing map room functionality at cell " + cell);
				return;
			}
			Debug.Log("create new map room functionality, cell " + cell);
			GameObject obj = UWE.Utils.InstantiateWrap(mapRoomFunctionalityPrefab);
			obj.transform.SetParent(base.transform, worldPositionStays: false);
			obj.transform.SetPositionAndRotation(transform.position, transform.rotation);
		}
	}

	public MapRoomFunctionality GetMapRoomFunctionalityForCell(Int3 cell)
	{
		MapRoomFunctionality[] componentsInChildren = GetComponentsInChildren<MapRoomFunctionality>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (NormalizeCell(WorldToGrid(componentsInChildren[i].transform.position)) == cell)
			{
				return componentsInChildren[i];
			}
		}
		return null;
	}

	private void BuildConnectorGeometry(Int3 cell, int index)
	{
		Int3.Bounds bounds = new Int3.Bounds(cell, cell);
		bool flag = GetFace(index, BaseAddLadderGhost.ladderFaceDir) == FaceType.Ladder;
		if (IsCellUsed(index))
		{
			Piece piece = (flag ? Piece.ConnectorTubeWindow : Piece.ConnectorTube);
			Transform obj = SpawnPiece(piece, cell, Quaternion.Euler(0f, 90f, 0f));
			BaseDeconstructable.MakeCellDeconstructable(obj, bounds, TechType.BaseConnector);
			obj.tag = "MainPieceGeometry";
		}
		if (flag)
		{
			SpawnPiece(Piece.ConnectorLadder, cell, Quaternion.identity);
		}
		Direction[] verticalDirections = VerticalDirections;
		foreach (Direction direction in verticalDirections)
		{
			if (!IsFaceUsed(index, direction))
			{
				continue;
			}
			Int3 adjacent = GetAdjacent(cell, direction);
			int cellIndex = GetCellIndex(adjacent);
			CellType cell2 = GetCell(adjacent);
			bool flag2 = false;
			switch (cell2)
			{
			case CellType.Empty:
			case CellType.Room:
			case CellType.Foundation:
			case CellType.OccupiedByOtherCell:
			case CellType.RechargePlatform:
			case CellType.WallFoundationN:
			case CellType.WallFoundationW:
			case CellType.WallFoundationS:
			case CellType.WallFoundationE:
				flag2 = true;
				break;
			case CellType.Corridor:
				if (GetCorridorDef(cellIndex).piece == Piece.CorridorLShape || (isGlass[cellIndex] && direction == Direction.Below))
				{
					flag2 = true;
				}
				break;
			}
			if (flag2)
			{
				Quaternion rotation = ((direction == Direction.Above) ? Quaternion.Euler(180f, 0f, 0f) : Quaternion.identity);
				SpawnPiece(Piece.ConnectorCap, cell, rotation);
			}
		}
	}

	[SuppressMessage("Gendarme.Rules.Maintainability", "AvoidComplexMethodsRule")]
	private void BuildRoomGeometry(Int3 cell)
	{
		Int3 @int = CellSize[1];
		Int3.Bounds bounds = new Int3.Bounds(cell, cell + @int - 1);
		BaseDeconstructable baseDeconstructable = null;
		Vector3 position = Int3.Scale(@int - Int3.one, halfCellSize);
		Quaternion identity = Quaternion.identity;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		if (GetCellMask(cell))
		{
			Transform obj = SpawnPiece(Piece.Room, cell, position, identity);
			baseDeconstructable = BaseDeconstructable.MakeCellDeconstructable(obj, bounds, TechType.BaseRoom);
			obj.tag = "MainPieceGeometry";
			Int3 adjacent = GetAdjacent(cell, Direction.Above);
			Int3 adjacent2 = GetAdjacent(cell, Direction.Below);
			flag2 = GetFace(new Face(cell, Direction.Above)) == FaceType.GlassDome;
			flag = CompareCellTypes(adjacent, @int, CellType.Room) || GetFace(new Face(cell, Direction.Above)) != FaceType.Solid;
			flag3 = CompareCellTypes(adjacent2, @int, CellType.Room) || GetFace(new Face(cell, Direction.Below)) != FaceType.Solid;
			if (!isGhost || GetFace(new Face(cell, Direction.Below)) == FaceType.Solid)
			{
				Piece piece = Piece.Invalid;
				if (CompareCellTypes(adjacent2, @int, sFoundationCheckTypes, hasAny: true))
				{
					piece = Piece.RoomExteriorFoundationBottom;
				}
				else if (!CompareCellTypes(adjacent2, @int, CellType.Room))
				{
					piece = Piece.RoomExteriorBottom;
					SpawnPiece(Piece.RoomAdjustableSupport, cell, position, identity, null, baseDeconstructable);
				}
				BaseExplicitFace.MakeFaceDeconstructable(SpawnPiece(piece, cell, position, identity, null, baseDeconstructable), new Face(cell, Direction.Below), baseDeconstructable);
			}
		}
		for (int i = 0; i < roomFaces.Length; i++)
		{
			RoomFace roomFace = roomFaces[i];
			Face face = new Face(cell + roomFace.offset, roomFace.direction);
			if (!GetFaceMask(face))
			{
				continue;
			}
			FaceType face2 = GetFace(face);
			bool num = IsCentralRoomCell(face.cell);
			bool flag4 = false;
			bool flag5 = false;
			bool flag6 = false;
			if (num)
			{
				Face face3 = new Face(face.cell, Direction.Below);
				flag4 = GetFaceMask(face3) && GetFace(face3) == FaceType.WaterPark;
				Face face4 = new Face(face.cell + new Int3(0, 1, 0), Direction.Below);
				flag5 = flag && GetFaceMask(face4) && GetFace(face4) == FaceType.WaterPark;
				Face face5 = new Face(face.cell + new Int3(0, -1, 0), Direction.Below);
				flag6 = flag3 && GetFaceMask(face5) && GetFace(face5) == FaceType.WaterPark;
				if (GetCellMask(cell))
				{
					if (face.direction == Direction.Above)
					{
						Piece piece2 = (flag2 ? Piece.RoomInteriorTopGlass : (flag4 ? Piece.RoomInteriorTopHole : Piece.RoomInteriorTop));
						Transform obj2 = SpawnPiece(piece2, cell, position, identity, null, baseDeconstructable);
						obj2.localPosition = Int3.Scale(roomFace.offset, cellSize) + roomFace.localOffset;
						BaseExplicitFace.MakeFaceDeconstructable(obj2, new Face(cell, Direction.Above), baseDeconstructable);
					}
					if (face.direction == Direction.Below)
					{
						Transform obj3 = SpawnPiece(flag6 ? Piece.RoomInteriorBottomHole : Piece.RoomInteriorBottom, cell, position, identity, null, baseDeconstructable);
						obj3.localPosition = Int3.Scale(roomFace.offset, cellSize) + roomFace.localOffset;
						BaseExplicitFace.MakeFaceDeconstructable(obj3, new Face(cell, Direction.Below), baseDeconstructable);
					}
				}
			}
			if ((face2 == FaceType.Solid && roomFace.direction == Direction.Above && flag2) || (face2 == FaceType.Solid && roomFace.direction == Direction.Below && flag6))
			{
				continue;
			}
			Piece piece3 = GetRoomPiece(face, face2, flag);
			if (piece3 == Piece.Invalid)
			{
				continue;
			}
			if (piece3 == Piece.RoomBioReactor && flag2)
			{
				piece3 = Piece.RoomBioReactorUnderDome;
			}
			if (piece3 == Piece.RoomNuclearReactor && flag2)
			{
				piece3 = Piece.RoomNuclearReactorUnderDome;
			}
			if (piece3 == Piece.RoomCoverSide && i % 2 == 1)
			{
				piece3 = Piece.RoomCoverSideVariant;
			}
			Transform transform = SpawnPiece(piece3, cell, roomFace.rotation, null, baseDeconstructable);
			transform.localPosition = Int3.Scale(roomFace.offset, cellSize) + roomFace.localOffset;
			if (isGhost)
			{
				continue;
			}
			if (face2 != FaceType.Solid)
			{
				TechType recipe = FaceToRecipe[(uint)face2];
				BaseDeconstructable baseDeconstructable2 = BaseDeconstructable.MakeFaceDeconstructable(transform, bounds, face, face2, recipe);
				transform.GetComponentsInChildren(includeInactive: true, sBaseModulesGeometry);
				int j = 0;
				for (int count = sBaseModulesGeometry.Count; j < count; j++)
				{
					sBaseModulesGeometry[j].geometryFace = face;
				}
				sBaseModulesGeometry.Clear();
				switch (face2)
				{
				case FaceType.FiltrationMachine:
					baseDeconstructable2.LinkModule(new Face(face.cell - anchor, face.direction));
					break;
				case FaceType.BioReactor:
					baseDeconstructable2.LinkModule(new Face(face.cell - anchor, face.direction));
					break;
				case FaceType.NuclearReactor:
					baseDeconstructable2.LinkModule(new Face(face.cell - anchor, face.direction));
					break;
				case FaceType.WaterPark:
					if (face.direction == Direction.Below)
					{
						Piece piece4 = Piece.Invalid;
						piece4 = (flag5 ? Piece.RoomWaterParkCeilingMiddle : (flag2 ? Piece.RoomWaterParkCeilingGlassDome : ((!flag) ? Piece.RoomWaterParkCeilingTop : Piece.RoomWaterParkCeilingGlass)));
						SpawnPiece(piece4, cell, baseDeconstructable).localPosition = Int3.Scale(roomFace.offset, cellSize) + roomFace.localOffset;
						piece4 = (flag6 ? Piece.RoomWaterParkFloorMiddle : Piece.RoomWaterParkFloorBottom);
						SpawnPiece(piece4, cell, baseDeconstructable).localPosition = Int3.Scale(roomFace.offset, cellSize) + roomFace.localOffset;
						if (!flag6)
						{
							baseDeconstructable2.LinkModule(new Face(face.cell - anchor, face.direction));
							WaterPark.GetWaterParkModule(this, face.cell, spawnIfNull: true).Rebuild(this, face.cell);
						}
					}
					break;
				}
			}
			else
			{
				BaseExplicitFace.MakeFaceDeconstructable(transform, face, baseDeconstructable);
			}
		}
	}

	[SuppressMessage("Gendarme.Rules.Maintainability", "AvoidComplexMethodsRule")]
	[SuppressMessage("Gendarme.Rules.Performance", "AvoidLargeNumberOfLocalVariablesRule")]
	private void BuildLargeRoomGeometry(Int3 cell, CellType cellType)
	{
		Int3 @int = CellSize[(uint)cellType];
		Int3.Bounds bounds = new Int3.Bounds(cell, cell + @int - 1);
		float y = ((cellType == CellType.LargeRoomRotated) ? (-90) : 0);
		Quaternion rotation = Quaternion.Euler(0f, y, 0f);
		BaseDeconstructable baseDeconstructable = null;
		Vector3 position = Int3.Scale(CellSize[(uint)cellType] - Int3.one, halfCellSize);
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int index;
		int index2;
		RoomFace[] array;
		if (cellType == CellType.LargeRoomRotated)
		{
			index = 2;
			index2 = 0;
			array = largeRoomRotatedFaces;
		}
		else
		{
			index = 0;
			index2 = 2;
			array = largeRoomFaces;
		}
		num = GetLargeRoomWaterParks(cell);
		num3 = GetLargeRoomWaterParks(cell + DirectionOffset[4]);
		num2 = GetLargeRoomWaterParks(cell + DirectionOffset[5]);
		flag4 = AreAlignedWaterParks(num, num2);
		if (GetCellMask(cell))
		{
			Transform obj = SpawnPiece(Piece.LargeRoom, cell, position, rotation);
			baseDeconstructable = BaseDeconstructable.MakeCellDeconstructable(obj, bounds, TechType.BaseLargeRoom);
			obj.tag = "MainPieceGeometry";
			Int3 adjacent = GetAdjacent(cell, Direction.Above);
			Int3 adjacent2 = GetAdjacent(cell, Direction.Below);
			flag2 = GetFace(new Face(cell, Direction.Above)) == FaceType.LargeGlassDome;
			flag = CompareCellTypes(adjacent, @int, cellType) || GetFace(new Face(cell, Direction.Above)) != FaceType.Solid;
			flag3 = CompareCellTypes(adjacent2, @int, cellType) || GetFace(new Face(cell, Direction.Below)) != FaceType.Solid;
			if (!isGhost || GetFace(new Face(cell, Direction.Below)) == FaceType.Solid)
			{
				Piece piece = Piece.Invalid;
				if (CompareCellTypes(adjacent2, @int, sFoundationCheckTypes, hasAny: true))
				{
					piece = Piece.LargeRoomExteriorFoundationBottom;
				}
				else if (CompareCellTypes(adjacent2, @int, CellType.Empty))
				{
					piece = Piece.LargeRoomExteriorBottom;
					SpawnPiece(Piece.LargeRoomAdjustableSupport, cell, position, rotation, null, baseDeconstructable);
				}
				BaseExplicitFace.MakeFaceDeconstructable(SpawnPiece(piece, cell, position, rotation, null, baseDeconstructable), new Face(cell, Direction.Below), baseDeconstructable);
			}
			Piece value;
			if (flag2)
			{
				value = Piece.LargeRoomInteriorTopGlass;
			}
			else if (!largeRoomInteriorTopPieces.TryGetValue(num, out value))
			{
				value = Piece.LargeRoomInteriorTop;
			}
			BaseExplicitFace.MakeFaceDeconstructable(SpawnPiece(value, cell, position, rotation, null, baseDeconstructable), new Face(cell, Direction.Above), baseDeconstructable);
			int key = (flag4 ? (num2 | num) : num);
			if (!largeRoomInteriorBottomPieces.TryGetValue(key, out var value2))
			{
				value2 = Piece.LargeRoomInteriorBottom;
			}
			BaseExplicitFace.MakeFaceDeconstructable(SpawnPiece(value2, cell, position, rotation, null, baseDeconstructable), new Face(cell, Direction.Below), baseDeconstructable);
		}
		for (int i = 0; i < array.Length; i++)
		{
			RoomFace roomFace = array[i];
			Face face = new Face(cell + roomFace.offset, roomFace.direction);
			if (!GetFaceMask(face))
			{
				continue;
			}
			FaceType face2 = GetFace(face);
			Face face3 = new Face(face.cell + DirectionOffset[5], Direction.Below);
			bool flag5 = flag3 && GetFaceMask(face3) && GetFace(face3) == FaceType.WaterPark;
			if ((face2 == FaceType.Solid && roomFace.direction == Direction.Above && flag2) || (face2 == FaceType.Solid && roomFace.direction == Direction.Below && flag5 && flag4))
			{
				continue;
			}
			Piece piece2 = ((cellType == CellType.LargeRoom) ? GetLargeRoomPiece(face, face2, flag) : GetLargeRoomRotatedPiece(face, face2, flag));
			if (piece2 == Piece.Invalid)
			{
				continue;
			}
			if (piece2 == Piece.RoomBioReactor && flag2)
			{
				piece2 = Piece.RoomBioReactorUnderDome;
			}
			if (piece2 == Piece.RoomNuclearReactor && flag2)
			{
				piece2 = Piece.RoomNuclearReactorUnderDome;
			}
			Transform transform = SpawnPiece(piece2, cell, roomFace.rotation, null, baseDeconstructable);
			transform.localPosition = Int3.Scale(roomFace.offset, cellSize) + roomFace.localOffset;
			if (isGhost)
			{
				continue;
			}
			if (face2 != FaceType.Solid)
			{
				TechType recipe = FaceToRecipe[(uint)face2];
				BaseDeconstructable baseDeconstructable2 = BaseDeconstructable.MakeFaceDeconstructable(transform, bounds, face, face2, recipe);
				transform.GetComponentsInChildren(includeInactive: true, sBaseModulesGeometry);
				int j = 0;
				for (int count = sBaseModulesGeometry.Count; j < count; j++)
				{
					sBaseModulesGeometry[j].geometryFace = face;
				}
				sBaseModulesGeometry.Clear();
				switch (face2)
				{
				case FaceType.FiltrationMachine:
					baseDeconstructable2.LinkModule(new Face(face.cell - anchor, face.direction));
					break;
				case FaceType.BioReactor:
					baseDeconstructable2.LinkModule(new Face(face.cell - anchor, face.direction));
					break;
				case FaceType.NuclearReactor:
					baseDeconstructable2.LinkModule(new Face(face.cell - anchor, face.direction));
					break;
				}
			}
			else
			{
				BaseExplicitFace.MakeFaceDeconstructable(transform, face, baseDeconstructable);
			}
		}
		for (int k = ((num == 6) ? 1 : 0); k < 3; k += 2)
		{
			if (((num >> k) & 3) != 3)
			{
				continue;
			}
			Int3 int2 = default(Int3);
			int2[index] = 1 + k;
			int2[index2] = 1;
			Vector3 localPosition = Int3.Scale(int2, cellSize);
			Transform transform2 = SpawnPiece(Piece.LargeRoomWaterParkWalls, cell, rotation, null, baseDeconstructable);
			transform2.localPosition = localPosition;
			int num4 = (num3 >> k) & 3;
			int num5 = (num2 >> k) & 3;
			bool num6 = num4 == 3 && AreAlignedWaterParks(num, num3);
			bool flag6 = num5 == 3 && AreAlignedWaterParks(num, num2);
			Piece piece = (num6 ? Piece.LargeRoomWaterParkCeilingMiddle : (flag2 ? Piece.LargeRoomWaterParkCeilingGlassDome : ((!flag) ? Piece.LargeRoomWaterParkCeilingTop : waterParkCeilingPieces[num4])));
			SpawnPiece(piece, cell, rotation, null, baseDeconstructable).localPosition = localPosition;
			piece = (flag6 ? Piece.LargeRoomWaterParkFloorMiddle : waterParkFloorPieces[num5]);
			SpawnPiece(piece, cell, rotation, null, baseDeconstructable).localPosition = localPosition;
			if (!isGhost)
			{
				Face face4 = new Face(cell + int2, Direction.Below);
				FaceType faceType = FaceType.WaterPark;
				TechType recipe2 = FaceToRecipe[(uint)faceType];
				BaseDeconstructable baseDeconstructable3 = BaseDeconstructable.MakeFaceDeconstructable(transform2, bounds, face4, faceType, recipe2);
				transform2.GetComponentsInChildren(includeInactive: true, sBaseModulesGeometry);
				int l = 0;
				for (int count2 = sBaseModulesGeometry.Count; l < count2; l++)
				{
					sBaseModulesGeometry[l].geometryFace = face4;
				}
				sBaseModulesGeometry.Clear();
				baseDeconstructable3.LinkModule(new Face(face4.cell - anchor, face4.direction));
				LargeRoomWaterPark largeRoomWaterPark = WaterPark.GetWaterParkModule(this, face4.cell, spawnIfNull: true) as LargeRoomWaterPark;
				if (largeRoomWaterPark != null)
				{
					largeRoomWaterPark.Rebuild(this, face4.cell);
				}
			}
		}
		using (Int3.RangeEnumerator rangeEnumerator = new Int3.RangeEnumerator(Int3.zero, @int - 1))
		{
			while (rangeEnumerator.MoveNext())
			{
				Int3 current = rangeEnumerator.Current;
				int num7 = 0;
				bool flag7 = false;
				Direction direction = Direction.North;
				Direction[] horizontalDirections = HorizontalDirections;
				foreach (Direction direction2 in horizontalDirections)
				{
					Face face5 = new Face(cell + current, direction2);
					if (GetFaceMask(face5))
					{
						switch (GetFace(face5))
						{
						case FaceType.Partition:
							num7 |= 1 << (int)direction2;
							break;
						case FaceType.PartitionDoor:
							flag7 = true;
							direction = direction2;
							break;
						}
					}
				}
				int maskRotation;
				Partition partition = GetPartition(cellType, current, num7, flag7, out maskRotation);
				Transform door;
				if (flag7)
				{
					Transform transform3 = SpawnPiece(Piece.PartitionDoor, cell, Int3.Scale(current, cellSize) + partition.doorOffset, partition.doorRotation, null, baseDeconstructable);
					if (isGhost)
					{
						BasePartitionDoor component = transform3.GetComponent<BasePartitionDoor>();
						if (component != null)
						{
							door = component.door;
							Vector3 localScale = door.localScale;
							localScale.x = 1.7f;
							door.localScale = localScale;
						}
					}
					if (isGhost)
					{
						partition.piece = Piece.Invalid;
					}
					else
					{
						BaseDeconstructable.MakeFaceDeconstructable(transform3, bounds, new Face(cell + current, direction), FaceType.PartitionDoor, TechType.BasePartitionDoor);
					}
				}
				if (partition.piece == Piece.Invalid)
				{
					continue;
				}
				Vector3 position2 = Int3.Scale(current, cellSize);
				door = SpawnPiece(partition.piece, cell, position2, partition.pieceRotation, null, baseDeconstructable);
				if (isGhost || flag7)
				{
					continue;
				}
				door.GetComponentsInChildren(includeInactive: true, sDeconstructables);
				for (int n = 0; n < sDeconstructables.Count; n++)
				{
					BaseDeconstructable baseDeconstructable4 = sDeconstructables[n];
					Direction hintDirection = baseDeconstructable4.hintDirection;
					if (hintDirection != Direction.Count)
					{
						hintDirection = (Direction)rotationCCWRemap[(int)(maskRotation * 4 + hintDirection)];
						baseDeconstructable4.Init(bounds, new Face(cell + current, hintDirection), FaceType.Partition, TechType.BasePartition);
					}
				}
				sDeconstructables.Clear();
			}
		}
	}

	private void BuildMoonpoolGeometry(Int3 cell, CellType cellType)
	{
		Int3 @int = CellSize[(uint)cellType];
		Int3.Bounds bounds = new Int3.Bounds(cell, cell + @int - 1);
		bool flag = cellType == CellType.MoonpoolRotated;
		BaseDeconstructable parent = null;
		Vector3 position = Int3.Scale(@int - Int3.one, halfCellSize);
		Quaternion rotation = Quaternion.Euler(0f, flag ? (-90) : 0, 0f);
		if (GetCellMask(cell))
		{
			Transform obj = SpawnPiece(Piece.Moonpool, cell, position, rotation);
			parent = BaseDeconstructable.MakeCellDeconstructable(obj, bounds, TechType.BaseMoonpool);
			obj.tag = "MainPieceGeometry";
		}
		RoomFace[] array = (flag ? moonpoolRotatedFaces : moonpoolFaces);
		for (int i = 0; i < array.Length; i++)
		{
			RoomFace roomFace = array[i];
			Face face = new Face(cell + roomFace.offset, roomFace.direction);
			if (!GetFaceMask(face))
			{
				continue;
			}
			FaceType face2 = GetFace(face);
			Piece piece = (flag ? GetMoonpoolRotatedPiece(face, face2) : GetMoonpoolPiece(face, face2));
			if (piece == Piece.Invalid)
			{
				continue;
			}
			Transform transform = SpawnPiece(piece, cell, roomFace.rotation);
			transform.localPosition = Int3.Scale(roomFace.offset, cellSize) + roomFace.localOffset;
			if (face2 != FaceType.Solid)
			{
				TechType recipe = FaceToRecipe[(uint)face2];
				BaseDeconstructable baseDeconstructable = BaseDeconstructable.MakeFaceDeconstructable(transform, bounds, face, face2, recipe);
				if (!isGhost)
				{
					transform.GetComponentsInChildren(includeInactive: true, sBaseModulesGeometry);
					int j = 0;
					for (int count = sBaseModulesGeometry.Count; j < count; j++)
					{
						sBaseModulesGeometry[j].geometryFace = face;
					}
					sBaseModulesGeometry.Clear();
					if (face2 == FaceType.UpgradeConsole)
					{
						baseDeconstructable.LinkModule(new Face(face.cell - anchor, face.direction));
					}
				}
			}
			else if (!isGhost)
			{
				BaseExplicitFace.MakeFaceDeconstructable(transform, face, parent);
			}
		}
	}

	public CellType GetCellType(Int3 cell)
	{
		return cells[baseShape.GetIndex(cell)];
	}

	private void BuildGeometryForCell(Int3 cell)
	{
		if (!IsValidCell(cell))
		{
			return;
		}
		int index = baseShape.GetIndex(cell);
		if (index != -1)
		{
			Transform transform = cellObjects[index];
			if ((bool)transform)
			{
				transform.GetComponent<BaseCell>().cell = cell;
			}
			CellType cellType = cells[index];
			switch (cellType)
			{
			case CellType.Foundation:
				BuildFoundationGeometry(cell);
				break;
			case CellType.WallFoundationN:
			case CellType.WallFoundationW:
			case CellType.WallFoundationS:
			case CellType.WallFoundationE:
				BuildWallFoundationGeometry(cell, cellType);
				break;
			case CellType.RechargePlatform:
				BuildRechargePlatformGeometry(cell);
				break;
			case CellType.Corridor:
				BuildCorridorGeometry(cell, index);
				break;
			case CellType.Connector:
				BuildConnectorGeometry(cell, index);
				break;
			case CellType.Room:
				BuildRoomGeometry(cell);
				break;
			case CellType.Moonpool:
			case CellType.MoonpoolRotated:
				BuildMoonpoolGeometry(cell, cellType);
				break;
			case CellType.Observatory:
				BuildObservatoryGeometry(cell);
				break;
			case CellType.MapRoom:
			case CellType.MapRoomRotated:
				BuildMapRoomGeometry(cell, index, cellType);
				break;
			case CellType.LargeRoom:
			case CellType.LargeRoomRotated:
				BuildLargeRoomGeometry(cell, cellType);
				break;
			case CellType.OccupiedByOtherCell:
			case CellType.ControlRoom:
			case CellType.ControlRoomRotated:
				break;
			}
		}
	}

	private void BuildTouchedCells()
	{
		foreach (Int3 touchedCell in touchedCells)
		{
			BuildGeometryForCell(touchedCell);
		}
		if (!isGhost)
		{
			Debug.Log("rebuilt " + touchedCells.Count + " cells");
		}
		touchedCells.Clear();
	}

	private void UpdateDeconstructables()
	{
		foreach (Int3 allCell in AllCells)
		{
			if (!(allCell == NormalizeCell(allCell)))
			{
				continue;
			}
			int cellIndex = GetCellIndex(allCell);
			Transform transform = cellObjects[cellIndex];
			CellType cellType = cells[cellIndex];
			if (cellType == CellType.Empty || cellType == CellType.OccupiedByOtherCell || !transform)
			{
				continue;
			}
			BaseCell component = transform.GetComponent<BaseCell>();
			if (component.cell != allCell)
			{
				Int3 @int = allCell - component.cell;
				BaseDeconstructable[] componentsInChildren = transform.GetComponentsInChildren<BaseDeconstructable>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].ShiftCell(@int);
				}
				BaseExplicitFace[] componentsInChildren2 = transform.GetComponentsInChildren<BaseExplicitFace>();
				for (int i = 0; i < componentsInChildren2.Length; i++)
				{
					componentsInChildren2[i].ShiftCell(@int);
				}
				IBaseModuleGeometry[] componentsInChildren3 = transform.GetComponentsInChildren<IBaseModuleGeometry>();
				foreach (IBaseModuleGeometry obj in componentsInChildren3)
				{
					Face geometryFace = obj.geometryFace;
					geometryFace.cell += @int;
					obj.geometryFace = geometryFace;
				}
				component.cell = allCell;
			}
		}
	}

	private void BuildGeometry()
	{
		foreach (Int3 allCell in AllCells)
		{
			BuildGeometryForCell(allCell);
		}
		if (!isGhost)
		{
			Debug.Log("rebuilt all cells");
		}
	}

	public GameObject SpawnModule(GameObject prefab, Face face)
	{
		CellType cell = GetCell(face.cell);
		GameObject gameObject = null;
		switch (cell)
		{
		case CellType.Room:
			gameObject = SpawnRoomModule(prefab, face);
			break;
		case CellType.Moonpool:
			gameObject = SpawnMoonpoolModule(prefab, face);
			break;
		case CellType.MoonpoolRotated:
			gameObject = SpawnMoonpoolRotatedModule(prefab, face);
			break;
		case CellType.LargeRoom:
			gameObject = SpawnLargeRoomModule(prefab, face);
			break;
		case CellType.LargeRoomRotated:
			gameObject = SpawnLargeRoomRotatedModule(prefab, face);
			break;
		case CellType.ControlRoom:
		case CellType.ControlRoomRotated:
			gameObject = SpawnControlRoomModule(prefab, face);
			break;
		}
		if (gameObject != null)
		{
			IBaseModule component = gameObject.GetComponent<IBaseModule>();
			if (component != null)
			{
				component.moduleFace = new Face(face.cell - anchor, face.direction);
			}
		}
		return gameObject;
	}

	private GameObject SpawnRoomModule(GameObject prefab, Face face)
	{
		if (GetRoomFaceLocation(face, out var position, out var rotation))
		{
			GameObject obj = UnityEngine.Object.Instantiate(prefab, position, rotation);
			obj.transform.SetParent(base.transform, worldPositionStays: false);
			return obj;
		}
		return null;
	}

	private GameObject SpawnMoonpoolModule(GameObject prefab, Face face)
	{
		if (GetMoonpoolFaceLocation(face, out var position, out var rotation))
		{
			GameObject obj = UnityEngine.Object.Instantiate(prefab, position, rotation);
			obj.transform.SetParent(base.transform, worldPositionStays: false);
			return obj;
		}
		return null;
	}

	private GameObject SpawnMoonpoolRotatedModule(GameObject prefab, Face face)
	{
		if (GetMoonpoolRotatedFaceLocation(face, out var position, out var rotation))
		{
			GameObject obj = UnityEngine.Object.Instantiate(prefab, position, rotation);
			obj.transform.SetParent(base.transform, worldPositionStays: false);
			return obj;
		}
		return null;
	}

	private GameObject SpawnLargeRoomModule(GameObject prefab, Face face)
	{
		if (GetLargeRoomFaceLocation(face, out var position, out var rotation))
		{
			GameObject obj = UnityEngine.Object.Instantiate(prefab, position, rotation);
			obj.transform.SetParent(base.transform, worldPositionStays: false);
			return obj;
		}
		return null;
	}

	private GameObject SpawnLargeRoomRotatedModule(GameObject prefab, Face face)
	{
		if (GetLargeRoomRotatedFaceLocation(face, out var position, out var rotation))
		{
			GameObject obj = UnityEngine.Object.Instantiate(prefab, position, rotation);
			obj.transform.SetParent(base.transform, worldPositionStays: false);
			return obj;
		}
		return null;
	}

	private GameObject SpawnControlRoomModule(GameObject prefab, Face face)
	{
		CellType cell = GetCell(face.cell);
		Vector3 vector = Int3.Scale(CellSize[(uint)cell] - Int3.one, halfCellSize);
		Vector3 position = GridToLocal(NormalizeCell(face.cell)) + vector;
		float y = ((cell == CellType.ControlRoomRotated) ? 90 : 0);
		Quaternion rotation = Quaternion.Euler(0f, y, 0f);
		GameObject obj = UnityEngine.Object.Instantiate(prefab, position, rotation);
		obj.transform.SetParent(base.transform, worldPositionStays: false);
		return obj;
	}

	private bool GetRoomFaceLocation(Face face, out Vector3 position, out Quaternion rotation)
	{
		int index = baseShape.GetIndex(face.cell);
		Int3 @int = NormalizeCell(face.cell);
		Direction direction = NormalizeFaceDirection(index, face.direction);
		for (int i = 0; i < roomFaces.Length; i++)
		{
			RoomFace roomFace = roomFaces[i];
			if (!(@int + roomFace.offset != face.cell) && roomFace.direction == direction)
			{
				position = GridToLocal(@int + roomFace.offset) + roomFace.localOffset;
				rotation = roomFace.rotation;
				return true;
			}
		}
		position = Vector3.zero;
		rotation = Quaternion.identity;
		return false;
	}

	private bool GetMoonpoolFaceLocation(Face face, out Vector3 position, out Quaternion rotation)
	{
		int index = baseShape.GetIndex(face.cell);
		Int3 @int = NormalizeCell(face.cell);
		Direction direction = NormalizeFaceDirection(index, face.direction);
		for (int i = 0; i < moonpoolFaces.Length; i++)
		{
			RoomFace roomFace = moonpoolFaces[i];
			if (!(@int + roomFace.offset != face.cell) && roomFace.direction == direction)
			{
				position = GridToLocal(@int + roomFace.offset) + roomFace.localOffset;
				rotation = roomFace.rotation;
				return true;
			}
		}
		position = Vector3.zero;
		rotation = Quaternion.identity;
		return false;
	}

	private bool GetMoonpoolRotatedFaceLocation(Face face, out Vector3 position, out Quaternion rotation)
	{
		int index = baseShape.GetIndex(face.cell);
		Int3 @int = NormalizeCell(face.cell);
		Direction direction = NormalizeFaceDirection(index, face.direction);
		for (int i = 0; i < moonpoolRotatedFaces.Length; i++)
		{
			RoomFace roomFace = moonpoolRotatedFaces[i];
			if (!(@int + roomFace.offset != face.cell) && roomFace.direction == direction)
			{
				position = GridToLocal(@int + roomFace.offset) + roomFace.localOffset;
				rotation = roomFace.rotation;
				return true;
			}
		}
		position = Vector3.zero;
		rotation = Quaternion.identity;
		return false;
	}

	private bool GetLargeRoomFaceLocation(Face face, out Vector3 position, out Quaternion rotation)
	{
		int index = baseShape.GetIndex(face.cell);
		Int3 @int = NormalizeCell(face.cell);
		Direction direction = NormalizeFaceDirection(index, face.direction);
		for (int i = 0; i < largeRoomFaces.Length; i++)
		{
			RoomFace roomFace = largeRoomFaces[i];
			if (!(@int + roomFace.offset != face.cell) && roomFace.direction == direction)
			{
				position = GridToLocal(@int + roomFace.offset) + roomFace.localOffset;
				rotation = roomFace.rotation;
				return true;
			}
		}
		position = Vector3.zero;
		rotation = Quaternion.identity;
		return false;
	}

	private bool GetLargeRoomRotatedFaceLocation(Face face, out Vector3 position, out Quaternion rotation)
	{
		int index = baseShape.GetIndex(face.cell);
		Int3 @int = NormalizeCell(face.cell);
		Direction direction = NormalizeFaceDirection(index, face.direction);
		for (int i = 0; i < largeRoomRotatedFaces.Length; i++)
		{
			RoomFace roomFace = largeRoomRotatedFaces[i];
			if (!(@int + roomFace.offset != face.cell) && roomFace.direction == direction)
			{
				position = GridToLocal(@int + roomFace.offset) + roomFace.localOffset;
				rotation = roomFace.rotation;
				return true;
			}
		}
		position = Vector3.zero;
		rotation = Quaternion.identity;
		return false;
	}

	private void Awake()
	{
		bool num = cells != null;
		Initialize();
		AllocateArrays();
		BaseGhost component = GetComponent<BaseGhost>();
		isGhost = component != null;
		if (num)
		{
			waitingForWorld = true;
			nextWorldPollTime = Time.time + 2f;
		}
		else
		{
			BuildGeometry();
		}
		RecalculateFlowData();
	}

	private void OnGlobalEntitiesLoaded()
	{
		FinishDeserialization();
	}

	private void FinishDeserialization()
	{
		if (deserializationFinished)
		{
			return;
		}
		Initialize();
		AllocateArrays();
		BindCellObjects();
		RecalculateFlowData();
		if (!isGhost)
		{
			foreach (BaseGhost ghost in ghosts)
			{
				Base ghostBase = ghost.GhostBase;
				if (ghostBase != null)
				{
					ghostBase.FinishDeserialization();
				}
			}
		}
		RebuildGeometry();
		DestroyIfEmpty();
		deserializationFinished = true;
	}

	private void RecomputeOccupiedBounds()
	{
		occupiedBounds = default(Bounds);
		bool flag = false;
		foreach (Int3 allCell in AllCells)
		{
			if (!IsCellEmpty(allCell))
			{
				Vector3 vector = GridToWorld(allCell);
				if (!flag)
				{
					occupiedBounds = new Bounds(vector, Vector3.zero);
					flag = true;
				}
				else
				{
					occupiedBounds.Encapsulate(vector);
				}
			}
		}
		if (!isGhost)
		{
			BaseRoot component = GetComponent<BaseRoot>();
			component.LOD.SetUseBoundsForDistanceChecks(occupiedBounds);
			component.LOD.UseBoundingBoxForVisibility = true;
		}
	}

	private void BuildAccessoryGeometry()
	{
		if (isGhost)
		{
			IBaseAccessoryGeometry[] componentsInChildren = GetComponentsInChildren<IBaseAccessoryGeometry>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].BuildGeometry(this, !isPlaced);
			}
			return;
		}
		int num = 0;
		if (currentAccessoryGeoIndex < 0)
		{
			currentAccessoryGeoIndex = rebuildAccessoryGeometry.Count - 1;
		}
		currentAccessoryGeoIndex = Mathf.Min(currentAccessoryGeoIndex, rebuildAccessoryGeometry.Count - 1);
		if (!LargeWorldStreamer.main.IsWorldSettled())
		{
			return;
		}
		for (int num2 = currentAccessoryGeoIndex; num2 > -1; num2--)
		{
			if (rebuildAccessoryGeometry[num2].obj == null)
			{
				rebuildAccessoryGeometry.RemoveAt(num2);
			}
			else if ((MainCameraControl.main.transform.position - rebuildAccessoryGeometry[num2].obj.transform.position).sqrMagnitude <= 625f)
			{
				rebuildAccessoryGeometry[num2].accessory.BuildGeometry(this, disableColliders: false);
				rebuildAccessoryGeometry.RemoveAt(num2);
			}
			num++;
			currentAccessoryGeoIndex = num2 - 1;
			if (num > 10)
			{
				num = 0;
				break;
			}
		}
	}

	private void Update()
	{
		BuildAccessoryGeometry();
		UpdateVisibility();
		if (autobuilding)
		{
			AutoBuild();
		}
	}

	private void HideOverlappedRenderers(GameObject obj)
	{
		Renderer[] componentsInChildren = obj.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			hiddenRenderers.Add(renderer);
			renderer.enabled = false;
		}
		hiddenObjects.Add(obj);
	}

	private void DisableOverlappedObject(GameObject obj)
	{
		obj.SetActive(value: false);
		hiddenObjects.Add(obj);
	}

	public void UpdateVisibility()
	{
		foreach (GameObject hiddenObject in hiddenObjects)
		{
			if ((bool)hiddenObject)
			{
				hiddenObject.SetActive(value: true);
			}
		}
		foreach (Renderer hiddenRenderer in hiddenRenderers)
		{
			if ((bool)hiddenRenderer)
			{
				hiddenRenderer.enabled = true;
			}
		}
		hiddenObjects.Clear();
		hiddenRenderers.Clear();
		if (isGhost)
		{
			return;
		}
		if ((bool)placementGhost)
		{
			if (placementGhost.TargetBase == this)
			{
				overlappedObjects.Clear();
				placementGhost.FindOverlappedObjects(overlappedObjects);
				foreach (GameObject overlappedObject in overlappedObjects)
				{
					HideOverlappedRenderers(overlappedObject);
				}
			}
			else
			{
				placementGhost = null;
			}
		}
		foreach (BaseGhost ghost in ghosts)
		{
			overlappedObjects.Clear();
			ghost.FindOverlappedObjects(overlappedObjects);
			foreach (GameObject overlappedObject2 in overlappedObjects)
			{
				DisableOverlappedObject(overlappedObject2);
			}
		}
	}

	public Int3 LocalToGrid(Vector3 localPoint)
	{
		int x = Mathf.RoundToInt(localPoint.x / cellSize.x) - cellOffset.x;
		int y = Mathf.RoundToInt(localPoint.y / cellSize.y) - cellOffset.y;
		int z = Mathf.RoundToInt(localPoint.z / cellSize.z) - cellOffset.z;
		return new Int3(x, y, z);
	}

	public Int3 WorldToGrid(Vector3 point)
	{
		Vector3 localPoint = WorldToLocal(point);
		return LocalToGrid(localPoint);
	}

	public Vector3 WorldToLocal(Vector3 point)
	{
		return base.transform.InverseTransformPoint(point);
	}

	public Vector3 WorldToLocalGrid(Vector3 point)
	{
		Vector3 vector = base.transform.InverseTransformPoint(point);
		return new Vector3(vector.x / cellSize.x - (float)cellOffset.x, vector.y / cellSize.y - (float)cellOffset.y, vector.z / cellSize.z - (float)cellOffset.z);
	}

	public Ray WorldToLocalRay(Ray ray)
	{
		return new Ray(base.transform.InverseTransformPoint(ray.origin), base.transform.InverseTransformDirection(ray.direction));
	}

	public static Direction NormalToDirection(Vector3 normal)
	{
		Direction result = Direction.Below;
		float num = -1f;
		Direction[] allDirections = AllDirections;
		foreach (Direction direction in allDirections)
		{
			float num2 = Vector3.Dot(DirectionNormals[(int)direction], normal);
			if (num2 > num)
			{
				num = num2;
				result = direction;
			}
		}
		return result;
	}

	public Vector3 GridToLocal(Int3 cell)
	{
		return Int3.Scale(cell + cellOffset, cellSize);
	}

	public bool GridToWorld(Int3 cell, Vector3 uvw, out Vector3 result)
	{
		if (!IsCellValid(cell))
		{
			result = Vector3.zero;
			return false;
		}
		Vector3 vector = Vector3.Scale(uvw - new Vector3(0.5f, 0.5f, 0.5f), cellSize);
		result = base.transform.TransformPoint(GridToLocal(cell) + vector);
		return true;
	}

	public Vector3 GridToWorld(Int3 cell)
	{
		return base.transform.TransformPoint(GridToLocal(cell));
	}

	private Vector3 GetFaceNormal(Direction direction)
	{
		return FaceNormals[(int)direction];
	}

	public Plane GetFacePlane(Face face)
	{
		Vector3 vector = GridToLocal(face.cell);
		Vector3 faceNormal = GetFaceNormal(face.direction);
		return new Plane(faceNormal, vector - Vector3.Scale(faceNormal, halfCellSize));
	}

	private Vector3 GetFaceLocalCenter(Face face)
	{
		Vector3 vector = GridToLocal(face.cell);
		Vector3 vector2 = Vector3.Scale(GetFaceNormal(face.direction), halfCellSize);
		return vector - vector2;
	}

	private bool GetNearestFace(Vector3 point, out Face face)
	{
		face = default(Face);
		Vector3 vector = base.transform.InverseTransformPoint(point);
		face.cell = LocalToGrid(vector);
		if (!IsCellValid(face.cell))
		{
			return false;
		}
		Vector3 vector2 = GridToLocal(face.cell);
		Vector3 vector3 = vector2 - halfCellSize;
		Vector3 vector4 = vector2 + halfCellSize;
		Vector3 vector5 = vector - vector3;
		Vector3 vector6 = vector4 - vector;
		Direction direction = Direction.West;
		float num = vector5.x;
		_ = Vector3.right;
		if (vector5.y < num)
		{
			direction = Direction.Below;
			num = vector5.y;
		}
		if (vector5.z < num)
		{
			direction = Direction.South;
			num = vector5.z;
		}
		if (vector6.x < num)
		{
			direction = Direction.East;
			num = vector6.x;
		}
		if (vector6.y < num)
		{
			direction = Direction.Above;
			num = vector6.y;
		}
		if (vector6.z < num)
		{
			direction = Direction.North;
			num = vector6.z;
		}
		face.direction = direction;
		return true;
	}

	private Int3 GetCellFromFaceIndex(int faceIndex)
	{
		int cellIndex = faceIndex / 6;
		return GetCellPointFromIndex(cellIndex);
	}

	private void CheckTouchedFaces()
	{
		for (int i = 0; i < baseShape.Size * 6; i++)
		{
			if (previousfaces[i] != faces[i])
			{
				Int3 cellFromFaceIndex = GetCellFromFaceIndex(i);
				TouchCell(cellFromFaceIndex);
			}
		}
	}

	public void RebuildGhostBases()
	{
		for (int i = 0; i < ghosts.Count; i++)
		{
			BaseGhost baseGhost = ghosts[i];
			baseGhost.AttachCorridorConnectors(disableColliders: false);
			baseGhost.RebuildGhostGeometry(disableCollison: false);
			ConstructableBase componentInParent = baseGhost.GetComponentInParent<ConstructableBase>();
			if (componentInParent != null)
			{
				componentInParent.OnGhostBasePostRebuildGeometry(baseGhost.GhostBase);
			}
		}
	}

	public void RebuildGeometry()
	{
		if (cellObjects != null)
		{
			Initialize();
			rebuildAccessoryGeometry.Clear();
			if (!isGhost)
			{
				CheckTouchedFaces();
			}
			if (touchedCells.Count == 0)
			{
				ClearGeometry();
			}
			else
			{
				CacheTouchedCells();
			}
			if (touchedCells.Count == 0)
			{
				BuildGeometry();
			}
			else
			{
				UpdateDeconstructables();
				BuildTouchedCells();
			}
			ClearCachedPieces();
			RecomputeOccupiedCells();
			RecomputeOccupiedBounds();
			StorePreviousFaces();
			if (isGhost)
			{
				BuildAccessoryGeometry();
			}
			else
			{
				StartCoroutine(CheckForAccessoriesAsync());
			}
			if (!isGhost)
			{
				RebuildGhostBases();
			}
			else
			{
				BuildAccessoryGeometry();
			}
			if (this.onPostRebuildGeometry != null)
			{
				this.onPostRebuildGeometry(this);
			}
		}
	}

	private IEnumerator CheckForAccessoriesAsync()
	{
		yield return null;
		foreach (Int3 allCell in AllCells)
		{
			Transform cellObject = GetCellObject(allCell);
			if (!cellObject)
			{
				continue;
			}
			foreach (Transform item in cellObject.transform)
			{
				IBaseAccessoryGeometry componentInChildren = item.GetComponentInChildren<IBaseAccessoryGeometry>();
				if (componentInChildren != null)
				{
					rebuildAccessoryGeometry.Add(new RebuildAccessoryGeometry(item.gameObject, componentInChildren));
				}
			}
		}
	}

	public void ClearCachedPieces()
	{
		foreach (CachedPiece cachedPiece in cachedPieces)
		{
			cachedPiece.obj.tag = "ToDestroy";
			Collider[] componentsInChildren = cachedPiece.obj.GetComponentsInChildren<Collider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].gameObject.tag = "ToDestroy";
			}
			UnityEngine.Object.Destroy(cachedPiece.obj);
		}
		cachedPieces.Clear();
	}

	private void CachePiece(GameObject obj, Int3 cell)
	{
		if (obj.name.Contains("Partition"))
		{
			UnityEngine.Object.Destroy(obj);
			return;
		}
		CachedPiece item = default(CachedPiece);
		item.obj = obj;
		item.obj.name = item.obj.name.Replace("(Clone)", "");
		item.cell = cell;
		cachedPieces.Add(item);
	}

	public void ClearGeometry()
	{
		if (cellObjects == null)
		{
			return;
		}
		for (int i = 0; i < cellObjects.Length; i++)
		{
			Transform transform = cellObjects[i];
			if ((bool)transform)
			{
				for (int num = transform.childCount - 1; num >= 0; num--)
				{
					Transform child = transform.GetChild(num);
					child.parent = null;
					UnityEngine.Object.Destroy(child.gameObject);
				}
			}
		}
	}

	public void CacheTouchedCells()
	{
		if (cellObjects == null)
		{
			return;
		}
		foreach (Int3 touchedCell in touchedCells)
		{
			int index = baseShape.GetIndex(touchedCell);
			if (index == -1)
			{
				continue;
			}
			Transform transform = cellObjects[index];
			if ((bool)transform)
			{
				for (int num = transform.childCount - 1; num >= 0; num--)
				{
					CachePiece(transform.GetChild(num).gameObject, touchedCell);
				}
			}
		}
	}

	private bool RaycastCellPlanes(Ray ray, Int3 cell, out Direction direction, out float distance)
	{
		distance = float.MaxValue;
		direction = Direction.Above;
		bool result = false;
		Vector3 vector = GridToLocal(cell);
		Direction[] allDirections = AllDirections;
		foreach (Direction direction2 in allDirections)
		{
			Vector3 vector2 = FaceNormals[(int)direction2];
			if (!(Vector3.Dot(vector2, ray.direction) >= 0f) && new Plane(vector2, vector - Vector3.Scale(vector2, halfCellSize)).Raycast(ray, out var enter) && enter < distance)
			{
				distance = enter;
				direction = direction2;
				result = true;
			}
		}
		return result;
	}

	private void DrawLocalStar(Vector3 point, Color color)
	{
		point = base.transform.TransformPoint(point);
		Debug.DrawLine(point - new Vector3(0.2f, 0f, 0f), point + new Vector3(0.2f, 0f, 0f), color);
		Debug.DrawLine(point - new Vector3(0f, 0.2f, 0f), point + new Vector3(0f, 0.2f, 0f), color);
		Debug.DrawLine(point - new Vector3(0f, 0f, 0.2f), point + new Vector3(0f, 0f, 0.2f), color);
	}

	private void DrawLocalStar(Vector3 point)
	{
		DrawLocalStar(point, Color.blue);
	}

	private void DrawFace(Face face)
	{
		Vector3 faceNormal = GetFaceNormal(face.direction);
		Vector3 vector = ((faceNormal.y == 0f) ? Vector3.up : Vector3.right);
		Vector3 a = Vector3.Cross(vector, faceNormal);
		vector = base.transform.TransformDirection(Vector3.Scale(vector, halfCellSize));
		a = base.transform.TransformDirection(Vector3.Scale(a, halfCellSize));
		Vector3 vector2 = base.transform.TransformPoint(GridToLocal(face.cell) - Vector3.Scale(faceNormal, halfCellSize));
		Debug.DrawLine(vector2 - vector - a, vector2 - vector + a);
		Debug.DrawLine(vector2 - vector + a, vector2 + vector + a);
		Debug.DrawLine(vector2 + vector + a, vector2 + vector - a);
		Debug.DrawLine(vector2 + vector - a, vector2 - vector - a);
	}

	private bool RaycastFace(Ray ray, out Face face)
	{
		Vector3 a = Int3.Scale(baseShape.ToInt3(), halfCellSize);
		Vector3 vector = (GridToLocal(new Int3(0)) + GridToLocal(baseShape.ToInt3() - 1)) * 0.5f;
		Direction[] allDirections = AllDirections;
		foreach (Direction direction in allDirections)
		{
			Vector3 vector2 = DirectionNormals[(int)direction];
			Plane plane = new Plane(vector2, vector + Vector3.Scale(a, vector2));
			if (Vector3.Dot(plane.normal, ray.direction) < 0f && plane.Raycast(ray, out var enter))
			{
				ray.origin += ray.direction * (enter - 0.1f);
			}
		}
		DrawLocalStar(ray.origin);
		Vector3 vector3 = base.transform.TransformPoint(ray.origin);
		float num = 0f;
		Int3 cell = LocalToGrid(ray.origin);
		bool flag = true;
		while (true)
		{
			if (!RaycastCellPlanes(ray, cell, out var direction2, out var distance))
			{
				face = default(Face);
				return false;
			}
			Int3 adjacent = GetAdjacent(cell, direction2);
			num += distance;
			if (!IsCellValid(cell) && !IsCellValid(adjacent) && !flag)
			{
				break;
			}
			if (IsCellEmpty(cell) != IsCellEmpty(adjacent) || GetFace(new Face(cell, direction2)) != 0)
			{
				face = new Face(cell, direction2);
				Vector3 end = base.transform.TransformPoint(ray.origin + ray.direction * num);
				Debug.DrawLine(vector3, end, Color.red);
				DrawFace(face);
				return true;
			}
			cell = adjacent;
			flag = false;
		}
		Debug.DrawLine(vector3, vector3 + base.transform.TransformDirection(ray.direction) * 20f, Color.black);
		face = default(Face);
		return false;
	}

	public void SetPlacementGhost(BaseGhost baseGhost)
	{
		if (baseGhost != placementGhost)
		{
			placementGhost = baseGhost;
		}
	}

	public bool PickFace(Transform camera, out Face face)
	{
		Ray ray = WorldToLocalRay(new Ray(camera.position, camera.forward));
		return RaycastFace(ray, out face);
	}

	public Vector3 GetClosestPoint(Vector3 position)
	{
		Vector3 result = Vector3.zero;
		GetClosestCell(position, out var _, out var worldPosition, out var _);
		int num = UWE.Utils.RaycastIntoSharedBuffer(position, Vector3.Normalize(worldPosition - position), (worldPosition - position).magnitude);
		float num2 = float.PositiveInfinity;
		for (int i = 0; i < num; i++)
		{
			RaycastHit raycastHit = UWE.Utils.sharedHitBuffer[i];
			if (!raycastHit.collider.isTrigger && UWE.Utils.IsAncestorOf(base.gameObject, raycastHit.collider.gameObject) && num2 > raycastHit.distance)
			{
				result = raycastHit.point;
				num2 = raycastHit.distance;
			}
		}
		return result;
	}

	public void GetClosestCell(Vector3 position, out Int3 cell, out Vector3 worldPosition, out float distance)
	{
		cell = Int3.zero;
		worldPosition = Vector3.zero;
		distance = float.PositiveInfinity;
		foreach (int occupiedCellIndex in occupiedCellIndexes)
		{
			if (cells[occupiedCellIndex] != CellType.OccupiedByOtherCell)
			{
				Int3 cellPointFromIndex = GetCellPointFromIndex(occupiedCellIndex);
				Vector3 vector = GridToWorld(cellPointFromIndex);
				Int3 @int = CellSize[(uint)GetCell(cellPointFromIndex)];
				Vector3 vector2 = (GridToWorld(cellPointFromIndex + @int - 1) - vector) * 0.5f + vector;
				float sqrMagnitude = (vector2 - position).sqrMagnitude;
				if (sqrMagnitude < distance)
				{
					distance = sqrMagnitude;
					worldPosition = vector2;
					cell = cellPointFromIndex;
				}
			}
		}
		distance = Mathf.Sqrt(distance);
	}

	public Int3 PickCell(Transform camera, Vector3 point, Int3 size)
	{
		Ray ray = WorldToLocalRay(new Ray(camera.position, camera.forward));
		Int3 @int = size - 1;
		Vector3 vector = Int3.Scale(@int, halfCellSize);
		Int3 int2 = new Int3(@int.x / 2, @int.y / 2, @int.z / 2);
		Int3 int3 = @int - int2 * 2;
		Int3 int4;
		if (!RaycastFace(ray, out var face))
		{
			Vector3 vector2 = base.transform.InverseTransformPoint(point);
			int4 = LocalToGrid(vector2 - vector);
		}
		else
		{
			int4 = face.cell - int2;
			Plane facePlane = GetFacePlane(face);
			if (facePlane.Raycast(ray, out var enter))
			{
				Vector3 vector3 = ray.origin + ray.direction * enter;
				DrawLocalStar(vector3, Color.red);
				Vector3 vector4 = vector3 - GetFaceLocalCenter(face);
				DrawLocalStar(GetFaceLocalCenter(face), Color.green);
				if (int3.x > 0 && facePlane.normal.x == 0f && vector4.x < 0f)
				{
					int4.x--;
				}
				if (int3.y > 0 && facePlane.normal.y == 0f && vector4.y < 0f)
				{
					int4.y--;
				}
				if (int3.z > 0 && facePlane.normal.z == 0f && vector4.z < 0f)
				{
					int4.z--;
				}
				DrawLocalStar(GridToLocal(int4), Color.yellow);
			}
		}
		return int4;
	}

	public void ClearFace(Face face, FaceType faceType)
	{
		TouchCell(face.cell);
		switch (faceType)
		{
		case FaceType.WaterPark:
		{
			TouchAdjacentVertical(face.cell);
			TouchBottomWaterParkCell(face.cell);
			CellType cell2 = GetCell(face.cell);
			int num3 = ((cell2 != CellType.LargeRoom && cell2 != CellType.LargeRoomRotated) ? 1 : 2);
			int index2 = ((cell2 == CellType.LargeRoomRotated) ? 2 : 0);
			for (int j = 0; j < num3; j++)
			{
				Face face2 = new Face(face.cell, Direction.North);
				face2.cell[index2] += j;
				Direction[] horizontalDirections = HorizontalDirections;
				foreach (Direction direction2 in horizontalDirections)
				{
					face2.direction = direction2;
					SetFaceType(face2, FaceType.None);
				}
				horizontalDirections = VerticalDirections;
				foreach (Direction direction3 in horizontalDirections)
				{
					face2.direction = direction3;
					SetFaceType(face2, FaceType.Solid);
				}
			}
			break;
		}
		case FaceType.Ladder:
		{
			TouchAdjacentVertical(face.cell);
			FaceType faceType3 = deconstructFaceTypes[(uint)faceType];
			SetFaceType(face, faceType3);
			if (!GetLadderExitCell(face, out var exit))
			{
				Debug.LogError("Could not find exit of ladder");
				break;
			}
			int index = baseShape.GetIndex(exit);
			Direction direction = ReverseDirection(face.direction);
			SetFace(index, direction, faceType3);
			int num = Mathf.Min(face.cell.y, exit.y);
			int num2 = Mathf.Max(face.cell.y, exit.y);
			for (int i = num + 1; i < num2; i++)
			{
				Int3 cell = new Int3(exit.x, i, exit.z);
				SetFaceType(new Face(cell, BaseAddLadderGhost.ladderFaceDir), faceType3);
			}
			break;
		}
		default:
		{
			FaceType faceType2 = deconstructFaceTypes[(uint)faceType];
			SetFaceType(face, faceType2);
			break;
		}
		}
	}

	public void ClearCell(Int3 cell)
	{
		int cellIndex = GetCellIndex(cell);
		if (cellIndex == -1)
		{
			return;
		}
		CellType cellType = cells[cellIndex];
		if (cellType == CellType.Empty)
		{
			return;
		}
		TouchCell(cell);
		TouchAdjacentVertical(cell);
		Int3 @int = CellSize[(uint)cellType];
		Int3 maxs = cell + @int - 1;
		foreach (Int3 item in Int3.Range(cell, maxs))
		{
			int index = baseShape.GetIndex(item);
			cells[index] = CellType.Empty;
			links[index] = 0;
			isGlass[index] = false;
			Direction[] allDirections = AllDirections;
			foreach (Direction direction in allDirections)
			{
				faces[GetFaceIndex(index, direction)] = FaceType.None;
			}
		}
		Transform transform = cellObjects[cellIndex];
		if (transform != null)
		{
			transform.SetParent(null, worldPositionStays: false);
			cellObjects[cellIndex] = null;
			UnityEngine.Object.Destroy(transform.gameObject);
		}
	}

	public bool IsValidObsConnection(Int3 cell, Direction direction)
	{
		Base @base = this;
		if (@base.isGhost)
		{
			BaseGhost component = GetComponent<BaseGhost>();
			@base = component.TargetBase;
			if (@base == null)
			{
				@base = GetComponentInParent<Base>();
			}
			cell += component.TargetOffset;
		}
		CellType cell2 = @base.GetCell(cell);
		if (cell2 == CellType.Room || cell2 == CellType.Corridor || cell2 == CellType.Moonpool || cell2 == CellType.MoonpoolRotated || cell2 == CellType.LargeRoom || cell2 == CellType.LargeRoomRotated)
		{
			return (@base.GetCellConnections(cell) & (1 << (int)direction)) != 0;
		}
		return false;
	}

	public Piece GetPiece(Face face, FaceType faceType)
	{
		CellType cell = GetCell(face.cell);
		Piece result = Piece.Invalid;
		switch (cell)
		{
		case CellType.Room:
			result = GetRoomPiece(face, faceType);
			break;
		case CellType.Observatory:
			result = observatoryFacePieces[(uint)faceType];
			break;
		case CellType.Moonpool:
			result = GetMoonpoolPiece(face, faceType);
			break;
		case CellType.MoonpoolRotated:
			result = GetMoonpoolRotatedPiece(face, faceType);
			break;
		case CellType.MapRoom:
		case CellType.MapRoomRotated:
			result = mapRoomFacePieces[(uint)faceType];
			break;
		case CellType.ControlRoom:
		case CellType.ControlRoomRotated:
			result = controlRoomFacePieces[(uint)faceType];
			break;
		case CellType.LargeRoom:
			result = GetLargeRoomPiece(face, faceType);
			break;
		case CellType.LargeRoomRotated:
			result = GetLargeRoomRotatedPiece(face, faceType);
			break;
		}
		return result;
	}

	public Transform SpawnCorridorConnector(Piece piece, Face face, Int3 parentCell)
	{
		if (piece == Piece.Invalid)
		{
			return null;
		}
		PieceDef pieceDef = pieces[(int)piece];
		Vector3 localPosition = Int3.Scale(face.cell, cellSize);
		Transform transform = GetCellObject(parentCell);
		if (transform == null)
		{
			transform = CreateCellObject(parentCell);
		}
		GameObject obj = UWE.Utils.InstantiateDeactivated(pieceDef.prefab.gameObject, transform, localPosition, FaceRotation[(int)face.direction]);
		obj.SetActive(value: true);
		return obj.transform;
	}

	public void GetAllCellConnections(Int3 cell, ICollection<Face> faces)
	{
		faces.Clear();
		Int3 @int = NormalizeCell(cell);
		int index = baseShape.GetIndex(@int);
		if (index == -1)
		{
			return;
		}
		switch (cells[index])
		{
		case CellType.Corridor:
		{
			byte b = links[index];
			Direction[] horizontalDirections = HorizontalDirections;
			foreach (Direction direction2 in horizontalDirections)
			{
				if ((b & (1 << (int)direction2)) != 0)
				{
					faces.Add(new Face(@int, direction2));
				}
			}
			break;
		}
		case CellType.Room:
			faces.Add(new Face(@int + new Int3(1, 0, 2), Direction.North));
			faces.Add(new Face(@int + new Int3(2, 0, 1), Direction.East));
			faces.Add(new Face(@int + new Int3(1, 0, 0), Direction.South));
			faces.Add(new Face(@int + new Int3(0, 0, 1), Direction.West));
			break;
		case CellType.Observatory:
		{
			Direction[] horizontalDirections = HorizontalDirections;
			foreach (Direction direction in horizontalDirections)
			{
				if (IsValidObsConnection(GetAdjacent(@int, direction), ReverseDirection(direction)))
				{
					faces.Add(new Face(@int, direction));
					break;
				}
			}
			break;
		}
		case CellType.Moonpool:
			faces.Add(new Face(@int + new Int3(1, 0, 2), Direction.North));
			faces.Add(new Face(@int + new Int3(2, 0, 2), Direction.North));
			faces.Add(new Face(@int + new Int3(0, 0, 1), Direction.West));
			faces.Add(new Face(@int + new Int3(3, 0, 1), Direction.East));
			faces.Add(new Face(@int + new Int3(1, 0, 0), Direction.South));
			faces.Add(new Face(@int + new Int3(2, 0, 0), Direction.South));
			break;
		case CellType.MoonpoolRotated:
			faces.Add(new Face(@int + new Int3(1, 0, 3), Direction.North));
			faces.Add(new Face(@int + new Int3(0, 0, 1), Direction.West));
			faces.Add(new Face(@int + new Int3(0, 0, 2), Direction.West));
			faces.Add(new Face(@int + new Int3(2, 0, 1), Direction.East));
			faces.Add(new Face(@int + new Int3(2, 0, 2), Direction.East));
			faces.Add(new Face(@int + new Int3(1, 0, 0), Direction.South));
			break;
		case CellType.MapRoom:
			faces.Add(new Face(@int + new Int3(0, 0, 1), Direction.West));
			faces.Add(new Face(@int + new Int3(2, 0, 1), Direction.East));
			break;
		case CellType.MapRoomRotated:
			faces.Add(new Face(@int + new Int3(1, 0, 2), Direction.North));
			faces.Add(new Face(@int + new Int3(1, 0, 0), Direction.South));
			break;
		case CellType.ControlRoom:
			faces.Add(new Face(@int + new Int3(0, 0, 1), Direction.West));
			faces.Add(new Face(@int + new Int3(2, 0, 1), Direction.East));
			break;
		case CellType.ControlRoomRotated:
			faces.Add(new Face(@int + new Int3(1, 0, 2), Direction.North));
			faces.Add(new Face(@int + new Int3(1, 0, 0), Direction.South));
			break;
		case CellType.LargeRoom:
			faces.Add(new Face(@int + new Int3(1, 0, 2), Direction.North));
			faces.Add(new Face(@int + new Int3(2, 0, 2), Direction.North));
			faces.Add(new Face(@int + new Int3(3, 0, 2), Direction.North));
			faces.Add(new Face(@int + new Int3(4, 0, 2), Direction.North));
			faces.Add(new Face(@int + new Int3(0, 0, 1), Direction.West));
			faces.Add(new Face(@int + new Int3(5, 0, 1), Direction.East));
			faces.Add(new Face(@int + new Int3(1, 0, 0), Direction.South));
			faces.Add(new Face(@int + new Int3(2, 0, 0), Direction.South));
			faces.Add(new Face(@int + new Int3(3, 0, 0), Direction.South));
			faces.Add(new Face(@int + new Int3(4, 0, 0), Direction.South));
			break;
		case CellType.LargeRoomRotated:
			faces.Add(new Face(@int + new Int3(1, 0, 5), Direction.North));
			faces.Add(new Face(@int + new Int3(0, 0, 4), Direction.West));
			faces.Add(new Face(@int + new Int3(0, 0, 3), Direction.West));
			faces.Add(new Face(@int + new Int3(0, 0, 2), Direction.West));
			faces.Add(new Face(@int + new Int3(0, 0, 1), Direction.West));
			faces.Add(new Face(@int + new Int3(2, 0, 4), Direction.East));
			faces.Add(new Face(@int + new Int3(2, 0, 3), Direction.East));
			faces.Add(new Face(@int + new Int3(2, 0, 2), Direction.East));
			faces.Add(new Face(@int + new Int3(2, 0, 1), Direction.East));
			faces.Add(new Face(@int + new Int3(1, 0, 0), Direction.South));
			break;
		case CellType.Foundation:
		case CellType.OccupiedByOtherCell:
		case CellType.Connector:
		case CellType.RechargePlatform:
		case CellType.WallFoundationN:
		case CellType.WallFoundationW:
		case CellType.WallFoundationS:
		case CellType.WallFoundationE:
			break;
		}
	}

	[SuppressMessage("Gendarme.Rules.Maintainability", "AvoidComplexMethodsRule")]
	public int GetCellConnections(Int3 cell)
	{
		Int3 @int = NormalizeCell(cell);
		int index = baseShape.GetIndex(@int);
		if (index == -1)
		{
			return 0;
		}
		CellType cellType = cells[index];
		int result = 0;
		Int3 int2 = cell - @int;
		switch (cellType)
		{
		case CellType.Corridor:
			result = links[index];
			break;
		case CellType.Room:
			if (int2.x == 0 && int2.z == 1)
			{
				result = 8;
			}
			else if (int2.x == 1 && int2.z == 2)
			{
				result = 1;
			}
			else if (int2.x == 2 && int2.z == 1)
			{
				result = 4;
			}
			else if (int2.x == 1 && int2.z == 0)
			{
				result = 2;
			}
			break;
		case CellType.MapRoom:
		case CellType.ControlRoom:
			if (int2.x == 0 && int2.z == 1)
			{
				result = 8;
			}
			else if (int2.x == 2 && int2.z == 1)
			{
				result = 4;
			}
			break;
		case CellType.MapRoomRotated:
		case CellType.ControlRoomRotated:
			if (int2.x == 1 && int2.z == 2)
			{
				result = 1;
			}
			else if (int2.x == 1 && int2.z == 0)
			{
				result = 2;
			}
			break;
		case CellType.Observatory:
			result = 4;
			if (IsValidObsConnection(GetAdjacent(cell, Direction.South), Direction.North))
			{
				result = 2;
			}
			else if (IsValidObsConnection(GetAdjacent(cell, Direction.West), Direction.East))
			{
				result = 8;
			}
			else if (IsValidObsConnection(GetAdjacent(cell, Direction.North), Direction.South))
			{
				result = 1;
			}
			break;
		case CellType.Moonpool:
			if ((int2.x == 1 && int2.z == 2) || (int2.x == 2 && int2.z == 2))
			{
				result = 1;
			}
			else if ((int2.x == 1 && int2.z == 0) || (int2.x == 2 && int2.z == 0))
			{
				result = 2;
			}
			else if (int2.x == 3 && int2.z == 1)
			{
				result = 4;
			}
			else if (int2.x == 0 && int2.z == 1)
			{
				result = 8;
			}
			break;
		case CellType.MoonpoolRotated:
			if ((int2.z == 1 && int2.x == 2) || (int2.z == 2 && int2.x == 2))
			{
				result = 4;
			}
			else if ((int2.z == 1 && int2.x == 0) || (int2.z == 2 && int2.x == 0))
			{
				result = 8;
			}
			else if (int2.z == 3 && int2.x == 1)
			{
				result = 1;
			}
			else if (int2.z == 0 && int2.x == 1)
			{
				result = 2;
			}
			break;
		case CellType.LargeRoom:
			if (int2.z == 2 && int2.x > 0 && int2.x < 5)
			{
				result = 1;
			}
			else if (int2.z == 0 && int2.x > 0 && int2.x < 5)
			{
				result = 2;
			}
			else if (int2.z == 1 && int2.x == 0)
			{
				result = 8;
			}
			else if (int2.z == 1 && int2.x == 5)
			{
				result = 4;
			}
			break;
		case CellType.LargeRoomRotated:
			if (int2.x == 0 && int2.z > 0 && int2.z < 5)
			{
				result = 8;
			}
			else if (int2.x == 2 && int2.z > 0 && int2.z < 5)
			{
				result = 4;
			}
			else if (int2.x == 1 && int2.z == 0)
			{
				result = 2;
			}
			else if (int2.x == 1 && int2.z == 5)
			{
				result = 1;
			}
			break;
		}
		return result;
	}

	public void FixRoomFloors()
	{
		foreach (Int3 allCell in AllCells)
		{
			CellType rawCellType = GetRawCellType(allCell);
			if (rawCellType != CellType.Room && rawCellType != CellType.LargeRoom && rawCellType != CellType.LargeRoomRotated)
			{
				continue;
			}
			int cellIndex = GetCellIndex(allCell);
			Direction[] verticalDirections = VerticalDirections;
			foreach (Direction direction in verticalDirections)
			{
				if (GetFace(cellIndex, direction) == FaceType.Hole)
				{
					bool flag = false;
					Int3 adjacent = GetAdjacent(allCell, direction);
					if (GetCell(adjacent) != rawCellType)
					{
						flag = true;
					}
					else if (!isGhost && IsCellUnderConstruction(adjacent))
					{
						flag = true;
					}
					if (flag)
					{
						SetFace(cellIndex, direction, FaceType.Solid);
					}
				}
			}
		}
	}

	private void FixCorridorLinksForCell(Int3 cell)
	{
		int cellConnections = GetCellConnections(cell);
		if (cellConnections == 0)
		{
			return;
		}
		int cellIndex = GetCellIndex(cell);
		CellType cell2 = GetCell(cell);
		Direction[] allDirections = AllDirections;
		foreach (Direction direction in allDirections)
		{
			Face face = new Face(cell, direction);
			int num = 1 << (int)direction;
			if (((cellConnections & num) != 0 || cell2 == CellType.Observatory) && GetFace(face) == FaceType.None)
			{
				SetFaceType(face, FaceType.Solid);
			}
		}
		Base @base = this;
		Int3 @int = Int3.zero;
		if (isGhost)
		{
			BaseGhost component = GetComponent<BaseGhost>();
			@base = component.TargetBase;
			if (!@base)
			{
				@base = GetComponentInParent<Base>();
			}
			@int = component.targetOffset;
		}
		allDirections = HorizontalDirections;
		foreach (Direction direction2 in allDirections)
		{
			int num2 = 1 << (int)direction2;
			if ((cellConnections & num2) == 0)
			{
				continue;
			}
			Face face2 = new Face(cell + @int, direction2);
			Face face3 = new Face(cell, direction2);
			Face adjacentFace = GetAdjacentFace(face2);
			int cellConnections2 = @base.GetCellConnections(adjacentFace.cell);
			FaceType face4 = GetFace(face3);
			if ((cellConnections2 & (1 << (int)adjacentFace.direction)) != 0)
			{
				FaceType face5 = @base.GetFace(adjacentFace);
				if (face4 == FaceType.Solid && (face5 == FaceType.Solid || face5 == FaceType.None || IsBulkhead(face5)) && !HasGhostFace(face3) && !HasGhostFace(adjacentFace))
				{
					RemoveFaceLinkedModule(face3, face4);
					SetFaceType(face3, FaceType.None);
				}
			}
		}
		if (cell2 != CellType.Corridor)
		{
			return;
		}
		allDirections = VerticalDirections;
		foreach (Direction direction3 in allDirections)
		{
			switch (GetFace(cellIndex, direction3))
			{
			case FaceType.Ladder:
			{
				Int3 exit = default(Int3);
				if (!GetLadderExitCell(cell, direction3, out exit))
				{
					SetFace(cellIndex, direction3, FaceType.Solid);
					break;
				}
				int cellIndex2 = GetCellIndex(exit);
				Direction direction5 = ReverseDirection(direction3);
				if (GetFace(cellIndex2, direction5) != FaceType.Ladder)
				{
					SetFace(cellIndex2, direction5, FaceType.Ladder);
				}
				break;
			}
			case FaceType.Solid:
			{
				Int3 adjacent = GetAdjacent(cell, direction3);
				Direction direction4 = ReverseDirection(direction3);
				if (CanConnectToCell(cell, direction3) && CanConnectToCell(adjacent, direction4))
				{
					SetFace(cellIndex, direction3, FaceType.Hole);
				}
				break;
			}
			case FaceType.Hole:
			{
				CellType cell3 = GetCell(GetAdjacent(cell, direction3));
				if (cell3 != CellType.Connector && cell3 != CellType.Corridor)
				{
					SetFace(cellIndex, direction3, FaceType.Solid);
				}
				break;
			}
			}
		}
	}

	private void UpgradeDeserializedFaces()
	{
		foreach (Int3 allCell in AllCells)
		{
			int index = baseShape.GetIndex(allCell);
			if (index != -1)
			{
				CellType cellType = cells[index];
				switch (cellType)
				{
				case CellType.Observatory:
					FixObservatoryNoneFace(allCell);
					break;
				case CellType.ControlRoom:
				case CellType.ControlRoomRotated:
					FixControlRoomModuleFace(allCell, cellType);
					break;
				}
			}
		}
	}

	private void FixObservatoryNoneFace(Int3 cell)
	{
		int cellConnections = GetCellConnections(cell);
		if (cellConnections == 0)
		{
			return;
		}
		Direction[] horizontalDirections = HorizontalDirections;
		foreach (Direction direction in horizontalDirections)
		{
			Face face = new Face(cell, direction);
			int num = 1 << (int)direction;
			if ((cellConnections & num) == 0 && GetFace(face) == FaceType.None)
			{
				SetFaceType(face, FaceType.Solid);
			}
		}
	}

	private void FixControlRoomModuleFace(Int3 cell, CellType cellType)
	{
		FaceDef[] array = faceDefs[(uint)cellType];
		for (int i = 0; i < array.Length; i++)
		{
			FaceDef faceDef = array[i];
			Face face = new Face(cell + faceDef.face.cell, faceDef.face.direction);
			if (faceDef.faceType == FaceType.ControlRoomModule && GetFace(face) != FaceType.ControlRoomModule)
			{
				SetFaceType(face, FaceType.ControlRoomModule);
			}
		}
	}

	public void FixCorridorLinks()
	{
		foreach (Int3 allCell in AllCells)
		{
			FixCorridorLinksForCell(allCell);
		}
	}

	private void RemoveFaceLinkedModule(Face face, FaceType faceType)
	{
		if (faceType == FaceType.FiltrationMachine)
		{
			FiltrationMachine filtrationMachine = GetModule(face) as FiltrationMachine;
			if (filtrationMachine != null)
			{
				UnityEngine.Object.Destroy(filtrationMachine.gameObject);
				return;
			}
			Debug.LogErrorFormat(this, "Unable to find and remove FiltrationMachine module in face {0}", face);
		}
	}

	public bool SetConnector(Int3 cell)
	{
		ClearCell(cell);
		int cellIndex = GetCellIndex(cell);
		cells[cellIndex] = CellType.Connector;
		links[cellIndex] = 0;
		return true;
	}

	public bool SetCorridor(Int3 cell, int corridorType, bool glass = false)
	{
		ClearCell(cell);
		int cellIndex = GetCellIndex(cell);
		cells[cellIndex] = CellType.Corridor;
		links[cellIndex] = (byte)corridorType;
		isGlass[cellIndex] = glass;
		Direction[] allDirections = AllDirections;
		foreach (Direction direction in allDirections)
		{
			SetFace(cellIndex, direction, FaceType.Solid);
		}
		UpdateFlowDataForCellAndNeighbors(cell);
		return true;
	}

	public bool SetCell(Int3 cell, CellType cellType)
	{
		foreach (Int3 item in Int3.Range(CellSize[(uint)cellType]))
		{
			ClearCell(cell + item);
			int index = baseShape.GetIndex(cell + item);
			if (item == Int3.zero)
			{
				cells[index] = cellType;
				continue;
			}
			cells[index] = CellType.OccupiedByOtherCell;
			links[index] = PackOffset(item);
		}
		FaceDef[] array = faceDefs[(uint)cellType];
		if (array != null)
		{
			FaceDef[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				FaceDef faceDef = array2[i];
				int faceIndex = GetFaceIndex(baseShape.GetIndex(faceDef.face.cell), faceDef.face.direction);
				faces[faceIndex] = faceDef.faceType;
			}
		}
		return true;
	}

	public void SetWallFoundation(Int3 cell, CellType cellType)
	{
		foreach (Int3 item in Int3.Range(CellSize[(uint)cellType]))
		{
			ClearCell(cell + item);
			int index = baseShape.GetIndex(cell + item);
			if (item == Int3.zero)
			{
				cells[index] = cellType;
				continue;
			}
			cells[index] = CellType.OccupiedByOtherCell;
			links[index] = PackOffset(item);
		}
	}

	public float GetHullStrength(Int3 cell)
	{
		int index = baseShape.GetIndex(cell);
		if (index == -1)
		{
			return 0f;
		}
		float y = GridToWorld(cell).y;
		float num = ApplyDepthScaling(CellHullStrength[(uint)cells[index]], y);
		if (isGlass[index])
		{
			num -= Mathf.Abs(num);
		}
		Direction[] allDirections = AllDirections;
		foreach (Direction direction in allDirections)
		{
			int faceIndex = GetFaceIndex(index, direction);
			FaceType faceType = faces[faceIndex];
			if ((faceType & FaceType.OccupiedByOtherFace) == 0)
			{
				num += ApplyDepthScaling(FaceHullStrength[(uint)faceType], y);
			}
		}
		return num;
	}

	public bool AreCellsConnected(Int3 u, Int3 v)
	{
		int index = baseShape.GetIndex(u);
		if (index == -1)
		{
			return false;
		}
		Int3 @int = v - u;
		Direction[] allDirections = AllDirections;
		foreach (Direction direction in allDirections)
		{
			if (DirectionOffset[(int)direction] == @int)
			{
				FaceType face = GetFace(index, direction);
				if (face != 0 && face != FaceType.ObsoleteDoor)
				{
					return face == FaceType.Ladder;
				}
				return true;
			}
		}
		return false;
	}

	public void AllocateMasks()
	{
		UWE.Utils.EnsureArraySize(ref masks, baseShape.Size, reset: true);
	}

	public void ClearMasks()
	{
		UWE.Utils.EnsureArraySize(ref masks, baseShape.Size, reset: true);
	}

	public void SetCellMask(Int3 cell, bool isMasked)
	{
		if (masks == null)
		{
			return;
		}
		int index = baseShape.GetIndex(cell);
		if (index != -1)
		{
			if (isMasked)
			{
				masks[index] |= 64;
			}
			else
			{
				masks[index] &= 191;
			}
		}
	}

	public void SetFaceMask(Face face, bool isMasked)
	{
		if (masks == null)
		{
			return;
		}
		int index = baseShape.GetIndex(face.cell);
		if (index != -1)
		{
			Direction direction = NormalizeFaceDirection(index, face.direction);
			int num = 1 << (int)direction;
			if (isMasked)
			{
				masks[index] |= (byte)num;
			}
			else
			{
				masks[index] &= (byte)(~num);
			}
		}
	}

	public bool GetCellMask(Int3 cell)
	{
		if (masks == null)
		{
			return true;
		}
		int index = baseShape.GetIndex(cell);
		if (index == -1)
		{
			return false;
		}
		return (masks[index] & 0x40) != 0;
	}

	public bool GetFaceMask(Face face)
	{
		if (masks == null)
		{
			return true;
		}
		int index = baseShape.GetIndex(face.cell);
		if (index == -1)
		{
			return false;
		}
		Direction direction = NormalizeFaceDirection(index, face.direction);
		int num = 1 << (int)direction;
		return (masks[index] & num) != 0;
	}

	public bool HasGhostFace(Face face)
	{
		bool result = false;
		foreach (BaseGhost ghost in ghosts)
		{
			Base ghostBase = ghost.GhostBase;
			if (ghostBase == null)
			{
				continue;
			}
			int index = ghostBase.baseShape.GetIndex(face.cell - ghost.TargetOffset);
			if (index != -1)
			{
				FaceType face2 = ghostBase.GetFace(index, face.direction);
				if (face2 != FaceType.Solid && face2 != 0 && !IsBulkhead(face2))
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	public bool GetGhostFace(Face face, out FaceType faceType)
	{
		faceType = FaceType.None;
		bool result = false;
		foreach (BaseGhost ghost in ghosts)
		{
			Base ghostBase = ghost.GhostBase;
			if (!(ghostBase == null))
			{
				int index = ghostBase.baseShape.GetIndex(face.cell - ghost.TargetOffset);
				if (index != -1 && ghostBase.IsFaceUsed(index, face.direction))
				{
					faceType = ghostBase.GetFace(index, face.direction);
					result = true;
				}
			}
		}
		return result;
	}

	public bool IsValidCell(Int3 cell)
	{
		if (masks == null)
		{
			return true;
		}
		Int3 @int = NormalizeCell(cell);
		CellType cell2 = GetCell(@int);
		foreach (Int3 item in Int3.Range(CellSize[(uint)cell2]))
		{
			int index = baseShape.GetIndex(@int + item);
			if (index != -1 && masks[index] != 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsCellUnderConstruction(Int3 cell)
	{
		return IsCellUnderConstruction(cell, ghosts);
	}

	private bool IsCellUnderConstruction(Int3 cell, List<BaseGhost> ghosts)
	{
		for (int i = 0; i < ghosts.Count; i++)
		{
			BaseGhost baseGhost = ghosts[i];
			if (!(baseGhost.GhostBase == null) && baseGhost.GhostBase.IsCellValid(cell - baseGhost.TargetOffset))
			{
				return true;
			}
		}
		return false;
	}

	public void OnPreDestroy()
	{
		VehicleDockingBay[] componentsInChildren = GetComponentsInChildren<VehicleDockingBay>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].SetVehicleUndocked();
		}
	}

	public void DestroyIfEmpty(BaseGhost ignoreGhost = null)
	{
		if (IsEmpty(ignoreGhost))
		{
			OnPreDestroy();
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public bool IsEmpty(BaseGhost ignoreGhost = null)
	{
		foreach (Int3 allCell in AllCells)
		{
			if (!IsCellEmpty(allCell))
			{
				return false;
			}
		}
		foreach (BaseGhost ghost in ghosts)
		{
			if (ghost != ignoreGhost)
			{
				return false;
			}
		}
		return true;
	}

	public void RegisterBaseGhost(BaseGhost ghost)
	{
		if (!ghosts.Contains(ghost))
		{
			ghosts.Add(ghost);
		}
	}

	public void DeregisterBaseGhost(BaseGhost ghost)
	{
		ghosts.Remove(ghost);
	}

	public bool IsBulkheadConnected(Int3.Bounds bounds)
	{
		foreach (Int3 item in bounds)
		{
			Direction[] allDirections = AllDirections;
			foreach (Direction direction in allDirections)
			{
				Face adjacentFace = GetAdjacentFace(new Face(item, direction));
				if (IsBulkhead(GetFace(adjacentFace)))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsGhostBulkheadConntected(Int3.Bounds bounds)
	{
		foreach (BaseGhost ghost in ghosts)
		{
			if ((bool)ghost.GhostBase && ghost.GhostBase.IsBulkheadConnected(new Int3.Bounds(bounds.mins - ghost.targetOffset, bounds.maxs - ghost.targetOffset)))
			{
				return true;
			}
		}
		return false;
	}
}
