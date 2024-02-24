using System.Collections.Generic;
using UnityEngine;

namespace UWE
{
	public class TempCollisionDisabler
	{
		private List<Collider> collidersToRestore = new List<Collider>();

		public void DisableColliders(GameObject obj)
		{
			if (collidersToRestore.Count > 0)
			{
				Debug.LogError("TempCollisionDisabler disable/restore call mismatch");
				return;
			}
			collidersToRestore.Clear();
			Collider[] componentsInChildren = obj.GetComponentsInChildren<Collider>(includeInactive: true);
			foreach (Collider collider in componentsInChildren)
			{
				if (collider.enabled)
				{
					collider.enabled = false;
					collidersToRestore.Add(collider);
				}
			}
		}

		public void RestoreColliders()
		{
			foreach (Collider item in collidersToRestore)
			{
				item.enabled = true;
			}
			collidersToRestore.Clear();
		}
	}
}
