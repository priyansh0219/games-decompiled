using System.Linq;
using Verse;

namespace RimWorld
{
	public static class DefGenerator
	{
		public static int StandardItemPathCost = 14;

		public static void GenerateImpliedDefs_PreResolve()
		{
			foreach (TerrainDef item in TerrainDefGenerator_Carpet.ImpliedTerrainDefs())
			{
				AddImpliedDef(item);
			}
			foreach (ThingDef item2 in ThingDefGenerator_Buildings.ImpliedBlueprintAndFrameDefs().Concat(ThingDefGenerator_Meat.ImpliedMeatDefs()).Concat(ThingDefGenerator_Techprints.ImpliedTechprintDefs())
				.Concat(ThingDefGenerator_Corpses.ImpliedCorpseDefs()))
			{
				AddImpliedDef(item2);
			}
			DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences(FailMode.Silent);
			foreach (TerrainDef item3 in TerrainDefGenerator_Stone.ImpliedTerrainDefs())
			{
				AddImpliedDef(item3);
			}
			foreach (RecipeDef item4 in RecipeDefGenerator.ImpliedRecipeDefs())
			{
				AddImpliedDef(item4);
			}
			foreach (PawnColumnDef item5 in PawnColumnDefgenerator.ImpliedPawnColumnDefs())
			{
				AddImpliedDef(item5);
			}
			foreach (ThingDef item6 in ThingDefGenerator_Neurotrainer.ImpliedThingDefs())
			{
				AddImpliedDef(item6);
			}
			foreach (GeneDef item7 in GeneDefGenerator.ImpliedGeneDefs())
			{
				AddImpliedDef(item7);
			}
		}

		public static void GenerateImpliedDefs_PostResolve()
		{
			foreach (KeyBindingCategoryDef item in KeyBindingDefGenerator.ImpliedKeyBindingCategoryDefs())
			{
				AddImpliedDef(item);
			}
			foreach (KeyBindingDef item2 in KeyBindingDefGenerator.ImpliedKeyBindingDefs())
			{
				AddImpliedDef(item2);
			}
		}

		public static void AddImpliedDef<T>(T def) where T : Def, new()
		{
			def.generated = true;
			def.modContentPack?.AddDef(def, "ImpliedDefs");
			def.PostLoad();
			DefDatabase<T>.Add(def);
		}
	}
}
