using TMPro;
using UWE;
using UnityEngine;

public class uGUI_HardcoreGameOver : uGUI_InputGroup
{
	private const FreezeTime.Id freezerId = FreezeTime.Id.HardcoreGameOver;

	[AssertLocalization]
	private const string gameOverKey = "HardcoreGameOver";

	public GameObject inputBlocker;

	public GameObject message;

	public TextMeshProUGUI text;

	public uGUI_NavigableControlGrid mainGrid;

	protected override void Awake()
	{
		inputBlocker.SetActive(value: false);
		base.Awake();
		message.SetActive(value: false);
	}

	protected override void Update()
	{
	}

	public override void OnSelect(bool lockMovement)
	{
		message.SetActive(value: true);
		text.text = Language.main.Get("HardcoreGameOver");
		FreezeTime.Begin(FreezeTime.Id.HardcoreGameOver);
		inputBlocker.SetActive(value: true);
		base.OnSelect(lockMovement);
		GamepadInputModule.current.SetCurrentGrid(mainGrid);
	}

	public override void OnDeselect()
	{
	}

	public void Show()
	{
		uGUI.main.overlays.gameObject.SetActive(value: false);
		uGUI_PlayerDeath.main.ResetOverlay();
		Select(lockMovement: true);
	}

	public void OnOkClick()
	{
		StartCoroutine(IngameMenu.QuitToMainMenuAsync());
		message.SetActive(value: false);
		FreezeTime.End(FreezeTime.Id.HardcoreGameOver);
	}
}
