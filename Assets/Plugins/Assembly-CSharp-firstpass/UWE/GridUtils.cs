using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UWE
{
	public static class GridUtils
	{
		public delegate bool CellBoolFunc(Int2 cell);

		public interface IBinaryReadWrite
		{
			bool Write(BinaryWriter w);

			bool Read(BinaryReader r);
		}

		private static int[,] Laplace2D = new int[3, 3]
		{
			{ 1, 1, 1 },
			{ 1, -8, 1 },
			{ 1, 1, 1 }
		};

		public static void UpdateDistanceField(FiniteGrid<float> field, Queue<Int2> dirtyCells, CellBoolFunc isCellValid)
		{
			while (dirtyCells.Count > 0)
			{
				Int2 u = dirtyCells.Dequeue();
				float num = field.Get(u);
				for (int i = -1; i <= 1; i++)
				{
					for (int j = -1; j <= 1; j++)
					{
						if (i == 0 && j == 0)
						{
							continue;
						}
						Int2 @int = new Int2(u.x + i, u.y + j);
						if (field.IsInbound(@int) && isCellValid(@int))
						{
							float distance = u.GetDistance(@int);
							float num2 = field.Get(@int);
							if ((double)(num + distance) < (double)num2 - 0.0001)
							{
								field.Set(@int, num + distance);
								dirtyCells.Enqueue(@int);
							}
						}
					}
				}
			}
		}

		public static void UpdateDistanceField(FiniteGrid<float> field, Queue<Int2> dirtyCells)
		{
			UpdateDistanceField(field, dirtyCells, (Int2 cell) => true);
		}

		public static void UpdateDistanceField(float[,] field, Queue<Int2> dirtyCells, CellBoolFunc isCellValid)
		{
			while (dirtyCells.Count > 0)
			{
				Int2 p = dirtyCells.Dequeue();
				float num = field.Get(p);
				for (int i = -1; i <= 1; i++)
				{
					for (int j = -1; j <= 1; j++)
					{
						if (i == 0 && j == 0)
						{
							continue;
						}
						Int2 @int = new Int2(p.x + i, p.y + j);
						if (field.CheckBounds(@int) && isCellValid(@int))
						{
							float distance = p.GetDistance(@int);
							float num2 = field.Get(@int);
							if ((double)(num + distance) < (double)num2 - 0.0001)
							{
								field.Set(@int, num + distance);
								dirtyCells.Enqueue(@int);
							}
						}
					}
				}
			}
		}

		public static void UpdateDistanceField(float[,] field, Queue<Int2> dirtyCells)
		{
			UpdateDistanceField(field, dirtyCells, (Int2 cell) => true);
		}

		public static float Max(FiniteGrid<float> field)
		{
			float num = float.NegativeInfinity;
			FiniteGrid<float>.Iter iter = field.GetIter();
			while (!iter.IsDone())
			{
				num = Mathf.Max(num, iter.Get());
				++iter;
			}
			return num;
		}

		public static Int2 MaxPos(FiniteGrid<float> field)
		{
			float num = float.NegativeInfinity;
			Int2 result = new Int2(-1, -1);
			FiniteGrid<float>.Iter iter = field.GetIter();
			while (!iter.IsDone())
			{
				if (iter.Get() > num)
				{
					result = iter.GetPos();
					num = iter.Get();
				}
				++iter;
			}
			return result;
		}

		public static float Min(FiniteGrid<float> field)
		{
			float num = float.PositiveInfinity;
			FiniteGrid<float>.Iter iter = field.GetIter();
			while (!iter.IsDone())
			{
				num = Mathf.Min(num, iter.Get());
				++iter;
			}
			return num;
		}

		public static void NormalizeField(FiniteGrid<float> field)
		{
			float max = Max(field);
			FiniteGrid<float>.Iter iter = field.GetIter();
			while (!iter.IsDone())
			{
				field.Set(iter.i, iter.j, Utils.Unlerp(iter.Get(), 0f, max));
				++iter;
			}
		}

		public static void NormalizeFieldBothEnds(FiniteGrid<float> field)
		{
			float max = Max(field);
			float min = Min(field);
			FiniteGrid<float>.Iter iter = field.GetIter();
			while (!iter.IsDone())
			{
				field.Set(iter.i, iter.j, Utils.Unlerp(iter.Get(), min, max));
				++iter;
			}
		}

		public static void Scale(FiniteGrid<float> field, float s)
		{
			field.ForEach(delegate(int x, int y, float fieldValue)
			{
				field.Set(x, y, fieldValue * s);
				return IterResult.Continue;
			});
		}

		public static void Map<T>(FiniteGrid<int> src, FiniteGrid<T> dest, IList<T> map)
		{
			dest.Reset(src.xRes, src.yRes);
			src.ForEach(delegate(int x, int y, int srcVal)
			{
				dest.Set(x, y, map[srcVal]);
				return IterResult.Continue;
			});
		}

		public static FiniteGrid<char> ReadCharGrid(string[] lines, char defaultChar, bool flipRows)
		{
			FiniteGrid<char> finiteGrid = new FiniteGrid<char>();
			int num = lines.Length;
			int num2 = 0;
			foreach (string text in lines)
			{
				num2 = Mathf.Max(num2, text.Length);
			}
			finiteGrid.Reset(num, num2);
			for (int j = 0; j < num; j++)
			{
				for (int k = 0; k < num2; k++)
				{
					char v = defaultChar;
					string text2 = (flipRows ? lines[num - 1 - j] : lines[j]);
					if (k < text2.Length)
					{
						v = text2[k];
					}
					finiteGrid.Set(j, k, v);
				}
			}
			return finiteGrid;
		}

		public static void Write<T>(StreamWriter writer, FiniteGrid<T> grid)
		{
			writer.WriteLine("grid");
			writer.WriteLine(string.Concat(grid.xRes));
			writer.WriteLine(string.Concat(grid.yRes));
			for (int num = grid.yRes - 1; num >= 0; num--)
			{
				string text = "";
				for (int i = 0; i < grid.xRes; i++)
				{
					text = string.Concat(text, grid[i, num], " ");
				}
				writer.WriteLine(text);
			}
		}

		public static bool Read(StreamReader reader, FiniteGrid<float> grid)
		{
			if (reader.ReadLine() != "grid")
			{
				Debug.LogError("Expected to read the line 'grid', but did not. Input is corrupted!");
				return false;
			}
			int num = Convert.ToInt32(reader.ReadLine());
			int yRes = Convert.ToInt32(reader.ReadLine());
			grid.Reset(num, yRes);
			for (int num2 = grid.yRes - 1; num2 >= 0; num2--)
			{
				string[] array = reader.ReadLine().Split(' ');
				if (array.Length != num + 1)
				{
					Debug.LogError("While reading float-grid, the line for y = " + num2 + " did not have enough elements! Expected " + num + ", only got " + (array.Length - 1));
					return false;
				}
				for (int i = 0; i < grid.xRes; i++)
				{
					grid[i, num2] = Convert.ToSingle(array[i]);
				}
			}
			return true;
		}

		public static bool Read(StreamReader reader, FiniteGrid<int> grid)
		{
			if (reader.ReadLine() != "grid")
			{
				Debug.LogError("Expected to read the line 'grid', but did not. Input is corrupted!");
				return false;
			}
			int num = Convert.ToInt32(reader.ReadLine());
			int yRes = Convert.ToInt32(reader.ReadLine());
			grid.Reset(num, yRes);
			for (int num2 = grid.yRes - 1; num2 >= 0; num2--)
			{
				string[] array = reader.ReadLine().Split(' ');
				if (array.Length != num + 1)
				{
					Debug.LogError("While reading int-grid, the line for y = " + num2 + " did not have enough elements! Expected " + num + ", only got " + (array.Length - 1));
					return false;
				}
				for (int i = 0; i < grid.xRes; i++)
				{
					grid[i, num2] = Convert.ToInt32(array[i]);
				}
			}
			return true;
		}

		public static void Write(BinaryWriter w, FiniteGrid<bool> grid)
		{
			w.Write(grid.xRes);
			w.Write(grid.yRes);
			for (int i = 0; i < grid.xRes * grid.yRes; i++)
			{
				w.Write(grid[i]);
			}
		}

		public static bool Read(BinaryReader r, FiniteGrid<bool> grid)
		{
			int xRes = r.ReadInt32();
			int yRes = r.ReadInt32();
			grid.Reset(xRes, yRes);
			for (int i = 0; i < grid.xRes * grid.yRes; i++)
			{
				grid[i] = r.ReadBoolean();
			}
			return true;
		}

		public static bool Write<T>(BinaryWriter w, FiniteGrid<T> grid) where T : IBinaryReadWrite
		{
			w.Write(grid.xRes);
			w.Write(grid.yRes);
			for (int i = 0; i < grid.xRes * grid.yRes; i++)
			{
				if (!grid[i].Write(w))
				{
					return false;
				}
			}
			return true;
		}

		public static bool Read<T>(BinaryReader r, FiniteGrid<T> grid) where T : IBinaryReadWrite
		{
			int xRes = r.ReadInt32();
			int yRes = r.ReadInt32();
			grid.Reset(xRes, yRes);
			for (int i = 0; i < grid.xRes * grid.yRes; i++)
			{
				if (!grid[i].Read(r))
				{
					return false;
				}
			}
			return true;
		}

		public static void Write(BinaryWriter w, FiniteGrid<int> grid)
		{
			w.Write(grid.xRes);
			w.Write(grid.yRes);
			for (int i = 0; i < grid.xRes * grid.yRes; i++)
			{
				w.Write(grid[i]);
			}
		}

		public static bool Read(BinaryReader r, FiniteGrid<int> grid)
		{
			int xRes = r.ReadInt32();
			int yRes = r.ReadInt32();
			grid.Reset(xRes, yRes);
			for (int i = 0; i < grid.xRes * grid.yRes; i++)
			{
				grid[i] = r.ReadInt32();
			}
			return true;
		}

		public static void Write(BinaryWriter w, FiniteGrid<float> grid)
		{
			w.Write(grid.xRes);
			w.Write(grid.yRes);
			for (int i = 0; i < grid.xRes * grid.yRes; i++)
			{
				w.Write(grid[i]);
			}
		}

		public static bool Read(BinaryReader r, FiniteGrid<float> grid)
		{
			int xRes = r.ReadInt32();
			int yRes = r.ReadInt32();
			grid.Reset(xRes, yRes);
			for (int i = 0; i < grid.xRes * grid.yRes; i++)
			{
				grid[i] = r.ReadSingle();
			}
			return true;
		}

		public static void TestBinaryReadWrite()
		{
			FiniteGrid<float> grid = new FiniteGrid<float>();
			grid.Reset(10, 10);
			grid.ForEach(delegate(int x, int y, float value)
			{
				grid[x, y] = (float)(x * 10 + y) + UnityEngine.Random.value;
				return true;
			});
			StreamWriter streamWriter = new StreamWriter("TEMP-grid-io-test.bin");
			Write(streamWriter, grid);
			streamWriter.Close();
			FiniteGrid<float> grid2 = new FiniteGrid<float>();
			StreamReader streamReader = new StreamReader("TEMP-grid-io-test.bin");
			Read(streamReader, grid2);
			streamReader.Close();
			grid.ForEach((int x, int y, float value) => true);
			File.Delete("TEMP-grid-io-test.bin");
		}

		public static void TestReadWrite()
		{
			FiniteGrid<float> grid = new FiniteGrid<float>();
			grid.Reset(10, 10);
			grid.ForEach(delegate(int x, int y, float value)
			{
				grid[x, y] = (float)(x * 10 + y) + UnityEngine.Random.value;
				return true;
			});
			StreamWriter streamWriter = new StreamWriter("TEMP-grid-io-test.txt");
			Write(streamWriter, grid);
			streamWriter.Close();
			FiniteGrid<float> grid2 = new FiniteGrid<float>();
			StreamReader streamReader = new StreamReader("TEMP-grid-io-test.txt");
			Read(streamReader, grid2);
			streamReader.Close();
			grid.ForEach((int x, int y, float value) => true);
			File.Delete("TEMP-grid-io-test.txt");
		}

		public static int Convolve3x3Single(this FiniteGrid<int> input, int[,] kernel, int x, int y)
		{
			int num = 0;
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					int x2 = x + i - 1;
					int y2 = y + j - 1;
					num += input.GetClamped(x2, y2) * kernel[i, j];
				}
			}
			return num;
		}

		public static void Convolve3x3(this FiniteGrid<int> input, int[,] kernel, FiniteGrid<int> output)
		{
			for (int i = 0; i < input.xRes; i++)
			{
				for (int j = 0; j < input.yRes; j++)
				{
					output[i, j] = input.Convolve3x3Single(kernel, i, j);
				}
			}
		}

		public static void ConvolveLaplace(this FiniteGrid<int> input, FiniteGrid<int> output)
		{
			input.Convolve3x3(Laplace2D, output);
		}

		public static void ComputeSobelMagnitudeSquared(this FiniteGrid<int> input, FiniteGrid<int> output)
		{
			int[,] kernel = new int[3, 3]
			{
				{ 1, 0, -1 },
				{ 2, 0, -2 },
				{ 1, 0, -1 }
			};
			int[,] kernel2 = new int[3, 3]
			{
				{ 1, 2, 1 },
				{ 0, 0, 0 },
				{ -1, -2, -1 }
			};
			for (int i = 0; i < input.xRes; i++)
			{
				for (int j = 0; j < input.yRes; j++)
				{
					int num = input.Convolve3x3Single(kernel2, i, j);
					int num2 = input.Convolve3x3Single(kernel, i, j);
					output[i, j] = num * num + num2 * num2;
				}
			}
		}

		public static int FloodFill<T>(T[,] grid, Int2 startPos, T newValue, CellBoolFunc isCellFloodable)
		{
			if (!isCellFloodable(startPos))
			{
				return 0;
			}
			Queue<Int2> queue = new Queue<Int2>();
			queue.Enqueue(startPos);
			int result = 0;
			while (queue.Count > 0)
			{
				Int2 p = queue.Dequeue();
				grid.Set(p, newValue);
				for (int i = 0; i < 4; i++)
				{
					Int2 @int = p.Get4Nbor(i);
					if (isCellFloodable(@int))
					{
						queue.Enqueue(@int);
					}
				}
			}
			return result;
		}
	}
}
