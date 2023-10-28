using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class CompTurretGun : ThingComp, IAttackTargetSearcher
	{
		private const int StartShootIntervalTicks = 10;

		private static readonly CachedTexture ToggleTurretIcon = new CachedTexture("UI/Gizmos/ToggleTurret");

		public Thing gun;

		protected int burstCooldownTicksLeft;

		protected int burstWarmupTicksLeft;

		protected LocalTargetInfo currentTarget = LocalTargetInfo.Invalid;

		private bool fireAtWill = true;

		private LocalTargetInfo lastAttackedTarget = LocalTargetInfo.Invalid;

		private int lastAttackTargetTick;

		private float curRotation;

		[Unsaved(false)]
		public Material turretMat;

		public Thing Thing => parent;

		private CompProperties_TurretGun Props => (CompProperties_TurretGun)props;

		public Verb CurrentEffectiveVerb => AttackVerb;

		public LocalTargetInfo LastAttackedTarget => lastAttackedTarget;

		public int LastAttackTargetTick => lastAttackTargetTick;

		public CompEquippable GunCompEq => gun.TryGetComp<CompEquippable>();

		public Verb AttackVerb => GunCompEq.PrimaryVerb;

		private bool WarmingUp => burstWarmupTicksLeft > 0;

		private bool CanShoot
		{
			get
			{
				if (parent is Pawn pawn)
				{
					if (!pawn.Spawned || pawn.Downed || pawn.Dead || !pawn.Awake())
					{
						return false;
					}
					if (pawn.stances.stunner.Stunned)
					{
						return false;
					}
					if (TurretDestroyed)
					{
						return false;
					}
					if (pawn.IsColonyMechPlayerControlled && !fireAtWill)
					{
						return false;
					}
				}
				CompCanBeDormant compCanBeDormant = parent.TryGetComp<CompCanBeDormant>();
				if (compCanBeDormant != null && !compCanBeDormant.Awake)
				{
					return false;
				}
				return true;
			}
		}

		public bool TurretDestroyed
		{
			get
			{
				if (parent is Pawn pawn && AttackVerb.verbProps.linkedBodyPartsGroup != null && AttackVerb.verbProps.ensureLinkedBodyPartsGroupAlwaysUsable && PawnCapacityUtility.CalculateNaturalPartsAverageEfficiency(pawn.health.hediffSet, AttackVerb.verbProps.linkedBodyPartsGroup) <= 0f)
				{
					return true;
				}
				return false;
			}
		}

		private Material TurretMat
		{
			get
			{
				if (turretMat == null)
				{
					turretMat = MaterialPool.MatFrom(Props.turretDef.graphicData.texPath);
				}
				return turretMat;
			}
		}

		public bool AutoAttack => Props.autoAttack;

		public override void PostPostMake()
		{
			base.PostPostMake();
			MakeGun();
		}

		private void MakeGun()
		{
			gun = ThingMaker.MakeThing(Props.turretDef);
			UpdateGunVerbs();
		}

		private void UpdateGunVerbs()
		{
			List<Verb> allVerbs = gun.TryGetComp<CompEquippable>().AllVerbs;
			for (int i = 0; i < allVerbs.Count; i++)
			{
				Verb verb = allVerbs[i];
				verb.caster = parent;
				verb.castCompleteCallback = delegate
				{
					burstCooldownTicksLeft = AttackVerb.verbProps.defaultCooldownTime.SecondsToTicks();
				};
			}
		}

		public override void CompTick()
		{
			base.CompTick();
			if (!CanShoot)
			{
				return;
			}
			if (currentTarget.IsValid)
			{
				curRotation = (currentTarget.Cell.ToVector3Shifted() - parent.DrawPos).AngleFlat() + Props.angleOffset;
			}
			AttackVerb.VerbTick();
			if (AttackVerb.state == VerbState.Bursting)
			{
				return;
			}
			if (WarmingUp)
			{
				burstWarmupTicksLeft--;
				if (burstWarmupTicksLeft == 0)
				{
					AttackVerb.TryStartCastOn(currentTarget, surpriseAttack: false, canHitNonTargetPawns: true, preventFriendlyFire: false, nonInterruptingSelfCast: true);
					lastAttackTargetTick = Find.TickManager.TicksGame;
					lastAttackedTarget = currentTarget;
				}
				return;
			}
			if (burstCooldownTicksLeft > 0)
			{
				burstCooldownTicksLeft--;
			}
			if (burstCooldownTicksLeft <= 0 && parent.IsHashIntervalTick(10))
			{
				currentTarget = (Thing)AttackTargetFinder.BestShootTargetFromCurrentPosition(this, TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable);
				if (currentTarget.IsValid)
				{
					burstWarmupTicksLeft = 1;
				}
				else
				{
					ResetCurrentTarget();
				}
			}
		}

		private void ResetCurrentTarget()
		{
			currentTarget = LocalTargetInfo.Invalid;
			burstWarmupTicksLeft = 0;
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			foreach (Gizmo item in base.CompGetGizmosExtra())
			{
				yield return item;
			}
			if (parent is Pawn pawn && pawn.IsColonyMechPlayerControlled)
			{
				Command_Toggle command_Toggle = new Command_Toggle();
				command_Toggle.defaultLabel = "CommandToggleTurret".Translate();
				command_Toggle.defaultDesc = "CommandToggleTurretDesc".Translate();
				command_Toggle.isActive = () => fireAtWill;
				command_Toggle.icon = ToggleTurretIcon.Texture;
				command_Toggle.toggleAction = delegate
				{
					fireAtWill = !fireAtWill;
				};
				yield return command_Toggle;
			}
		}

		public override void PostDraw()
		{
			base.PostDraw();
			Rot4 rotation = parent.Rotation;
			Vector3 vector = new Vector3(0f, (rotation == Rot4.North) ? (3f / 74f) : (-3f / 74f), 0f);
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(parent.DrawPos + vector, curRotation.ToQuat(), Vector3.one);
			Graphics.DrawMesh(MeshPool.plane10, matrix, TurretMat, 0);
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref burstCooldownTicksLeft, "burstCooldownTicksLeft", 0);
			Scribe_Values.Look(ref burstWarmupTicksLeft, "burstWarmupTicksLeft", 0);
			Scribe_TargetInfo.Look(ref currentTarget, "currentTarget");
			Scribe_Deep.Look(ref gun, "gun");
			Scribe_Values.Look(ref fireAtWill, "fireAtWill", defaultValue: true);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (gun == null)
				{
					Log.Error("CompTurrentGun had null gun after loading. Recreating.");
					MakeGun();
				}
				else
				{
					UpdateGunVerbs();
				}
			}
		}
	}
}
