using TMPro;
using UnityEngine;

public class CyclopsDecoyScreenHUDManager : MonoBehaviour
{
	[AssertNotNull]
	public TextMeshProUGUI curCountText;

	[AssertNotNull]
	public TextMeshProUGUI maxCountText;

	[AssertNotNull]
	public CyclopsDecoyManager decoyManager;

	[AssertNotNull]
	public Animator animator;

	public void UpdateDecoyScreen()
	{
		curCountText.text = decoyManager.decoyCount.ToString();
		maxCountText.text = "/" + decoyManager.decoyMax;
	}

	public void SlotNewDecoy()
	{
		animator.SetTrigger("Pulse");
	}
}
