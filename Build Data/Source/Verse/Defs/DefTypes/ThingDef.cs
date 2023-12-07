using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;
using UnityEngine;
using Verse.Sound;
using Verse.AI;
using RimWorld;

namespace Verse
{

public enum FillCategory : byte
{
	None,
	Partial,
	Full,
}

public enum DrawerType : byte
{
	None,
	RealtimeOnly,
	MapMeshOnly,
	MapMeshAndRealTime
}

public enum ResourceCountPriority : byte
{
	Uncounted,

	Last,
	Middle,
	First
}

public enum SurfaceType : byte
{
	None,
	Item,
	Eat
}

public class DamageMultiplier
{
	public DamageDef	damageDef = null;
	public float		multiplier = 1f;
}

public class ThingDef : BuildableDef
{
	//Basics
	public Type						thingClass;
	public ThingCategory 			category;
	public TickerType				tickerType		= TickerType.Never;
	public int						stackLimit		= 1;
	public IntVec2					size			= new IntVec2(1,1);
	public bool						destroyable		= true;
	public bool						rotatable		= true;
	public bool						smallVolume;
	public bool						useHitPoints	= true;
	public bool						receivesSignals;
	public List<CompProperties>		comps			= new List<CompProperties>();

    //Development
    [NoTranslate]
    public string                   devNote;

	//Misc
	public List<ThingDefCountClass>	killedLeavings;
    public List<ThingDefCountClass> killedLeavingsPlayerHostile; //only drop if this thing was hostile to the player
	public List<ThingDefCountClass>	butcherProducts;
	public List<ThingDefCountClass>	smeltProducts;
	public bool						smeltable;
	public bool						burnableByRecipe;
	public bool						randomizeRotationOnSpawn;
	public List<DamageMultiplier>	damageMultipliers;
    public bool                     isTechHediff;
	public RecipeMakerProperties	recipeMaker;
	public ThingDef					minifiedDef;
	public bool						isUnfinishedThing;
	public bool						leaveResourcesWhenKilled;
	public ThingDef					slagDef;
	public bool						isFrameInt;
    public List<IntVec3>            multipleInteractionCellOffsets; // exclusive with interactionCellOffset
	public IntVec3					interactionCellOffset	= IntVec3.Zero;
    public bool						hasInteractionCell;
	public ThingDef					interactionCellIcon;
	public bool						interactionCellIconReverse;
	public ThingDef					filthLeaving;
	public bool						forceDebugSpawnable;
	public bool						intricate;
	public bool						scatterableOnMapGen		= true;
	public float					deepCommonality			= 0;
	public int						deepCountPerCell		= 300;
	public int						deepCountPerPortion		= -1;
	public IntRange					deepLumpSizeRange		= IntRange.zero;
	public float					generateCommonality		= 1f;
	public float					generateAllowChance		= 1f;
	private bool					canOverlapZones			= true;
	public FloatRange				startingHpRange			= FloatRange.One;
	[NoTranslate] public List<string> thingSetMakerTags;
	public bool						alwaysFlee;
	public List<RecipeDef>			recipes;
    public bool                     messageOnDeteriorateInStorage = true;
    public bool                     deteriorateFromEnvironmentalEffects = true;
    public bool                     canDeteriorateUnspawned = false;
    public bool                     canLoadIntoCaravan = true;
    public bool                     isMechClusterThreat;
    public FloatRange               displayNumbersBetweenSameDefDistRange = FloatRange.Zero;
    public int                      minRewardCount = 1;
    public bool                     preventSkyfallersLandingOn;
    public FactionDef               requiresFactionToAcquire;
    public float                    relicChance = 0;
    public OrderedTakeGroupDef      orderedTakeGroup;
    public int                      allowedArchonexusCount = 0; // 0 is cannot bring, -1 is automatically calculate count, > 0 is explicit count
    public int                      possessionCount = 0; //How many can be randomly given to a starting pawn
    public bool                     notifyMapRemoved = false;
    public bool                     canScatterOver = true;

	//Visuals
	public GraphicData				graphicData;
	public DrawerType				drawerType			= DrawerType.RealtimeOnly;
	public bool						drawOffscreen;
	public ColorGenerator			colorGenerator;
	public float					hideAtSnowDepth		= 99999f;
	public bool						drawDamagedOverlay	= true;
	public bool						castEdgeShadows;
	public float					staticSunShadowHeight;
	public bool						useSameGraphicForGhost;
    public List<ThingStyleChance>   randomStyle;
    public float                    randomStyleChance;
    public bool                     canEditAnyStyle;
    public ThingDef                 defaultStuff;
    public int                      killedLeavingsExpandRect;
    public float                    minifiedDrawScale = 1;

	//Interface
	public bool						selectable;
    public bool                     containedPawnsSelectable = false;
    public bool                     containedItemsSelectable = false;
	public bool 					neverMultiSelect;
	public bool						isAutoAttackableMapObject;
	public bool						hasTooltip;
	public List<Type>				inspectorTabs;
	[Unsaved] public List<InspectTabBase> inspectorTabsResolved;
	public bool						seeThroughFog;
	public bool						drawGUIOverlay;
	public bool						drawGUIOverlayQuality = true;
	public ResourceCountPriority	resourceReadoutPriority		= ResourceCountPriority.Uncounted;
	public bool						resourceReadoutAlwaysShow;
	public bool						drawPlaceWorkersWhileSelected;
	public bool						drawPlaceWorkersWhileInstallBlueprintSelected;
	public ConceptDef				storedConceptLearnOpportunity;
	public float					uiIconScale					= 1f;
	public bool						hasCustomRectForSelector;

	//AI hints
	public bool						alwaysHaulable;
	public bool						designateHaulable;
	public List<ThingCategoryDef>	thingCategories;
	public bool						mineable;
	public bool						socialPropernessMatters;
	public bool						stealable = true;

	//Sounds
	public SoundDef					soundDrop;
	public SoundDef					soundPickup;
	public SoundDef					soundInteract;
	public SoundDef					soundImpactDefault;
    public SoundDef                 soundPlayInstrument;
    public SoundDef                 soundOpen;

	//Save/load
    public bool						saveCompressible;
	public bool						isSaveable = true;

	//Physics
	public bool						holdsRoof;
	public float					fillPercent;
	public bool						coversFloor;
	public bool						neverOverlapFloors;
	public SurfaceType				surfaceType = SurfaceType.None;
	public bool						blockPlants;
	public bool						blockLight;
	public bool						blockWind;

	//Trade
	public Tradeability				tradeability = Tradeability.All;
	[NoTranslate] public List<string> tradeTags;
	public bool						tradeNeverStack;
    public bool                     tradeNeverGenerateStacked;
    public bool                     healthAffectsPrice = true;
	public ColorGenerator			colorGeneratorInTraderStock;
	
    //Used with equipment, apparel or races
    private List<VerbProperties>	verbs = null;
	public List<Tool>				tools;
    
	//Used with equipment/inventory/artificial body parts/implants
    public float                    equippedAngleOffset;
    public float                    equippedDistanceOffset;  
	public EquipmentType			equipmentType	= EquipmentType.None;
	public TechLevel				techLevel		= TechLevel.Undefined;
    public List<WeaponClassDef>     weaponClasses;
	[NoTranslate] public List<string> weaponTags;
    [NoTranslate] public List<string> techHediffsTags;
    public bool                     violentTechHediff;
	public bool						destroyOnDrop;   //Basically means whether this can exist spawned by itself (e.g. turret-only weapons cannot)
	public List<StatModifier>		equippedStatOffsets;
	public SoundDef					meleeHitSound;
    public float                    recoilPower = 1f;
    public float                    recoilRelaxation = 10f;
    public bool                     rotateInShelves = true;
	
	//Used with blueprints
	public BuildableDef				entityDefToBuild;

	//Used with shells
	public ThingDef					projectileWhenLoaded;
    
    //Used with ideo buildings
    public RulePackDef              ideoBuildingNamerBase;

	//Various sub-properties
	public IngestibleProperties		ingestible;
	public FilthProperties			filth;
	public GasProperties			gas;
	public BuildingProperties		building;
	public RaceProperties			race;
	public ApparelProperties		apparel;
	public MoteProperties			mote;
	public PlantProperties			plant;
	public ProjectileProperties		projectile;
	public StuffProperties			stuffProps;
	public SkyfallerProperties		skyfaller;
	public PawnFlyerProperties		pawnFlyer;
    public RitualFocusProperties    ritualFocus;
    public IngredientProperties     ingredient;

	// Alerts
	/// <summary>
	/// <see cref="Alert_CannotBeUsedRoofed"/>
	/// </summary>
	public bool						canBeUsedUnderRoof = true;

	//Cached
	[Unsaved] private string		descriptionDetailedCached;
	[Unsaved] public Graphic		interactionCellGraphic;
    [Unsaved] private bool?         isNaturalOrganCached;
    [Unsaved] private List<StyleCategoryDef> cachedRelevantStyleCategories = null;

	//Constants
	public const int				SmallUnitPerVolume = 10;
	public const float				SmallVolumePerUnit = 0.1f;
	public const float              ArchonexusMaxItemStackMass = 5f;
	public const int                ArchonexusMaxItemStackCount = 25;
	public const float              ArchonexusMaxItemStackValue = 2000f;
	public const int                ArchonexusAutoCalculateValue = -1;  // if allowedArchonexusCount is this value, calculate according to special formula


	//======================== Misc properties ==============================
	public bool	EverHaulable{get{return alwaysHaulable || designateHaulable;}}
    public bool EverPollutable => !building.isNaturalRock;
	public float VolumePerUnit{get{return !smallVolume ? 1 : SmallVolumePerUnit;}}
	public override IntVec2 Size{get{return size;}}
	public bool DiscardOnDestroyed{get{return race == null;}}
	public int	BaseMaxHitPoints	{get{return Mathf.RoundToInt(this.GetStatValueAbstract( StatDefOf.MaxHitPoints ));}}
	public float BaseFlammability	{get{return this.GetStatValueAbstract( StatDefOf.Flammability );}}
	public float BaseMarketValue
	{
		get
		{
			return this.GetStatValueAbstract( StatDefOf.MarketValue );
		}
		set
		{
			this.SetStatBaseValue( StatDefOf.MarketValue, value );
		}
	}
	public float BaseMass
	{
		get
		{
			return this.GetStatValueAbstract( StatDefOf.Mass );
		}
	}
	public int ArchonexusMaxAllowedCount
	{
		get
		{
			if (allowedArchonexusCount == ArchonexusAutoCalculateValue)
				return Mathf.Min(stackLimit,
					ArchonexusMaxItemStackCount, // 25
					BaseMass > 0 ? (int)(ArchonexusMaxItemStackMass / BaseMass) : 0, // count less than 5kg
					BaseMarketValue > 0 ? (int)(ArchonexusMaxItemStackValue / BaseMarketValue) : 0); // count less than 2000 silver

			return allowedArchonexusCount;
		}
	}
	public bool PlayerAcquirable
	{
		get
		{
			if(destroyOnDrop)
                return false;

            if(this == ThingDefOf.ReinforcedBarrel && Find.Storyteller != null && Find.Storyteller.difficulty.classicMortars)
                return false;
            
            if(requiresFactionToAcquire != null && Find.World != null && Find.World.factionManager != null)
                return Find.FactionManager.FirstFactionOfDef(requiresFactionToAcquire) != null;

            return true;
		}
	}
	public bool EverTransmitsPower
	{
		get
		{
			for( int i=0; i<comps.Count; i++ )
			{
				var p = comps[i] as CompProperties_Power;

				if( p != null && p.transmitsPower )
					return true;
			}
			return false;
		}
	}
	public bool Minifiable{get{return minifiedDef != null;}}
	public bool	HasThingIDNumber
	{
		get
		{
			return category != ThingCategory.Mote;
		}
	}
	private List<RecipeDef> allRecipesCached = null;
	public List<RecipeDef> AllRecipes
	{
		get
		{
			if( allRecipesCached == null )
			{
				allRecipesCached = new List<RecipeDef>();
				if( recipes != null )
				{
					for(int i=0; i<recipes.Count; i++ )
					{
						allRecipesCached.Add(recipes[i]);
					}
				}

				var recipeDefs = DefDatabase<RecipeDef>.AllDefsListForReading;
				for( int i=0; i<recipeDefs.Count; i++ )
				{
					if( recipeDefs[i].recipeUsers != null && recipeDefs[i].recipeUsers.Contains(this) )
						allRecipesCached.Add(recipeDefs[i]);
				}
			}

			return allRecipesCached;
		}
	}
	public bool ConnectToPower
	{
		get
		{
			if( EverTransmitsPower )
				return false;

			for( int i=0; i<comps.Count; i++ )
			{
				if( comps[i].compClass == typeof(CompPowerBattery) )
					return true;

				if( comps[i].compClass == typeof(CompPowerTrader) )
					return true;
			}
			return false;
		}
	}
	public bool CoexistsWithFloors
	{
		get
		{
			return !neverOverlapFloors && !coversFloor;
		}
	}
	public FillCategory Fillage
	{
		get
		{
			if( fillPercent < 0.01f )
				return FillCategory.None;
			else if( fillPercent > 0.99f )
				return FillCategory.Full;
			else
				return FillCategory.Partial;
		}
	}
	public bool MakeFog{get{return Fillage == FillCategory.Full;}}
	public bool CanOverlapZones
	{
		get
		{
			// buildings which support plants can't overlap zones,
			// (so there is no growing zone and a building which supports plants on the same cell)
			if( building != null && building.SupportsPlants )
				return false;

			//Nothing impassable can overlap a zone, except plants and anything explicitly defined
			if( passability == Traversability.Impassable && category != ThingCategory.Plant && !HasComp(typeof(CompTransporter)) )
				return false;

			if( surfaceType >= SurfaceType.Item )
				return false;

			if( typeof(ISlotGroupParent).IsAssignableFrom(thingClass) )
				return false;

			if( !canOverlapZones )
				return false;

			//Blueprints and frames inherit from the def they want to build
			if( IsBlueprint || IsFrame )
			{
				var thingDefToBuild = entityDefToBuild as ThingDef;
				if( thingDefToBuild != null )
					return thingDefToBuild.CanOverlapZones;
			}

			return true;
		}
	}
	public bool	CountAsResource{get{return resourceReadoutPriority != ResourceCountPriority.Uncounted;}}
	private static List<VerbProperties> EmptyVerbPropertiesList = new List<VerbProperties>();
	public List<VerbProperties> Verbs
	{
		get
		{
			if( verbs != null )
				return verbs;
			return EmptyVerbPropertiesList;
		}
	}
	public bool CanHaveFaction
	{
		get
		{
			if( IsBlueprint || IsFrame )
				return true;

			switch( category )
			{
				case ThingCategory.Pawn: return true;
				case ThingCategory.Building: return true;
			}

			return false;
		}
	}
	public bool Claimable
	{
		get
		{
			return building != null && building.claimable && !building.isNaturalRock;
		}
	}
	public ThingCategoryDef FirstThingCategory
	{
		get
		{
			if( thingCategories.NullOrEmpty() )
				return null;

			return thingCategories[0];
		}
	}
	public float MedicineTendXpGainFactor
	{
		get
		{
			return Mathf.Clamp( this.GetStatValueAbstract(StatDefOf.MedicalPotency)*0.7f, SkillTuning.XpPerTendFactor_NoMedicine, 1.0f );
		}
	}
	public bool CanEverDeteriorate
	{
		get
		{
			if( !useHitPoints )
				return false;

            return category == ThingCategory.Item || (plant != null && plant.canDeteriorate);
		}
	}

	public bool CanInteractThroughCorners
	{
		get
		{
			//We can ALWAYS touch roof holders via corners,
			//this is so we can repair or construct wall corners from inside the corner,
			//the only exception are natural rocks and smoothed rocks -> we don't want to always allow mining diagonally

			if( category != ThingCategory.Building )
				return false;
			
			if( !holdsRoof )
				return false;
			
			if( building != null && building.isNaturalRock && !IsSmoothed )
				return false;
			
			return true;
		}
	}
	/// <summary>
	/// Returns true if this thing affects regions (e.g. is a wall), i.e. whether the regions should be rebuilt whenever this thing is spawned or despawned.
	/// </summary>
	public bool AffectsRegions
	{
		get
		{
			// see RegionTypeUtility.GetExpectedRegionType()
			return passability == Traversability.Impassable || IsDoor || IsFence;
		}
	}
	/// <summary>
	/// Returns true if this thing affects reachability (e.g. is a wall), i.e. whether the reachability cache should be cleared whenever this thing is spawned or despawned.
	/// </summary>
	public bool AffectsReachability
	{
		get
		{
			// see TouchPathEndModeUtility.IsCornerTouchAllowed()

			//Things which affect regions always affect reachability
			if( AffectsRegions )
				return true;

			if( passability == Traversability.Impassable || IsDoor )
				return true;

			//Makes occupied cells reachable diagonally
			if( TouchPathEndModeUtility.MakesOccupiedCellsAlwaysReachableDiagonally(this) )
				return true;

			return false;
		}
	}

	public string DescriptionDetailed
	{
		get
		{
			if( descriptionDetailedCached == null )
			{
				var sb = new StringBuilder();
				sb.Append(description);
				
				if( IsApparel )
				{
					// Add apparel info
                    sb.AppendLine();
					sb.AppendLine();
					sb.AppendLine(string.Format("{0}: {1}", "Layer".Translate(), apparel.GetLayersString()));
					sb.Append(string.Format("{0}: {1}", "Covers".Translate(), apparel.GetCoveredOuterPartsString(BodyDefOf.Human)));
					if( equippedStatOffsets != null && equippedStatOffsets.Count > 0 )
					{
						sb.AppendLine();
						sb.AppendLine();
                        for( int i=0; i<equippedStatOffsets.Count; i++ )
						{
                            if( i > 0 )
                                sb.AppendLine();
                            var stat = equippedStatOffsets[i];
							sb.Append(string.Format("{0}: {1}", stat.stat.LabelCap, stat.ValueToStringAsOffset));
						}
					}
				}

				descriptionDetailedCached = sb.ToString();
			}

			return descriptionDetailedCached;
		}
	}
    public bool CanBenefitFromCover
    {
        get
        {
            if (category == ThingCategory.Pawn)
                return true;

            if (building != null && building.IsTurret)
                return true;

            return false;
        }
    }
    public bool PotentiallySmeltable
    {
        get
        {
            //Explicitly set to not be smeltable.
            if (!smeltable)
                return false;

            if (MadeFromStuff)
            {
                var stuff = GenStuff.AllowedStuffsFor(this);
                foreach (var s in stuff)
                {
                    //Has at least one smeltable stuff that can make it.
                    if (s.smeltable)
                        return true;
                }

                return false;
            }

            return true;
        }
    }

    public bool HasSingleOrMultipleInteractionCells => hasInteractionCell || !multipleInteractionCellOffsets.NullOrEmpty();

	//Properties: IsKindOfThing bools
	public bool IsApparel{get{return apparel != null;}}
	public bool IsBed { get{return typeof(Building_Bed).IsAssignableFrom(thingClass);} }
	public bool IsCorpse { get{return typeof(Corpse).IsAssignableFrom(thingClass);} }
	public bool IsFrame { get{return isFrameInt;}}
	public bool IsBlueprint { get{return entityDefToBuild != null && category == ThingCategory.Ethereal;}}
	public bool IsStuff				{get{return stuffProps != null;}}
	public bool IsMedicine { get{return statBases.StatListContains(StatDefOf.MedicalPotency);}}
	public bool IsDoor{get{return typeof(Building_Door).IsAssignableFrom(thingClass);}}
	public bool IsFence{get{return building != null && building.isFence;}}
	public bool IsFilth{get{return filth != null;}}
	public bool IsIngestible{get{return ingestible != null;}}
	public bool IsNutritionGivingIngestible{get{return IsIngestible && ingestible.CachedNutrition > 0;}}
    public bool IsNutritionGivingIngestibleForHumanlikeBabies => IsNutritionGivingIngestible && ingestible.HumanEdible && ingestible.babiesCanIngest;
	public bool IsWeapon { get { return category == ThingCategory.Item && (!verbs.NullOrEmpty() || !tools.NullOrEmpty()) && !IsApparel; } }
	public bool IsCommsConsole{get{return typeof(Building_CommsConsole).IsAssignableFrom(thingClass);}}
	public bool IsOrbitalTradeBeacon{get{return typeof(Building_OrbitalTradeBeacon).IsAssignableFrom(thingClass);}}
	public bool IsFoodDispenser{get{return typeof(Building_NutrientPasteDispenser).IsAssignableFrom(thingClass);}}
	public bool IsDrug{get{return ingestible != null && ingestible.drugCategory != DrugCategory.None;}}
	public bool IsPleasureDrug{get{return IsDrug && ingestible.joy > 0;}}
	public bool IsNonMedicalDrug{get{return IsDrug && ingestible.drugCategory != DrugCategory.Medical;}}
	public bool IsTable{get{return surfaceType == SurfaceType.Eat && HasComp(typeof(CompGatherSpot));}}
	public bool IsWorkTable{get{return typeof(Building_WorkTable).IsAssignableFrom(thingClass);}}
	public bool IsShell{get{return projectileWhenLoaded != null;}}
	public bool IsArt{get{return IsWithinCategory(ThingCategoryDefOf.BuildingsArt);}}
	public bool IsSmoothable{get{return building != null && building.smoothedThing != null;}}
	public bool IsSmoothed{get{return building != null && building.unsmoothedThing != null;}}
	public bool IsMetal{get{return stuffProps != null && stuffProps.categories.Contains(StuffCategoryDefOf.Metallic);}}
    public bool IsCryptosleepCasket => typeof(Building_CryptosleepCasket).IsAssignableFrom(thingClass);
    public bool IsGibbetCage => typeof(Building_GibbetCage).IsAssignableFrom(thingClass);
    public bool IsMechGestator => typeof(Building_MechGestator).IsAssignableFrom(thingClass);
    public bool IsMechRecharger => typeof(Building_MechCharger).IsAssignableFrom(thingClass);
    public bool IsAddictiveDrug
	{
		get
		{
			var compDrug = GetCompProperties<CompProperties_Drug>();
			return compDrug != null && compDrug.addictiveness > 0;
		}
	}
	public bool IsMeat
	{
		get
		{
			return category == ThingCategory.Item
				&& thingCategories != null
				&& thingCategories.Contains(ThingCategoryDefOf.MeatRaw);
		}
	}
    public bool IsEgg
    {
        get
        {
            return category == ThingCategory.Item
                && thingCategories != null 
                && (thingCategories.Contains(ThingCategoryDefOf.EggsFertilized) || thingCategories.Contains(ThingCategoryDefOf.EggsUnfertilized));
        }
    }
	public bool IsLeather
	{
		get
		{
			return category == ThingCategory.Item
				&& thingCategories != null
				&& thingCategories.Contains(ThingCategoryDefOf.Leathers);
		}
	}
    public bool IsWool
    {
        get
        {
            return category == ThingCategory.Item
                && thingCategories != null
                && thingCategories.Contains(ThingCategoryDefOf.Wools);
        }
    }
	public bool IsRangedWeapon
	{
		get
		{
			if( !IsWeapon )
				return false;
			
			if( !verbs.NullOrEmpty() )
			{
				for( int i = 0; i < verbs.Count; i++ )
				{
					if( !verbs[i].IsMeleeAttack )
						return true;
				}
			}

			return false;
		}
	}
	public bool IsMeleeWeapon
	{
		get
		{
			return IsWeapon && !IsRangedWeapon;
		}
	}
	public bool IsWeaponUsingProjectiles
	{
		get
		{
			if( !IsWeapon )
				return false;
			
			if( !verbs.NullOrEmpty() )
			{
				for( int i = 0; i < verbs.Count; i++ )
				{
					if( verbs[i].LaunchesProjectile )
						return true;
				}
			}

			return false;
		}
	}
    public bool IsShieldThatBlocksRanged => HasComp(typeof(CompShield)) && GetCompProperties<CompProperties_Shield>().blocksRangedWeapons;
	public bool IsBuildingArtificial
	{
		get
		{
			// check for frame to handle special case: floor frames are not buildings
			return (category == ThingCategory.Building || IsFrame)
				&& !(building != null && (building.isNaturalRock || building.isResourceRock));
		}
	}
	public bool IsNonResourceNaturalRock
	{
		get
		{
			return category == ThingCategory.Building
				&& building.isNaturalRock
				&& !building.isResourceRock
				&& !IsSmoothed;
		}
	}
    public bool IsNaturalOrgan
    {
        get
        {
            if( isNaturalOrganCached == null )
            {
                if( category != ThingCategory.Item )
                    isNaturalOrganCached = false;
                else
                {
                    var bodyParts = DefDatabase<BodyPartDef>.AllDefsListForReading;
                    isNaturalOrganCached = false;
                    for( int i = 0; i < bodyParts.Count; i++ )
                    {
                        if( bodyParts[i].spawnThingOnRemoved == this )
                        {
                            isNaturalOrganCached = true;
                            break;
                        }
                    }
                }
            }

            return isNaturalOrganCached.Value;
        }
    }
    public bool IsFungus => ingestible != null && ingestible.foodType.HasFlag(FoodTypeFlags.Fungus);
    public bool IsAnimalProduct => ingestible != null && ingestible.foodType.HasFlag(FoodTypeFlags.AnimalProduct);
    public bool IsProcessedFood => ingestible != null && ingestible.foodType.HasFlag(FoodTypeFlags.Processed);
    public bool CanAffectLinker => graphicData != null && graphicData.Linked || IsDoor; // doors might impact links with neighboring asymmetric linkers (like fences)
    public bool IsNonDeconstructibleAttackableBuilding => IsBuildingArtificial && !building.IsDeconstructible && destroyable && !mineable && this != ThingDefOf.BurningPowerCell;
    public bool IsPlant => typeof(Plant).IsAssignableFrom(thingClass);
    public bool IsStudiable => HasComp(typeof(CompStudiable));
    public List<StyleCategoryDef> RelevantStyleCategories
    {
        get
        {
            if (cachedRelevantStyleCategories == null)
            {
                cachedRelevantStyleCategories = new List<StyleCategoryDef>();

                foreach (var cat in DefDatabase<StyleCategoryDef>.AllDefs)
                {
                    if (cat.thingDefStyles.NullOrEmpty())
                        continue;

                    foreach (var t in cat.thingDefStyles)
                    {
                        if (t.ThingDef == this)
                        {
                            cachedRelevantStyleCategories.Add(cat);
                            break;
                        }
                    }
                }
            }

            return cachedRelevantStyleCategories;
        }
    }
    public bool BlocksPlanting(bool canWipePlants = false)
    {
        //Nothing that supports plants blocks planting
        if( building != null && building.SupportsPlants )
            return false;
        
        if( blockPlants )
            return true;

        //All plants block each other
        if( !canWipePlants && category == ThingCategory.Plant )
            return true;

        if( Fillage > FillCategory.None )
            return true;

        //This includes things like power conduits
        //if( category == EntityCategory.Building )
        //	return true;

        if( this.IsEdifice() )
            return true;

        return false;
    }
	
	public bool	EverStorable(bool willMinifyIfPossible)
	{
		//Minified things are always storable
		if( typeof(MinifiedThing).IsAssignableFrom(thingClass) )
			return true;

		if( !thingCategories.NullOrEmpty() )
		{
			//Storable item
			if( category == ThingCategory.Item )
				return true;

			//Can be minified
			if( willMinifyIfPossible && Minifiable )
				return true;
		}
		
		return false;
	}

	private Dictionary<ThingDef, Thing> concreteExamplesInt;
	public Thing GetConcreteExample(ThingDef stuff = null) // this method is used for non-debug purposes, which is a hack
	{
		if( concreteExamplesInt == null )
			concreteExamplesInt = new Dictionary<ThingDef, Thing>();

		if( stuff == null )
			stuff = ThingDefOf.Steel;
		
		if( !concreteExamplesInt.ContainsKey(stuff) )
		{
			if( this.race == null )
				concreteExamplesInt[stuff] = ThingMaker.MakeThing(this, MadeFromStuff ? stuff : null);	// We can't store null keys in a dictionary, so we store null stuff under "steel", then pass the right parameter in here.
			else
				concreteExamplesInt[stuff] = PawnGenerator.GeneratePawn(DefDatabase<PawnKindDef>.AllDefsListForReading.Where(pkd => pkd.race == this).FirstOrDefault());
		}

		return concreteExamplesInt[stuff];
	}
	
	//========================== Comp stuff ================================

	public CompProperties CompDefFor<T>() where T:ThingComp
	{
        for (int i = 0; i < comps.Count; i++)
        {
            if (comps[i].compClass == typeof(T))
                return comps[i];
        }

        return null;
	}

	public CompProperties CompDefForAssignableFrom<T>() where T:ThingComp
	{
        for (int i = 0; i < comps.Count; i++)
        {
            if (typeof(T).IsAssignableFrom(comps[i].compClass))
                return comps[i];
        }

        return null;
	}

	public bool HasComp(Type compType)
	{
		for( int i = 0; i < comps.Count; i++ )
		{
			if( comps[i].compClass == compType )
				return true;
		}
		return false;
	}

    public bool HasAssignableCompFrom(Type compType)
    {
        for (int i = 0; i < comps.Count; i++)
		{
			if (compType.IsAssignableFrom(comps[i].compClass))
				return true;
		}
		return false;
    }

	public T GetCompProperties<T>() where T : CompProperties
	{
		for( int i = 0; i < comps.Count; i++ )
		{
			var c = comps[i] as T;

			if( c != null )
				return c;
		}

		return null;
	}

	//========================== Loading and resolving ================================

	public override void PostLoad()
	{
		if( graphicData != null )
		{
			LongEventHandler.ExecuteWhenFinished(() =>
				{
					if( graphicData.shaderType == null )
						graphicData.shaderType = ShaderTypeDefOf.Cutout;

					graphic = graphicData.Graphic;

                    if (drawerType != DrawerType.RealtimeOnly)
                    {
                        var atlasGrp = category.ToAtlasGroup();
                        graphic.TryInsertIntoAtlas(atlasGrp);

                        if (atlasGrp == TextureAtlasGroup.Building && Minifiable)
                            graphic.TryInsertIntoAtlas(TextureAtlasGroup.Item); // For lookup when minified
                    }
				});
		}

		//Assign tools ids
		if( tools != null )
		{
			for( int i = 0; i < tools.Count; i++ )
			{
				tools[i].id = i.ToString();
			}
		}

		//Hack: verb inherits my label
		if( verbs != null && verbs.Count == 1 && verbs[0].label.NullOrEmpty()) 
			verbs[0].label = label;

		base.PostLoad();

		//Avoid null refs on things that didn't have a building properties defined
		if( category == ThingCategory.Building && building == null )
			building = new BuildingProperties();

		if( building != null )
			building.PostLoadSpecial(this);

		if( apparel != null )
			apparel.PostLoadSpecial(this);

		if( plant != null )
			plant.PostLoadSpecial(this);

		if (comps != null)
		{
			foreach (var comp in comps)
			{
				comp.PostLoadSpecial(this);
			}
		}
	}

	protected override void ResolveIcon()
	{
		base.ResolveIcon();
		
		if( category == ThingCategory.Pawn )
		{
			if (!race.Humanlike)
			{
				var pawnKind = race.AnyPawnKind;
				if (pawnKind != null)
				{
                    var lifeStage = ModsConfig.BiotechActive && pawnKind.RaceProps.IsMechanoid ? 
                        pawnKind.lifeStages.First() : //show shiny new mech for biotech
                        pawnKind.lifeStages.Last();
					
                    var bodyMat = lifeStage.bodyGraphicData.Graphic.MatAt(Rot4.East);
					uiIcon = (Texture2D)bodyMat.mainTexture;
					uiIconColor = bodyMat.color;
				}
			}
			else
			{
				//No UI icons for humanlikes because they use a special renderer
			}
		}
		else
		{
			//Resolve color
			var stuff = GenStuff.DefaultStuffFor(this);
			if( colorGenerator != null && (stuff == null || stuff.stuffProps.allowColorGenerators) )
				uiIconColor = colorGenerator.ExemplaryColor;
			else if ( stuff != null )
                uiIconColor = GetColorForStuff(stuff);
            else if( graphicData != null )
				uiIconColor = graphicData.color;

			//DrawMatSingle always faces the camera, so we sometimes need to rotate it (e.g. if it's Graphic_Single)
			if( rotatable
				&& graphic != null
				&& graphic != BaseContent.BadGraphic
				&& graphic.ShouldDrawRotated
				&& defaultPlacingRot == Rot4.South )
			{
				uiIconAngle = 180f + graphic.DrawRotatedExtraAngleOffset;
			}
		}
	}

	public override void ResolveReferences()
	{
		base.ResolveReferences();

		if( ingestible != null )
			ingestible.parent = this;

		if( stuffProps != null )
			stuffProps.parent = this;

		if( building != null )
			building.ResolveReferencesSpecial();

		if( graphicData != null )
			graphicData.ResolveReferencesSpecial();

		if( race != null )
			race.ResolveReferencesSpecial();

		if( stuffProps != null )
			stuffProps.ResolveReferencesSpecial();

		//Default sounds
		if( soundImpactDefault == null )
			soundImpactDefault = SoundDefOf.BulletImpact_Ground;	
		if( soundDrop == null )
			soundDrop = SoundDefOf.Standard_Drop;
		if( soundPickup == null )
			soundPickup = SoundDefOf.Standard_Pickup;
		if( soundInteract == null )
			soundInteract = SoundDefOf.Standard_Pickup;

		//Resolve itabs
		if( inspectorTabs != null && inspectorTabs.Any() )
		{
			inspectorTabsResolved = new List<InspectTabBase>();

			for( int i = 0; i < inspectorTabs.Count; i++ )
			{
				try
				{
					inspectorTabsResolved.Add(InspectTabManager.GetSharedInstance(inspectorTabs[i]));
				}
				catch( Exception e )
				{
					Log.Error("Could not instantiate inspector tab of type " + inspectorTabs[i] + ": " + e);
				}
			}
		}

        if (comps != null)
        {
            for (int i = 0; i < comps.Count; i++)
            {
                comps[i].ResolveReferences(this);
            }
        }
	}


	public override IEnumerable<string> ConfigErrors()
	{
		foreach( string str in base.ConfigErrors() )
		{
			yield return str;
		}

		if( category != ThingCategory.Ethereal && label.NullOrEmpty() )
			yield return "no label";

		if( graphicData != null )
		{
			foreach( var err in graphicData.ConfigErrors(this) )
			{
				yield return err;
			}
		}

		if( projectile != null )
		{
			foreach( var err in projectile.ConfigErrors(this) )
			{
				yield return err;
			}
		}
		
		if( statBases != null )
		{
			foreach( var statBase in statBases )
			{
				if( statBases.Where( st=>st.stat == statBase.stat ).Count() > 1 )
					yield return "defines the stat base " + statBase.stat + " more than once.";
			}
		}
		
		if( !BeautyUtility.BeautyRelevant(category) && this.StatBaseDefined(StatDefOf.Beauty) )
			yield return "Beauty stat base is defined, but Things of category " + category + " cannot have beauty.";

		if( !BeautyUtility.BeautyRelevant(category) && this.StatBaseDefined(StatDefOf.BeautyOutdoors) )
			yield return "BeautyOutdoors stat base is defined, but Things of category " + category + " cannot have beauty.";

		if( char.IsNumber(defName[defName.Length-1]) )
			yield return "ends with a numerical digit, which is not allowed on ThingDefs.";

		if( thingClass == null )
			yield return "has null thingClass.";

		if( comps.Count > 0 && !typeof(ThingWithComps).IsAssignableFrom( thingClass ) )	
			yield return "has components but it's thingClass is not a ThingWithComps";

		if( ConnectToPower && drawerType == DrawerType.RealtimeOnly && IsFrame )
			yield return "connects to power but does not add to map mesh. Will not create wire meshes.";

		if( costList != null )
		{
			foreach( ThingDefCountClass cost in costList )
			{
				if( cost.count == 0 )
					yield return "cost in " + cost.thingDef + " is zero.";
			}
		}

		if( thingCategories != null )
		{
			var doubleCat = thingCategories.FirstOrDefault( cat=>thingCategories.Count(c=>c==cat) > 1 );
			if( doubleCat != null )
				yield return "has duplicate thingCategory " + doubleCat + ".";
		}

		if( Fillage == FillCategory.Full && category != ThingCategory.Building )
			yield return "gives full cover but is not a building.";
	
		if( equipmentType != EquipmentType.None )
		{
			if( techLevel == TechLevel.Undefined && !destroyOnDrop )
				yield return "is equipment but has no tech level.";

			if( !comps.Any(c=>c.compClass == typeof(CompEquippable) ) )
				yield return "is equipment but has no CompEquippable";
		}

		if( thingClass == typeof(Bullet) && projectile.damageDef == null )
			yield return " is a bullet but has no damageDef.";

		if( destroyOnDrop )
		{
			if( tradeability != Tradeability.None )
				yield return "destroyOnDrop but tradeability is " + tradeability;
		}

		if( stackLimit > 1 && !drawGUIOverlay )
			yield return "has stackLimit > 1 but also has drawGUIOverlay = false.";

		if( damageMultipliers != null )
		{
			foreach( DamageMultiplier mult in damageMultipliers )
			{
				if( damageMultipliers.Where( m=>m.damageDef == mult.damageDef ).Count() > 1 )
				{
					yield return "has multiple damage multipliers for damageDef " + mult.damageDef;
					break;
				}
			}
		}

		if( Fillage == FillCategory.Full && !this.IsEdifice() )
			yield return "fillPercent is 1.00 but is not edifice";

		if( MadeFromStuff && constructEffect != null )
			yield return "madeFromStuff but has a defined constructEffect (which will always be overridden by stuff's construct animation).";

		if( MadeFromStuff && stuffCategories.NullOrEmpty() )
			yield return "madeFromStuff but has no stuffCategories.";

		if( costList.NullOrEmpty() && costStuffCount <= 0 && recipeMaker != null )
			yield return "has a recipeMaker but no costList or costStuffCount.";

		if( this.GetStatValueAbstract( StatDefOf.DeteriorationRate ) > 0.00001f && !CanEverDeteriorate && !destroyOnDrop )
			yield return "has >0 DeteriorationRate but can't deteriorate.";

		if( smeltProducts != null && !smeltable )
			yield return "has smeltProducts but has smeltable=false";


        if (smeltable
            && smeltProducts.NullOrEmpty()
            && CostList.NullOrEmpty()
            && !IsStuff
            && !MadeFromStuff
            && !destroyOnDrop)
            yield return "is smeltable but does not give anything for smelting.";


		if( equipmentType != EquipmentType.None && verbs.NullOrEmpty() && tools.NullOrEmpty() )
			yield return "is equipment but has no verbs or tools";

		if( Minifiable && thingCategories.NullOrEmpty() )
			yield return "is minifiable but not in any thing category";

		if( category == ThingCategory.Building && !Minifiable && !thingCategories.NullOrEmpty() )
			yield return "is not minifiable yet has thing categories (could be confusing in thing filters because it can't be moved/stored anyway)";
		
		if( !destroyOnDrop &&
            this != ThingDefOf.MinifiedThing &&
            this != ThingDefOf.MinifiedTree &&
			(EverHaulable || Minifiable) &&
			(statBases.NullOrEmpty() || !statBases.Any(s => s.stat == StatDefOf.Mass)) )
			yield return "is haulable, but does not have an authored mass value";

		if( ingestible == null && this.GetStatValueAbstract(StatDefOf.Nutrition) != 0 )
			yield return "has nutrition but ingestible properties are null";

		if( BaseFlammability != 0f && !useHitPoints && category != ThingCategory.Pawn && !destroyOnDrop )
			yield return "flammable but has no hitpoints (will burn indefinitely)";

		if ( graphicData != null && graphicData.shadowData != null  )
		{
			//This works fine in some cases
			//if( castEdgeShadows )
			//	yield return "graphicData defines a shadowInfo but castEdgeShadows is also true";

			if( staticSunShadowHeight > 0)
				yield return "graphicData defines a shadowInfo but staticSunShadowHeight > 0";
		}

		if( saveCompressible && Claimable )
			yield return "claimable item is compressible; faction will be unset after load";
		
		if( deepCommonality > 0 != deepLumpSizeRange.TrueMax > 0 )
			yield return "if deepCommonality or deepLumpSizeRange is set, the other also must be set";

		if( deepCommonality > 0 && deepCountPerPortion <= 0 )
			yield return "deepCommonality > 0 but deepCountPerPortion is not set";

		if( verbs != null )
		{
			for( int i = 0; i < verbs.Count; i++ )
			{
				foreach( var err in verbs[i].ConfigErrors(this) )
				{
					yield return string.Format("verb {0}: {1}", i, err);
				}
			}
		}

		if( building != null )
		{
			foreach( var err in building.ConfigErrors(this) )
			{
				yield return err;
			}
		}

		if( apparel != null )
		{
			foreach( var err in apparel.ConfigErrors(this) )
			{
				yield return err;
			}
		}

		if( comps != null )
		{
			for( int i=0; i<comps.Count; i++ )
			{
				foreach( var err in comps[i].ConfigErrors(this) )
				{
					yield return err;
				}
			}
		}

		if( race != null )
		{
			foreach( var e in race.ConfigErrors(this) )
			{
				yield return e;
			}

            if (race.body != null)
            {
                if( race != null && tools != null )
                {
                    for( int i=0; i<tools.Count; i++ )
                    {
                        if( tools[i].linkedBodyPartsGroup != null && !race.body.AllParts.Any(part=>part.groups.Contains(tools[i].linkedBodyPartsGroup) ) )
                            yield return "has tool with linkedBodyPartsGroup " + tools[i].linkedBodyPartsGroup + " but body " + race.body + " has no parts with that group.";
                    }
                }
            }
		}

		if( ingestible != null )
		{
			foreach( var e in ingestible.ConfigErrors() )
			{
				yield return e;
			}
		}

		if( plant != null )
		{
			foreach( var e in plant.ConfigErrors() )
			{
				yield return e;
			}
		}

		if( tools != null )
		{
			var dupeTool = tools.SelectMany(lhs => tools.Where(rhs => lhs != rhs && lhs.id == rhs.id)).FirstOrDefault();
			if( dupeTool != null )
				yield return string.Format("duplicate thingdef tool id {0}", dupeTool.id);

			foreach( var t in tools )
			{
				foreach( var e in t.ConfigErrors() )
				{
					yield return e;
				}
			}
		}

        if (!randomStyle.NullOrEmpty())
        {
            foreach (var s in randomStyle)
            {
                if (s.Chance <= 0)
                    yield return "style chance <= 0.";
            }

            if (!comps.Any(c => c.compClass == typeof(CompStyleable)))
                yield return "random style assigned, but missing CompStyleable!";
        }

        if (relicChance > 0 && category != ThingCategory.Item)
        {
            yield return "relic chance > 0 but category != item";
        }

        if (hasInteractionCell && !multipleInteractionCellOffsets.NullOrEmpty())
        {
            yield return "both single and multiple interaction cells are defined, it should be one or the other";
        }
	}

	public static ThingDef Named(string defName)
	{
		return DefDatabase<ThingDef>.GetNamed(defName);
	}
        
    //========================== Misc ================================

    public string LabelAsStuff
    {
        get
        {
            if (!stuffProps.stuffAdjective.NullOrEmpty())
            {
                return stuffProps.stuffAdjective;
            }
            else
            {
                return label;
            }
        }
    }

	public bool IsWithinCategory(ThingCategoryDef category)
	{
		if( thingCategories == null )
			return false;

		for( int i = 0; i < thingCategories.Count; ++i )
		{
			var cur = thingCategories[i];
			while( cur != null )
			{
				if( cur == category )
					return true;
				
				cur = cur.parent;
			}
		}

		return false;
	}

    public void Notify_UnlockedByResearch()
    {
        if(comps != null)
        {
            for(var i = 0; i < comps.Count; i++)
            {
                comps[i].Notify_PostUnlockedByResearch(this);
            }
        }
    }

	//===========================================================================
	//=========================== Info card stats ===============================
	//===========================================================================

	public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
    {
		foreach( var stat in base.SpecialDisplayStats(req) )
		{
			yield return stat;
		}

		if(apparel != null)
        {
            string coveredParts = apparel.GetCoveredOuterPartsString(BodyDefOf.Human);
            yield return new StatDrawEntry( StatCategoryDefOf.Apparel, "Covers".Translate(), coveredParts, "Stat_Thing_Apparel_Covers_Desc".Translate(), StatDisplayOrder.Thing_Apparel_Covers);

			yield return new StatDrawEntry( StatCategoryDefOf.Apparel, "Layer".Translate(), apparel.GetLayersString(), "Stat_Thing_Apparel_Layer_Desc".Translate(), StatDisplayOrder.Thing_Apparel_Layer);
			yield return new StatDrawEntry( StatCategoryDefOf.Apparel, "Stat_Thing_Apparel_CountsAsClothingNudity_Name".Translate(), apparel.countsAsClothingForNudity ? "Yes".Translate() : "No".Translate(), "Stat_Thing_Apparel_CountsAsClothingNudity_Desc".Translate(), StatDisplayOrder.Thing_Apparel_CountsAsClothingNudity);
            if( ModsConfig.BiotechActive )
            {
                yield return new StatDrawEntry(StatCategoryDefOf.Apparel,
                    "Stat_Thing_Apparel_ValidLifestage".Translate(),
                    apparel.developmentalStageFilter.ToCommaList().CapitalizeFirst(),
                    "Stat_Thing_Apparel_ValidLifestage_Desc".Translate(),
                    StatDisplayOrder.Thing_Apparel_ValidLifestage);
            }
        }

		if( IsMedicine && MedicineTendXpGainFactor != 1f )
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "MedicineXpGainFactor".Translate(), MedicineTendXpGainFactor.ToStringPercent(), "Stat_Thing_Drug_MedicineXpGainFactor_Desc".Translate(), StatDisplayOrder.Thing_Drug_MedicineXpGainFactor);

		if( (fillPercent > 0 && (category == ThingCategory.Item || category == ThingCategory.Building || category == ThingCategory.Plant)))
		{
			var sde = new StatDrawEntry(StatCategoryDefOf.Basics, "CoverEffectiveness".Translate(), this.BaseBlockChance().ToStringPercent(), "CoverEffectivenessExplanation".Translate(), StatDisplayOrder.Thing_CoverEffectiveness);
			yield return sde;
		}

		if( constructionSkillPrerequisite > 0 )
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "SkillRequiredToBuild".Translate(SkillDefOf.Construction.LabelCap), constructionSkillPrerequisite.ToString(), "SkillRequiredToBuildExplanation".Translate(SkillDefOf.Construction.LabelCap), StatDisplayOrder.Thing_ConstructionSkillRequired);

		if( artisticSkillPrerequisite > 0 )
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "SkillRequiredToBuild".Translate(SkillDefOf.Artistic.LabelCap), artisticSkillPrerequisite.ToString(), "SkillRequiredToBuildExplanation".Translate(SkillDefOf.Artistic.LabelCap), StatDisplayOrder.Thing_ConstructionSkillRequired);

		var recipeUsers = DefDatabase<RecipeDef>.AllDefsListForReading.Where(r => r.recipeUsers != null && r.products.Count == 1 && r.products.Any(p => p.thingDef == this) && !r.IsSurgery)
			.SelectMany(r => r.recipeUsers)
			.Select(u => u.label)
            .Distinct()
			.ToArray();
		
		if ( recipeUsers.Any() )
		{
			var createdAt = GenText.ToCommaList(recipeUsers).CapitalizeFirst();
			yield return new StatDrawEntry( StatCategoryDefOf.Basics, "CreatedAt".Translate(), createdAt, "Stat_Thing_CreatedAt_Desc".Translate(), StatDisplayOrder.Thing_CreatedAt);
		}

		// Sleeping spots
		if (this.thingClass != null && typeof(Building_Bed).IsAssignableFrom(this.thingClass) && !this.statBases.StatListContains(StatDefOf.BedRestEffectiveness))
			yield return new StatDrawEntry(StatCategoryDefOf.Building, StatDefOf.BedRestEffectiveness, StatDefOf.BedRestEffectiveness.valueIfMissing, StatRequest.ForEmpty());

        if( !verbs.NullOrEmpty() )
        {
            var verb = verbs.First(x => x.isPrimary);

            //Verbs can be native verbs held by pawns, or weapon verbs
            StatCategoryDef verbStatCategory = category == ThingCategory.Pawn ? StatCategoryDefOf.PawnCombat : null;

			float warmup = verb.warmupTime;
			if( warmup > 0 )
            {
                yield return new StatDrawEntry( verbStatCategory ?? StatCategoryDefOf.Weapon_Ranged, "RangedWarmupTime".Translate(), warmup.ToString("0.##") + " " + "LetterSecond".Translate(), "Stat_Thing_Weapon_RangedWarmupTime_Desc".Translate(), StatDisplayOrder.Thing_Weapon_MeleeWarmupTime);
            }

			//NOTE: this won't work with custom projectiles, e.g. mortars
            if(verb.defaultProjectile != null && verb.defaultProjectile.projectile.damageDef.harmsHealth)
			{
                var statCat = verbStatCategory ?? StatCategoryDefOf.Weapon_Ranged;
				var damageAmountExplanation = new StringBuilder();
				damageAmountExplanation.AppendLine("Stat_Thing_Damage_Desc".Translate());
				damageAmountExplanation.AppendLine();
				float dam = verb.defaultProjectile.projectile.GetDamageAmount(req.Thing, damageAmountExplanation);
                yield return new StatDrawEntry(statCat, "Damage".Translate(), dam.ToString(),damageAmountExplanation.ToString(), StatDisplayOrder.Thing_Damage);

				if( verb.defaultProjectile.projectile.damageDef.armorCategory != null )
				{
					var armorPenetrationExplanation = new StringBuilder();
					float ap = verb.defaultProjectile.projectile.GetArmorPenetration(req.Thing, armorPenetrationExplanation);
					var fullExplanation = "ArmorPenetrationExplanation".Translate();
					if( armorPenetrationExplanation.Length != 0 )
						fullExplanation += "\n\n" + armorPenetrationExplanation;
                    yield return new StatDrawEntry(statCat, "ArmorPenetration".Translate(), ap.ToStringPercent(), fullExplanation, StatDisplayOrder.Thing_Weapon_ArmorPenetration );
				}

                //Damage vs buildings
                var dmgBuildings = verb.defaultProjectile.projectile.damageDef.buildingDamageFactor;
                var dmgBuildingsImpassable = verb.defaultProjectile.projectile.damageDef.buildingDamageFactorImpassable;
                var dmgBuildingsPassable = verb.defaultProjectile.projectile.damageDef.buildingDamageFactorPassable;
                if (dmgBuildings != 1)
                    yield return new StatDrawEntry(statCat, "BuildingDamageFactor".Translate(), dmgBuildings.ToStringPercent(), "BuildingDamageFactorExplanation".Translate(), StatDisplayOrder.Thing_Weapon_BuildingDamageFactor);
                if (dmgBuildingsImpassable != 1)
                    yield return new StatDrawEntry(statCat, "BuildingDamageFactorImpassable".Translate(), dmgBuildingsImpassable.ToStringPercent(), "BuildingDamageFactorImpassableExplanation".Translate(), StatDisplayOrder.Thing_WeaponBuildingDamageFactorImpassable);
                if (dmgBuildingsPassable != 1)
                    yield return new StatDrawEntry(statCat, "BuildingDamageFactorPassable".Translate(), dmgBuildingsPassable.ToStringPercent(), "BuildingDamageFactorPassableExplanation".Translate(), StatDisplayOrder.Thing_WeaponBuildingDamageFactorPassable);
			}

            if (verb.defaultProjectile == null && verb.beamDamageDef != null)
            {
                yield return new StatDrawEntry(verbStatCategory ?? StatCategoryDefOf.Weapon_Ranged,
                    "ArmorPenetration".Translate(),
                    verb.beamDamageDef.defaultArmorPenetration.ToStringPercent(),
                    "ArmorPenetrationExplanation".Translate(),
                    StatDisplayOrder.Thing_Weapon_ArmorPenetration );
            }
  
            if(verb.Ranged)
            {
                int burstShotCount = verb.burstShotCount;
                float burstShotFireRate = 60f / verb.ticksBetweenBurstShots.TicksToSeconds();
                float range = verb.range;
                var statCat = verbStatCategory ?? StatCategoryDefOf.Weapon_Ranged;

                if(burstShotCount > 1)
                {
                    yield return new StatDrawEntry( statCat, "BurstShotCount".Translate(), burstShotCount.ToString(), "Stat_Thing_Weapon_BurstShotCount_Desc".Translate(), StatDisplayOrder.Thing_Weapon_BurstShotCount);
                    yield return new StatDrawEntry( statCat, "BurstShotFireRate".Translate(), burstShotFireRate.ToString("0.##") + " rpm", "Stat_Thing_Weapon_BurstShotFireRate_Desc".Translate(), StatDisplayOrder.Thing_Weapon_BurstShotFireRate);
                }

				//We round range to the nearest whole number; we don't want to show the ".9"'s we use to avoid weird-shaped
				//max-range circles
                yield return new StatDrawEntry( statCat, "Range".Translate(), range.ToString("F0"), "Stat_Thing_Weapon_Range_Desc".Translate(), StatDisplayOrder.Thing_Weapon_Range);

				if( verb.defaultProjectile != null && verb.defaultProjectile.projectile != null && verb.defaultProjectile.projectile.stoppingPower != 0f )
					yield return new StatDrawEntry( statCat, "StoppingPower".Translate(), verb.defaultProjectile.projectile.stoppingPower.ToString("F1"), "StoppingPowerExplanation".Translate(), StatDisplayOrder.Thing_Weapon_StoppingPower );
            }

            if( verb.ForcedMissRadius > 0f )
            {
                var statCat = verbStatCategory ?? StatCategoryDefOf.Weapon_Ranged;

                yield return new StatDrawEntry(statCat, "MissRadius".Translate(), verb.ForcedMissRadius.ToString("0.#"), "Stat_Thing_Weapon_MissRadius_Desc".Translate(), StatDisplayOrder.Thing_Weapon_MissRadius);
                yield return new StatDrawEntry(statCat, "DirectHitChance".Translate(), (1f / GenRadial.NumCellsInRadius(verb.ForcedMissRadius)).ToStringPercent(), "Stat_Thing_Weapon_DirectHitChance_Desc".Translate(), StatDisplayOrder.Thing_Weapon_DirectHitChance);
            }
        }
            
        if( plant != null )
        {
			foreach( var s in plant.SpecialDisplayStats() )
			{
				yield return s;
			}  
        }
            
        if( ingestible != null )
        {
			foreach( var s in ingestible.SpecialDisplayStats() )
			{
				yield return s;
			}
        }
            
        if( race != null )
        {
			foreach( var s in race.SpecialDisplayStats(this, req) )
			{
				yield return s;
			}
        }

		if( building != null )
		{
			foreach( var s in building.SpecialDisplayStats(this, req) )
			{
				yield return s;
			}
		}
                   
        if( isTechHediff )
        {
            //We have to iterate through all recipes to see where this body part item or implant is used
			var recipes = DefDatabase<RecipeDef>.AllDefs.Where(x => x.addsHediff != null && x.IsIngredient(this));
            foreach( var s in MedicalRecipesUtility.GetMedicalStatsFromRecipeDefs(recipes) )
                yield return s;
        }
        
		for( int i = 0; i < comps.Count; i++ )
		{
			foreach( var s in comps[i].SpecialDisplayStats(req) )
			{
				yield return s;
			}
		}

        if (building != null)
        {
            if ( building.mineableThing != null)
            {
                var hyperlinks = new Dialog_InfoCard.Hyperlink[] { new Dialog_InfoCard.Hyperlink(building.mineableThing) };

                yield return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, "Stat_MineableThing_Name".Translate(), building.mineableThing.LabelCap , "Stat_MineableThing_Desc".Translate(), StatDisplayOrder.Thing_Mineable, null, hyperlinks);

                StringBuilder yieldReportText = new StringBuilder();
                yieldReportText.AppendLine("Stat_MiningYield_Desc".Translate());
                yieldReportText.AppendLine();
                yieldReportText.AppendLine("StatsReport_DifficultyMultiplier".Translate(Find.Storyteller.difficultyDef.label) + ": " + Find.Storyteller.difficulty.mineYieldFactor.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Factor));
                yield return new StatDrawEntry( StatCategoryDefOf.Basics, "Stat_MiningYield_Name".Translate(), Mathf.CeilToInt(building.EffectiveMineableYield).ToString("F0"), yieldReportText.ToString(), StatDisplayOrder.Thing_Mineable, null, hyperlinks);
            }

            if ( building.IsTurret)
            {
                var turret = building.turretGunDef;
                yield return new StatDrawEntry(StatCategoryDefOf.BasicsImportant, "Stat_Weapon_Name".Translate(), turret.LabelCap, "Stat_Weapon_Desc".Translate(), StatDisplayOrder.Thing_Weapon, null, new [] { new Dialog_InfoCard.Hyperlink(turret) });
                
                //Turret gun stats
                StatRequest request = StatRequest.For(turret, null);
                foreach (var s in turret.SpecialDisplayStats(request))
                {
                    if (s.category == StatCategoryDefOf.Weapon_Ranged)
                        yield return s;
                }

                for (int i = 0; i < turret.statBases.Count; i++)
                {
                    var statMod = turret.statBases[i];
                    if (statMod.stat.category == StatCategoryDefOf.Weapon_Ranged)
                        yield return new StatDrawEntry(StatCategoryDefOf.Weapon_Ranged, statMod.stat, statMod.value, request);
                }
            }
        }

        if (IsMeat)
		{
			List<ThingDef> pawnKinds = new List<ThingDef>();
			foreach (var thingDef in DefDatabase<ThingDef>.AllDefs)
			{
				if (thingDef.race != null && thingDef.race.meatDef == this)
					pawnKinds.Add(thingDef);
			}

			string label = string.Join(", ", pawnKinds.Select((p) => p.label).ToArray()).CapitalizeFirst();
			yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "Stat_SourceSpecies_Name".Translate(), label , "Stat_SourceSpecies_Desc".Translate(), StatDisplayOrder.Thing_Meat_SourceSpecies, null, Dialog_InfoCard.DefsToHyperlinks(pawnKinds));
		}

		if (IsLeather)
		{
			List<ThingDef> pawnKinds = new List<ThingDef>();
			foreach (var thingDef in DefDatabase<ThingDef>.AllDefs)
			{
				if (thingDef.race != null && thingDef.race.leatherDef == this)
					pawnKinds.Add(thingDef);
			}

			string label = string.Join(", ", pawnKinds.Select((p) => p.label).ToArray()).CapitalizeFirst();
			yield return new StatDrawEntry(StatCategoryDefOf.BasicsPawn, "Stat_SourceSpecies_Name".Translate(), label, "Stat_SourceSpecies_Desc".Translate(), StatDisplayOrder.Thing_Meat_SourceSpecies,
				null, Dialog_InfoCard.DefsToHyperlinks(pawnKinds));
		}
        
        //Now print out the equippedStatOffsets (how I-as-equipment offset pawn's stats when equipped)
        if( !equippedStatOffsets.NullOrEmpty() )
        {
            for( int i=0; i<equippedStatOffsets.Count; i++ )
            {
                var stat = equippedStatOffsets[i].stat;
                var val = equippedStatOffsets[i].value;
                var explanation = new StringBuilder(stat.description);
                
                if( req.HasThing && stat.Worker != null )
                {
                    explanation.AppendLine();
                    explanation.AppendLine();
                    explanation.AppendLine("StatsReport_BaseValue".Translate() + ": " + stat.ValueToString(val, ToStringNumberSense.Offset, stat.finalizeEquippedStatOffset));
                    
                    val = StatWorker.StatOffsetFromGear(req.Thing, stat);

                    if( !stat.parts.NullOrEmpty() )
                    {
                        explanation.AppendLine();
                        
                    	for( int p = 0; p < stat.parts.Count; p++ )
                    	{
                    		var exPart = stat.parts[p].ExplanationPart(req);
            
                    		if( !exPart.NullOrEmpty() )
                    		{
                                explanation.AppendLine( exPart );
                    		}
                    	}
                    }
                    
                    explanation.AppendLine();
                    explanation.AppendLine("StatsReport_FinalValue".Translate() + ": " + stat.ValueToString(val, ToStringNumberSense.Offset, !stat.formatString.NullOrEmpty()));
                }

                yield return new StatDrawEntry( StatCategoryDefOf.EquippedStatOffsets,
                    equippedStatOffsets[i].stat,
                    val,
                    StatRequest.ForEmpty(),
                    numberSense: ToStringNumberSense.Offset,
                    forceUnfinalizedMode: true)
                    .SetReportText(explanation.ToString());
            }
        }

        if (IsDrug)
        {
            foreach (var s in DrugStatsUtility.SpecialDisplayStats(this))
            {
                yield return s;
            }
        }
	}

}
}
