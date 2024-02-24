using Oculus.Platform.Models;

namespace Oculus.Platform
{
	public static class Users
	{
		public static Request<User> Get(ulong userID)
		{
			if (Core.IsInitialized())
			{
				return new Request<User>(CAPI.ovr_User_Get(userID));
			}
			return null;
		}

		public static Request<User> GetLoggedInUser()
		{
			if (Core.IsInitialized())
			{
				return new Request<User>(CAPI.ovr_User_GetLoggedInUser());
			}
			return null;
		}

		public static Request<UserList> GetLoggedInUserFriends()
		{
			if (Core.IsInitialized())
			{
				return new Request<UserList>(CAPI.ovr_User_GetLoggedInUserFriends());
			}
			return null;
		}

		public static Request<UserList> GetUserFriends(ulong userID)
		{
			if (Core.IsInitialized())
			{
				return new Request<UserList>(CAPI.ovr_User_GetFriends(userID));
			}
			return null;
		}

		public static Request<string> GetAccessToken()
		{
			if (Core.IsInitialized())
			{
				return new Request<string>(CAPI.ovr_User_GetAccessToken());
			}
			return null;
		}

		public static Request<UserProof> GetUserProof()
		{
			if (Core.IsInitialized())
			{
				return new Request<UserProof>(CAPI.ovr_User_GetUserProof());
			}
			return null;
		}
	}
}
