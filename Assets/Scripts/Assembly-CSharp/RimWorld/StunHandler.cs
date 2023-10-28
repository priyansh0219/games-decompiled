using UnityEngine;
using Verse;

namespace RimWorld
{
	public class StunHandler : IExposable
	{
		public Thing parent;

		private int stunTicksLeft;

		private Mote moteStun;

		private bool showStunMote = true;

		private int EMPAdaptedTicksLeft;

		private Effecter empEffecter;

		private bool stunFromEMP;

		public const float StunDurationTicksPerDamage = 30f;

		private const int MechBandShockwaveStunTicks = 1200;

		public bool Stunned => stunTicksLeft > 0;

		private int EMPAdaptationTicksDuration
		{
			get
			{
				if (parent is Pawn pawn && pawn.RaceProps.IsMechanoid)
				{
					return 2200;
				}
				return 0;
			}
		}

		private bool AffectedByEMP
		{
			get
			{
				if (parent is Pawn pawn)
				{
					return !pawn.RaceProps.IsFlesh;
				}
				return true;
			}
		}

		private bool AffectedByMechBandShockwave
		{
			get
			{
				if (parent is Pawn pawn)
				{
					return pawn.RaceProps.IsMechanoid;
				}
				return true;
			}
		}

		public int StunTicksLeft => stunTicksLeft;

		public StunHandler(Thing parent)
		{
			this.parent = parent;
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref stunTicksLeft, "stunTicksLeft", 0);
			Scribe_Values.Look(ref showStunMote, "showStunMote", defaultValue: false);
			Scribe_Values.Look(ref EMPAdaptedTicksLeft, "EMPAdaptedTicksLeft", 0);
			Scribe_Values.Look(ref stunFromEMP, "stunFromEMP", defaultValue: false);
		}

		public void StunHandlerTick()
		{
			if (EMPAdaptedTicksLeft > 0)
			{
				EMPAdaptedTicksLeft--;
			}
			if (stunTicksLeft > 0)
			{
				stunTicksLeft--;
				if (showStunMote && (moteStun == null || moteStun.Destroyed))
				{
					moteStun = MoteMaker.MakeStunOverlay(parent);
				}
				if (parent is Pawn pawn && pawn.Downed)
				{
					stunTicksLeft = 0;
				}
				if (moteStun != null)
				{
					moteStun.Maintain();
				}
				if (AffectedByEMP && stunFromEMP)
				{
					if (empEffecter == null)
					{
						empEffecter = EffecterDefOf.DisabledByEMP.Spawn();
					}
					empEffecter.EffectTick(parent, parent);
				}
			}
			else if (empEffecter != null)
			{
				empEffecter.Cleanup();
				empEffecter = null;
				stunFromEMP = false;
			}
		}

		public void Notify_DamageApplied(DamageInfo dinfo)
		{
			if (parent is Pawn pawn && (pawn.Downed || pawn.Dead))
			{
				return;
			}
			if (dinfo.Def == DamageDefOf.Stun)
			{
				StunFor(Mathf.RoundToInt(dinfo.Amount * 30f), dinfo.Instigator);
			}
			else if (dinfo.Def == DamageDefOf.EMP && AffectedByEMP)
			{
				if (EMPAdaptedTicksLeft <= 0)
				{
					float num = 1f;
					if (ModsConfig.BiotechActive)
					{
						num = Mathf.Clamp01(1f - parent.GetStatValue(StatDefOf.EMPResistance));
					}
					int num2 = Mathf.RoundToInt(dinfo.Amount * 30f * num);
					StunFor(Mathf.RoundToInt(num2), dinfo.Instigator);
					EMPAdaptedTicksLeft = EMPAdaptationTicksDuration;
					stunFromEMP = true;
				}
				else
				{
					MoteMaker.ThrowText(new Vector3((float)parent.Position.x + 1f, parent.Position.y, (float)parent.Position.z + 1f), parent.Map, "Adapted".Translate(), Color.white);
				}
			}
			else if (ModsConfig.BiotechActive && dinfo.Def == DamageDefOf.MechBandShockwave && AffectedByMechBandShockwave)
			{
				StunFor(Mathf.RoundToInt(1200f), dinfo.Instigator);
			}
		}

		public void StunFor(int ticks, Thing instigator, bool addBattleLog = true, bool showMote = true)
		{
			stunTicksLeft = Mathf.Max(stunTicksLeft, ticks);
			showStunMote = showMote;
			if (addBattleLog)
			{
				Find.BattleLog.Add(new BattleLogEntry_Event(parent, RulePackDefOf.Event_Stun, instigator));
			}
		}
	}
}
