using UnityEngine;

public class HintSwimToSurface : MonoBehaviour
{
	public float oxygenThreshold = 10f;

	public int maxNumToShow = 3;

	private bool initialized;

	private int numShown;

	private string message;

	private bool show;

	[AssertLocalization]
	private const string swimToSurfaceMessage = "SwimToSurface";

	private void OnDisable()
	{
		Deinitialize();
	}

	private void Update()
	{
		Initialize();
		Track();
	}

	private void Initialize()
	{
		if (!initialized)
		{
			initialized = true;
			OnLanguageChanged();
			Language.OnLanguageChanged += OnLanguageChanged;
		}
	}

	private void Deinitialize()
	{
		if (initialized)
		{
			initialized = false;
			numShown = 0;
			Language.OnLanguageChanged -= OnLanguageChanged;
		}
	}

	private void OnLanguageChanged()
	{
		message = Language.main.Get("SwimToSurface");
	}

	private void Track()
	{
		if (!initialized)
		{
			return;
		}
		Hint main = Hint.main;
		if (main == null)
		{
			return;
		}
		bool num = show;
		bool flag = IsVisible();
		show = ShouldShowWarning() && flag;
		uGUI_PopupMessage uGUI_PopupMessage2 = main.message;
		if (show)
		{
			uGUI_PopupMessage2.anchor = TextAnchor.UpperCenter;
			if (!uGUI_PopupMessage2.isShowingMessage || uGUI_PopupMessage2.showingMessage != message)
			{
				uGUI_PopupMessage2.SetText(message, TextAnchor.MiddleLeft);
				uGUI_PopupMessage2.Show(-1f);
			}
		}
		else if (uGUI_PopupMessage2.isShowingMessage && uGUI_PopupMessage2.showingMessage == message)
		{
			uGUI_PopupMessage2.Hide();
		}
		if (num && !show && flag)
		{
			numShown++;
		}
	}

	private bool ShouldShowWarning()
	{
		Player main = Player.main;
		if (main == null)
		{
			return false;
		}
		if (GameModeUtils.IsOptionActive(GameModeOption.NoHints))
		{
			return false;
		}
		if (numShown >= maxNumToShow)
		{
			return false;
		}
		float oxygenAvailable = main.GetOxygenAvailable();
		float depthOf = Ocean.GetDepthOf(main.gameObject);
		Vehicle vehicle = main.GetVehicle();
		if (oxygenAvailable < oxygenThreshold && depthOf > 0f && main.IsSwimming() && vehicle == null)
		{
			return true;
		}
		return false;
	}

	private bool IsVisible()
	{
		if (MiscSettings.pdaPause)
		{
			Player main = Player.main;
			PDA pDA = ((main != null) ? main.GetPDA() : null);
			if (pDA != null && pDA.isInUse)
			{
				return false;
			}
		}
		return true;
	}
}
