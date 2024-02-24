using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Oculus.Platform.Models;

namespace Oculus.Platform
{
	public static class Matchmaking
	{
		public class CustomQuery
		{
			public struct Criterion
			{
				public string key;

				public MatchmakingCriterionImportance importance;

				public Dictionary<string, object> parameters;

				public Criterion(string key_, MatchmakingCriterionImportance importance_)
				{
					key = key_;
					importance = importance_;
					parameters = null;
				}
			}

			public Dictionary<string, object> data;

			public Criterion[] criteria;

			public IntPtr ToUnmanaged()
			{
				CAPI.ovrMatchmakingCustomQueryData structure = default(CAPI.ovrMatchmakingCustomQueryData);
				if (criteria != null && criteria.Length != 0)
				{
					structure.criterionArrayCount = (uint)criteria.Length;
					CAPI.ovrMatchmakingCriterion[] array = new CAPI.ovrMatchmakingCriterion[criteria.Length];
					for (int i = 0; i < criteria.Length; i++)
					{
						array[i].importance_ = criteria[i].importance;
						array[i].key_ = criteria[i].key;
						if (criteria[i].parameters != null && criteria[i].parameters.Count > 0)
						{
							array[i].parameterArrayCount = (uint)criteria[i].parameters.Count;
							array[i].parameterArray = CAPI.ArrayOfStructsToIntPtr(CAPI.DictionaryToOVRKeyValuePairs(criteria[i].parameters));
						}
						else
						{
							array[i].parameterArrayCount = 0u;
							array[i].parameterArray = IntPtr.Zero;
						}
					}
					structure.criterionArray = CAPI.ArrayOfStructsToIntPtr(array);
				}
				else
				{
					structure.criterionArrayCount = 0u;
					structure.criterionArray = IntPtr.Zero;
				}
				if (data != null && data.Count > 0)
				{
					structure.dataArrayCount = (uint)data.Count;
					structure.dataArray = CAPI.ArrayOfStructsToIntPtr(CAPI.DictionaryToOVRKeyValuePairs(data));
				}
				else
				{
					structure.dataArrayCount = 0u;
					structure.dataArray = IntPtr.Zero;
				}
				IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(structure));
				Marshal.StructureToPtr(structure, intPtr, fDeleteOld: true);
				return intPtr;
			}
		}

		public static Request<Room> CreateAndEnqueueRoom(string pool, uint maxUsers, bool subscribeToNotifications = false, CustomQuery customQuery = null)
		{
			if (Core.IsInitialized())
			{
				return new Request<Room>(CAPI.ovr_Matchmaking_CreateAndEnqueueRoom(pool, maxUsers, subscribeToNotifications, customQuery?.ToUnmanaged() ?? IntPtr.Zero));
			}
			return null;
		}

		public static Request<Room> CreateRoom(string pool, uint maxUsers, bool subscribeToNotifications = false)
		{
			if (Core.IsInitialized())
			{
				return new Request<Room>(CAPI.ovr_Matchmaking_CreateRoom(pool, maxUsers, subscribeToNotifications));
			}
			return null;
		}

		public static Request<RoomList> Browse(string pool, CustomQuery customQuery = null)
		{
			if (Core.IsInitialized())
			{
				return new Request<RoomList>(CAPI.ovr_Matchmaking_Browse(pool, customQuery?.ToUnmanaged() ?? IntPtr.Zero));
			}
			return null;
		}

		public static Request ReportResultsInsecure(ulong roomID, Dictionary<string, int> data)
		{
			if (Core.IsInitialized())
			{
				CAPI.ovrKeyValuePair[] array = new CAPI.ovrKeyValuePair[data.Count];
				int num = 0;
				foreach (KeyValuePair<string, int> datum in data)
				{
					array[num++] = new CAPI.ovrKeyValuePair(datum.Key, datum.Value);
				}
				return new Request(CAPI.ovr_Matchmaking_ReportResultInsecure(roomID, array, (uint)array.Length));
			}
			return null;
		}

		public static Request Enqueue(string pool, CustomQuery customQuery = null)
		{
			if (Core.IsInitialized())
			{
				return new Request(CAPI.ovr_Matchmaking_Enqueue(pool, customQuery?.ToUnmanaged() ?? IntPtr.Zero));
			}
			return null;
		}

		public static Request EnqueueRoom(ulong roomID, CustomQuery customQuery = null)
		{
			if (Core.IsInitialized())
			{
				return new Request(CAPI.ovr_Matchmaking_EnqueueRoom(roomID, customQuery?.ToUnmanaged() ?? IntPtr.Zero));
			}
			return null;
		}

		public static Request StartMatch(ulong roomID)
		{
			if (Core.IsInitialized())
			{
				return new Request(CAPI.ovr_Matchmaking_StartMatch(roomID));
			}
			return null;
		}

		public static Request Cancel(string pool, string traceID)
		{
			if (Core.IsInitialized())
			{
				return new Request(CAPI.ovr_Matchmaking_Cancel(pool, traceID));
			}
			return null;
		}

		public static void SetMatchFoundNotificationCallback(Message<Room>.Callback callback)
		{
			Callback.SetNotificationCallback(Message.MessageType.Notification_Matchmaking_MatchFound, callback);
		}
	}
}
