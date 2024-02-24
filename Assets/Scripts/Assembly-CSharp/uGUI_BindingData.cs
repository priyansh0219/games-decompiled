using UnityEngine;
using UnityEngine.UI;

public class uGUI_BindingData : ScriptableObject
{
	[Header("Normal")]
	public SpriteState stateNormal;

	[Space]
	[Header("Selected")]
	public SpriteState stateSelected;

	[Space]
	[Header("Active")]
	public SpriteState stateActive;
}
