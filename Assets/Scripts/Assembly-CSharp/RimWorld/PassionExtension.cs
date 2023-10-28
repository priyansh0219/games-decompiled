namespace RimWorld
{
	public static class PassionExtension
	{
		public static Passion IncrementPassion(this Passion passion)
		{
			switch (passion)
			{
			case Passion.None:
				return Passion.Minor;
			case Passion.Minor:
				return Passion.Major;
			default:
				return passion;
			}
		}
	}
}
