using UnityEngine;
using UnityEngine.UI;

public class uGUI_LogEntryData : ScriptableObject
{
	[Header("Play")]
	[AssertNotNull]
	public SpriteState spritePlay;

	[Header("Stop")]
	[AssertNotNull]
	public SpriteState spriteStop;
}
