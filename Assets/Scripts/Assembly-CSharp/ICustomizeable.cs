using UnityEngine;

public interface ICustomizeable
{
	Vector3[] GetColors();

	string GetName();

	Vector3 GetColor(int i);

	void SetName(string text);

	void SetColor(int index, Vector3 hsb, Color color);

	void RegisterInputHandler(SubNameInput input);

	void UnregisterInputHandler(SubNameInput input);
}
