using UnityEngine;

public interface IEcoTarget
{
	EcoTargetType GetTargetType();

	Vector3 GetPosition();

	string GetName();

	GameObject GetGameObject();
}
