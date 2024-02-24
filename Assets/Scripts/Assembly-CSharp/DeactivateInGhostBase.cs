using UnityEngine;

internal class DeactivateInGhostBase : MonoBehaviour
{
	public bool activeInBase = true;

	public bool activeInGhostBase;

	private void OnAddedToBase(Base baseComp)
	{
		if (!(baseComp == null))
		{
			if (baseComp.isGhost)
			{
				base.gameObject.SetActive(activeInGhostBase);
			}
			else
			{
				base.gameObject.SetActive(activeInBase);
			}
		}
	}
}
