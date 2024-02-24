using UWE;
using UnityEngine;

[RequireComponent(typeof(EnergyMixin))]
public class Welder : PlayerTool
{
	public FMODASRPlayer weldSound;

	public VFXController fxControl;

	public float weldEnergyCost = 1f;

	private float timeLastWelded;

	private float healthPerWeld = 10f;

	private bool usedThisFrame;

	private LiveMixin activeWeldTarget;

	private bool fxIsPlaying;

	private float timeTillLightbarUpdate;

	[AssertLocalization]
	private const string weldHandText = "Weld";

	public override void OnToolUseAnim(GUIHand hand)
	{
		Weld();
	}

	private void OnDisable()
	{
		activeWeldTarget = null;
	}

	private void Weld()
	{
		if (!(activeWeldTarget != null))
		{
			return;
		}
		EnergyMixin component = base.gameObject.GetComponent<EnergyMixin>();
		if (!component.IsDepleted() && activeWeldTarget.AddHealth(healthPerWeld) > 0f)
		{
			if (fxControl != null && !fxIsPlaying && MiscSettings.flashes)
			{
				int i = (Player.main.IsUnderwater() ? 1 : 0);
				fxControl.Play(i);
				fxIsPlaying = true;
			}
			timeLastWelded = Time.time;
			component.ConsumeEnergy(weldEnergyCost);
		}
	}

	private void UpdateTarget()
	{
		activeWeldTarget = null;
		if (!(usingPlayer != null))
		{
			return;
		}
		Vector3 position = default(Vector3);
		GameObject closestObj = null;
		UWE.Utils.TraceFPSTargetPosition(Player.main.gameObject, 2f, ref closestObj, ref position);
		if (closestObj == null)
		{
			InteractionVolumeUser component = Player.main.gameObject.GetComponent<InteractionVolumeUser>();
			if (component != null && component.GetMostRecent() != null)
			{
				closestObj = component.GetMostRecent().gameObject;
			}
		}
		if (!closestObj)
		{
			return;
		}
		LiveMixin liveMixin = closestObj.FindAncestor<LiveMixin>();
		if (!liveMixin)
		{
			return;
		}
		if (liveMixin.IsWeldable())
		{
			activeWeldTarget = liveMixin;
			return;
		}
		WeldablePoint weldablePoint = closestObj.FindAncestor<WeldablePoint>();
		if (weldablePoint != null && weldablePoint.transform.IsChildOf(liveMixin.transform))
		{
			activeWeldTarget = liveMixin;
		}
	}

	private void UpdateUI()
	{
		if (activeWeldTarget != null)
		{
			float healthFraction = activeWeldTarget.GetHealthFraction();
			if (healthFraction < 1f)
			{
				HandReticle main = HandReticle.main;
				main.SetProgress(healthFraction);
				main.SetText(HandReticle.TextType.Hand, "Weld", translate: true);
				main.SetIcon(HandReticle.IconType.Progress, 1.5f);
			}
		}
	}

	public override void OnHolster()
	{
		base.OnHolster();
		StopWeldingFX();
	}

	private void StopWeldingFX()
	{
		weldSound.Stop();
		if (fxControl != null)
		{
			fxControl.StopAndDestroy(0f);
			fxIsPlaying = false;
		}
	}

	private void Update()
	{
		usedThisFrame = false;
		if (base.isDrawn)
		{
			if (AvatarInputHandler.main.IsEnabled() && Player.main.IsAlive() && GameInput.GetButtonHeld(GameInput.Button.RightHand) && !Player.main.IsBleederAttached())
			{
				usedThisFrame = true;
			}
			if (usedThisFrame)
			{
				weldSound.Play();
			}
			else
			{
				StopWeldingFX();
			}
			UpdateTarget();
			UpdateUI();
		}
	}

	public override bool GetUsedToolThisFrame()
	{
		return usedThisFrame;
	}

	private void UpdateLightbar()
	{
		if (fxIsPlaying)
		{
			timeTillLightbarUpdate -= Time.deltaTime;
			if (timeTillLightbarUpdate < 0f)
			{
				PlatformUtils.SetLightbarColor(Random.ColorHSV(0.05f, 0.175f, 0.95f, 1f, 0.35f, 0.7f));
				timeTillLightbarUpdate = 1f / 15f + Random.Range(-0.013333335f, 0.013333335f);
			}
		}
	}
}
