using UnityEngine;

public class VFXPrecursorDisableGunTerminal : MonoBehaviour
{
	[AssertNotNull]
	public VFXLerpColor lockScreenColorLerper;

	public void OnPrecursorDesactivationTerminalLock()
	{
		lockScreenColorLerper.reverse = false;
		lockScreenColorLerper.Play();
	}

	public void OnPrecursorDesactivationTerminalUnlock()
	{
		lockScreenColorLerper.reverse = true;
		lockScreenColorLerper.Play();
	}
}
