using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CyclopsHelmHUDManager : MonoBehaviour
{
	[AssertNotNull]
	public Image hpBar;

	[AssertNotNull]
	public Image noiseBar;

	[AssertNotNull]
	public Image fireWarningSprite;

	[AssertNotNull]
	public Image creatureAttackSprite;

	[AssertNotNull]
	public TextMeshProUGUI powerText;

	[AssertNotNull]
	public TextMeshProUGUI depthText;

	[AssertNotNull]
	public GameObject hornObject;

	[AssertNotNull]
	public CanvasGroup canvasGroup;

	[AssertNotNull]
	public GameObject sonarUpgrade;

	[AssertNotNull]
	public GameObject shieldUpgrade;

	[AssertNotNull]
	public FMOD_CustomEmitter creatureDamagesSFX;

	[AssertNotNull]
	public TextMeshProUGUI engineOffText;

	[AssertNotNull]
	public Animator engineToggleAnimator;

	[AssertNotNull]
	public SubRoot subRoot;

	[AssertNotNull]
	public LiveMixin subLiveMixin;

	[AssertNotNull]
	public CyclopsNoiseManager noiseManager;

	[AssertNotNull]
	public CyclopsMotorMode motorMode;

	[AssertNotNull]
	public CrushDamage crushDamage;

	[AssertNotNull]
	public BehaviourLOD LOD;

	public bool fireWarning;

	public bool creatureAttackWarning;

	public bool hullDamageWarning;

	private bool hudActive;

	private float warningAlpha;

	private bool oldWarningState;

	private int lastDepthUsedForString = -1;

	private int lastCrushDepthUsedForString = -1;

	private int lastPowerPctUsedForString = -1;

	private void Start()
	{
		subRoot.BroadcastMessage("NewAlarmState", null, SendMessageOptions.DontRequireReceiver);
		engineOffText.text = Language.main.Get("CyclopsEngineOffWarning");
	}

	private void Update()
	{
		if (!LOD.IsFull())
		{
			return;
		}
		if (subLiveMixin.IsAlive())
		{
			float healthFraction = subLiveMixin.GetHealthFraction();
			hpBar.fillAmount = Mathf.Lerp(hpBar.fillAmount, healthFraction, Time.deltaTime * 2f);
			float noisePercent = noiseManager.GetNoisePercent();
			noiseBar.fillAmount = Mathf.Lerp(noiseBar.fillAmount, noisePercent, Time.deltaTime);
			int num = Mathf.CeilToInt(subRoot.powerRelay.GetPower() / subRoot.powerRelay.GetMaxPower() * 100f);
			if (lastPowerPctUsedForString != num)
			{
				powerText.text = $"{num}%";
				lastPowerPctUsedForString = num;
			}
			int num2 = (int)crushDamage.GetDepth();
			int num3 = (int)crushDamage.crushDepth;
			Color color = Color.white;
			if (num2 >= num3)
			{
				color = Color.red;
			}
			if (lastDepthUsedForString != num2 || lastCrushDepthUsedForString != num3)
			{
				lastDepthUsedForString = num2;
				lastCrushDepthUsedForString = num3;
				depthText.text = $"{num2}m / {num3}m";
			}
			depthText.color = color;
			engineOffText.gameObject.SetActive(!motorMode.engineOn);
			fireWarningSprite.gameObject.SetActive(fireWarning ? true : false);
			creatureAttackSprite.gameObject.SetActive(creatureAttackWarning ? true : false);
			hullDamageWarning = ((subLiveMixin.GetHealthFraction() < 0.8f) ? true : false);
		}
		if (Player.main.currentSub == subRoot && !subRoot.subDestroyed)
		{
			if (fireWarning && creatureAttackWarning)
			{
				subRoot.voiceNotificationManager.PlayVoiceNotification(subRoot.creatureAttackNotification);
			}
			else if (creatureAttackWarning)
			{
				subRoot.voiceNotificationManager.PlayVoiceNotification(subRoot.creatureAttackNotification, subRoot);
			}
			else if (fireWarning)
			{
				subRoot.voiceNotificationManager.PlayVoiceNotification(subRoot.fireNotification);
			}
			else if (noiseManager.GetNoisePercent() > 0.9f && !IsInvoking("PlayCavitationWarningAfterSeconds"))
			{
				Invoke("PlayCavitationWarningAfterSeconds", 2f);
			}
			else if (hullDamageWarning)
			{
				subRoot.voiceNotificationManager.PlayVoiceNotification(subRoot.hullDamageNotification);
			}
			if (fireWarning || creatureAttackWarning)
			{
				subRoot.subWarning = true;
			}
			else
			{
				subRoot.subWarning = false;
			}
			warningAlpha = Mathf.PingPong(Time.time * 5f, 1f);
			fireWarningSprite.color = new Color(1f, 1f, 1f, warningAlpha);
			creatureAttackSprite.color = new Color(1f, 1f, 1f, warningAlpha);
			if (hudActive)
			{
				canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 1f, Time.deltaTime * 3f);
				canvasGroup.interactable = true;
			}
			else
			{
				canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0f, Time.deltaTime * 3f);
				canvasGroup.interactable = false;
			}
		}
		else
		{
			subRoot.subWarning = false;
		}
		if (oldWarningState != subRoot.subWarning)
		{
			subRoot.BroadcastMessage("NewAlarmState", null, SendMessageOptions.DontRequireReceiver);
		}
		oldWarningState = subRoot.subWarning;
	}

	private void PlayCavitationWarningAfterSeconds()
	{
		subRoot.voiceNotificationManager.PlayVoiceNotification(subRoot.cavitatingNotification);
	}

	private void ClearCreatureWarning()
	{
		creatureAttackWarning = false;
	}

	private void ClearFireWarning()
	{
		fireWarning = false;
	}

	public void UpdateAbilities()
	{
		sonarUpgrade.SetActive(subRoot.sonarUpgrade);
		shieldUpgrade.SetActive(subRoot.shieldUpgrade);
	}

	public void SetFireWarning(bool state)
	{
		fireWarning = state;
	}

	public void SetCreatureAttackWarning(bool state)
	{
		creatureAttackWarning = state;
	}

	public void StartPiloting()
	{
		hudActive = true;
		hornObject.SetActive(value: true);
		if (motorMode.engineOn)
		{
			engineToggleAnimator.SetTrigger("EngineOn");
		}
		else
		{
			engineToggleAnimator.SetTrigger("EngineOff");
		}
	}

	public void InvokeChangeEngineState(bool changeToState)
	{
		_ = motorMode.engineOn;
		if (changeToState)
		{
			engineToggleAnimator.SetTrigger("StartEngine");
		}
		else
		{
			engineToggleAnimator.SetTrigger("StopEngine");
		}
	}

	public void StopPiloting()
	{
		hudActive = false;
		hornObject.SetActive(value: false);
	}

	public void OnTakeFireDamage()
	{
		CancelInvoke("ClearFireWarning");
		Invoke("ClearFireWarning", 10f);
		fireWarning = true;
	}

	public void OnTakeCreatureDamage()
	{
		CancelInvoke("ClearCreatureWarning");
		Invoke("ClearCreatureWarning", 10f);
		creatureAttackWarning = true;
		creatureDamagesSFX.Play();
		MainCameraControl.main.ShakeCamera(1.5f);
	}

	public void OnTakeCollisionDamage(float value)
	{
		value *= 1.5f;
		value = Mathf.Clamp(value / 100f, 0.5f, 1.5f);
		MainCameraControl.main.ShakeCamera(value);
	}
}
