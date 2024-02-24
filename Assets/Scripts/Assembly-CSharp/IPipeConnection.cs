using UnityEngine;

public interface IPipeConnection
{
	void SetParent(IPipeConnection parent);

	IPipeConnection GetParent();

	void SetRoot(IPipeConnection root);

	IPipeConnection GetRoot();

	void AddChild(IPipeConnection child);

	void RemoveChild(IPipeConnection child);

	GameObject GetGameObject();

	bool GetProvidesOxygen();

	Vector3 GetAttachPoint();

	void UpdateOxygen();
}
