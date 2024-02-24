using System;
using Newtonsoft.Json;
using Oculus.Platform.Models;
using UnityEngine;

namespace Oculus.Platform
{
	public abstract class Message<T> : Message
	{
		public new delegate void Callback(Message<T> message);

		private T data;

		public T Data => data;

		public Message(IntPtr c_message)
			: base(c_message)
		{
			if (!base.IsError)
			{
				data = GetDataFromMessage(c_message);
			}
		}

		protected abstract T GetDataFromMessage(IntPtr c_message);
	}
	public class Message
	{
		public delegate void Callback(Message message);

		public enum MessageType : uint
		{
			Unknown = 0u,
			Achievements_AddCount = 65495601u,
			Achievements_AddFields = 346693929u,
			Achievements_GetAllDefinitions = 64177549u,
			Achievements_GetAllProgress = 1335877149u,
			Achievements_GetDefinitionsByName = 1653670332u,
			Achievements_GetProgressByName = 354837425u,
			Achievements_Unlock = 1497156573u,
			Entitlement_GetIsViewerEntitled = 409688241u,
			IAP_ConsumePurchase = 532378329u,
			IAP_GetProductsBySKU = 2124073717u,
			IAP_GetViewerPurchases = 974095385u,
			IAP_LaunchCheckoutFlow = 1067126029u,
			Leaderboard_GetEntries = 1572030284u,
			Leaderboard_GetEntriesAfterRank = 406293487u,
			Leaderboard_GetNextEntries = 1310751961u,
			Leaderboard_GetPreviousEntries = 1224858304u,
			Leaderboard_WriteEntry = 293587198u,
			Matchmaking_Browse = 509948616u,
			Matchmaking_Cancel = 543705519u,
			Matchmaking_CreateAndEnqueueRoom = 1615617480u,
			Matchmaking_CreateRoom = 54203178u,
			Matchmaking_Enqueue = 1086418033u,
			Matchmaking_EnqueueRoom = 1888108644u,
			Matchmaking_ReportResultInsecure = 439800205u,
			Matchmaking_StartMatch = 1154746693u,
			Room_CreateAndJoinPrivate = 1977017207u,
			Room_Get = 1704628152u,
			Room_GetCurrent = 161916164u,
			Room_GetCurrentForUser = 234887141u,
			Room_GetInvitableUsers = 506615698u,
			Room_GetModeratedRooms = 159645047u,
			Room_InviteUser = 1093266451u,
			Room_Join = 382373641u,
			Room_KickUser = 1233344310u,
			Room_Leave = 1916281973u,
			Room_SetDescription = 809796911u,
			Room_UpdateDataStore = 40779816u,
			Room_UpdateOwner = 850803997u,
			User_Get = 1808768583u,
			User_GetAccessToken = 111696574u,
			User_GetLoggedInUser = 1131361373u,
			User_GetLoggedInUserFriends = 1484532365u,
			User_GetUserProof = 578880643u,
			Notification_Matchmaking_MatchFound = 197393623u,
			Notification_Networking_ConnectionStateChange = 1577243802u,
			Notification_Networking_PeerConnectRequest = 1295114959u,
			Notification_Room_InviteAccepted = 1829794225u,
			Notification_Room_RoomUpdate = 1626094639u
		}

		internal delegate Message ExtraMessageTypesHandler(IntPtr messageHandle, MessageType message_type);

		private MessageType type;

		private ulong requestID;

		private Error error;

		public MessageType Type => type;

		public bool IsError => error != null;

		public ulong RequestID => requestID;

		internal static ExtraMessageTypesHandler HandleExtraMessageTypes { private get; set; }

		public Message(IntPtr c_message)
		{
			type = (MessageType)CAPI.ovr_Message_GetType(c_message);
			bool num = CAPI.ovr_Message_IsError(c_message);
			requestID = CAPI.ovr_Message_GetRequestID(c_message);
			if (num)
			{
				IntPtr message = CAPI.ovr_Message_GetError(c_message);
				error = new Error(CAPI.ovr_Error_GetCode(message), CAPI.ovr_Error_GetMessage(message), CAPI.ovr_Error_GetHttpCode(message));
			}
			else if (Debug.isDebugBuild)
			{
				string text = CAPI.ovr_Message_GetString(c_message);
				if (text != null)
				{
					Debug.Log(text);
				}
				else
				{
					Debug.Log($"null message string {c_message}");
				}
			}
		}

		public virtual Error GetError()
		{
			return error;
		}

		public virtual UserProof GetUserProof()
		{
			return null;
		}

		public virtual Product GetProduct()
		{
			return null;
		}

		public virtual ProductList GetProductList()
		{
			return null;
		}

		public virtual string GetString()
		{
			return null;
		}

		public virtual Purchase GetPurchase()
		{
			return null;
		}

		public virtual PurchaseList GetPurchaseList()
		{
			return null;
		}

		public virtual User GetUser()
		{
			return null;
		}

		public virtual UserList GetUserList()
		{
			return null;
		}

		public virtual Room GetRoom()
		{
			return null;
		}

		public virtual RoomList GetRoomList()
		{
			return null;
		}

		public virtual NetworkingPeer GetNetworkingPeer()
		{
			return null;
		}

		public virtual AchievementDefinitionList GetAchievementDefinitions()
		{
			return null;
		}

		public virtual AchievementProgressList GetAchievementProgressList()
		{
			return null;
		}

		public virtual bool GetLeaderboardDidUpdate()
		{
			return false;
		}

		public virtual LeaderboardEntryList GetLeaderboardEntryList()
		{
			return null;
		}

		public virtual MatchmakingEnqueueResult GetMatchmakingEnqueueResult()
		{
			return null;
		}

		public virtual MatchmakingEnqueueResultAndRoom GetMatchmakingEnqueueResultAndRoom()
		{
			return null;
		}

		public static T Deserialize<T>(IntPtr c_message)
		{
			return Deserialize<T>(CAPI.ovr_Message_GetString(c_message));
		}

		public static T Deserialize<T>(string json)
		{
			return JsonConvert.DeserializeObject<T>(json);
		}

		internal static Message ParseMessageHandle(IntPtr messageHandle)
		{
			if (messageHandle.ToInt64() == 0L)
			{
				return null;
			}
			Message message = null;
			MessageType messageType = (MessageType)CAPI.ovr_Message_GetType(messageHandle);
			switch (messageType)
			{
			case MessageType.User_GetLoggedInUser:
			case MessageType.User_Get:
				message = new MessageWithUser(messageHandle);
				break;
			case MessageType.Room_GetInvitableUsers:
			case MessageType.User_GetLoggedInUserFriends:
				message = new MessageWithUserList(messageHandle);
				break;
			case MessageType.Room_GetCurrent:
			case MessageType.Room_GetCurrentForUser:
				message = new MessageWithCurrentRoom(messageHandle);
				break;
			case MessageType.IAP_GetViewerPurchases:
				message = new MessageWithPurchaseList(messageHandle);
				break;
			case MessageType.IAP_GetProductsBySKU:
				message = new MessageWithProductList(messageHandle);
				break;
			case MessageType.IAP_LaunchCheckoutFlow:
				message = new MessageWithPurchase(messageHandle);
				break;
			case MessageType.IAP_ConsumePurchase:
				message = new Message(messageHandle);
				break;
			case MessageType.Notification_Room_RoomUpdate:
				message = new MessageWithRoom(messageHandle);
				break;
			case MessageType.Room_Get:
				message = new MessageWithRoom(messageHandle);
				break;
			case MessageType.Room_UpdateDataStore:
			case MessageType.Room_Join:
			case MessageType.Room_SetDescription:
			case MessageType.Room_UpdateOwner:
			case MessageType.Room_InviteUser:
			case MessageType.Room_KickUser:
			case MessageType.Room_Leave:
			case MessageType.Room_CreateAndJoinPrivate:
				message = new MessageWithViewerRoom(messageHandle);
				break;
			case MessageType.Room_GetModeratedRooms:
			case MessageType.Matchmaking_Browse:
				message = new MessageWithRoomList(messageHandle);
				break;
			case MessageType.Matchmaking_CreateAndEnqueueRoom:
				message = new MessageWithMatchmakingEnqueueResultAndRoom(messageHandle);
				break;
			case MessageType.Matchmaking_CreateRoom:
				message = new MessageWithViewerRoom(messageHandle);
				break;
			case MessageType.Matchmaking_Enqueue:
			case MessageType.Matchmaking_EnqueueRoom:
				message = new MessageWithMatchmakingEnqueueResult(messageHandle);
				break;
			case MessageType.Matchmaking_Cancel:
				message = new Message(messageHandle);
				break;
			case MessageType.Matchmaking_ReportResultInsecure:
			case MessageType.Matchmaking_StartMatch:
				message = new Message(messageHandle);
				break;
			case MessageType.Notification_Matchmaking_MatchFound:
				message = new MessageWithMatchMadeRoom(messageHandle);
				break;
			case MessageType.Notification_Networking_PeerConnectRequest:
				message = new MessageWithNetworkingPeer(messageHandle);
				break;
			case MessageType.Notification_Networking_ConnectionStateChange:
				message = new MessageWithNetworkingPeer(messageHandle);
				break;
			case MessageType.Achievements_GetAllDefinitions:
			case MessageType.Achievements_GetDefinitionsByName:
				message = new MessageWithAchievementDefinitionList(messageHandle);
				break;
			case MessageType.User_GetUserProof:
				message = new MessageWithUserProof(messageHandle);
				break;
			case MessageType.Achievements_GetProgressByName:
			case MessageType.Achievements_GetAllProgress:
				message = new MessageWithAchievementProgressList(messageHandle);
				break;
			case MessageType.Achievements_AddCount:
			case MessageType.Achievements_AddFields:
			case MessageType.Achievements_Unlock:
				message = new Message(messageHandle);
				break;
			case MessageType.Leaderboard_WriteEntry:
				message = new MessageWithLeaderboardDidUpdate(messageHandle);
				break;
			case MessageType.Leaderboard_GetEntriesAfterRank:
			case MessageType.Leaderboard_GetPreviousEntries:
			case MessageType.Leaderboard_GetNextEntries:
			case MessageType.Leaderboard_GetEntries:
				message = new MessageWithLeaderboardEntries(messageHandle);
				break;
			case MessageType.Entitlement_GetIsViewerEntitled:
				message = new Message(messageHandle);
				break;
			case MessageType.User_GetAccessToken:
			case MessageType.Notification_Room_InviteAccepted:
				message = new MessageWithString(messageHandle);
				break;
			default:
				if (HandleExtraMessageTypes != null)
				{
					message = HandleExtraMessageTypes(messageHandle, messageType);
				}
				if (message == null)
				{
					Debug.LogError($"Unrecognized message type {messageType}\n");
				}
				break;
			}
			return message;
		}

		public static Message PopMessage()
		{
			if (!Core.IsInitialized())
			{
				return null;
			}
			IntPtr intPtr = CAPI.ovr_PopMessage();
			Message result = ParseMessageHandle(intPtr);
			CAPI.ovr_FreeMessage(intPtr);
			return result;
		}
	}
}
