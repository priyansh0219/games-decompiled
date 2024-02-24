using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Unity.Mathematics;
using UnityEngine;

public static class SystemExtensions
{
	public delegate bool PickFunc<T>(T item);

	public delegate void GameObjectFunc(GameObject go);

	public delegate void ComponentFunc<C>(C go) where C : Component;

	public delegate Color Array2DColoring<T>(T[,] array, int x, int y);

	public delegate Color ValueToColor<T>(T val);

	private static float dl = 0.1f;

	private static Vector3 dx = new Vector3(dl, 0f, 0f);

	private static Vector3 dz = new Vector3(0f, 0f, dl);

	private static float[,] GaussianKernel3x3 = new float[3, 3]
	{
		{ 0.07511361f, 0.123841405f, 0.07511361f },
		{ 0.123841405f, 0.20417996f, 0.123841405f },
		{ 0.07511361f, 0.123841405f, 0.07511361f }
	};

	private static float[,] GaussianKernel5x5 = new float[5, 5]
	{
		{
			0.0036630037f,
			0.014652015f,
			1f / 39f,
			0.014652015f,
			0.0036630037f
		},
		{
			0.014652015f,
			0.05860806f,
			2f / 21f,
			0.05860806f,
			0.014652015f
		},
		{
			1f / 39f,
			2f / 21f,
			0.15018316f,
			2f / 21f,
			1f / 39f
		},
		{
			0.014652015f,
			0.05860806f,
			2f / 21f,
			0.05860806f,
			0.014652015f
		},
		{
			0.0036630037f,
			0.014652015f,
			1f / 39f,
			0.014652015f,
			0.0036630037f
		}
	};

	public static void Shuffle<T>(this IList<T> list)
	{
		int num = list.Count;
		while (num > 1)
		{
			num--;
			int index = UnityEngine.Random.Range(0, num + 1);
			T value = list[index];
			list[index] = list[num];
			list[num] = value;
		}
	}

	public static void Shuffle<T>(this IList<T> list, System.Random rng)
	{
		int num = list.Count;
		while (num > 1)
		{
			num--;
			int index = rng.Next(0, num + 1);
			T value = list[index];
			list[index] = list[num];
			list[num] = value;
		}
	}

	public static T GetWrap<T>(this IList<T> list, int i)
	{
		return list[i % list.Count];
	}

	public static T GetClamped<T>(this IList<T> list, int i)
	{
		return list[i.Clamp(0, list.Count - 1)];
	}

	public static T GetLast<T>(this IList<T> list)
	{
		return list[list.Count - 1];
	}

	public static T GetReverse<T>(this IList<T> list, int j)
	{
		return list[list.Count - 1 - j];
	}

	public static T GetRandom<T>(this IList<T> list)
	{
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public static T GetRandom<T>(this IList<T> list, System.Random rng)
	{
		return list[rng.Next(list.Count)];
	}

	public static int IndexOf<T>(this IList<T> list, T item, IEqualityComparer<T> comparer)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (comparer.Equals(list[i], item))
			{
				return i;
			}
		}
		return -1;
	}

	public static void AppendRange(this IList<int> list, int start, int upperBound)
	{
		for (int i = start; i < upperBound; i++)
		{
			list.Add(i);
		}
	}

	public static void RemoveFast(this IList list, int index)
	{
		list[index] = list[list.Count - 1];
		list.RemoveAt(list.Count - 1);
	}

	public static T GetFromSubset<T>(this IList<T> list, int wanted, PickFunc<T> belongs, T defaultVal)
	{
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			if (belongs(list[i]))
			{
				if (num == wanted)
				{
					return list[i];
				}
				num++;
			}
		}
		return defaultVal;
	}

	public static T GetRandomFromSubset<T>(this IList<T> list, PickFunc<T> belongs, T defaultVal)
	{
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			if (belongs(list[i]))
			{
				num++;
			}
		}
		int wanted = UnityEngine.Random.Range(0, num);
		return list.GetFromSubset(wanted, belongs, defaultVal);
	}

	public static List<T> ShallowCopy<T>(this List<T> list)
	{
		List<T> list2 = new List<T>();
		list2.Capacity = list.Count;
		for (int i = 0; i < list.Count; i++)
		{
			list2.Add(list[i]);
		}
		return list2;
	}

	public static int FindMax(this List<int> ints)
	{
		int num = -1;
		for (int i = 0; i < ints.Count; i++)
		{
			if (num == -1 || ints[i] > ints[num])
			{
				num = i;
			}
		}
		return num;
	}

	public static HashSet<T> ToSet<T>(this IEnumerable<T> enumerable)
	{
		return enumerable.ToSet(EqualityComparer<T>.Default);
	}

	public static HashSet<T> ToSet<T>(this IEnumerable<T> enumerable, IEqualityComparer<T> comparer)
	{
		return new HashSet<T>(enumerable, comparer);
	}

	public static T FirstOrFallback<T>(this IEnumerable<T> enumerable, T fallback)
	{
		IEnumerator<T> enumerator = enumerable.GetEnumerator();
		if (enumerator.MoveNext())
		{
			return enumerator.Current;
		}
		return fallback;
	}

	public static V GetOrAddNew<K, V>(this IDictionary<K, V> dict, K key) where V : class, new()
	{
		if (!dict.TryGetValue(key, out var value))
		{
			value = new V();
			dict.Add(key, value);
		}
		return value;
	}

	public static V GetOrDefault<K, V>(this IDictionary<K, V> dict, K key, V defaultValue)
	{
		if (dict.TryGetValue(key, out var value))
		{
			return value;
		}
		return defaultValue;
	}

	public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
	{
		IEnumerator<T> enumerator = items.GetEnumerator();
		while (enumerator.MoveNext())
		{
			action(enumerator.Current);
		}
	}

	public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> items)
	{
		foreach (T item in items)
		{
			set.Add(item);
		}
	}

	public static void RemoveRange<T>(this HashSet<T> set, IEnumerable<T> items)
	{
		foreach (T item in items)
		{
			set.Remove(item);
		}
	}

	public static TR Aggregate<T, TR>(this IEnumerable<T> items, Func<T, TR> selector, Func<TR, TR, TR> combinator)
	{
		using (IEnumerator<T> enumerator = items.GetEnumerator())
		{
			if (!enumerator.MoveNext())
			{
				return default(TR);
			}
			TR val = selector(enumerator.Current);
			while (enumerator.MoveNext())
			{
				val = combinator(val, selector(enumerator.Current));
			}
			return val;
		}
	}

	public static bool SafeMoveNext(this IEnumerator coroutine, string profileSampleName)
	{
		return coroutine.SafeMoveNext();
	}

	public static bool SafeMoveNext(this IEnumerator coroutine)
	{
		try
		{
			return coroutine.MoveNext();
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
		return false;
	}

	public static void SetTrianglesUWE(this Mesh mesh, int[] indices, int numIndices)
	{
		mesh.subMeshCount = 1;
		mesh.SetTriangles(0, indices, numIndices, calculateBounds: true);
	}

	public static Vector3 SampleNormal(this Terrain terrain, Vector3 wsPos)
	{
		float y = (terrain.SampleHeight(wsPos + dx) - terrain.SampleHeight(wsPos - dx)) / (2f * dl);
		float y2 = (terrain.SampleHeight(wsPos + dz) - terrain.SampleHeight(wsPos - dz)) / (2f * dl);
		Vector3 normalized = new Vector3(1f, y, 0f).normalized;
		return Vector3.Cross(new Vector3(0f, y2, 1f).normalized, normalized).normalized;
	}

	public static Vector3 Clamp(this Vector3 v, Vector3 min, Vector3 max)
	{
		return new Vector3(Mathf.Clamp(v.x, min.x, max.x), Mathf.Clamp(v.y, min.y, max.y), Mathf.Clamp(v.z, min.z, max.z));
	}

	public static Vector3 WithY(this Vector3 v, float newy)
	{
		return new Vector3(v.x, newy, v.z);
	}

	public static float GetMinComponent(this Vector3 v)
	{
		return Mathf.Min(Mathf.Min(v.x, v.y), v.z);
	}

	public static Vector3 RandomWithin(this Bounds b)
	{
		Vector3 uvw = new Vector3(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
		return b.UVWToPoint(uvw);
	}

	public static Vector3 UVWToPoint(this Bounds b, Vector3 uvw)
	{
		return b.min + Vector3.Scale(uvw, b.size);
	}

	public static Vector3 GetBottomCenter(this Bounds b)
	{
		return b.UVWToPoint(new Vector3(0.5f, 0f, 0.5f));
	}

	public static T GetComponentProfiled<T>(this Component component) where T : Component
	{
		return component.GetComponent<T>();
	}

	public static T GetComponentProfiled<T>(this GameObject gameObject) where T : Component
	{
		return gameObject.GetComponent<T>();
	}

	public static C FindAncestor<C>(this GameObject go) where C : Component
	{
		Transform transform = go.transform;
		while (transform != null && transform.gameObject.GetComponent(typeof(C)) == null)
		{
			transform = transform.parent;
		}
		if (transform != null)
		{
			return (C)transform.GetComponent(typeof(C));
		}
		return null;
	}

	public static int GetDistanceToParent(this GameObject go, GameObject par)
	{
		int num = 0;
		Transform transform = go.transform;
		while (transform != null && transform.gameObject != par)
		{
			transform = transform.parent;
			num++;
		}
		if (transform == null)
		{
			return -1;
		}
		return num;
	}

	public static C FindEnabledAncestor<C>(this GameObject go) where C : Behaviour
	{
		Transform transform = go.transform;
		while (transform != null)
		{
			C[] components = transform.gameObject.GetComponents<C>();
			transform = transform.parent;
			for (int i = 0; i < components.Length; i++)
			{
				if (components[i].enabled)
				{
					return components[i];
				}
			}
		}
		return null;
	}

	public static void DoDepthFirst(this GameObject go, GameObjectFunc func)
	{
		func(go);
		foreach (Transform item in go.transform)
		{
			item.gameObject.DoDepthFirst(func);
		}
	}

	public static void ForComponentInHierarchy<C>(this GameObject go, ComponentFunc<C> func) where C : Component
	{
		go.DoDepthFirst(delegate(GameObject child)
		{
			C component = child.GetComponent<C>();
			if (component != null)
			{
				func(component);
			}
		});
	}

	public static void SetHideFlagRecursive(this GameObject go, HideFlags flag)
	{
		go.hideFlags |= flag;
		foreach (Transform item in go.transform)
		{
			item.gameObject.SetHideFlagRecursive(flag);
		}
	}

	public static void RemoveSubset<T>(this HashSet<T> set, PickFunc<T> func)
	{
		List<T> list = new List<T>();
		foreach (T item in set)
		{
			if (func(item))
			{
				list.Add(item);
			}
		}
		foreach (T item2 in list)
		{
			set.Remove(item2);
		}
	}

	public static string[] SplitByChar(this string s, char c)
	{
		return s.Split(c);
	}

	[Obsolete("Use Add instead", true)]
	public static void SafeAdd<T>(this HashSet<T> set, T obj)
	{
		if (!set.Contains(obj))
		{
			set.Add(obj);
		}
	}

	public static bool UniqueAddSlow<T>(this List<T> list, T obj)
	{
		if (!list.Contains(obj))
		{
			list.Add(obj);
			return true;
		}
		return false;
	}

	public static int GetBitsPerPixel(TextureFormat format)
	{
		switch (format)
		{
		case TextureFormat.Alpha8:
			return 8;
		case TextureFormat.ARGB4444:
			return 16;
		case TextureFormat.RGBA4444:
			return 16;
		case TextureFormat.RGB24:
			return 24;
		case TextureFormat.RGBA32:
			return 32;
		case TextureFormat.ARGB32:
			return 32;
		case TextureFormat.RGB565:
			return 16;
		case TextureFormat.DXT1:
			return 4;
		case TextureFormat.DXT5:
			return 8;
		case TextureFormat.PVRTC_RGB2:
			return 2;
		case TextureFormat.PVRTC_RGBA2:
			return 2;
		case TextureFormat.PVRTC_RGB4:
			return 4;
		case TextureFormat.PVRTC_RGBA4:
			return 4;
		case TextureFormat.ETC_RGB4:
			return 4;
		case TextureFormat.ETC2_RGBA8:
			return 8;
		case TextureFormat.BGRA32:
			return 32;
		default:
			UnityEngine.Debug.LogError("TextureFormat not supported: " + format);
			return 0;
		}
	}

	public static int CalculateSizeBytes(this Texture tTexture)
	{
		int num = tTexture.width;
		int num2 = tTexture.height;
		if (tTexture is Texture2D)
		{
			Texture2D obj = tTexture as Texture2D;
			int bitsPerPixel = GetBitsPerPixel(obj.format);
			int mipmapCount = obj.mipmapCount;
			int i = 1;
			int num3 = 0;
			for (; i <= mipmapCount; i++)
			{
				num3 += num * num2 * bitsPerPixel / 8;
				num /= 2;
				num2 /= 2;
			}
			return num3;
		}
		if (tTexture is Cubemap)
		{
			int bitsPerPixel2 = GetBitsPerPixel((tTexture as Cubemap).format);
			return num * num2 * 6 * bitsPerPixel2 / 8;
		}
		return 0;
	}

	public static float CalculateSizeMB(this Texture tTexture)
	{
		return (float)tTexture.CalculateSizeBytes() / 1024f / 1024f;
	}

	public static bool IsBetween<T>(this T value, T low, T high) where T : IComparable<T>
	{
		if (value.CompareTo(low) >= 0)
		{
			return value.CompareTo(high) <= 0;
		}
		return false;
	}

	public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
	{
		if (val.CompareTo(min) < 0)
		{
			return min;
		}
		if (val.CompareTo(max) > 0)
		{
			return max;
		}
		return val;
	}

	public static float Clamp01(this float val)
	{
		return val.Clamp(0f, 1f);
	}

	public static int SafeMod(this int x, int m)
	{
		return (x % m + m) % m;
	}

	public static float Snap(this float x, float stride)
	{
		return stride * (float)Mathf.RoundToInt(x / stride);
	}

	public static string ToTwoDecimalString(this float x)
	{
		return x.ToString("F2");
	}

	public static int Snap(this int x, int step, int offset)
	{
		return (x - offset) / step * step + offset;
	}

	[Obsolete("Use System.Array.IndexOf instead", true)]
	public static int Find<T>(this T[] list, T query) where T : IEquatable<T>
	{
		for (int i = 0; i < list.Length; i++)
		{
			if (list[i].Equals(query))
			{
				return i;
			}
		}
		return -1;
	}

	[Obsolete("Use System.Array.IndexOf instead", true)]
	public static bool Contains<T>(this T[] list, T query) where T : IEquatable<T>
	{
		return list.Find(query) != -1;
	}

	public static T GetClamped<T>(this T[] list, int i)
	{
		int num = Mathf.Clamp(i, 0, list.Length - 1);
		return list[num];
	}

	[Obsolete("Use System.Array.IndexOf instead", true)]
	public static bool In<T>(this T source, params T[] list) where T : IEquatable<T>
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		return list.Contains(source);
	}

	public static float DistanceSqrXZ(this Vector3 from, Vector3 to)
	{
		float num = from.x - to.x;
		float num2 = from.z - to.z;
		return num * num + num2 * num2;
	}

	public static Vector3 XPart(this Vector3 v)
	{
		return new Vector3(v.x, 0f, 0f);
	}

	public static Vector3 YPart(this Vector3 v)
	{
		return new Vector3(0f, v.y, 0f);
	}

	public static Vector3 ZPart(this Vector3 v)
	{
		return new Vector3(0f, 0f, v.z);
	}

	public static bool InBox(this Vector3 v, Vector3 mins, Vector3 maxs)
	{
		if (v.x >= mins.x && v.y >= mins.y && v.z >= mins.z && v.x <= maxs.x && v.y <= maxs.y)
		{
			return v.z <= maxs.z;
		}
		return false;
	}

	public static Vector3 AddScalar(this Vector3 v, float s)
	{
		return new Vector3(v.x + s, v.y + s, v.z + s);
	}

	public static Vector3 _X0Z(this Vector3 v)
	{
		return new Vector3(v.x, 0f, v.z);
	}

	public static Vector3 _0Y0(this Vector3 v)
	{
		return new Vector3(0f, v.y, 0f);
	}

	public static Vector2 XZ(this Vector3 v)
	{
		return new Vector2(v.x, v.z);
	}

	public static Vector2 XY(this Vector3 v)
	{
		return new Vector2(v.x, v.y);
	}

	public static Vector3 Floor(this Vector3 v)
	{
		return new Vector3(Mathf.Floor(v.x), Mathf.Floor(v.y), Mathf.Floor(v.z));
	}

	public static Vector3 Round(this Vector3 v)
	{
		return new Vector3(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z));
	}

	public static bool HasAnyNaNs(this Vector3 v)
	{
		if (!float.IsNaN(v.x) && !float.IsNaN(v.y))
		{
			return float.IsNaN(v.z);
		}
		return true;
	}

	public static bool HasAnyInfs(this Vector3 v)
	{
		if (!float.IsInfinity(v.x) && !float.IsInfinity(v.y))
		{
			return float.IsInfinity(v.z);
		}
		return true;
	}

	public static Vector2 Fraction(this Vector2 v)
	{
		return new Vector2(v.x - Mathf.Floor(v.x), v.y - Mathf.Floor(v.y));
	}

	public static Vector2 PlusHalf(this Vector2 v)
	{
		return v + new Vector2(0.5f, 0.5f);
	}

	public static Vector2 Clamp(this Vector2 v, Rect r)
	{
		return new Vector2(v.x.Clamp(r.min.x, r.max.x), v.y.Clamp(r.min.y, r.max.y));
	}

	public static Vector2 Clamp(this Vector2 v, Vector2 min, Vector2 max)
	{
		return new Vector2(Mathf.Clamp(v.x, min.x, max.x), Mathf.Clamp(v.y, min.y, max.y));
	}

	public static Vector2 OneMinusY(this Vector2 v)
	{
		return new Vector2(v.x, 1f - v.y);
	}

	public static Vector2 GetSize(this Rect r)
	{
		return new Vector2(r.width, r.height);
	}

	public static Vector2 GetUV(this Rect r, Vector2 pos)
	{
		return new Vector2((pos.x - r.min.x) / r.width, (pos.y - r.min.y) / r.height);
	}

	public static void Write(this BinaryWriter writer, Vector3 v)
	{
		writer.WriteVector3(v);
	}

	public static void WriteVector3(this BinaryWriter writer, Vector3 v)
	{
		writer.WriteSingle(v.x);
		writer.WriteSingle(v.y);
		writer.WriteSingle(v.z);
	}

	public static Vector3 ReadVector3(this BinaryReader reader)
	{
		Vector3 zero = Vector3.zero;
		zero.x = reader.ReadSingle();
		zero.y = reader.ReadSingle();
		zero.z = reader.ReadSingle();
		return zero;
	}

	public static void Write(this BinaryWriter writer, Int3 v)
	{
		writer.WriteInt3(v);
	}

	public static void WriteInt3(this BinaryWriter writer, Int3 v)
	{
		writer.WriteInt32(v.x);
		writer.WriteInt32(v.y);
		writer.WriteInt32(v.z);
	}

	public static Int3 ReadInt3(this BinaryReader reader)
	{
		return new Int3(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
	}

	public static void Write(this BinaryWriter writer, Quaternion v)
	{
		writer.WriteQuaternion(v);
	}

	public static void WriteQuaternion(this BinaryWriter writer, Quaternion v)
	{
		writer.WriteSingle(v.w);
		writer.WriteSingle(v.x);
		writer.WriteSingle(v.y);
		writer.WriteSingle(v.z);
	}

	public static Quaternion ReadQuaternion(this BinaryReader reader)
	{
		Quaternion identity = Quaternion.identity;
		identity.w = reader.ReadSingle();
		identity.x = reader.ReadSingle();
		identity.y = reader.ReadSingle();
		identity.z = reader.ReadSingle();
		return identity;
	}

	public static void WriteColor(this BinaryWriter writer, Color c)
	{
		writer.WriteSingle(c.r);
		writer.WriteSingle(c.g);
		writer.WriteSingle(c.b);
		writer.WriteSingle(c.a);
	}

	public static Color ReadColor(this BinaryReader reader)
	{
		Color result = default(Color);
		result.r = reader.ReadSingle();
		result.g = reader.ReadSingle();
		result.b = reader.ReadSingle();
		result.a = reader.ReadSingle();
		return result;
	}

	[Obsolete]
	public static void WriteInt3Class(this BinaryWriter writer, Int3Class p)
	{
		writer.WriteInt32(p.x);
		writer.WriteInt32(p.y);
		writer.WriteInt32(p.z);
	}

	[Obsolete]
	public static Int3Class ReadInt3Class(this BinaryReader reader)
	{
		return new Int3Class
		{
			x = reader.ReadInt32(),
			y = reader.ReadInt32(),
			z = reader.ReadInt32()
		};
	}

	public static void WriteStringOrNull(this BinaryWriter writer, string s)
	{
		bool flag = s == null;
		writer.WriteBoolean(flag);
		if (!flag)
		{
			writer.WriteString(s);
		}
	}

	public static string ReadStringOrNull(this BinaryReader reader)
	{
		if (!reader.ReadBoolean())
		{
			return reader.ReadString();
		}
		return null;
	}

	public static void WriteSingle(this BinaryWriter writer, float val)
	{
		writer.Write(val);
	}

	public static void WriteInt64(this BinaryWriter writer, long val)
	{
		writer.Write(val);
	}

	public static void WriteInt32(this BinaryWriter writer, int val)
	{
		writer.Write(val);
	}

	public static void WriteUInt32(this BinaryWriter writer, uint val)
	{
		writer.Write(val);
	}

	public static void WriteByte(this BinaryWriter writer, byte val)
	{
		writer.Write(val);
	}

	public static void WriteBoolean(this BinaryWriter writer, bool val)
	{
		writer.Write(val);
	}

	public static void WriteString(this BinaryWriter writer, string val)
	{
		writer.Write(val);
	}

	public static void WriteBytes(this BinaryWriter writer, byte[] val)
	{
		writer.Write(val);
	}

	public static Int3 Dims<T>(this Array3<T> array)
	{
		return new Int3(array.sizeX, array.sizeY, array.sizeZ);
	}

	public static bool CheckBounds<T>(this Array3<T> array, Int3 p)
	{
		return array.CheckBounds(p.x, p.y, p.z);
	}

	public static T Get<T>(this Array3<T> array, Int3 p)
	{
		return array.Get(p.x, p.y, p.z);
	}

	public static void Set<T>(this Array3<T> array, Int3 p, T value)
	{
		array.Set(p.x, p.y, p.z, value);
	}

	public static Int3.RangeEnumerator Indices<T>(this Array3<T> array)
	{
		return new Int3.RangeEnumerator(new Int3(0, 0, 0), new Int3(array.sizeX - 1, array.sizeY - 1, array.sizeZ - 1));
	}

	public static bool CheckBounds<T>(this T[,] array, Int2 p)
	{
		if (p.x >= 0 && p.y >= 0 && p.x < array.GetLength(0))
		{
			return p.y < array.GetLength(1);
		}
		return false;
	}

	public static T Get<T>(this T[,] array, Int2 p)
	{
		return array[p.x, p.y];
	}

	public static T GetFlipX<T>(this T[,] array, Int2 p)
	{
		return array[array.GetLength(0) - p.x - 1, p.y];
	}

	public static Vector2 GetUV<T>(this T[,] array, Int2 p)
	{
		return new Vector2((float)p.x * 1f / (float)array.GetLength(0), (float)p.y * 1f / (float)array.GetLength(1));
	}

	public static T Get<T>(this T[,] array, Int2 p, T defaultVal)
	{
		if (!array.CheckBounds(p))
		{
			return defaultVal;
		}
		return array[p.x, p.y];
	}

	public static T GetClamped<T>(this T[,] array, Int2 p)
	{
		Int2 p2 = p.Clamp(new Int2(0), new Int2(array.GetLength(0) - 1, array.GetLength(1) - 1));
		return array.Get(p2);
	}

	public static T Set<T>(this T[,] array, Int2 p, T val)
	{
		return array[p.x, p.y] = val;
	}

	public static T SetFlipX<T>(this T[,] array, Int2 p, T val)
	{
		return array[array.GetLength(0) - p.x - 1, p.y] = val;
	}

	public static T SetFlipYX<T>(this T[,] array, Int2 p, T val)
	{
		return array[array.GetLength(1) - p.y - 1, p.x] = val;
	}

	public static T SetYX<T>(this T[,] array, Int2 p, T val)
	{
		return array[p.y, p.x] = val;
	}

	public static Int2.RangeEnumerator Indices<T>(this T[,] array)
	{
		return Int2.Indices(array);
	}

	public static void SavePNG<T>(this T[,] array, string pngPath, Array2DColoring<T> colorMap, bool flipXY = false)
	{
		Texture2D texture2D = new Texture2D(array.GetLength(0), array.GetLength(1));
		texture2D.name = "SystemExtensions.SavePNG";
		texture2D.Resize(array.Dims().x, array.Dims().y, TextureFormat.RGB24, hasMipMap: false);
		for (int i = 0; i < array.GetLength(0); i++)
		{
			for (int j = 0; j < array.GetLength(1); j++)
			{
				if (flipXY)
				{
					texture2D.SetPixel(j, array.GetLength(0) - 1 - i, colorMap(array, i, j));
				}
				else
				{
					texture2D.SetPixel(i, j, colorMap(array, i, j));
				}
			}
		}
		texture2D.Apply();
		File.WriteAllBytes(pngPath, texture2D.EncodeToPNG());
	}

	public static void SavePNG<T>(this T[,] array, string pngPath, ValueToColor<T> colorMap, bool flipXY = false)
	{
		Texture2D texture2D = new Texture2D(array.GetLength(0), array.GetLength(1));
		texture2D.name = "SystemExtensions.SavePNG";
		texture2D.Resize(array.Dims().x, array.Dims().y, TextureFormat.RGB24, hasMipMap: false);
		for (int i = 0; i < array.GetLength(0); i++)
		{
			for (int j = 0; j < array.GetLength(1); j++)
			{
				if (flipXY)
				{
					texture2D.SetPixel(j, array.GetLength(0) - 1 - i, colorMap(array[i, j]));
				}
				else
				{
					texture2D.SetPixel(i, j, colorMap(array[i, j]));
				}
			}
		}
		texture2D.Apply();
		File.WriteAllBytes(pngPath, texture2D.EncodeToPNG());
	}

	public static void SavePNG(this Texture2D tex, string path)
	{
		File.WriteAllBytes(path, tex.EncodeToPNG());
	}

	public static Int2 Dims<T>(this T[,] grid)
	{
		return new Int2(grid.GetLength(0), grid.GetLength(1));
	}

	public static Int3 Dims<T>(this T[,,] grid)
	{
		return new Int3(grid.GetLength(0), grid.GetLength(1), grid.GetLength(2));
	}

	public static float GetG3Blurred(this float[,] grid, Int2 center, int upsampleScale = 1)
	{
		float num = 0f;
		Int2.RangeEnumerator enumerator = Int2.Range(new Int2(-1, -1), new Int2(1, 1)).GetEnumerator();
		while (enumerator.MoveNext())
		{
			Int2 current = enumerator.Current;
			float num2 = GaussianKernel3x3.Get(current + new Int2(1, 1));
			Int2 p = ((center + current) / upsampleScale).Clamp(Int2.zero, grid.Dims() - 1);
			num += num2 * grid.Get(p);
		}
		return num;
	}

	public static float GetG3BlurredIndicator<T>(this T[,] grid, Int2 center, T refVal, int upsampleScale)
	{
		float num = 0f;
		Int2.RangeEnumerator enumerator = Int2.Range(new Int2(-1, -1), new Int2(1, 1)).GetEnumerator();
		while (enumerator.MoveNext())
		{
			Int2 current = enumerator.Current;
			float num2 = GaussianKernel3x3.Get(current + new Int2(1, 1));
			Int2 p = ((center + current) / upsampleScale).Clamp(Int2.zero, grid.Dims() - 1);
			if (grid.Get(p).Equals(refVal))
			{
				num += num2;
			}
		}
		return num;
	}

	public static float SmoothBilerp(this float[,] grid, Vector2 coord)
	{
		Int2 @int = Int2.FloorToInt2(coord);
		Int2 p = @int + new Int2(0, 1);
		Int2 p2 = @int + new Int2(1, 0);
		Int2 p3 = @int + new Int2(1, 1);
		float clamped = grid.GetClamped(@int);
		float clamped2 = grid.GetClamped(p);
		float clamped3 = grid.GetClamped(p2);
		float clamped4 = grid.GetClamped(p3);
		float num = Mathf.SmoothStep(0f, 1f, coord.x - (float)@int.x);
		float num2 = Mathf.SmoothStep(0f, 1f, coord.y - (float)@int.y);
		return clamped * (1f - num) * (1f - num2) + clamped3 * num * (1f - num2) + clamped2 * (1f - num) * num2 + clamped4 * num * num2;
	}

	public static float Bilerp(this float[,] grid, Vector2 coord)
	{
		Int2 @int = Int2.FloorToInt2(coord);
		Int2 p = @int + new Int2(0, 1);
		Int2 p2 = @int + new Int2(1, 0);
		Int2 p3 = @int + new Int2(1, 1);
		float clamped = grid.GetClamped(@int);
		float clamped2 = grid.GetClamped(p);
		float clamped3 = grid.GetClamped(p2);
		float clamped4 = grid.GetClamped(p3);
		float num = coord.x - (float)@int.x;
		float num2 = coord.y - (float)@int.y;
		return clamped * (1f - num) * (1f - num2) + clamped3 * num * (1f - num2) + clamped2 * (1f - num) * num2 + clamped4 * num * num2;
	}

	public static float BilerpUV(this float[,] grid, Vector2 uv)
	{
		Vector2 coord = new Vector2(uv.x * (float)grid.GetLength(0), uv.y * (float)grid.GetLength(1));
		return grid.Bilerp(coord);
	}

	public static float Bilerp(this int[,] grid, Vector2 coord)
	{
		Int2 @int = Int2.FloorToInt2(coord);
		Int2 p = @int + new Int2(0, 1);
		Int2 p2 = @int + new Int2(1, 0);
		Int2 p3 = @int + new Int2(1, 1);
		int clamped = grid.GetClamped(@int);
		int clamped2 = grid.GetClamped(p);
		int clamped3 = grid.GetClamped(p2);
		int clamped4 = grid.GetClamped(p3);
		float num = coord.x - (float)@int.x;
		float num2 = coord.y - (float)@int.y;
		return (float)clamped * (1f - num) * (1f - num2) + (float)clamped3 * num * (1f - num2) + (float)clamped2 * (1f - num) * num2 + (float)clamped4 * num * num2;
	}

	public static float BilerpUV(this int[,] grid, Vector2 uv)
	{
		Vector2 coord = new Vector2(uv.x * (float)grid.GetLength(0), uv.y * (float)grid.GetLength(1));
		return grid.Bilerp(coord);
	}

	public static float Bilerp(this ushort[,] grid, Vector2 coord)
	{
		Int2 @int = Int2.FloorToInt2(coord);
		Int2 p = @int + new Int2(0, 1);
		Int2 p2 = @int + new Int2(1, 0);
		Int2 p3 = @int + new Int2(1, 1);
		ushort clamped = grid.GetClamped(@int);
		ushort clamped2 = grid.GetClamped(p);
		ushort clamped3 = grid.GetClamped(p2);
		ushort clamped4 = grid.GetClamped(p3);
		float num = coord.x - (float)@int.x;
		float num2 = coord.y - (float)@int.y;
		return (float)(int)clamped * (1f - num) * (1f - num2) + (float)(int)clamped3 * num * (1f - num2) + (float)(int)clamped2 * (1f - num) * num2 + (float)(int)clamped4 * num * num2;
	}

	public static float BilerpUV(this ushort[,] grid, Vector2 uv)
	{
		Vector2 coord = new Vector2(uv.x * (float)grid.GetLength(0), uv.y * (float)grid.GetLength(1));
		return grid.Bilerp(coord);
	}

	public static bool EqualAround<T>(this T[,] grid, Int2 cell, T refVal, int numRings)
	{
		Int2.RangeEnumerator enumerator = Int2.Range(cell - numRings, cell + numRings + 1).GetEnumerator();
		while (enumerator.MoveNext())
		{
			Int2 current = enumerator.Current;
			if (!grid.CheckBounds(current))
			{
				return false;
			}
			if (!grid.Get(current).Equals(refVal))
			{
				return false;
			}
		}
		return true;
	}

	public static void Relax(this float[,] grid, bool[,] isFixed, float[,] relaxed)
	{
		Int2.RangeEnumerator enumerator = Int2.Indices(grid).GetEnumerator();
		while (enumerator.MoveNext())
		{
			Int2 current = enumerator.Current;
			if (isFixed.Get(current))
			{
				relaxed.Set(current, grid.Get(current));
				continue;
			}
			float num = 0f;
			Int2.RangeEnumerator enumerator2 = Int2.Range(current - 1, current + 1).GetEnumerator();
			while (enumerator2.MoveNext())
			{
				Int2 current2 = enumerator2.Current;
				if (current2 != current)
				{
					num += grid.GetClamped(current2);
				}
			}
			relaxed.Set(current, num / 8f);
		}
	}

	public static void Linear2XResample(this float[,] grid, out float[,] result)
	{
		Int2 @int = Int2.Dims(grid);
		result = new float[@int.x * 2 - 1, @int.y * 2 - 1];
		Int2.RangeEnumerator enumerator = Int2.Indices(result).GetEnumerator();
		while (enumerator.MoveNext())
		{
			Int2 current = enumerator.Current;
			Vector2 coord = current.ToVector2() * 0.5f;
			result.Set(current, grid.Bilerp(coord));
		}
	}

	public static Array3<bool> ComputeDual(this Array3<bool> primal)
	{
		Array3<bool> array = new Array3<bool>(primal.sizeX + 1, primal.sizeY + 1, primal.sizeZ + 1);
		foreach (Int3 item in array.Indices())
		{
			array.Set(item, value: false);
			foreach (Int3 item2 in Int3.Range(item - 1, item))
			{
				if (primal.CheckBounds(item2) && primal.Get(item2))
				{
					array.Set(item, value: true);
					break;
				}
			}
		}
		return array;
	}

	public static bool[,] ComputeDual(this bool[,] primal)
	{
		Int2 @int = Int2.Dims(primal);
		bool[,] array = new bool[@int.x + 1, @int.y + 1];
		Int2.RangeEnumerator enumerator = array.Indices().GetEnumerator();
		while (enumerator.MoveNext())
		{
			Int2 current = enumerator.Current;
			array.Set(current, val: false);
			Int2.RangeEnumerator enumerator2 = Int2.Range(current - 1, current).GetEnumerator();
			while (enumerator2.MoveNext())
			{
				Int2 current2 = enumerator2.Current;
				if (primal.CheckBounds(current2) && primal.Get(current2))
				{
					array.Set(current, val: true);
					break;
				}
			}
		}
		return array;
	}

	public static float Trilerp(this bool[,,] grid, Vector3 coord)
	{
		int num = Mathf.FloorToInt(coord.x);
		int num2 = Mathf.FloorToInt(coord.y);
		int num3 = Mathf.FloorToInt(coord.z);
		int num4 = num + 1;
		int num5 = num2 + 1;
		int num6 = num3 + 1;
		float num7 = (grid[num, num2, num3] ? 1f : 0f);
		float num8 = (grid[num, num2, num6] ? 1f : 0f);
		float num9 = (grid[num, num5, num3] ? 1f : 0f);
		float num10 = (grid[num, num5, num6] ? 1f : 0f);
		float num11 = (grid[num4, num2, num3] ? 1f : 0f);
		float num12 = (grid[num4, num2, num6] ? 1f : 0f);
		float num13 = (grid[num4, num5, num3] ? 1f : 0f);
		float num14 = (grid[num4, num5, num6] ? 1f : 0f);
		float num15 = coord.x - (float)num;
		float num16 = coord.y - (float)num2;
		float num17 = coord.z - (float)num3;
		float num18 = (1f - num15) * num7 + num15 * num11;
		float num19 = (1f - num15) * num9 + num15 * num13;
		float num20 = (1f - num15) * num8 + num15 * num12;
		float num21 = (1f - num15) * num10 + num15 * num14;
		float num22 = (1f - num16) * num18 + num16 * num19;
		float num23 = (1f - num16) * num20 + num16 * num21;
		return (1f - num17) * num22 + num17 * num23;
	}

	public static float Bilerp(this bool[,] grid, Vector2 coord)
	{
		Int2 @int = Int2.FloorToInt2(coord);
		Int2 p = @int + new Int2(0, 1);
		Int2 p2 = @int + new Int2(1, 0);
		Int2 p3 = @int + new Int2(1, 1);
		float num = (grid.Get(@int) ? 1f : 0f);
		float num2 = (grid.Get(p) ? 1f : 0f);
		float num3 = (grid.Get(p2) ? 1f : 0f);
		float num4 = (grid.Get(p3) ? 1f : 0f);
		float num5 = coord.x - (float)@int.x;
		float num6 = coord.y - (float)@int.y;
		return num * (1f - num5) * (1f - num6) + num3 * num5 * (1f - num6) + num2 * (1f - num5) * num6 + num4 * num5 * num6;
	}

	public static string GetFullHierarchyPath(this GameObject go)
	{
		string text = go.name;
		if (go.transform.parent == null)
		{
			return text;
		}
		GameObject gameObject = go.transform.parent.gameObject;
		while (gameObject != null)
		{
			text = gameObject.name + "/" + text;
			gameObject = ((!(gameObject.transform.parent != null)) ? null : gameObject.transform.parent.gameObject);
		}
		return text;
	}

	public static void CopyLocals(this Transform dest, Transform src)
	{
		dest.localPosition = src.localPosition;
		dest.localRotation = src.localRotation;
		dest.localScale = src.localScale;
	}

	public static void MakeLocalsIdentity(this Transform trans)
	{
		trans.localPosition = Vector3.zero;
		trans.localRotation = Quaternion.identity;
		trans.localScale = new Vector3(1f, 1f, 1f);
	}

	public static bool AreLocalsNearIdentity(this Transform trans)
	{
		if ((double)trans.localPosition.magnitude > 0.0001)
		{
			return false;
		}
		if ((double)trans.eulerAngles.magnitude > 0.0001)
		{
			return false;
		}
		if ((double)(trans.localScale - new Vector3(1f, 1f, 1f)).magnitude > 0.0001)
		{
			return false;
		}
		return true;
	}

	public static int GetCCWYawTurns(this Transform trans)
	{
		float y = trans.eulerAngles.y;
		return (4 - Mathf.RoundToInt(y / 90f)) % 4;
	}

	public static float ToFloat(this bool b)
	{
		if (!b)
		{
			return 0f;
		}
		return 1f;
	}

	public static float Min(this float[,] grid)
	{
		float num = grid[0, 0];
		Int2.RangeEnumerator enumerator = grid.Indices().GetEnumerator();
		while (enumerator.MoveNext())
		{
			Int2 current = enumerator.Current;
			num = Mathf.Min(grid.Get(current), num);
		}
		return num;
	}

	public static float Max(this float[,] grid)
	{
		float num = grid[0, 0];
		Int2.RangeEnumerator enumerator = grid.Indices().GetEnumerator();
		while (enumerator.MoveNext())
		{
			Int2 current = enumerator.Current;
			num = Mathf.Max(grid.Get(current), num);
		}
		return num;
	}

	public static Color ToOpaque(this Color c)
	{
		return new Color(c.r, c.g, c.b, 1f);
	}

	public static Color ToAlpha(this Color c, float alpha)
	{
		return new Color(c.r, c.g, c.b, alpha);
	}

	public static Color WithAlpha(this Color c, float alpha)
	{
		return new Color(c.r, c.g, c.b, alpha);
	}

	public static StreamReader ToReader(this string s)
	{
		return new StreamReader(new MemoryStream(Encoding.ASCII.GetBytes(s)));
	}

	public static Int2 GetSize(this Texture2D tex)
	{
		return new Int2(tex.width, tex.height);
	}

	public static bool CheckBounds(this Texture2D tex, Int2 p)
	{
		if (p >= Int2.zero)
		{
			return p < tex.GetSize();
		}
		return false;
	}

	public static Int2.RangeEnumerator Indices(this Texture2D tex)
	{
		return Int2.Range(tex.GetSize());
	}

	public static bool ApproxEquals(this Color u, Color v, float eps)
	{
		if (Mathf.Abs(u.r - v.r) < eps && Mathf.Abs(u.g - v.g) < eps)
		{
			return Mathf.Abs(u.b - v.b) < eps;
		}
		return false;
	}

	public static Vector3 AddScreenOffset(this Camera cam, Vector3 wsPos, Vector3 ssOffset)
	{
		return cam.ScreenToWorldPoint(cam.WorldToScreenPoint(wsPos) + ssOffset);
	}

	public static int CycleNext<T>(this Enum val)
	{
		int num = Convert.ToInt32(val) + 1;
		int length = Enum.GetValues(typeof(T)).Length;
		return num % length;
	}

	public static string ToValidFileName(this string s)
	{
		string text = s;
		char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
		foreach (char oldChar in invalidFileNameChars)
		{
			text = text.Replace(oldChar, '_');
		}
		return text;
	}

	public static string SubstringFromOccuranceOf(this string s, string query, int occurance)
	{
		int num = s.IndexOf(query);
		for (int i = 0; i < occurance; i++)
		{
			if (num == -1)
			{
				break;
			}
			num = s.IndexOf(query, num + 1);
		}
		return s.Substring(num + 1);
	}

	public static V LookupSafe<K, V>(this Dictionary<K, V> dict, K key, V defaultValue)
	{
		if (dict.ContainsKey(key))
		{
			return dict[key];
		}
		return defaultValue;
	}

	public static bool Equals(this Color32 a, Color32 b)
	{
		if (a.r == b.r && a.g == b.g && a.b == b.b)
		{
			return a.a == b.a;
		}
		return false;
	}

	public static Int3 ToInt3(this Color32 c)
	{
		return Int3.FromRGB(c);
	}

	public static void Restart(this Stopwatch sw)
	{
		sw.Reset();
		sw.Start();
	}

	public static string NullOrID(this UnityEngine.Object o)
	{
		if (o == null)
		{
			return "null";
		}
		return string.Concat(o.GetInstanceID());
	}

	public static Quaternion GetInverse(this Quaternion q)
	{
		return Quaternion.Inverse(q);
	}

	public static float JustAngle(this Quaternion q)
	{
		q.ToAngleAxis(out var angle, out var _);
		return angle;
	}

	public static string Truncate(this string source, int length)
	{
		if (source.Length > length)
		{
			source = source.Substring(0, length);
		}
		return source;
	}

	public static List<T> SubtractFrom<T>(this HashSet<T> set, ICollection<T> col)
	{
		List<T> list = new List<T>();
		foreach (T item in col)
		{
			if (!set.Contains(item))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static byte NextByte(this ref Unity.Mathematics.Random rng)
	{
		return Convert.ToByte(rng.NextInt(256));
	}

	public static Vector3 WithZ(this Vector2 xy, float z)
	{
		return new Vector3(xy.x, xy.y, z);
	}

	public static bool IsEqualTo<T>(this T[] a, T[] b)
	{
		return a.IsEqualTo(b, EqualityComparer<T>.Default);
	}

	public static bool IsEqualTo<T>(this T[] a, T[] b, IEqualityComparer<T> comparer)
	{
		if (a == null && b == null)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (a.Length != b.Length)
		{
			return false;
		}
		for (int i = 0; i < a.Length; i++)
		{
			if (!comparer.Equals(a[i], b[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static IEnumerable<T> Zip<A, B, T>(this IEnumerable<A> seqA, IEnumerable<B> seqB, Func<A, B, T> func)
	{
		if (seqA == null)
		{
			throw new ArgumentNullException("seqA");
		}
		if (seqB == null)
		{
			throw new ArgumentNullException("seqB");
		}
		return seqA.Zip35Deferred(seqB, func);
	}

	private static IEnumerable<T> Zip35Deferred<A, B, T>(this IEnumerable<A> seqA, IEnumerable<B> seqB, Func<A, B, T> func)
	{
		using (IEnumerator<A> iteratorA = seqA.GetEnumerator())
		{
			using (IEnumerator<B> iteratorB = seqB.GetEnumerator())
			{
				while (iteratorA.MoveNext() && iteratorB.MoveNext())
				{
					yield return func(iteratorA.Current, iteratorB.Current);
				}
			}
		}
	}

	public static string GetLongestCommonPrefix(this string[] names, char separator, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
	{
		if (names == null || names.Length == 0)
		{
			return string.Empty;
		}
		string[] value = names.Select((string dir) => dir.Split(separator)).Aggregate((string[] first, string[] second) => (from pair in first.Zip(second, (string a, string b) => new KeyValuePair<string, string>(a, b)).TakeWhile((KeyValuePair<string, string> pair) => pair.Key.Equals(pair.Value, comparison))
			select pair.Key).ToArray());
		return string.Join(separator.ToString(), value);
	}
}
