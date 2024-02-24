using System.Collections.Generic;
using Oculus.Platform.Models;

namespace Oculus.Platform
{
	public static class Rooms
	{
		public static Request<Room> CreateAndJoinPrivate(JoinPolicy joinPolicy, uint maxUsers, bool subscribeToNotifications = false)
		{
			if (Core.IsInitialized())
			{
				return new Request<Room>(CAPI.ovr_Room_CreateAndJoinPrivate((uint)joinPolicy, maxUsers, subscribeToNotifications));
			}
			return null;
		}

		public static Request<Room> Get(ulong roomID)
		{
			if (Core.IsInitialized())
			{
				return new Request<Room>(CAPI.ovr_Room_Get(roomID));
			}
			return null;
		}

		public static Request<Room> KickUser(ulong roomID, ulong userID, int kickDurationSeconds)
		{
			if (Core.IsInitialized())
			{
				return new Request<Room>(CAPI.ovr_Room_KickUser(roomID, userID, kickDurationSeconds));
			}
			return null;
		}

		public static Request<Room> Leave(ulong roomID)
		{
			if (Core.IsInitialized())
			{
				return new Request<Room>(CAPI.ovr_Room_Leave(roomID));
			}
			return null;
		}

		public static Request<Room> Join(ulong roomID, bool subscribeToNotifications = false)
		{
			if (Core.IsInitialized())
			{
				return new Request<Room>(CAPI.ovr_Room_Join(roomID, subscribeToNotifications));
			}
			return null;
		}

		public static Request<Room> GetCurrent()
		{
			if (Core.IsInitialized())
			{
				return new Request<Room>(CAPI.ovr_Room_GetCurrent());
			}
			return null;
		}

		public static Request<Room> GetCurrentForUser(ulong userID)
		{
			if (Core.IsInitialized())
			{
				return new Request<Room>(CAPI.ovr_Room_GetCurrentForUser(userID));
			}
			return null;
		}

		public static Request<UserList> GetInvitableUsers()
		{
			if (Core.IsInitialized())
			{
				return new Request<UserList>(CAPI.ovr_Room_GetInvitableUsers());
			}
			return null;
		}

		public static Request<Room> SetDescription(ulong roomID, string description)
		{
			if (Core.IsInitialized())
			{
				return new Request<Room>(CAPI.ovr_Room_SetDescription(roomID, description));
			}
			return null;
		}

		public static Request<Room> UpdateDataStore(ulong roomID, Dictionary<string, string> data)
		{
			if (Core.IsInitialized())
			{
				CAPI.ovrKeyValuePair[] array = new CAPI.ovrKeyValuePair[data.Count];
				int num = 0;
				foreach (KeyValuePair<string, string> datum in data)
				{
					array[num++] = new CAPI.ovrKeyValuePair(datum.Key, datum.Value);
				}
				return new Request<Room>(CAPI.ovr_Room_UpdateDataStore(roomID, array, (uint)array.Length));
			}
			return null;
		}

		public static Request<Room> InviteUser(ulong roomID, string inviteToken)
		{
			if (Core.IsInitialized())
			{
				return new Request<Room>(CAPI.ovr_Room_InviteUser(roomID, inviteToken));
			}
			return null;
		}

		public static Request<Room> UpdateOwner(ulong roomID, ulong userID)
		{
			if (Core.IsInitialized())
			{
				return new Request<Room>(CAPI.ovr_Room_UpdateOwner(roomID, userID));
			}
			return null;
		}

		public static Request<RoomList> GetModeratedRooms()
		{
			if (Core.IsInitialized())
			{
				return new Request<RoomList>(CAPI.ovr_Room_GetModeratedRooms());
			}
			return null;
		}

		public static void SetUpdateNotificationCallback(Message<Room>.Callback callback)
		{
			Callback.SetNotificationCallback(Message.MessageType.Notification_Room_RoomUpdate, callback);
		}
	}
}
