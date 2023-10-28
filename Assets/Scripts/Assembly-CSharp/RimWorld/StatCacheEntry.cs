namespace RimWorld
{
	public class StatCacheEntry
	{
		public float statValue;

		public int gameTick;

		public StatCacheEntry(float statValue, int gameTick)
		{
			this.statValue = statValue;
			this.gameTick = gameTick;
		}
	}
}
