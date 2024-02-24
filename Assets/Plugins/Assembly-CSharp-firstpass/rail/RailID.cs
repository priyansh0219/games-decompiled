namespace rail
{
	public class RailID
	{
		public ulong id_;

		public EnumRailIDDomain GetDomain()
		{
			if ((int)(id_ >> 56) == 1)
			{
				return EnumRailIDDomain.kRailIDDomainPublic;
			}
			return EnumRailIDDomain.kRailIDDomainInvalid;
		}
	}
}
