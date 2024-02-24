using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UWE
{
	[Serializable]
	public class FiniteGrid<T>
	{
		public delegate bool CellFunc(int x, int y, T value);

		public delegate bool PickFunc(int x, int y, T value);

		public struct Iter
		{
			private FiniteGrid<T> host;

			public int i { get; private set; }

			public int j { get; private set; }

			public Iter(FiniteGrid<T> host)
			{
				this = default(Iter);
				i = 0;
				j = 0;
				this.host = host;
			}

			public bool IsDone()
			{
				return i >= host.yRes;
			}

			public T Get()
			{
				return host.Get(i, j);
			}

			public Int2 GetPos()
			{
				return new Int2(i, j);
			}

			public void Next()
			{
				j++;
				if (j >= host.xRes)
				{
					i++;
					j = 0;
				}
			}

			public static Iter operator ++(Iter iter)
			{
				iter.Next();
				return iter;
			}
		}

		public delegate Color PixelFunc(Int2 p, T item);

		private T[] data;

		public Vector3 wsSize;

		public int xRes { get; private set; }

		public int yRes { get; private set; }

		public int numLong => xRes;

		public int numLat => yRes;

		public T this[int x, int y]
		{
			get
			{
				return Get(x, y);
			}
			set
			{
				Set(x, y, value);
			}
		}

		public T this[int i]
		{
			get
			{
				return data[i];
			}
			set
			{
				data[i] = value;
			}
		}

		public T this[Int2 p]
		{
			get
			{
				return Get(p);
			}
			set
			{
				Set(p, value);
			}
		}

		public FiniteGrid()
		{
			xRes = -1;
			yRes = -1;
			data = null;
		}

		public float GetCellWidth()
		{
			return wsSize.x / (float)xRes;
		}

		public float GetCellDepth()
		{
			return wsSize.z / (float)yRes;
		}

		public Int2 XZToCell(Vector3 xz)
		{
			int x = Mathf.FloorToInt(xz.x / wsSize.x * (float)xRes);
			int y = Mathf.FloorToInt(xz.z / wsSize.z * (float)yRes);
			return new Int2(x, y);
		}

		public Int2 UVToCell(Vector2 uv)
		{
			int x = Mathf.FloorToInt(uv.x * (float)xRes);
			int y = Mathf.FloorToInt(uv.y * (float)yRes);
			return new Int2(x, y);
		}

		public Vector2 GetCellCenterUV(Int2 p)
		{
			return new Vector2(((float)p.x + 0.5f) / (float)xRes, ((float)p.y + 0.5f) / (float)yRes);
		}

		public Vector2 GetCellCornerUV(Int2 p)
		{
			return new Vector2((float)p.x / (float)xRes, (float)p.y / (float)yRes);
		}

		public Vector2 GetRandomUVInCell(Int2 p)
		{
			return new Vector2(((float)p.x + UnityEngine.Random.value) / (float)xRes, ((float)p.y + UnityEngine.Random.value) / (float)yRes);
		}

		public Vector3 GetCellCenterXZ(Int2 p)
		{
			return new Vector3(((float)p.x + 0.5f) / (float)xRes * wsSize.x, 0f, ((float)p.y + 0.5f) / (float)yRes * wsSize.z);
		}

		public void Reset(int xRes, int yRes)
		{
			this.xRes = xRes;
			this.yRes = yRes;
			data = new T[xRes * yRes];
			for (int i = 0; i < xRes * yRes; i++)
			{
				data[i] = default(T);
			}
		}

		public bool IsInbound(int x, int y)
		{
			if (x >= 0 && x < xRes && y >= 0)
			{
				return y < yRes;
			}
			return false;
		}

		public bool IsInbound(Int2 p)
		{
			return IsInbound(p.x, p.y);
		}

		public bool IsOnEdge(Int2 p)
		{
			if (p.x != 0 && p.y != 0 && p.x != xRes - 1)
			{
				return p.y == yRes - 1;
			}
			return true;
		}

		public int IndexOf(int x, int y)
		{
			return y * xRes + x;
		}

		public T Get(int x, int y, T defaultVal)
		{
			if (IsInbound(x, y))
			{
				return data[IndexOf(x, y)];
			}
			return defaultVal;
		}

		public T Get(int x, int y)
		{
			return data[IndexOf(x, y)];
		}

		public T Get(Int2 u)
		{
			return Get(u.x, u.y);
		}

		public T Get(Int2 u, T defaultVal)
		{
			return Get(u.x, u.y, defaultVal);
		}

		public T Get(Vector3 wsPos)
		{
			return Get(XZToCell(wsPos));
		}

		public T GetClamped(int x, int y)
		{
			return Get(x.Clamp(0, xRes - 1), y.Clamp(0, yRes - 1));
		}

		public void Set(int x, int y, T v)
		{
			data[IndexOf(x, y)] = v;
		}

		public void Set(Int2 u, T val)
		{
			Set(u.x, u.y, val);
		}

		public void ForEach(CellFunc func)
		{
			for (int i = 0; i < xRes; i++)
			{
				for (int j = 0; j < yRes; j++)
				{
					if (!func(i, j, Get(i, j)))
					{
						return;
					}
				}
			}
		}

		public bool ForNeighborhood(Int2 center, int radialCellCount, CellFunc func)
		{
			bool result = true;
			for (int i = -radialCellCount; i <= radialCellCount; i++)
			{
				for (int j = -radialCellCount; j <= radialCellCount; j++)
				{
					int x = center.x + i;
					int y = center.y + j;
					if (IsInbound(x, y))
					{
						if (!func(x, y, Get(x, y)))
						{
							return result;
						}
					}
					else
					{
						result = false;
					}
				}
			}
			return result;
		}

		public bool ForNeighborhood(Int2 center, float radius, CellFunc func)
		{
			int radialCellCount = Mathf.CeilToInt(radius / (wsSize.x / (float)xRes));
			return ForNeighborhood(center, radialCellCount, func);
		}

		public int Count(PickFunc func)
		{
			int num = 0;
			ForEach(delegate(int x, int y, T value)
			{
				if (func(x, y, value))
				{
					num++;
				}
				return true;
			});
			return num;
		}

		public bool Pick(PickFunc func, int id, ref Int2 posOut)
		{
			bool found = false;
			Int2 pickedPos = default(Int2);
			int num = 0;
			ForEach(delegate(int x, int y, T value)
			{
				if (func(x, y, value))
				{
					if (num == id)
					{
						pickedPos = new Int2(x, y);
						found = true;
						return false;
					}
					num++;
				}
				return true;
			});
			posOut = pickedPos;
			return found;
		}

		public List<Int2> Collect(PickFunc func)
		{
			List<Int2> poss = new List<Int2>();
			ForEach(delegate(int x, int y, T value)
			{
				if (func(x, y, value))
				{
					poss.Add(new Int2(x, y));
				}
				return true;
			});
			return poss;
		}

		public FiniteGrid<T> Duplicate()
		{
			FiniteGrid<T> finiteGrid = new FiniteGrid<T>();
			finiteGrid.Reset(xRes, yRes);
			for (int i = 0; i < data.Length; i++)
			{
				finiteGrid.data[i] = data[i];
			}
			return finiteGrid;
		}

		public void SetAll(T val)
		{
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = val;
			}
		}

		public Iter GetIter()
		{
			return new Iter(this);
		}

		public void GetSubBounds(CellFunc func, out Int2 mins, out Int2 maxs)
		{
			mins = new Int2(xRes, yRes);
			maxs = new Int2(-1, -1);
			for (int i = 0; i < xRes; i++)
			{
				for (int j = 0; j < yRes; j++)
				{
					if (func(i, j, Get(i, j)))
					{
						mins.x = Mathf.Min(i, mins.x);
						mins.y = Mathf.Min(j, mins.y);
						maxs.x = Mathf.Max(i, maxs.x);
						maxs.y = Mathf.Max(j, maxs.y);
					}
				}
			}
		}

		public void SaveToPNG(string pngPath, PixelFunc func)
		{
			Texture2D texture2D = new Texture2D(xRes, yRes);
			for (int i = 0; i < xRes; i++)
			{
				for (int j = 0; j < yRes; j++)
				{
					texture2D.SetPixel(i, j, func(new Int2(i, j), Get(i, j)));
				}
			}
			texture2D.Apply();
			File.WriteAllBytes(pngPath, texture2D.EncodeToPNG());
		}
	}
}
