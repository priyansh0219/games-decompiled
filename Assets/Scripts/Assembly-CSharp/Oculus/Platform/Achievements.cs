using Oculus.Platform.Models;

namespace Oculus.Platform
{
	public static class Achievements
	{
		public static Request<AchievementDefinitionList> GetAllDefinitions()
		{
			if (Core.IsInitialized())
			{
				return new Request<AchievementDefinitionList>(CAPI.ovr_Achievements_GetAllDefinitions());
			}
			return null;
		}

		public static Request<AchievementDefinitionList> GetDefinitionsByName(string[] names)
		{
			if (Core.IsInitialized())
			{
				return new Request<AchievementDefinitionList>(CAPI.ovr_Achievements_GetDefinitionsByName(names, names.Length));
			}
			return null;
		}

		public static Request<AchievementProgressList> GetAllProgress()
		{
			if (Core.IsInitialized())
			{
				return new Request<AchievementProgressList>(CAPI.ovr_Achievements_GetAllProgress());
			}
			return null;
		}

		public static Request<AchievementProgressList> GetProgressByName(string[] names)
		{
			if (Core.IsInitialized())
			{
				return new Request<AchievementProgressList>(CAPI.ovr_Achievements_GetProgressByName(names, names.Length));
			}
			return null;
		}

		public static Request Unlock(string name)
		{
			if (Core.IsInitialized())
			{
				return new Request(CAPI.ovr_Achievements_Unlock(name));
			}
			return null;
		}

		public static Request AddCount(string name, ulong count)
		{
			if (Core.IsInitialized())
			{
				return new Request(CAPI.ovr_Achievements_AddCount(name, count));
			}
			return null;
		}

		public static Request AddFields(string name, string fields)
		{
			if (Core.IsInitialized())
			{
				return new Request(CAPI.ovr_Achievements_AddFields(name, fields));
			}
			return null;
		}
	}
}
