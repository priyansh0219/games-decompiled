using System.Collections.Generic;
using System.Text;
using Gendarme;
using UnityEngine;

public class DebugTargetConsoleCommand : MonoBehaviour
{
	public enum Reason
	{
		Accept = 0,
		AncestorOfIgnoredParent = 1,
		TriggerNotUseable = 2,
		ColliderNotUseable = 3,
		FromTechTypeExcludeList = 4
	}

	private struct Item
	{
		public float radius;

		public RaycastHit hit;

		public Reason reason;
	}

	private const ManagedUpdate.Queue updateQueue = ManagedUpdate.Queue.UpdateAfterInput;

	[SuppressMessage("Gendarme.Rules.Concurrency", "NonConstantStaticFieldsShouldNotBeVisibleRule")]
	public static float radius = 0f;

	public static DebugTargetConsoleCommand main;

	private static bool isRecording = false;

	private static StringBuilder sb = new StringBuilder();

	private static string result = string.Empty;

	private static HashSet<Collider> uniqueColliders = new HashSet<Collider>();

	private static List<Item> items = new List<Item>();

	private static int frame = -1;

	private bool debug;

	private Material material;

	private Mesh cubeMesh;

	private Mesh sphereMesh;

	private Mesh cylinderMesh;

	private Mesh hemisphereMesh;

	private MaterialPropertyBlock _propertyBlock;

	[SuppressMessage("Gendarme.Rules.Concurrency", "WriteStaticFieldFromInstanceMethodRule")]
	private void Awake()
	{
		main = this;
		Shader shader = Shader.Find("Standard");
		material = new Material(shader);
		cubeMesh = new Mesh();
		MathExtensions.Cube(ref cubeMesh, 1f, 1f, 1f);
		sphereMesh = new Mesh();
		MathExtensions.Sphere(ref sphereMesh, 0.5f, 16);
		cylinderMesh = new Mesh();
		MathExtensions.Cylinder(ref cylinderMesh, 0.5f, 1f, 16, 1);
		hemisphereMesh = new Mesh();
		MathExtensions.Hemisphere(ref hemisphereMesh, 0.5f, 16);
		_propertyBlock = new MaterialPropertyBlock();
		DevConsole.RegisterConsoleCommand(this, "target", caseSensitiveArgs: true);
	}

	private void OnEnable()
	{
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.UpdateAfterInput, OnUpdate);
	}

	private void OnDisable()
	{
		ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.UpdateAfterInput, OnUpdate);
	}

	private void OnDestroy()
	{
		Object.Destroy(material);
		Object.Destroy(cubeMesh);
		Object.Destroy(sphereMesh);
		Object.Destroy(cylinderMesh);
		Object.Destroy(hemisphereMesh);
	}

	private void OnUpdate()
	{
		if (debug && frame == Time.frameCount)
		{
			Dbg.Write(result);
		}
	}

	private void OnConsoleCommand_target(NotificationCenter.Notification n)
	{
		debug = !debug;
		ErrorMessage.AddMessage(string.Format("Targeting debug is now {0}", debug ? "On" : "Off"));
	}

	public static void RecordNext()
	{
		if (!(main == null) && main.debug)
		{
			isRecording = true;
			sb.Length = 0;
			items.Clear();
			frame = Time.frameCount;
		}
	}

	public static void Log(float radius)
	{
		if (isRecording)
		{
			if (radius <= 0f)
			{
				sb.Append("Raycast:\n");
			}
			else
			{
				sb.AppendFormat("Spherecast (radius = {0}):\n", radius);
			}
		}
	}

	public static void Log(Reason reason, RaycastHit hit)
	{
		if (isRecording)
		{
			Item item = default(Item);
			item.radius = radius;
			item.hit = hit;
			item.reason = reason;
			Item item2 = item;
			items.Add(item2);
		}
	}

	public static void Stop()
	{
		if (!isRecording)
		{
			return;
		}
		isRecording = false;
		items.Sort(Comparer);
		float num = float.MinValue;
		int num2 = 1;
		int count = items.Count;
		uniqueColliders.Clear();
		for (int i = 0; i < count; i++)
		{
			Item item = items[i];
			Collider collider = item.hit.collider;
			if (collider != null)
			{
				uniqueColliders.Add(collider);
			}
			if (item.reason == Reason.Accept)
			{
				break;
			}
		}
		int num3 = 0;
		int count2 = uniqueColliders.Count;
		for (int j = 0; j < count; j++)
		{
			Item item2 = items[j];
			RaycastHit hit = item2.hit;
			Reason reason = item2.reason;
			bool flag = reason == Reason.Accept;
			if (num != item2.radius)
			{
				num2 = 1;
				num = item2.radius;
				if (j > 0)
				{
					sb.Append("\n");
				}
				if (num <= 0f)
				{
					sb.Append("Raycast:\n");
				}
				else
				{
					sb.AppendFormat("Spherecast (radius={0})\n", num);
				}
			}
			sb.AppendFormat(" {0} {1}\n   {2:0.000} ", num2, MathExtensions.FormatPath(hit.collider.transform, 50, "   "), hit.distance);
			if (flag)
			{
				sb.Append("<color=#94DE00FF>");
			}
			else
			{
				sb.Append("<color=#DF4026FF>");
			}
			switch (reason)
			{
			case Reason.Accept:
			{
				sb.Append("accepted");
				TechType techType = CraftData.GetTechType(hit.collider.gameObject);
				if (techType != 0)
				{
					sb.Append(" TechType: ");
					sb.Append(techType.AsString());
				}
				break;
			}
			case Reason.AncestorOfIgnoredParent:
				sb.Append("discarded (ancestor of ignored parent)");
				break;
			case Reason.TriggerNotUseable:
				sb.Append("discarded (trigger not in Useable layer)");
				break;
			case Reason.ColliderNotUseable:
				sb.Append("discarded (collider in NotUseable layer)");
				break;
			case Reason.FromTechTypeExcludeList:
				sb.Append("discarded (from tech type exclude list)");
				break;
			default:
				sb.Append("unhandled reason");
				break;
			}
			sb.Append("\n</color>");
			Collider collider2 = hit.collider;
			if (collider2 != null && uniqueColliders.Remove(collider2))
			{
				Color color = (flag ? Color.green : Color.red);
				float f = ((count2 > 1) ? ((float)num3 / (float)(count2 - 1)) : 1f);
				f = Mathf.Pow(f, 2.2f);
				color.a = Mathf.Lerp(0.2f, 1f, f);
				main.DrawCollider(collider2, color);
				num3++;
			}
			num2++;
			if (reason == Reason.Accept)
			{
				break;
			}
		}
		uniqueColliders.Clear();
		items.Clear();
		result = sb.ToString();
	}

	private static int Comparer(Item x, Item y)
	{
		int num = x.radius.CompareTo(y.radius);
		if (num != 0)
		{
			return num;
		}
		return x.hit.distance.CompareTo(y.hit.distance);
	}

	public void DrawCollider(Collider collider, Color color)
	{
		if (collider == null)
		{
			return;
		}
		_ = collider.transform;
		int layer = 0;
		Camera camera = null;
		_propertyBlock.SetColor(ShaderPropertyID._Color, color);
		BoxCollider boxCollider = collider as BoxCollider;
		if (boxCollider != null)
		{
			Graphics.DrawMesh(cubeMesh, MathExtensions.GetBoxColliderMatrix(boxCollider), material, layer, camera, 0, _propertyBlock);
			return;
		}
		MeshCollider meshCollider = collider as MeshCollider;
		if (meshCollider != null)
		{
			Graphics.DrawMesh(meshCollider.sharedMesh, MathExtensions.GetMeshColliderMatrix(meshCollider), material, layer, camera, 0, _propertyBlock);
			return;
		}
		SphereCollider sphereCollider = collider as SphereCollider;
		if (sphereCollider != null)
		{
			Graphics.DrawMesh(sphereMesh, MathExtensions.GetSphereColliderMatrix(sphereCollider), material, layer, camera, 0, _propertyBlock);
			return;
		}
		CapsuleCollider capsuleCollider = collider as CapsuleCollider;
		if (capsuleCollider != null)
		{
			MathExtensions.GetCapsuleColliderMatrix(capsuleCollider, out var m, out var m2, out var m3);
			Graphics.DrawMesh(cylinderMesh, m, material, layer, camera, 0, _propertyBlock);
			Graphics.DrawMesh(hemisphereMesh, m2, material, layer, camera, 0, _propertyBlock);
			Graphics.DrawMesh(hemisphereMesh, m3, material, layer, camera, 0, _propertyBlock);
			return;
		}
		CharacterController characterController = collider as CharacterController;
		if (characterController != null)
		{
			MathExtensions.GetCharacterControllerMatrix(characterController, out var m4, out var m5, out var m6);
			Graphics.DrawMesh(cylinderMesh, m4, material, layer, camera, 0, _propertyBlock);
			Graphics.DrawMesh(hemisphereMesh, m5, material, layer, camera, 0, _propertyBlock);
			Graphics.DrawMesh(hemisphereMesh, m6, material, layer, camera, 0, _propertyBlock);
		}
	}
}
