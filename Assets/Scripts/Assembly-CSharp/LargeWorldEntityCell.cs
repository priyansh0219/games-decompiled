using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class LargeWorldEntityCell : MonoBehaviour
{
	public EntityCell cell;

	private void OnDrawGizmosSelected()
	{
		if (cell != null)
		{
			Vector3 vector = cell.GetSize().ToVector3();
			switch (cell.Level)
			{
			case 0:
				Gizmos.color = Color.green.ToAlpha(0.25f);
				break;
			case 1:
				Gizmos.color = Color.red.ToAlpha(0.25f);
				break;
			case 2:
				Gizmos.color = Color.blue.ToAlpha(0.25f);
				break;
			case 3:
				Gizmos.color = Color.white.ToAlpha(0.25f);
				break;
			default:
				Gizmos.color = new Color(1f, 0f, 1f, 1f);
				break;
			}
			Gizmos.DrawCube(base.transform.position, vector * 0.95f);
		}
	}
}
