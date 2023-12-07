using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Verse;
using UnityEngine;


namespace RimWorld{
public class PawnGenOption
{
	//Config
	public PawnKindDef			kind;
	public float 				selectionWeight;

	//Properties
	public float Cost{get{return kind.combatPower;}}

	public override string ToString()
	{
		return "(" + (kind!=null?kind.ToString():"null")
			+ " w=" + selectionWeight.ToString("F2")
			+ " c=" + (kind!=null?Cost.ToString("F2"):"null") + ")";
	}

	public void LoadDataFromXmlCustom( XmlNode xmlRoot )
	{
		DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef( this, "kind", xmlRoot.Name );
		selectionWeight = ParseHelper.FromString<float>( xmlRoot.FirstChild.Value );
	}
}

public struct PawnGenOptionWithXenotype
{
    private PawnGenOption option;
    private XenotypeDef xenotype;
    private float selectionWeight;

    public PawnGenOption Option => option;
    public XenotypeDef Xenotype => xenotype;
    public float SelectionWeight => selectionWeight;

    public PawnGenOptionWithXenotype(PawnGenOption option, XenotypeDef xenotype, float selectionWeight)
    {
        this.option = option;
        this.xenotype = xenotype;
        this.selectionWeight = selectionWeight;
    }

    public float Cost => xenotype != null ? option.Cost * xenotype.combatPowerFactor : option.Cost;
}

public class RoyalImplantRule
{
	public HediffDef implantHediff;
	public RoyalTitleDef minTitle;
	public int maxLevel;
}

public class MemeWeight
{
	public MemeDef meme;
	public float selectionWeight;

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "meme", xmlRoot.Name);
		selectionWeight = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
	}
}

public class FactionDef : Def
{
	//General config
	public bool					isPlayer = false;
    public RulePackDef          factionNameMaker;
    public RulePackDef          settlementNameMaker;
    public RulePackDef          playerInitialSettlementNameMaker;
	[MustTranslate] public string fixedName = null;
	public bool					humanlikeFaction = true;
	public bool					hidden = false;
	public float				listOrderPriority = 0f;
	public List<PawnGroupMaker>	pawnGroupMakers = null;
	public SimpleCurve			raidCommonalityFromPointsCurve = null;
	public bool					autoFlee = true;
	public FloatRange			attackersDownPercentageRangeForAutoFlee = new FloatRange(.4f, .7f);
	public bool					canSiege = false;
	public bool					canStageAttacks = false;
	public bool					canUseAvoidGrid = true;
	public float				earliestRaidDays = 0;
	public FloatRange			allowedArrivalTemperatureRange = new FloatRange(-1000, 1000);
    public SimpleCurve          minSettlementTemperatureChanceCurve = null; // Factor applied when finding a settlement tile 
	public PawnKindDef			basicMemberKind;
	public List<ResearchProjectTagDef>	startingResearchTags = null;
	public List<ResearchProjectTagDef>  startingTechprintsResearchTags = null;
	[NoTranslate] public List<string>	recipePrerequisiteTags = null;
	public bool					rescueesCanJoin = false;
	[MustTranslate] public string pawnSingular = "member";
	[MustTranslate] public string pawnsPlural = "members";
	[MustTranslate] public string leaderTitle = "leader";
	[MustTranslate] public string leaderTitleFemale;
    [MustTranslate] public string royalFavorLabel;
    [NoTranslate] public string royalFavorIconPath;
	public List<PawnKindDef>	fixedLeaderKinds = null;
	public bool					leaderForceGenerateNewPawn;
	public float				forageabilityFactor = 1f;
	public SimpleCurve			maxPawnCostPerTotalPointsCurve = null;
	public List<string>			royalTitleTags;
	[NoTranslate] public string categoryTag;
    public bool                 hostileToFactionlessHumanlikes;
    public ThingDef             dropPodActive; //override default dropod
    public ThingDef             dropPodIncoming; //override default dropod

	//Faction generation
	public int					requiredCountAtGameStart = 0;
	public int					maxCountAtGameStart = 9999;
	public bool					canMakeRandomly = false;
	public float				settlementGenerationWeight = 0f;
    public bool                 generateNewLeaderFromMapMembersOnly = false;
    public int                  maxConfigurableAtWorldCreation = -1;
    public int                  startingCountAtWorldCreation = 1;
    public int                  configurationListOrderPriority;
    public FactionDef           replacesFaction;
    public bool                 displayInFactionSelection = true;

	//Humanlike faction config
	public TechLevel			techLevel = TechLevel.Undefined;
	[NoTranslate] public List<BackstoryCategoryFilter> backstoryFilters = null;
	[NoTranslate] List<string> backstoryCategories = null;
	public ThingFilter			apparelStuffFilter = null;
	public ThingSetMakerDef     raidLootMaker = null;
    public SimpleCurve          raidLootValueFromPointsCurve;
	public List<TraderKindDef>	caravanTraderKinds = new List<TraderKindDef>();
	public List<TraderKindDef>	visitorTraderKinds = new List<TraderKindDef>();
	public List<TraderKindDef>	baseTraderKinds = new List<TraderKindDef>();
    public XenotypeSet          xenotypeSet;
    public FloatRange           melaninRange = FloatRange.ZeroToOne;
    public List<RaidStrategyDef> disallowedRaidStrategies;
    public List<RaidAgeRestrictionDef> disallowedRaidAgeRestrictions;

	//Relations (can apply to non-humanlike factions)
	public bool					mustStartOneEnemy = false;
    public bool                 naturalEnemy = false;
	public bool					permanentEnemy = false;
    public bool                 permanentEnemyToEveryoneExceptPlayer = false;
    public List<FactionDef>     permanentEnemyToEveryoneExcept;

	//World drawing
	[NoTranslate] public string	settlementTexturePath;
	[NoTranslate] public string	factionIconPath;
	public List<Color>			colorSpectrum;

	// Royal titles
	public List<PawnRelationDef> royalTitleInheritanceRelations;
	public Type royalTitleInheritanceWorkerClass = null;
	public List<RoyalImplantRule> royalImplantRules;
    [System.Obsolete("Will be removed in the future")]
    public RoyalTitleDef minTitleForBladelinkWeapons;
    public string renounceTitleMessage;

    //Ideo generation
	public List<CultureDef> allowedCultures;
    public List<MemeDef> requiredMemes;
    public List<MemeDef> allowedMemes;
    public List<MemeDef> disallowedMemes;
    public List<PreceptDef> disallowedPrecepts;
    public List<MemeWeight> structureMemeWeights;
    public bool classicIdeo;
    
    //Comms Dialog
    [MayTranslate] public string dialogFactionGreetingHostile;
    [MayTranslate] public string dialogFactionGreetingHostileAppreciative;
    [MayTranslate] public string dialogFactionGreetingWary;
    [MayTranslate] public string dialogFactionGreetingWarm;
    [MayTranslate] public string dialogMilitaryAidSent;

	//Unsaved
	[Unsaved] private Texture2D	factionIcon;
	[Unsaved] private Texture2D	settlementTexture;
    [Unsaved] private Texture2D royalFavorIcon;
    [Unsaved] private string cachedDescription;

    //Cache
	[Unsaved] private List<RoyalTitleDef> royalTitlesAwardableInSeniorityOrderForReading;
	[Unsaved] private List<RoyalTitleDef> royalTitlesAllInSeniorityOrderForReading;
	[Unsaved] private RoyalTitleInheritanceWorker royalTitleInheritanceWorker;

    
	//Properties
	public List<RoyalTitleDef> RoyalTitlesAwardableInSeniorityOrderForReading
	{
		get
		{
			// Cache init
			if (royalTitlesAwardableInSeniorityOrderForReading == null)
			{
				royalTitlesAwardableInSeniorityOrderForReading = new List<RoyalTitleDef>();
				if (royalTitleTags != null && ModLister.RoyaltyInstalled)
				{
					foreach (var titleDef in DefDatabase<RoyalTitleDef>.AllDefsListForReading)
					{
						if (titleDef.Awardable && titleDef.tags.SharesElementWith(royalTitleTags))
							royalTitlesAwardableInSeniorityOrderForReading.Add(titleDef);
					}

					royalTitlesAwardableInSeniorityOrderForReading.SortBy(t => t.seniority);
				}
			}
			return royalTitlesAwardableInSeniorityOrderForReading;
		}
	}
    public List<RoyalTitleDef> RoyalTitlesAllInSeniorityOrderForReading
    {
        get
        {
			// Cache init
			if (royalTitlesAllInSeniorityOrderForReading == null)
			{
				royalTitlesAllInSeniorityOrderForReading = new List<RoyalTitleDef>();
				if (royalTitleTags != null && ModLister.RoyaltyInstalled)
				{
					foreach (var titleDef in DefDatabase<RoyalTitleDef>.AllDefsListForReading)
					{
						if (titleDef.tags.SharesElementWith(royalTitleTags))
							royalTitlesAllInSeniorityOrderForReading.Add(titleDef);
					}

					royalTitlesAllInSeniorityOrderForReading.SortBy(t => t.seniority);
				}
			}
			return royalTitlesAllInSeniorityOrderForReading;
        }
    }
	public RoyalTitleInheritanceWorker RoyalTitleInheritanceWorker
	{
		get
		{
			if (royalTitleInheritanceWorker == null && royalTitleInheritanceWorkerClass != null)
				royalTitleInheritanceWorker = (RoyalTitleInheritanceWorker)Activator.CreateInstance(royalTitleInheritanceWorkerClass);
			return royalTitleInheritanceWorker;
		}
	}
	public bool CanEverBeNonHostile
	{
		get
		{
			return !permanentEnemy;
		}
	}
	public Texture2D SettlementTexture
	{
		get
		{
			if( settlementTexture == null )
			{
				if( !settlementTexturePath.NullOrEmpty() )
					settlementTexture = ContentFinder<Texture2D>.Get(settlementTexturePath);
				else
					settlementTexture = BaseContent.BadTex;
			}

			return settlementTexture;
		}
	}
	public Texture2D FactionIcon
	{
		get
		{
			if( factionIcon == null )
			{
				if( !factionIconPath.NullOrEmpty() )
					factionIcon = ContentFinder<Texture2D>.Get(factionIconPath);
				else
					factionIcon = BaseContent.BadTex;
			}

			return factionIcon;
		}
	}
    public Texture2D RoyalFavorIcon
    {
        get
        {
            if( royalFavorIcon == null && !royalFavorIconPath.NullOrEmpty() )
                royalFavorIcon = ContentFinder<Texture2D>.Get(royalFavorIconPath);

            return royalFavorIcon;
        }
    }
	public bool HasRoyalTitles
	{
		get { return RoyalTitlesAwardableInSeniorityOrderForReading.Count > 0; }
	}

    public Color DefaultColor
    {
        get
        {
            if(colorSpectrum.NullOrEmpty())
                return Color.white;
            return colorSpectrum[0];
        }
    }

    public float BaselinerChance => xenotypeSet == null ? 1f : xenotypeSet.BaselinerChance;
    public string Description
    {
        get
        {
            if (cachedDescription == null)
            {
                if (description.NullOrEmpty())
                    description = string.Empty;
                else
                    cachedDescription = description;

                if (ModsConfig.BiotechActive && humanlikeFaction)
                {
                    List<XenotypeChance> chances = new List<XenotypeChance>();
                    cachedDescription += "\n\n" + ("MemberXenotypeChances".Translate() + ":").AsTipTitle() + "\n";
                    if (BaselinerChance > 0)
                        chances.Add(new XenotypeChance(XenotypeDefOf.Baseliner, BaselinerChance));

                    if (xenotypeSet != null)
                    {
                        for (int i = 0; i < xenotypeSet.Count; i++)
                        {
                            if (xenotypeSet[i].xenotype != XenotypeDefOf.Baseliner)
                                chances.Add(xenotypeSet[i]);
                        }
                    }

                    if (chances.Any())
                    {
                        chances.SortBy(x => -x.chance);
                        cachedDescription += chances.Select(x => x.xenotype.LabelCap.ToString() + ": " + Mathf.Min(x.chance, 1f).ToStringPercent()).ToLineList("  - ");
                    }
                }
            }

            return cachedDescription;
        }
    }

	public float MinPointsToGeneratePawnGroup(PawnGroupKindDef groupKind, PawnGroupMakerParms parms = null)
	{
		if( pawnGroupMakers == null )
			return 0;

		var groups = pawnGroupMakers.Where(x => x.kindDef == groupKind);

		if( !groups.Any() )
			return 0;

        return groups.Min(pgm => pgm.MinPointsToGenerateAnything(this, parms));
	}

	public bool CanUseStuffForApparel( ThingDef stuffDef )
	{
		if( apparelStuffFilter == null )
			return true;

		return apparelStuffFilter.Allows( stuffDef );
	}

	public float RaidCommonalityFromPoints( float points )
	{
		if( points < 0 || raidCommonalityFromPointsCurve == null )
			return 1f;

		return raidCommonalityFromPointsCurve.Evaluate(points);
	}

	public override void ResolveReferences()
	{
		base.ResolveReferences();

		if( apparelStuffFilter != null )
			apparelStuffFilter.ResolveReferences();
	}
	
	public override void PostLoad()
	{
		if (backstoryCategories != null && backstoryCategories.Count > 0)
		{
			if (backstoryFilters == null)
				backstoryFilters = new List<BackstoryCategoryFilter>();
			backstoryFilters.Add(new BackstoryCategoryFilter() { categories = backstoryCategories });
		}
	}
	
	public override IEnumerable<string> ConfigErrors()
	{
		foreach( var error in base.ConfigErrors() )
			yield return error;

		if( pawnGroupMakers != null && maxPawnCostPerTotalPointsCurve == null )
			yield return "has pawnGroupMakers but missing maxPawnCostPerTotalPointsCurve";
            
		if( techLevel == TechLevel.Undefined )
			yield return defName + " has no tech level.";

		if( humanlikeFaction )
		{
			if( backstoryFilters.NullOrEmpty() )
				yield return defName + " is humanlikeFaction but has no backstory categories.";
		}
        
		if( permanentEnemy )
		{
			if( mustStartOneEnemy )
				yield return "permanentEnemy has mustStartOneEnemy = true, which is redundant";
		}

        if( disallowedMemes != null && allowedMemes != null )
            yield return "both disallowedMemes (black list) and allowedMemes (white list) are defined";

        if( requiredMemes != null )
        {
            var requiredNotAllowedMeme = requiredMemes.FirstOrDefault(x => !IdeoUtility.IsMemeAllowedFor(x, this));
            if( requiredNotAllowedMeme != null )
                yield return "has a required meme which is not allowed: " + requiredNotAllowedMeme.defName;
        }

        if (raidLootValueFromPointsCurve == null)
            yield return "raidLootValueFromPointsCurve must be defined";

        if((dropPodActive == null) != (dropPodIncoming == null))
           yield return "Both drop pod and drop pod incoming must be defined or both must be undefined"; 
	} 

	public static FactionDef Named( string defName )
	{
		return DefDatabase<FactionDef>.GetNamed(defName);
	}
}}

