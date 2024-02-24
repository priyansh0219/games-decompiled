using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Oculus.Platform
{
	public class CAPI
	{
		public struct ovrKeyValuePair
		{
			public string key_;

			private KeyValuePairType valueType_;

			public string stringValue_;

			public int intValue_;

			public double doubleValue_;

			public ovrKeyValuePair(string key, string value)
			{
				key_ = key;
				valueType_ = KeyValuePairType.String;
				stringValue_ = value;
				intValue_ = 0;
				doubleValue_ = 0.0;
			}

			public ovrKeyValuePair(string key, int value)
			{
				key_ = key;
				valueType_ = KeyValuePairType.Int;
				intValue_ = value;
				stringValue_ = null;
				doubleValue_ = 0.0;
			}

			public ovrKeyValuePair(string key, double value)
			{
				key_ = key;
				valueType_ = KeyValuePairType.Double;
				doubleValue_ = value;
				stringValue_ = null;
				intValue_ = 0;
			}
		}

		public struct ovrMatchmakingCriterion
		{
			public string key_;

			public MatchmakingCriterionImportance importance_;

			public IntPtr parameterArray;

			public uint parameterArrayCount;

			public ovrMatchmakingCriterion(string key, MatchmakingCriterionImportance importance)
			{
				key_ = key;
				importance_ = importance;
				parameterArray = IntPtr.Zero;
				parameterArrayCount = 0u;
			}
		}

		public struct ovrMatchmakingCustomQueryData
		{
			public IntPtr dataArray;

			public uint dataArrayCount;

			public IntPtr criterionArray;

			public uint criterionArrayCount;
		}

		public const string DLL_NAME = "LibOVRPlatform64_1";

		public static string ovr_Message_GetString(IntPtr message)
		{
			return Marshal.PtrToStringAnsi(ovr_Message_GetString_Unsafe(message));
		}

		public static string ovr_Error_GetMessage(IntPtr message)
		{
			return Marshal.PtrToStringAnsi(ovr_Error_GetMessage_Unsafe(message));
		}

		public static IntPtr ArrayOfStructsToIntPtr(Array ar)
		{
			int num = 0;
			for (int i = 0; i < ar.Length; i++)
			{
				num += Marshal.SizeOf(ar.GetValue(i));
			}
			IntPtr intPtr = Marshal.AllocHGlobal(num);
			IntPtr intPtr2 = intPtr;
			for (int j = 0; j < ar.Length; j++)
			{
				Marshal.StructureToPtr(ar.GetValue(j), intPtr2, fDeleteOld: false);
				intPtr2 = (IntPtr)((long)intPtr2 + Marshal.SizeOf(ar.GetValue(j)));
			}
			return intPtr;
		}

		public static ovrKeyValuePair[] DictionaryToOVRKeyValuePairs(Dictionary<string, object> dict)
		{
			if (dict == null || dict.Count == 0)
			{
				return null;
			}
			ovrKeyValuePair[] array = new ovrKeyValuePair[dict.Count];
			int num = 0;
			foreach (KeyValuePair<string, object> item in dict)
			{
				if (item.Value.GetType() == typeof(int))
				{
					array[num] = new ovrKeyValuePair(item.Key, (int)item.Value);
				}
				else if (item.Value.GetType() == typeof(string))
				{
					array[num] = new ovrKeyValuePair(item.Key, (string)item.Value);
				}
				else
				{
					if (!(item.Value.GetType() == typeof(double)))
					{
						throw new Exception("Only int, double or string are allowed types in CustomQuery.data");
					}
					array[num] = new ovrKeyValuePair(item.Key, (double)item.Value);
				}
				num++;
			}
			return array;
		}

		[DllImport("LibOVRPlatform64_1")]
		public static extern bool ovr_UnityInitWrapper();

		[DllImport("LibOVRPlatform64_1")]
		public static extern bool ovr_UnityInitWrapperStandalone(string accessToken, IntPtr loggingCB);

		[DllImport("LibOVRPlatform64_1")]
		public static extern bool ovr_UnityInitWrapperWindows(string appId, IntPtr loggingCB);

		[DllImport("LibOVRPlatform64_1")]
		public static extern bool ovr_SetDeveloperAccessToken(string accessToken);

		[DllImport("LibOVRPlatform64_1")]
		public static extern IntPtr ovr_PopMessage();

		[DllImport("LibOVRPlatform64_1")]
		public static extern void ovr_FreeMessage(IntPtr message);

		[DllImport("LibOVRPlatform64_1")]
		public static extern uint ovr_Message_GetType(IntPtr message);

		[DllImport("LibOVRPlatform64_1")]
		public static extern bool ovr_Message_IsError(IntPtr message);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_User_GetAccessToken();

		[DllImport("LibOVRPlatform64_1")]
		public static extern IntPtr ovr_Message_GetError(IntPtr message);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_Message_GetRequestID(IntPtr message);

		[DllImport("LibOVRPlatform64_1", CharSet = CharSet.Unicode, EntryPoint = "ovr_Message_GetString")]
		private static extern IntPtr ovr_Message_GetString_Unsafe(IntPtr message);

		[DllImport("LibOVRPlatform64_1")]
		public static extern IntPtr ovr_Message_GetNetworkingPeer(IntPtr message);

		[DllImport("LibOVRPlatform64_1")]
		public static extern uint ovr_NetworkingPeer_GetState(IntPtr networkingPeer);

		[DllImport("LibOVRPlatform64_1")]
		public static extern uint ovr_NetworkingPeer_GetSendPolicy(IntPtr networkingPeer);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_NetworkingPeer_GetID(IntPtr networkingPeer);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_Packet_GetSenderID(IntPtr packet);

		[DllImport("LibOVRPlatform64_1")]
		public static extern uint ovr_Packet_GetSize(IntPtr packet);

		[DllImport("LibOVRPlatform64_1")]
		public static extern IntPtr ovr_Packet_GetBytes(IntPtr packet);

		[DllImport("LibOVRPlatform64_1")]
		public static extern uint ovr_Packet_GetSendPolicy(IntPtr packet);

		[DllImport("LibOVRPlatform64_1")]
		public static extern int ovr_Error_GetCode(IntPtr error);

		[DllImport("LibOVRPlatform64_1")]
		public static extern int ovr_Error_GetHttpCode(IntPtr error);

		[DllImport("LibOVRPlatform64_1", CharSet = CharSet.Unicode, EntryPoint = "ovr_Error_GetMessage")]
		private static extern IntPtr ovr_Error_GetMessage_Unsafe(IntPtr error);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_Entitlement_GetIsViewerEntitled();

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_HTTP_GetWithMessageType(string url, int messageType);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_User_Get(ulong userID);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_User_GetLoggedInUser();

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_User_GetFriends(ulong userID);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_User_GetLoggedInUserFriends();

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_User_GetUserProof();

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_Room_Get(ulong id);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_Room_Leave(ulong roomID);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_Room_KickUser(ulong roomID, ulong userID, int kickDurationSeconds);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_Room_Join(ulong roomID, bool subscribeToUpdates);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_Room_CreateAndJoinPrivate(uint joinPolicy, uint max_users, bool subscribeToUpdates);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_Matchmaking_CreateAndEnqueueRoom(string pool, uint maxUsers, bool subscribeToUpdates, IntPtr customQuery);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_Matchmaking_CreateRoom(string pool, uint maxUsers, bool subscribeToUpdates);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_Matchmaking_Browse(string pool, IntPtr customQuery);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_Matchmaking_Enqueue(string pool, IntPtr customQuery);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_Matchmaking_EnqueueRoom(ulong roomID, IntPtr customQuery);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_Matchmaking_ReportResultInsecure(ulong roomID, ovrKeyValuePair[] data, uint numItems);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_Matchmaking_StartMatch(ulong roomID);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_Matchmaking_Cancel(string pool, string traceID);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_Room_SetDescription(ulong roomID, string description);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_Room_UpdateDataStore(ulong roomID, ovrKeyValuePair[] data, uint numItems);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_Room_GetCurrent();

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_Room_GetCurrentForUser(ulong userID);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_Room_GetInvitableUsers();

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_Room_InviteUser(ulong roomID, string inviteToken);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_Room_UpdateOwner(ulong roomID, ulong userID);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_Room_GetModeratedRooms();

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_IAP_GetViewerPurchases();

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_IAP_GetProductsBySKU(string[] skus, int count);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_IAP_ConsumePurchase(string sku);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_IAP_LaunchCheckoutFlow(string offerID);

		[DllImport("LibOVRPlatform64_1")]
		public static extern bool ovr_Net_SendPacket(ulong userID, uint length, byte[] bytes, SendPolicy policy);

		[DllImport("LibOVRPlatform64_1")]
		public static extern void ovr_Net_Connect(ulong userID);

		[DllImport("LibOVRPlatform64_1")]
		public static extern void ovr_Net_Accept(ulong peerID);

		[DllImport("LibOVRPlatform64_1")]
		public static extern void ovr_Net_Close(ulong peerID);

		[DllImport("LibOVRPlatform64_1")]
		public static extern IntPtr ovr_Net_ReadPacket();

		[DllImport("LibOVRPlatform64_1", CallingConvention = CallingConvention.Cdecl)]
		public static extern void ovr_Packet_Free(IntPtr packet);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_Achievements_GetAllDefinitions();

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_Achievements_GetDefinitionsByName(string[] names, int count);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_Achievements_GetAllProgress();

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_Achievements_GetProgressByName(string[] names, int count);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_Achievements_Unlock(string name);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_Achievements_AddCount(string name, ulong count);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_Achievements_AddFields(string name, string fields);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_Leaderboard_WriteEntry(string leaderboardName, long score, byte[] extraData, uint extraDataLength, bool forceUpdate);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_Leaderboard_GetEntries(string leaderboardName, int limit, LeaderboardFilterType filter, LeaderboardStartAt startAt);

		[DllImport("LibOVRPlatform64_1")]
		public static extern ulong ovr_Leaderboard_GetEntriesAfterRank(string leaderboardName, int limit, ulong afterRank);
	}
}
