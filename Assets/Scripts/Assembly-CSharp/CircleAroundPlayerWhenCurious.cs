public class CircleAroundPlayerWhenCurious : CircleAroundPlayer
{
	public override bool GetInterestedInPlayer()
	{
		if (interestedInPlayer)
		{
			return creature.Curiosity.Value > 0.5f;
		}
		return false;
	}
}
