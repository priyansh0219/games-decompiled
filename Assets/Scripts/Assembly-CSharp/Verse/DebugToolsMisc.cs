using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;

namespace Verse
{
	public static class DebugToolsMisc
	{
		[DebugAction("General", null, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void AttachFire()
		{
			foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
			{
				item.TryAttachFire(1f);
			}
		}

		[DebugAction("General", null, false, false, false, 0, false, actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static DebugActionNode SetQuality()
		{
			DebugActionNode debugActionNode = new DebugActionNode();
			foreach (QualityCategory value in Enum.GetValues(typeof(QualityCategory)))
			{
				QualityCategory qualityInner = value;
				debugActionNode.AddChild(new DebugActionNode(qualityInner.ToString(), DebugActionType.ToolMap, delegate
				{
					foreach (Thing thing in UI.MouseCell().GetThingList(Find.CurrentMap))
					{
						thing.TryGetComp<CompQuality>()?.SetQuality(qualityInner, ArtGenerationContext.Outsider);
					}
				}));
			}
			return debugActionNode;
		}

		[DebugAction("General", null, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.IsCurrentlyOnMap)]
		public static void MeasureDrawSize()
		{
			Vector3 first;
			DebugTools.curMeasureTool = new DrawMeasureTool("first corner...", delegate
			{
				first = UI.MouseMapPosition();
				DebugTools.curMeasureTool = new DrawMeasureTool("second corner...", delegate
				{
					Vector3 vector = UI.MouseMapPosition();
					Rect rect = default(Rect);
					rect.xMin = Mathf.Min(first.x, vector.x);
					rect.yMin = Mathf.Min(first.z, vector.z);
					rect.xMax = Mathf.Max(first.x, vector.x);
					rect.yMax = Mathf.Max(first.z, vector.z);
					string text = $"Center: ({rect.center.x},{rect.center.y})";
					text += $"\nSize: ({rect.size.x},{rect.size.y})";
					if (Find.Selector.SingleSelectedObject != null)
					{
						Thing singleSelectedThing = Find.Selector.SingleSelectedThing;
						Vector3 drawPos = singleSelectedThing.DrawPos;
						Vector2 v = rect.center - new Vector2(drawPos.x, drawPos.z);
						text += $"\nOffset: ({v.x},{v.y})";
						Vector2 vector2 = v.RotatedBy(0f - singleSelectedThing.Rotation.AsAngle);
						text += $"\nUnrotated offset: ({vector2.x},{vector2.y})";
					}
					Log.Message(text);
					MeasureDrawSize();
				}, first);
			});
		}

		[DebugAction("General", "Pollution +1%", false, false, false, 0, false, actionType = DebugActionType.ToolWorld, allowedGameStates = AllowedGameStates.PlayingOnWorld, requiresBiotech = true)]
		private static void IncreasePollutionSmall()
		{
			int num = GenWorld.MouseTile();
			if (num >= 0)
			{
				WorldPollutionUtility.PolluteWorldAtTile(num, 0.01f);
			}
		}

		[DebugAction("General", "Pollution +25%", false, false, false, 0, false, actionType = DebugActionType.ToolWorld, allowedGameStates = AllowedGameStates.PlayingOnWorld, requiresBiotech = true)]
		private static void IncreasePollutionLarge()
		{
			int num = GenWorld.MouseTile();
			if (num >= 0)
			{
				WorldPollutionUtility.PolluteWorldAtTile(num, 0.25f);
			}
		}

		[DebugAction("General", "Pollution -25%", false, false, false, 0, false, actionType = DebugActionType.ToolWorld, allowedGameStates = AllowedGameStates.PlayingOnWorld, requiresBiotech = true)]
		private static void DecreasePollutionLarge()
		{
			int num = GenWorld.MouseTile();
			if (num >= 0)
			{
				WorldPollutionUtility.PolluteWorldAtTile(num, -0.25f);
			}
		}

		[DebugAction("General", null, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.IsCurrentlyOnMap, requiresBiotech = true)]
		private static void ResetBossgroupCooldown()
		{
			Find.BossgroupManager.lastBossgroupCalled = Find.TickManager.TicksGame - 120000;
		}

		[DebugAction("General", null, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.IsCurrentlyOnMap, requiresBiotech = true)]
		private static void ResetBossgroupKilledPawns()
		{
			Find.BossgroupManager.DebugResetDefeatedPawns();
		}

		[DebugAction("Insect", "Spawn cocoon infestation", false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.IsCurrentlyOnMap, hideInSubMenu = true, requiresBiotech = true)]
		private static List<DebugActionNode> SpawnCocoonInfestationWithPoints()
		{
			List<DebugActionNode> list = new List<DebugActionNode>();
			foreach (float item2 in DebugActionsUtility.PointsOptions(extended: false))
			{
				float localP = item2;
				DebugActionNode item = new DebugActionNode(localP + " points", DebugActionType.ToolMap, delegate
				{
					CocoonInfestationUtility.SpawnCocoonInfestation(UI.MouseCell(), Find.CurrentMap, localP);
				});
				list.Add(item);
			}
			return list;
		}

		[DebugAction("General", null, false, false, false, 0, false, actionType = DebugActionType.Action)]
		private static void BenchmarkPerformance()
		{
			Messages.Message($"Running benchmark, results displayed in {30f} seconds", MessageTypeDefOf.NeutralEvent, historical: false);
			PerformanceBenchmarkUtility.StartBenchmark();
		}
	}
}
