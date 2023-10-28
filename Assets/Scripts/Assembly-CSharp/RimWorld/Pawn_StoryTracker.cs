using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Pawn_StoryTracker : IExposable
	{
		private Pawn pawn;

		private BackstoryDef childhood;

		private BackstoryDef adulthood;

		private Color hairColor = Color.white;

		public Color? skinColorOverride;

		public HeadTypeDef headType;

		public BodyTypeDef bodyType;

		public HairDef hairDef;

		public TraitSet traits;

		public string title;

		public string birthLastName;

		public Color? favoriteColor;

		public FurDef furDef;

		[Unsaved(false)]
		private Color? skinColorBase;

		[Unsaved(false)]
		private float melanin = -1f;

		[Unsaved(false)]
		private List<BackstoryDef> backstoriesCache;

		[Unsaved(false)]
		private string bodyNakedGraphicPathCache;

		[Unsaved(false)]
		private string furGraphicPath;

		private static readonly FloatRange VoicePitchFactorRange = new FloatRange(0.85f, 1.15f);

		public BackstoryDef Childhood
		{
			get
			{
				return childhood;
			}
			set
			{
				if (pawn.DevelopmentalStage.Adult() && value.spawnCategories.Contains("Child"))
				{
					Log.Warning($"Assigning active child backstory to adult pawn {pawn}.");
				}
				backstoriesCache = null;
				childhood = value;
			}
		}

		public Color HairColor
		{
			get
			{
				return hairColor;
			}
			set
			{
				hairColor = value;
				if (ModsConfig.BiotechActive && pawn.genes != null && pawn.genes.GenesListForReading.Any((Gene x) => x.def.graphicData != null && x.def.graphicData.skinIsHairColor && x.Active))
				{
					skinColorOverride = hairColor;
				}
			}
		}

		public BackstoryDef Adulthood
		{
			get
			{
				return adulthood;
			}
			set
			{
				backstoriesCache = null;
				adulthood = value;
			}
		}

		public string Title
		{
			get
			{
				if (title != null)
				{
					return title;
				}
				return TitleDefault;
			}
			set
			{
				title = null;
				if (value != Title && !value.NullOrEmpty())
				{
					title = value;
				}
			}
		}

		public string TitleCap => Title.CapitalizeFirst();

		public string TitleDefault
		{
			get
			{
				if (adulthood != null)
				{
					return adulthood.TitleFor(pawn.gender);
				}
				if (childhood != null)
				{
					return childhood.TitleFor(pawn.gender);
				}
				return "";
			}
		}

		public string TitleDefaultCap => TitleDefault.CapitalizeFirst();

		public string TitleShort
		{
			get
			{
				if (title != null)
				{
					return title;
				}
				if (adulthood != null)
				{
					return adulthood.TitleShortFor(pawn.gender);
				}
				if (childhood != null)
				{
					return childhood.TitleShortFor(pawn.gender);
				}
				return "";
			}
		}

		public string TitleShortCap => TitleShort.CapitalizeFirst();

		public Color SkinColorBase
		{
			get
			{
				if (!skinColorBase.HasValue && pawn.genes != null)
				{
					GeneDef melaninGene = pawn.genes.GetMelaninGene();
					if (melaninGene != null)
					{
						skinColorBase = melaninGene.skinColorBase;
					}
					else
					{
						GeneDef geneDef = PawnSkinColors.RandomSkinColorGene(pawn);
						if (geneDef != null)
						{
							pawn.genes.AddGene(geneDef, xenogene: false);
						}
					}
				}
				return skinColorBase.Value;
			}
			set
			{
				skinColorBase = value;
			}
		}

		public Color SkinColor => skinColorOverride ?? SkinColorBase;

		public bool SkinColorOverriden => skinColorOverride.HasValue;

		public List<BackstoryDef> AllBackstories
		{
			get
			{
				if (backstoriesCache == null)
				{
					backstoriesCache = new List<BackstoryDef>();
					if (childhood != null)
					{
						backstoriesCache.Add(childhood);
					}
					if (adulthood != null)
					{
						backstoriesCache.Add(adulthood);
					}
				}
				return backstoriesCache;
			}
		}

		public WorkTags DisabledWorkTagsBackstoryAndTraits
		{
			get
			{
				WorkTags workTags = WorkTags.None;
				if (childhood != null)
				{
					workTags |= childhood.workDisables;
				}
				if (adulthood != null)
				{
					workTags |= adulthood.workDisables;
				}
				for (int i = 0; i < traits.allTraits.Count; i++)
				{
					if (!traits.allTraits[i].Suppressed)
					{
						workTags |= traits.allTraits[i].def.disabledWorkTags;
					}
				}
				return workTags;
			}
		}

		public WorkTags DisabledWorkTagsBackstoryTraitsAndGenes
		{
			get
			{
				WorkTags workTags = DisabledWorkTagsBackstoryAndTraits;
				if (pawn.genes != null)
				{
					workTags |= pawn.genes.DisabledWorkTags;
				}
				return workTags;
			}
		}

		public float VoicePitchFactor => VoicePitchFactorRange.RandomInRangeSeeded(pawn.thingIDNumber);

		public Pawn_StoryTracker(Pawn pawn)
		{
			this.pawn = pawn;
			traits = new TraitSet(pawn);
		}

		public void ExposeData()
		{
			Scribe_Defs.Look(ref bodyType, "bodyType");
			Scribe_Defs.Look(ref hairDef, "hairDef");
			Scribe_Values.Look(ref hairColor, "hairColor");
			Scribe_Values.Look(ref skinColorOverride, "skinColorOverride");
			Scribe_Deep.Look(ref traits, "traits", pawn);
			Scribe_Values.Look(ref title, "title");
			Scribe_Values.Look(ref birthLastName, "birthLastName");
			Scribe_Values.Look(ref favoriteColor, "favoriteColor");
			Scribe_Defs.Look(ref furDef, "furDef");
			Scribe_Defs.Look(ref headType, "headType");
			if (Scribe.mode != LoadSaveMode.Saving)
			{
				Scribe_Values.Look(ref melanin, "melanin", -1f);
				if (Scribe.mode == LoadSaveMode.PostLoadInit && melanin >= 0f)
				{
					pawn.genes.InitializeGenesFromOldSave(melanin);
				}
			}
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				string headGraphicPath = null;
				Scribe_Values.Look(ref headGraphicPath, "headGraphicPath");
				if (!headGraphicPath.NullOrEmpty())
				{
					TryGetRandomHeadFromSet(DefDatabase<HeadTypeDef>.AllDefs.Where((HeadTypeDef x) => x.graphicPath == headGraphicPath));
				}
			}
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (birthLastName == null && pawn.Name is NameTriple)
				{
					birthLastName = ((NameTriple)pawn.Name).Last;
				}
				if (hairDef == null)
				{
					hairDef = DefDatabase<HairDef>.AllDefs.RandomElement();
				}
				if (headType == null)
				{
					TryGetRandomHeadFromSet(DefDatabase<HeadTypeDef>.AllDefs.Where((HeadTypeDef x) => x.randomChosen));
				}
				if (ModsConfig.BiotechActive && pawn.DevelopmentalStage.Child() && bodyType != BodyTypeDefOf.Child)
				{
					Log.Warning(string.Concat(pawn, " had body type set to ", bodyType?.label, " as a child."));
					bodyType = BodyTypeDefOf.Child;
				}
			}
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				string childDefName = null;
				Scribe_Values.Look(ref childDefName, "childhood");
				if (!childDefName.NullOrEmpty())
				{
					childhood = DefDatabase<BackstoryDef>.AllDefs.FirstOrDefault((BackstoryDef b) => b.defName == childDefName);
					if (childhood == null)
					{
						Log.Error($"Couldn't load child backstory {childDefName} for {pawn.Name}. Giving random.");
						childhood = DefDatabase<BackstoryDef>.AllDefs.Where((BackstoryDef bs) => bs.slot == BackstorySlot.Childhood).RandomElement();
					}
				}
				string adultDefName = null;
				Scribe_Values.Look(ref adultDefName, "adulthood");
				if (adultDefName.NullOrEmpty())
				{
					return;
				}
				adulthood = DefDatabase<BackstoryDef>.AllDefs.FirstOrDefault((BackstoryDef b) => b.defName == adultDefName);
				if (adulthood == null)
				{
					Log.Error($"Couldn't load adult backstory {adultDefName} for {pawn.Name}. Giving random.");
					adulthood = DefDatabase<BackstoryDef>.AllDefs.Where((BackstoryDef bs) => bs.slot == BackstorySlot.Adulthood).RandomElement();
				}
			}
			else if (Scribe.mode == LoadSaveMode.Saving)
			{
				Scribe_Defs.Look(ref childhood, "childhood");
				Scribe_Defs.Look(ref adulthood, "adulthood");
			}
		}

		public BackstoryDef GetBackstory(BackstorySlot slot)
		{
			if (slot == BackstorySlot.Childhood)
			{
				return childhood;
			}
			return adulthood;
		}

		public bool TryGetRandomHeadFromSet(IEnumerable<HeadTypeDef> options)
		{
			Rand.PushState(pawn.thingIDNumber);
			bool result = options.Where((HeadTypeDef h) => CanUseHeadType(h)).TryRandomElementByWeight((HeadTypeDef x) => x.selectionWeight, out headType);
			Rand.PopState();
			return result;
			bool CanUseHeadType(HeadTypeDef head)
			{
				if (ModsConfig.BiotechActive && !head.requiredGenes.NullOrEmpty())
				{
					if (pawn.genes == null)
					{
						return false;
					}
					foreach (GeneDef requiredGene in head.requiredGenes)
					{
						if (!pawn.genes.HasGene(requiredGene))
						{
							return false;
						}
					}
				}
				if (head.gender != 0)
				{
					return head.gender == pawn.gender;
				}
				return true;
			}
		}
	}
}
