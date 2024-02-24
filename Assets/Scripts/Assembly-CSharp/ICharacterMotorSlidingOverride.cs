using UnityEngine;

public interface ICharacterMotorSlidingOverride
{
	void OnPlayerHit(ControllerColliderHit hit, GroundMotor motor);

	bool IsTooSteep();

	Vector3 GetMoveDirection();
}
