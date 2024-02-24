using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Gendarme;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;

namespace UWE
{
	public class Utils
	{
		public static class GUI
		{
			public static int LayoutIntField(int x, string label)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(label);
				string s = GUILayout.TextField(string.Concat(x));
				GUILayout.EndHorizontal();
				if (int.TryParse(s, out var result))
				{
					return result;
				}
				return x;
			}
		}

		private static List<Renderer> checkRenderers = new List<Renderer>();

		public static Facing[] FacingValues = new Facing[4]
		{
			Facing.North,
			Facing.East,
			Facing.South,
			Facing.West
		};

		public static DateTime s_Epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);

		private static Vector3[] indexToAxis = new Vector3[3]
		{
			new Vector3(1f, 0f, 0f),
			new Vector3(0f, 1f, 0f),
			new Vector3(0f, 0f, 1f)
		};

		[SuppressMessage("Gendarme.Rules.Concurrency", "NonConstantStaticFieldsShouldNotBeVisibleRule")]
		public static RaycastHit[] sharedHitBuffer = new RaycastHit[256];

		[SuppressMessage("Gendarme.Rules.Concurrency", "NonConstantStaticFieldsShouldNotBeVisibleRule")]
		public static Collider[] sharedColliderBuffer = new Collider[256];

		private static Stack<bool> LockCursorStack = new Stack<bool>();

		private static bool _alwaysLockCursor = false;

		private static bool _lockCursor = false;

		public static string memStatLogPrefix = null;

		public static int memStatLogCount = 0;

		public static List<Color> colors = new List<Color>
		{
			Color.blue,
			Color.cyan,
			Color.gray,
			Color.green,
			Color.grey,
			Color.magenta,
			Color.red,
			Color.white,
			Color.yellow
		};

		private static bool physicsSyncTransformsSave = false;

		private static bool inPhysicsSyncTransformSection = false;

		public static Vector2 half2 => new Vector2(0.5f, 0.5f);

		public static Vector3 half3 => new Vector3(0.5f, 0.5f, 0.5f);

		public static bool alwaysLockCursor
		{
			get
			{
				return _alwaysLockCursor;
			}
			set
			{
				_alwaysLockCursor = value;
				UpdateCusorLockState();
			}
		}

		public static bool lockCursor
		{
			get
			{
				if (alwaysLockCursor)
				{
					return true;
				}
				return !Cursor.visible;
			}
			set
			{
				_lockCursor = value;
				UpdateCusorLockState();
			}
		}

		public static Transform GetChildWithTag(Transform t, string tag)
		{
			if (t != null && !string.IsNullOrEmpty(tag))
			{
				if (t.CompareTag(tag))
				{
					return t;
				}
				for (int i = 0; i < t.childCount; i++)
				{
					Transform childWithTag = GetChildWithTag(t.GetChild(i), tag);
					if (childWithTag != null)
					{
						return childWithTag;
					}
				}
			}
			return null;
		}

		public static Bounds GetEncapsulatedAABB(GameObject go, int maxRenderers = -1)
		{
			go.GetComponentsInChildren(includeInactive: false, checkRenderers);
			Bounds result = new Bounds(go.transform.position, Vector3.zero);
			if (checkRenderers.Count > 0 && checkRenderers[0].gameObject.activeSelf && checkRenderers[0].gameObject.activeInHierarchy && !(checkRenderers[0] is ParticleSystemRenderer))
			{
				result = checkRenderers[0].bounds;
			}
			if (checkRenderers.Count > 1)
			{
				int num = checkRenderers.Count;
				if (maxRenderers != -1 && maxRenderers + 1 < num)
				{
					num = maxRenderers + 1;
				}
				for (int i = 1; i < num; i++)
				{
					Renderer renderer = checkRenderers[i];
					if (renderer.gameObject.activeSelf && renderer.gameObject.activeInHierarchy && !(renderer is ParticleSystemRenderer) && !(renderer is LineRenderer))
					{
						result.Encapsulate(checkRenderers[i].bounds);
					}
				}
			}
			return result;
		}

		public static void EnsureArraySize<T>(ref T[] array, int size, bool reset = false)
		{
			if (array == null)
			{
				array = new T[size];
				return;
			}
			int num = array.Length;
			if (array.Length < size)
			{
				array = new T[size * 2];
			}
			if (reset)
			{
				for (int i = 0; i < num; i++)
				{
					array[i] = default(T);
				}
			}
		}

		public static void CopyArray<T>(T[] from, ref T[] to)
		{
			if (from != null)
			{
				EnsureArraySize(ref to, from.Length);
				for (int i = 0; i < from.Length; i++)
				{
					to[i] = from[i];
				}
			}
		}

		public static float GetAABBVolume(GameObject go)
		{
			Bounds encapsulatedAABB = GetEncapsulatedAABB(go);
			return encapsulatedAABB.size.x * encapsulatedAABB.size.y * encapsulatedAABB.size.z;
		}

		public static Vector3 RandomOnUnitCircle()
		{
			float f = UnityEngine.Random.value * 360f * ((float)System.Math.PI / 180f);
			return new Vector2(Mathf.Cos(f), Mathf.Sin(f));
		}

		public static Vector3 GetRandomVectorInDirection(Vector3 baseDirection, float amount)
		{
			amount = Mathf.Clamp01(amount);
			Vector3 vector = Vector3.Normalize(new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)));
			return Vector3.Normalize(baseDirection * (1f - amount) + vector * amount);
		}

		public static void SetIsKinematicAndUpdateInterpolation(GameObject go, bool isKinematic, bool setCollisionDetectionMode = false)
		{
			Rigidbody[] componentsInChildren = go.GetComponentsInChildren<Rigidbody>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				SetIsKinematicAndUpdateInterpolation(componentsInChildren[i], isKinematic, setCollisionDetectionMode);
			}
		}

		public static void SetIsKinematicAndUpdateInterpolation(Rigidbody rigidbody, bool isKinematic, bool setCollisionDetectionMode = false)
		{
			if (setCollisionDetectionMode && isKinematic)
			{
				rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
			}
			rigidbody.isKinematic = isKinematic;
			rigidbody.interpolation = ((!isKinematic) ? RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None);
			if (setCollisionDetectionMode && !isKinematic)
			{
				rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			}
		}

		public static void SetCollidersEnabled(GameObject go, bool enabled)
		{
			Collider[] componentsInChildren = go.GetComponentsInChildren<Collider>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = enabled;
			}
		}

		public static void SetIsKinematic(Rigidbody rigidbody, bool state)
		{
			if ((bool)rigidbody)
			{
				rigidbody.isKinematic = state;
			}
		}

		public static void SetEnabled(Behaviour behaviour, bool state)
		{
			if ((bool)behaviour)
			{
				behaviour.enabled = state;
			}
		}

		public static Vector3 Abs(Vector3 v)
		{
			return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
		}

		public static float Min(Vector3 v)
		{
			return Mathf.Min(v.x, Mathf.Min(v.y, v.z));
		}

		public static Vector3 EulerDiff(Vector3 a, Vector3 b)
		{
			Vector3 zero = Vector3.zero;
			zero.x = Mathf.DeltaAngle(a.x, b.x);
			zero.y = Mathf.DeltaAngle(a.y, b.y);
			zero.z = Mathf.DeltaAngle(a.z, b.z);
			return zero;
		}

		public static Vector3 LerpEuler(Vector3 from, Vector3 to, float amount)
		{
			Vector3 zero = Vector3.zero;
			zero.x = Mathf.LerpAngle(from.x, to.x, amount);
			zero.y = Mathf.LerpAngle(from.y, to.y, amount);
			zero.z = Mathf.LerpAngle(from.z, to.z, amount);
			return zero;
		}

		public static Rigidbody GetRootRigidbody(GameObject go)
		{
			GameObject gameObject = GetEntityRoot(go);
			if (gameObject == null)
			{
				gameObject = go;
			}
			return gameObject.GetComponent<Rigidbody>();
		}

		public static GameObject GetEntityRoot(GameObject go)
		{
			PrefabIdentifier prefabIdentifier = go.GetComponent<PrefabIdentifier>();
			if (prefabIdentifier == null)
			{
				prefabIdentifier = go.GetComponentInParent<PrefabIdentifier>();
			}
			if (prefabIdentifier != null)
			{
				return prefabIdentifier.gameObject;
			}
			return null;
		}

		public static void ZeroTransformRigidLocals(Transform t)
		{
			t.localPosition = Vector3.zero;
			t.localRotation = Quaternion.identity;
		}

		public static void ZeroTransform(Transform t)
		{
			t.localPosition = Vector3.zero;
			t.localRotation = Quaternion.identity;
			t.localScale = new Vector3(1f, 1f, 1f);
		}

		public static void ZeroTransform(GameObject go)
		{
			ZeroTransform(go.transform);
		}

		public static float GetRadiusScale(Vector3 scale)
		{
			return Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
		}

		public static float Max(Vector3 v)
		{
			return Mathf.Max(v.x, Mathf.Max(v.y, v.z));
		}

		public static Vector3 Div(Vector3 u, Vector3 v)
		{
			return new Vector3(u.x / v.x, u.y / v.y, u.z / v.z);
		}

		public static float Slerp(float from, float to, float amount)
		{
			if (amount < 0f)
			{
				amount = 0f - amount;
			}
			if (Mathf.Abs(to - from) < amount)
			{
				return to;
			}
			return from + Mathf.Sign(to - from) * amount;
		}

		public static Vector3 ZeroY(Vector3 v)
		{
			return new Vector3(v.x, 0f, v.z);
		}

		public static float Wrap(float n, float min, float max)
		{
			if (n > max)
			{
				return min + max - n;
			}
			if (n < min)
			{
				return max + (n - min);
			}
			return n;
		}

		public static float GetYawDegFromVector(Vector3 v)
		{
			if ((double)Mathf.Abs(v.x) < 0.001 || (double)Mathf.Abs(v.z) < 0.001)
			{
				return 0f;
			}
			float num = Mathf.Atan2(v.x, v.z);
			if (num < 0f)
			{
				num += (float)System.Math.PI * 2f;
			}
			return num * 57.29578f;
		}

		public static float GetPitchDegFromVector(Vector3 v)
		{
			return (0f - Mathf.Asin(Mathf.Clamp(v.y, -1f, 1f))) * 57.29578f;
		}

		public static Vector3 SlerpVector(Vector3 from, Vector3 to, Vector3 amount)
		{
			return new Vector3(Slerp(from.x, to.x, amount.x), Slerp(from.y, to.y, amount.y), Slerp(from.z, to.z, amount.z));
		}

		public static Vector3 SlerpVector(Vector3 from, Vector3 to, float amount)
		{
			return new Vector3(Slerp(from.x, to.x, amount), Slerp(from.y, to.y, amount), Slerp(from.z, to.z, amount));
		}

		public static Vector3 LerpVector(Vector3 from, Vector3 to, float amount)
		{
			return new Vector3(Mathf.Lerp(from.x, to.x, amount), Mathf.Lerp(from.y, to.y, amount), Mathf.Lerp(from.z, to.z, amount));
		}

		public static Vector2 LerpVector(Vector2 from, Vector2 to, float amount)
		{
			return new Vector2(Mathf.Lerp(from.x, to.x, amount), Mathf.Lerp(from.y, to.y, amount));
		}

		public static Color LerpColor(Color from, Color to, float amount)
		{
			return new Color(Mathf.Lerp(from.r, to.r, amount), Mathf.Lerp(from.g, to.g, amount), Mathf.Lerp(from.b, to.b, amount), Mathf.Lerp(from.a, to.a, amount));
		}

		public static int Lerp(int a, int b, int t0, int t1, int t)
		{
			if (t0 == t1)
			{
				return a;
			}
			return (a * (t1 - t) + b * (t - t0)) / (t1 - t0);
		}

		public static IEnumerator LerpTransform(Transform tr, Vector3 localPosition, Quaternion localRotation, Vector3 localScale, float lerpDuration)
		{
			Vector3 initialOffset = tr.localPosition;
			Quaternion initialRotation = tr.localRotation;
			Vector3 initialScale = tr.localScale;
			float lerpFactor = 0f;
			while (lerpFactor < 1f)
			{
				lerpFactor += Time.deltaTime / lerpDuration;
				tr.localPosition = Vector3.Lerp(initialOffset, localPosition, lerpFactor);
				tr.localRotation = Quaternion.Slerp(initialRotation, localRotation, lerpFactor);
				tr.localScale = Vector3.Lerp(initialScale, localScale, lerpFactor);
				yield return null;
			}
		}

		public static bool SharingHierarchy(GameObject obj1, GameObject obj2)
		{
			if (obj1 != null && obj2 != null)
			{
				if (!IsAncestorOf(obj1, obj2))
				{
					return IsAncestorOf(obj2, obj1);
				}
				return true;
			}
			return false;
		}

		public static bool IsAncestorOf(GameObject ancestor, GameObject obj)
		{
			while (true)
			{
				if (ancestor == obj)
				{
					return true;
				}
				if (obj.transform.parent == null)
				{
					break;
				}
				obj = obj.transform.parent.gameObject;
			}
			return false;
		}

		public static double GetSystemTime()
		{
			return (DateTime.Now.ToUniversalTime() - s_Epoch).TotalMilliseconds;
		}

		public static Rect RectFromScreenSpace(float left, float top, float width, float height)
		{
			int height2 = Screen.height;
			int width2 = Screen.width;
			return new Rect(left * (float)width2, top * (float)height2, width * (float)width2, height * (float)height2);
		}

		public static string CheckUnityEvent(UnityEventBase evt, string eventName)
		{
			for (int i = 0; i < evt.GetPersistentEventCount(); i++)
			{
				if (!evt.GetPersistentTarget(i))
				{
					return $"Missing target object {eventName}";
				}
				if (string.IsNullOrEmpty(evt.GetPersistentMethodName(i)))
				{
					return $"Missing target method {eventName}";
				}
			}
			return null;
		}

		public static bool IsInsideCollider(SphereCollider sphere, Vector3 pos)
		{
			Vector3 vector = sphere.transform.InverseTransformPoint(pos) - sphere.center;
			float radius = sphere.radius;
			return vector.sqrMagnitude <= radius * radius;
		}

		public static bool IsInsideCollider(BoxCollider box, Vector3 pos)
		{
			Vector3 vector = box.transform.InverseTransformPoint(pos);
			Vector3 v = box.center - vector;
			Vector3 vector2 = box.size * 0.5f;
			return v.InBox(-vector2, vector2);
		}

		public static bool IsInsideCollider(CapsuleCollider collider, Vector3 pointToCheck)
		{
			Vector3 vector = indexToAxis[collider.direction];
			Vector3 vector2 = collider.transform.TransformPoint(collider.center + vector * 0.5f * collider.height);
			Vector3 value = collider.transform.TransformPoint(collider.center - vector * 0.5f * collider.height) - vector2;
			float magnitude = value.magnitude;
			Vector3 vector3 = Vector3.Normalize(value);
			Vector3 rhs = pointToCheck - vector2;
			float radius = collider.radius;
			float num = Mathf.Clamp(Vector3.Dot(vector3, rhs), 0f + radius, magnitude - radius);
			return (vector2 + num * vector3 - pointToCheck).magnitude <= radius;
		}

		public static bool IsInsideCollider(Collider collider, Vector3 pos)
		{
			if ((object)collider != null)
			{
				if (collider is BoxCollider box)
				{
					return IsInsideCollider(box, pos);
				}
				if (collider is SphereCollider sphere)
				{
					return IsInsideCollider(sphere, pos);
				}
				if (collider is CapsuleCollider collider2)
				{
					return IsInsideCollider(collider2, pos);
				}
			}
			return false;
		}

		public static Vector3 ClosestPoint(Collider collider, Vector3 pos)
		{
			if ((object)collider != null && collider is MeshCollider meshCollider)
			{
				MeshCollider meshCollider2 = meshCollider;
				if (!meshCollider2.convex)
				{
					return meshCollider2.ClosestPointOnBounds(pos);
				}
			}
			return collider.ClosestPoint(pos);
		}

		public static GameObject FindAncestorWithName(GameObject child, string name)
		{
			GameObject gameObject = child;
			GameObject result = null;
			while ((bool)gameObject)
			{
				if (gameObject.name == name)
				{
					result = gameObject;
					break;
				}
				gameObject = ((gameObject.transform.parent != null) ? gameObject.transform.parent.gameObject : null);
			}
			return result;
		}

		private static bool GrowRaycastHitBufferIfNecessary(int numHits)
		{
			if (numHits >= sharedHitBuffer.Length)
			{
				sharedHitBuffer = new RaycastHit[sharedHitBuffer.Length * 2];
				return true;
			}
			return false;
		}

		public static int CapsuleCastIntoSharedBuffer(Vector3 point1, Vector3 point2, float radius, Vector3 direction, float maxDistance = float.PositiveInfinity, int layermask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			int num = Physics.CapsuleCastNonAlloc(point1, point2, radius, direction, sharedHitBuffer, maxDistance, layermask, queryTriggerInteraction);
			if (GrowRaycastHitBufferIfNecessary(num))
			{
				return CapsuleCastIntoSharedBuffer(point1, point2, radius, direction, maxDistance, layermask, queryTriggerInteraction);
			}
			return num;
		}

		public static int RaycastIntoSharedBuffer(Ray ray, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			int num = Physics.RaycastNonAlloc(ray, sharedHitBuffer, maxDistance, layerMask, queryTriggerInteraction);
			if (GrowRaycastHitBufferIfNecessary(num))
			{
				return RaycastIntoSharedBuffer(ray, maxDistance, layerMask, queryTriggerInteraction);
			}
			return num;
		}

		public static int RaycastIntoSharedBuffer(Vector3 origin, Vector3 direction, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			int num = Physics.RaycastNonAlloc(origin, direction, sharedHitBuffer, maxDistance, layerMask, queryTriggerInteraction);
			if (GrowRaycastHitBufferIfNecessary(num))
			{
				return RaycastIntoSharedBuffer(origin, direction, maxDistance, layerMask, queryTriggerInteraction);
			}
			return num;
		}

		public static int SpherecastIntoSharedBuffer(Ray ray, float radius, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			int num = Physics.SphereCastNonAlloc(ray, radius, sharedHitBuffer, maxDistance, layerMask, queryTriggerInteraction);
			if (GrowRaycastHitBufferIfNecessary(num))
			{
				return SpherecastIntoSharedBuffer(ray, radius, maxDistance, layerMask, queryTriggerInteraction);
			}
			return num;
		}

		public static int SpherecastIntoSharedBuffer(Vector3 origin, float radius, Vector3 direction, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			int num = Physics.SphereCastNonAlloc(origin, radius, direction, sharedHitBuffer, maxDistance, layerMask, queryTriggerInteraction);
			if (GrowRaycastHitBufferIfNecessary(num))
			{
				return SpherecastIntoSharedBuffer(origin, radius, direction, maxDistance, layerMask, queryTriggerInteraction);
			}
			return num;
		}

		private static bool GrowColliderBufferIfNecessary(int numHits)
		{
			if (numHits >= sharedColliderBuffer.Length)
			{
				sharedColliderBuffer = new Collider[sharedColliderBuffer.Length * 2];
				return true;
			}
			return false;
		}

		private static void ClearSharedColliderBuffer()
		{
			for (int i = 0; i < sharedColliderBuffer.Length && sharedColliderBuffer[i] != null; i++)
			{
				sharedColliderBuffer[i] = null;
			}
		}

		public static int OverlapSphereIntoSharedBuffer(Vector3 position, float radius, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			ClearSharedColliderBuffer();
			int num = Physics.OverlapSphereNonAlloc(position, radius, sharedColliderBuffer, layerMask, queryTriggerInteraction);
			if (GrowColliderBufferIfNecessary(num))
			{
				return OverlapSphereIntoSharedBuffer(position, radius, layerMask, queryTriggerInteraction);
			}
			return num;
		}

		public static int OverlapBoxIntoSharedBuffer(Vector3 center, Vector3 halfExtents, Quaternion orientation, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal)
		{
			ClearSharedColliderBuffer();
			int num = Physics.OverlapBoxNonAlloc(center, halfExtents, sharedColliderBuffer, orientation, layerMask, queryTriggerInteraction);
			if (GrowColliderBufferIfNecessary(num))
			{
				return OverlapBoxIntoSharedBuffer(center, halfExtents, orientation, layerMask, queryTriggerInteraction);
			}
			return num;
		}

		public static bool TraceForTerrain(Ray ray, float maxDistance, out RaycastHit hitInfo)
		{
			return Physics.Raycast(ray, out hitInfo, maxDistance, Voxeland.GetTerrainLayerMask(), QueryTriggerInteraction.Ignore);
		}

		public static Vector3 ClampVector(Vector3 b, Vector3 i)
		{
			i.x = Mathf.Clamp(i.x, 0f - b.x, b.x);
			i.y = Mathf.Clamp(i.y, 0f - b.y, b.y);
			i.z = Mathf.Clamp(i.z, 0f - b.z, b.z);
			return i;
		}

		public static Vector3 ClampWithin(Bounds b, Vector3 p)
		{
			return p.Clamp(b.min, b.max);
		}

		public static Vector3 ClampWithin(Bounds b, Vector3 p, float margin)
		{
			return p.Clamp(b.min + new Vector3(margin, margin, margin), b.max - new Vector3(margin, margin, margin));
		}

		private static int GatherRaycastTargets(float maxDist, float sphereRadius, bool preferSphereCast = false)
		{
			int num = 0;
			Camera camera = MainCamera.camera;
			Vector3 position = camera.transform.position;
			Vector3 forward = camera.transform.forward;
			if (!preferSphereCast)
			{
				num = RaycastIntoSharedBuffer(new Ray(position, forward), maxDist, -2097153);
				if (num > 0)
				{
					return num;
				}
			}
			return SpherecastIntoSharedBuffer(position + forward * sphereRadius, sphereRadius, forward, maxDist, -2097153);
		}

		public static bool TraceForFPSTarget(GameObject ignoreObj, float maxDist, float sphereRadius, out GameObject closestObj, out float closestDist, bool preferSphereHits = false)
		{
			bool result = false;
			closestObj = null;
			closestDist = 0f;
			int num = GatherRaycastTargets(maxDist, sphereRadius, preferSphereHits);
			for (int i = 0; i < num; i++)
			{
				RaycastHit raycastHit = sharedHitBuffer[i];
				if ((!(ignoreObj != null) || !IsAncestorOf(ignoreObj, raycastHit.collider.gameObject)) && (!raycastHit.collider || !raycastHit.collider.isTrigger || raycastHit.collider.gameObject.layer == LayerMask.NameToLayer("Useable")) && (closestObj == null || raycastHit.distance < closestDist))
				{
					closestObj = raycastHit.collider.gameObject;
					closestDist = raycastHit.distance;
					result = true;
				}
			}
			return result;
		}

		public static bool TraceHitComponentNormal<T>(GameObject ignoreObj, float maxDist, float sphereRadius, out Vector3 surfaceNormal) where T : MonoBehaviour
		{
			Vector3 hitPoint;
			return TraceHitComponentNormal<T>(ignoreObj, maxDist, sphereRadius, out surfaceNormal, out hitPoint);
		}

		public static bool TraceHitComponentNormal<T>(GameObject ignoreObj, float maxDist, float sphereRadius, out Vector3 surfaceNormal, out Vector3 hitPoint) where T : MonoBehaviour
		{
			bool result = false;
			GameObject gameObject = null;
			float num = 0f;
			Vector3 vector = Vector3.up;
			hitPoint = Vector3.zero;
			surfaceNormal = Vector3.zero;
			int num2 = GatherRaycastTargets(maxDist, sphereRadius);
			for (int i = 0; i < num2; i++)
			{
				RaycastHit raycastHit = sharedHitBuffer[i];
				if ((!(ignoreObj != null) || !IsAncestorOf(ignoreObj, raycastHit.collider.gameObject)) && (gameObject == null || raycastHit.distance < num))
				{
					gameObject = raycastHit.collider.gameObject;
					hitPoint = raycastHit.point;
					vector = raycastHit.normal;
					num = raycastHit.distance;
				}
			}
			if ((bool)gameObject && (bool)gameObject.transform.parent && gameObject.GetComponentInParent<T>() != null)
			{
				UnityEngine.Debug.DrawLine(hitPoint, hitPoint + surfaceNormal, Color.white, 0f);
				surfaceNormal = vector;
				result = true;
			}
			return result;
		}

		public static bool TraceForTarget(Vector3 startPos, Vector3 direction, GameObject ignoreObj, float maxDist, ref GameObject closestObj, ref Vector3 position, bool includeTriggers = false)
		{
			bool result = false;
			int num = RaycastIntoSharedBuffer(new Ray(startPos, direction), maxDist);
			if (num == 0)
			{
				num = SpherecastIntoSharedBuffer(startPos, 0.7f, direction, maxDist);
			}
			closestObj = null;
			float num2 = 0f;
			for (int i = 0; i < num; i++)
			{
				RaycastHit raycastHit = sharedHitBuffer[i];
				if ((!(ignoreObj != null) || !IsAncestorOf(ignoreObj, raycastHit.collider.gameObject)) && (!raycastHit.collider || !raycastHit.collider.isTrigger || includeTriggers) && (closestObj == null || raycastHit.distance < num2))
				{
					closestObj = raycastHit.collider.gameObject;
					num2 = raycastHit.distance;
					position = raycastHit.point;
					result = true;
				}
			}
			return result;
		}

		public static bool TraceFPSTargetPosition(GameObject ignoreObj, float maxDist, ref GameObject closestObj, ref Vector3 position, bool includeUseableTriggers = true)
		{
			Vector3 normal;
			return TraceFPSTargetPosition(ignoreObj, maxDist, ref closestObj, ref position, out normal, includeUseableTriggers);
		}

		public static bool TraceFPSTargetPosition(GameObject ignoreObj, float maxDist, ref GameObject closestObj, ref Vector3 position, out Vector3 normal, bool includeUseableTriggers = true)
		{
			bool result = false;
			normal = Vector3.up;
			Camera camera = MainCamera.camera;
			Vector3 position2 = camera.transform.position;
			int num = RaycastIntoSharedBuffer(new Ray(position2, camera.transform.forward), maxDist, -2097153);
			if (num == 0)
			{
				num = SpherecastIntoSharedBuffer(position2, 0.7f, camera.transform.forward, maxDist, -2097153);
			}
			closestObj = null;
			float num2 = 0f;
			for (int i = 0; i < num; i++)
			{
				RaycastHit raycastHit = sharedHitBuffer[i];
				if ((!(ignoreObj != null) || !IsAncestorOf(ignoreObj, raycastHit.collider.gameObject)) && (!raycastHit.collider || !raycastHit.collider.isTrigger || (includeUseableTriggers && raycastHit.collider.gameObject.layer == LayerMask.NameToLayer("Useable"))) && (closestObj == null || raycastHit.distance < num2))
				{
					closestObj = raycastHit.collider.gameObject;
					num2 = raycastHit.distance;
					position = raycastHit.point;
					normal = raycastHit.normal;
					result = true;
				}
			}
			return result;
		}

		public static T GetComponentInHierarchy<T>(GameObject go) where T : Component
		{
			T val = go.GetComponent<T>();
			if (val == null)
			{
				val = go.GetComponentInChildren<T>();
			}
			if (val == null)
			{
				val = go.GetComponentInParent<T>();
			}
			return val;
		}

		public static LODGroup SetupDummyLOD(GameObject root, float switchFraction)
		{
			Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
			List<Renderer> renderers = new List<Renderer>();
			root.DoDepthFirst(delegate(GameObject go)
			{
				if (!(go.GetComponent<Renderer>() == null) && !(go.name == "__LowLOD"))
				{
					if (bounds.size == Vector3.zero)
					{
						bounds = go.GetComponent<Renderer>().bounds;
					}
					else
					{
						bounds.Encapsulate(go.GetComponent<Renderer>().bounds.min);
						bounds.Encapsulate(go.GetComponent<Renderer>().bounds.max);
					}
					renderers.Add(go.GetComponent<Renderer>());
				}
			});
			if (renderers.Count == 0)
			{
				return null;
			}
			LODGroup lODGroup = root.GetComponent<LODGroup>();
			if (lODGroup == null)
			{
				lODGroup = root.AddComponent<LODGroup>();
			}
			GameObject gameObject = null;
			foreach (Transform item in root.transform)
			{
				if (item.gameObject.name == "__LowLOD")
				{
					gameObject = item.gameObject;
					break;
				}
			}
			if (gameObject == null)
			{
				gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
				gameObject.name = "__LowLOD";
				UnityEngine.Object.DestroyImmediate(gameObject.GetComponent<Collider>());
				gameObject.transform.position = bounds.center;
				gameObject.transform.localScale = bounds.size;
				gameObject.transform.parent = root.transform;
				gameObject.GetComponent<Renderer>().sharedMaterial = renderers[0].sharedMaterial;
			}
			lODGroup.SetLODs(new LOD[2]
			{
				new LOD(switchFraction, renderers.ToArray()),
				new LOD(0f, new Renderer[1] { gameObject.GetComponent<Renderer>() })
			});
			lODGroup.RecalculateBounds();
			return lODGroup;
		}

		public static float GetFarplaneForFogSettings()
		{
			if (RenderSettings.fogMode == FogMode.Linear)
			{
				return RenderSettings.fogEndDistance;
			}
			if (RenderSettings.fogMode == FogMode.Exponential)
			{
				return Mathf.Log(526.3158f) / RenderSettings.fogDensity;
			}
			if (RenderSettings.fogMode == FogMode.ExponentialSquared)
			{
				return Mathf.Sqrt(Mathf.Log(526.3158f)) / RenderSettings.fogDensity;
			}
			return 0f;
		}

		public static void DisableEditingRecursive(GameObject obj)
		{
			obj.hideFlags |= HideFlags.NotEditable;
			foreach (Transform item in obj.transform)
			{
				DisableEditingRecursive(item.gameObject);
			}
		}

		public static int CeilShiftRight(int x, int shift)
		{
			return x + (1 << shift) - 1 >> shift;
		}

		public static int CeilDiv(int x, int y)
		{
			return (x + y - 1) / y;
		}

		public static int RoundDiv(int x, int y)
		{
			return (x + y / 2) / y;
		}

		public static Vector3 SafeDiv(Vector3 numerator, float denominator)
		{
			if (denominator != 0f)
			{
				return numerator / denominator;
			}
			return Vector3.zero;
		}

		public static float SafeDiv(float numerator, int denominator)
		{
			if (denominator != 0)
			{
				return numerator / (float)denominator;
			}
			return 0f;
		}

		public static float SafeDiv(float numerator, float denominator)
		{
			if (denominator != 0f)
			{
				return numerator / denominator;
			}
			return 0f;
		}

		public static double Repeat(double t, double length)
		{
			return t - System.Math.Floor(t / length) * length;
		}

		public static int Modulus(int v, int m)
		{
			return (v % m + m) % m;
		}

		public static Color JetColormap(float fraction)
		{
			Color a;
			Color b;
			if (fraction.IsBetween(0f, 0.25f))
			{
				a = Color.blue;
				b = Color.cyan;
				fraction -= 0f;
			}
			else if (fraction.IsBetween(0.25f, 0.5f))
			{
				a = Color.cyan;
				b = Color.green;
				fraction -= 0.25f;
			}
			else if (fraction.IsBetween(0.5f, 0.75f))
			{
				a = Color.green;
				b = Color.yellow;
				fraction -= 0.5f;
			}
			else
			{
				a = Color.yellow;
				b = Color.red;
				fraction -= 0.75f;
			}
			return Color.Lerp(a, b, fraction * 4f);
		}

		public static Color RandomColor()
		{
			return new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
		}

		public static Int3 ConvertColorToLightBarRange(Color inColor)
		{
			Int3 result = default(Int3);
			result.x = Mathf.Clamp(Mathf.FloorToInt(inColor.r * 255f), 0, 255);
			result.y = Mathf.Clamp(Mathf.FloorToInt(inColor.g * 255f), 0, 255);
			result.z = Mathf.Clamp(Mathf.FloorToInt(inColor.b * 255f), 0, 255);
			if (result.x < 13 && result.y < 13 && result.z < 13)
			{
				result.x = 13;
				result.y = 13;
				result.z = 13;
			}
			return result;
		}

		public static Color GammaToLinear(Color gammaColor)
		{
			Color result = default(Color);
			result.r = Mathf.GammaToLinearSpace(gammaColor.r);
			result.g = Mathf.GammaToLinearSpace(gammaColor.g);
			result.b = Mathf.GammaToLinearSpace(gammaColor.b);
			result.a = gammaColor.a;
			return result;
		}

		public static Vector3 RandomUVW()
		{
			return new Vector3(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
		}

		public static float Sample(AnimationCurve distribution)
		{
			float value;
			float value2;
			float num;
			do
			{
				value = UnityEngine.Random.value;
				value2 = UnityEngine.Random.value;
				num = distribution.Evaluate(value);
			}
			while (!(value2 < num));
			return value;
		}

		public static AnimationCurve Clone(AnimationCurve value)
		{
			return new AnimationCurve(value.keys);
		}

		public static Gradient Clone(Gradient value)
		{
			Gradient gradient = new Gradient();
			gradient.SetKeys(value.colorKeys, value.alphaKeys);
			return gradient;
		}

		public static AnimationCurve ConstantCurve(float value)
		{
			return AnimationCurve.Linear(0f, value, 1f, value);
		}

		public static Gradient ConstantGradient(Color color)
		{
			GradientColorKey[] colorKeys = new GradientColorKey[2]
			{
				new GradientColorKey(color, 0f),
				new GradientColorKey(color, 1f)
			};
			GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2]
			{
				new GradientAlphaKey(color.a, 0f),
				new GradientAlphaKey(color.a, 1f)
			};
			Gradient gradient = new Gradient();
			gradient.SetKeys(colorKeys, alphaKeys);
			return gradient;
		}

		public static Gradient DayNightGradient(Color color)
		{
			GradientColorKey[] colorKeys = new GradientColorKey[8]
			{
				new GradientColorKey(Color.Lerp(Color.black, color, 0.18f), 0f),
				new GradientColorKey(Color.Lerp(Color.black, color, 0.27f), 0.05f),
				new GradientColorKey(Color.Lerp(Color.black, color, 0.91f), 0.2f),
				new GradientColorKey(Color.Lerp(Color.black, color, 1f), 0.25f),
				new GradientColorKey(Color.Lerp(Color.black, color, 1f), 0.75f),
				new GradientColorKey(Color.Lerp(Color.black, color, 0.91f), 0.8f),
				new GradientColorKey(Color.Lerp(Color.black, color, 0.27f), 0.95f),
				new GradientColorKey(Color.Lerp(Color.black, color, 0.18f), 1f)
			};
			GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2]
			{
				new GradientAlphaKey(color.a, 0f),
				new GradientAlphaKey(color.a, 1f)
			};
			Gradient gradient = new Gradient();
			gradient.SetKeys(colorKeys, alphaKeys);
			return gradient;
		}

		public static float IntensityToGamma(float linearIntensity)
		{
			return Mathf.LinearToGammaSpace(linearIntensity * 2f);
		}

		public static float IntensityToLinear(float gammaIntensity)
		{
			return Mathf.GammaToLinearSpace(gammaIntensity) / 2f;
		}

		public static float Unlerp(float x, float min, float max)
		{
			return (x - min) / (max - min);
		}

		public static float SmoothUnlerp(float x, float min, float max)
		{
			return Mathf.SmoothStep(0f, 1f, Unlerp(x, min, max));
		}

		public static Vector3 GetFacingVector(Facing f)
		{
			switch (f)
			{
			case Facing.North:
				return new Vector3(0f, 0f, 1f);
			case Facing.South:
				return new Vector3(0f, 0f, -1f);
			case Facing.East:
				return new Vector3(1f, 0f, 0f);
			default:
				return new Vector3(-1f, 0f, 0f);
			}
		}

		public static float SineWaveNegOneToOne(float time)
		{
			return Mathf.Clamp(Mathf.Sin(-(float)System.Math.PI + (float)System.Math.PI * 2f * time), -1f, 1f);
		}

		public static void WriteBoolGrid3D(Array3<bool> g, BinaryWriter writer)
		{
			writer.Write(g.sizeX);
			writer.Write(g.sizeY);
			writer.Write(g.sizeZ);
			for (int i = 0; i < g.sizeX; i++)
			{
				for (int j = 0; j < g.sizeY; j++)
				{
					for (int k = 0; k < g.sizeZ; k++)
					{
						writer.Write(g[i, j, k]);
					}
				}
			}
		}

		public static Array3<bool> ReadBoolGrid3D(BinaryReader reader)
		{
			int num = reader.ReadInt32();
			int num2 = reader.ReadInt32();
			int num3 = reader.ReadInt32();
			Array3<bool> array = new Array3<bool>(num, num2, num3);
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < num2; j++)
				{
					for (int k = 0; k < num3; k++)
					{
						array[i, j, k] = reader.ReadBoolean();
					}
				}
			}
			return array;
		}

		public static float GetPointToBoxDistanceSquared(Vector3 point, Vector3 min, Vector3 max)
		{
			return (new Vector3(Mathf.Clamp(point.x, min.x, max.x), Mathf.Clamp(point.y, min.y, max.y), Mathf.Clamp(point.z, min.z, max.z)) - point).sqrMagnitude;
		}

		public static bool MakeAngleInCCWBounds(ref float degs, float minDegs, float maxDegs)
		{
			MakeCCWBounds(ref minDegs, ref maxDegs);
			while (degs < minDegs)
			{
				if ((double)Mathf.Abs(degs - minDegs) < 0.0001)
				{
					degs = minDegs;
				}
				else
				{
					degs += 360f;
				}
			}
			while (degs > maxDegs)
			{
				if ((double)Mathf.Abs(degs - maxDegs) < 0.0001)
				{
					degs = maxDegs;
				}
				else
				{
					degs -= 360f;
				}
			}
			if (degs <= maxDegs)
			{
				return degs >= minDegs;
			}
			return false;
		}

		public static void MakeCCWBounds(ref float minDegs, ref float maxDegs)
		{
			while (maxDegs < minDegs)
			{
				maxDegs += 360f;
			}
			while (minDegs + 360f <= maxDegs)
			{
				minDegs += 360f;
			}
		}

		public static bool IsNotNullOrEmpty(string s)
		{
			return !string.IsNullOrEmpty(s);
		}

		public static string ComputeHashSHA256(string message, string salt)
		{
			return HexToString(ComputeHashSHA256(Encoding.UTF8.GetBytes(message + salt)));
		}

		public static byte[] ComputeHashSHA256(byte[] data)
		{
			using (SHA256Managed sHA256Managed = new SHA256Managed())
			{
				return sHA256Managed.ComputeHash(data);
			}
		}

		public static string HexToString(byte[] data)
		{
			StringBuilder stringBuilder = new StringBuilder(data.Length * 2);
			foreach (byte b in data)
			{
				stringBuilder.AppendFormat("{0:x2}", b);
			}
			return stringBuilder.ToString();
		}

		public static long SDBMHash(string s)
		{
			long num = 0L;
			for (int i = 0; i < s.Length; i++)
			{
				num = s[i] + (num << 6) + (num << 16) - num;
			}
			return num;
		}

		public static int StevesDumbIntHash(string s)
		{
			int num = 0;
			for (int i = 0; i < s.Length; i++)
			{
				num = s[i] + (num << 3) + (num << 8) - num;
			}
			return num;
		}

		public static C Spawn<C>(C prefabComp, Transform parent = null) where C : MonoBehaviour
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(prefabComp.gameObject, prefabComp.gameObject.transform.position, prefabComp.gameObject.transform.rotation);
			gameObject.transform.parent = parent;
			return gameObject.GetComponent<C>();
		}

		public static GameObject Instantiate(GameObject prefab, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return EditorModifications.Instantiate(prefab, parent, localPosition, localRotation, localScale, awake: true);
		}

		public static GameObject Instantiate(GameObject prefab, Transform parent, Vector3 localPosition, Quaternion localRotation)
		{
			return EditorModifications.Instantiate(prefab, parent, localPosition, localRotation, awake: true);
		}

		public static GameObject InstantiateDeactivated(GameObject prefab, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return EditorModifications.Instantiate(prefab, parent, localPosition, localRotation, localScale, awake: false);
		}

		public static GameObject InstantiateDeactivated(GameObject prefab, Transform parent, Vector3 localPosition, Quaternion localRotation)
		{
			return EditorModifications.Instantiate(prefab, parent, localPosition, localRotation, awake: false);
		}

		public static GameObject InstantiateDeactivated(GameObject prefab, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return EditorModifications.Instantiate(prefab, null, localPosition, localRotation, localScale, awake: false);
		}

		public static GameObject InstantiateDeactivated(GameObject prefab, Vector3 localPosition, Quaternion localRotation)
		{
			return EditorModifications.Instantiate(prefab, null, localPosition, localRotation, awake: false);
		}

		public static GameObject InstantiateDeactivated(GameObject prefab)
		{
			return EditorModifications.Instantiate(prefab, null, Vector3.zero, Quaternion.identity, awake: false);
		}

		public static int HashInt(int a)
		{
			a = a ^ 0x3D ^ (a >> 16);
			a += a << 3;
			a ^= a >> 4;
			a *= 668265261;
			a ^= a >> 15;
			return a;
		}

		public static OutwardWalker3D Rings(int ringBound)
		{
			return new OutwardWalker3D(ringBound);
		}

		public static bool SameSign(float a, float b)
		{
			if (a > 0f)
			{
				return b >= 0f;
			}
			if (a < 0f)
			{
				return b <= 0f;
			}
			return true;
		}

		public static bool SamePair(int a, int b, int x, int y)
		{
			if (a != x || b != y)
			{
				if (a == y)
				{
					return b == x;
				}
				return false;
			}
			return true;
		}

		public static string ToDebugString(List<string> strs)
		{
			if (strs == null)
			{
				return "<null>";
			}
			return "[" + string.Join(",", strs.ToArray()) + "]";
		}

		public static string ToString(IEnumerable<object> items)
		{
			return string.Join(", ", items.Select((object p) => p.ToString()).ToArray());
		}

		public static float StableNoise2D(Vector2 co)
		{
			float num = Mathf.Sin(Vector2.Dot(co, new Vector2(12.9898f, 78.233f))) * 43758.547f;
			return num - Mathf.Floor(num);
		}

		public static float StableNoise(float x)
		{
			return StableNoise2D(new Vector2(x, x));
		}

		public static float Gaussian(Vector2 x, Vector2 center, float powScale)
		{
			float magnitude = (x - center).magnitude;
			return Mathf.Exp(-1f * magnitude * magnitude * powScale);
		}

		public static string GetRelativePath(string relativeTo, string fullPath)
		{
			if (string.IsNullOrEmpty(relativeTo))
			{
				throw new ArgumentNullException("relativeTo");
			}
			if (string.IsNullOrEmpty(fullPath))
			{
				throw new ArgumentNullException("fullPath");
			}
			Uri uri = new Uri(relativeTo);
			Uri uri2 = new Uri(fullPath);
			if (uri.Scheme != uri2.Scheme)
			{
				return fullPath;
			}
			string text = Uri.UnescapeDataString(uri.MakeRelativeUri(uri2).ToString());
			if (uri2.Scheme.ToUpperInvariant() == "FILE")
			{
				text = text.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			}
			return text;
		}

		public static bool EitherExists(params string[] files)
		{
			for (int i = 0; i < files.Length; i++)
			{
				if (FileUtils.FileExists(files[i]))
				{
					return true;
				}
			}
			return false;
		}

		public static string Either(params string[] files)
		{
			foreach (string text in files)
			{
				if (FileUtils.FileExists(text))
				{
					return text;
				}
			}
			return null;
		}

		public static Stream OpenEither(out int chosenFile, params string[] files)
		{
			for (int i = 0; i < files.Length; i++)
			{
				string path = files[i];
				if (FileUtils.FileExists(path))
				{
					chosenFile = i;
					return FileUtils.ReadFile(path);
				}
			}
			throw new FileNotFoundException();
		}

		public static Stream TryOpenEither(out int chosenFile, params string[] files)
		{
			for (int i = 0; i < files.Length; i++)
			{
				string path = files[i];
				if (FileUtils.FileExists(path))
				{
					chosenFile = i;
					return FileUtils.ReadFile(path);
				}
			}
			chosenFile = -1;
			return null;
		}

		public static StreamReader OpenEitherText(out int chosenFile, params string[] files)
		{
			for (int i = 0; i < files.Length; i++)
			{
				string path = files[i];
				if (FileUtils.FileExists(path))
				{
					chosenFile = i;
					return FileUtils.ReadTextFile(path);
				}
			}
			throw new FileNotFoundException();
		}

		[SuppressMessage("Subnautica.Rules", "EnsureLocalDisposalRule")]
		public static PooledBinaryReader OpenEitherBinary(out int chosenFile, params string[] files)
		{
			for (int i = 0; i < files.Length; i++)
			{
				string path = files[i];
				if (FileUtils.FileExists(path))
				{
					chosenFile = i;
					return new PooledBinaryReader(FileUtils.ReadFile(path));
				}
			}
			throw new FileNotFoundException();
		}

		[SuppressMessage("Gendarme.Rules.Portability", "MonoCompatibilityReviewRule")]
		public static bool GetDiskFull(Exception ex)
		{
			int num = Marshal.GetHRForException(ex) & 0xFFFF;
			if (num != 39)
			{
				return num == 112;
			}
			return true;
		}

		public static void CopyDirectory(string SourcePath, string DestinationPath)
		{
			DuplicateDirectory(SourcePath, DestinationPath);
		}

		public static void DuplicateDirectory(string SourcePath, string DestinationPath)
		{
			if (!Directory.Exists(SourcePath))
			{
				throw new Exception("DuplicateDirectory: SourcePath='" + SourcePath + "' DNE!");
			}
			if (!Directory.Exists(DestinationPath))
			{
				Directory.CreateDirectory(DestinationPath);
			}
			string[] directories = Directory.GetDirectories(SourcePath, "*", SearchOption.AllDirectories);
			for (int i = 0; i < directories.Length; i++)
			{
				Directory.CreateDirectory(directories[i].Replace(SourcePath, DestinationPath));
			}
			directories = Directory.GetFiles(SourcePath, "*.*", SearchOption.AllDirectories);
			foreach (string obj in directories)
			{
				File.Copy(obj, obj.Replace(SourcePath, DestinationPath), overwrite: true);
			}
		}

		public static bool IsDirectoryEmpty(string path)
		{
			try
			{
				return !Directory.EnumerateFileSystemEntries(path).Any();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
				return false;
			}
		}

		public static void MoveDirectory(string SourcePath, string DestinationPath, SearchOption searchOption)
		{
			if (!Directory.Exists(SourcePath))
			{
				throw new Exception("MoveDirectory: SourcePath='" + SourcePath + "' DNE!");
			}
			if (!Directory.Exists(DestinationPath))
			{
				Directory.CreateDirectory(DestinationPath);
			}
			string[] directories = Directory.GetDirectories(SourcePath, "*", searchOption);
			for (int i = 0; i < directories.Length; i++)
			{
				Directory.CreateDirectory(directories[i].Replace(SourcePath, DestinationPath));
			}
			directories = Directory.GetFiles(SourcePath, "*.*", searchOption);
			foreach (string text in directories)
			{
				string text2 = text.Replace(SourcePath, DestinationPath);
				File.Copy(text, text2, overwrite: true);
				if (File.Exists(text2))
				{
					try
					{
						File.Delete(text);
					}
					catch (Exception exception)
					{
						UnityEngine.Debug.LogException(exception);
					}
				}
			}
			directories = Directory.GetDirectories(SourcePath, "*", searchOption);
			foreach (string path in directories)
			{
				if (Directory.GetFiles(path).Length == 0)
				{
					try
					{
						Directory.Delete(path);
					}
					catch (Exception exception2)
					{
						UnityEngine.Debug.LogException(exception2);
					}
				}
			}
		}

		public static string GenerateNumberedFileName(string prefix, string suffix)
		{
			int num = 0;
			string text;
			while (true)
			{
				text = prefix + num + suffix;
				if (!File.Exists(text))
				{
					break;
				}
				num++;
			}
			return text;
		}

		public static void Swap<T>(ref T lhs, ref T rhs)
		{
			T val = lhs;
			lhs = rhs;
			rhs = val;
		}

		public static float StevePerlin(Vector2 uv)
		{
			return Mathf.Clamp01(Mathf.PerlinNoise(uv.x * 10f, uv.y * 10f)) * 2f - 1f;
		}

		public static bool ToggleChanged(bool val, string label)
		{
			bool num = val;
			val = GUILayout.Toggle(val, label);
			return num != val;
		}

		public static int Read<T>(List<T> list, StreamReader reader) where T : IASCIIReadWrite, new()
		{
			int num = int.Parse(reader.ReadLine().Trim());
			for (int i = 0; i < num; i++)
			{
				T item = new T();
				item.Read(reader);
				list.Add(item);
			}
			return num;
		}

		public static void Write<T>(List<T> list, StreamWriter writer) where T : IASCIIReadWrite
		{
			writer.WriteLine(list.Count);
			foreach (T item in list)
			{
				item.Write(writer);
			}
		}

		public static ushort[,] LoadRawU16SquareGrid(string filename)
		{
			using (Stream stream = FileUtils.ReadFile(filename))
			{
				using (BinaryReader binaryReader = new BinaryReader(stream))
				{
					int num = Mathf.FloorToInt(Mathf.Sqrt(stream.Length / 2));
					ushort[,] array = new ushort[num, num];
					byte[] array2 = binaryReader.ReadBytes((int)(stream.Length / 1));
					Buffer.BlockCopy(array2, 0, array, 0, array2.Length);
					return array;
				}
			}
		}

		public static float GetTimeElapsedMS(Stopwatch watch)
		{
			return (float)watch.ElapsedTicks * 1f / 10000f;
		}

		public static Vector3 CornerBoxRotatePoint(Vector3 mins, Vector3 maxs, int turns, Vector3 origPoint)
		{
			Quaternion quaternion = Quaternion.AngleAxis((float)turns * -90f, Vector3.up);
			Vector3 lhs = quaternion * mins;
			Vector3 rhs = quaternion * maxs;
			Vector3 vector = Vector3.Min(lhs, rhs);
			return quaternion * origPoint + mins - vector;
		}

		public static Bounds MinMaxBounds(Vector3 mins, Vector3 maxs)
		{
			Bounds result = default(Bounds);
			result.SetMinMax(mins, maxs);
			return result;
		}

		public static void PushLockCursor(bool lc)
		{
			LockCursorStack.Push(lockCursor);
			lockCursor = lc;
		}

		public static void PopLockCursor()
		{
			lockCursor = LockCursorStack.Pop();
		}

		private static void UpdateCusorLockState()
		{
			if (_alwaysLockCursor)
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
			else
			{
				Cursor.lockState = (_lockCursor ? CursorLockMode.Locked : CursorLockMode.None);
				Cursor.visible = !_lockCursor;
			}
		}

		public static void EnqueueWrap(WorkerThread thread, IWorkerTask task)
		{
			thread.Enqueue(WorkerTask.ExecuteTaskDelegate, task, null);
		}

		public static void DestroyWrap(UnityEngine.Object o, float time = 0f)
		{
			UnityEngine.Object.Destroy(o, time);
		}

		public static GameObject InstantiateWrap(GameObject prefab, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion))
		{
			return UnityEngine.Object.Instantiate(prefab, position, rotation);
		}

		public static bool IsPrefabZUp(GameObject prefab)
		{
			return Mathf.Abs(Mathf.DeltaAngle(prefab.transform.localEulerAngles.x, 270f)) < 10f;
		}

		public static string GetValuesAsString<E>()
		{
			string text = "";
			E[] array = (E[])Enum.GetValues(typeof(E));
			for (int i = 0; i < array.Length; i++)
			{
				E val = array[i];
				text = text + val.ToString() + ",";
			}
			return text;
		}

		public static T ParseEnum<T>(string val)
		{
			return (T)Enum.Parse(typeof(T), val, ignoreCase: true);
		}

		public static bool TryParseEnum<T>(string val, out T result) where T : struct
		{
			try
			{
				result = (T)Enum.Parse(typeof(T), val, ignoreCase: true);
				return true;
			}
			catch
			{
				result = default(T);
				return false;
			}
		}

		public static bool TryParseBatchNumber(string fileName, out Int3 result)
		{
			result = default(Int3);
			int num = 0;
			foreach (char c in fileName)
			{
				int num2 = c - 48;
				if (num % 2 == 0)
				{
					if (num2 >= 0 && num2 <= 9)
					{
						result[num / 2] = num2;
						num++;
					}
					else
					{
						num = 0;
					}
				}
				else if (num2 >= 0 && num2 <= 9)
				{
					result[num / 2] = result[num / 2] * 10 + num2;
				}
				else
				{
					if (num == 5)
					{
						break;
					}
					num = ((c == '-') ? (num + 1) : 0);
				}
			}
			if (num == 5)
			{
				return true;
			}
			result = Int3.negativeOne;
			return false;
		}

		public static void LogMonoMemStats(string prefix = null)
		{
			if (prefix != null)
			{
				memStatLogCount = 0;
				memStatLogPrefix = prefix;
			}
			UnityEngine.Debug.Log("mono used/heap (" + memStatLogPrefix + " " + memStatLogCount + ") = " + (float)Profiler.GetMonoUsedSizeLong() / 1024f / 1024f + " / " + (float)Profiler.GetMonoHeapSizeLong() / 1024f / 1024f + " MB");
			memStatLogCount++;
		}

		public static T[] EnsureMinSize<T>(string label, ref T[] array, int size)
		{
			if (array == null || array.Length < size)
			{
				Array.Resize(ref array, Mathf.NextPowerOfTwo(size));
			}
			return array;
		}

		public static bool RayThenCapsuleCast(Vector3 start, Vector3 end, float capsuleRadius, Vector3 sweepDirection, out RaycastHit hitInfo, int layerMask = -5)
		{
			if (Physics.Raycast(start, sweepDirection, out hitInfo, (end - start).magnitude, layerMask))
			{
				return true;
			}
			return Physics.CapsuleCast(start, end, capsuleRadius, sweepDirection, out hitInfo, (end - start).magnitude, layerMask);
		}

		public static string GetName(Action method)
		{
			return method.Method.Name;
		}

		public static string GetName<T>(Action<T> method)
		{
			return method.Method.Name;
		}

		public static string GetName<TResult>(Func<TResult> method)
		{
			return method.Method.Name;
		}

		public static string GetName<T, TResult>(Func<T, TResult> method)
		{
			return method.Method.Name;
		}

		public static string GetName(Delegate method)
		{
			return method.Method.Name;
		}

		public static void InvokeOnce(MonoBehaviour behaviour, Action method, float delay)
		{
			InvokeOnce(behaviour, GetName(method), delay);
		}

		public static void InvokeOnce(MonoBehaviour behaviour, string method, float delay)
		{
			behaviour.CancelInvoke(method);
			behaviour.Invoke(method, delay);
		}

		private static Vector3[] VertexPositionsFromMeshBuffer(MeshBuffer meshBuffer)
		{
			Vector3[] array = new Vector3[meshBuffer.numVerts];
			for (int i = 0; i < meshBuffer.numVerts; i++)
			{
				if (meshBuffer.colliderVertices != null)
				{
					array[i] = meshBuffer.colliderVertices[i].position;
				}
				else if (meshBuffer.grassVertices != null)
				{
					array[i] = meshBuffer.grassVertices[i].position;
				}
				else if (meshBuffer.layerVertices != null)
				{
					array[i] = meshBuffer.layerVertices[i].position;
				}
			}
			return array;
		}

		public static void DumpOBJFile(string path, MeshBuffer meshBuffer)
		{
			Vector3[] vertices = VertexPositionsFromMeshBuffer(meshBuffer);
			DumpOBJFile(path, vertices, meshBuffer.triangles, meshBuffer.numVerts, meshBuffer.numTris);
		}

		public static void DumpOBJFile(string path, Vector3[] vertices, IAlloc<ushort> triInds, int numVerts, int numTris, int startVert = 0, int startTri = 0)
		{
			using (StreamWriter streamWriter = FileUtils.CreateTextFile(path))
			{
				streamWriter.WriteLine("# " + numVerts + " verts, " + numTris + " tris");
				for (int i = 0; i < numVerts; i++)
				{
					Vector3 vector = vertices[startVert + i];
					streamWriter.WriteLine("v " + vector.x + " " + vector.y + " " + vector.z);
				}
				for (int j = 0; j < numTris / 3; j++)
				{
					int num = triInds[startTri + 3 * j] + 1;
					int num2 = triInds[startTri + (3 * j + 1)] + 1;
					int num3 = triInds[startTri + (3 * j + 2)] + 1;
					streamWriter.WriteLine("f " + num + " " + num2 + " " + num3);
				}
			}
		}

		public static bool ByteEquals(byte[] a, byte[] b)
		{
			if (a.Length != b.Length)
			{
				return false;
			}
			for (int i = 0; i < a.Length; i++)
			{
				if (a[i] != b[i])
				{
					return false;
				}
			}
			return true;
		}

		public static void EnterPhysicsSyncSection()
		{
			if (!inPhysicsSyncTransformSection)
			{
				physicsSyncTransformsSave = Physics.autoSyncTransforms;
				inPhysicsSyncTransformSection = true;
			}
			Physics.autoSyncTransforms = true;
		}

		public static void ExitPhysicsSyncSection()
		{
			if (inPhysicsSyncTransformSection)
			{
				Physics.autoSyncTransforms = physicsSyncTransformsSave;
				inPhysicsSyncTransformSection = false;
			}
		}
	}
}
