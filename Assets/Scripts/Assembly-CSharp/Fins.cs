using UnityEngine;

public class Fins : MonoBehaviour, IEquippable
{
	private bool equipped;

	public void OnEquip(GameObject sender, string slot)
	{
		equipped = true;
	}

	public void OnUnequip(GameObject sender, string slot)
	{
		equipped = false;
	}

	public void UpdateEquipped(GameObject sender, string slot)
	{
	}
}
