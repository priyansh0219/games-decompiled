using System;

namespace Story
{
	[Serializable]
	public class BiomeGoal : StoryGoal
	{
		public string biome;

		public float minStayDuration;

		public bool Trigger(string playerBiome, float stayDuration)
		{
			if (!string.Equals(playerBiome, biome, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			if (stayDuration < minStayDuration)
			{
				return false;
			}
			Trigger();
			return true;
		}
	}
}
