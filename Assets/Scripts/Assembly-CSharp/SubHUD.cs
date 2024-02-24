using UnityEngine;

[RequireComponent(typeof(GUIText))]
public class SubHUD : MonoBehaviour
{
	private void Start()
	{
		Utils.GetLocalPlayerComp().playerModeChanged.AddHandler(base.gameObject, OnPlayerModeChanged);
	}

	private void OnPlayerModeChanged(Player.Mode mode)
	{
		SetActive(mode == Player.Mode.Piloting);
	}

	private void SetActive(bool activeState)
	{
		base.gameObject.GetComponent<GUIText>().enabled = activeState;
	}
}
