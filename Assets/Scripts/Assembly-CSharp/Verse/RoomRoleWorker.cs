namespace Verse
{
	public abstract class RoomRoleWorker
	{
		public virtual string PostProcessedLabel(string baseLabel)
		{
			return baseLabel;
		}

		public abstract float GetScore(Room room);
	}
}
