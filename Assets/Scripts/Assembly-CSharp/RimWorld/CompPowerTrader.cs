using System;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class CompPowerTrader : CompPower
	{
		public Action powerStartedAction;

		public Action powerStoppedAction;

		private bool powerOnInt;

		public float powerOutputInt;

		private bool powerLastOutputted;

		private Sustainer sustainerPowered;

		protected CompFlickable flickableComp;

		public const string PowerTurnedOnSignal = "PowerTurnedOn";

		public const string PowerTurnedOffSignal = "PowerTurnedOff";

		private OverlayHandle? overlayPowerOff;

		private OverlayHandle? overlayNeedsPower;

		public float PowerOutput
		{
			get
			{
				return powerOutputInt;
			}
			set
			{
				powerOutputInt = value;
				if (powerOutputInt > 0f)
				{
					powerLastOutputted = true;
				}
				if (powerOutputInt < 0f)
				{
					powerLastOutputted = false;
				}
			}
		}

		public float EnergyOutputPerTick => PowerOutput * CompPower.WattsToWattDaysPerTick;

		public bool PowerOn
		{
			get
			{
				return powerOnInt;
			}
			set
			{
				if (powerOnInt == value)
				{
					return;
				}
				powerOnInt = value;
				if (powerOnInt)
				{
					if (!FlickUtility.WantsToBeOn(parent))
					{
						Log.Warning(string.Concat("Tried to power on ", parent, " which did not desire it."));
						return;
					}
					if (parent.IsBrokenDown())
					{
						Log.Warning(string.Concat("Tried to power on ", parent, " which is broken down."));
						return;
					}
					if (powerStartedAction != null)
					{
						powerStartedAction();
					}
					parent.BroadcastCompSignal("PowerTurnedOn");
					SoundDef soundDef = ((CompProperties_Power)parent.def.CompDefForAssignableFrom<CompPowerTrader>()).soundPowerOn;
					if (soundDef.NullOrUndefined())
					{
						soundDef = SoundDefOf.Power_OnSmall;
					}
					soundDef.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
					StartSustainerPoweredIfInactive();
				}
				else
				{
					if (powerStoppedAction != null)
					{
						powerStoppedAction();
					}
					parent.BroadcastCompSignal("PowerTurnedOff");
					SoundDef soundDef2 = ((CompProperties_Power)parent.def.CompDefForAssignableFrom<CompPowerTrader>()).soundPowerOff;
					if (soundDef2.NullOrUndefined())
					{
						soundDef2 = SoundDefOf.Power_OffSmall;
					}
					if (parent.Spawned)
					{
						soundDef2.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
					}
					EndSustainerPoweredIfActive();
				}
				UpdateOverlays();
			}
		}

		public string DebugString
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine(parent.LabelCap + " CompPower:");
				stringBuilder.AppendLine("   PowerOn: " + PowerOn);
				stringBuilder.AppendLine("   energyProduction: " + PowerOutput);
				return stringBuilder.ToString();
			}
		}

		public override void ReceiveCompSignal(string signal)
		{
			switch (signal)
			{
			case "FlickedOff":
			case "ScheduledOff":
			case "Breakdown":
			case "AutoPoweredWantsOff":
				PowerOn = false;
				break;
			}
			if (signal == "RanOutOfFuel" && powerLastOutputted)
			{
				PowerOn = false;
			}
			UpdateOverlays();
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			flickableComp = parent.GetComp<CompFlickable>();
			if (PowerOn)
			{
				LongEventHandler.ExecuteWhenFinished(delegate
				{
					StartSustainerPoweredIfInactive();
				});
			}
			UpdateOverlays();
		}

		public override void PostDeSpawn(Map map)
		{
			base.PostDeSpawn(map);
			EndSustainerPoweredIfActive();
			powerOutputInt = 0f;
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref powerOnInt, "powerOn", defaultValue: true);
		}

		private void UpdateOverlays()
		{
			if (!parent.Spawned)
			{
				return;
			}
			parent.Map.overlayDrawer.Disable(parent, ref overlayPowerOff);
			parent.Map.overlayDrawer.Disable(parent, ref overlayNeedsPower);
			if (!parent.IsBrokenDown())
			{
				if (flickableComp != null && !flickableComp.SwitchIsOn && !overlayPowerOff.HasValue)
				{
					overlayPowerOff = parent.Map.overlayDrawer.Enable(parent, OverlayTypes.PowerOff);
				}
				else if (FlickUtility.WantsToBeOn(parent) && !PowerOn && !overlayNeedsPower.HasValue && base.Props.showPowerNeededIfOff)
				{
					overlayNeedsPower = parent.Map.overlayDrawer.Enable(parent, OverlayTypes.NeedsPower);
				}
			}
		}

		public override void SetUpPowerVars()
		{
			base.SetUpPowerVars();
			CompProperties_Power compProperties_Power = base.Props;
			if (!PowerOn && !Mathf.Approximately(compProperties_Power.idlePowerDraw, -1f))
			{
				PowerOutput = 0f - compProperties_Power.idlePowerDraw;
			}
			else
			{
				PowerOutput = 0f - compProperties_Power.PowerConsumption;
			}
			powerLastOutputted = compProperties_Power.PowerConsumption <= 0f;
		}

		public override void ResetPowerVars()
		{
			base.ResetPowerVars();
			powerOnInt = false;
			powerOutputInt = 0f;
			powerLastOutputted = false;
			sustainerPowered = null;
			if (flickableComp != null)
			{
				flickableComp.ResetToOn();
			}
		}

		public override void LostConnectParent()
		{
			base.LostConnectParent();
			PowerOn = false;
		}

		public override string CompInspectStringExtra()
		{
			string text;
			if (powerLastOutputted && !base.Props.alwaysDisplayAsUsingPower)
			{
				text = "PowerOutput".Translate() + ": " + PowerOutput.ToString("#####0") + " W";
			}
			else if (base.Props.idlePowerDraw >= 0f && Mathf.Approximately(PowerOutput, 0f - base.Props.idlePowerDraw))
			{
				text = "PowerNeeded".Translate() + ": " + base.Props.idlePowerDraw.ToString("#####0") + " W";
				text += " (" + "PowerActiveNeeded".Translate(base.Props.PowerConsumption.ToString("#####0")) + ")";
			}
			else
			{
				text = "PowerNeeded".Translate() + ": " + base.Props.PowerConsumption.ToString("#####0") + " W";
			}
			return text + "\n" + base.CompInspectStringExtra();
		}

		private void StartSustainerPoweredIfInactive()
		{
			CompProperties_Power compProperties_Power = base.Props;
			if (!compProperties_Power.soundAmbientPowered.NullOrUndefined() && sustainerPowered == null)
			{
				SoundInfo info = SoundInfo.InMap(parent);
				sustainerPowered = compProperties_Power.soundAmbientPowered.TrySpawnSustainer(info);
			}
		}

		private void EndSustainerPoweredIfActive()
		{
			if (sustainerPowered != null)
			{
				sustainerPowered.End();
				sustainerPowered = null;
			}
		}
	}
}
