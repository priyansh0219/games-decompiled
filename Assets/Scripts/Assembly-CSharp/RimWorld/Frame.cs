using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class Frame : Building, IThingHolder, IConstructible, IStorageGroupMember, IStoreSettingsParent
	{
		public ThingOwner resourceContainer;

		public float workDone;

		public ColorInt? glowerColorOverride;

		public StorageGroup storageGroup;

		public StorageSettings storageSettings;

		private Material cachedCornerMat;

		private Material cachedTileMat;

		protected const float UnderfieldOverdrawFactor = 1.15f;

		protected const float CenterOverdrawFactor = 0.5f;

		private const int LongConstructionProjectThreshold = 9500;

		private static readonly Material UnderfieldMat = MaterialPool.MatFrom("Things/Building/BuildingFrame/Underfield", ShaderDatabase.Transparent);

		private static readonly Texture2D CornerTex = ContentFinder<Texture2D>.Get("Things/Building/BuildingFrame/Corner");

		private static readonly Texture2D TileTex = ContentFinder<Texture2D>.Get("Things/Building/BuildingFrame/Tile");

		[TweakValue("Pathfinding", 0f, 1000f)]
		public static ushort AvoidUnderConstructionPathFindCost = 800;

		private List<ThingDefCountClass> cachedMaterialsNeeded = new List<ThingDefCountClass>();

		private ThingDef BuildDef => def.entityDefToBuild as ThingDef;

		public float WorkToBuild => def.entityDefToBuild.GetStatValueAbstract(StatDefOf.WorkToBuild, base.Stuff);

		public float WorkLeft => WorkToBuild - workDone;

		public float PercentComplete => workDone / WorkToBuild;

		public override string Label => LabelEntityToBuild + "FrameLabelExtra".Translate();

		public string LabelEntityToBuild
		{
			get
			{
				string text = def.entityDefToBuild.label;
				if (base.StyleSourcePrecept != null)
				{
					text = base.StyleSourcePrecept.TransformThingLabel(text);
				}
				if (base.Stuff != null)
				{
					return "ThingMadeOfStuffLabel".Translate(base.Stuff.LabelAsStuff, text);
				}
				return text;
			}
		}

		public override Color DrawColor
		{
			get
			{
				if (!def.MadeFromStuff)
				{
					List<ThingDefCountClass> costList = def.entityDefToBuild.CostList;
					if (costList != null)
					{
						for (int i = 0; i < costList.Count; i++)
						{
							ThingDef thingDef = costList[i].thingDef;
							if (thingDef.IsStuff && thingDef.stuffProps.color != Color.white)
							{
								return def.GetColorForStuff(thingDef);
							}
						}
					}
					return new Color(0.6f, 0.6f, 0.6f);
				}
				return base.DrawColor;
			}
		}

		public EffecterDef ConstructionEffect
		{
			get
			{
				if (base.Stuff != null && base.Stuff.stuffProps.constructEffect != null)
				{
					return base.Stuff.stuffProps.constructEffect;
				}
				if (def.entityDefToBuild.constructEffect != null)
				{
					return def.entityDefToBuild.constructEffect;
				}
				return EffecterDefOf.ConstructMetal;
			}
		}

		private Material CornerMat
		{
			get
			{
				if (cachedCornerMat == null)
				{
					cachedCornerMat = MaterialPool.MatFrom(CornerTex, ShaderDatabase.Cutout, DrawColor);
				}
				return cachedCornerMat;
			}
		}

		private Material TileMat
		{
			get
			{
				if (cachedTileMat == null)
				{
					cachedTileMat = MaterialPool.MatFrom(TileTex, ShaderDatabase.Cutout, DrawColor);
				}
				return cachedTileMat;
			}
		}

		StorageGroup IStorageGroupMember.Group
		{
			get
			{
				return storageGroup;
			}
			set
			{
				storageGroup = value;
			}
		}

		Map IStorageGroupMember.Map => base.Map;

		StorageSettings IStorageGroupMember.StoreSettings => GetStoreSettings();

		StorageSettings IStorageGroupMember.ParentStoreSettings => GetParentStoreSettings();

		StorageSettings IStorageGroupMember.ThingStoreSettings => storageSettings;

		public string StorageGroupTag => BuildDef?.building?.storageGroupTag;

		bool IStorageGroupMember.DrawConnectionOverlay => !StorageGroupTag.NullOrEmpty();

		bool IStorageGroupMember.DrawStorageTab => !StorageGroupTag.NullOrEmpty();

		bool IStoreSettingsParent.StorageTabVisible => !StorageGroupTag.NullOrEmpty();

		public Frame()
		{
			resourceContainer = new ThingOwner<Thing>(this, oneStackOnly: false);
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return resourceContainer;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref workDone, "workDone", 0f);
			Scribe_Deep.Look(ref resourceContainer, "resourceContainer", this);
			Scribe_Values.Look(ref glowerColorOverride, "glowerColorOverride");
			Scribe_References.Look(ref storageGroup, "storageGroup");
			Scribe_Deep.Look(ref storageSettings, "storageSettings");
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			bool spawned = base.Spawned;
			Map map = base.Map;
			base.Destroy(mode);
			if (spawned)
			{
				ThingUtility.CheckAutoRebuildOnDestroyed(this, mode, map, def.entityDefToBuild);
			}
		}

		public ThingDef EntityToBuildStuff()
		{
			return base.Stuff;
		}

		public List<ThingDefCountClass> MaterialsNeeded()
		{
			cachedMaterialsNeeded.Clear();
			List<ThingDefCountClass> list = def.entityDefToBuild.CostListAdjusted(base.Stuff);
			for (int i = 0; i < list.Count; i++)
			{
				ThingDefCountClass thingDefCountClass = list[i];
				int num = resourceContainer.TotalStackCountOfDef(thingDefCountClass.thingDef);
				int num2 = thingDefCountClass.count - num;
				if (num2 > 0)
				{
					cachedMaterialsNeeded.Add(new ThingDefCountClass(thingDefCountClass.thingDef, num2));
				}
			}
			return cachedMaterialsNeeded;
		}

		public void CompleteConstruction(Pawn worker)
		{
			if (worker.Faction != null)
			{
				QuestUtility.SendQuestTargetSignals(worker.Faction.questTags, "BuiltBuilding", this.Named("SUBJECT"));
			}
			List<CompHasSources> list = new List<CompHasSources>();
			for (int i = 0; i < resourceContainer.Count; i++)
			{
				CompHasSources compHasSources = resourceContainer[i].TryGetComp<CompHasSources>();
				if (compHasSources != null)
				{
					list.Add(compHasSources);
				}
			}
			resourceContainer.ClearAndDestroyContents();
			Map map = base.Map;
			bool flag = Find.Selector.IsSelected(this);
			Destroy();
			if (this.GetStatValue(StatDefOf.WorkToBuild) > 150f && def.entityDefToBuild is ThingDef && ((ThingDef)def.entityDefToBuild).category == ThingCategory.Building)
			{
				SoundDefOf.Building_Complete.PlayOneShot(new TargetInfo(base.Position, map));
			}
			ThingDef thingDef = def.entityDefToBuild as ThingDef;
			Thing thing = null;
			if (thingDef != null)
			{
				thing = ThingMaker.MakeThing(thingDef, base.Stuff);
				thing.SetFactionDirect(base.Faction);
				CompQuality compQuality = thing.TryGetComp<CompQuality>();
				if (compQuality != null)
				{
					QualityCategory q = QualityUtility.GenerateQualityCreatedByPawn(worker, SkillDefOf.Construction);
					compQuality.SetQuality(q, ArtGenerationContext.Colony);
					QualityUtility.SendCraftNotification(thing, worker);
				}
				CompArt compArt = thing.TryGetComp<CompArt>();
				if (compArt != null)
				{
					if (compQuality == null)
					{
						compArt.InitializeArt(ArtGenerationContext.Colony);
					}
					compArt.JustCreatedBy(worker);
				}
				CompHasSources compHasSources2 = thing.TryGetComp<CompHasSources>();
				if (compHasSources2 != null && !list.NullOrEmpty())
				{
					for (int j = 0; j < list.Count; j++)
					{
						list[j].TransferSourcesTo(compHasSources2);
					}
				}
				if (GetIdeoForStyle(worker) != null)
				{
					thing.StyleDef = base.StyleDef;
				}
				thing.HitPoints = Mathf.CeilToInt((float)HitPoints / (float)base.MaxHitPoints * (float)thing.MaxHitPoints);
				GenSpawn.Spawn(thing, base.Position, map, base.Rotation, WipeMode.FullRefund);
				if (thing is Building building)
				{
					worker.GetLord()?.AddBuilding(building);
					building.StyleSourcePrecept = base.StyleSourcePrecept;
				}
				if (thing is IStorageGroupMember member)
				{
					member.SetStorageGroup(storageGroup);
				}
				if (thing is Building_Storage building_Storage && storageSettings != null)
				{
					building_Storage.settings.CopyFrom(storageSettings);
				}
				this.SetStorageGroup(null);
				if (thingDef != null)
				{
					Color? ideoColorForBuilding = IdeoUtility.GetIdeoColorForBuilding(thingDef, base.Faction);
					if (ideoColorForBuilding.HasValue)
					{
						thing.SetColor(ideoColorForBuilding.Value);
					}
				}
			}
			else
			{
				map.terrainGrid.SetTerrain(base.Position, (TerrainDef)def.entityDefToBuild);
				FilthMaker.RemoveAllFilth(base.Position, map);
			}
			worker.records.Increment(RecordDefOf.ThingsConstructed);
			if (thing != null && thing.GetStatValue(StatDefOf.WorkToBuild) >= 9500f)
			{
				TaleRecorder.RecordTale(TaleDefOf.CompletedLongConstructionProject, worker, thing.def);
			}
			if (thing != null && flag)
			{
				Find.Selector.Select(thing, playSound: false, forceDesignatorDeselect: false);
			}
			CompGlower compGlower;
			if (glowerColorOverride.HasValue && (compGlower = thing?.TryGetComp<CompGlower>()) != null)
			{
				compGlower.GlowColor = glowerColorOverride.Value;
			}
		}

		private Ideo GetIdeoForStyle(Pawn worker)
		{
			if (worker.Ideo != null)
			{
				return worker.Ideo;
			}
			if (ModsConfig.BiotechActive && worker.IsColonyMech)
			{
				Pawn overseer = worker.GetOverseer();
				if (overseer?.Ideo != null)
				{
					return overseer.Ideo;
				}
			}
			return null;
		}

		public void FailConstruction(Pawn worker)
		{
			Map map = base.Map;
			Destroy(DestroyMode.FailConstruction);
			Blueprint_Build blueprint_Build = null;
			if (def.entityDefToBuild.blueprintDef != null)
			{
				blueprint_Build = (Blueprint_Build)ThingMaker.MakeThing(def.entityDefToBuild.blueprintDef);
				blueprint_Build.stuffToUse = base.Stuff;
				blueprint_Build.SetFactionDirect(base.Faction);
				blueprint_Build.InheritStyle(base.StyleSourcePrecept, base.StyleDef);
				if (blueprint_Build is IStorageGroupMember storageGroupMember)
				{
					storageGroupMember.Group = storageGroup;
				}
				if (blueprint_Build is Blueprint_Storage blueprint_Storage && storageSettings != null)
				{
					blueprint_Storage.settings.CopyFrom(storageSettings);
				}
				GenSpawn.Spawn(blueprint_Build, base.Position, map, base.Rotation, WipeMode.FullRefund);
			}
			worker.GetLord()?.Notify_ConstructionFailed(worker, this, blueprint_Build);
			MoteMaker.ThrowText(DrawPos, map, "TextMote_ConstructionFail".Translate(), 6f);
			if (base.Faction == Faction.OfPlayer && WorkToBuild > 1400f)
			{
				Messages.Message("MessageConstructionFailed".Translate(LabelEntityToBuild, worker.LabelShort, worker.Named("WORKER")), new TargetInfo(base.Position, map), MessageTypeDefOf.NegativeEvent);
			}
		}

		public override void Draw()
		{
			Vector2 vector = new Vector2(def.size.x, def.size.z);
			vector.x *= 1.15f;
			vector.y *= 1.15f;
			Vector3 s = new Vector3(vector.x, 1f, vector.y);
			Matrix4x4 matrix = default(Matrix4x4);
			matrix.SetTRS(DrawPos, base.Rotation.AsQuat, s);
			Graphics.DrawMesh(MeshPool.plane10, matrix, UnderfieldMat, 0);
			int num = 4;
			for (int i = 0; i < num; i++)
			{
				float num2 = (float)Mathf.Min(base.RotatedSize.x, base.RotatedSize.z) * 0.38f;
				IntVec3 intVec = default(IntVec3);
				switch (i)
				{
				case 0:
					intVec = new IntVec3(-1, 0, -1);
					break;
				case 1:
					intVec = new IntVec3(-1, 0, 1);
					break;
				case 2:
					intVec = new IntVec3(1, 0, 1);
					break;
				case 3:
					intVec = new IntVec3(1, 0, -1);
					break;
				}
				Vector3 vector2 = default(Vector3);
				vector2.x = (float)intVec.x * ((float)base.RotatedSize.x / 2f - num2 / 2f);
				vector2.z = (float)intVec.z * ((float)base.RotatedSize.z / 2f - num2 / 2f);
				Vector3 s2 = new Vector3(num2, 1f, num2);
				Matrix4x4 matrix2 = default(Matrix4x4);
				matrix2.SetTRS(DrawPos + Vector3.up * 0.03f + vector2, new Rot4(i).AsQuat, s2);
				Graphics.DrawMesh(MeshPool.plane10, matrix2, CornerMat, 0);
			}
			int num3 = Mathf.CeilToInt((PercentComplete - 0f) / 1f * (float)base.RotatedSize.x * (float)base.RotatedSize.z * 4f);
			IntVec2 intVec2 = base.RotatedSize * 2;
			for (int j = 0; j < num3; j++)
			{
				IntVec2 intVec3 = default(IntVec2);
				intVec3.z = j / intVec2.x;
				intVec3.x = j - intVec3.z * intVec2.x;
				Vector3 vector3 = new Vector3((float)intVec3.x * 0.5f, 0f, (float)intVec3.z * 0.5f) + DrawPos;
				vector3.x -= (float)base.RotatedSize.x * 0.5f - 0.25f;
				vector3.z -= (float)base.RotatedSize.z * 0.5f - 0.25f;
				Vector3 s3 = new Vector3(0.5f, 1f, 0.5f);
				Matrix4x4 matrix3 = default(Matrix4x4);
				matrix3.SetTRS(vector3 + Vector3.up * 0.02f, Quaternion.identity, s3);
				Graphics.DrawMesh(MeshPool.plane10, matrix3, TileMat, 0);
			}
			Comps_PostDraw();
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			Gizmo selectMonumentMarkerGizmo = QuestUtility.GetSelectMonumentMarkerGizmo(this);
			if (selectMonumentMarkerGizmo != null)
			{
				yield return selectMonumentMarkerGizmo;
			}
			Command command = BuildCopyCommandUtility.BuildCopyCommand(def.entityDefToBuild, base.Stuff, base.StyleSourcePrecept as Precept_Building, base.StyleDef, styleOverridden: true, glowerColorOverride);
			if (command != null)
			{
				yield return command;
			}
			if (base.Faction != Faction.OfPlayer)
			{
				yield break;
			}
			foreach (Command item in BuildRelatedCommandUtility.RelatedBuildCommands(def.entityDefToBuild))
			{
				yield return item;
			}
			if (StorageGroupTag.NullOrEmpty())
			{
				yield break;
			}
			foreach (Gizmo item2 in StorageGroupUtility.StorageGroupMemberGizmos(this))
			{
				yield return item2;
			}
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
			stringBuilder.AppendLineIfNotEmpty();
			stringBuilder.AppendLine("ContainedResources".Translate() + ":");
			List<ThingDefCountClass> list = def.entityDefToBuild.CostListAdjusted(base.Stuff);
			for (int i = 0; i < list.Count; i++)
			{
				ThingDefCountClass need = list[i];
				int num = need.count;
				foreach (ThingDefCountClass item in from needed in MaterialsNeeded()
					where needed.thingDef == need.thingDef
					select needed)
				{
					num -= item.count;
				}
				stringBuilder.AppendLine((string)(need.thingDef.LabelCap + ": ") + num + " / " + need.count);
			}
			stringBuilder.Append("WorkLeft".Translate() + ": " + WorkLeft.ToStringWorkAmount());
			if (base.StyleDef?.Category != null && base.StyleSourcePrecept == null)
			{
				stringBuilder.AppendInNewLine("Style".Translate() + ": " + base.StyleDef.Category.LabelCap);
			}
			return stringBuilder.ToString();
		}

		public override ushort PathFindCostFor(Pawn p)
		{
			if (base.Faction == null)
			{
				return 0;
			}
			if (def.entityDefToBuild is TerrainDef)
			{
				return 0;
			}
			if (p.Faction == base.Faction || p.HostFaction == base.Faction)
			{
				return AvoidUnderConstructionPathFindCost;
			}
			return 0;
		}

		public StorageSettings GetStoreSettings()
		{
			if (storageGroup != null)
			{
				return storageGroup.GetStoreSettings();
			}
			return storageSettings;
		}

		public StorageSettings GetParentStoreSettings()
		{
			return BuildDef?.building?.fixedStorageSettings;
		}

		void IStoreSettingsParent.Notify_SettingsChanged()
		{
		}

		public override IEnumerable<InspectTabBase> GetInspectTabs()
		{
			if (StorageGroupTag.NullOrEmpty() || BuildDef.inspectorTabsResolved == null)
			{
				yield break;
			}
			foreach (InspectTabBase item in BuildDef.inspectorTabsResolved)
			{
				yield return item;
			}
		}
	}
}
