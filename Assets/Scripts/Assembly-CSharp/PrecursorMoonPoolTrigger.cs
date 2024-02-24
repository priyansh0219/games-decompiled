using System.Collections.Generic;
using UWE;
using UnityEngine;

public class PrecursorMoonPoolTrigger : MonoBehaviour, IWaterPlane
{
	public static bool inMoonpool;

	[AssertNotNull]
	public BoxCollider boxCollider;

	[Range(0f, 1f)]
	public float waterPlaneY;

	private Player checkPlayer;

	private readonly HashSet<Vehicle> checkVehicles = new HashSet<Vehicle>();

	private void OnTriggerEnter(Collider col)
	{
		if (!(col.gameObject != null) || !(col.gameObject.GetComponentInChildren<IgnoreTrigger>() != null))
		{
			GameObject entityRoot = UWE.Utils.GetEntityRoot(col.gameObject);
			if (!entityRoot)
			{
				entityRoot = col.gameObject;
			}
			Player componentInHierarchy = UWE.Utils.GetComponentInHierarchy<Player>(entityRoot);
			if ((bool)componentInHierarchy)
			{
				checkPlayer = componentInHierarchy;
				componentInHierarchy.SetWaterPlane(this);
			}
			Vehicle componentInHierarchy2 = UWE.Utils.GetComponentInHierarchy<Vehicle>(entityRoot);
			if ((bool)componentInHierarchy2)
			{
				checkVehicles.Add(componentInHierarchy2);
			}
			WorldForces component = entityRoot.GetComponent<WorldForces>();
			if ((bool)component)
			{
				component.waterDepth = base.transform.position.y + GetWaterPlaneY();
			}
		}
	}

	private void OnTriggerExit(Collider col)
	{
		if (!(col.gameObject.GetComponentInChildren<IgnoreTrigger>() != null))
		{
			GameObject entityRoot = UWE.Utils.GetEntityRoot(col.gameObject);
			if (!entityRoot)
			{
				entityRoot = col.gameObject;
			}
			Player componentInHierarchy = UWE.Utils.GetComponentInHierarchy<Player>(entityRoot);
			if ((bool)componentInHierarchy)
			{
				checkPlayer = null;
				componentInHierarchy.SetWaterPlane(null);
			}
			Vehicle componentInHierarchy2 = UWE.Utils.GetComponentInHierarchy<Vehicle>(entityRoot);
			if ((bool)componentInHierarchy2)
			{
				checkVehicles.Remove(componentInHierarchy2);
			}
			WorldForces component = entityRoot.GetComponent<WorldForces>();
			if ((bool)component)
			{
				component.waterDepth = Ocean.GetOceanLevel();
			}
		}
	}

	private bool CheckPlayerWithinBounds()
	{
		float num = (boxCollider.size.x + boxCollider.size.y + boxCollider.size.z) / 3f * 2f;
		if (Vector3.Distance(Player.main.transform.position, base.transform.position) < num)
		{
			return true;
		}
		return false;
	}

	private void Update()
	{
		inMoonpool = checkPlayer != null;
		if ((bool)checkPlayer)
		{
			float y = checkPlayer.transform.position.y;
			float num = base.transform.position.y + GetWaterPlaneY();
			bool precursorOutOfWater = y > num;
			if (!checkPlayer.SetPrecursorOutOfWater(precursorOutOfWater))
			{
				checkPlayer.SetWaterPlane(null);
				checkPlayer = null;
			}
		}
		HashSet<Vehicle>.Enumerator enumerator = checkVehicles.GetEnumerator();
		while (enumerator.MoveNext())
		{
			Vehicle current = enumerator.Current;
			if ((bool)current)
			{
				float y2 = current.transform.position.y;
				float num2 = base.transform.position.y + GetWaterPlaneY();
				bool precursorOutOfWater2 = y2 > num2;
				current.precursorOutOfWater = precursorOutOfWater2;
			}
		}
	}

	private float GetWaterPlaneY()
	{
		float num = boxCollider.size.y / 2f;
		return Mathf.Lerp(0f - num, num, waterPlaneY);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		float y = base.transform.position.y + GetWaterPlaneY();
		float x = base.transform.position.x;
		float z = base.transform.position.z;
		float num = boxCollider.size.x / 2f;
		float num2 = boxCollider.size.z / 2f;
		Vector3[] array = new Vector3[4]
		{
			new Vector3(x - num, y, z - num2),
			new Vector3(x - num, y, z + num2),
			new Vector3(x + num, y, z - num2),
			new Vector3(x + num, y, z + num2)
		};
		Gizmos.DrawLine(array[0], array[1]);
		Gizmos.DrawLine(array[1], array[2]);
		Gizmos.DrawLine(array[2], array[3]);
		Gizmos.DrawLine(array[3], array[0]);
		Gizmos.DrawLine(array[0], array[2]);
		Gizmos.DrawLine(array[1], array[3]);
	}

	public float GetWaterLevel()
	{
		return base.transform.position.y + GetWaterPlaneY();
	}
}
