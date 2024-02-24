using TMPro;
using UWE;
using UnityEngine;

public class uGUI_PleaseWait : MonoBehaviour
{
	private const ManagedUpdate.Queue queue = ManagedUpdate.Queue.LateUpdate;

	[AssertLocalization]
	private const string pleaseWait = "PleaseWait";

	[AssertNotNull]
	public CanvasGroup canvasGroup;

	[AssertNotNull]
	public TextMeshProUGUI text;

	private void Awake()
	{
		canvasGroup.SetVisible(visible: false);
	}

	private void OnEnable()
	{
		ManagedUpdate.Subscribe(ManagedUpdate.Queue.LateUpdate, OnUpdate);
		OnLanguageChanged();
		Language.OnLanguageChanged += OnLanguageChanged;
	}

	private void OnDisable()
	{
		Language.OnLanguageChanged -= OnLanguageChanged;
		ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.LateUpdate, OnUpdate);
	}

	private void OnUpdate()
	{
		canvasGroup.SetVisible(FreezeTime.PleaseWait);
	}

	private void OnLanguageChanged()
	{
		text.SetText(Language.main.Get("PleaseWait"));
	}
}
