using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FMOD;
using Platform.IO;
using UnityEngine;

public static class MathExtensions
{
	public const float HALFPI = (float)Math.PI / 2f;

	public const float fixedDeltaTime = 0.02f;

	public const int maxPhysicsIterations = 20;

	private static List<char> _invaidFileChars;

	private static List<string> sNames = new List<string>();

	private static readonly Quaternion[] capsuleRotations = new Quaternion[3]
	{
		Quaternion.Euler(0f, 0f, -90f),
		Quaternion.identity,
		Quaternion.Euler(90f, 0f, 0f)
	};

	private static List<char> invalidFileChars
	{
		get
		{
			if (_invaidFileChars == null)
			{
				_invaidFileChars = new List<char>(Platform.IO.Path.GetInvalidFileNameChars());
				_invaidFileChars.AddRange(Platform.IO.Path.GetInvalidPathChars());
			}
			return _invaidFileChars;
		}
	}

	public static Vector3 Divide(Vector3 a, Vector3 b)
	{
		return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
	}

	public static bool Compare(float a, float b, float e)
	{
		return (a - b - e) * (a - b + e) <= 0f;
	}

	public static Vector3 RotateVectorAroundAxisAngle(Vector3 n, Vector3 v, float a)
	{
		n.Normalize();
		float num = Mathf.Cos(a);
		float num2 = Mathf.Sin(a);
		return v * num + Vector3.Dot(v, n) * n * (1f - num) + Vector3.Cross(n, v) * num2;
	}

	public static float RepeatAngle(float angle)
	{
		return angle - 360f * (1f + Mathf.Floor((angle - 180f) / 360f));
	}

	public static int EncodeIndex(int x, int y)
	{
		return (x + y) * (x + y + 1) / 2 + y;
	}

	public static float StableLerp(float current, float target, float coef, float deltaTime)
	{
		return StableLerp(current, target, coef, coef, deltaTime);
	}

	public static float StableLerp(float current, float target, float coefUp, float coefDown, float deltaTime)
	{
		float num = 1f - ((current <= target) ? coefUp : coefDown) / 60f;
		if (num <= 0f)
		{
			return target;
		}
		float p = 60f * deltaTime;
		float num2 = Mathf.Pow(num, p);
		return Mathf.Lerp(current, target, 1f - num2);
	}

	public static void Normalize(List<float> values)
	{
		int count = values.Count;
		if (values != null && count != 0)
		{
			float num = 0f;
			for (int i = 0; i < count; i++)
			{
				float num3 = (values[i] = Mathf.Max(0f, values[i]));
				num += num3;
			}
			for (int j = 0; j < count; j++)
			{
				values[j] /= num;
			}
		}
	}

	public static Vector2 FrustumSize(Camera cam, float distance)
	{
		return FrustumSize(cam.fieldOfView, cam.aspect, distance);
	}

	public static Vector2 FrustumSize(float fieldOfView, float aspect, float distance)
	{
		Vector2 result = default(Vector2);
		result.y = distance * Mathf.Tan(fieldOfView * 0.5f * ((float)Math.PI / 180f));
		result.x = result.y * aspect;
		return result;
	}

	public static float SmoothValue(float value, float factor)
	{
		value = Mathf.Clamp01(value);
		factor = Mathf.Clamp01(factor);
		return 1f - value + 2f * Mathf.Sqrt(value) * factor - factor * factor;
	}

	public static float EaseInSine(float value)
	{
		return Mathf.Sin((value - 1f) * (float)Math.PI * 0.5f) + 1f;
	}

	public static float EaseOutSine(float value)
	{
		return Mathf.Sin(value * (float)Math.PI * 0.5f);
	}

	public static float EaseInOutSine(float value)
	{
		return 0.5f * (1f - Mathf.Cos((float)Math.PI * value));
	}

	public static float EvaluateLine(float x1, float y1, float x2, float y2, float x)
	{
		float num = (y2 - y1) / (x2 - x1);
		float num2 = y1 - num * x1;
		return num * x + num2;
	}

	public static void Oscillation(float reduction, float frequency, float seed, float t, out float o, out float o1)
	{
		seed = Mathf.Clamp01(seed);
		float num = 0.5f * Mathf.Pow(reduction, 0f - t);
		float num2 = Mathf.Sin((t * frequency + seed) * 2f * (float)Math.PI);
		o = num * (1f + num2);
		o1 = num * (1f - num2);
	}

	public static bool CoinRotation(ref float current, float target, ref float timePrev, float timeNow, ref float velocity, float springCoef, float velocityDamp, float velocityMax)
	{
		float num = 0.02f;
		float num2 = timeNow - timePrev;
		int num3 = Mathf.FloorToInt(num2);
		if (num3 > 20)
		{
			num3 = 1;
			num = num2;
		}
		timePrev += (float)num3 * num;
		float a = current;
		for (int i = 0; i < num3; i++)
		{
			Spring(ref velocity, ref current, target, springCoef, num, velocityDamp, velocityMax);
			if (Mathf.Abs(target - current) < 1f && Mathf.Abs(velocity) < 1f)
			{
				velocity = 0f;
				current = target;
			}
		}
		return !Mathf.Approximately(a, current);
	}

	public static void Spring(ref float velocity, ref float current, float target, float coef, float dT, float velocityDamp, float velocityMax = -1f)
	{
		float num = target - current;
		float num2 = coef * num;
		velocity = (velocity + num2 * dT) * velocityDamp;
		if (velocityMax > 0f)
		{
			float num3 = ((velocity >= 0f) ? 1f : (-1f));
			if (velocity * num3 > velocityMax)
			{
				velocity = velocityMax * num3;
			}
		}
		current += velocity * dT;
	}

	public static void UniqueRandomNumbersInRange(int min, int max, int count, ref List<int> numbers)
	{
		if (count <= 0)
		{
			return;
		}
		if (numbers.Capacity < count)
		{
			numbers.Capacity = count;
		}
		if (max - min < count)
		{
			UnityEngine.Debug.LogError("MathExtensions.UniqueRandomNumbersInRange : Specified range is below required numbers count! Make sure that (max - min) >= count");
			return;
		}
		int num = 0;
		while (num < count)
		{
			int item = UnityEngine.Random.Range(min, max);
			if (!numbers.Contains(item))
			{
				numbers.Add(item);
				num++;
			}
		}
	}

	public static string Color2Hex(Color c, float alpha = 1f)
	{
		int num = Mathf.RoundToInt(255f * c.r);
		int num2 = Mathf.RoundToInt(255f * c.g);
		int num3 = Mathf.RoundToInt(255f * c.b);
		int num4 = Mathf.RoundToInt(255f * c.a * alpha);
		return $"{num:X2}{num2:X2}{num3:X2}{num4:X2}";
	}

	public static Texture2D ScaleTexture(Texture2D src, int width, int height, bool mipmap, bool linear = false)
	{
		Texture2D texture2D = new Texture2D(width, height, src.format, mipmap, linear);
		texture2D.name = "MathExtensions.ScaleTexture";
		texture2D.wrapMode = TextureWrapMode.Clamp;
		float num = 1f / (float)width;
		float num2 = 1f / (float)height;
		float num3 = 0f;
		float num4 = 0f;
		num3 = 0.5f;
		num4 = 0.5f;
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				texture2D.SetPixel(i, j, src.GetPixelBilinear(num * ((float)i + num3), num2 * ((float)j + num4)));
			}
		}
		texture2D.Apply();
		return texture2D;
	}

	public static Texture2D ScaleTexture(Texture2D src, int width, bool mipmap, bool linear = false)
	{
		float num = (float)src.height / (float)src.width;
		return ScaleTexture(src, width, Mathf.FloorToInt(num * (float)width), mipmap, linear);
	}

	public static Texture2D LoadTexture(byte[] bytes)
	{
		Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false, linear: false);
		texture2D.name = "MathExtensions.LoadTexture";
		if (!texture2D.LoadImage(bytes))
		{
			return null;
		}
		texture2D.wrapMode = TextureWrapMode.Clamp;
		return texture2D;
	}

	public static string FilterNonFileChars(string input)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (char c in input)
		{
			if (!invalidFileChars.Contains(c))
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	public static string GetUniqueFileName(Platform.IO.DirectoryInfo dirInfo, string prefix, string extension, int numberOfDigits, bool startFromOne, bool dense)
	{
		if (dirInfo == null || !dirInfo.Exists)
		{
			return null;
		}
		string searchPattern = prefix + "*." + extension;
		string text = "D" + numberOfDigits;
		numberOfDigits = Mathf.Clamp(numberOfDigits, 1, 20);
		prefix = FilterNonFileChars(prefix);
		extension = FilterNonFileChars(extension);
		int num = (startFromOne ? 1 : 0);
		int num2 = -1;
		Platform.IO.FileInfo[] files = dirInfo.GetFiles(searchPattern, SearchOption.TopDirectoryOnly);
		List<int> list = new List<int>();
		int length = prefix.Length;
		int i = 0;
		for (int num3 = files.Length; i < num3; i++)
		{
			string name = files[i].Name;
			string text2 = name.Substring(length, name.LastIndexOf('.') - length);
			int result = -1;
			if (text2.Length >= numberOfDigits && int.TryParse(text2, out result) && result >= 0)
			{
				list.Add(result);
				if (result > num2)
				{
					num2 = result;
				}
			}
		}
		if (dense)
		{
			int count = list.Count;
			if (count > 1)
			{
				list.Sort();
				int num4 = num;
				for (int j = 0; j < count; j++)
				{
					int num5 = list[j];
					int num6 = num5 - num4;
					if (num6 < 0)
					{
						num = num4;
						break;
					}
					if (num6 > 1)
					{
						num = num4 + 1;
						break;
					}
					if (num6 == 1)
					{
						if (j == count - 1)
						{
							num = num5 + 1;
						}
						else
						{
							num4 = num5;
						}
					}
				}
			}
			else if (count == 1 && num == list[0])
			{
				num++;
			}
		}
		else
		{
			num = Mathf.Max(num, num2 + 1);
		}
		return prefix + num.ToString(text) + "." + extension;
	}

	public static void RectFit(float width, float height, float parentWidth, float parentHeight, RectScaleMode mode, out Vector2 scale, out Vector2 offset)
	{
		RectFit(width, height, parentWidth / parentHeight, mode, out scale, out offset);
	}

	public static void RectFit(float width, float height, float parentAspect, RectScaleMode mode, out Vector2 scale, out Vector2 offset)
	{
		float num = width / height;
		switch (mode)
		{
		case RectScaleMode.Fit:
			if (num > parentAspect)
			{
				scale = new Vector2(1f, num / parentAspect);
			}
			else
			{
				scale = new Vector2(parentAspect / num, 1f);
			}
			break;
		case RectScaleMode.Envelope:
			if (num > parentAspect)
			{
				scale = new Vector2(parentAspect / num, 1f);
			}
			else
			{
				scale = new Vector2(1f, num / parentAspect);
			}
			break;
		default:
			scale = new Vector2(1f, 1f);
			break;
		}
		offset = new Vector2((1f - scale.x) * 0.5f, (1f - scale.y) * 0.5f);
	}

	public static float Trapezoid(float a, float b, float c, float d, float t, bool wrap = true)
	{
		float num = a + b + c + d;
		if (wrap)
		{
			t %= num;
		}
		return Mathf.Clamp01(Mathf.Min((b > 0f) ? ((t - a) / b) : 0f, (d > 0f) ? ((num - t) / d) : 0f));
	}

	public static float Step(float a, float b, float t, bool wrap = true)
	{
		if (wrap)
		{
			t %= a + b;
		}
		return Mathf.Clamp01((b > 0f) ? ((t - a) / b) : 0f);
	}

	public static void ShuffleAll<T>(IList<T> list, T current)
	{
		int count = list.Count;
		if (count == 0)
		{
			return;
		}
		int num = list.IndexOf(current);
		if (num == count - 1)
		{
			num = 0;
			if (num != count - 1)
			{
				T value = list[num];
				list[num] = list[count - 1];
				list[count - 1] = value;
			}
		}
		ShuffleAll(list, num);
	}

	public static void ShuffleAll<T>(IList<T> list, int n = -1)
	{
		int count = list.Count;
		if (count == 0)
		{
			return;
		}
		if (n < 0 || n >= count - 1)
		{
			n = -1;
		}
		while (n < count - 1)
		{
			n++;
			int num = UnityEngine.Random.Range(n, count);
			if (n != num)
			{
				T value = list[n];
				list[n] = list[num];
				list[num] = value;
			}
		}
	}

	public static T Shuffle<T>(IList<T> list, ref int n)
	{
		int count = list.Count;
		if (count == 0)
		{
			return default(T);
		}
		if (n < 0 || n >= count - 1)
		{
			n = -1;
		}
		n++;
		int num = UnityEngine.Random.Range(n, count);
		if (n != num)
		{
			T value = list[n];
			list[n] = list[num];
			list[num] = value;
		}
		return list[n];
	}

	public static void ExecuteOnAllChildren(Transform t, Action<Transform> a)
	{
		if (a != null)
		{
			a(t);
			for (int i = 0; i < t.childCount; i++)
			{
				ExecuteOnAllChildren(t.GetChild(i), a);
			}
		}
	}

	public static string FormatPath(Transform t)
	{
		if (t == null)
		{
			return string.Empty;
		}
		string text = string.Empty;
		while (t.parent != null)
		{
			text = $"/{t.name}{text}";
			t = t.parent;
		}
		return text;
	}

	public static string FormatPath(Transform t, int maxCharsInLine, string indent)
	{
		if (t == null)
		{
			return string.Empty;
		}
		sNames.Clear();
		int num = 0;
		Transform transform = t;
		while (transform != null)
		{
			string name = transform.name;
			sNames.Add(name);
			num += name.Length;
			transform = transform.parent;
		}
		if (num == 0)
		{
			return string.Empty;
		}
		num += sNames.Count - 1;
		int num2 = ((maxCharsInLine <= 0) ? 1 : ((num + maxCharsInLine - 1) / maxCharsInLine));
		if (!string.IsNullOrEmpty(indent))
		{
			num += num2 * indent.Length;
		}
		StringBuilder stringBuilder = new StringBuilder(num);
		int num3 = 0;
		for (int num4 = sNames.Count - 1; num4 >= 0; num4--)
		{
			string text = sNames[num4];
			for (int i = ((num4 != sNames.Count - 1) ? (-1) : 0); i < text.Length; i++)
			{
				stringBuilder.Append((i == -1) ? '/' : text[i]);
				num3++;
				if (num3 == maxCharsInLine)
				{
					num3 = 0;
					stringBuilder.Append('\n');
					if (!string.IsNullOrEmpty(indent))
					{
						stringBuilder.Append(indent);
					}
				}
			}
		}
		return stringBuilder.ToString();
	}

	public static void Sphere(ref Mesh mesh, float radius, int segments)
	{
		Sphere(ref mesh, radius, segments, segments / 2);
	}

	public static void Sphere(ref Mesh mesh, float radius, int cols, int rows)
	{
		if (cols < 3)
		{
			cols = 3;
		}
		if (rows < 2)
		{
			rows = 2;
		}
		int num = (rows - 1) * (cols + 1) + cols * 2;
		Vector3[] array = new Vector3[num];
		Vector2[] array2 = new Vector2[num];
		Vector3[] array3 = new Vector3[num];
		int[] array4 = new int[(rows - 1) * cols * 6];
		float num2 = 0.25f;
		for (int i = 0; i <= cols; i++)
		{
			float num3 = (float)i / (float)cols;
			float f = (num2 + num3) * 2f * (float)Math.PI;
			float num4 = Mathf.Cos(f);
			float num5 = Mathf.Sin(f);
			for (int j = 0; j <= rows; j++)
			{
				if (i != cols || (j != 0 && j != rows))
				{
					float num6 = (float)j / (float)rows;
					float f2 = num6 * (float)Math.PI;
					float num7 = Mathf.Sin(f2);
					float num8 = Mathf.Cos(f2);
					Vector3 vector = new Vector3(num4 * num7, 0f - num8, num5 * num7);
					int num9 = ((j > 0) ? (cols + (j - 1) * (cols + 1) + i) : i);
					array[num9] = radius * vector;
					array2[num9] = new Vector2(num3, num6);
					array3[num9] = vector;
				}
			}
			if (i < cols)
			{
				array4[i * 3] = i;
				array4[i * 3 + 1] = cols + i;
				array4[i * 3 + 2] = cols + i + 1;
				for (int k = 1; k < rows - 1; k++)
				{
					int num10 = cols + (k - 1) * (cols + 1) + i;
					int num11 = cols + k * (cols + 1) + i;
					int num12 = num10;
					int num13 = num10 + 1;
					int num14 = num11;
					int num15 = num11 + 1;
					int num16 = cols * 3 + ((k - 1) * cols + i) * 6;
					array4[num16] = num12;
					array4[num16 + 1] = num14;
					array4[num16 + 2] = num15;
					array4[num16 + 3] = num15;
					array4[num16 + 4] = num13;
					array4[num16 + 5] = num12;
				}
				int num17 = cols + (rows - 2) * (cols + 1) + i;
				int num18 = num17 + 1;
				int num19 = cols + (rows - 1) * (cols + 1) + i;
				int num20 = cols * 3 + cols * (rows - 2) * 6 + i * 3;
				array4[num20] = num17;
				array4[num20 + 1] = num19;
				array4[num20 + 2] = num18;
			}
		}
		if (mesh == null)
		{
			mesh = new Mesh();
		}
		else
		{
			mesh.Clear();
		}
		mesh.vertices = array;
		mesh.uv = array2;
		mesh.normals = array3;
		mesh.SetTriangles(array4, 0, calculateBounds: true);
	}

	public static void Hemisphere(ref Mesh mesh, float radius, int segments)
	{
		Hemisphere(ref mesh, radius, segments, segments / 4);
	}

	public static void Hemisphere(ref Mesh mesh, float radius, int cols, int rows)
	{
		if (cols < 3)
		{
			cols = 3;
		}
		if (rows < 1)
		{
			rows = 1;
		}
		int num = rows * (cols + 1) + cols;
		Vector3[] array = new Vector3[num];
		Vector2[] array2 = new Vector2[num];
		Vector3[] array3 = new Vector3[num];
		int[] array4 = new int[(rows - 1) * cols * 6 + cols * 3];
		float num2 = 0.25f;
		for (int i = 0; i <= cols; i++)
		{
			float num3 = (float)i / (float)cols;
			float f = (num2 + num3) * 2f * (float)Math.PI;
			float num4 = Mathf.Cos(f);
			float num5 = Mathf.Sin(f);
			for (int j = 0; j <= rows; j++)
			{
				if (i != cols || j != rows)
				{
					float num6 = (float)j / (float)rows;
					float f2 = num6 * 0.5f * (float)Math.PI;
					float num7 = Mathf.Cos(f2);
					float y = Mathf.Sin(f2);
					Vector3 vector = new Vector3(num4 * num7, y, num5 * num7);
					int num8 = j * (cols + 1) + i;
					array[num8] = radius * vector;
					array2[num8] = new Vector2(num3, num6);
					array3[num8] = vector;
				}
			}
			if (i < cols)
			{
				for (int k = 0; k < rows - 1; k++)
				{
					int num9 = k * (cols + 1) + i;
					int num10 = (k + 1) * (cols + 1) + i;
					int num11 = num9;
					int num12 = num9 + 1;
					int num13 = num10;
					int num14 = num10 + 1;
					int num15 = (k * cols + i) * 6;
					array4[num15] = num11;
					array4[num15 + 1] = num13;
					array4[num15 + 2] = num14;
					array4[num15 + 3] = num14;
					array4[num15 + 4] = num12;
					array4[num15 + 5] = num11;
				}
				int num16 = (rows - 1) * (cols + 1) + i;
				int num17 = num16 + 1;
				int num18 = rows * (cols + 1) + i;
				int num19 = cols * (rows - 1) * 6 + i * 3;
				array4[num19] = num16;
				array4[num19 + 1] = num18;
				array4[num19 + 2] = num17;
			}
		}
		if (mesh == null)
		{
			mesh = new Mesh();
		}
		else
		{
			mesh.Clear();
		}
		mesh.vertices = array;
		mesh.uv = array2;
		mesh.normals = array3;
		mesh.SetTriangles(array4, 0, calculateBounds: true);
	}

	public static void Cylinder(ref Mesh mesh, float radius, float height, int cols, int rows)
	{
		if (cols < 3)
		{
			cols = 3;
		}
		if (rows < 1)
		{
			rows = 1;
		}
		int num = (rows + 1) * (cols + 1);
		Vector3[] array = new Vector3[num];
		Vector2[] array2 = new Vector2[num];
		Vector3[] array3 = new Vector3[num];
		int[] array4 = new int[cols * rows * 6];
		float num2 = 1f / (float)cols;
		float num3 = 0.25f;
		for (int i = 0; i <= cols; i++)
		{
			float f = (num3 + (float)i * num2) * 2f * (float)Math.PI;
			float num4 = Mathf.Cos(f);
			float num5 = Mathf.Sin(f);
			float x = (float)i / (float)cols;
			Vector3 vector = new Vector3(radius * num4, 0f, radius * num5);
			Vector2 vector2 = new Vector2(x, 0f);
			Vector3 vector3 = new Vector3(num4, 0f, num5);
			for (int j = 0; j <= rows; j++)
			{
				int num6 = j * (cols + 1) + i;
				float num7 = (float)j / (float)rows;
				vector.y = -0.5f * height + num7 * height;
				vector2.y = num7;
				array[num6] = vector;
				array2[num6] = vector2;
				array3[num6] = vector3;
				if (i < cols && j < rows)
				{
					int num8 = (j + 1) * (cols + 1) + i;
					int num9 = num6;
					int num10 = num6 + 1;
					int num11 = num8;
					int num12 = num8 + 1;
					int num13 = (j * cols + i) * 6;
					array4[num13] = num9;
					array4[num13 + 1] = num11;
					array4[num13 + 2] = num12;
					array4[num13 + 3] = num12;
					array4[num13 + 4] = num10;
					array4[num13 + 5] = num9;
				}
			}
		}
		if (mesh == null)
		{
			mesh = new Mesh();
		}
		else
		{
			mesh.Clear();
		}
		mesh.vertices = array;
		mesh.uv = array2;
		mesh.normals = array3;
		mesh.SetTriangles(array4, 0, calculateBounds: true);
	}

	public static void Cube(ref Mesh mesh, float w, float h, float d)
	{
		w *= 0.5f;
		h *= 0.5f;
		d *= 0.5f;
		Vector3 vector = new Vector3(0f - w, d, 0f - h);
		Vector3 vector2 = new Vector3(0f - w, d, h);
		Vector3 vector3 = new Vector3(w, d, h);
		Vector3 vector4 = new Vector3(w, d, 0f - h);
		Vector3 vector5 = new Vector3(0f - w, 0f - d, 0f - h);
		Vector3 vector6 = new Vector3(0f - w, 0f - d, h);
		Vector3 vector7 = new Vector3(w, 0f - d, h);
		Vector3 vector8 = new Vector3(w, 0f - d, 0f - h);
		Vector2 vector9 = new Vector2(0f, 0f);
		Vector2 vector10 = new Vector2(0f, 1f);
		Vector2 vector11 = new Vector2(1f, 1f);
		Vector2 vector12 = new Vector2(1f, 0f);
		if (mesh == null)
		{
			mesh = new Mesh();
		}
		else
		{
			mesh.Clear();
		}
		mesh.vertices = new Vector3[24]
		{
			vector, vector2, vector3, vector4, vector5, vector, vector4, vector8, vector8, vector4,
			vector3, vector7, vector7, vector3, vector2, vector6, vector6, vector2, vector, vector5,
			vector6, vector5, vector8, vector7
		};
		mesh.uv = new Vector2[24]
		{
			vector9, vector10, vector11, vector12, vector9, vector10, vector11, vector12, vector9, vector10,
			vector11, vector12, vector9, vector10, vector11, vector12, vector9, vector10, vector11, vector12,
			vector9, vector10, vector11, vector12
		};
		mesh.normals = new Vector3[24]
		{
			Vector3.up,
			Vector3.up,
			Vector3.up,
			Vector3.up,
			Vector3.back,
			Vector3.back,
			Vector3.back,
			Vector3.back,
			Vector3.right,
			Vector3.right,
			Vector3.right,
			Vector3.right,
			Vector3.forward,
			Vector3.forward,
			Vector3.forward,
			Vector3.forward,
			Vector3.left,
			Vector3.left,
			Vector3.left,
			Vector3.left,
			Vector3.down,
			Vector3.down,
			Vector3.down,
			Vector3.down
		};
		int[] triangles = new int[36]
		{
			0, 1, 2, 2, 3, 0, 4, 5, 6, 6,
			7, 4, 8, 9, 10, 10, 11, 8, 12, 13,
			14, 14, 15, 12, 16, 17, 18, 18, 19, 16,
			20, 21, 22, 22, 23, 20
		};
		mesh.SetTriangles(triangles, 0, calculateBounds: true);
	}

	public static Matrix4x4 GetBoxColliderMatrix(BoxCollider c)
	{
		Transform transform = c.transform;
		return Matrix4x4.TRS(transform.TransformPoint(c.center), transform.rotation, Vector3.Scale(transform.lossyScale, c.size));
	}

	public static Matrix4x4 GetMeshColliderMatrix(MeshCollider c)
	{
		return c.transform.localToWorldMatrix;
	}

	public static Matrix4x4 GetSphereColliderMatrix(SphereCollider c)
	{
		Transform transform = c.transform;
		Vector3 lossyScale = transform.lossyScale;
		float num = c.radius * 2f * Mathf.Max(Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y)), Mathf.Abs(lossyScale.z));
		return Matrix4x4.TRS(transform.TransformPoint(c.center), transform.rotation, new Vector3(num, num, num));
	}

	public static void GetCapsuleMatrix(Vector3 position, Quaternion rotation, Vector3 scale, int direction, float radius, float height, out Matrix4x4 m1, out Matrix4x4 m2, out Matrix4x4 m3)
	{
		if (direction < 0 || direction > 2)
		{
			direction = 1;
		}
		float num = 2f * radius * Mathf.Max(Mathf.Abs(scale[(direction + 1) % 3]), Mathf.Abs(scale[(direction + 2) % 3]));
		float num2 = Mathf.Max(0f, height * Mathf.Abs(scale[direction]) - num);
		Vector3 vector = default(Vector3);
		vector[direction] = 0.5f * num2;
		Quaternion quaternion = capsuleRotations[direction];
		Matrix4x4 matrix4x = Matrix4x4.TRS(position, rotation, Vector3.one);
		m1 = matrix4x * Matrix4x4.TRS(Vector3.zero, quaternion, new Vector3(num, num2, num));
		m2 = matrix4x * Matrix4x4.TRS(vector, quaternion, new Vector3(num, num, num));
		m3 = matrix4x * Matrix4x4.TRS(-vector, quaternion * Quaternion.Euler(0f, 0f, 180f), new Vector3(num, num, num));
	}

	public static void GetCapsuleColliderMatrix(CapsuleCollider c, out Matrix4x4 m1, out Matrix4x4 m2, out Matrix4x4 m3)
	{
		Transform transform = c.transform;
		GetCapsuleMatrix(transform.TransformPoint(c.center), transform.rotation, transform.lossyScale, c.direction, c.radius, c.height, out m1, out m2, out m3);
	}

	public static void GetCharacterControllerMatrix(CharacterController c, out Matrix4x4 m1, out Matrix4x4 m2, out Matrix4x4 m3)
	{
		Transform transform = c.transform;
		GetCapsuleMatrix(transform.TransformPoint(c.center), Quaternion.identity, transform.lossyScale, 1, c.radius, c.height, out m1, out m2, out m3);
	}

	public static Vector3 ToUnityVector(this VECTOR vec)
	{
		Vector3 result = default(Vector3);
		result.x = vec.x;
		result.y = vec.y;
		result.z = vec.z;
		return result;
	}

	public static uint HexAsDec(uint hex)
	{
		uint num = 0u;
		for (int num2 = 7; num2 >= 0; num2--)
		{
			num = num * 10 + ((hex >> num2 * 4) & 0xF);
		}
		return num;
	}

	public static float LogAttenuation(float x, float p, float m, float s)
	{
		return (1f - s) / Mathf.Pow(Mathf.Max(0f, x - m + 1f), p) + s;
	}

	public static int NextPowerOf2(int v)
	{
		v = Math.Abs(v);
		v--;
		v |= v >> 1;
		v |= v >> 2;
		v |= v >> 4;
		v |= v >> 8;
		v |= v >> 16;
		v++;
		return v;
	}

	public static float DelayedFadeOut(float sustain, float release, float time)
	{
		if (release <= 0f)
		{
			return 0f;
		}
		return Mathf.Max(Math.Sign(sustain - time), Mathf.Lerp(Mathf.Cos((float)Math.PI / 2f * (time - sustain) / release), 0f, Math.Sign(time - sustain - release)));
	}

	public static Vector4 GetFrameScaleOffset(int cols, int rows, bool flipY, int frame)
	{
		int num = cols * rows;
		frame %= num;
		float num2 = 1f / (float)cols;
		float num3 = 1f / (float)rows;
		int num4 = frame / cols;
		int num5 = frame - num4 * cols;
		if (flipY)
		{
			num4 = rows - 1 - num4;
		}
		float z = (float)num5 * num2;
		float w = (float)num4 * num3;
		return new Vector4(num2, num3, z, w);
	}

	public static int GetSizeRank(long size, out double divisor, out string metric)
	{
		int num = 0;
		divisor = 1.0;
		while ((size /= 1024) != 0L)
		{
			num++;
			divisor *= 1024.0;
		}
		switch (num)
		{
		case 0:
			metric = "B";
			break;
		case 1:
			metric = "kB";
			break;
		case 2:
			metric = "MB";
			break;
		case 3:
			metric = "GB";
			break;
		case 4:
			metric = "TB";
			break;
		case 5:
			metric = "PB";
			break;
		case 6:
			metric = "EB";
			break;
		case 7:
			metric = "ZB";
			break;
		default:
			metric = "YB";
			break;
		}
		return num;
	}

	public static int Mod(int i, int l)
	{
		return (i % l + l) % l;
	}

	public static bool IsDestroyed(Delegate d)
	{
		if ((object)d == null)
		{
			return true;
		}
		if (d.Method.IsStatic)
		{
			return false;
		}
		object target = d.Target;
		if (target == null)
		{
			return true;
		}
		if (target.Equals(null))
		{
			return true;
		}
		return false;
	}
}
