using UnityEngine;

public interface IExosuitArm
{
	GameObject GetGameObject();

	GameObject GetInteractableRoot(GameObject target);

	void SetSide(Exosuit.Arm arm);

	bool OnUseDown(out float cooldownDuration);

	bool OnUseHeld(out float cooldownDuration);

	bool OnUseUp(out float cooldownDuration);

	bool OnAltDown();

	void Update(ref Quaternion aimDirection);

	void ResetArm();
}
