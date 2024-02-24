using UnityEngine;

public class uGUI_SceneHUD : uGUI_Scene
{
	[AssertNotNull]
	public GameObject backgroundBarsDouble;

	[AssertNotNull]
	public GameObject backgroundBarsQuad;

	[AssertNotNull]
	public GameObject barOxygen;

	[AssertNotNull]
	public GameObject barHealth;

	[AssertNotNull]
	public GameObject barFood;

	[AssertNotNull]
	public GameObject barWater;

	private bool _initialized;

	private bool _active;

	private int _mode;

	private bool _showOxygen;

	private bool _showHealth;

	private bool _showFood;

	private bool _showWater;

	public bool active => _active;

	private void Awake()
	{
		UpdateElements();
	}

	private void Update()
	{
		Initialize();
		if (_initialized)
		{
			bool flag = IsActive();
			if (_active != flag)
			{
				_active = flag;
				UpdateElements();
			}
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
			GameModeUtils.TriggerHandler(GameModeChanged);
			GameModeUtils.onGameModeChanged.AddHandler(this, GameModeChanged);
		}
	}

	private void Deinitialize()
	{
		if (_initialized)
		{
			_initialized = false;
			_active = false;
			GameModeUtils.onGameModeChanged.RemoveHandler(this, GameModeChanged);
			ResetHUD();
		}
	}

	private void ResetHUD()
	{
		_mode = 0;
		_showOxygen = false;
		_showHealth = false;
		_showFood = false;
		_showWater = false;
	}

	private void GameModeChanged(GameModeOption gameMode)
	{
		ResetHUD();
		if (GameModeUtils.IsOptionActive(gameMode, GameModeOption.NoSurvival))
		{
			if (GameModeUtils.IsOptionActive(gameMode, GameModeOption.NoOxygen) && GameModeUtils.IsOptionActive(gameMode, GameModeOption.NoAggression))
			{
				_mode = 0;
			}
			else
			{
				_mode = 1;
				_showOxygen = (_showHealth = true);
			}
		}
		else
		{
			_mode = 2;
			_showOxygen = (_showHealth = (_showFood = (_showWater = true)));
		}
		UpdateElements();
	}

	private void UpdateElements()
	{
		backgroundBarsDouble.SetActive(_active && _mode == 1);
		backgroundBarsQuad.SetActive(_active && _mode == 2);
		barOxygen.SetActive(_active && _showOxygen);
		barHealth.SetActive(_active && _showHealth);
		barFood.SetActive(_active && _showFood);
		barWater.SetActive(_active && _showWater);
	}

	private bool IsActive()
	{
		if (!uGUI.isMainLevel)
		{
			return false;
		}
		if (uGUI.isIntro)
		{
			return false;
		}
		if (LaunchRocket.isLaunching)
		{
			return false;
		}
		Player main = Player.main;
		if (main == null)
		{
			return false;
		}
		if (main.GetMode() == Player.Mode.Piloting || main.cinematicModeActive)
		{
			return false;
		}
		uGUI_CameraDrone main2 = uGUI_CameraDrone.main;
		if (main2 != null && main2.GetCamera() != null)
		{
			return false;
		}
		return true;
	}
}
