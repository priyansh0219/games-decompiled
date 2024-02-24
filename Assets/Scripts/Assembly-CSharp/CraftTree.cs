using System.Collections.Generic;
using Gendarme;

public class CraftTree
{
	public enum Type
	{
		None = 0,
		Fabricator = 1,
		Constructor = 2,
		Workbench = 3,
		Unused1 = 4,
		Unused2 = 5,
		SeamothUpgrades = 6,
		MapRoom = 7,
		Centrifuge = 8,
		CyclopsFabricator = 9,
		Rocket = 10
	}

	private static HashSet<TechType> craftableTech;

	private static CraftTree fabricator;

	private static CraftTree constructor;

	private static CraftTree workbench;

	private static CraftTree seamothUpgrades;

	private static CraftTree mapRoom;

	private static CraftTree centrifuge;

	private static CraftTree cyclopsFabricator;

	private static bool initialized;

	public string id { get; private set; }

	public CraftNode nodes { get; private set; }

	private static CraftNode FabricatorScheme()
	{
		return new CraftNode("Root").AddNode(new CraftNode("Resources", TreeAction.Expand).AddNode(new CraftNode("BasicMaterials", TreeAction.Expand).AddNode(new CraftNode("Titanium", TreeAction.Craft, TechType.Titanium), new CraftNode("TitaniumIngot", TreeAction.Craft, TechType.TitaniumIngot), new CraftNode("FiberMesh", TreeAction.Craft, TechType.FiberMesh), new CraftNode("Silicone", TreeAction.Craft, TechType.Silicone), new CraftNode("Glass", TreeAction.Craft, TechType.Glass), new CraftNode("Bleach", TreeAction.Craft, TechType.Bleach), new CraftNode("Lubricant", TreeAction.Craft, TechType.Lubricant), new CraftNode("EnameledGlass", TreeAction.Craft, TechType.EnameledGlass), new CraftNode("PlasteelIngot", TreeAction.Craft, TechType.PlasteelIngot)), new CraftNode("AdvancedMaterials", TreeAction.Expand).AddNode(new CraftNode("HydrochloricAcid", TreeAction.Craft, TechType.HydrochloricAcid), new CraftNode("Benzene", TreeAction.Craft, TechType.Benzene), new CraftNode("AramidFibers", TreeAction.Craft, TechType.AramidFibers), new CraftNode("Aerogel", TreeAction.Craft, TechType.Aerogel), new CraftNode("Polyaniline", TreeAction.Craft, TechType.Polyaniline), new CraftNode("HatchingEnzymes", TreeAction.Craft, TechType.HatchingEnzymes)), new CraftNode("Electronics", TreeAction.Expand).AddNode(new CraftNode("CopperWire", TreeAction.Craft, TechType.CopperWire), new CraftNode("Battery", TreeAction.Craft, TechType.Battery), new CraftNode("PrecursorIonBattery", TreeAction.Craft, TechType.PrecursorIonBattery), new CraftNode("PowerCell", TreeAction.Craft, TechType.PowerCell), new CraftNode("PrecursorIonPowerCell", TreeAction.Craft, TechType.PrecursorIonPowerCell), new CraftNode("ComputerChip", TreeAction.Craft, TechType.ComputerChip), new CraftNode("WiringKit", TreeAction.Craft, TechType.WiringKit), new CraftNode("AdvancedWiringKit", TreeAction.Craft, TechType.AdvancedWiringKit), new CraftNode("ReactorRod", TreeAction.Craft, TechType.ReactorRod))), new CraftNode("Survival", TreeAction.Expand).AddNode(new CraftNode("Water", TreeAction.Expand).AddNode(new CraftNode("FilteredWater", TreeAction.Craft, TechType.FilteredWater), new CraftNode("DisinfectedWater", TreeAction.Craft, TechType.DisinfectedWater)), new CraftNode("CookedFood", TreeAction.Expand).AddNode(new CraftNode("CookedHoleFish", TreeAction.Craft, TechType.CookedHoleFish), new CraftNode("CookedPeeper", TreeAction.Craft, TechType.CookedPeeper), new CraftNode("CookedBladderfish", TreeAction.Craft, TechType.CookedBladderfish), new CraftNode("CookedGarryFish", TreeAction.Craft, TechType.CookedGarryFish), new CraftNode("CookedHoverfish", TreeAction.Craft, TechType.CookedHoverfish), new CraftNode("CookedReginald", TreeAction.Craft, TechType.CookedReginald), new CraftNode("CookedSpadefish", TreeAction.Craft, TechType.CookedSpadefish), new CraftNode("CookedBoomerang", TreeAction.Craft, TechType.CookedBoomerang), new CraftNode("CookedLavaBoomerang", TreeAction.Craft, TechType.CookedLavaBoomerang), new CraftNode("CookedEyeye", TreeAction.Craft, TechType.CookedEyeye), new CraftNode("CookedLavaEyeye", TreeAction.Craft, TechType.CookedLavaEyeye), new CraftNode("CookedOculus", TreeAction.Craft, TechType.CookedOculus), new CraftNode("CookedHoopfish", TreeAction.Craft, TechType.CookedHoopfish), new CraftNode("CookedSpinefish", TreeAction.Craft, TechType.CookedSpinefish)), new CraftNode("CuredFood", TreeAction.Expand).AddNode(new CraftNode("CuredHoleFish", TreeAction.Craft, TechType.CuredHoleFish), new CraftNode("CuredPeeper", TreeAction.Craft, TechType.CuredPeeper), new CraftNode("CuredBladderfish", TreeAction.Craft, TechType.CuredBladderfish), new CraftNode("CuredGarryFish", TreeAction.Craft, TechType.CuredGarryFish), new CraftNode("CuredHoverfish", TreeAction.Craft, TechType.CuredHoverfish), new CraftNode("CuredReginald", TreeAction.Craft, TechType.CuredReginald), new CraftNode("CuredSpadefish", TreeAction.Craft, TechType.CuredSpadefish), new CraftNode("CuredBoomerang", TreeAction.Craft, TechType.CuredBoomerang), new CraftNode("CuredLavaBoomerang", TreeAction.Craft, TechType.CuredLavaBoomerang), new CraftNode("CuredEyeye", TreeAction.Craft, TechType.CuredEyeye), new CraftNode("CuredLavaEyeye", TreeAction.Craft, TechType.CuredLavaEyeye), new CraftNode("CuredOculus", TreeAction.Craft, TechType.CuredOculus), new CraftNode("CuredHoopfish", TreeAction.Craft, TechType.CuredHoopfish), new CraftNode("CuredSpinefish", TreeAction.Craft, TechType.CuredSpinefish))), new CraftNode("Personal", TreeAction.Expand).AddNode(new CraftNode("Equipment", TreeAction.Expand).AddNode(new CraftNode("Tank", TreeAction.Craft, TechType.Tank), new CraftNode("DoubleTank", TreeAction.Craft, TechType.DoubleTank), new CraftNode("Fins", TreeAction.Craft, TechType.Fins), new CraftNode("RadiationSuit", TreeAction.Craft, TechType.RadiationSuit), new CraftNode("ReinforcedDiveSuit", TreeAction.Craft, TechType.ReinforcedDiveSuit), new CraftNode("Stillsuit", TreeAction.Craft, TechType.WaterFiltrationSuit), new CraftNode("FirstAidKit", TreeAction.Craft, TechType.FirstAidKit), new CraftNode("FireExtinguisher", TreeAction.Craft, TechType.FireExtinguisher), new CraftNode("Rebreather", TreeAction.Craft, TechType.Rebreather), new CraftNode("Compass", TreeAction.Craft, TechType.Compass), new CraftNode("Pipe", TreeAction.Craft, TechType.Pipe), new CraftNode("PipeSurfaceFloater", TreeAction.Craft, TechType.PipeSurfaceFloater), new CraftNode("PrecursorKey_Purple", TreeAction.Craft, TechType.PrecursorKey_Purple), new CraftNode("PrecursorKey_Blue", TreeAction.Craft, TechType.PrecursorKey_Blue), new CraftNode("PrecursorKey_Orange", TreeAction.Craft, TechType.PrecursorKey_Orange)), new CraftNode("Tools", TreeAction.Expand).AddNode(new CraftNode("Scanner", TreeAction.Craft, TechType.Scanner), new CraftNode("Welder", TreeAction.Craft, TechType.Welder), new CraftNode("Flashlight", TreeAction.Craft, TechType.Flashlight), new CraftNode("Knife", TreeAction.Craft, TechType.Knife), new CraftNode("DiveReel", TreeAction.Craft, TechType.DiveReel), new CraftNode("AirBladder", TreeAction.Craft, TechType.AirBladder), new CraftNode("Flare", TreeAction.Craft, TechType.Flare), new CraftNode("Builder", TreeAction.Craft, TechType.Builder), new CraftNode("LaserCutter", TreeAction.Craft, TechType.LaserCutter), new CraftNode("StasisRifle", TreeAction.Craft, TechType.StasisRifle), new CraftNode("PropulsionCannon", TreeAction.Craft, TechType.PropulsionCannon), new CraftNode("LEDLight", TreeAction.Craft, TechType.LEDLight))), new CraftNode("Machines", TreeAction.Expand).AddNode(new CraftNode("Seaglide", TreeAction.Craft, TechType.Seaglide), new CraftNode("Constructor", TreeAction.Craft, TechType.Constructor), new CraftNode("Beacon", TreeAction.Craft, TechType.Beacon), new CraftNode("SmallStorage", TreeAction.Craft, TechType.SmallStorage), new CraftNode("Gravsphere", TreeAction.Craft, TechType.Gravsphere), new CraftNode("CyclopsDecoy", TreeAction.Craft, TechType.CyclopsDecoy)));
	}

	private static CraftNode ConstructorScheme()
	{
		return new CraftNode("Root").AddNode(new CraftNode("Vehicles", TreeAction.Expand).AddNode(new CraftNode("Seamoth", TreeAction.Craft, TechType.Seamoth), new CraftNode("Cyclops", TreeAction.Craft, TechType.Cyclops), new CraftNode("Exosuit", TreeAction.Craft, TechType.Exosuit)), new CraftNode("Rocket", TreeAction.Expand).AddNode(new CraftNode("RocketBase", TreeAction.Craft, TechType.RocketBase)));
	}

	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	private static CraftNode RocketScheme()
	{
		return new CraftNode("Root").AddNode(new CraftNode("RocketStage1", TreeAction.Craft, TechType.RocketStage1), new CraftNode("RocketStage2", TreeAction.Craft, TechType.RocketStage2), new CraftNode("RocketStage3", TreeAction.Craft, TechType.RocketStage3));
	}

	private static CraftNode WorkbenchScheme()
	{
		return new CraftNode("Root").AddNode(new CraftNode("LithiumIonBattery", TreeAction.Craft, TechType.LithiumIonBattery), new CraftNode("HeatBlade", TreeAction.Craft, TechType.HeatBlade), new CraftNode("PlasteelTank", TreeAction.Craft, TechType.PlasteelTank), new CraftNode("HighCapacityTank", TreeAction.Craft, TechType.HighCapacityTank), new CraftNode("UltraGlideFins", TreeAction.Craft, TechType.UltraGlideFins), new CraftNode("SwimChargeFins", TreeAction.Craft, TechType.SwimChargeFins), new CraftNode("RepulsionCannon", TreeAction.Craft, TechType.RepulsionCannon), new CraftNode("CyclopsHullModule2", TreeAction.Craft, TechType.CyclopsHullModule2), new CraftNode("CyclopsHullModule3", TreeAction.Craft, TechType.CyclopsHullModule3), new CraftNode("SeamothHullModule2", TreeAction.Craft, TechType.VehicleHullModule2), new CraftNode("SeamothHullModule3", TreeAction.Craft, TechType.VehicleHullModule3), new CraftNode("ExoHullModule2", TreeAction.Craft, TechType.ExoHullModule2));
	}

	private static CraftNode SeamothUpgradesScheme()
	{
		return new CraftNode("Root").AddNode(new CraftNode("CommonModules", TreeAction.Expand).AddNode(new CraftNode("VehicleArmorPlating", TreeAction.Craft, TechType.VehicleArmorPlating), new CraftNode("VehiclePowerUpgradeModule", TreeAction.Craft, TechType.VehiclePowerUpgradeModule), new CraftNode("VehicleStorageModule", TreeAction.Craft, TechType.VehicleStorageModule)), new CraftNode("SeamothModules", TreeAction.Expand).AddNode(new CraftNode("VehicleHullModule1", TreeAction.Craft, TechType.VehicleHullModule1), new CraftNode("SeamothSolarCharge", TreeAction.Craft, TechType.SeamothSolarCharge), new CraftNode("SeamothElectricalDefense", TreeAction.Craft, TechType.SeamothElectricalDefense), new CraftNode("SeamothTorpedoModule", TreeAction.Craft, TechType.SeamothTorpedoModule), new CraftNode("SeamothSonarModule", TreeAction.Craft, TechType.SeamothSonarModule)), new CraftNode("ExosuitModules", TreeAction.Expand).AddNode(new CraftNode("ExoHullModule1", TreeAction.Craft, TechType.ExoHullModule1), new CraftNode("ExosuitThermalReactorModule", TreeAction.Craft, TechType.ExosuitThermalReactorModule), new CraftNode("ExosuitJetUpgradeModule", TreeAction.Craft, TechType.ExosuitJetUpgradeModule), new CraftNode("ExosuitPropulsionArmModule", TreeAction.Craft, TechType.ExosuitPropulsionArmModule), new CraftNode("ExosuitGrapplingArmModule", TreeAction.Craft, TechType.ExosuitGrapplingArmModule), new CraftNode("ExosuitDrillArmModule", TreeAction.Craft, TechType.ExosuitDrillArmModule), new CraftNode("ExosuitTorpedoArmModule", TreeAction.Craft, TechType.ExosuitTorpedoArmModule)), new CraftNode("Torpedoes", TreeAction.Expand).AddNode(new CraftNode("WhirlpoolTorpedo", TreeAction.Craft, TechType.WhirlpoolTorpedo), new CraftNode("GasTorpedo", TreeAction.Craft, TechType.GasTorpedo)));
	}

	private static CraftNode MapRoomSheme()
	{
		return new CraftNode("Root").AddNode(new CraftNode("MapRoomHUDChip", TreeAction.Craft, TechType.MapRoomHUDChip), new CraftNode("MapRoomCamera", TreeAction.Craft, TechType.MapRoomCamera), new CraftNode("MapRoomUpgradeScanRange", TreeAction.Craft, TechType.MapRoomUpgradeScanRange), new CraftNode("MapRoomUpgradeScanSpeed", TreeAction.Craft, TechType.MapRoomUpgradeScanSpeed));
	}

	private static CraftNode CentrifugeScheme()
	{
		return new CraftNode("Root").AddNode(new CraftNode("Placeholder1", TreeAction.Craft, TechType.Titanium), new CraftNode("Placeholder2", TreeAction.Craft, TechType.Titanium));
	}

	private static CraftNode CyclopsFabricatorScheme()
	{
		return new CraftNode("Root").AddNode(new CraftNode("CyclopsHullModule1", TreeAction.Craft, TechType.CyclopsHullModule1), new CraftNode("PowerUpgradeModule", TreeAction.Craft, TechType.PowerUpgradeModule), new CraftNode("CyclopsShieldModule", TreeAction.Craft, TechType.CyclopsShieldModule), new CraftNode("CyclopsSonarModule", TreeAction.Craft, TechType.CyclopsSonarModule), new CraftNode("CyclopsSeamothRepairModule", TreeAction.Craft, TechType.CyclopsSeamothRepairModule), new CraftNode("CyclopsFireSuppressionModule", TreeAction.Craft, TechType.CyclopsFireSuppressionModule), new CraftNode("CyclopsDecoyModule", TreeAction.Craft, TechType.CyclopsDecoyModule), new CraftNode("CyclopsThermalReactorModule", TreeAction.Craft, TechType.CyclopsThermalReactorModule));
	}

	public CraftTree(string id, CraftNode scheme)
	{
		this.id = id;
		nodes = scheme;
	}

	private static void AddToCraftableTech(CraftTree tree)
	{
		using (IEnumerator<CraftNode> enumerator = tree.nodes.Traverse(includeSelf: false))
		{
			while (enumerator.MoveNext())
			{
				CraftNode current = enumerator.Current;
				if (current.action == TreeAction.Craft)
				{
					TechType techType = current.techType0;
					if (techType != 0)
					{
						craftableTech.Add(techType);
					}
				}
			}
		}
	}

	public static void Initialize()
	{
		if (!initialized)
		{
			initialized = true;
			fabricator = new CraftTree("Fabricator", FabricatorScheme());
			constructor = new CraftTree("Constructor", ConstructorScheme());
			workbench = new CraftTree("Workbench", WorkbenchScheme());
			seamothUpgrades = new CraftTree("SeamothUpgrades", SeamothUpgradesScheme());
			mapRoom = new CraftTree("MapRoom", MapRoomSheme());
			centrifuge = new CraftTree("Centrifuge", CentrifugeScheme());
			cyclopsFabricator = new CraftTree("CyclopsFabricator", CyclopsFabricatorScheme());
			craftableTech = new HashSet<TechType>();
			AddToCraftableTech(fabricator);
			AddToCraftableTech(constructor);
			AddToCraftableTech(workbench);
			AddToCraftableTech(seamothUpgrades);
			AddToCraftableTech(mapRoom);
			AddToCraftableTech(centrifuge);
			AddToCraftableTech(cyclopsFabricator);
		}
	}

	private static bool NodeHasKnown(CraftNode src)
	{
		if (KnownTech.Contains(src.techType0))
		{
			return true;
		}
		foreach (CraftNode item in src)
		{
			if (NodeHasKnown(item))
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasKnown(Type treeType)
	{
		foreach (CraftNode node in GetTree(treeType).nodes)
		{
			if (NodeHasKnown(node))
			{
				return true;
			}
		}
		return false;
	}

	public static CraftTree GetTree(Type treeType)
	{
		Initialize();
		switch (treeType)
		{
		case Type.Fabricator:
			return fabricator;
		case Type.Constructor:
			return constructor;
		case Type.Workbench:
			return workbench;
		case Type.MapRoom:
			return mapRoom;
		case Type.SeamothUpgrades:
			return seamothUpgrades;
		case Type.Centrifuge:
			return centrifuge;
		case Type.CyclopsFabricator:
			return cyclopsFabricator;
		default:
			return null;
		}
	}

	public static bool IsCraftable(TechType techType)
	{
		Initialize();
		return craftableTech.Contains(techType);
	}
}
