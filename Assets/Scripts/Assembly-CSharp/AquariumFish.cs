using UnityEngine;

public class AquariumFish : MonoBehaviour, ICompileTimeCheckable
{
	[AssertNotNull]
	public GameObject model;

	private void OnKill()
	{
		Object.Destroy(this);
	}

	public string CompileTimeCheck()
	{
		if (model.GetComponentInChildren<SkyApplier>(includeInactive: true) == null)
		{
			return $"Aquarium fish model ({model.name}) must have a SkyApplier component in the hierarchy";
		}
		return null;
	}
}
