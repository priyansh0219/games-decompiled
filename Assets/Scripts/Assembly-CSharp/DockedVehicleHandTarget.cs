using UnityEngine;

public class DockedVehicleHandTarget : CinematicModeTriggerBase
{
	[AssertNotNull]
	public VehicleDockingBay dockingBay;

	[AssertNotNull]
	public PlayerCinematicController seamothCinematicController;

	[AssertNotNull]
	public PlayerCinematicController exosuitCinematicController;

	[AssertLocalization]
	private const string enterExosuitKey = "EnterExosuit";

	[AssertLocalization]
	private const string enterSeamothKey = "EnterSeamoth";

	[AssertLocalization]
	private const string depthWarningKey = "DockedVehicleDepthWarning";

	[AssertLocalization(2)]
	private const string vehicleStatusFormatKey = "VehicleStatusFormat";

	[AssertLocalization(1)]
	private const string vehicleStatusChargedFormatKey = "VehicleStatusChargedFormat";

	[AssertLocalization]
	private const string noVehicleDockedKey = "NoVehicleDocked";

	[AssertLocalization]
	private const string insufficientUndockingClearanceKey = "InsufficientUndockingClearance";

	public void Start()
	{
		InvokeRepeating("UpdateValid", Random.value, 1f);
		base.isValidHandTarget = false;
	}

	protected override void OnStartCinematicMode()
	{
		dockingBay.OnUndockingStart();
	}

	public override void OnHandHover(GUIHand hand)
	{
		Vehicle dockedVehicle = dockingBay.GetDockedVehicle();
		if (dockedVehicle != null)
		{
			string text = ((dockedVehicle is Exosuit) ? "EnterExosuit" : "EnterSeamoth");
			if (!dockingBay.HasUndockingClearance())
			{
				HandReticle.main.SetText(HandReticle.TextType.Hand, "InsufficientUndockingClearance", translate: true);
				HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
				return;
			}
			CrushDamage crushDamage = dockedVehicle.crushDamage;
			if (crushDamage != null)
			{
				float crushDepth = crushDamage.crushDepth;
				if (Ocean.GetDepthOf(Player.main.gameObject) > crushDepth)
				{
					HandReticle.main.SetText(HandReticle.TextType.Hand, text, translate: true, GameInput.Button.LeftHand);
					HandReticle.main.SetText(HandReticle.TextType.HandSubscript, "DockedVehicleDepthWarning", translate: true);
					return;
				}
			}
			EnergyMixin component = dockedVehicle.GetComponent<EnergyMixin>();
			LiveMixin liveMixin = dockedVehicle.liveMixin;
			if (component.charge < component.capacity)
			{
				string format = Language.main.GetFormat("VehicleStatusFormat", liveMixin.GetHealthFraction(), component.GetEnergyScalar());
				HandReticle.main.SetText(HandReticle.TextType.Hand, text, translate: true, GameInput.Button.LeftHand);
				HandReticle.main.SetText(HandReticle.TextType.HandSubscript, format, translate: false);
			}
			else
			{
				string format2 = Language.main.GetFormat("VehicleStatusChargedFormat", liveMixin.GetHealthFraction());
				HandReticle.main.SetText(HandReticle.TextType.Hand, text, translate: true, GameInput.Button.LeftHand);
				HandReticle.main.SetText(HandReticle.TextType.HandSubscript, format2, translate: false);
			}
			HandReticle.main.SetIcon(HandReticle.IconType.Hand);
		}
		else
		{
			HandReticle.main.SetText(HandReticle.TextType.Hand, "NoVehicleDocked", translate: true);
			HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, translate: false);
		}
	}

	private void UpdateValid()
	{
		base.isValidHandTarget = dockingBay.GetDockedVehicle() != null;
	}

	private void OnPlayerCinematicModeStart(PlayerCinematicController cinematicController)
	{
		dockingBay.subRoot.BroadcastMessage("OnLaunchBayOpening", SendMessageOptions.DontRequireReceiver);
	}

	public new void OnPlayerCinematicModeEnd(PlayerCinematicController cinematicController)
	{
		dockingBay.OnUndockingComplete(cinematicController.GetPlayer());
	}

	public override void OnHandClick(GUIHand hand)
	{
		if (dockingBay.HasUndockingClearance())
		{
			if (dockingBay.GetDockedVehicle() != null)
			{
				cinematicController = ((dockingBay.GetDockedVehicle() is Exosuit) ? exosuitCinematicController : seamothCinematicController);
			}
			base.OnHandClick(hand);
		}
	}
}
