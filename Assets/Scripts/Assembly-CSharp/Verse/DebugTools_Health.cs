using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public static class DebugTools_Health
	{
		public static List<DebugMenuOption> Options_RestorePart(Pawn p)
		{
			if (p == null)
			{
				throw new ArgumentNullException("p");
			}
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (BodyPartRecord notMissingPart in p.health.hediffSet.GetNotMissingParts())
			{
				BodyPartRecord localPart = notMissingPart;
				list.Add(new DebugMenuOption(localPart.LabelCap, DebugMenuOptionMode.Action, delegate
				{
					p.health.RestorePart(localPart);
				}));
			}
			return list;
		}

		public static List<DebugActionNode> Options_ApplyDamage()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			foreach (DamageDef allDef in DefDatabase<DamageDef>.AllDefs)
			{
				DamageDef localDef = allDef;
				list.Add(new DebugActionNode(localDef.defName)
				{
					actionType = DebugActionType.ToolMap,
					action = delegate
					{
						Pawn pawn = Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).OfType<Pawn>().FirstOrDefault();
						if (pawn != null)
						{
							Find.WindowStack.Add(new Dialog_DebugOptionListLister(Options_Damage_BodyParts(pawn, localDef)));
						}
					}
				});
			}
			return list;
		}

		private static List<DebugMenuOption> Options_Damage_BodyParts(Pawn p, DamageDef def)
		{
			if (p == null)
			{
				throw new ArgumentNullException("p");
			}
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			list.Add(new DebugMenuOption("(no body part)", DebugMenuOptionMode.Action, delegate
			{
				p.TakeDamage(new DamageInfo(def, 5f));
			}));
			foreach (BodyPartRecord allPart in p.RaceProps.body.AllParts)
			{
				BodyPartRecord localPart = allPart;
				list.Add(new DebugMenuOption(localPart.LabelCap, DebugMenuOptionMode.Action, delegate
				{
					p.TakeDamage(new DamageInfo(def, 5f, 0f, -1f, null, localPart));
				}));
			}
			return list;
		}

		public static List<DebugMenuOption> Options_AddHediff(Pawn pawn)
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (HediffDef item in DefDatabase<HediffDef>.AllDefs.OrderBy((HediffDef d) => d.hediffClass.ToStringSafe()))
			{
				HediffDef localDef = item;
				DebugMenuOptionMode mode = ((pawn == null) ? DebugMenuOptionMode.Tool : DebugMenuOptionMode.Action);
				list.Add(new DebugMenuOption(localDef.LabelCap, mode, delegate
				{
					Pawn pawn2 = ((mode != DebugMenuOptionMode.Tool) ? pawn : (from t in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell())
						where t is Pawn
						select t).Cast<Pawn>().FirstOrDefault());
					if (pawn2 != null)
					{
						Find.WindowStack.Add(new Dialog_DebugOptionListLister(Options_Hediff_BodyParts(pawn2, localDef)));
					}
				}));
			}
			return list;
		}

		private static List<DebugMenuOption> Options_Hediff_BodyParts(Pawn p, HediffDef def)
		{
			if (p == null)
			{
				throw new ArgumentNullException("p");
			}
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			list.Add(new DebugMenuOption("(no body part)", DebugMenuOptionMode.Action, delegate
			{
				p.health.AddHediff(def).PostDebugAdd();
			}));
			foreach (BodyPartRecord item in p.RaceProps.body.AllParts.OrderBy((BodyPartRecord pa) => pa.Label))
			{
				BodyPartRecord localPart = item;
				list.Add(new DebugMenuOption(localPart.LabelCap, DebugMenuOptionMode.Action, delegate
				{
					p.health.AddHediff(def, localPart).PostDebugAdd();
				}));
			}
			return list;
		}

		public static List<DebugMenuOption> Options_RemoveHediff(Pawn pawn)
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
			{
				Hediff localH = hediff;
				list.Add(new DebugMenuOption(localH.LabelCap + ((localH.Part != null) ? string.Concat(" (", localH.Part.def, ")") : ""), DebugMenuOptionMode.Action, delegate
				{
					pawn.health.RemoveHediff(localH);
				}));
			}
			return list;
		}
	}
}
