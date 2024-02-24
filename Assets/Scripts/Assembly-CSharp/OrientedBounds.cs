using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

[Serializable]
public struct OrientedBounds : IComparable<OrientedBounds>
{
	private static readonly Vector3 maxVector = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

	private static readonly Vector3 minVector = new Vector3(float.MinValue, float.MinValue, float.MinValue);

	public Vector3 position;

	public Quaternion rotation;

	public Vector3 extents;

	public float volume => extents.x * extents.y * extents.z;

	public Vector3 size
	{
		get
		{
			return extents * 2f;
		}
		set
		{
			extents = value * 0.5f;
		}
	}

	public OrientedBounds(Vector3 position, Quaternion rotation, Vector3 extents)
	{
		this.position = position;
		this.rotation = rotation;
		this.extents = extents;
	}

	public override string ToString()
	{
		return $"[OrientedBounds: position={position}, rotation={rotation}, extents={extents}]";
	}

	public static Matrix4x4 TransformMatrix(Vector3 p, Quaternion r)
	{
		if (r.IsDistinguishedIdentity())
		{
			r = Quaternion.identity;
		}
		float num = r.x * r.x;
		float num2 = r.y * r.y;
		float num3 = r.z * r.z;
		float num4 = r.w * r.w;
		if (!Mathf.Approximately(num + num2 + num3 + num4, 1f))
		{
			float num5 = 1f / Mathf.Sqrt(num + num2 + num3 + num4);
			r.x *= num5;
			r.y *= num5;
			r.z *= num5;
			r.w *= num5;
		}
		Matrix4x4 result = default(Matrix4x4);
		result.m03 = p.x;
		result.m13 = p.y;
		result.m23 = p.z;
		result.m00 = 1f - 2f * (num2 + num3);
		result.m11 = 1f - 2f * (num + num3);
		result.m22 = 1f - 2f * (num + num2);
		float num6 = r.x * r.y;
		float num7 = r.z * r.w;
		result.m10 = 2f * (num6 + num7);
		result.m01 = 2f * (num6 - num7);
		num6 = r.x * r.z;
		num7 = r.y * r.w;
		result.m20 = 2f * (num6 - num7);
		result.m02 = 2f * (num6 + num7);
		num6 = r.y * r.z;
		num7 = r.x * r.w;
		result.m21 = 2f * (num6 + num7);
		result.m12 = 2f * (num6 - num7);
		result.m33 = 1f;
		return result;
	}

	public static Matrix4x4 InverseTransformMatrix(Vector3 p, Quaternion r)
	{
		if (r.IsDistinguishedIdentity())
		{
			r = Quaternion.identity;
		}
		Matrix4x4 result = default(Matrix4x4);
		float num = r.x * r.x;
		float num2 = r.y * r.y;
		float num3 = r.z * r.z;
		float num4 = r.w * r.w;
		if (!Mathf.Approximately(num + num2 + num3 + num4, 1f))
		{
			float num5 = 1f / Mathf.Sqrt(num + num2 + num3 + num4);
			r.x *= num5;
			r.y *= num5;
			r.z *= num5;
			r.w *= num5;
		}
		result.m00 = 1f - 2f * (num2 + num3);
		result.m11 = 1f - 2f * (num + num3);
		result.m22 = 1f - 2f * (num + num2);
		float num6 = r.x * r.y;
		float num7 = r.z * r.w;
		result.m10 = 2f * (num6 - num7);
		result.m01 = 2f * (num6 + num7);
		num6 = r.x * r.z;
		num7 = r.y * r.w;
		result.m20 = 2f * (num6 + num7);
		result.m02 = 2f * (num6 - num7);
		num6 = r.y * r.z;
		num7 = r.x * r.w;
		result.m21 = 2f * (num6 - num7);
		result.m12 = 2f * (num6 + num7);
		Vector3 vector = result.MultiplyVector(-p);
		result.m03 = vector.x;
		result.m13 = vector.y;
		result.m23 = vector.z;
		result.m33 = 1f;
		return result;
	}

	public static OrientedBounds ToWorldBounds(Transform tr, OrientedBounds localBounds)
	{
		if (tr == null)
		{
			return localBounds;
		}
		if (localBounds.rotation.IsDistinguishedIdentity())
		{
			localBounds.rotation = Quaternion.identity;
		}
		Matrix4x4 m = tr.localToWorldMatrix * TransformMatrix(localBounds.position, localBounds.rotation);
		Vector3 translation = GetTranslation(m);
		Quaternion quaternion = tr.rotation * localBounds.rotation;
		Vector3 vector = Vector3.Scale(GetScale(m), localBounds.extents);
		return new OrientedBounds(translation, quaternion, vector);
	}

	public static OrientedBounds ToLocalBounds(Transform tr, OrientedBounds worldBounds)
	{
		if (tr == null)
		{
			return worldBounds;
		}
		if (worldBounds.rotation.IsDistinguishedIdentity())
		{
			worldBounds.rotation = Quaternion.identity;
		}
		Matrix4x4 worldToLocalMatrix = tr.worldToLocalMatrix;
		Vector3 vector = worldToLocalMatrix.MultiplyVector(worldBounds.position - tr.position);
		Quaternion quaternion = Quaternion.Inverse(tr.rotation) * worldBounds.rotation;
		Vector3 vector2 = Vector3.Scale(GetScale(worldToLocalMatrix), worldBounds.extents);
		return new OrientedBounds(vector, quaternion, vector2);
	}

	public static Vector3 GetTranslation(Matrix4x4 m)
	{
		return new Vector3(m.m03, m.m13, m.m23);
	}

	public static Vector3 GetScale(Matrix4x4 m)
	{
		return new Vector3(new Vector3(m.m00, m.m10, m.m20).magnitude, new Vector3(m.m01, m.m11, m.m21).magnitude, new Vector3(m.m02, m.m12, m.m22).magnitude);
	}

	public static Matrix4x4 GetWorldToLocalMatrix(Transform tr, Vector3 position, Quaternion rotation)
	{
		return GetWorldToLocalMatrix(tr.worldToLocalMatrix, position, rotation);
	}

	public static Matrix4x4 GetLocalToWorldMatrix(Transform tr, Vector3 position, Quaternion rotation)
	{
		return GetLocalToWorldMatrix(tr.localToWorldMatrix, position, rotation);
	}

	public static Matrix4x4 GetWorldToLocalMatrix(Matrix4x4 worldToLocalMatrix, Vector3 position, Quaternion rotation)
	{
		return InverseTransformMatrix(position, rotation) * worldToLocalMatrix;
	}

	public static Matrix4x4 GetLocalToWorldMatrix(Matrix4x4 localToWorldMatrix, Vector3 position, Quaternion rotation)
	{
		return localToWorldMatrix * TransformMatrix(position, rotation);
	}

	public static void EncapsulateRenderers(Transform tr, GameObject target, Quaternion boundsRotation, out Vector3 center, out Vector3 extents)
	{
		List<Renderer> list = new List<Renderer>();
		target.GetComponentsInChildren(includeInactive: false, list);
		EncapsulateRenderers(GetWorldToLocalMatrix(tr, Vector3.zero, boundsRotation), list, out center, out extents);
		center = boundsRotation * center;
	}

	public static void EncapsulateRenderers(Matrix4x4 worldToLocalMatrix, List<Renderer> renderers, out Vector3 center, out Vector3 extents)
	{
		if (renderers == null || renderers.Count == 0)
		{
			center = Vector3.zero;
			extents = Vector3.zero;
			return;
		}
		Vector3 min = maxVector;
		Vector3 max = minVector;
		int i = 0;
		for (int count = renderers.Count; i < count; i++)
		{
			Renderer renderer = renderers[i];
			Mesh mesh = null;
			if (renderer is MeshRenderer)
			{
				MeshFilter component = renderer.GetComponent<MeshFilter>();
				if (component != null)
				{
					mesh = component.sharedMesh;
				}
			}
			else if (renderer is SkinnedMeshRenderer)
			{
				mesh = (renderer as SkinnedMeshRenderer).sharedMesh;
			}
			if (!(mesh == null))
			{
				Bounds bounds = mesh.bounds;
				MinMaxBounds(worldToLocalMatrix * renderer.transform.localToWorldMatrix, bounds.center, bounds.extents, ref min, ref max);
			}
		}
		if (min != maxVector && max != minVector)
		{
			extents = (max - min) * 0.5f;
			center = min + extents;
		}
		else
		{
			center = Vector3.zero;
			extents = Vector3.zero;
		}
	}

	public static void MinMaxBounds(Matrix4x4 boundsToLocalMatrix, Vector3 center, Vector3 extents, ref Vector3 min, ref Vector3 max)
	{
		Vector3 vector = center - extents;
		Vector3 vector2 = center + extents;
		for (int i = 0; i < 2; i++)
		{
			float x = ((i == 0) ? vector.x : vector2.x);
			for (int j = 0; j < 2; j++)
			{
				float y = ((j == 0) ? vector.y : vector2.y);
				for (int k = 0; k < 2; k++)
				{
					float z = ((k == 0) ? vector.z : vector2.z);
					Vector3 rhs = boundsToLocalMatrix.MultiplyPoint3x4(new Vector3(x, y, z));
					min = Vector3.Min(min, rhs);
					max = Vector3.Max(max, rhs);
				}
			}
		}
	}

	public static bool Contains(Matrix4x4 worldToLocalMatrix, Vector3 extents, Vector3 worldPoint)
	{
		Vector3 vector = worldToLocalMatrix.MultiplyPoint3x4(worldPoint);
		if (Mathf.Abs(vector.x) < extents.x && Mathf.Abs(vector.y) < extents.y)
		{
			return Mathf.Abs(vector.z) < extents.z;
		}
		return false;
	}

	public static bool Contains(OrientedBounds worldBounds, Vector3 worldPoint)
	{
		Vector3 vector = Quaternion.Inverse(worldBounds.rotation) * (worldPoint - worldBounds.position);
		if (Mathf.Abs(vector.x) < worldBounds.extents.x && Mathf.Abs(vector.y) < worldBounds.extents.y)
		{
			return Mathf.Abs(vector.z) < worldBounds.extents.z;
		}
		return false;
	}

	public static OrientedBounds FromCollider(Collider collider)
	{
		Transform transform = collider.transform;
		Vector3 vector;
		Quaternion identity;
		Vector3 vector2;
		if (collider is BoxCollider boxCollider)
		{
			vector = transform.TransformPoint(boxCollider.center);
			identity = transform.rotation;
			vector2 = Vector3.Scale(boxCollider.size * 0.5f, transform.lossyScale);
		}
		else if (collider is SphereCollider sphereCollider)
		{
			float radius = sphereCollider.radius;
			vector = transform.TransformPoint(sphereCollider.center);
			identity = transform.rotation;
			vector2 = Vector3.Scale(new Vector3(radius, radius, radius), transform.lossyScale);
		}
		else if (collider is CapsuleCollider capsuleCollider)
		{
			float num = capsuleCollider.radius * 2f;
			float value = Mathf.Max(capsuleCollider.height, num);
			Vector3 vector3 = new Vector3(num, num, num);
			vector3[capsuleCollider.direction] = value;
			vector = transform.TransformPoint(capsuleCollider.center);
			identity = transform.rotation;
			vector2 = Vector3.Scale(vector3 * 0.5f, transform.lossyScale);
		}
		else if (collider is MeshCollider meshCollider && meshCollider.sharedMesh != null)
		{
			vector = meshCollider.bounds.center;
			identity = transform.rotation;
			vector2 = Vector3.Scale(meshCollider.sharedMesh.bounds.extents, transform.lossyScale);
		}
		else
		{
			Bounds bounds = collider.bounds;
			vector = bounds.center;
			identity = Quaternion.identity;
			vector2 = bounds.extents;
		}
		return new OrientedBounds(vector, identity, vector2);
	}

	public static void DrawGizmo(Transform transform, OrientedBounds localBounds, Color color)
	{
		Matrix4x4 matrix = Gizmos.matrix;
		Color color2 = Gizmos.color;
		OrientedBounds orientedBounds = ToWorldBounds(transform, localBounds);
		Gizmos.color = color;
		Gizmos.matrix = TransformMatrix(orientedBounds.position, orientedBounds.rotation);
		Gizmos.DrawCube(Vector3.zero, orientedBounds.size);
		Gizmos.color = color2;
		Gizmos.matrix = matrix;
	}

	[Conditional("UNITY_EDITOR")]
	public static void DrawDebug(Vector3 position, Quaternion rotation, Vector3 extents, Color color)
	{
		Matrix4x4 matrix4x = TransformMatrix(Vector3.zero, rotation);
		Vector3 i = matrix4x.MultiplyVector(Vector3.right);
		Vector3 j = matrix4x.MultiplyVector(Vector3.up);
		Vector3 k = matrix4x.MultiplyVector(Vector3.forward);
		DrawWireBox(position, i, j, k, extents, color, color, color);
	}

	[Conditional("UNITY_EDITOR")]
	public static void DrawDebug(OrientedBounds bounds, Color color)
	{
	}

	private static void DrawWireBox(Vector3 worldOrigin, Vector3 i, Vector3 j, Vector3 k, Vector3 extents, Color colorX, Color colorY, Color colorZ)
	{
		Vector3 vector = worldOrigin - i * extents.x - j * extents.y - k * extents.z;
		Vector3 vector2 = worldOrigin - i * extents.x - j * extents.y + k * extents.z;
		Vector3 vector3 = worldOrigin + i * extents.x - j * extents.y + k * extents.z;
		Vector3 vector4 = worldOrigin + i * extents.x - j * extents.y - k * extents.z;
		Vector3 vector5 = worldOrigin - i * extents.x + j * extents.y - k * extents.z;
		Vector3 vector6 = worldOrigin - i * extents.x + j * extents.y + k * extents.z;
		Vector3 vector7 = worldOrigin + i * extents.x + j * extents.y + k * extents.z;
		Vector3 vector8 = worldOrigin + i * extents.x + j * extents.y - k * extents.z;
		UnityEngine.Debug.DrawLine(vector, vector2, colorZ);
		UnityEngine.Debug.DrawLine(vector2, vector3, colorX);
		UnityEngine.Debug.DrawLine(vector3, vector4, colorZ);
		UnityEngine.Debug.DrawLine(vector4, vector, colorX);
		UnityEngine.Debug.DrawLine(vector5, vector6, colorZ);
		UnityEngine.Debug.DrawLine(vector6, vector7, colorX);
		UnityEngine.Debug.DrawLine(vector7, vector8, colorZ);
		UnityEngine.Debug.DrawLine(vector8, vector5, colorX);
		UnityEngine.Debug.DrawLine(vector, vector5, colorY);
		UnityEngine.Debug.DrawLine(vector2, vector6, colorY);
		UnityEngine.Debug.DrawLine(vector3, vector7, colorY);
		UnityEngine.Debug.DrawLine(vector4, vector8, colorY);
	}

	public int CompareTo(OrientedBounds other)
	{
		return (int)(other.volume - volume);
	}
}
