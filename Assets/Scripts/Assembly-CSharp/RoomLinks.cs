using System.Collections.Generic;
using UWE;
using UnityEngine;

public class RoomLinks : MonoBehaviour
{
	public class CyclopsRoomsComparer : IEqualityComparer<CyclopsRooms>
	{
		public bool Equals(CyclopsRooms x, CyclopsRooms y)
		{
			int num = (int)x;
			return num.Equals((int)y);
		}

		public int GetHashCode(CyclopsRooms obj)
		{
			return (int)obj;
		}
	}

	public CyclopsRooms room;

	public CyclopsRooms[] roomLinks;

	private static CyclopsRoomsComparer sCyclopsRoomsComparer = new CyclopsRoomsComparer();

	public static CyclopsRoomsComparer RoomsComparer => sCyclopsRoomsComparer;

	private void OnTriggerEnter(Collider col)
	{
		GameObject entityRoot = UWE.Utils.GetEntityRoot(col.gameObject);
		if (!entityRoot)
		{
			entityRoot = col.gameObject;
		}
		Player componentInHierarchy = UWE.Utils.GetComponentInHierarchy<Player>(entityRoot);
		if ((bool)componentInHierarchy && !(componentInHierarchy.currentSub == null))
		{
			SendMessageUpwards("SetPlayerRoom", room, SendMessageOptions.DontRequireReceiver);
		}
	}
}
