using UnityEngine;

public class StorageObstacle : MonoBehaviour, IObstacle
{
	[AssertNotNull]
	public StorageContainer storageContainer;

	public bool IsDeconstructionObstacle()
	{
		return true;
	}

	bool IObstacle.CanDeconstruct(out string reason)
	{
		if (storageContainer.preventDeconstructionIfNotEmpty && !storageContainer.IsEmpty())
		{
			reason = Language.main.Get("DeconstructNonEmptyStorageContainerError");
			return false;
		}
		reason = null;
		return true;
	}
}
