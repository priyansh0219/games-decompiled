using UnityEngine;

public interface IEquippable
{
	void OnEquip(GameObject sender, string slot);

	void OnUnequip(GameObject sender, string slot);

	void UpdateEquipped(GameObject sender, string slot);
}
