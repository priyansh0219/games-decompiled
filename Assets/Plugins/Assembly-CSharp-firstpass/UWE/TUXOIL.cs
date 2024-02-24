namespace UWE
{
	public static class TUXOIL
	{
		public enum TileType
		{
			T = 0,
			U = 1,
			X = 2,
			O = 3,
			I = 4,
			L = 5,
			Filler = 6,
			Count = 7
		}

		public interface IGrid
		{
			bool IsTiled(Int2 p);
		}

		public class ArrayWrapper : IGrid
		{
			private bool[,] array;

			public ArrayWrapper(bool[,] array)
			{
				this.array = array;
			}

			public bool IsTiled(Int2 p)
			{
				if (p.x >= array.GetLength(0) || p.y >= array.GetLength(1) || p.x < 0 || p.y < 0)
				{
					return false;
				}
				return array[p.x, p.y];
			}
		}

		public static bool ComputeTile(IGrid grid, Int2 p, out TileType type, out Facing facing)
		{
			facing = Facing.North;
			type = TileType.Count;
			if (!grid.IsTiled(p))
			{
				return false;
			}
			Facing[] facingValues = Utils.FacingValues;
			foreach (Facing facing2 in facingValues)
			{
				bool flag = grid.IsTiled(p.Get4Nbor((int)facing2));
				bool flag2 = grid.IsTiled(p.Get4Nbor((int)(facing2 + 1)));
				bool flag3 = grid.IsTiled(p.Get4Nbor((int)(facing2 + 2)));
				bool flag4 = grid.IsTiled(p.Get4Nbor((int)(facing2 + 3)));
				facing = facing2;
				if (!flag && flag2 && flag3 && flag4)
				{
					type = TileType.T;
					return true;
				}
				if (flag && flag2 && !flag3 && !flag4)
				{
					type = TileType.L;
					return true;
				}
				if (flag && flag3 && !flag2 && !flag4)
				{
					type = TileType.I;
					return true;
				}
				if (flag && flag2 && flag3 && flag4)
				{
					type = TileType.X;
					return true;
				}
				if (flag && !flag2 && !flag3 && !flag4)
				{
					type = TileType.U;
					return true;
				}
				if (!flag && !flag2 && !flag3 && !flag4)
				{
					type = TileType.O;
					return true;
				}
			}
			return false;
		}
	}
}
