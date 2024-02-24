using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class uGUI_RocketBuildScreen : MonoBehaviour
{
	[AssertNotNull]
	public Sprite[] rocketStageImages;

	[AssertNotNull]
	public GameObject root;

	[AssertNotNull]
	public GameObject buildScreen;

	[AssertNotNull]
	public GameObject customizeScreen;

	[AssertNotNull]
	public GameObject buildAnimationScreen;

	[AssertNotNull]
	public RocketBuilderTooltip rocketTooltip;

	[AssertNotNull]
	public PlayerDistanceTracker playerDistanceTracker;

	[AssertNotNull]
	public Image buildNextIcon;

	[AssertNotNull]
	public TextMeshProUGUI buildNextText;

	[AssertNotNull]
	public TextMeshProUGUI constructButtonText;

	[AssertLocalization]
	private const string constructButtonKey = "ConstructButton";

	private void Start()
	{
		string text = Language.main.Get("ConstructButton");
		constructButtonText.text = text;
	}

	public void SetBuildingAnimationScreen()
	{
		buildScreen.SetActive(value: false);
		customizeScreen.SetActive(value: false);
		buildAnimationScreen.SetActive(value: true);
	}

	public void AdvanceToStage(int stage)
	{
		stage = Mathf.Clamp(stage, 0, 5);
		if (stage == 5)
		{
			buildScreen.SetActive(value: false);
			customizeScreen.SetActive(value: true);
			buildAnimationScreen.SetActive(value: false);
			return;
		}
		buildScreen.SetActive(value: true);
		customizeScreen.SetActive(value: false);
		buildAnimationScreen.SetActive(value: false);
		buildNextIcon.sprite = rocketStageImages[Mathf.Clamp(stage - 1, 0, 3)];
		string key = ((stage == 0) ? Rocket.RocketStages.RocketBaseLadder : ((Rocket.RocketStages)stage)).ToString();
		string text = Language.main.Get(key);
		buildNextText.text = text;
		rocketTooltip.SetTooltipTech(stage);
	}

	public void ContructButton()
	{
		root.BroadcastMessage("StartRocketConstruction");
	}

	private void OnEnable()
	{
		CancelInvoke();
		InvokeRepeating("SetTooltipStateBasedOnDistance", 0f, 0.5f);
	}

	private void OnDisable()
	{
		CancelInvoke("SetTooltipStateBasedOnDistance");
	}

	private void SetTooltipStateBasedOnDistance()
	{
		bool active = playerDistanceTracker.distanceToPlayer < 5f;
		rocketTooltip.gameObject.SetActive(active);
	}
}
