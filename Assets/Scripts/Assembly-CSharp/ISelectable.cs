using Gendarme;
using UnityEngine;

public interface ISelectable
{
	bool IsValid();

	RectTransform GetRect();

	[SuppressMessage("Gendarme.Rules.Naming", "AvoidRedundancyInMethodNameRule")]
	bool OnButtonDown(GameInput.Button button);
}
