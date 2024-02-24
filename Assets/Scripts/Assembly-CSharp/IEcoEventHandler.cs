using UnityEngine;

public interface IEcoEventHandler
{
	EcoEventType GetEventType();

	float GetRange();

	Vector3 GetPosition();

	string GetName();

	void OnEcoEvent(EcoEvent e);
}
