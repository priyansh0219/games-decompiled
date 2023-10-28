namespace Verse
{
	public class SubEffecter_SprayerTriggeredChance : SubEffecter_Sprayer
	{
		public SubEffecter_SprayerTriggeredChance(SubEffecterDef def, Effecter parent)
			: base(def, parent)
		{
		}

		public override void SubTrigger(TargetInfo A, TargetInfo B, int overrideSpawnTick = -1)
		{
			float chancePerTick = def.chancePerTick;
			if (Rand.Value < chancePerTick)
			{
				MakeMote(A, B, overrideSpawnTick);
			}
		}
	}
}
