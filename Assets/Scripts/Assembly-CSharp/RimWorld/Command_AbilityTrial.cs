using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Command_AbilityTrial : Command_AbilitySpeech
	{
		public Command_AbilityTrial(Ability ability)
			: base(ability)
		{
			defaultLabel = "Accuse".Translate();
			defaultIconColor = Color.white;
		}
	}
}
