using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Discord
{
	public class LobbyManager
	{
		internal struct FFIEvents
		{
			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void LobbyUpdateHandler(IntPtr ptr, long lobbyId);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void LobbyDeleteHandler(IntPtr ptr, long lobbyId, uint reason);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void MemberConnectHandler(IntPtr ptr, long lobbyId, long userId);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void MemberUpdateHandler(IntPtr ptr, long lobbyId, long userId);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void MemberDisconnectHandler(IntPtr ptr, long lobbyId, long userId);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void LobbyMessageHandler(IntPtr ptr, long lobbyId, long userId, IntPtr dataPtr, int dataLen);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void SpeakingHandler(IntPtr ptr, long lobbyId, long userId, bool speaking);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void NetworkMessageHandler(IntPtr ptr, long lobbyId, long userId, byte channelId, IntPtr dataPtr, int dataLen);

			internal LobbyUpdateHandler OnLobbyUpdate;

			internal LobbyDeleteHandler OnLobbyDelete;

			internal MemberConnectHandler OnMemberConnect;

			internal MemberUpdateHandler OnMemberUpdate;

			internal MemberDisconnectHandler OnMemberDisconnect;

			internal LobbyMessageHandler OnLobbyMessage;

			internal SpeakingHandler OnSpeaking;

			internal NetworkMessageHandler OnNetworkMessage;
		}

		internal struct FFIMethods
		{
			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result GetLobbyCreateTransactionMethod(IntPtr methodsPtr, ref LobbyTransaction transaction);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result GetLobbyUpdateTransactionMethod(IntPtr methodsPtr, long lobbyId, ref LobbyTransaction transaction);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result GetMemberUpdateTransactionMethod(IntPtr methodsPtr, long lobbyId, long userId, ref LobbyMemberTransaction transaction);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void CreateLobbyCallback(IntPtr ptr, Result result, ref Lobby lobby);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void CreateLobbyMethod(IntPtr methodsPtr, IntPtr transaction, IntPtr callbackData, CreateLobbyCallback callback);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void UpdateLobbyCallback(IntPtr ptr, Result result);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void UpdateLobbyMethod(IntPtr methodsPtr, long lobbyId, IntPtr transaction, IntPtr callbackData, UpdateLobbyCallback callback);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void DeleteLobbyCallback(IntPtr ptr, Result result);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void DeleteLobbyMethod(IntPtr methodsPtr, long lobbyId, IntPtr callbackData, DeleteLobbyCallback callback);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void ConnectLobbyCallback(IntPtr ptr, Result result, ref Lobby lobby);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void ConnectLobbyMethod(IntPtr methodsPtr, long lobbyId, [MarshalAs(UnmanagedType.LPStr)] string secret, IntPtr callbackData, ConnectLobbyCallback callback);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void ConnectLobbyWithActivitySecretCallback(IntPtr ptr, Result result, ref Lobby lobby);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void ConnectLobbyWithActivitySecretMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string activitySecret, IntPtr callbackData, ConnectLobbyWithActivitySecretCallback callback);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void DisconnectLobbyCallback(IntPtr ptr, Result result);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void DisconnectLobbyMethod(IntPtr methodsPtr, long lobbyId, IntPtr callbackData, DisconnectLobbyCallback callback);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result GetLobbyMethod(IntPtr methodsPtr, long lobbyId, ref Lobby lobby);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result GetLobbyActivitySecretMethod(IntPtr methodsPtr, long lobbyId, StringBuilder secret);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result GetLobbyMetadataValueMethod(IntPtr methodsPtr, long lobbyId, [MarshalAs(UnmanagedType.LPStr)] string key, StringBuilder value);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result GetLobbyMetadataKeyMethod(IntPtr methodsPtr, long lobbyId, int index, StringBuilder key);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result LobbyMetadataCountMethod(IntPtr methodsPtr, long lobbyId, ref int count);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result MemberCountMethod(IntPtr methodsPtr, long lobbyId, ref int count);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result GetMemberUserIdMethod(IntPtr methodsPtr, long lobbyId, int index, ref long userId);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result GetMemberUserMethod(IntPtr methodsPtr, long lobbyId, long userId, ref User user);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result GetMemberMetadataValueMethod(IntPtr methodsPtr, long lobbyId, long userId, [MarshalAs(UnmanagedType.LPStr)] string key, StringBuilder value);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result GetMemberMetadataKeyMethod(IntPtr methodsPtr, long lobbyId, long userId, int index, StringBuilder key);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result MemberMetadataCountMethod(IntPtr methodsPtr, long lobbyId, long userId, ref int count);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void UpdateMemberCallback(IntPtr ptr, Result result);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void UpdateMemberMethod(IntPtr methodsPtr, long lobbyId, long userId, IntPtr transaction, IntPtr callbackData, UpdateMemberCallback callback);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void SendLobbyMessageCallback(IntPtr ptr, Result result);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void SendLobbyMessageMethod(IntPtr methodsPtr, long lobbyId, byte[] data, int dataLen, IntPtr callbackData, SendLobbyMessageCallback callback);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result GetSearchQueryMethod(IntPtr methodsPtr, ref LobbySearchQuery query);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void SearchCallback(IntPtr ptr, Result result);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void SearchMethod(IntPtr methodsPtr, IntPtr query, IntPtr callbackData, SearchCallback callback);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void LobbyCountMethod(IntPtr methodsPtr, ref int count);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result GetLobbyIdMethod(IntPtr methodsPtr, int index, ref long lobbyId);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void ConnectVoiceCallback(IntPtr ptr, Result result);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void ConnectVoiceMethod(IntPtr methodsPtr, long lobbyId, IntPtr callbackData, ConnectVoiceCallback callback);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void DisconnectVoiceCallback(IntPtr ptr, Result result);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void DisconnectVoiceMethod(IntPtr methodsPtr, long lobbyId, IntPtr callbackData, DisconnectVoiceCallback callback);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result ConnectNetworkMethod(IntPtr methodsPtr, long lobbyId);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result DisconnectNetworkMethod(IntPtr methodsPtr, long lobbyId);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result FlushNetworkMethod(IntPtr methodsPtr);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result OpenNetworkChannelMethod(IntPtr methodsPtr, long lobbyId, byte channelId, bool reliable);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate Result SendNetworkMessageMethod(IntPtr methodsPtr, long lobbyId, long userId, byte channelId, byte[] data, int dataLen);

			internal GetLobbyCreateTransactionMethod GetLobbyCreateTransaction;

			internal GetLobbyUpdateTransactionMethod GetLobbyUpdateTransaction;

			internal GetMemberUpdateTransactionMethod GetMemberUpdateTransaction;

			internal CreateLobbyMethod CreateLobby;

			internal UpdateLobbyMethod UpdateLobby;

			internal DeleteLobbyMethod DeleteLobby;

			internal ConnectLobbyMethod ConnectLobby;

			internal ConnectLobbyWithActivitySecretMethod ConnectLobbyWithActivitySecret;

			internal DisconnectLobbyMethod DisconnectLobby;

			internal GetLobbyMethod GetLobby;

			internal GetLobbyActivitySecretMethod GetLobbyActivitySecret;

			internal GetLobbyMetadataValueMethod GetLobbyMetadataValue;

			internal GetLobbyMetadataKeyMethod GetLobbyMetadataKey;

			internal LobbyMetadataCountMethod LobbyMetadataCount;

			internal MemberCountMethod MemberCount;

			internal GetMemberUserIdMethod GetMemberUserId;

			internal GetMemberUserMethod GetMemberUser;

			internal GetMemberMetadataValueMethod GetMemberMetadataValue;

			internal GetMemberMetadataKeyMethod GetMemberMetadataKey;

			internal MemberMetadataCountMethod MemberMetadataCount;

			internal UpdateMemberMethod UpdateMember;

			internal SendLobbyMessageMethod SendLobbyMessage;

			internal GetSearchQueryMethod GetSearchQuery;

			internal SearchMethod Search;

			internal LobbyCountMethod LobbyCount;

			internal GetLobbyIdMethod GetLobbyId;

			internal ConnectVoiceMethod ConnectVoice;

			internal DisconnectVoiceMethod DisconnectVoice;

			internal ConnectNetworkMethod ConnectNetwork;

			internal DisconnectNetworkMethod DisconnectNetwork;

			internal FlushNetworkMethod FlushNetwork;

			internal OpenNetworkChannelMethod OpenNetworkChannel;

			internal SendNetworkMessageMethod SendNetworkMessage;
		}

		public delegate void CreateLobbyHandler(Result result, ref Lobby lobby);

		public delegate void UpdateLobbyHandler(Result result);

		public delegate void DeleteLobbyHandler(Result result);

		public delegate void ConnectLobbyHandler(Result result, ref Lobby lobby);

		public delegate void ConnectLobbyWithActivitySecretHandler(Result result, ref Lobby lobby);

		public delegate void DisconnectLobbyHandler(Result result);

		public delegate void UpdateMemberHandler(Result result);

		public delegate void SendLobbyMessageHandler(Result result);

		public delegate void SearchHandler(Result result);

		public delegate void ConnectVoiceHandler(Result result);

		public delegate void DisconnectVoiceHandler(Result result);

		public delegate void LobbyUpdateHandler(long lobbyId);

		public delegate void LobbyDeleteHandler(long lobbyId, uint reason);

		public delegate void MemberConnectHandler(long lobbyId, long userId);

		public delegate void MemberUpdateHandler(long lobbyId, long userId);

		public delegate void MemberDisconnectHandler(long lobbyId, long userId);

		public delegate void LobbyMessageHandler(long lobbyId, long userId, byte[] data);

		public delegate void SpeakingHandler(long lobbyId, long userId, bool speaking);

		public delegate void NetworkMessageHandler(long lobbyId, long userId, byte channelId, byte[] data);

		private IntPtr MethodsPtr;

		private FFIMethods Methods => (FFIMethods)Marshal.PtrToStructure(MethodsPtr, typeof(FFIMethods));

		public event LobbyUpdateHandler OnLobbyUpdate;

		public event LobbyDeleteHandler OnLobbyDelete;

		public event MemberConnectHandler OnMemberConnect;

		public event MemberUpdateHandler OnMemberUpdate;

		public event MemberDisconnectHandler OnMemberDisconnect;

		public event LobbyMessageHandler OnLobbyMessage;

		public event SpeakingHandler OnSpeaking;

		public event NetworkMessageHandler OnNetworkMessage;

		internal LobbyManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events)
		{
			if (eventsPtr == IntPtr.Zero)
			{
				throw new ResultException(Result.InternalError);
			}
			InitEvents(eventsPtr, ref events);
			MethodsPtr = ptr;
			if (MethodsPtr == IntPtr.Zero)
			{
				throw new ResultException(Result.InternalError);
			}
		}

		private void InitEvents(IntPtr eventsPtr, ref FFIEvents events)
		{
			events.OnLobbyUpdate = delegate(IntPtr ptr, long lobbyId)
			{
				if (this.OnLobbyUpdate != null)
				{
					this.OnLobbyUpdate(lobbyId);
				}
			};
			events.OnLobbyDelete = delegate(IntPtr ptr, long lobbyId, uint reason)
			{
				if (this.OnLobbyDelete != null)
				{
					this.OnLobbyDelete(lobbyId, reason);
				}
			};
			events.OnMemberConnect = delegate(IntPtr ptr, long lobbyId, long userId)
			{
				if (this.OnMemberConnect != null)
				{
					this.OnMemberConnect(lobbyId, userId);
				}
			};
			events.OnMemberUpdate = delegate(IntPtr ptr, long lobbyId, long userId)
			{
				if (this.OnMemberUpdate != null)
				{
					this.OnMemberUpdate(lobbyId, userId);
				}
			};
			events.OnMemberDisconnect = delegate(IntPtr ptr, long lobbyId, long userId)
			{
				if (this.OnMemberDisconnect != null)
				{
					this.OnMemberDisconnect(lobbyId, userId);
				}
			};
			events.OnLobbyMessage = delegate(IntPtr ptr, long lobbyId, long userId, IntPtr dataPtr, int dataLen)
			{
				if (this.OnLobbyMessage != null)
				{
					byte[] array2 = new byte[dataLen];
					Marshal.Copy(dataPtr, array2, 0, dataLen);
					this.OnLobbyMessage(lobbyId, userId, array2);
				}
			};
			events.OnSpeaking = delegate(IntPtr ptr, long lobbyId, long userId, bool speaking)
			{
				if (this.OnSpeaking != null)
				{
					this.OnSpeaking(lobbyId, userId, speaking);
				}
			};
			events.OnNetworkMessage = delegate(IntPtr ptr, long lobbyId, long userId, byte channelId, IntPtr dataPtr, int dataLen)
			{
				if (this.OnNetworkMessage != null)
				{
					byte[] array = new byte[dataLen];
					Marshal.Copy(dataPtr, array, 0, dataLen);
					this.OnNetworkMessage(lobbyId, userId, channelId, array);
				}
			};
			Marshal.StructureToPtr(events, eventsPtr, fDeleteOld: false);
		}

		public LobbyTransaction GetLobbyCreateTransaction()
		{
			LobbyTransaction transaction = default(LobbyTransaction);
			Result result = Methods.GetLobbyCreateTransaction(MethodsPtr, ref transaction);
			if (result != 0)
			{
				throw new ResultException(result);
			}
			return transaction;
		}

		public LobbyTransaction GetLobbyUpdateTransaction(long lobbyId)
		{
			LobbyTransaction transaction = default(LobbyTransaction);
			Result result = Methods.GetLobbyUpdateTransaction(MethodsPtr, lobbyId, ref transaction);
			if (result != 0)
			{
				throw new ResultException(result);
			}
			return transaction;
		}

		public LobbyMemberTransaction GetMemberUpdateTransaction(long lobbyId, long userId)
		{
			LobbyMemberTransaction transaction = default(LobbyMemberTransaction);
			Result result = Methods.GetMemberUpdateTransaction(MethodsPtr, lobbyId, userId, ref transaction);
			if (result != 0)
			{
				throw new ResultException(result);
			}
			return transaction;
		}

		public void CreateLobby(LobbyTransaction transaction, CreateLobbyHandler callback)
		{
			FFIMethods.CreateLobbyCallback createLobbyCallback = delegate(IntPtr ptr, Result result, ref Lobby lobby)
			{
				Utility.Release(ptr);
				callback(result, ref lobby);
			};
			Methods.CreateLobby(MethodsPtr, transaction.MethodsPtr, Utility.Retain(createLobbyCallback), createLobbyCallback);
			transaction.MethodsPtr = IntPtr.Zero;
		}

		public void UpdateLobby(long lobbyId, LobbyTransaction transaction, UpdateLobbyHandler callback)
		{
			FFIMethods.UpdateLobbyCallback updateLobbyCallback = delegate(IntPtr ptr, Result result)
			{
				Utility.Release(ptr);
				callback(result);
			};
			Methods.UpdateLobby(MethodsPtr, lobbyId, transaction.MethodsPtr, Utility.Retain(updateLobbyCallback), updateLobbyCallback);
			transaction.MethodsPtr = IntPtr.Zero;
		}

		public void DeleteLobby(long lobbyId, DeleteLobbyHandler callback)
		{
			FFIMethods.DeleteLobbyCallback deleteLobbyCallback = delegate(IntPtr ptr, Result result)
			{
				Utility.Release(ptr);
				callback(result);
			};
			Methods.DeleteLobby(MethodsPtr, lobbyId, Utility.Retain(deleteLobbyCallback), deleteLobbyCallback);
		}

		public void ConnectLobby(long lobbyId, string secret, ConnectLobbyHandler callback)
		{
			FFIMethods.ConnectLobbyCallback connectLobbyCallback = delegate(IntPtr ptr, Result result, ref Lobby lobby)
			{
				Utility.Release(ptr);
				callback(result, ref lobby);
			};
			Methods.ConnectLobby(MethodsPtr, lobbyId, secret, Utility.Retain(connectLobbyCallback), connectLobbyCallback);
		}

		public void ConnectLobbyWithActivitySecret(string activitySecret, ConnectLobbyWithActivitySecretHandler callback)
		{
			FFIMethods.ConnectLobbyWithActivitySecretCallback connectLobbyWithActivitySecretCallback = delegate(IntPtr ptr, Result result, ref Lobby lobby)
			{
				Utility.Release(ptr);
				callback(result, ref lobby);
			};
			Methods.ConnectLobbyWithActivitySecret(MethodsPtr, activitySecret, Utility.Retain(connectLobbyWithActivitySecretCallback), connectLobbyWithActivitySecretCallback);
		}

		public void DisconnectLobby(long lobbyId, DisconnectLobbyHandler callback)
		{
			FFIMethods.DisconnectLobbyCallback disconnectLobbyCallback = delegate(IntPtr ptr, Result result)
			{
				Utility.Release(ptr);
				callback(result);
			};
			Methods.DisconnectLobby(MethodsPtr, lobbyId, Utility.Retain(disconnectLobbyCallback), disconnectLobbyCallback);
		}

		public Lobby GetLobby(long lobbyId)
		{
			Lobby lobby = default(Lobby);
			Result result = Methods.GetLobby(MethodsPtr, lobbyId, ref lobby);
			if (result != 0)
			{
				throw new ResultException(result);
			}
			return lobby;
		}

		public string GetLobbyActivitySecret(long lobbyId)
		{
			StringBuilder stringBuilder = new StringBuilder(128);
			Result result = Methods.GetLobbyActivitySecret(MethodsPtr, lobbyId, stringBuilder);
			if (result != 0)
			{
				throw new ResultException(result);
			}
			return stringBuilder.ToString();
		}

		public string GetLobbyMetadataValue(long lobbyId, string key)
		{
			StringBuilder stringBuilder = new StringBuilder(4096);
			Result result = Methods.GetLobbyMetadataValue(MethodsPtr, lobbyId, key, stringBuilder);
			if (result != 0)
			{
				throw new ResultException(result);
			}
			return stringBuilder.ToString();
		}

		public string GetLobbyMetadataKey(long lobbyId, int index)
		{
			StringBuilder stringBuilder = new StringBuilder(256);
			Result result = Methods.GetLobbyMetadataKey(MethodsPtr, lobbyId, index, stringBuilder);
			if (result != 0)
			{
				throw new ResultException(result);
			}
			return stringBuilder.ToString();
		}

		public int LobbyMetadataCount(long lobbyId)
		{
			int count = 0;
			Result result = Methods.LobbyMetadataCount(MethodsPtr, lobbyId, ref count);
			if (result != 0)
			{
				throw new ResultException(result);
			}
			return count;
		}

		public int MemberCount(long lobbyId)
		{
			int count = 0;
			Result result = Methods.MemberCount(MethodsPtr, lobbyId, ref count);
			if (result != 0)
			{
				throw new ResultException(result);
			}
			return count;
		}

		public long GetMemberUserId(long lobbyId, int index)
		{
			long userId = 0L;
			Result result = Methods.GetMemberUserId(MethodsPtr, lobbyId, index, ref userId);
			if (result != 0)
			{
				throw new ResultException(result);
			}
			return userId;
		}

		public User GetMemberUser(long lobbyId, long userId)
		{
			User user = default(User);
			Result result = Methods.GetMemberUser(MethodsPtr, lobbyId, userId, ref user);
			if (result != 0)
			{
				throw new ResultException(result);
			}
			return user;
		}

		public string GetMemberMetadataValue(long lobbyId, long userId, string key)
		{
			StringBuilder stringBuilder = new StringBuilder(4096);
			Result result = Methods.GetMemberMetadataValue(MethodsPtr, lobbyId, userId, key, stringBuilder);
			if (result != 0)
			{
				throw new ResultException(result);
			}
			return stringBuilder.ToString();
		}

		public string GetMemberMetadataKey(long lobbyId, long userId, int index)
		{
			StringBuilder stringBuilder = new StringBuilder(256);
			Result result = Methods.GetMemberMetadataKey(MethodsPtr, lobbyId, userId, index, stringBuilder);
			if (result != 0)
			{
				throw new ResultException(result);
			}
			return stringBuilder.ToString();
		}

		public int MemberMetadataCount(long lobbyId, long userId)
		{
			int count = 0;
			Result result = Methods.MemberMetadataCount(MethodsPtr, lobbyId, userId, ref count);
			if (result != 0)
			{
				throw new ResultException(result);
			}
			return count;
		}

		public void UpdateMember(long lobbyId, long userId, LobbyMemberTransaction transaction, UpdateMemberHandler callback)
		{
			FFIMethods.UpdateMemberCallback updateMemberCallback = delegate(IntPtr ptr, Result result)
			{
				Utility.Release(ptr);
				callback(result);
			};
			Methods.UpdateMember(MethodsPtr, lobbyId, userId, transaction.MethodsPtr, Utility.Retain(updateMemberCallback), updateMemberCallback);
			transaction.MethodsPtr = IntPtr.Zero;
		}

		public void SendLobbyMessage(long lobbyId, byte[] data, SendLobbyMessageHandler callback)
		{
			FFIMethods.SendLobbyMessageCallback sendLobbyMessageCallback = delegate(IntPtr ptr, Result result)
			{
				Utility.Release(ptr);
				callback(result);
			};
			Methods.SendLobbyMessage(MethodsPtr, lobbyId, data, data.Length, Utility.Retain(sendLobbyMessageCallback), sendLobbyMessageCallback);
		}

		public LobbySearchQuery GetSearchQuery()
		{
			LobbySearchQuery query = default(LobbySearchQuery);
			Result result = Methods.GetSearchQuery(MethodsPtr, ref query);
			if (result != 0)
			{
				throw new ResultException(result);
			}
			return query;
		}

		public void Search(LobbySearchQuery query, SearchHandler callback)
		{
			FFIMethods.SearchCallback searchCallback = delegate(IntPtr ptr, Result result)
			{
				Utility.Release(ptr);
				callback(result);
			};
			Methods.Search(MethodsPtr, query.MethodsPtr, Utility.Retain(searchCallback), searchCallback);
			query.MethodsPtr = IntPtr.Zero;
		}

		public int LobbyCount()
		{
			int count = 0;
			Methods.LobbyCount(MethodsPtr, ref count);
			return count;
		}

		public long GetLobbyId(int index)
		{
			long lobbyId = 0L;
			Result result = Methods.GetLobbyId(MethodsPtr, index, ref lobbyId);
			if (result != 0)
			{
				throw new ResultException(result);
			}
			return lobbyId;
		}

		public void ConnectVoice(long lobbyId, ConnectVoiceHandler callback)
		{
			FFIMethods.ConnectVoiceCallback connectVoiceCallback = delegate(IntPtr ptr, Result result)
			{
				Utility.Release(ptr);
				callback(result);
			};
			Methods.ConnectVoice(MethodsPtr, lobbyId, Utility.Retain(connectVoiceCallback), connectVoiceCallback);
		}

		public void DisconnectVoice(long lobbyId, DisconnectVoiceHandler callback)
		{
			FFIMethods.DisconnectVoiceCallback disconnectVoiceCallback = delegate(IntPtr ptr, Result result)
			{
				Utility.Release(ptr);
				callback(result);
			};
			Methods.DisconnectVoice(MethodsPtr, lobbyId, Utility.Retain(disconnectVoiceCallback), disconnectVoiceCallback);
		}

		public void ConnectNetwork(long lobbyId)
		{
			Result result = Methods.ConnectNetwork(MethodsPtr, lobbyId);
			if (result != 0)
			{
				throw new ResultException(result);
			}
		}

		public void DisconnectNetwork(long lobbyId)
		{
			Result result = Methods.DisconnectNetwork(MethodsPtr, lobbyId);
			if (result != 0)
			{
				throw new ResultException(result);
			}
		}

		public void FlushNetwork()
		{
			Result result = Methods.FlushNetwork(MethodsPtr);
			if (result != 0)
			{
				throw new ResultException(result);
			}
		}

		public void OpenNetworkChannel(long lobbyId, byte channelId, bool reliable)
		{
			Result result = Methods.OpenNetworkChannel(MethodsPtr, lobbyId, channelId, reliable);
			if (result != 0)
			{
				throw new ResultException(result);
			}
		}

		public void SendNetworkMessage(long lobbyId, long userId, byte channelId, byte[] data)
		{
			Result result = Methods.SendNetworkMessage(MethodsPtr, lobbyId, userId, channelId, data, data.Length);
			if (result != 0)
			{
				throw new ResultException(result);
			}
		}

		public IEnumerable<User> GetMemberUsers(long lobbyID)
		{
			int num = MemberCount(lobbyID);
			List<User> list = new List<User>();
			for (int i = 0; i < num; i++)
			{
				list.Add(GetMemberUser(lobbyID, GetMemberUserId(lobbyID, i)));
			}
			return list;
		}

		public void SendLobbyMessage(long lobbyID, string data, SendLobbyMessageHandler handler)
		{
			SendLobbyMessage(lobbyID, Encoding.UTF8.GetBytes(data), handler);
		}
	}
}
