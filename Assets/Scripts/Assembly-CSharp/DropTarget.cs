using UnityEngine;

public class DropTarget : MonoBehaviour
{
	public virtual bool DoesAcceptDrop(Pickupable p)
	{
		return false;
	}
}
