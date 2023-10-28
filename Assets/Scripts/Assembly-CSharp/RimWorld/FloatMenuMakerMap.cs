using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public static class FloatMenuMakerMap
	{
		public static Pawn makingFor;

		private static List<Pawn> tmpPawns = new List<Pawn>();

		private static FloatMenuOption[] equivalenceGroupTempStorage;

		private static bool CanTakeOrder(Pawn pawn)
		{
			if (!pawn.IsColonistPlayerControlled)
			{
				return pawn.IsColonyMech;
			}
			return true;
		}

		public static void TryMakeFloatMenu(Pawn pawn)
		{
			if (!CanTakeOrder(pawn))
			{
				return;
			}
			if (pawn.Downed)
			{
				Messages.Message("IsIncapped".Translate(pawn.LabelCap, pawn), pawn, MessageTypeDefOf.RejectInput, historical: false);
			}
			else if (ModsConfig.BiotechActive && pawn.Deathresting)
			{
				Messages.Message("IsDeathresting".Translate(pawn.Named("PAWN")), pawn, MessageTypeDefOf.RejectInput, historical: false);
			}
			else
			{
				if (pawn.Map != Find.CurrentMap)
				{
					return;
				}
				Lord lord = pawn.GetLord();
				if (lord != null && lord.LordJob is LordJob_Ritual lordJob_Ritual)
				{
					Messages.Message("ParticipatingInRitual".Translate(pawn, lordJob_Ritual.RitualLabel), pawn, MessageTypeDefOf.RejectInput, historical: false);
					return;
				}
				List<FloatMenuOption> list = ChoicesAtFor(UI.MouseMapPosition(), pawn);
				if (list.Count == 0)
				{
					return;
				}
				bool flag = true;
				FloatMenuOption floatMenuOption = null;
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].Disabled || !list[i].autoTakeable)
					{
						flag = false;
						break;
					}
					if (floatMenuOption == null || list[i].autoTakeablePriority > floatMenuOption.autoTakeablePriority)
					{
						floatMenuOption = list[i];
					}
				}
				if (flag && floatMenuOption != null)
				{
					floatMenuOption.Chosen(colonistOrdering: true, null);
					return;
				}
				FloatMenuMap floatMenuMap = new FloatMenuMap(list, pawn.LabelCap, UI.MouseMapPosition());
				floatMenuMap.givesColonistOrders = true;
				Find.WindowStack.Add(floatMenuMap);
			}
		}

		public static void TryMakeFloatMenu_NonPawn(Thing selectedThing)
		{
			if (selectedThing.Map != Find.CurrentMap)
			{
				return;
			}
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			IntVec3 c = IntVec3.FromVector3(UI.MouseMapPosition());
			if (!c.InBounds(Find.CurrentMap))
			{
				return;
			}
			foreach (Thing item in selectedThing.Map.thingGrid.ThingsAt(c))
			{
				if (item == selectedThing)
				{
					continue;
				}
				foreach (FloatMenuOption item2 in item.GetFloatMenuOptions_NonPawn(selectedThing))
				{
					list.Add(item2);
				}
			}
			if (list.Any())
			{
				Find.WindowStack.Add(new FloatMenu(list));
			}
		}

		public static bool TryMakeMultiSelectFloatMenu(List<Pawn> pawns)
		{
			tmpPawns.Clear();
			tmpPawns.AddRange(pawns);
			tmpPawns.RemoveAll(InvalidPawnForMultiSelectOption);
			if (!tmpPawns.Any())
			{
				return false;
			}
			List<FloatMenuOption> list = ChoicesAtForMultiSelect(UI.MouseMapPosition(), tmpPawns);
			if (!list.Any())
			{
				tmpPawns.Clear();
				return false;
			}
			FloatMenu window = new FloatMenu(list)
			{
				givesColonistOrders = true
			};
			Find.WindowStack.Add(window);
			tmpPawns.Clear();
			return true;
		}

		public static bool InvalidPawnForMultiSelectOption(Pawn x)
		{
			Lord lord = x.GetLord();
			if (CanTakeOrder(x) && !x.Downed && x.Map == Find.CurrentMap)
			{
				if (lord != null)
				{
					return lord.LordJob is LordJob_Ritual;
				}
				return false;
			}
			return true;
		}

		public static List<FloatMenuOption> ChoicesAtFor(Vector3 clickPos, Pawn pawn, bool suppressAutoTakeableGoto = false)
		{
			IntVec3 intVec = IntVec3.FromVector3(clickPos);
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			Lord lord = pawn.GetLord();
			if (!intVec.InBounds(pawn.Map) || !CanTakeOrder(pawn) || (lord != null && lord.LordJob is LordJob_Ritual))
			{
				return list;
			}
			if (pawn.Map != Find.CurrentMap)
			{
				return list;
			}
			makingFor = pawn;
			try
			{
				if (intVec.Fogged(pawn.Map))
				{
					if (pawn.Drafted)
					{
						FloatMenuOption floatMenuOption = GotoLocationOption(intVec, pawn, suppressAutoTakeableGoto);
						if (floatMenuOption != null && !floatMenuOption.Disabled)
						{
							list.Add(floatMenuOption);
						}
					}
				}
				else
				{
					if (pawn.Drafted)
					{
						AddDraftedOrders(clickPos, pawn, list, suppressAutoTakeableGoto);
					}
					if (pawn.RaceProps.Humanlike)
					{
						AddHumanlikeOrders(clickPos, pawn, list);
					}
					if (!pawn.Drafted && (!pawn.RaceProps.IsMechanoid || DebugSettings.allowUndraftedMechOrders))
					{
						AddUndraftedOrders(clickPos, pawn, list);
					}
					foreach (FloatMenuOption item in pawn.GetExtraFloatMenuOptionsFor(intVec))
					{
						list.Add(item);
					}
				}
			}
			finally
			{
				makingFor = null;
			}
			return list;
		}

		public static List<FloatMenuOption> ChoicesAtForMultiSelect(Vector3 clickPos, List<Pawn> pawns)
		{
			IntVec3 c = IntVec3.FromVector3(clickPos);
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			Map map = pawns[0].Map;
			if (!c.InBounds(map) || map != Find.CurrentMap)
			{
				return list;
			}
			foreach (Thing item in map.thingGrid.ThingsAt(c))
			{
				foreach (FloatMenuOption multiSelectFloatMenuOption in item.GetMultiSelectFloatMenuOptions(pawns))
				{
					list.Add(multiSelectFloatMenuOption);
				}
			}
			return list;
		}

		private static void AddDraftedOrders(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts, bool suppressAutoTakeableGoto = false)
		{
			IntVec3 clickCell = IntVec3.FromVector3(clickPos);
			foreach (LocalTargetInfo item5 in GenUI.TargetsAt(clickPos, TargetingParameters.ForAttackHostile(), thingsOnly: true))
			{
				LocalTargetInfo attackTarg = item5;
				if (ModsConfig.BiotechActive && pawn.IsColonyMech && !MechanitorUtility.InMechanitorCommandRange(pawn, attackTarg))
				{
					continue;
				}
				if (pawn.equipment.Primary != null && !pawn.equipment.PrimaryEq.PrimaryVerb.verbProps.IsMeleeAttack)
				{
					string failStr;
					Action rangedAct = FloatMenuUtility.GetRangedAttackAction(pawn, attackTarg, out failStr);
					string text = "FireAt".Translate(attackTarg.Thing.Label, attackTarg.Thing);
					MenuOptionPriority priority = ((!attackTarg.HasThing || !pawn.HostileTo(attackTarg.Thing)) ? MenuOptionPriority.VeryLow : MenuOptionPriority.AttackEnemy);
					FloatMenuOption floatMenuOption = new FloatMenuOption("", null, priority, null, item5.Thing);
					if (rangedAct == null)
					{
						text = text + ": " + failStr;
					}
					else
					{
						floatMenuOption.autoTakeable = !attackTarg.HasThing || attackTarg.Thing.HostileTo(Faction.OfPlayer);
						floatMenuOption.autoTakeablePriority = 40f;
						floatMenuOption.action = delegate
						{
							FleckMaker.Static(attackTarg.Thing.DrawPos, attackTarg.Thing.Map, FleckDefOf.FeedbackShoot);
							rangedAct();
						};
					}
					floatMenuOption.Label = text;
					opts.Add(floatMenuOption);
				}
				string failStr2;
				Action meleeAct = FloatMenuUtility.GetMeleeAttackAction(pawn, attackTarg, out failStr2);
				string text2 = ((!(attackTarg.Thing is Pawn pawn2) || !pawn2.Downed) ? ((string)"MeleeAttack".Translate(attackTarg.Thing.Label, attackTarg.Thing)) : ((string)"MeleeAttackToDeath".Translate(attackTarg.Thing.Label, attackTarg.Thing)));
				MenuOptionPriority priority2 = ((!attackTarg.HasThing || !pawn.HostileTo(attackTarg.Thing)) ? MenuOptionPriority.VeryLow : MenuOptionPriority.AttackEnemy);
				FloatMenuOption floatMenuOption2 = new FloatMenuOption("", null, priority2, null, attackTarg.Thing);
				if (meleeAct == null)
				{
					text2 = text2 + ": " + failStr2.CapitalizeFirst();
				}
				else
				{
					floatMenuOption2.autoTakeable = !attackTarg.HasThing || attackTarg.Thing.HostileTo(Faction.OfPlayer);
					floatMenuOption2.autoTakeablePriority = 30f;
					floatMenuOption2.action = delegate
					{
						FleckMaker.Static(attackTarg.Thing.DrawPos, attackTarg.Thing.Map, FleckDefOf.FeedbackMelee);
						meleeAct();
					};
				}
				floatMenuOption2.Label = text2;
				opts.Add(floatMenuOption2);
			}
			if (!pawn.RaceProps.IsMechanoid)
			{
				if (pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
				{
					foreach (LocalTargetInfo item6 in GenUI.TargetsAt(clickPos, TargetingParameters.ForCarry(pawn), thingsOnly: true))
					{
						LocalTargetInfo carryTarget = item6;
						FloatMenuOption item = (pawn.CanReach(carryTarget, PathEndMode.ClosestTouch, Danger.Deadly) ? FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Carry".Translate(carryTarget.Thing), delegate
						{
							carryTarget.Thing.SetForbidden(value: false, warnOnFail: false);
							Job job8 = JobMaker.MakeJob(JobDefOf.CarryDownedPawnDrafted, carryTarget);
							job8.count = 1;
							pawn.jobs.TryTakeOrderedJob(job8, JobTag.Misc);
						}), pawn, carryTarget) : new FloatMenuOption("CannotCarry".Translate(carryTarget.Thing) + ": " + "NoPath".Translate().CapitalizeFirst(), null));
						opts.Add(item);
					}
				}
				if (pawn.IsCarryingPawn())
				{
					Pawn carriedPawn = (Pawn)pawn.carryTracker.CarriedThing;
					if (!carriedPawn.IsPrisonerOfColony)
					{
						foreach (LocalTargetInfo item7 in GenUI.TargetsAt(clickPos, TargetingParameters.ForDraftedCarryBed(carriedPawn, pawn, carriedPawn.GuestStatus), thingsOnly: true))
						{
							LocalTargetInfo destTarget2 = item7;
							FloatMenuOption item2 = (pawn.CanReach(destTarget2, PathEndMode.ClosestTouch, Danger.Deadly) ? FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("PlaceIn".Translate(carriedPawn, destTarget2.Thing), delegate
							{
								destTarget2.Thing.SetForbidden(value: false, warnOnFail: false);
								Job job7 = JobMaker.MakeJob(JobDefOf.TakeDownedPawnToBedDrafted, pawn.carryTracker.CarriedThing, destTarget2);
								job7.count = 1;
								pawn.jobs.TryTakeOrderedJob(job7, JobTag.Misc);
							}), pawn, destTarget2) : new FloatMenuOption("CannotPlaceIn".Translate(carriedPawn, destTarget2.Thing) + ": " + "NoPath".Translate().CapitalizeFirst(), null));
							opts.Add(item2);
						}
					}
					foreach (LocalTargetInfo item8 in GenUI.TargetsAt(clickPos, TargetingParameters.ForDraftedCarryBed(carriedPawn, pawn, GuestStatus.Prisoner), thingsOnly: true))
					{
						LocalTargetInfo destTarget = item8;
						FloatMenuOption item3;
						if (!pawn.CanReach(destTarget, PathEndMode.ClosestTouch, Danger.Deadly))
						{
							item3 = new FloatMenuOption("CannotPlaceIn".Translate(carriedPawn, destTarget.Thing) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
						}
						else
						{
							TaggedString taggedString = "PlaceIn".Translate(carriedPawn, destTarget.Thing);
							if (!carriedPawn.IsPrisonerOfColony)
							{
								taggedString += ": " + "ArrestChance".Translate(carriedPawn.GetAcceptArrestChance(pawn).ToStringPercent());
							}
							item3 = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(taggedString, delegate
							{
								destTarget.Thing.SetForbidden(value: false, warnOnFail: false);
								Job job6 = JobMaker.MakeJob(JobDefOf.CarryToPrisonerBedDrafted, pawn.carryTracker.CarriedThing, destTarget);
								job6.count = 1;
								pawn.jobs.TryTakeOrderedJob(job6, JobTag.Misc);
							}), pawn, destTarget);
						}
						opts.Add(item3);
					}
					foreach (LocalTargetInfo item9 in GenUI.TargetsAt(clickPos, TargetingParameters.ForDraftedCarryTransporter(carriedPawn), thingsOnly: true))
					{
						Thing transporterThing = item9.Thing;
						if (transporterThing == null)
						{
							continue;
						}
						CompTransporter compTransporter = transporterThing.TryGetComp<CompTransporter>();
						if (compTransporter.Shuttle != null && !compTransporter.Shuttle.IsAllowedNow(carriedPawn))
						{
							continue;
						}
						if (!pawn.CanReach(transporterThing, PathEndMode.ClosestTouch, Danger.Deadly))
						{
							opts.Add(new FloatMenuOption("CannotPlaceIn".Translate(carriedPawn, transporterThing) + ": " + "NoPath".Translate().CapitalizeFirst(), null));
							continue;
						}
						if (compTransporter.Shuttle == null && !compTransporter.LeftToLoadContains(carriedPawn))
						{
							opts.Add(new FloatMenuOption("CannotPlaceIn".Translate(carriedPawn, transporterThing) + ": " + "NotPartOfLaunchGroup".Translate(), null));
							continue;
						}
						string label = "PlaceIn".Translate(carriedPawn, transporterThing);
						Action action = delegate
						{
							if (!compTransporter.LoadingInProgressOrReadyToLaunch)
							{
								TransporterUtility.InitiateLoading(Gen.YieldSingle(compTransporter));
							}
							Job job5 = JobMaker.MakeJob(JobDefOf.HaulToTransporter, carriedPawn, transporterThing);
							job5.ignoreForbidden = true;
							job5.count = 1;
							pawn.jobs.TryTakeOrderedJob(job5, JobTag.Misc);
						};
						opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action), pawn, transporterThing));
					}
					foreach (LocalTargetInfo item10 in GenUI.TargetsAt(clickPos, TargetingParameters.ForDraftedCarryCryptosleepCasket(pawn), thingsOnly: true))
					{
						Thing casket = item10.Thing;
						TaggedString taggedString2 = "PlaceIn".Translate(carriedPawn, casket);
						if (((Building_CryptosleepCasket)casket).HasAnyContents)
						{
							opts.Add(new FloatMenuOption("CannotPlaceIn".Translate(carriedPawn, casket) + ": " + "CryptosleepCasketOccupied".Translate(), null));
							continue;
						}
						if (carriedPawn.IsQuestLodger())
						{
							opts.Add(new FloatMenuOption("CannotPlaceIn".Translate(carriedPawn, casket) + ": " + "CryptosleepCasketGuestsNotAllowed".Translate(), null));
							continue;
						}
						if (carriedPawn.GetExtraHostFaction() != null)
						{
							opts.Add(new FloatMenuOption("CannotPlaceIn".Translate(carriedPawn, casket) + ": " + "CryptosleepCasketGuestPrisonersNotAllowed".Translate(), null));
							continue;
						}
						Action action2 = delegate
						{
							Job job4 = JobMaker.MakeJob(JobDefOf.CarryToCryptosleepCasketDrafted, carriedPawn, casket);
							job4.count = 1;
							job4.playerForced = true;
							pawn.jobs.TryTakeOrderedJob(job4, JobTag.Misc);
						};
						opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(taggedString2, action2), pawn, casket));
					}
				}
				if (pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
				{
					foreach (LocalTargetInfo item11 in GenUI.TargetsAt(clickPos, TargetingParameters.ForTend(pawn), thingsOnly: true))
					{
						Pawn tendTarget = (Pawn)item11.Thing;
						if (!tendTarget.health.HasHediffsNeedingTend())
						{
							opts.Add(new FloatMenuOption("CannotTend".Translate(tendTarget) + ": " + "TendingNotRequired".Translate(tendTarget), null));
							continue;
						}
						if (pawn.WorkTypeIsDisabled(WorkTypeDefOf.Doctor))
						{
							opts.Add(new FloatMenuOption("CannotTend".Translate(tendTarget) + ": " + "CannotPrioritizeWorkTypeDisabled".Translate(WorkTypeDefOf.Doctor.gerundLabel), null));
							continue;
						}
						if (!pawn.CanReach(tendTarget, PathEndMode.ClosestTouch, Danger.Deadly))
						{
							opts.Add(new FloatMenuOption("CannotTend".Translate(tendTarget) + ": " + "NoPath".Translate().CapitalizeFirst(), null));
							continue;
						}
						Thing medicine = HealthAIUtility.FindBestMedicine(pawn, tendTarget, onlyUseInventory: true);
						TaggedString taggedString3 = "Tend".Translate(tendTarget);
						Action action3 = delegate
						{
							Job job3 = JobMaker.MakeJob(JobDefOf.TendPatient, tendTarget, medicine);
							job3.count = 1;
							job3.draftedTend = true;
							pawn.jobs.TryTakeOrderedJob(job3, JobTag.Misc);
						};
						if (tendTarget == pawn && pawn.playerSettings != null && !pawn.playerSettings.selfTend)
						{
							action3 = null;
							taggedString3 = "CannotGenericWorkCustom".Translate("Tend".Translate(tendTarget).ToString().UncapitalizeFirst()) + ": " + "SelfTendDisabled".Translate().CapitalizeFirst();
						}
						else if (tendTarget.InAggroMentalState && !tendTarget.health.hediffSet.HasHediff(HediffDefOf.Scaria))
						{
							action3 = null;
							taggedString3 = "CannotGenericWorkCustom".Translate("Tend".Translate(tendTarget).ToString().UncapitalizeFirst()) + ": " + "PawnIsInMentalState".Translate(tendTarget).CapitalizeFirst();
						}
						else if (medicine == null)
						{
							taggedString3 += " (" + "WithoutMedicine".Translate() + ")";
						}
						opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(taggedString3, action3), pawn, tendTarget));
						if (medicine != null && action3 != null && pawn.CanReserve(tendTarget))
						{
							opts.Add(new FloatMenuOption("Tend".Translate(tendTarget) + " (" + "WithoutMedicine".Translate() + ")", delegate
							{
								Job job2 = JobMaker.MakeJob(JobDefOf.TendPatient, tendTarget, null);
								job2.count = 1;
								job2.draftedTend = true;
								pawn.jobs.TryTakeOrderedJob(job2, JobTag.Misc);
							}));
						}
					}
					if (pawn.skills != null && !pawn.skills.GetSkill(SkillDefOf.Construction).TotallyDisabled)
					{
						foreach (LocalTargetInfo item12 in GenUI.TargetsAt(clickPos, TargetingParameters.ForRepair(pawn), thingsOnly: true))
						{
							Thing repairTarget = item12.Thing;
							if (!pawn.CanReach(repairTarget, PathEndMode.Touch, Danger.Deadly))
							{
								opts.Add(new FloatMenuOption("CannotRepair".Translate(repairTarget) + ": " + "NoPath".Translate().CapitalizeFirst(), null));
							}
							else if (RepairUtility.PawnCanRepairNow(pawn, repairTarget))
							{
								FloatMenuOption item4 = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("RepairThing".Translate(repairTarget), delegate
								{
									Job job = JobMaker.MakeJob(JobDefOf.Repair, repairTarget);
									pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
								}), pawn, repairTarget);
								opts.Add(item4);
							}
						}
					}
				}
			}
			AddJobGiverWorkOrders(clickPos, pawn, opts, drafted: true);
			FloatMenuOption floatMenuOption3 = GotoLocationOption(clickCell, pawn, suppressAutoTakeableGoto);
			if (floatMenuOption3 != null)
			{
				opts.Add(floatMenuOption3);
			}
		}

		private static void AddHumanlikeOrders(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
		{
			IntVec3 c = IntVec3.FromVector3(clickPos);
			foreach (Thing thing7 in c.GetThingList(pawn.Map))
			{
				if (!(thing7 is Pawn pawn2))
				{
					continue;
				}
				Lord lord = pawn2.GetLord();
				if (lord == null || lord.CurLordToil == null)
				{
					continue;
				}
				IEnumerable<FloatMenuOption> enumerable = lord.CurLordToil.ExtraFloatMenuOptions(pawn2, pawn);
				if (enumerable == null)
				{
					continue;
				}
				foreach (FloatMenuOption item10 in enumerable)
				{
					opts.Add(item10);
				}
			}
			if (pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
			{
				foreach (LocalTargetInfo item11 in GenUI.TargetsAt(clickPos, TargetingParameters.ForArrest(pawn), thingsOnly: true))
				{
					bool flag = item11.HasThing && item11.Thing is Pawn && ((Pawn)item11.Thing).IsWildMan();
					if (!pawn.Drafted && !flag)
					{
						continue;
					}
					if (item11.Thing is Pawn && (pawn.InSameExtraFaction((Pawn)item11.Thing, ExtraFactionType.HomeFaction) || pawn.InSameExtraFaction((Pawn)item11.Thing, ExtraFactionType.MiniFaction)))
					{
						opts.Add(new FloatMenuOption("CannotArrest".Translate() + ": " + "SameFaction".Translate((Pawn)item11.Thing), null));
						continue;
					}
					if (!pawn.CanReach(item11, PathEndMode.OnCell, Danger.Deadly))
					{
						opts.Add(new FloatMenuOption("CannotArrest".Translate() + ": " + "NoPath".Translate().CapitalizeFirst(), null));
						continue;
					}
					Pawn pTarg2 = (Pawn)item11.Thing;
					Action action = delegate
					{
						Building_Bed building_Bed4 = RestUtility.FindBedFor(pTarg2, pawn, checkSocialProperness: false, ignoreOtherReservations: false, GuestStatus.Prisoner);
						if (building_Bed4 == null)
						{
							building_Bed4 = RestUtility.FindBedFor(pTarg2, pawn, checkSocialProperness: false, ignoreOtherReservations: true, GuestStatus.Prisoner);
						}
						if (building_Bed4 == null)
						{
							Messages.Message("CannotArrest".Translate() + ": " + "NoPrisonerBed".Translate(), pTarg2, MessageTypeDefOf.RejectInput, historical: false);
						}
						else
						{
							Job job31 = JobMaker.MakeJob(JobDefOf.Arrest, pTarg2, building_Bed4);
							job31.count = 1;
							pawn.jobs.TryTakeOrderedJob(job31, JobTag.Misc);
							if (pTarg2.Faction != null && ((pTarg2.Faction != Faction.OfPlayer && !pTarg2.Faction.Hidden) || pTarg2.IsQuestLodger()))
							{
								TutorUtility.DoModalDialogIfNotKnown(ConceptDefOf.ArrestingCreatesEnemies, pTarg2.GetAcceptArrestChance(pawn).ToStringPercent());
							}
						}
					};
					opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("TryToArrest".Translate(item11.Thing.LabelCap, item11.Thing, pTarg2.GetAcceptArrestChance(pawn).ToStringPercent()), action, MenuOptionPriority.High, null, item11.Thing), pawn, pTarg2));
				}
			}
			foreach (Thing thing8 in c.GetThingList(pawn.Map))
			{
				Thing t2 = thing8;
				if (t2.def.ingestible == null || !pawn.RaceProps.CanEverEat(t2) || !t2.IngestibleNow)
				{
					continue;
				}
				string text = ((!t2.def.ingestible.ingestCommandString.NullOrEmpty()) ? ((string)t2.def.ingestible.ingestCommandString.Formatted(t2.LabelShort)) : ((string)"ConsumeThing".Translate(t2.LabelShort, t2)));
				if (!t2.IsSociallyProper(pawn))
				{
					text = text + ": " + "ReservedForPrisoners".Translate().CapitalizeFirst();
				}
				else if (FoodUtility.MoodFromIngesting(pawn, t2, t2.def) < 0f)
				{
					text = string.Format("{0}: ({1})", text, "WarningFoodDisliked".Translate());
				}
				if ((!t2.def.IsDrug || !ModsConfig.IdeologyActive || new HistoryEvent(HistoryEventDefOf.IngestedDrug, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo(out var opt, text) || PawnUtility.CanTakeDrugForDependency(pawn, t2.def)) && (!t2.def.IsNonMedicalDrug || !ModsConfig.IdeologyActive || new HistoryEvent(HistoryEventDefOf.IngestedRecreationalDrug, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo(out opt, text) || PawnUtility.CanTakeDrugForDependency(pawn, t2.def)) && (!t2.def.IsDrug || !ModsConfig.IdeologyActive || t2.def.ingestible.drugCategory != DrugCategory.Hard || new HistoryEvent(HistoryEventDefOf.IngestedHardDrug, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo(out opt, text)))
				{
					if (t2.def.IsNonMedicalDrug && !pawn.CanTakeDrug(t2.def))
					{
						opt = new FloatMenuOption(text + ": " + TraitDefOf.DrugDesire.DataAtDegree(-1).GetLabelCapFor(pawn), null);
					}
					else if (FoodUtility.InappropriateForTitle(t2.def, pawn, allowIfStarving: true))
					{
						opt = new FloatMenuOption(text + ": " + "FoodBelowTitleRequirements".Translate(pawn.royalty.MostSeniorTitle.def.GetLabelFor(pawn).CapitalizeFirst()).CapitalizeFirst(), null);
					}
					else if (!pawn.CanReach(t2, PathEndMode.OnCell, Danger.Deadly))
					{
						opt = new FloatMenuOption(text + ": " + "NoPath".Translate().CapitalizeFirst(), null);
					}
					else
					{
						MenuOptionPriority priority = ((t2 is Corpse) ? MenuOptionPriority.Low : MenuOptionPriority.Default);
						int maxAmountToPickup = FoodUtility.GetMaxAmountToPickup(t2, pawn, FoodUtility.WillIngestStackCountOf(pawn, t2.def, FoodUtility.NutritionForEater(pawn, t2)));
						opt = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, delegate
						{
							int maxAmountToPickup2 = FoodUtility.GetMaxAmountToPickup(t2, pawn, FoodUtility.WillIngestStackCountOf(pawn, t2.def, FoodUtility.NutritionForEater(pawn, t2)));
							if (maxAmountToPickup2 != 0)
							{
								t2.SetForbidden(value: false);
								Job job30 = JobMaker.MakeJob(JobDefOf.Ingest, t2);
								job30.count = maxAmountToPickup2;
								pawn.jobs.TryTakeOrderedJob(job30, JobTag.Misc);
							}
						}, priority), pawn, t2);
						if (maxAmountToPickup == 0)
						{
							opt.action = null;
						}
					}
				}
				opts.Add(opt);
			}
			foreach (LocalTargetInfo item12 in GenUI.TargetsAt(clickPos, TargetingParameters.ForQuestPawnsWhoWillJoinColony(pawn), thingsOnly: true))
			{
				Pawn toHelpPawn = (Pawn)item12.Thing;
				FloatMenuOption item4 = (pawn.CanReach(item12, PathEndMode.Touch, Danger.Deadly) ? FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(toHelpPawn.IsPrisoner ? "FreePrisoner".Translate() : "OfferHelp".Translate(), delegate
				{
					pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.OfferHelp, toHelpPawn), JobTag.Misc);
				}, MenuOptionPriority.RescueOrCapture, null, toHelpPawn), pawn, toHelpPawn) : new FloatMenuOption("CannotGoNoPath".Translate(), null));
				opts.Add(item4);
			}
			ChildcareUtility.BreastfeedFailReason? reason;
			if (pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
			{
				List<Thing> thingList = c.GetThingList(pawn.Map);
				foreach (Thing item13 in thingList)
				{
					Corpse corpse2 = item13 as Corpse;
					if (corpse2 == null || !corpse2.IsInValidStorage())
					{
						continue;
					}
					StoragePriority priority2 = StoreUtility.CurrentHaulDestinationOf(corpse2).GetStoreSettings().Priority;
					if (StoreUtility.TryFindBestBetterNonSlotGroupStorageFor(corpse2, pawn, pawn.Map, priority2, Faction.OfPlayer, out var haulDestination, acceptSamePriority: true) && haulDestination.GetStoreSettings().Priority == priority2 && haulDestination is Building_Grave)
					{
						Building_Grave grave = haulDestination as Building_Grave;
						opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("PrioritizeGeneric".Translate("Burying".Translate(), corpse2.Label).CapitalizeFirst(), delegate
						{
							pawn.jobs.TryTakeOrderedJob(HaulAIUtility.HaulToContainerJob(pawn, corpse2, grave), JobTag.Misc);
						}), pawn, new LocalTargetInfo(corpse2)));
					}
				}
				foreach (Thing item14 in thingList)
				{
					Corpse corpse = item14 as Corpse;
					if (corpse == null)
					{
						continue;
					}
					Building_GibbetCage cage = Building_GibbetCage.FindGibbetCageFor(corpse, pawn);
					if (cage != null)
					{
						opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("PlaceIn".Translate(corpse, cage), delegate
						{
							pawn.jobs.TryTakeOrderedJob(HaulAIUtility.HaulToContainerJob(pawn, corpse, cage), JobTag.Misc);
						}), pawn, new LocalTargetInfo(corpse)));
					}
					if (ModsConfig.BiotechActive && corpse.InnerPawn.health.hediffSet.HasHediff(HediffDefOf.MechlinkImplant))
					{
						opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Extract".Translate() + " " + HediffDefOf.MechlinkImplant.label, delegate
						{
							Job job29 = JobMaker.MakeJob(JobDefOf.RemoveMechlink, corpse);
							pawn.jobs.TryTakeOrderedJob(job29, JobTag.Misc);
						}), pawn, new LocalTargetInfo(corpse)));
					}
				}
				foreach (LocalTargetInfo item15 in GenUI.TargetsAt(clickPos, TargetingParameters.ForRescue(pawn), thingsOnly: true))
				{
					Pawn victim3 = (Pawn)item15.Thing;
					if (victim3.InBed() || !pawn.CanReserveAndReach(victim3, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, ignoreOtherReservations: true) || victim3.mindState.WillJoinColonyIfRescued)
					{
						continue;
					}
					if (!victim3.IsPrisonerOfColony && !victim3.IsSlaveOfColony && (!victim3.InMentalState || victim3.health.hediffSet.HasHediff(HediffDefOf.Scaria)) && !victim3.IsColonyMech)
					{
						bool isBaby = ChildcareUtility.CanSuckle(victim3, out reason);
						if (victim3.Faction == Faction.OfPlayer || victim3.Faction == null || !victim3.Faction.HostileTo(Faction.OfPlayer) || isBaby)
						{
							TaggedString taggedString = ((HealthAIUtility.ShouldSeekMedicalRest(victim3) || !victim3.ageTracker.CurLifeStage.alwaysDowned) ? "Rescue".Translate(victim3.LabelCap, victim3) : "PutSomewhereSafe".Translate(victim3.LabelCap, victim3));
							opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(taggedString, delegate
							{
								if (isBaby)
								{
									pawn.jobs.TryTakeOrderedJob(ChildcareUtility.MakeBringBabyToSafetyJob(pawn, victim3), JobTag.Misc);
								}
								else
								{
									Building_Bed building_Bed3 = RestUtility.FindBedFor(victim3, pawn, checkSocialProperness: false);
									if (building_Bed3 == null)
									{
										building_Bed3 = RestUtility.FindBedFor(victim3, pawn, checkSocialProperness: false, ignoreOtherReservations: true);
									}
									if (building_Bed3 == null)
									{
										string text7 = ((!victim3.RaceProps.Animal) ? ((string)"NoNonPrisonerBed".Translate()) : ((string)"NoAnimalBed".Translate()));
										Messages.Message("CannotRescue".Translate() + ": " + text7, victim3, MessageTypeDefOf.RejectInput, historical: false);
									}
									else
									{
										Job job28 = JobMaker.MakeJob(JobDefOf.Rescue, victim3, building_Bed3);
										job28.count = 1;
										pawn.jobs.TryTakeOrderedJob(job28, JobTag.Misc);
										PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Rescuing, KnowledgeAmount.Total);
									}
								}
							}, MenuOptionPriority.RescueOrCapture, null, victim3), pawn, victim3));
						}
					}
					if (victim3.IsSlaveOfColony && !victim3.InMentalState)
					{
						opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("ReturnToSlaveBed".Translate(), delegate
						{
							Building_Bed building_Bed2 = RestUtility.FindBedFor(victim3, pawn, checkSocialProperness: false, ignoreOtherReservations: false, GuestStatus.Slave);
							if (building_Bed2 == null)
							{
								building_Bed2 = RestUtility.FindBedFor(victim3, pawn, checkSocialProperness: false, ignoreOtherReservations: true, GuestStatus.Slave);
							}
							if (building_Bed2 == null)
							{
								Messages.Message("CannotRescue".Translate() + ": " + "NoSlaveBed".Translate(), victim3, MessageTypeDefOf.RejectInput, historical: false);
							}
							else
							{
								Job job27 = JobMaker.MakeJob(JobDefOf.Rescue, victim3, building_Bed2);
								job27.count = 1;
								pawn.jobs.TryTakeOrderedJob(job27, JobTag.Misc);
								PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Rescuing, KnowledgeAmount.Total);
							}
						}, MenuOptionPriority.RescueOrCapture, null, victim3), pawn, victim3));
					}
					if (!victim3.RaceProps.Humanlike)
					{
						continue;
					}
					LifeStageDef curLifeStage = victim3.ageTracker.CurLifeStage;
					if ((curLifeStage != null && curLifeStage.claimable) || (!victim3.InMentalState && victim3.Faction == Faction.OfPlayer && (!victim3.Downed || (!victim3.guilt.IsGuilty && !victim3.IsPrisonerOfColony))))
					{
						continue;
					}
					TaggedString taggedString2 = "Capture".Translate(victim3.LabelCap, victim3);
					if (!victim3.guest.Recruitable)
					{
						taggedString2 += " (" + "Unrecruitable".Translate() + ")";
					}
					if (victim3.Faction != null && victim3.Faction != Faction.OfPlayer && !victim3.Faction.Hidden && !victim3.Faction.HostileTo(Faction.OfPlayer) && !victim3.IsPrisonerOfColony)
					{
						taggedString2 += ": " + "AngersFaction".Translate().CapitalizeFirst();
					}
					opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(taggedString2, delegate
					{
						Building_Bed building_Bed = RestUtility.FindBedFor(victim3, pawn, checkSocialProperness: false, ignoreOtherReservations: false, GuestStatus.Prisoner);
						if (building_Bed == null)
						{
							building_Bed = RestUtility.FindBedFor(victim3, pawn, checkSocialProperness: false, ignoreOtherReservations: true, GuestStatus.Prisoner);
						}
						if (building_Bed == null)
						{
							Messages.Message("CannotCapture".Translate() + ": " + "NoPrisonerBed".Translate(), victim3, MessageTypeDefOf.RejectInput, historical: false);
						}
						else
						{
							Job job26 = JobMaker.MakeJob(JobDefOf.Capture, victim3, building_Bed);
							job26.count = 1;
							pawn.jobs.TryTakeOrderedJob(job26, JobTag.Misc);
							PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Capturing, KnowledgeAmount.Total);
							if (victim3.Faction != null && victim3.Faction != Faction.OfPlayer && !victim3.Faction.Hidden && !victim3.Faction.HostileTo(Faction.OfPlayer) && !victim3.IsPrisonerOfColony)
							{
								Messages.Message("MessageCapturingWillAngerFaction".Translate(victim3.Named("PAWN")).AdjustedFor(victim3), victim3, MessageTypeDefOf.CautionInput, historical: false);
							}
						}
					}, MenuOptionPriority.RescueOrCapture, null, victim3), pawn, victim3));
				}
				foreach (LocalTargetInfo item16 in GenUI.TargetsAt(clickPos, TargetingParameters.ForRescue(pawn), thingsOnly: true))
				{
					LocalTargetInfo localTargetInfo = item16;
					Pawn victim2 = (Pawn)localTargetInfo.Thing;
					if (!victim2.Downed || !pawn.CanReserveAndReach(victim2, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, ignoreOtherReservations: true) || Building_CryptosleepCasket.FindCryptosleepCasketFor(victim2, pawn, ignoreOtherReservations: true) == null)
					{
						continue;
					}
					string text2 = "CarryToCryptosleepCasket".Translate(localTargetInfo.Thing.LabelCap, localTargetInfo.Thing);
					JobDef jDef = JobDefOf.CarryToCryptosleepCasket;
					Action action2 = delegate
					{
						Building_CryptosleepCasket building_CryptosleepCasket = Building_CryptosleepCasket.FindCryptosleepCasketFor(victim2, pawn);
						if (building_CryptosleepCasket == null)
						{
							building_CryptosleepCasket = Building_CryptosleepCasket.FindCryptosleepCasketFor(victim2, pawn, ignoreOtherReservations: true);
						}
						if (building_CryptosleepCasket == null)
						{
							Messages.Message("CannotCarryToCryptosleepCasket".Translate() + ": " + "NoCryptosleepCasket".Translate(), victim2, MessageTypeDefOf.RejectInput, historical: false);
						}
						else
						{
							Job job25 = JobMaker.MakeJob(jDef, victim2, building_CryptosleepCasket);
							job25.count = 1;
							pawn.jobs.TryTakeOrderedJob(job25, JobTag.Misc);
						}
					};
					if (victim2.IsQuestLodger())
					{
						text2 += " (" + "CryptosleepCasketGuestsNotAllowed".Translate() + ")";
						opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text2, null, MenuOptionPriority.Default, null, victim2), pawn, victim2));
					}
					else if (victim2.GetExtraHostFaction() != null)
					{
						text2 += " (" + "CryptosleepCasketGuestPrisonersNotAllowed".Translate() + ")";
						opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text2, null, MenuOptionPriority.Default, null, victim2), pawn, victim2));
					}
					else
					{
						opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text2, action2, MenuOptionPriority.Default, null, victim2), pawn, victim2));
					}
				}
				if (ModsConfig.IdeologyActive)
				{
					foreach (LocalTargetInfo item17 in GenUI.TargetsAt(clickPos, TargetingParameters.ForCarryToBiosculpterPod(pawn), thingsOnly: true))
					{
						Pawn pawn3 = (Pawn)item17.Thing;
						if ((pawn3.IsColonist && pawn3.Downed) || pawn3.IsPrisonerOfColony)
						{
							CompBiosculpterPod.AddCarryToPodJobs(opts, pawn, pawn3);
						}
					}
				}
				if (ModsConfig.RoyaltyActive)
				{
					foreach (LocalTargetInfo item18 in GenUI.TargetsAt(clickPos, TargetingParameters.ForShuttle(pawn), thingsOnly: true))
					{
						LocalTargetInfo localTargetInfo2 = item18;
						Pawn victim = (Pawn)localTargetInfo2.Thing;
						if (!victim.Spawned)
						{
							continue;
						}
						Predicate<Thing> validator = (Thing thing) => thing.TryGetComp<CompShuttle>()?.IsAllowedNow(victim) ?? false;
						Thing shuttleThing = GenClosest.ClosestThingReachable(victim.Position, victim.Map, ThingRequest.ForDef(ThingDefOf.Shuttle), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, validator);
						if (shuttleThing == null || !pawn.CanReserveAndReach(victim, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, ignoreOtherReservations: true) || pawn.WorkTypeIsDisabled(WorkTypeDefOf.Hauling))
						{
							continue;
						}
						string label = "CarryToShuttle".Translate(localTargetInfo2.Thing);
						Action action3 = delegate
						{
							CompShuttle compShuttle = shuttleThing.TryGetComp<CompShuttle>();
							if (!compShuttle.LoadingInProgressOrReadyToLaunch)
							{
								TransporterUtility.InitiateLoading(Gen.YieldSingle(compShuttle.Transporter));
							}
							Job job24 = JobMaker.MakeJob(JobDefOf.HaulToTransporter, victim, shuttleThing);
							job24.ignoreForbidden = true;
							job24.count = 1;
							pawn.jobs.TryTakeOrderedJob(job24, JobTag.Misc);
						};
						opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action3), pawn, victim));
					}
				}
				if (ModsConfig.IdeologyActive)
				{
					foreach (Thing thing5 in thingList)
					{
						CompHackable compHackable = thing5.TryGetComp<CompHackable>();
						if (compHackable == null)
						{
							continue;
						}
						if (compHackable.IsHacked)
						{
							opts.Add(new FloatMenuOption("CannotHack".Translate(thing5.Label) + ": " + "AlreadyHacked".Translate(), null));
						}
						else if (!HackUtility.IsCapableOfHacking(pawn))
						{
							opts.Add(new FloatMenuOption("CannotHack".Translate(thing5.Label) + ": " + "IncapableOfHacking".Translate(), null));
						}
						else if (!pawn.CanReach(thing5, PathEndMode.ClosestTouch, Danger.Deadly))
						{
							opts.Add(new FloatMenuOption("CannotHack".Translate(thing5.Label) + ": " + "NoPath".Translate().CapitalizeFirst(), null));
						}
						else if (thing5.def == ThingDefOf.AncientEnemyTerminal)
						{
							opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Hack".Translate(thing5.Label), delegate
							{
								Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmHackEnenyTerminal".Translate(ThingDefOf.AncientEnemyTerminal.label), delegate
								{
									pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Hack, thing5), JobTag.Misc);
								}));
							}), pawn, new LocalTargetInfo(thing5)));
						}
						else
						{
							TaggedString taggedString3 = ((thing5.def == ThingDefOf.AncientCommsConsole) ? "Hack".Translate("ToDropSupplies".Translate()) : "Hack".Translate(thing5.Label));
							opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(taggedString3, delegate
							{
								pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Hack, thing5), JobTag.Misc);
							}), pawn, new LocalTargetInfo(thing5)));
						}
					}
					foreach (LocalTargetInfo thing4 in GenUI.TargetsAt(clickPos, TargetingParameters.ForBuilding(ThingDefOf.ArchonexusCore)))
					{
						if (!pawn.CanReach(thing4, PathEndMode.InteractionCell, Danger.Deadly))
						{
							opts.Add(new FloatMenuOption("CannotInvoke".Translate("Power".Translate()) + ": " + "NoPath".Translate().CapitalizeFirst(), null));
							continue;
						}
						if (!((Building_ArchonexusCore)(Thing)thing4).CanActivateNow)
						{
							opts.Add(new FloatMenuOption("CannotInvoke".Translate("Power".Translate()) + ": " + "AlreadyInvoked".Translate(), null));
							continue;
						}
						opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Invoke".Translate("Power".Translate()), delegate
						{
							pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.ActivateArchonexusCore, thing4), JobTag.Misc);
						}), pawn, thing4));
					}
				}
				if (ModsConfig.IdeologyActive)
				{
					foreach (Thing thing3 in thingList)
					{
						CompRelicContainer container = thing3.TryGetComp<CompRelicContainer>();
						if (container == null)
						{
							continue;
						}
						if (container.Full)
						{
							string text3 = "ExtractRelic".Translate(container.ContainedThing.Label);
							if (!StoreUtility.TryFindBestBetterStorageFor(container.ContainedThing, pawn, pawn.Map, StoragePriority.Unstored, pawn.Faction, out var foundCell, out var _))
							{
								opts.Add(new FloatMenuOption(text3 + " (" + HaulAIUtility.NoEmptyPlaceLowerTrans + ")", null));
							}
							else
							{
								Job job3 = JobMaker.MakeJob(JobDefOf.ExtractRelic, thing3, container.ContainedThing, foundCell);
								job3.count = 1;
								opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text3, delegate
								{
									pawn.jobs.TryTakeOrderedJob(job3, JobTag.Misc);
								}), pawn, new LocalTargetInfo(thing3)));
							}
						}
						else
						{
							IEnumerable<Thing> enumerable2 = pawn.Map.listerThings.AllThings.Where((Thing x) => CompRelicContainer.IsRelic(x) && pawn.CanReach(x, PathEndMode.ClosestTouch, Danger.Deadly));
							if (!enumerable2.Any())
							{
								opts.Add(new FloatMenuOption("NoRelicToInstall".Translate(), null));
							}
							else
							{
								foreach (Thing item19 in enumerable2)
								{
									Job job2 = JobMaker.MakeJob(JobDefOf.InstallRelic, item19, thing3, thing3.InteractionCell);
									job2.count = 1;
									opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("InstallRelic".Translate(item19.Label), delegate
									{
										pawn.jobs.TryTakeOrderedJob(job2, JobTag.Misc);
									}), pawn, new LocalTargetInfo(thing3)));
								}
							}
						}
						if (!pawn.Map.IsPlayerHome && !pawn.IsFormingCaravan() && container.Full)
						{
							opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("ExtractRelicToInventory".Translate(container.ContainedThing.Label, 300.ToStringTicksToPeriod()), delegate
							{
								Job job23 = JobMaker.MakeJob(JobDefOf.ExtractToInventory, thing3, container.ContainedThing, thing3.InteractionCell);
								job23.count = 1;
								pawn.jobs.TryTakeOrderedJob(job23, JobTag.Misc);
							}), pawn, new LocalTargetInfo(thing3)));
						}
					}
					foreach (Thing item20 in thingList)
					{
						if (!CompRelicContainer.IsRelic(item20))
						{
							continue;
						}
						IEnumerable<Thing> searchSet = from x in item20.Map.listerThings.ThingsOfDef(ThingDefOf.Reliquary)
							where x.TryGetComp<CompRelicContainer>().ContainedThing == null
							select x;
						Thing thing6 = GenClosest.ClosestThing_Global_Reachable(item20.Position, item20.Map, searchSet, PathEndMode.Touch, TraverseParms.For(pawn), 9999f, (Thing t) => pawn.CanReserve(t));
						if (thing6 == null)
						{
							opts.Add(new FloatMenuOption("InstallInReliquary".Translate() + " (" + "NoEmptyReliquary".Translate() + ")", null));
							continue;
						}
						Job job = JobMaker.MakeJob(JobDefOf.InstallRelic, item20, thing6, thing6.InteractionCell);
						job.count = 1;
						opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("InstallInReliquary".Translate(), delegate
						{
							pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
						}), pawn, new LocalTargetInfo(item20)));
					}
				}
				if (ModsConfig.BiotechActive && MechanitorUtility.IsMechanitor(pawn))
				{
					foreach (Thing thing2 in thingList)
					{
						Pawn mech;
						if ((mech = thing2 as Pawn) == null || !mech.IsColonyMech)
						{
							continue;
						}
						if (mech.GetOverseer() != pawn)
						{
							if (!pawn.CanReach(mech, PathEndMode.Touch, Danger.Deadly))
							{
								opts.Add(new FloatMenuOption("CannotControlMech".Translate(mech.LabelShort) + ": " + "NoPath".Translate().CapitalizeFirst(), null));
							}
							else if (!MechanitorUtility.CanControlMech(pawn, mech))
							{
								AcceptanceReport acceptanceReport = MechanitorUtility.CanControlMech(pawn, mech);
								if (!acceptanceReport.Reason.NullOrEmpty())
								{
									opts.Add(new FloatMenuOption("CannotControlMech".Translate(mech.LabelShort) + ": " + acceptanceReport.Reason, null));
								}
							}
							else
							{
								opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("ControlMech".Translate(mech.LabelShort), delegate
								{
									Job job22 = JobMaker.MakeJob(JobDefOf.ControlMech, thing2);
									pawn.jobs.TryTakeOrderedJob(job22, JobTag.Misc);
								}), pawn, new LocalTargetInfo(thing2)));
							}
							opts.Add(new FloatMenuOption("CannotDisassembleMech".Translate(mech.LabelCap) + ": " + "MustBeOverseer".Translate().CapitalizeFirst(), null));
						}
						else
						{
							opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("DisconnectMech".Translate(mech.LabelShort), delegate
							{
								MechanitorUtility.ForceDisconnectMechFromOverseer(mech);
							}, MenuOptionPriority.Low, null, null, 0f, null, null, playSelectionSound: true, -10), pawn, new LocalTargetInfo(thing2)));
							if (!mech.IsFighting())
							{
								opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("DisassembleMech".Translate(mech.LabelCap), delegate
								{
									Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmDisassemblingMech".Translate(mech.LabelCap) + ":\n" + (from x in MechanitorUtility.IngredientsFromDisassembly(mech.def)
										select x.Summary).ToLineList("  - "), delegate
									{
										pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.DisassembleMech, thing2), JobTag.Misc);
									}, destructive: true));
								}, MenuOptionPriority.Low, null, null, 0f, null, null, playSelectionSound: true, -20), pawn, new LocalTargetInfo(thing2)));
							}
						}
						if (!pawn.Drafted || !MechRepairUtility.CanRepair(mech))
						{
							continue;
						}
						if (!pawn.CanReach(mech, PathEndMode.Touch, Danger.Deadly))
						{
							opts.Add(new FloatMenuOption("CannotRepairMech".Translate(mech.LabelShort) + ": " + "NoPath".Translate().CapitalizeFirst(), null));
							continue;
						}
						opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("RepairThing".Translate(mech.LabelShort), delegate
						{
							Job job21 = JobMaker.MakeJob(JobDefOf.RepairMech, mech);
							pawn.jobs.TryTakeOrderedJob(job21, JobTag.Misc);
						}), pawn, new LocalTargetInfo(thing2)));
					}
				}
				if (ModsConfig.BiotechActive)
				{
					foreach (Thing item21 in thingList)
					{
						Pawn p2;
						if ((p2 = item21 as Pawn) == null || !p2.IsSelfShutdown())
						{
							continue;
						}
						Building_MechCharger charger = JobGiver_GetEnergy_Charger.GetClosestCharger(p2, pawn, forced: false);
						if (charger == null)
						{
							charger = JobGiver_GetEnergy_Charger.GetClosestCharger(p2, pawn, forced: true);
						}
						if (charger == null)
						{
							opts.Add(new FloatMenuOption("CannotCarryToRecharger".Translate(p2.Named("PAWN")) + ": " + "CannotCarryToRechargerNoneAvailable".Translate(), null));
							continue;
						}
						if (!pawn.CanReach(charger, PathEndMode.Touch, Danger.Deadly))
						{
							opts.Add(new FloatMenuOption("CannotCarryToRecharger".Translate(p2.Named("PAWN")) + ": " + "NoPath".Translate().CapitalizeFirst(), null));
							continue;
						}
						opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("CarryToRechargerOrdered".Translate(p2.Named("PAWN")), delegate
						{
							Job job20 = JobMaker.MakeJob(JobDefOf.HaulMechToCharger, p2, charger, charger.InteractionCell);
							job20.count = 1;
							pawn.jobs.TryTakeOrderedJob(job20, JobTag.Misc);
						}), pawn, new LocalTargetInfo(p2)));
					}
				}
			}
			if (ModsConfig.BiotechActive && pawn.CanDeathrest())
			{
				List<Thing> thingList2 = c.GetThingList(pawn.Map);
				for (int i = 0; i < thingList2.Count; i++)
				{
					Building_Bed bed;
					if ((bed = thingList2[i] as Building_Bed) == null || !bed.def.building.bed_humanlike)
					{
						continue;
					}
					if (!pawn.CanReach(bed, PathEndMode.OnCell, Danger.Deadly))
					{
						opts.Add(new FloatMenuOption("CannotDeathrest".Translate() + ": " + "NoPath".Translate().CapitalizeFirst(), null));
						continue;
					}
					AcceptanceReport acceptanceReport2 = bed.CompAssignableToPawn.CanAssignTo(pawn);
					if (!acceptanceReport2.Accepted)
					{
						opts.Add(new FloatMenuOption("CannotDeathrest".Translate() + ": " + acceptanceReport2.Reason, null));
						continue;
					}
					if ((!bed.CompAssignableToPawn.HasFreeSlot || !RestUtility.BedOwnerWillShare(bed, pawn, pawn.guest.GuestStatus)) && !bed.IsOwner(pawn))
					{
						opts.Add(new FloatMenuOption("CannotDeathrest".Translate() + ": " + "AssignedToOtherPawn".Translate(bed).CapitalizeFirst(), null));
						continue;
					}
					bool flag2 = false;
					foreach (IntVec3 item22 in bed.OccupiedRect())
					{
						if (item22.GetRoof(bed.Map) == null)
						{
							flag2 = true;
							break;
						}
					}
					if (flag2)
					{
						opts.Add(new FloatMenuOption("CannotDeathrest".Translate() + ": " + "ThingIsSkyExposed".Translate(bed).CapitalizeFirst(), null));
					}
					else if (RestUtility.IsValidBedFor(bed, pawn, pawn, checkSocialProperness: true, allowMedBedEvenIfSetToNoCare: false, ignoreOtherReservations: false, pawn.GuestStatus))
					{
						opts.Add(new FloatMenuOption("StartDeathrest".Translate(), delegate
						{
							Job job19 = JobMaker.MakeJob(JobDefOf.Deathrest, bed);
							job19.forceSleep = true;
							pawn.jobs.TryTakeOrderedJob(job19, JobTag.Misc);
						}));
					}
				}
			}
			if (ModsConfig.BiotechActive && pawn.IsBloodfeeder() && pawn.genes?.GetFirstGeneOfType<Gene_Hemogen>() != null)
			{
				foreach (LocalTargetInfo item23 in GenUI.TargetsAt(clickPos, TargetingParameters.ForBloodfeeding(pawn)))
				{
					Pawn targPawn3 = (Pawn)item23.Thing;
					if (!pawn.CanReach(targPawn3, PathEndMode.ClosestTouch, Danger.Deadly))
					{
						opts.Add(new FloatMenuOption("CannotBloodfeedOn".Translate(targPawn3.Named("PAWN")) + ": " + "NoPath".Translate().CapitalizeFirst(), null));
						continue;
					}
					AcceptanceReport acceptanceReport3 = JobGiver_GetHemogen.CanFeedOnPrisoner(pawn, targPawn3);
					if (acceptanceReport3.Accepted)
					{
						opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("BloodfeedOn".Translate(targPawn3.Named("PAWN")), delegate
						{
							Job job18 = JobMaker.MakeJob(JobDefOf.PrisonerBloodfeed, targPawn3);
							pawn.jobs.TryTakeOrderedJob(job18, JobTag.Misc);
						}), pawn, targPawn3));
					}
					else if (!acceptanceReport3.Reason.NullOrEmpty())
					{
						opts.Add(new FloatMenuOption("CannotBloodfeedOn".Translate(targPawn3.Named("PAWN")) + ": " + acceptanceReport3.Reason.CapitalizeFirst(), null));
					}
				}
			}
			if (ModsConfig.BiotechActive && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
			{
				foreach (LocalTargetInfo item24 in GenUI.TargetsAt(clickPos, TargetingParameters.ForCarryDeathresterToBed(pawn)))
				{
					Pawn targPawn2 = (Pawn)item24.Thing;
					if (targPawn2.InBed())
					{
						continue;
					}
					if (!pawn.CanReach(targPawn2, PathEndMode.ClosestTouch, Danger.Deadly))
					{
						opts.Add(new FloatMenuOption("CannotCarry".Translate(targPawn2) + ": " + "NoPath".Translate().CapitalizeFirst(), null));
						continue;
					}
					Thing bestBedOrCasket = GenClosest.ClosestThingReachable(targPawn2.PositionHeld, pawn.Map, ThingRequest.ForDef(ThingDefOf.DeathrestCasket), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, (Thing casket) => casket.Faction == Faction.OfPlayer && RestUtility.IsValidBedFor(casket, targPawn2, pawn, checkSocialProperness: true, allowMedBedEvenIfSetToNoCare: false, ignoreOtherReservations: false, targPawn2.GuestStatus));
					if (bestBedOrCasket == null)
					{
						bestBedOrCasket = RestUtility.FindBedFor(targPawn2, pawn, checkSocialProperness: false);
					}
					if (bestBedOrCasket != null)
					{
						opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("CarryToSpecificThing".Translate(bestBedOrCasket), delegate
						{
							Job job17 = JobMaker.MakeJob(JobDefOf.DeliverToBed, targPawn2, bestBedOrCasket);
							job17.count = 1;
							pawn.jobs.TryTakeOrderedJob(job17, JobTag.Misc);
						}, MenuOptionPriority.RescueOrCapture, null, targPawn2), pawn, targPawn2));
					}
					else
					{
						opts.Add(new FloatMenuOption("CannotCarry".Translate(targPawn2) + ": " + "NoCasketOrBed".Translate(), null));
					}
				}
			}
			if (ModsConfig.BiotechActive && pawn.genes != null)
			{
				foreach (LocalTargetInfo item25 in GenUI.TargetsAt(clickPos, TargetingParameters.ForXenogermAbsorption(pawn), thingsOnly: true))
				{
					Pawn targPawn = (Pawn)item25.Thing;
					if (!pawn.CanReserveAndReach(targPawn, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, ignoreOtherReservations: true))
					{
						continue;
					}
					FloatMenuOption item5 = (pawn.IsQuestLodger() ? new FloatMenuOption("CannotAbsorbXenogerm".Translate(targPawn.Named("PAWN")) + ": " + "TemporaryFactionMember".Translate(pawn.Named("PAWN")), null) : (GeneUtility.SameXenotype(pawn, targPawn) ? new FloatMenuOption("CannotAbsorbXenogerm".Translate(targPawn.Named("PAWN")) + ": " + "SameXenotype".Translate(pawn.Named("PAWN")), null) : (targPawn.health.hediffSet.HasHediff(HediffDefOf.XenogermLossShock) ? new FloatMenuOption("CannotAbsorbXenogerm".Translate(targPawn.Named("PAWN")) + ": " + "XenogermLossShockPresent".Translate(targPawn.Named("PAWN")), null) : (CompAbilityEffect_ReimplantXenogerm.PawnIdeoCanAcceptReimplant(targPawn, pawn) ? FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("AbsorbXenogerm".Translate(targPawn.Named("PAWN")), delegate
					{
						if (targPawn.IsPrisonerOfColony && !targPawn.Downed)
						{
							Messages.Message("MessageTargetMustBeDownedToForceReimplant".Translate(targPawn.Named("PAWN")), targPawn, MessageTypeDefOf.RejectInput, historical: false);
						}
						else if (GeneUtility.PawnWouldDieFromReimplanting(targPawn))
						{
							Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("WarningPawnWillDieFromReimplanting".Translate(targPawn.Named("PAWN")), delegate
							{
								GeneUtility.GiveReimplantJob(pawn, targPawn);
							}, destructive: true));
						}
						else
						{
							GeneUtility.GiveReimplantJob(pawn, targPawn);
						}
					}), pawn, targPawn) : new FloatMenuOption("CannotAbsorbXenogerm".Translate(targPawn.Named("PAWN")) + ": " + "IdeoligionForbids".Translate(), null)))));
					opts.Add(item5);
				}
			}
			if (ModsConfig.BiotechActive && ChildcareUtility.CanBreastfeed(pawn, out reason) && !pawn.Downed && !pawn.Drafted)
			{
				foreach (LocalTargetInfo item26 in GenUI.TargetsAt(clickPos, TargetingParameters.ForBabyCare(pawn), thingsOnly: true))
				{
					Pawn baby = (Pawn)item26.Thing;
					if (ChildcareUtility.CanSuckle(baby, out reason) && ChildcareUtility.HasBreastfeedCompatibleFactions(pawn, baby))
					{
						ChildcareUtility.BreastfeedFailReason? reason2;
						FloatMenuOption item6 = (ChildcareUtility.CanMomAutoBreastfeedBabyNow(pawn, baby, forced: true, out reason2) ? FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("BabyCareBreastfeed".Translate(baby.Named("BABY")), delegate
						{
							pawn.jobs.TryTakeOrderedJob(ChildcareUtility.MakeBreastfeedJob(baby), JobTag.Misc);
						}), pawn, baby) : new FloatMenuOption("BabyCareBreastfeedUnable".Translate(baby.Named("BABY")) + ": " + reason2.Value.Translate(pawn, pawn, baby).CapitalizeFirst(), null));
						opts.Add(item6);
					}
				}
			}
			if (!pawn.Drafted && ModsConfig.BiotechActive)
			{
				foreach (LocalTargetInfo item27 in GenUI.TargetsAt(clickPos, TargetingParameters.ForRomance(pawn), thingsOnly: true))
				{
					Pawn pawn4 = (Pawn)item27.Thing;
					if (!pawn4.Drafted && !ChildcareUtility.CanSuckle(pawn4, out reason))
					{
						FloatMenuOption option;
						float chance;
						bool flag3 = RelationsUtility.RomanceOption(pawn, pawn4, out option, out chance);
						if (option != null)
						{
							option.Label = (flag3 ? "CanRomance" : "CannotRomance").Translate(option.Label);
							opts.Add(option);
						}
					}
				}
			}
			foreach (LocalTargetInfo item28 in GenUI.TargetsAt(clickPos, TargetingParameters.ForStrip(pawn), thingsOnly: true))
			{
				LocalTargetInfo stripTarg = item28;
				FloatMenuOption item7 = (pawn.CanReach(stripTarg, PathEndMode.ClosestTouch, Danger.Deadly) ? ((stripTarg.Pawn != null && stripTarg.Pawn.HasExtraHomeFaction()) ? new FloatMenuOption("CannotStrip".Translate(stripTarg.Thing.LabelCap, stripTarg.Thing) + ": " + "QuestRelated".Translate().CapitalizeFirst(), null) : (pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) ? FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Strip".Translate(stripTarg.Thing.LabelCap, stripTarg.Thing), delegate
				{
					stripTarg.Thing.SetForbidden(value: false, warnOnFail: false);
					pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Strip, stripTarg), JobTag.Misc);
					StrippableUtility.CheckSendStrippingImpactsGoodwillMessage(stripTarg.Thing);
				}), pawn, stripTarg) : new FloatMenuOption("CannotStrip".Translate(stripTarg.Thing.LabelCap, stripTarg.Thing) + ": " + "Incapable".Translate().CapitalizeFirst(), null))) : new FloatMenuOption("CannotStrip".Translate(stripTarg.Thing.LabelCap, stripTarg.Thing) + ": " + "NoPath".Translate().CapitalizeFirst(), null));
				opts.Add(item7);
			}
			if (pawn.equipment != null)
			{
				List<Thing> thingList3 = c.GetThingList(pawn.Map);
				for (int j = 0; j < thingList3.Count; j++)
				{
					if (thingList3[j].TryGetComp<CompEquippable>() == null)
					{
						continue;
					}
					ThingWithComps equipment = (ThingWithComps)thingList3[j];
					string labelShort = equipment.LabelShort;
					FloatMenuOption item8;
					string cantReason;
					if (equipment.def.IsWeapon && pawn.WorkTagIsDisabled(WorkTags.Violent))
					{
						item8 = new FloatMenuOption("CannotEquip".Translate(labelShort) + ": " + "IsIncapableOfViolenceLower".Translate(pawn.LabelShort, pawn), null);
					}
					else if (equipment.def.IsRangedWeapon && pawn.WorkTagIsDisabled(WorkTags.Shooting))
					{
						item8 = new FloatMenuOption("CannotEquip".Translate(labelShort) + ": " + "IsIncapableOfShootingLower".Translate(pawn), null);
					}
					else if (!pawn.CanReach(equipment, PathEndMode.ClosestTouch, Danger.Deadly))
					{
						item8 = new FloatMenuOption("CannotEquip".Translate(labelShort) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
					}
					else if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
					{
						item8 = new FloatMenuOption("CannotEquip".Translate(labelShort) + ": " + "Incapable".Translate().CapitalizeFirst(), null);
					}
					else if (equipment.IsBurning())
					{
						item8 = new FloatMenuOption("CannotEquip".Translate(labelShort) + ": " + "BurningLower".Translate(), null);
					}
					else if (pawn.IsQuestLodger() && !EquipmentUtility.QuestLodgerCanEquip(equipment, pawn))
					{
						item8 = new FloatMenuOption("CannotEquip".Translate(labelShort) + ": " + "QuestRelated".Translate().CapitalizeFirst(), null);
					}
					else if (!EquipmentUtility.CanEquip(equipment, pawn, out cantReason, checkBonded: false))
					{
						item8 = new FloatMenuOption("CannotEquip".Translate(labelShort) + ": " + cantReason.CapitalizeFirst(), null);
					}
					else
					{
						string text4 = "Equip".Translate(labelShort);
						if (equipment.def.IsRangedWeapon && pawn.story != null && pawn.story.traits.HasTrait(TraitDefOf.Brawler))
						{
							text4 += " " + "EquipWarningBrawler".Translate();
						}
						if (EquipmentUtility.AlreadyBondedToWeapon(equipment, pawn))
						{
							text4 += " " + "BladelinkAlreadyBonded".Translate();
							TaggedString dialogText = "BladelinkAlreadyBondedDialog".Translate(pawn.Named("PAWN"), equipment.Named("WEAPON"), pawn.equipment.bondedWeapon.Named("BONDEDWEAPON"));
							item8 = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text4, delegate
							{
								Find.WindowStack.Add(new Dialog_MessageBox(dialogText));
							}, MenuOptionPriority.High), pawn, equipment);
						}
						else
						{
							item8 = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text4, delegate
							{
								string personaWeaponConfirmationText = EquipmentUtility.GetPersonaWeaponConfirmationText(equipment, pawn);
								if (!personaWeaponConfirmationText.NullOrEmpty())
								{
									Find.WindowStack.Add(new Dialog_MessageBox(personaWeaponConfirmationText, "Yes".Translate(), delegate
									{
										Equip();
									}, "No".Translate()));
								}
								else
								{
									Equip();
								}
							}, MenuOptionPriority.High), pawn, equipment);
						}
					}
					opts.Add(item8);
					void Equip()
					{
						equipment.SetForbidden(value: false);
						pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Equip, equipment), JobTag.Misc);
						FleckMaker.Static(equipment.DrawPos, equipment.MapHeld, FleckDefOf.FeedbackEquip);
						PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.EquippingWeapons, KnowledgeAmount.Total);
					}
				}
			}
			foreach (Pair<CompReloadable, Thing> item29 in ReloadableUtility.FindPotentiallyReloadableGear(pawn, c.GetThingList(pawn.Map)))
			{
				CompReloadable comp = item29.First;
				Thing second = item29.Second;
				string text5 = "Reload".Translate(comp.parent.Named("GEAR"), NamedArgumentUtility.Named(comp.AmmoDef, "AMMO")) + " (" + comp.LabelRemaining + ")";
				if (!pawn.CanReach(second, PathEndMode.ClosestTouch, Danger.Deadly))
				{
					opts.Add(new FloatMenuOption(text5 + ": " + "NoPath".Translate().CapitalizeFirst(), null));
					continue;
				}
				if (!comp.NeedsReload(allowForcedReload: true))
				{
					opts.Add(new FloatMenuOption(text5 + ": " + "ReloadFull".Translate(), null));
					continue;
				}
				List<Thing> chosenAmmo;
				if ((chosenAmmo = ReloadableUtility.FindEnoughAmmo(pawn, second.Position, comp, forceReload: true)) == null)
				{
					opts.Add(new FloatMenuOption(text5 + ": " + "ReloadNotEnough".Translate(), null));
					continue;
				}
				if (pawn.carryTracker.AvailableStackSpace(comp.AmmoDef) < comp.MinAmmoNeeded(allowForcedReload: true))
				{
					opts.Add(new FloatMenuOption(text5 + ": " + "ReloadCannotCarryEnough".Translate(NamedArgumentUtility.Named(comp.AmmoDef, "AMMO")), null));
					continue;
				}
				Action action4 = delegate
				{
					pawn.jobs.TryTakeOrderedJob(JobGiver_Reload.MakeReloadJob(comp, chosenAmmo), JobTag.Misc);
				};
				opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text5, action4), pawn, second));
			}
			if (pawn.apparel != null)
			{
				foreach (Thing item30 in pawn.Map.thingGrid.ThingsAt(c))
				{
					Apparel apparel = item30 as Apparel;
					if (apparel == null)
					{
						continue;
					}
					string key = "CannotWear";
					string key2 = "ForceWear";
					if (apparel.def.apparel.LastLayer.IsUtilityLayer)
					{
						key = "CannotEquipApparel";
						key2 = "ForceEquipApparel";
					}
					string cantReason2;
					FloatMenuOption item9 = ((!pawn.CanReach(apparel, PathEndMode.ClosestTouch, Danger.Deadly)) ? new FloatMenuOption(key.Translate(apparel.Label, apparel) + ": " + "NoPath".Translate().CapitalizeFirst(), null) : (apparel.IsBurning() ? new FloatMenuOption(key.Translate(apparel.Label, apparel) + ": " + "Burning".Translate(), null) : (pawn.apparel.WouldReplaceLockedApparel(apparel) ? new FloatMenuOption(key.Translate(apparel.Label, apparel) + ": " + "WouldReplaceLockedApparel".Translate().CapitalizeFirst(), null) : ((!ApparelUtility.HasPartsToWear(pawn, apparel.def)) ? new FloatMenuOption(key.Translate(apparel.Label, apparel) + ": " + "CannotWearBecauseOfMissingBodyParts".Translate().CapitalizeFirst(), null) : (EquipmentUtility.CanEquip(apparel, pawn, out cantReason2) ? FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(key2.Translate(apparel.LabelShort, apparel), delegate
					{
						Action action7 = delegate
						{
							apparel.SetForbidden(value: false);
							Job job16 = JobMaker.MakeJob(JobDefOf.Wear, apparel);
							pawn.jobs.TryTakeOrderedJob(job16, JobTag.Misc);
						};
						Apparel apparelReplacedByNewApparel = ApparelUtility.GetApparelReplacedByNewApparel(pawn, apparel);
						if (apparelReplacedByNewApparel == null || !ModsConfig.BiotechActive || !MechanitorUtility.TryConfirmBandwidthLossFromDroppingThing(pawn, apparelReplacedByNewApparel, action7))
						{
							action7();
						}
					}, MenuOptionPriority.High), pawn, apparel) : new FloatMenuOption(key.Translate(apparel.Label, apparel) + ": " + cantReason2, null))))));
					opts.Add(item9);
				}
			}
			if (pawn.IsFormingCaravan())
			{
				foreach (Thing item3 in c.GetItems(pawn.Map))
				{
					if (!item3.def.EverHaulable || !item3.def.canLoadIntoCaravan)
					{
						continue;
					}
					Pawn packTarget = GiveToPackAnimalUtility.UsablePackAnimalWithTheMostFreeSpace(pawn) ?? pawn;
					JobDef jobDef = ((packTarget == pawn) ? JobDefOf.TakeInventory : JobDefOf.GiveToPackAnimal);
					if (!pawn.CanReach(item3, PathEndMode.ClosestTouch, Danger.Deadly))
					{
						opts.Add(new FloatMenuOption("CannotLoadIntoCaravan".Translate(item3.Label, item3) + ": " + "NoPath".Translate().CapitalizeFirst(), null));
						continue;
					}
					if (MassUtility.WillBeOverEncumberedAfterPickingUp(packTarget, item3, 1))
					{
						opts.Add(new FloatMenuOption("CannotLoadIntoCaravan".Translate(item3.Label, item3) + ": " + "TooHeavy".Translate(), null));
						continue;
					}
					LordJob_FormAndSendCaravan lordJob = (LordJob_FormAndSendCaravan)pawn.GetLord().LordJob;
					float capacityLeft = CaravanFormingUtility.CapacityLeft(lordJob);
					if (item3.stackCount == 1)
					{
						float capacityLeft2 = capacityLeft - item3.GetStatValue(StatDefOf.Mass);
						opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(CaravanFormingUtility.AppendOverweightInfo("LoadIntoCaravan".Translate(item3.Label, item3), capacityLeft2), delegate
						{
							item3.SetForbidden(value: false, warnOnFail: false);
							Job job15 = JobMaker.MakeJob(jobDef, item3);
							job15.count = 1;
							job15.checkEncumbrance = packTarget == pawn;
							pawn.jobs.TryTakeOrderedJob(job15, JobTag.Misc);
						}, MenuOptionPriority.High), pawn, item3));
						continue;
					}
					if (MassUtility.WillBeOverEncumberedAfterPickingUp(packTarget, item3, item3.stackCount))
					{
						opts.Add(new FloatMenuOption("CannotLoadIntoCaravanAll".Translate(item3.Label, item3) + ": " + "TooHeavy".Translate(), null));
					}
					else
					{
						float capacityLeft3 = capacityLeft - (float)item3.stackCount * item3.GetStatValue(StatDefOf.Mass);
						opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(CaravanFormingUtility.AppendOverweightInfo("LoadIntoCaravanAll".Translate(item3.Label, item3), capacityLeft3), delegate
						{
							item3.SetForbidden(value: false, warnOnFail: false);
							Job job14 = JobMaker.MakeJob(jobDef, item3);
							job14.count = item3.stackCount;
							job14.checkEncumbrance = packTarget == pawn;
							pawn.jobs.TryTakeOrderedJob(job14, JobTag.Misc);
						}, MenuOptionPriority.High), pawn, item3));
					}
					opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("LoadIntoCaravanSome".Translate(item3.LabelNoCount, item3), delegate
					{
						int to3 = Mathf.Min(MassUtility.CountToPickUpUntilOverEncumbered(packTarget, item3), item3.stackCount);
						Dialog_Slider window3 = new Dialog_Slider(delegate(int val)
						{
							float capacityLeft4 = capacityLeft - (float)val * item3.GetStatValue(StatDefOf.Mass);
							return CaravanFormingUtility.AppendOverweightInfo("LoadIntoCaravanCount".Translate(item3.LabelNoCount, item3).Formatted(val), capacityLeft4);
						}, 1, to3, delegate(int count)
						{
							item3.SetForbidden(value: false, warnOnFail: false);
							Job job13 = JobMaker.MakeJob(jobDef, item3);
							job13.count = count;
							job13.checkEncumbrance = packTarget == pawn;
							pawn.jobs.TryTakeOrderedJob(job13, JobTag.Misc);
						});
						Find.WindowStack.Add(window3);
					}, MenuOptionPriority.High), pawn, item3));
				}
			}
			if (!pawn.IsFormingCaravan())
			{
				foreach (Thing item2 in c.GetItems(pawn.Map))
				{
					if (!item2.def.EverHaulable || !PawnUtility.CanPickUp(pawn, item2.def) || (pawn.Map.IsPlayerHome && !JobGiver_DropUnusedInventory.ShouldKeepDrugInInventory(pawn, item2)))
					{
						continue;
					}
					if (!pawn.CanReach(item2, PathEndMode.ClosestTouch, Danger.Deadly))
					{
						opts.Add(new FloatMenuOption("CannotPickUp".Translate(item2.Label, item2) + ": " + "NoPath".Translate().CapitalizeFirst(), null));
						continue;
					}
					if (MassUtility.WillBeOverEncumberedAfterPickingUp(pawn, item2, 1))
					{
						opts.Add(new FloatMenuOption("CannotPickUp".Translate(item2.Label, item2) + ": " + "TooHeavy".Translate(), null));
						continue;
					}
					int maxAllowedToPickUp = PawnUtility.GetMaxAllowedToPickUp(pawn, item2.def);
					if (maxAllowedToPickUp == 0)
					{
						opts.Add(new FloatMenuOption("CannotPickUp".Translate(item2.Label, item2) + ": " + "MaxPickUpAllowed".Translate(item2.def.orderedTakeGroup.max, item2.def.orderedTakeGroup.label), null));
						continue;
					}
					if (item2.stackCount == 1 || maxAllowedToPickUp == 1)
					{
						opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("PickUpOne".Translate(item2.LabelNoCount, item2), delegate
						{
							item2.SetForbidden(value: false, warnOnFail: false);
							Job job12 = JobMaker.MakeJob(JobDefOf.TakeInventory, item2);
							job12.count = 1;
							job12.checkEncumbrance = true;
							job12.takeInventoryDelay = 120;
							pawn.jobs.TryTakeOrderedJob(job12, JobTag.Misc);
						}, MenuOptionPriority.High), pawn, item2));
						continue;
					}
					if (maxAllowedToPickUp < item2.stackCount)
					{
						opts.Add(new FloatMenuOption("CannotPickUpAll".Translate(item2.Label, item2) + ": " + "MaxPickUpAllowed".Translate(item2.def.orderedTakeGroup.max, item2.def.orderedTakeGroup.label), null));
					}
					else if (MassUtility.WillBeOverEncumberedAfterPickingUp(pawn, item2, item2.stackCount))
					{
						opts.Add(new FloatMenuOption("CannotPickUpAll".Translate(item2.Label, item2) + ": " + "TooHeavy".Translate(), null));
					}
					else
					{
						opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("PickUpAll".Translate(item2.Label, item2), delegate
						{
							item2.SetForbidden(value: false, warnOnFail: false);
							Job job11 = JobMaker.MakeJob(JobDefOf.TakeInventory, item2);
							job11.count = item2.stackCount;
							job11.checkEncumbrance = true;
							job11.takeInventoryDelay = 120;
							pawn.jobs.TryTakeOrderedJob(job11, JobTag.Misc);
						}, MenuOptionPriority.High), pawn, item2));
					}
					opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("PickUpSome".Translate(item2.LabelNoCount, item2), delegate
					{
						int b = Mathf.Min(MassUtility.CountToPickUpUntilOverEncumbered(pawn, item2), item2.stackCount);
						int to2 = Mathf.Min(maxAllowedToPickUp, b);
						Dialog_Slider window2 = new Dialog_Slider("PickUpCount".Translate(item2.LabelNoCount, item2), 1, to2, delegate(int count)
						{
							item2.SetForbidden(value: false, warnOnFail: false);
							Job job10 = JobMaker.MakeJob(JobDefOf.TakeInventory, item2);
							job10.count = count;
							job10.checkEncumbrance = true;
							job10.takeInventoryDelay = 120;
							pawn.jobs.TryTakeOrderedJob(job10, JobTag.Misc);
						});
						Find.WindowStack.Add(window2);
					}, MenuOptionPriority.High), pawn, item2));
				}
			}
			if (!pawn.Map.IsPlayerHome && !pawn.IsFormingCaravan())
			{
				foreach (Thing item in c.GetItems(pawn.Map))
				{
					if (!item.def.EverHaulable)
					{
						continue;
					}
					Pawn bestPackAnimal = GiveToPackAnimalUtility.UsablePackAnimalWithTheMostFreeSpace(pawn);
					if (bestPackAnimal == null)
					{
						continue;
					}
					if (!pawn.CanReach(item, PathEndMode.ClosestTouch, Danger.Deadly))
					{
						opts.Add(new FloatMenuOption("CannotGiveToPackAnimal".Translate(item.Label, item) + ": " + "NoPath".Translate().CapitalizeFirst(), null));
						continue;
					}
					if (MassUtility.WillBeOverEncumberedAfterPickingUp(bestPackAnimal, item, 1))
					{
						opts.Add(new FloatMenuOption("CannotGiveToPackAnimal".Translate(item.Label, item) + ": " + "TooHeavy".Translate(), null));
						continue;
					}
					if (item.stackCount == 1)
					{
						opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("GiveToPackAnimal".Translate(item.Label, item), delegate
						{
							item.SetForbidden(value: false, warnOnFail: false);
							Job job9 = JobMaker.MakeJob(JobDefOf.GiveToPackAnimal, item);
							job9.count = 1;
							pawn.jobs.TryTakeOrderedJob(job9, JobTag.Misc);
						}, MenuOptionPriority.High), pawn, item));
						continue;
					}
					if (MassUtility.WillBeOverEncumberedAfterPickingUp(bestPackAnimal, item, item.stackCount))
					{
						opts.Add(new FloatMenuOption("CannotGiveToPackAnimalAll".Translate(item.Label, item) + ": " + "TooHeavy".Translate(), null));
					}
					else
					{
						opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("GiveToPackAnimalAll".Translate(item.Label, item), delegate
						{
							item.SetForbidden(value: false, warnOnFail: false);
							Job job8 = JobMaker.MakeJob(JobDefOf.GiveToPackAnimal, item);
							job8.count = item.stackCount;
							pawn.jobs.TryTakeOrderedJob(job8, JobTag.Misc);
						}, MenuOptionPriority.High), pawn, item));
					}
					opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("GiveToPackAnimalSome".Translate(item.LabelNoCount, item), delegate
					{
						int to = Mathf.Min(MassUtility.CountToPickUpUntilOverEncumbered(bestPackAnimal, item), item.stackCount);
						Dialog_Slider window = new Dialog_Slider("GiveToPackAnimalCount".Translate(item.LabelNoCount, item), 1, to, delegate(int count)
						{
							item.SetForbidden(value: false, warnOnFail: false);
							Job job7 = JobMaker.MakeJob(JobDefOf.GiveToPackAnimal, item);
							job7.count = count;
							pawn.jobs.TryTakeOrderedJob(job7, JobTag.Misc);
						});
						Find.WindowStack.Add(window);
					}, MenuOptionPriority.High), pawn, item));
				}
			}
			if (!pawn.Map.IsPlayerHome && pawn.Map.exitMapGrid.MapUsesExitGrid)
			{
				foreach (LocalTargetInfo item31 in GenUI.TargetsAt(clickPos, TargetingParameters.ForRescue(pawn), thingsOnly: true))
				{
					Pawn p = (Pawn)item31.Thing;
					if (p.Faction != Faction.OfPlayer && !p.IsPrisonerOfColony && !CaravanUtility.ShouldAutoCapture(p, Faction.OfPlayer))
					{
						continue;
					}
					if (!pawn.CanReach(p, PathEndMode.ClosestTouch, Danger.Deadly))
					{
						opts.Add(new FloatMenuOption("CannotCarryToExit".Translate(p.Label, p) + ": " + "NoPath".Translate().CapitalizeFirst(), null));
						continue;
					}
					if (!RCellFinder.TryFindBestExitSpot(pawn, out var exitSpot))
					{
						opts.Add(new FloatMenuOption("CannotCarryToExit".Translate(p.Label, p) + ": " + "NoPath".Translate().CapitalizeFirst(), null));
						continue;
					}
					TaggedString taggedString4 = ((p.Faction == Faction.OfPlayer || p.IsPrisonerOfColony) ? "CarryToExit".Translate(p.Label, p) : "CarryToExitAndCapture".Translate(p.Label, p));
					opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(taggedString4, delegate
					{
						Job job6 = JobMaker.MakeJob(JobDefOf.CarryDownedPawnToExit, p, exitSpot);
						job6.count = 1;
						job6.failIfCantJoinOrCreateCaravan = true;
						pawn.jobs.TryTakeOrderedJob(job6, JobTag.Misc);
					}, MenuOptionPriority.High), pawn, item31));
				}
			}
			if (pawn.equipment != null && pawn.equipment.Primary != null && GenUI.TargetsAt(clickPos, TargetingParameters.ForSelf(pawn), thingsOnly: true).Any())
			{
				if (pawn.IsQuestLodger() && !EquipmentUtility.QuestLodgerCanUnequip(pawn.equipment.Primary, pawn))
				{
					opts.Add(new FloatMenuOption("CannotDrop".Translate(pawn.equipment.Primary.Label, pawn.equipment.Primary) + ": " + "QuestRelated".Translate().CapitalizeFirst(), null));
				}
				else
				{
					Action action5 = delegate
					{
						pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.DropEquipment, pawn.equipment.Primary), JobTag.Misc);
					};
					opts.Add(new FloatMenuOption("Drop".Translate(pawn.equipment.Primary.Label, pawn.equipment.Primary), action5, MenuOptionPriority.Default, null, pawn));
				}
			}
			foreach (LocalTargetInfo item32 in GenUI.TargetsAt(clickPos, TargetingParameters.ForTrade(), thingsOnly: true))
			{
				if (!pawn.CanReach(item32, PathEndMode.OnCell, Danger.Deadly))
				{
					opts.Add(new FloatMenuOption("CannotTrade".Translate() + ": " + "NoPath".Translate().CapitalizeFirst(), null));
					continue;
				}
				if (pawn.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
				{
					opts.Add(new FloatMenuOption("CannotPrioritizeWorkTypeDisabled".Translate(SkillDefOf.Social.LabelCap), null));
					continue;
				}
				if (!pawn.CanTradeWith(((Pawn)item32.Thing).Faction, ((Pawn)item32.Thing).TraderKind).Accepted)
				{
					opts.Add(new FloatMenuOption("CannotTrade".Translate() + ": " + "MissingTitleAbility".Translate().CapitalizeFirst(), null));
					continue;
				}
				Pawn pTarg = (Pawn)item32.Thing;
				Action action6 = delegate
				{
					Job job5 = JobMaker.MakeJob(JobDefOf.TradeWithPawn, pTarg);
					job5.playerForced = true;
					pawn.jobs.TryTakeOrderedJob(job5, JobTag.Misc);
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.InteractingWithTraders, KnowledgeAmount.Total);
				};
				string text6 = "";
				if (pTarg.Faction != null)
				{
					text6 = " (" + pTarg.Faction.Name + ")";
				}
				opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("TradeWith".Translate(pTarg.LabelShort + ", " + pTarg.TraderKind.label) + text6, action6, MenuOptionPriority.InitiateSocial, null, item32.Thing), pawn, pTarg));
			}
			foreach (LocalTargetInfo casket2 in GenUI.TargetsAt(clickPos, TargetingParameters.ForOpen(pawn), thingsOnly: true))
			{
				if (!pawn.CanReach(casket2, PathEndMode.OnCell, Danger.Deadly))
				{
					opts.Add(new FloatMenuOption("CannotOpen".Translate(casket2.Thing) + ": " + "NoPath".Translate().CapitalizeFirst(), null));
				}
				else if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
				{
					opts.Add(new FloatMenuOption("CannotOpen".Translate(casket2.Thing) + ": " + "Incapable".Translate().CapitalizeFirst(), null));
				}
				else if (casket2.Thing.Map.designationManager.DesignationOn(casket2.Thing, DesignationDefOf.Open) == null)
				{
					opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Open".Translate(casket2.Thing), delegate
					{
						Job job4 = JobMaker.MakeJob(JobDefOf.Open, casket2.Thing);
						job4.ignoreDesignations = true;
						pawn.jobs.TryTakeOrderedJob(job4, JobTag.Misc);
					}, MenuOptionPriority.High), pawn, casket2.Thing));
				}
			}
			foreach (Thing item33 in pawn.Map.thingGrid.ThingsAt(c))
			{
				foreach (FloatMenuOption floatMenuOption in item33.GetFloatMenuOptions(pawn))
				{
					opts.Add(floatMenuOption);
				}
			}
		}

		private static void AddUndraftedOrders(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
		{
			if (equivalenceGroupTempStorage == null || equivalenceGroupTempStorage.Length != DefDatabase<WorkGiverEquivalenceGroupDef>.DefCount)
			{
				equivalenceGroupTempStorage = new FloatMenuOption[DefDatabase<WorkGiverEquivalenceGroupDef>.DefCount];
			}
			IntVec3 c = IntVec3.FromVector3(clickPos);
			bool flag = false;
			bool flag2 = false;
			foreach (Thing item in pawn.Map.thingGrid.ThingsAt(c))
			{
				flag2 = true;
				if (pawn.CanReach(item, PathEndMode.Touch, Danger.Deadly))
				{
					flag = true;
					break;
				}
			}
			if (!flag2 || flag)
			{
				AddJobGiverWorkOrders(clickPos, pawn, opts, drafted: false);
			}
		}

		private static void AddJobGiverWorkOrders(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts, bool drafted)
		{
			if (pawn.thinker.TryGetMainTreeThinkNode<JobGiver_Work>() == null)
			{
				return;
			}
			IntVec3 clickCell = IntVec3.FromVector3(clickPos);
			TargetingParameters targetingParameters = new TargetingParameters();
			targetingParameters.canTargetPawns = true;
			targetingParameters.canTargetBuildings = true;
			targetingParameters.canTargetItems = true;
			targetingParameters.mapObjectTargetsMustBeAutoAttackable = false;
			foreach (Thing item in GenUI.ThingsUnderMouse(clickPos, 1f, targetingParameters))
			{
				if (!item.Spawned)
				{
					continue;
				}
				bool flag = false;
				foreach (WorkTypeDef item2 in DefDatabase<WorkTypeDef>.AllDefsListForReading)
				{
					for (int i = 0; i < item2.workGiversByPriority.Count; i++)
					{
						WorkGiverDef workGiver2 = item2.workGiversByPriority[i];
						if ((drafted && !workGiver2.canBeDoneWhileDrafted) || !(workGiver2.Worker is WorkGiver_Scanner workGiver_Scanner) || !workGiver_Scanner.def.directOrderable)
						{
							continue;
						}
						JobFailReason.Clear();
						if ((!workGiver_Scanner.PotentialWorkThingRequest.Accepts(item) && (workGiver_Scanner.PotentialWorkThingsGlobal(pawn) == null || !workGiver_Scanner.PotentialWorkThingsGlobal(pawn).Contains(item))) || workGiver_Scanner.ShouldSkip(pawn, forced: true))
						{
							continue;
						}
						string text = null;
						Action action = null;
						PawnCapacityDef pawnCapacityDef = workGiver_Scanner.MissingRequiredCapacity(pawn);
						if (pawnCapacityDef != null)
						{
							text = "CannotMissingHealthActivities".Translate(pawnCapacityDef.label);
						}
						else
						{
							Job job = (workGiver_Scanner.HasJobOnThing(pawn, item, forced: true) ? workGiver_Scanner.JobOnThing(pawn, item, forced: true) : null);
							if (job == null)
							{
								if (JobFailReason.HaveReason)
								{
									text = (JobFailReason.CustomJobString.NullOrEmpty() ? ((string)"CannotGenericWork".Translate(workGiver_Scanner.def.verb, item.LabelShort, item)) : ((string)"CannotGenericWorkCustom".Translate(JobFailReason.CustomJobString)));
									text = text + ": " + JobFailReason.Reason.CapitalizeFirst();
								}
								else
								{
									if (!item.IsForbidden(pawn))
									{
										continue;
									}
									text = (item.Position.InAllowedArea(pawn) ? ((string)"CannotPrioritizeForbidden".Translate(item.Label, item)) : ((string)("CannotPrioritizeForbiddenOutsideAllowedArea".Translate() + ": " + pawn.playerSettings.EffectiveAreaRestriction.Label)));
								}
							}
							else
							{
								WorkTypeDef workType = workGiver_Scanner.def.workType;
								if (pawn.WorkTagIsDisabled(workGiver_Scanner.def.workTags))
								{
									text = "CannotPrioritizeWorkGiverDisabled".Translate(workGiver_Scanner.def.label);
								}
								else if (pawn.jobs.curJob != null && pawn.jobs.curJob.JobIsSameAs(job))
								{
									text = "CannotGenericAlreadyAm".Translate(workGiver_Scanner.PostProcessedGerund(job), item.LabelShort, item);
								}
								else if (pawn.workSettings.GetPriority(workType) == 0)
								{
									text = (pawn.WorkTypeIsDisabled(workType) ? ((string)"CannotPrioritizeWorkTypeDisabled".Translate(workType.gerundLabel)) : ((!"CannotPrioritizeNotAssignedToWorkType".CanTranslate()) ? ((string)"CannotPrioritizeWorkTypeDisabled".Translate(workType.pawnLabel)) : ((string)"CannotPrioritizeNotAssignedToWorkType".Translate(workType.gerundLabel))));
								}
								else if (job.def == JobDefOf.Research && item is Building_ResearchBench)
								{
									text = "CannotPrioritizeResearch".Translate();
								}
								else if (item.IsForbidden(pawn))
								{
									text = (item.Position.InAllowedArea(pawn) ? ((string)"CannotPrioritizeForbidden".Translate(item.Label, item)) : ((string)("CannotPrioritizeForbiddenOutsideAllowedArea".Translate() + ": " + pawn.playerSettings.EffectiveAreaRestriction.Label)));
								}
								else if (!pawn.CanReach(item, workGiver_Scanner.PathEndMode, Danger.Deadly))
								{
									text = (item.Label + ": " + "NoPath".Translate().CapitalizeFirst()).CapitalizeFirst();
								}
								else
								{
									text = "PrioritizeGeneric".Translate(workGiver_Scanner.PostProcessedGerund(job), item.Label).CapitalizeFirst();
									string text2 = workGiver_Scanner.JobInfo(pawn, job);
									if (!string.IsNullOrEmpty(text2))
									{
										text = text + ": " + text2;
									}
									Job localJob2 = job;
									WorkGiver_Scanner localScanner2 = workGiver_Scanner;
									job.workGiverDef = workGiver_Scanner.def;
									action = delegate
									{
										if (pawn.jobs.TryTakeOrderedJobPrioritizedWork(localJob2, localScanner2, clickCell))
										{
											if (workGiver2.forceMote != null)
											{
												MoteMaker.MakeStaticMote(clickCell, pawn.Map, workGiver2.forceMote);
											}
											if (workGiver2.forceFleck != null)
											{
												FleckMaker.Static(clickCell, pawn.Map, workGiver2.forceFleck);
											}
										}
									};
								}
							}
						}
						if (DebugViewSettings.showFloatMenuWorkGivers)
						{
							text += $" (from {workGiver2.defName})";
						}
						FloatMenuOption menuOption = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, action), pawn, item, "ReservedBy", workGiver_Scanner.GetReservationLayer(pawn, item));
						if (drafted && workGiver2.autoTakeablePriorityDrafted != -1)
						{
							menuOption.autoTakeable = true;
							menuOption.autoTakeablePriority = workGiver2.autoTakeablePriorityDrafted;
						}
						if (opts.Any((FloatMenuOption op) => op.Label == menuOption.Label))
						{
							continue;
						}
						if (workGiver2.equivalenceGroup != null)
						{
							if (equivalenceGroupTempStorage[workGiver2.equivalenceGroup.index] == null || (equivalenceGroupTempStorage[workGiver2.equivalenceGroup.index].Disabled && !menuOption.Disabled))
							{
								equivalenceGroupTempStorage[workGiver2.equivalenceGroup.index] = menuOption;
								flag = true;
							}
						}
						else
						{
							opts.Add(menuOption);
						}
					}
				}
				if (!flag)
				{
					continue;
				}
				for (int j = 0; j < equivalenceGroupTempStorage.Length; j++)
				{
					if (equivalenceGroupTempStorage[j] != null)
					{
						opts.Add(equivalenceGroupTempStorage[j]);
						equivalenceGroupTempStorage[j] = null;
					}
				}
			}
			foreach (WorkTypeDef item3 in DefDatabase<WorkTypeDef>.AllDefsListForReading)
			{
				for (int k = 0; k < item3.workGiversByPriority.Count; k++)
				{
					WorkGiverDef workGiver = item3.workGiversByPriority[k];
					if ((drafted && !workGiver.canBeDoneWhileDrafted) || !(workGiver.Worker is WorkGiver_Scanner workGiver_Scanner2) || !workGiver_Scanner2.def.directOrderable)
					{
						continue;
					}
					JobFailReason.Clear();
					if (!workGiver_Scanner2.PotentialWorkCellsGlobal(pawn).Contains(clickCell) || workGiver_Scanner2.ShouldSkip(pawn, forced: true))
					{
						continue;
					}
					Action action2 = null;
					string label = null;
					PawnCapacityDef pawnCapacityDef2 = workGiver_Scanner2.MissingRequiredCapacity(pawn);
					if (pawnCapacityDef2 != null)
					{
						label = "CannotMissingHealthActivities".Translate(pawnCapacityDef2.label);
					}
					else
					{
						Job job2 = (workGiver_Scanner2.HasJobOnCell(pawn, clickCell, forced: true) ? workGiver_Scanner2.JobOnCell(pawn, clickCell, forced: true) : null);
						if (job2 == null)
						{
							if (JobFailReason.HaveReason)
							{
								if (!JobFailReason.CustomJobString.NullOrEmpty())
								{
									label = "CannotGenericWorkCustom".Translate(JobFailReason.CustomJobString);
								}
								else
								{
									label = "CannotGenericWork".Translate(workGiver_Scanner2.def.verb, "AreaLower".Translate());
								}
								label = label + ": " + JobFailReason.Reason.CapitalizeFirst();
							}
							else
							{
								if (!clickCell.IsForbidden(pawn))
								{
									continue;
								}
								if (!clickCell.InAllowedArea(pawn))
								{
									label = "CannotPrioritizeForbiddenOutsideAllowedArea".Translate() + ": " + pawn.playerSettings.EffectiveAreaRestriction.Label;
								}
								else
								{
									label = "CannotPrioritizeCellForbidden".Translate();
								}
							}
						}
						else
						{
							WorkTypeDef workType2 = workGiver_Scanner2.def.workType;
							if (pawn.jobs.curJob != null && pawn.jobs.curJob.JobIsSameAs(job2))
							{
								label = "CannotGenericAlreadyAmCustom".Translate(workGiver_Scanner2.PostProcessedGerund(job2));
							}
							else if (pawn.workSettings.GetPriority(workType2) == 0)
							{
								if (pawn.WorkTypeIsDisabled(workType2))
								{
									label = "CannotPrioritizeWorkTypeDisabled".Translate(workType2.gerundLabel);
								}
								else if ("CannotPrioritizeNotAssignedToWorkType".CanTranslate())
								{
									label = "CannotPrioritizeNotAssignedToWorkType".Translate(workType2.gerundLabel);
								}
								else
								{
									label = "CannotPrioritizeWorkTypeDisabled".Translate(workType2.pawnLabel);
								}
							}
							else if (clickCell.IsForbidden(pawn))
							{
								if (!clickCell.InAllowedArea(pawn))
								{
									label = "CannotPrioritizeForbiddenOutsideAllowedArea".Translate() + ": " + pawn.playerSettings.EffectiveAreaRestriction.Label;
								}
								else
								{
									label = "CannotPrioritizeCellForbidden".Translate();
								}
							}
							else if (!pawn.CanReach(clickCell, PathEndMode.Touch, Danger.Deadly))
							{
								label = "AreaLower".Translate().CapitalizeFirst() + ": " + "NoPath".Translate().CapitalizeFirst();
							}
							else
							{
								label = "PrioritizeGeneric".Translate(workGiver_Scanner2.PostProcessedGerund(job2), "AreaLower".Translate()).CapitalizeFirst();
								Job localJob = job2;
								WorkGiver_Scanner localScanner = workGiver_Scanner2;
								job2.workGiverDef = workGiver_Scanner2.def;
								action2 = delegate
								{
									if (pawn.jobs.TryTakeOrderedJobPrioritizedWork(localJob, localScanner, clickCell))
									{
										if (workGiver.forceMote != null)
										{
											MoteMaker.MakeStaticMote(clickCell, pawn.Map, workGiver.forceMote);
										}
										if (workGiver.forceFleck != null)
										{
											FleckMaker.Static(clickCell, pawn.Map, workGiver.forceFleck);
										}
									}
								};
							}
						}
					}
					if (!opts.Any((FloatMenuOption op) => op.Label == label.TrimEnd()))
					{
						FloatMenuOption floatMenuOption = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action2), pawn, clickCell);
						if (drafted && workGiver.autoTakeablePriorityDrafted != -1)
						{
							floatMenuOption.autoTakeable = true;
							floatMenuOption.autoTakeablePriority = workGiver.autoTakeablePriorityDrafted;
						}
						opts.Add(floatMenuOption);
					}
				}
			}
		}

		private static FloatMenuOption GotoLocationOption(IntVec3 clickCell, Pawn pawn, bool suppressAutoTakeableGoto)
		{
			if (suppressAutoTakeableGoto)
			{
				return null;
			}
			IntVec3 curLoc = CellFinder.StandableCellNear(clickCell, pawn.Map, 2.9f);
			if (curLoc.IsValid && curLoc != pawn.Position)
			{
				if (ModsConfig.BiotechActive && pawn.IsColonyMech && !MechanitorUtility.InMechanitorCommandRange(pawn, curLoc))
				{
					return new FloatMenuOption("CannotGoOutOfRange".Translate() + ": " + "OutOfCommandRange".Translate(), null);
				}
				if (!pawn.CanReach(curLoc, PathEndMode.OnCell, Danger.Deadly))
				{
					return new FloatMenuOption("CannotGoNoPath".Translate(), null);
				}
				Action action = delegate
				{
					PawnGotoAction(clickCell, pawn, RCellFinder.BestOrderedGotoDestNear(curLoc, pawn));
				};
				return new FloatMenuOption("GoHere".Translate(), action, MenuOptionPriority.GoHere)
				{
					autoTakeable = true,
					autoTakeablePriority = 10f
				};
			}
			return null;
		}

		public static void PawnGotoAction(IntVec3 clickCell, Pawn pawn, IntVec3 gotoLoc)
		{
			bool flag;
			if (pawn.Position == gotoLoc || (pawn.CurJobDef == JobDefOf.Goto && pawn.CurJob.targetA.Cell == gotoLoc))
			{
				flag = true;
			}
			else
			{
				Job job = JobMaker.MakeJob(JobDefOf.Goto, gotoLoc);
				if (pawn.Map.exitMapGrid.IsExitCell(clickCell))
				{
					job.exitMapOnArrival = !pawn.IsColonyMech;
				}
				else if (!pawn.Map.IsPlayerHome && !pawn.Map.exitMapGrid.MapUsesExitGrid && CellRect.WholeMap(pawn.Map).IsOnEdge(clickCell, 3) && pawn.Map.Parent.GetComponent<FormCaravanComp>() != null && MessagesRepeatAvoider.MessageShowAllowed("MessagePlayerTriedToLeaveMapViaExitGrid-" + pawn.Map.uniqueID, 60f))
				{
					if (pawn.Map.Parent.GetComponent<FormCaravanComp>().CanFormOrReformCaravanNow)
					{
						Messages.Message("MessagePlayerTriedToLeaveMapViaExitGrid_CanReform".Translate(), pawn.Map.Parent, MessageTypeDefOf.RejectInput, historical: false);
					}
					else
					{
						Messages.Message("MessagePlayerTriedToLeaveMapViaExitGrid_CantReform".Translate(), pawn.Map.Parent, MessageTypeDefOf.RejectInput, historical: false);
					}
				}
				flag = pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
			}
			if (flag)
			{
				FleckMaker.Static(gotoLoc, pawn.Map, FleckDefOf.FeedbackGoto);
			}
		}
	}
}
