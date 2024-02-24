using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[ExecuteInEditMode]
public class VFXFabricating : MonoBehaviour, ICompileTimeCheckable
{
	public float localMinY;

	public float localMaxY;

	public Vector3 posOffset;

	public Vector3 eulerOffset;

	public float scaleFactor = 1f;

	public bool allowOnAnyComponent;

	public float minY => base.transform.position.y + localMinY;

	public float maxY => base.transform.position.y + localMaxY;

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.cyan;
		Vector3 from = base.transform.position + posOffset;
		Vector3 to = base.transform.position + posOffset;
		from.y = minY + posOffset.y;
		to.y = maxY + posOffset.y;
		Gizmos.DrawLine(from, to);
		MeshFilter componentInChildren = base.gameObject.GetComponentInChildren<MeshFilter>();
		if (componentInChildren != null)
		{
			Mesh sharedMesh = componentInChildren.sharedMesh;
			if (sharedMesh != null)
			{
				Gizmos.DrawWireMesh(sharedMesh, base.transform.position + posOffset, Quaternion.Euler(eulerOffset), Vector3.one * scaleFactor);
			}
		}
	}

	public string CompileTimeCheck()
	{
		if (allowOnAnyComponent)
		{
			return null;
		}
		HashSet<Type> hashSet = new HashSet<Type>
		{
			typeof(Transform),
			typeof(VFXFabricating),
			typeof(MeshFilter),
			typeof(MeshRenderer),
			typeof(SkinnedMeshRenderer),
			typeof(SkyApplier),
			typeof(LODGroup),
			typeof(Animator)
		};
		List<Component> list = new List<Component>();
		StringBuilder stringBuilder = null;
		GetComponentsInChildren(includeInactive: true, list);
		for (int i = 0; i < list.Count; i++)
		{
			Component component = list[i];
			if (!(component != null))
			{
				continue;
			}
			Type type = component.GetType();
			if (!hashSet.Contains(type))
			{
				if (!Targeting.GetRoot(base.gameObject, out var _, out var gameObject))
				{
					gameObject = base.gameObject;
				}
				if (stringBuilder == null)
				{
					stringBuilder = new StringBuilder();
					stringBuilder.AppendFormat("GameObject (and it's children) with VFXFabricating script will be instantiated as a ghost model for fabrication effect, so it should only contain visual stuff. The following components doesn't meet this requirement:\n");
				}
				stringBuilder.AppendFormat(" - '{0}' component on '{1}' gameObject\n", type.Name, gameObject.name);
			}
		}
		if (stringBuilder != null)
		{
			stringBuilder.AppendFormat("Move them out of gameObject with VFXFabricating component hierarchy. Use FollowTransform component on your gameObject if you need to attach it to a specific transform in this hierarchy.\n");
			return stringBuilder.ToString();
		}
		return null;
	}
}
