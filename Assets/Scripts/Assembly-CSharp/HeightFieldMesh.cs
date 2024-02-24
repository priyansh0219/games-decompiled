using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class HeightFieldMesh : IDisposable
{
	private struct Node
	{
		public float2 v0;

		public float2 v1;

		public float2 v2;

		public float height;

		public Node(float2 _v0, float2 _v1, float2 _v2, float _height)
		{
			v0 = _v0;
			v1 = _v1;
			v2 = _v2;
			height = _height;
		}
	}

	[BurstCompile]
	private struct RenderJob : IJob
	{
		public NativeQueue<Node> nodeStack;

		public float2 v0;

		public float2 v1;

		public float2 v2;

		public float2 v3;

		public Vector3 viewPoint;

		public Vector3 center;

		public float size;

		public float height;

		public float errorThreshold;

		public NativeArray<Plane> frustumPlanes;

		public bool frustumCull;

		public float minPatchSize;

		private float oneOverSqrt2;

		private float minHeight;

		public NativeQueue<float4x4> matrices;

		private bool GetShouldSubdivide(Vector3 viewPoint, float height, float invErrorThreshold, Vector3 mid)
		{
			float num = 2f * height * oneOverSqrt2;
			float sqrMagnitude = (viewPoint - mid).sqrMagnitude;
			float num2 = height * invErrorThreshold + num;
			return num2 * num2 > sqrMagnitude;
		}

		public void Execute()
		{
			oneOverSqrt2 = 1f / math.sqrt(2f);
			minHeight = minPatchSize * oneOverSqrt2;
			nodeStack.Enqueue(new Node(v0, v1, v2, height));
			nodeStack.Enqueue(new Node(v2, v3, v0, height));
			float4x4 identity = float4x4.identity;
			identity.c1 = new float4(0f, 1f, 0f, 0f);
			float invErrorThreshold = 1f / errorThreshold;
			while (nodeStack.Count > 0)
			{
				Node n = nodeStack.Dequeue();
				if (!frustumCull || IsNodeInFrustum(n, center.y))
				{
					float2 @float = (n.v0 + n.v2) * 0.5f;
					if (n.height > minHeight && GetShouldSubdivide(viewPoint, n.height, invErrorThreshold, new float3(@float.x, center.y, @float.y)))
					{
						float num = n.height * oneOverSqrt2;
						nodeStack.Enqueue(new Node(n.v1, @float, n.v0, num));
						nodeStack.Enqueue(new Node(n.v2, @float, n.v1, num));
						continue;
					}
					float2 float2 = n.v0 - n.v1;
					float2 float3 = n.v2 - n.v1;
					identity.c0 = new float4(float2.x, 0f, float2.y, 0f);
					identity.c2 = new float4(float3.x, 0f, float3.y, 0f);
					identity.c3 = new float4(n.v1.x, center.y, n.v1.y, 1f);
					matrices.Enqueue(identity);
				}
			}
		}

		private bool IsNodeInFrustum(Node n, float y)
		{
			Vector3 point = new Vector3(n.v0.x, y, n.v0.y);
			Vector3 point2 = new Vector3(n.v1.x, y, n.v1.y);
			Vector3 point3 = new Vector3(n.v2.x, y, n.v2.y);
			for (int i = 0; i < frustumPlanes.Length; i++)
			{
				Plane plane = frustumPlanes[i];
				if (!plane.GetSide(point) && !plane.GetSide(point2) && !plane.GetSide(point3))
				{
					return false;
				}
			}
			return true;
		}
	}

	private Mesh patchMesh;

	private bool _castShadows;

	private bool _receiveShadows;

	private bool _frustumCull = true;

	private JobHandle jobHandle;

	private RenderJob job;

	private NativeQueue<float4x4> matrices;

	private NativeArray<Plane> frustumPlanes;

	private NativeQueue<Node> nodeStack;

	public bool castShadows
	{
		get
		{
			return _castShadows;
		}
		set
		{
			_castShadows = value;
		}
	}

	public bool receiveShadows
	{
		get
		{
			return _receiveShadows;
		}
		set
		{
			_receiveShadows = value;
		}
	}

	public bool frustumCull
	{
		get
		{
			return _frustumCull;
		}
		set
		{
			_frustumCull = value;
		}
	}

	public HeightFieldMesh(int numTrianglesPerPatch)
	{
		int numRows = 2 * (int)Mathf.Ceil((-2f + Mathf.Sqrt(4f + 8f * (float)numTrianglesPerPatch)) / 8f);
		patchMesh = CreatePatchMesh(numRows);
		patchMesh.bounds = new Bounds(new Vector3(0.5f, 0f, 0.5f), new Vector3(100f, 100f, 100f));
		frustumPlanes = new NativeArray<Plane>(6, Allocator.Persistent);
		matrices = new NativeQueue<float4x4>(Allocator.Persistent);
		nodeStack = new NativeQueue<Node>(Allocator.Persistent);
	}

	public void Dispose()
	{
		jobHandle.Complete();
		matrices.Dispose();
		frustumPlanes.Dispose();
		nodeStack.Dispose();
	}

	private static Mesh CreatePatchMesh(int numRows)
	{
		int num = numRows * (numRows + 4) + 1;
		int num2 = 2 * numRows * (numRows + 1) * 3;
		Vector3 a = new Vector3(1f, 0f, 0f);
		Vector3 vector = new Vector3(0f, 0f, 0f);
		Vector3 a2 = new Vector3(0f, 0f, 1f);
		Vector3[] array = new Vector3[num];
		int num3 = 0;
		for (int i = 0; i < numRows; i++)
		{
			Vector3 a3 = Vector3.Lerp(a, vector, (float)i / (float)numRows);
			Vector3 vector2 = Vector3.Lerp(a2, vector, (float)i / (float)numRows);
			Vector3 b = Vector3.Lerp(a, vector, (float)(i + 1) / (float)numRows);
			Vector3 b2 = Vector3.Lerp(a2, vector, (float)(i + 1) / (float)numRows);
			Vector3 vector3 = Vector3.Lerp(a3, b, 0.5f);
			Vector3 vector4 = Vector3.Lerp(vector2, b2, 0.5f);
			array[num3] = vector3;
			num3++;
			int num4 = (numRows - i - 1) * 2 + 3;
			float num5 = 1f / ((float)num4 - 1f);
			for (int j = 0; j < num4; j++)
			{
				Vector3 vector5 = Vector3.Lerp(a3, vector2, (float)j * num5);
				array[num3] = vector5;
				num3++;
			}
			array[num3] = vector4;
			num3++;
		}
		array[num3] = vector;
		num3++;
		int[] array2 = new int[num2];
		int num6 = 0;
		int num7 = 0;
		for (int k = 0; k < numRows; k++)
		{
			int num8 = (numRows - k - 1) * 2 + 3;
			int num9 = ((k + 1 != numRows) ? 1 : 0);
			array2[num6++] = num7 + 1;
			array2[num6++] = num7;
			array2[num6++] = num7 + 2;
			array2[num6++] = num7 + 2;
			array2[num6++] = num7;
			array2[num6++] = num7 + num8 + 2 + num9;
			for (int l = 0; l < (num8 - 3) / 2; l++)
			{
				array2[num6++] = num7 + 2 + l * 2;
				array2[num6++] = num7 + 2 + l * 2 + num8 + 1;
				array2[num6++] = num7 + 3 + l * 2;
				array2[num6++] = num7 + 3 + l * 2;
				array2[num6++] = num7 + 2 + l * 2 + num8 + 1;
				array2[num6++] = num7 + 2 + l * 2 + num8 + 2;
				array2[num6++] = num7 + 3 + l * 2;
				array2[num6++] = num7 + 2 + l * 2 + num8 + 2;
				array2[num6++] = num7 + 2 + l * 2 + num8 + 3;
				array2[num6++] = num7 + 3 + l * 2;
				array2[num6++] = num7 + 2 + l * 2 + num8 + 3;
				array2[num6++] = num7 + 4 + l * 2;
			}
			array2[num6++] = num7 + num8 - 1;
			array2[num6++] = num7 + num8 + 1;
			array2[num6++] = num7 + num8;
			array2[num6++] = num7 + num8 + 1;
			array2[num6++] = num7 + num8 - 1;
			array2[num6++] = num7 + 2 * num8 - 1 + num9;
			num7 += num8 + 2;
		}
		return new Mesh
		{
			vertices = array,
			triangles = array2
		};
	}

	public bool FinalizeRender(Material material, Camera camera = null)
	{
		jobHandle.Complete();
		bool result = matrices.Count > 0;
		while (matrices.Count > 0)
		{
			float4x4 float4x = matrices.Dequeue();
			Graphics.DrawMesh(patchMesh, float4x, material, 0, camera, 0, null, castShadows, receiveShadows);
		}
		return result;
	}

	public void BeginRender(Vector3 viewPoint, Vector3 center, float size, float minPatchSize, float errorThreshold, Camera camera, float frustumDilation)
	{
		jobHandle.Complete();
		Plane[] sharedFrustumPlanes = ((camera != null) ? camera : MainCamera.camera).GetSharedFrustumPlanes();
		for (int i = 0; i < frustumPlanes.Length; i++)
		{
			Plane plane = sharedFrustumPlanes[i];
			Plane value = frustumPlanes[i];
			value.normal = plane.normal;
			value.distance = plane.distance + frustumDilation;
			frustumPlanes[i] = value;
		}
		float2 @float = new float2(size + center.x, 0f - size + center.z);
		float2 float2 = new float2(0f - size + center.x, 0f - size + center.z);
		float2 float3 = new float2(0f - size + center.x, size + center.z);
		float2 v = new float2(size + center.x, size + center.z);
		job = new RenderJob
		{
			nodeStack = nodeStack,
			matrices = matrices,
			v0 = @float,
			v1 = float2,
			v2 = float3,
			v3 = v,
			minPatchSize = minPatchSize,
			height = GetRegularRightTriangleHeight(@float, float2, float3),
			viewPoint = viewPoint,
			center = center,
			size = size,
			errorThreshold = errorThreshold,
			frustumPlanes = frustumPlanes,
			frustumCull = _frustumCull
		};
		jobHandle = job.Schedule();
	}

	private static float GetRegularRightTriangleHeight(Vector2 v0, Vector2 v1, Vector2 v2)
	{
		return Mathf.Sqrt((v2 - v1).sqrMagnitude * 0.5f);
	}
}
