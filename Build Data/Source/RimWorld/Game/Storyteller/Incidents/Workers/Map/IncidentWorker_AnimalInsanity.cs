using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;



namespace RimWorld{

	
//Special case of AnimalInsanity
public class IncidentWorker_AnimalInsanitySingle : IncidentWorker
{
	private const int FixedPoints = 30; //one squirrel

	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if( !base.CanFireNowSub(parms) )
			return false;
			
		var map = (Map)parms.target;

		return TryFindRandomAnimal(map, out _);
	}

	protected override bool TryExecuteWorker( IncidentParms parms )
	{
		var map = (Map)parms.target;

		Pawn animal;
		if( !TryFindRandomAnimal(map, out animal) )
			return false;

		IncidentWorker_AnimalInsanityMass.DriveInsane( animal );

		string letter;
        letter = "AnimalInsanitySingle".Translate(animal.Label, animal.Named("ANIMAL")).CapitalizeFirst();

        SendStandardLetter("LetterLabelAnimalInsanitySingle".Translate(animal.Label, animal.Named("ANIMAL")).CapitalizeFirst(),
			letter,
			LetterDefOf.ThreatSmall,
			parms,
			animal);
		
		return true;
	}

	private bool TryFindRandomAnimal(Map map, out Pawn animal)
	{
		int maxPoints = 150;
		if( GenDate.DaysPassedSinceSettle < 7 )
			maxPoints = 40;

		return map.mapPawns.AllPawnsSpawned
			.Where(p => p.RaceProps.Animal
				&& p.kindDef.combatPower <= maxPoints
				&& IncidentWorker_AnimalInsanityMass.AnimalUsable(p))
			.TryRandomElement(out animal);
	}
}






public class IncidentWorker_AnimalInsanityMass : IncidentWorker
{
	public static bool AnimalUsable( Pawn p )
	{
		return p.Spawned
			&& !p.Position.Fogged(p.Map)
			&& (!p.InMentalState || !p.MentalStateDef.IsAggro)
			&& !p.Downed
			&& p.Faction == null;
	}

	public static void DriveInsane( Pawn p )
	{
		p.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter, forceWake: true);
	}

	protected override bool TryExecuteWorker( IncidentParms parms )
	{
		var map = (Map)parms.target;

		if( parms.points <= 0 )
		{
			Log.Error("AnimalInsanity running without points.");
			parms.points = (int)(map.strengthWatcher.StrengthRating * 50);
		}

		float adjustedPoints = parms.points;
		if( adjustedPoints > 250 )
		{
			//Halve the amount of points over 250
			adjustedPoints -= 250;
			adjustedPoints *= 0.5f;
			adjustedPoints += 250;
		}


		//Choose an animal kind
		IEnumerable<PawnKindDef> animalKinds = DefDatabase<PawnKindDef>.AllDefs
												.Where( def => def.RaceProps.Animal
														&& def.combatPower <= adjustedPoints
														&& map.mapPawns.AllPawnsSpawned.Where(p=>p.kindDef == def 
																							&& AnimalUsable(p) ).Count() >= 3 );

		PawnKindDef animalDef;
		if( !animalKinds.TryRandomElement(out animalDef) )
			return false;

		List<Pawn> allUsableAnimals = map.mapPawns.AllPawnsSpawned
												.Where(p=>p.kindDef == animalDef
												&& AnimalUsable(p) )
												.ToList();

		float pointsPerAnimal = animalDef.combatPower;
		float pointsSpent = 0;
		int animalsMaddened = 0;
        Pawn lastAnimal = null;
		allUsableAnimals.Shuffle();
		foreach( Pawn animal in allUsableAnimals )
		{
			if( pointsSpent+pointsPerAnimal > adjustedPoints )
				break;

			DriveInsane(animal);

			pointsSpent += pointsPerAnimal;
			animalsMaddened++;
            lastAnimal = animal;
		}

		//Not enough points/animals for even one animal to be maddened
		if( pointsSpent == 0 )
			return false;

		string letter;
		string letterLabel;
		LetterDef letterDef;
		if( animalsMaddened == 1 )
		{
            letterLabel = "LetterLabelAnimalInsanitySingle".Translate(lastAnimal.LabelShort, lastAnimal.Named("ANIMAL")).CapitalizeFirst();
            letter = "AnimalInsanitySingle".Translate(lastAnimal.LabelShort, lastAnimal.Named("ANIMAL")).CapitalizeFirst();
			letterDef = LetterDefOf.ThreatSmall;
		}
		else
		{
            letterLabel = "LetterLabelAnimalInsanityMultiple".Translate(animalDef.GetLabelPlural()).CapitalizeFirst();
            letter = "AnimalInsanityMultiple".Translate(animalDef.GetLabelPlural()).CapitalizeFirst();
			letterDef = LetterDefOf.ThreatBig;
		}

        SendStandardLetter(letterLabel, letter, letterDef, parms, lastAnimal);

		SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera(map);

		if( map == Find.CurrentMap )
            Find.CameraDriver.shaker.DoShake(1.0f);

		return true;
	}
}

}