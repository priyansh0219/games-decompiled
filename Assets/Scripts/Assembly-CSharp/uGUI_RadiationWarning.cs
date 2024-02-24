using TMPro;
using UnityEngine;

public class uGUI_RadiationWarning : MonoBehaviour
{
	[AssertNotNull]
	public GameObject warning;

	[AssertNotNull]
	public TextMeshProUGUI text;

	private bool _initialized;

	private void Update()
	{
		Initialize();
		bool flag = IsRadiated();
		if (warning.activeSelf != flag)
		{
			warning.SetActive(flag);
		}
	}

	private void OnDisable()
	{
		Deinitialize();
	}

	private void Initialize()
	{
		if (!_initialized)
		{
			_initialized = true;
			OnLanguageChanged();
			Language.OnLanguageChanged += OnLanguageChanged;
		}
	}

	private void Deinitialize()
	{
		if (_initialized)
		{
			_initialized = false;
			Language.OnLanguageChanged -= OnLanguageChanged;
		}
	}

	private void OnLanguageChanged()
	{
		text.text = Language.main.Get("RadiationDetected");
	}

	private bool IsRadiated()
	{
		if (!_initialized)
		{
			return false;
		}
		Player main = Player.main;
		if (main == null)
		{
			return false;
		}
		PDA pDA = main.GetPDA();
		if (pDA != null && pDA.isInUse)
		{
			return false;
		}
		return main.radiationAmount > 0f;
	}
}
