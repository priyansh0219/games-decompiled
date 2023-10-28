using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class ComplexLayout : IExposable
	{
		private const int MinAdjacencyForDisconnectedRoom = 3;

		public CellRect container;

		public RoomLayoutCellType[,] cellTypes;

		public int[,] roomIds;

		private int currentRoomId;

		private List<ComplexRoom> rooms = new List<ComplexRoom>();

		private static List<ComplexRoom> tmpRooms = new List<ComplexRoom>();

		private static Queue<ComplexRoom> tmpRoomQueue = new Queue<ComplexRoom>();

		private static HashSet<ComplexRoom> tmpSeenRooms = new HashSet<ComplexRoom>();

		public List<ComplexRoom> Rooms => rooms;

		public int Area
		{
			get
			{
				int num = 0;
				for (int i = 0; i < rooms.Count; i++)
				{
					num += rooms[i].Area;
				}
				return num;
			}
		}

		public void Init(CellRect container)
		{
			this.container = container;
			cellTypes = new RoomLayoutCellType[container.Width, container.Height];
			roomIds = new int[container.Width, container.Height];
			for (int i = 0; i < container.Width; i++)
			{
				for (int j = 0; j < container.Height; j++)
				{
					roomIds[i, j] = -1;
				}
			}
		}

		public void AddRoom(List<CellRect> rects)
		{
			for (int i = 0; i < rects.Count; i++)
			{
				rects[i] = rects[i].ClipInsideRect(container);
			}
			rooms.Add(new ComplexRoom(rects));
		}

		public void Add(IntVec3 position, RoomLayoutCellType cellType)
		{
			if (cellTypes.InBounds(position.x, position.z))
			{
				cellTypes[position.x, position.z] = cellType;
			}
		}

		public bool IsWallAt(IntVec3 position)
		{
			if (cellTypes.InBounds(position.x, position.z))
			{
				return cellTypes[position.x, position.z] == RoomLayoutCellType.Wall;
			}
			return false;
		}

		public bool IsFloorAt(IntVec3 position)
		{
			if (cellTypes.InBounds(position.x, position.z))
			{
				return cellTypes[position.x, position.z] == RoomLayoutCellType.Floor;
			}
			return false;
		}

		public bool IsDoorAt(IntVec3 position)
		{
			if (cellTypes.InBounds(position.x, position.z))
			{
				return cellTypes[position.x, position.z] == RoomLayoutCellType.Door;
			}
			return false;
		}

		public bool IsEmptyAt(IntVec3 position)
		{
			if (cellTypes.InBounds(position.x, position.z))
			{
				return cellTypes[position.x, position.z] == RoomLayoutCellType.Empty;
			}
			return false;
		}

		public bool IsOutside(IntVec3 position)
		{
			if (cellTypes.InBounds(position.x, position.z))
			{
				return cellTypes[position.x, position.z] == RoomLayoutCellType.Empty;
			}
			return true;
		}

		public int GetRoomIdAt(IntVec3 position)
		{
			if (!roomIds.InBounds(position.x, position.z))
			{
				return -2;
			}
			return roomIds[position.x, position.z];
		}

		public bool TryMinimizeLayoutWithoutDisconnection()
		{
			if (rooms.Count == 1)
			{
				return false;
			}
			for (int num = rooms.Count - 1; num >= 0; num--)
			{
				if (IsAdjacentToLayoutEdge(rooms[num]) && !WouldDisconnectRoomsIfRemoved(rooms[num]))
				{
					rooms.RemoveAt(num);
					return true;
				}
			}
			return false;
		}

		private bool WouldDisconnectRoomsIfRemoved(ComplexRoom room)
		{
			tmpRooms.Clear();
			tmpRooms.AddRange(rooms);
			tmpRooms.Remove(room);
			tmpSeenRooms.Clear();
			tmpRoomQueue.Clear();
			tmpRoomQueue.Enqueue(tmpRooms.First());
			while (tmpRoomQueue.Count > 0)
			{
				ComplexRoom complexRoom = tmpRoomQueue.Dequeue();
				tmpSeenRooms.Add(complexRoom);
				foreach (ComplexRoom tmpRoom in tmpRooms)
				{
					if (complexRoom != tmpRoom && !tmpSeenRooms.Contains(tmpRoom) && complexRoom.IsAdjacentTo(tmpRoom, 3))
					{
						tmpRoomQueue.Enqueue(tmpRoom);
					}
				}
			}
			int count = tmpRooms.Count;
			int count2 = tmpSeenRooms.Count;
			tmpRooms.Clear();
			tmpSeenRooms.Clear();
			return count2 != count;
		}

		public bool IsAdjacentToLayoutEdge(ComplexRoom room)
		{
			for (int i = 0; i < room.rects.Count; i++)
			{
				if (room.rects[i].minX == container.minX || room.rects[i].maxX == container.maxX || room.rects[i].minZ == container.minZ || room.rects[i].maxZ == container.maxZ)
				{
					return true;
				}
			}
			return false;
		}

		public void Finish()
		{
			for (int i = 0; i < 4; i++)
			{
				Rot4 dir = new Rot4(i);
				foreach (ComplexRoom room in rooms)
				{
					for (int j = 0; j < room.rects.Count; j++)
					{
						foreach (IntVec3 edgeCell in room.rects[j].GetEdgeCells(dir))
						{
							IntVec3 facingCell = dir.FacingCell + edgeCell;
							if (!IsWallAt(facingCell) && !room.rects.Any((CellRect r) => r.Contains(facingCell)))
							{
								Add(edgeCell, RoomLayoutCellType.Wall);
							}
						}
					}
					foreach (CellRect rect in room.rects)
					{
						foreach (IntVec3 cell in rect.Cells)
						{
							roomIds[cell.x, cell.z] = currentRoomId;
							if (!IsWallAt(cell))
							{
								Add(cell, RoomLayoutCellType.Floor);
							}
						}
					}
					currentRoomId++;
				}
			}
			for (int k = container.minX; k < container.maxX; k++)
			{
				for (int l = container.minZ; l < container.maxZ; l++)
				{
					IntVec3 intVec = new IntVec3(k, 0, l);
					if (IsWallAt(intVec) || !IsFloorAt(intVec))
					{
						continue;
					}
					int num = 0;
					IntVec3[] cardinalDirections = GenAdj.CardinalDirections;
					foreach (IntVec3 intVec2 in cardinalDirections)
					{
						if (IsWallAt(intVec + intVec2))
						{
							num++;
						}
					}
					int num2 = 0;
					int num3 = 0;
					cardinalDirections = GenAdj.DiagonalDirections;
					foreach (IntVec3 intVec3 in cardinalDirections)
					{
						if (IsWallAt(intVec + intVec3))
						{
							num2++;
						}
						else if (!IsFloorAt(intVec + intVec3))
						{
							num3++;
						}
					}
					if (num > 1 && (num2 < 2 || num3 > 0))
					{
						Add(intVec, RoomLayoutCellType.Wall);
					}
				}
			}
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref container, "container");
			Scribe_Collections.Look(ref rooms, "rooms", LookMode.Deep);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				Init(container);
				Finish();
			}
		}
	}
}
