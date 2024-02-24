using UnityEngine;

public sealed class uGUI_SafeAreaDisabler : MonoBehaviour
{
	[SerializeField]
	[AssertNotNull]
	private uGUI_SafeAreaScaler scaler;

	private void OnEnable()
	{
		scaler.SetDisabledState(state: true);
	}

	private void OnDisable()
	{
		scaler.SetDisabledState(state: false);
	}
}
