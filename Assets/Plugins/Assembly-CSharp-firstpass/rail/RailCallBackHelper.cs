using System;
using System.Collections.Generic;

namespace rail
{
	public class RailCallBackHelper
	{
		private static readonly object locker_ = new object();

		private static Dictionary<RAILEventID, RailEventCallBackHandler> eventHandlers_ = new Dictionary<RAILEventID, RailEventCallBackHandler>();

		private static RailEventCallBackFunction delegate_ = OnRailCallBack;

		~RailCallBackHelper()
		{
			UnregisterAllCallback();
		}

		public void RegisterCallback(RAILEventID event_id, RailEventCallBackHandler handler)
		{
			lock (locker_)
			{
				if (eventHandlers_.ContainsKey(event_id))
				{
					Dictionary<RAILEventID, RailEventCallBackHandler> dictionary = eventHandlers_;
					dictionary[event_id] = (RailEventCallBackHandler)Delegate.Combine(dictionary[event_id], handler);
				}
				else
				{
					eventHandlers_.Add(event_id, handler);
					rail_api.CSharpRailRegisterEvent(event_id, delegate_);
				}
			}
		}

		public void UnregisterCallback(RAILEventID event_id, RailEventCallBackHandler handler)
		{
			lock (locker_)
			{
				if (eventHandlers_.ContainsKey(event_id))
				{
					Dictionary<RAILEventID, RailEventCallBackHandler> dictionary = eventHandlers_;
					dictionary[event_id] = (RailEventCallBackHandler)Delegate.Remove(dictionary[event_id], handler);
					if (eventHandlers_[event_id] == null)
					{
						rail_api.CSharpRailUnRegisterEvent(event_id, delegate_);
						eventHandlers_.Remove(event_id);
					}
				}
			}
		}

		public void UnregisterCallback(RAILEventID event_id)
		{
			lock (locker_)
			{
				rail_api.CSharpRailUnRegisterEvent(event_id, delegate_);
			}
		}

		public void UnregisterAllCallback()
		{
			lock (locker_)
			{
				rail_api.CSharpRailUnRegisterAllEvent();
			}
		}

		public static void OnRailCallBack(RAILEventID event_id, IntPtr data)
		{
			RailEventCallBackHandler railEventCallBackHandler = eventHandlers_[event_id];
			if (railEventCallBackHandler != null)
			{
				switch (event_id)
				{
				case RAILEventID.kRailPlatformNotifyEventJoinGameByGameServer:
				{
					RailPlatformNotifyEventJoinGameByGameServer railPlatformNotifyEventJoinGameByGameServer = new RailPlatformNotifyEventJoinGameByGameServer();
					RailConverter.Cpp2Csharp(data, railPlatformNotifyEventJoinGameByGameServer);
					railEventCallBackHandler(event_id, railPlatformNotifyEventJoinGameByGameServer);
					break;
				}
				case RAILEventID.kRailEventQueryPlayerBannedStatus:
				{
					QueryPlayerBannedStatus queryPlayerBannedStatus = new QueryPlayerBannedStatus();
					RailConverter.Cpp2Csharp(data, queryPlayerBannedStatus);
					railEventCallBackHandler(event_id, queryPlayerBannedStatus);
					break;
				}
				case RAILEventID.kRailEventNetworkCreateSessionFailed:
				{
					CreateSessionFailed createSessionFailed = new CreateSessionFailed();
					RailConverter.Cpp2Csharp(data, createSessionFailed);
					railEventCallBackHandler(event_id, createSessionFailed);
					break;
				}
				case RAILEventID.kRailEventGameServerRemoveFavoriteGameServer:
				{
					AsyncRemoveFavoriteGameServerResult asyncRemoveFavoriteGameServerResult = new AsyncRemoveFavoriteGameServerResult();
					RailConverter.Cpp2Csharp(data, asyncRemoveFavoriteGameServerResult);
					railEventCallBackHandler(event_id, asyncRemoveFavoriteGameServerResult);
					break;
				}
				case RAILEventID.kRailEventVoiceChannelAddUsersResult:
				{
					VoiceChannelAddUsersResult voiceChannelAddUsersResult = new VoiceChannelAddUsersResult();
					RailConverter.Cpp2Csharp(data, voiceChannelAddUsersResult);
					railEventCallBackHandler(event_id, voiceChannelAddUsersResult);
					break;
				}
				case RAILEventID.kRailEventUsersInviteJoinGameResult:
				{
					RailUsersInviteJoinGameResult railUsersInviteJoinGameResult = new RailUsersInviteJoinGameResult();
					RailConverter.Cpp2Csharp(data, railUsersInviteJoinGameResult);
					railEventCallBackHandler(event_id, railUsersInviteJoinGameResult);
					break;
				}
				case RAILEventID.kRailEventNetChannelChannelException:
				{
					ChannelException ex = new ChannelException();
					RailConverter.Cpp2Csharp(data, ex);
					railEventCallBackHandler(event_id, ex);
					break;
				}
				case RAILEventID.kRailEventUsersRespondInvation:
				{
					RailUsersRespondInvation railUsersRespondInvation = new RailUsersRespondInvation();
					RailConverter.Cpp2Csharp(data, railUsersRespondInvation);
					railEventCallBackHandler(event_id, railUsersRespondInvation);
					break;
				}
				case RAILEventID.kRailEventGameServerGetSessionTicket:
				{
					AsyncAcquireGameServerSessionTicketResponse asyncAcquireGameServerSessionTicketResponse = new AsyncAcquireGameServerSessionTicketResponse();
					RailConverter.Cpp2Csharp(data, asyncAcquireGameServerSessionTicketResponse);
					railEventCallBackHandler(event_id, asyncAcquireGameServerSessionTicketResponse);
					break;
				}
				case RAILEventID.kRailEventBrowserStateChanged:
				{
					BrowserRenderStateChanged browserRenderStateChanged = new BrowserRenderStateChanged();
					RailConverter.Cpp2Csharp(data, browserRenderStateChanged);
					railEventCallBackHandler(event_id, browserRenderStateChanged);
					break;
				}
				case RAILEventID.kRailEventUserSpaceRemoveSpaceWorkResult:
				{
					AsyncRemoveSpaceWorkResult asyncRemoveSpaceWorkResult = new AsyncRemoveSpaceWorkResult();
					RailConverter.Cpp2Csharp(data, asyncRemoveSpaceWorkResult);
					railEventCallBackHandler(event_id, asyncRemoveSpaceWorkResult);
					break;
				}
				case RAILEventID.kRailEventScreenshotTakeScreenshotRequest:
				{
					ScreenshotRequestInfo screenshotRequestInfo = new ScreenshotRequestInfo();
					RailConverter.Cpp2Csharp(data, screenshotRequestInfo);
					railEventCallBackHandler(event_id, screenshotRequestInfo);
					break;
				}
				case RAILEventID.kRailEventSessionTicketAuthSessionTicket:
				{
					StartSessionWithPlayerResponse startSessionWithPlayerResponse = new StartSessionWithPlayerResponse();
					RailConverter.Cpp2Csharp(data, startSessionWithPlayerResponse);
					railEventCallBackHandler(event_id, startSessionWithPlayerResponse);
					break;
				}
				case RAILEventID.kRailEventStatsPlayerStatsStored:
				{
					PlayerStatsStored playerStatsStored = new PlayerStatsStored();
					RailConverter.Cpp2Csharp(data, playerStatsStored);
					railEventCallBackHandler(event_id, playerStatsStored);
					break;
				}
				case RAILEventID.kRailEventRoomGotRoomMembers:
				{
					RoomMembersInfo roomMembersInfo = new RoomMembersInfo();
					RailConverter.Cpp2Csharp(data, roomMembersInfo);
					railEventCallBackHandler(event_id, roomMembersInfo);
					break;
				}
				case RAILEventID.kRailEventAppQuerySubscribeWishPlayStateResult:
				{
					QuerySubscribeWishPlayStateResult querySubscribeWishPlayStateResult = new QuerySubscribeWishPlayStateResult();
					RailConverter.Cpp2Csharp(data, querySubscribeWishPlayStateResult);
					railEventCallBackHandler(event_id, querySubscribeWishPlayStateResult);
					break;
				}
				case RAILEventID.kRailEventSessionTicketGetSessionTicket:
				{
					AcquireSessionTicketResponse acquireSessionTicketResponse = new AcquireSessionTicketResponse();
					RailConverter.Cpp2Csharp(data, acquireSessionTicketResponse);
					railEventCallBackHandler(event_id, acquireSessionTicketResponse);
					break;
				}
				case RAILEventID.kRailEventGameServerAddFavoriteGameServer:
				{
					AsyncAddFavoriteGameServerResult asyncAddFavoriteGameServerResult = new AsyncAddFavoriteGameServerResult();
					RailConverter.Cpp2Csharp(data, asyncAddFavoriteGameServerResult);
					railEventCallBackHandler(event_id, asyncAddFavoriteGameServerResult);
					break;
				}
				case RAILEventID.kRailEventNetChannelCreateChannelResult:
				{
					CreateChannelResult createChannelResult = new CreateChannelResult();
					RailConverter.Cpp2Csharp(data, createChannelResult);
					railEventCallBackHandler(event_id, createChannelResult);
					break;
				}
				case RAILEventID.kRailEventScreenshotPublishScreenshotFinished:
				{
					PublishScreenshotResult publishScreenshotResult = new PublishScreenshotResult();
					RailConverter.Cpp2Csharp(data, publishScreenshotResult);
					railEventCallBackHandler(event_id, publishScreenshotResult);
					break;
				}
				case RAILEventID.kRailEventAssetsSplitToFinished:
				{
					SplitAssetsToFinished splitAssetsToFinished = new SplitAssetsToFinished();
					RailConverter.Cpp2Csharp(data, splitAssetsToFinished);
					railEventCallBackHandler(event_id, splitAssetsToFinished);
					break;
				}
				case RAILEventID.kRailEventBrowserCreateResult:
				{
					CreateBrowserResult createBrowserResult = new CreateBrowserResult();
					RailConverter.Cpp2Csharp(data, createBrowserResult);
					railEventCallBackHandler(event_id, createBrowserResult);
					break;
				}
				case RAILEventID.kRailEventFriendsSetMetadataResult:
				{
					RailFriendsSetMetadataResult railFriendsSetMetadataResult = new RailFriendsSetMetadataResult();
					RailConverter.Cpp2Csharp(data, railFriendsSetMetadataResult);
					railEventCallBackHandler(event_id, railFriendsSetMetadataResult);
					break;
				}
				case RAILEventID.kRailEventStorageAsyncDeleteStreamFileResult:
				{
					AsyncDeleteStreamFileResult asyncDeleteStreamFileResult = new AsyncDeleteStreamFileResult();
					RailConverter.Cpp2Csharp(data, asyncDeleteStreamFileResult);
					railEventCallBackHandler(event_id, asyncDeleteStreamFileResult);
					break;
				}
				case RAILEventID.kRailEventUsersGetUsersInfo:
				{
					RailUsersInfoData railUsersInfoData = new RailUsersInfoData();
					RailConverter.Cpp2Csharp(data, railUsersInfoData);
					railEventCallBackHandler(event_id, railUsersInfoData);
					break;
				}
				case RAILEventID.kRailEventGameServerPlayerListResult:
				{
					GetGameServerPlayerListResult getGameServerPlayerListResult = new GetGameServerPlayerListResult();
					RailConverter.Cpp2Csharp(data, getGameServerPlayerListResult);
					railEventCallBackHandler(event_id, getGameServerPlayerListResult);
					break;
				}
				case RAILEventID.kRailEventDlcRefundChanged:
				{
					DlcRefundChanged dlcRefundChanged = new DlcRefundChanged();
					RailConverter.Cpp2Csharp(data, dlcRefundChanged);
					railEventCallBackHandler(event_id, dlcRefundChanged);
					break;
				}
				case RAILEventID.kRailEventAssetsUpdateAssetPropertyFinished:
				{
					UpdateAssetsPropertyFinished updateAssetsPropertyFinished = new UpdateAssetsPropertyFinished();
					RailConverter.Cpp2Csharp(data, updateAssetsPropertyFinished);
					railEventCallBackHandler(event_id, updateAssetsPropertyFinished);
					break;
				}
				case RAILEventID.kRailEventUserSpaceSyncResult:
				{
					SyncSpaceWorkResult syncSpaceWorkResult = new SyncSpaceWorkResult();
					RailConverter.Cpp2Csharp(data, syncSpaceWorkResult);
					railEventCallBackHandler(event_id, syncSpaceWorkResult);
					break;
				}
				case RAILEventID.kRailEventDlcQueryIsOwnedDlcsResult:
				{
					QueryIsOwnedDlcsResult queryIsOwnedDlcsResult = new QueryIsOwnedDlcsResult();
					RailConverter.Cpp2Csharp(data, queryIsOwnedDlcsResult);
					railEventCallBackHandler(event_id, queryIsOwnedDlcsResult);
					break;
				}
				case RAILEventID.kRailEventRoomNotifyRoomDataReceived:
				{
					RoomDataReceived roomDataReceived = new RoomDataReceived();
					RailConverter.Cpp2Csharp(data, roomDataReceived);
					railEventCallBackHandler(event_id, roomDataReceived);
					break;
				}
				case RAILEventID.kRailEventRoomNotifyRoomGameServerChanged:
				{
					NotifyRoomGameServerChange notifyRoomGameServerChange = new NotifyRoomGameServerChange();
					RailConverter.Cpp2Csharp(data, notifyRoomGameServerChange);
					railEventCallBackHandler(event_id, notifyRoomGameServerChange);
					break;
				}
				case RAILEventID.kRailEventLeaderboardEntryReceived:
				{
					LeaderboardEntryReceived leaderboardEntryReceived = new LeaderboardEntryReceived();
					RailConverter.Cpp2Csharp(data, leaderboardEntryReceived);
					railEventCallBackHandler(event_id, leaderboardEntryReceived);
					break;
				}
				case RAILEventID.kRailEventFriendsFriendsListChanged:
				{
					RailFriendsListChanged railFriendsListChanged = new RailFriendsListChanged();
					RailConverter.Cpp2Csharp(data, railFriendsListChanged);
					railEventCallBackHandler(event_id, railFriendsListChanged);
					break;
				}
				case RAILEventID.kRailEventFriendsClearMetadataResult:
				{
					RailFriendsClearMetadataResult railFriendsClearMetadataResult = new RailFriendsClearMetadataResult();
					RailConverter.Cpp2Csharp(data, railFriendsClearMetadataResult);
					railEventCallBackHandler(event_id, railFriendsClearMetadataResult);
					break;
				}
				case RAILEventID.kRailEventRoomGetAllDataResult:
				{
					RoomAllData roomAllData = new RoomAllData();
					RailConverter.Cpp2Csharp(data, roomAllData);
					railEventCallBackHandler(event_id, roomAllData);
					break;
				}
				case RAILEventID.kRailEventRoomClearRoomMetadataResult:
				{
					ClearRoomMetadataInfo clearRoomMetadataInfo = new ClearRoomMetadataInfo();
					RailConverter.Cpp2Csharp(data, clearRoomMetadataInfo);
					railEventCallBackHandler(event_id, clearRoomMetadataInfo);
					break;
				}
				case RAILEventID.kRailEventHttpSessionResponseResult:
				{
					RailHttpSessionResponse railHttpSessionResponse = new RailHttpSessionResponse();
					RailConverter.Cpp2Csharp(data, railHttpSessionResponse);
					railEventCallBackHandler(event_id, railHttpSessionResponse);
					break;
				}
				case RAILEventID.kRailEventDlcUninstallFinished:
				{
					DlcUninstallFinished dlcUninstallFinished = new DlcUninstallFinished();
					RailConverter.Cpp2Csharp(data, dlcUninstallFinished);
					railEventCallBackHandler(event_id, dlcUninstallFinished);
					break;
				}
				case RAILEventID.kRailEventSystemStateChanged:
				{
					RailSystemStateChanged railSystemStateChanged = new RailSystemStateChanged();
					RailConverter.Cpp2Csharp(data, railSystemStateChanged);
					railEventCallBackHandler(event_id, railSystemStateChanged);
					break;
				}
				case RAILEventID.kRailEventUtilsGetImageDataResult:
				{
					RailGetImageDataResult railGetImageDataResult = new RailGetImageDataResult();
					RailConverter.Cpp2Csharp(data, railGetImageDataResult);
					railEventCallBackHandler(event_id, railGetImageDataResult);
					break;
				}
				case RAILEventID.kRailEventNetChannelJoinChannelResult:
				{
					JoinChannelResult joinChannelResult = new JoinChannelResult();
					RailConverter.Cpp2Csharp(data, joinChannelResult);
					railEventCallBackHandler(event_id, joinChannelResult);
					break;
				}
				case RAILEventID.kRailEventStorageShareToSpaceWorkResult:
				{
					ShareStorageToSpaceWorkResult shareStorageToSpaceWorkResult = new ShareStorageToSpaceWorkResult();
					RailConverter.Cpp2Csharp(data, shareStorageToSpaceWorkResult);
					railEventCallBackHandler(event_id, shareStorageToSpaceWorkResult);
					break;
				}
				case RAILEventID.kRailEventUserSpaceGetMySubscribedWorksResult:
				{
					AsyncGetMySubscribedWorksResult asyncGetMySubscribedWorksResult = new AsyncGetMySubscribedWorksResult();
					RailConverter.Cpp2Csharp(data, asyncGetMySubscribedWorksResult);
					railEventCallBackHandler(event_id, asyncGetMySubscribedWorksResult);
					break;
				}
				case RAILEventID.kRailEventRoomGetUserRoomListResult:
				{
					UserRoomListInfo userRoomListInfo = new UserRoomListInfo();
					RailConverter.Cpp2Csharp(data, userRoomListInfo);
					railEventCallBackHandler(event_id, userRoomListInfo);
					break;
				}
				case RAILEventID.kRailEventBrowserTitleChanged:
				{
					BrowserRenderTitleChanged browserRenderTitleChanged = new BrowserRenderTitleChanged();
					RailConverter.Cpp2Csharp(data, browserRenderTitleChanged);
					railEventCallBackHandler(event_id, browserRenderTitleChanged);
					break;
				}
				case RAILEventID.kRailEventStorageAsyncReadStreamFileResult:
				{
					AsyncReadStreamFileResult asyncReadStreamFileResult = new AsyncReadStreamFileResult();
					RailConverter.Cpp2Csharp(data, asyncReadStreamFileResult);
					railEventCallBackHandler(event_id, asyncReadStreamFileResult);
					break;
				}
				case RAILEventID.kRailEventVoiceChannelDataCaptured:
				{
					VoiceDataCapturedEvent voiceDataCapturedEvent = new VoiceDataCapturedEvent();
					RailConverter.Cpp2Csharp(data, voiceDataCapturedEvent);
					railEventCallBackHandler(event_id, voiceDataCapturedEvent);
					break;
				}
				case RAILEventID.kRailEventVoiceChannelInviteEvent:
				{
					VoiceChannelInviteEvent voiceChannelInviteEvent = new VoiceChannelInviteEvent();
					RailConverter.Cpp2Csharp(data, voiceChannelInviteEvent);
					railEventCallBackHandler(event_id, voiceChannelInviteEvent);
					break;
				}
				case RAILEventID.kRailEventInGameStorePurchasePayWindowClosed:
				{
					RailInGameStorePurchasePayWindowClosed railInGameStorePurchasePayWindowClosed = new RailInGameStorePurchasePayWindowClosed();
					RailConverter.Cpp2Csharp(data, railInGameStorePurchasePayWindowClosed);
					railEventCallBackHandler(event_id, railInGameStorePurchasePayWindowClosed);
					break;
				}
				case RAILEventID.kRailEventLeaderboardUploaded:
				{
					LeaderboardUploaded leaderboardUploaded = new LeaderboardUploaded();
					RailConverter.Cpp2Csharp(data, leaderboardUploaded);
					railEventCallBackHandler(event_id, leaderboardUploaded);
					break;
				}
				case RAILEventID.kRailEventVoiceChannelLeaveResult:
				{
					LeaveVoiceChannelResult leaveVoiceChannelResult = new LeaveVoiceChannelResult();
					RailConverter.Cpp2Csharp(data, leaveVoiceChannelResult);
					railEventCallBackHandler(event_id, leaveVoiceChannelResult);
					break;
				}
				case RAILEventID.kRailEventPlayerGetAuthenticateURL:
				{
					GetAuthenticateURLResult getAuthenticateURLResult = new GetAuthenticateURLResult();
					RailConverter.Cpp2Csharp(data, getAuthenticateURLResult);
					railEventCallBackHandler(event_id, getAuthenticateURLResult);
					break;
				}
				case RAILEventID.kRailEventGameServerGetMetadataResult:
				{
					GetGameServerMetadataResult getGameServerMetadataResult = new GetGameServerMetadataResult();
					RailConverter.Cpp2Csharp(data, getGameServerMetadataResult);
					railEventCallBackHandler(event_id, getGameServerMetadataResult);
					break;
				}
				case RAILEventID.kRailEventAssetsCompleteConsumeFinished:
				{
					CompleteConsumeAssetsFinished completeConsumeAssetsFinished = new CompleteConsumeAssetsFinished();
					RailConverter.Cpp2Csharp(data, completeConsumeAssetsFinished);
					railEventCallBackHandler(event_id, completeConsumeAssetsFinished);
					break;
				}
				case RAILEventID.kRailEventStatsPlayerStatsReceived:
				{
					PlayerStatsReceived playerStatsReceived = new PlayerStatsReceived();
					RailConverter.Cpp2Csharp(data, playerStatsReceived);
					railEventCallBackHandler(event_id, playerStatsReceived);
					break;
				}
				case RAILEventID.kRailEventVoiceChannelPushToTalkKeyChangedEvent:
				{
					VoiceChannelPushToTalkKeyChangedEvent voiceChannelPushToTalkKeyChangedEvent = new VoiceChannelPushToTalkKeyChangedEvent();
					RailConverter.Cpp2Csharp(data, voiceChannelPushToTalkKeyChangedEvent);
					railEventCallBackHandler(event_id, voiceChannelPushToTalkKeyChangedEvent);
					break;
				}
				case RAILEventID.kRailEventFriendsGetMetadataResult:
				{
					RailFriendsGetMetadataResult railFriendsGetMetadataResult = new RailFriendsGetMetadataResult();
					RailConverter.Cpp2Csharp(data, railFriendsGetMetadataResult);
					railEventCallBackHandler(event_id, railFriendsGetMetadataResult);
					break;
				}
				case RAILEventID.kRailEventRoomGetMemberMetadataResult:
				{
					GetMemberMetadataInfo getMemberMetadataInfo = new GetMemberMetadataInfo();
					RailConverter.Cpp2Csharp(data, getMemberMetadataInfo);
					railEventCallBackHandler(event_id, getMemberMetadataInfo);
					break;
				}
				case RAILEventID.kRailEventAssetsCompleteConsumeByExchangeAssetsToFinished:
				{
					CompleteConsumeByExchangeAssetsToFinished completeConsumeByExchangeAssetsToFinished = new CompleteConsumeByExchangeAssetsToFinished();
					RailConverter.Cpp2Csharp(data, completeConsumeByExchangeAssetsToFinished);
					railEventCallBackHandler(event_id, completeConsumeByExchangeAssetsToFinished);
					break;
				}
				case RAILEventID.kRailEventUserSpaceSearchSpaceWorkResult:
				{
					AsyncSearchSpaceWorksResult asyncSearchSpaceWorksResult = new AsyncSearchSpaceWorksResult();
					RailConverter.Cpp2Csharp(data, asyncSearchSpaceWorksResult);
					railEventCallBackHandler(event_id, asyncSearchSpaceWorksResult);
					break;
				}
				case RAILEventID.kRailEventStatsGlobalStatsReceived:
				{
					GlobalStatsRequestReceived globalStatsRequestReceived = new GlobalStatsRequestReceived();
					RailConverter.Cpp2Csharp(data, globalStatsRequestReceived);
					railEventCallBackHandler(event_id, globalStatsRequestReceived);
					break;
				}
				case RAILEventID.kRailEventAssetsExchangeAssetsToFinished:
				{
					ExchangeAssetsToFinished exchangeAssetsToFinished = new ExchangeAssetsToFinished();
					RailConverter.Cpp2Csharp(data, exchangeAssetsToFinished);
					railEventCallBackHandler(event_id, exchangeAssetsToFinished);
					break;
				}
				case RAILEventID.kRailEventStatsNumOfPlayerReceived:
				{
					NumberOfPlayerReceived numberOfPlayerReceived = new NumberOfPlayerReceived();
					RailConverter.Cpp2Csharp(data, numberOfPlayerReceived);
					railEventCallBackHandler(event_id, numberOfPlayerReceived);
					break;
				}
				case RAILEventID.kRailEventVoiceChannelUsersSpeakingStateChangedEvent:
				{
					VoiceChannelUsersSpeakingStateChangedEvent voiceChannelUsersSpeakingStateChangedEvent = new VoiceChannelUsersSpeakingStateChangedEvent();
					RailConverter.Cpp2Csharp(data, voiceChannelUsersSpeakingStateChangedEvent);
					railEventCallBackHandler(event_id, voiceChannelUsersSpeakingStateChangedEvent);
					break;
				}
				case RAILEventID.kRailEventFriendsGetInviteCommandLine:
				{
					RailFriendsGetInviteCommandLine railFriendsGetInviteCommandLine = new RailFriendsGetInviteCommandLine();
					RailConverter.Cpp2Csharp(data, railFriendsGetInviteCommandLine);
					railEventCallBackHandler(event_id, railFriendsGetInviteCommandLine);
					break;
				}
				case RAILEventID.kRailEventBrowserNavigeteResult:
				{
					BrowserRenderNavigateResult browserRenderNavigateResult = new BrowserRenderNavigateResult();
					RailConverter.Cpp2Csharp(data, browserRenderNavigateResult);
					railEventCallBackHandler(event_id, browserRenderNavigateResult);
					break;
				}
				case RAILEventID.kRailEventDlcOwnershipChanged:
				{
					DlcOwnershipChanged dlcOwnershipChanged = new DlcOwnershipChanged();
					RailConverter.Cpp2Csharp(data, dlcOwnershipChanged);
					railEventCallBackHandler(event_id, dlcOwnershipChanged);
					break;
				}
				case RAILEventID.kRailEventStorageQueryQuotaResult:
				{
					AsyncQueryQuotaResult asyncQueryQuotaResult = new AsyncQueryQuotaResult();
					RailConverter.Cpp2Csharp(data, asyncQueryQuotaResult);
					railEventCallBackHandler(event_id, asyncQueryQuotaResult);
					break;
				}
				case RAILEventID.kRailEventRoomCreated:
				{
					CreateRoomInfo createRoomInfo = new CreateRoomInfo();
					RailConverter.Cpp2Csharp(data, createRoomInfo);
					railEventCallBackHandler(event_id, createRoomInfo);
					break;
				}
				case RAILEventID.kRailEventRoomLeaveRoomResult:
				{
					LeaveRoomInfo leaveRoomInfo = new LeaveRoomInfo();
					RailConverter.Cpp2Csharp(data, leaveRoomInfo);
					railEventCallBackHandler(event_id, leaveRoomInfo);
					break;
				}
				case RAILEventID.kRailEventAchievementPlayerAchievementReceived:
				{
					PlayerAchievementReceived playerAchievementReceived = new PlayerAchievementReceived();
					RailConverter.Cpp2Csharp(data, playerAchievementReceived);
					railEventCallBackHandler(event_id, playerAchievementReceived);
					break;
				}
				case RAILEventID.kRailEventBrowserJavascriptEvent:
				{
					JavascriptEventResult javascriptEventResult = new JavascriptEventResult();
					RailConverter.Cpp2Csharp(data, javascriptEventResult);
					railEventCallBackHandler(event_id, javascriptEventResult);
					break;
				}
				case RAILEventID.kRailEventGameServerListResult:
				{
					GetGameServerListResult getGameServerListResult = new GetGameServerListResult();
					RailConverter.Cpp2Csharp(data, getGameServerListResult);
					railEventCallBackHandler(event_id, getGameServerListResult);
					break;
				}
				case RAILEventID.kRailEventNetChannelMemberStateChanged:
				{
					ChannelMemberStateChanged channelMemberStateChanged = new ChannelMemberStateChanged();
					RailConverter.Cpp2Csharp(data, channelMemberStateChanged);
					railEventCallBackHandler(event_id, channelMemberStateChanged);
					break;
				}
				case RAILEventID.kRailEventInGamePurchaseFinishOrderResult:
				{
					RailInGamePurchaseFinishOrderResponse railInGamePurchaseFinishOrderResponse = new RailInGamePurchaseFinishOrderResponse();
					RailConverter.Cpp2Csharp(data, railInGamePurchaseFinishOrderResponse);
					railEventCallBackHandler(event_id, railInGamePurchaseFinishOrderResponse);
					break;
				}
				case RAILEventID.kRailEventVoiceChannelJoinedResult:
				{
					JoinVoiceChannelResult joinVoiceChannelResult = new JoinVoiceChannelResult();
					RailConverter.Cpp2Csharp(data, joinVoiceChannelResult);
					railEventCallBackHandler(event_id, joinVoiceChannelResult);
					break;
				}
				case RAILEventID.kRailEventAssetsUpdateConsumeFinished:
				{
					UpdateConsumeAssetsFinished updateConsumeAssetsFinished = new UpdateConsumeAssetsFinished();
					RailConverter.Cpp2Csharp(data, updateConsumeAssetsFinished);
					railEventCallBackHandler(event_id, updateConsumeAssetsFinished);
					break;
				}
				case RAILEventID.kRailEventUserSpaceUpdateMetadataResult:
				{
					AsyncUpdateMetadataResult asyncUpdateMetadataResult = new AsyncUpdateMetadataResult();
					RailConverter.Cpp2Csharp(data, asyncUpdateMetadataResult);
					railEventCallBackHandler(event_id, asyncUpdateMetadataResult);
					break;
				}
				case RAILEventID.kRailEventFriendsOnlineStateChanged:
				{
					RailFriendsOnlineStateChanged railFriendsOnlineStateChanged = new RailFriendsOnlineStateChanged();
					RailConverter.Cpp2Csharp(data, railFriendsOnlineStateChanged);
					railEventCallBackHandler(event_id, railFriendsOnlineStateChanged);
					break;
				}
				case RAILEventID.kRailEventUsersInviteUsersResult:
				{
					RailUsersInviteUsersResult railUsersInviteUsersResult = new RailUsersInviteUsersResult();
					RailConverter.Cpp2Csharp(data, railUsersInviteUsersResult);
					railEventCallBackHandler(event_id, railUsersInviteUsersResult);
					break;
				}
				case RAILEventID.kRailEventGameServerSetMetadataResult:
				{
					SetGameServerMetadataResult setGameServerMetadataResult = new SetGameServerMetadataResult();
					RailConverter.Cpp2Csharp(data, setGameServerMetadataResult);
					railEventCallBackHandler(event_id, setGameServerMetadataResult);
					break;
				}
				case RAILEventID.kRailEventTextInputShowTextInputWindowResult:
				{
					RailTextInputResult railTextInputResult = new RailTextInputResult();
					RailConverter.Cpp2Csharp(data, railTextInputResult);
					railEventCallBackHandler(event_id, railTextInputResult);
					break;
				}
				case RAILEventID.kRailEventRoomNotifyMetadataChanged:
				{
					NotifyMetadataChange notifyMetadataChange = new NotifyMetadataChange();
					RailConverter.Cpp2Csharp(data, notifyMetadataChange);
					railEventCallBackHandler(event_id, notifyMetadataChange);
					break;
				}
				case RAILEventID.kRailEventIMEHelperTextInputSelectedResult:
				{
					RailIMEHelperTextInputSelectedResult railIMEHelperTextInputSelectedResult = new RailIMEHelperTextInputSelectedResult();
					RailConverter.Cpp2Csharp(data, railIMEHelperTextInputSelectedResult);
					railEventCallBackHandler(event_id, railIMEHelperTextInputSelectedResult);
					break;
				}
				case RAILEventID.kRailEventUserSpaceVoteSpaceWorkResult:
				{
					AsyncVoteSpaceWorkResult asyncVoteSpaceWorkResult = new AsyncVoteSpaceWorkResult();
					RailConverter.Cpp2Csharp(data, asyncVoteSpaceWorkResult);
					railEventCallBackHandler(event_id, asyncVoteSpaceWorkResult);
					break;
				}
				case RAILEventID.kRailEventRoomSetMemberMetadataResult:
				{
					SetMemberMetadataInfo setMemberMetadataInfo = new SetMemberMetadataInfo();
					RailConverter.Cpp2Csharp(data, setMemberMetadataInfo);
					railEventCallBackHandler(event_id, setMemberMetadataInfo);
					break;
				}
				case RAILEventID.kRailEventInGameStorePurchasePayWindowDisplayed:
				{
					RailInGameStorePurchasePayWindowDisplayed railInGameStorePurchasePayWindowDisplayed = new RailInGameStorePurchasePayWindowDisplayed();
					RailConverter.Cpp2Csharp(data, railInGameStorePurchasePayWindowDisplayed);
					railEventCallBackHandler(event_id, railInGameStorePurchasePayWindowDisplayed);
					break;
				}
				case RAILEventID.kRailEventShowFloatingNotifyWindow:
				{
					ShowNotifyWindow showNotifyWindow = new ShowNotifyWindow();
					RailConverter.Cpp2Csharp(data, showNotifyWindow);
					railEventCallBackHandler(event_id, showNotifyWindow);
					break;
				}
				case RAILEventID.kRailEventUsersShowUserHomepageWindowResult:
				{
					RailShowUserHomepageWindowResult railShowUserHomepageWindowResult = new RailShowUserHomepageWindowResult();
					RailConverter.Cpp2Csharp(data, railShowUserHomepageWindowResult);
					railEventCallBackHandler(event_id, railShowUserHomepageWindowResult);
					break;
				}
				case RAILEventID.kRailEventBrowserReloadResult:
				{
					ReloadBrowserResult reloadBrowserResult = new ReloadBrowserResult();
					RailConverter.Cpp2Csharp(data, reloadBrowserResult);
					railEventCallBackHandler(event_id, reloadBrowserResult);
					break;
				}
				case RAILEventID.kRailEventInGamePurchasePurchaseProductsToAssetsResult:
				{
					RailInGamePurchasePurchaseProductsToAssetsResponse railInGamePurchasePurchaseProductsToAssetsResponse = new RailInGamePurchasePurchaseProductsToAssetsResponse();
					RailConverter.Cpp2Csharp(data, railInGamePurchasePurchaseProductsToAssetsResponse);
					railEventCallBackHandler(event_id, railInGamePurchasePurchaseProductsToAssetsResponse);
					break;
				}
				case RAILEventID.kRailEventNetChannelInviteJoinChannelRequest:
				{
					InviteJoinChannelRequest inviteJoinChannelRequest = new InviteJoinChannelRequest();
					RailConverter.Cpp2Csharp(data, inviteJoinChannelRequest);
					railEventCallBackHandler(event_id, inviteJoinChannelRequest);
					break;
				}
				case RAILEventID.kRailEventRoomGetRoomMetadataResult:
				{
					GetRoomMetadataInfo getRoomMetadataInfo = new GetRoomMetadataInfo();
					RailConverter.Cpp2Csharp(data, getRoomMetadataInfo);
					railEventCallBackHandler(event_id, getRoomMetadataInfo);
					break;
				}
				case RAILEventID.kRailEventScreenshotTakeScreenshotFinished:
				{
					TakeScreenshotResult takeScreenshotResult = new TakeScreenshotResult();
					RailConverter.Cpp2Csharp(data, takeScreenshotResult);
					railEventCallBackHandler(event_id, takeScreenshotResult);
					break;
				}
				case RAILEventID.kRailEventDlcCheckAllDlcsStateReadyResult:
				{
					CheckAllDlcsStateReadyResult checkAllDlcsStateReadyResult = new CheckAllDlcsStateReadyResult();
					RailConverter.Cpp2Csharp(data, checkAllDlcsStateReadyResult);
					railEventCallBackHandler(event_id, checkAllDlcsStateReadyResult);
					break;
				}
				case RAILEventID.kRailEventRoomZoneListResult:
				{
					ZoneInfoList zoneInfoList = new ZoneInfoList();
					RailConverter.Cpp2Csharp(data, zoneInfoList);
					railEventCallBackHandler(event_id, zoneInfoList);
					break;
				}
				case RAILEventID.kRailEventInGameStorePurchasePaymentResult:
				{
					RailInGameStorePurchaseResult railInGameStorePurchaseResult = new RailInGameStorePurchaseResult();
					RailConverter.Cpp2Csharp(data, railInGameStorePurchaseResult);
					railEventCallBackHandler(event_id, railInGameStorePurchaseResult);
					break;
				}
				case RAILEventID.kRailEventGameServerRegisterToServerListResult:
				{
					GameServerRegisterToServerListResult gameServerRegisterToServerListResult = new GameServerRegisterToServerListResult();
					RailConverter.Cpp2Csharp(data, gameServerRegisterToServerListResult);
					railEventCallBackHandler(event_id, gameServerRegisterToServerListResult);
					break;
				}
				case RAILEventID.kRailEventFriendsQueryPlayedWithFriendsTimeResult:
				{
					RailFriendsQueryPlayedWithFriendsTimeResult railFriendsQueryPlayedWithFriendsTimeResult = new RailFriendsQueryPlayedWithFriendsTimeResult();
					RailConverter.Cpp2Csharp(data, railFriendsQueryPlayedWithFriendsTimeResult);
					railEventCallBackHandler(event_id, railFriendsQueryPlayedWithFriendsTimeResult);
					break;
				}
				case RAILEventID.kRailEventGameServerAuthSessionTicket:
				{
					GameServerStartSessionWithPlayerResponse gameServerStartSessionWithPlayerResponse = new GameServerStartSessionWithPlayerResponse();
					RailConverter.Cpp2Csharp(data, gameServerStartSessionWithPlayerResponse);
					railEventCallBackHandler(event_id, gameServerStartSessionWithPlayerResponse);
					break;
				}
				case RAILEventID.kRailEventRoomJoinRoomResult:
				{
					JoinRoomInfo joinRoomInfo = new JoinRoomInfo();
					RailConverter.Cpp2Csharp(data, joinRoomInfo);
					railEventCallBackHandler(event_id, joinRoomInfo);
					break;
				}
				case RAILEventID.kRailEventRoomNotifyMemberkicked:
				{
					NotifyRoomMemberKicked notifyRoomMemberKicked = new NotifyRoomMemberKicked();
					RailConverter.Cpp2Csharp(data, notifyRoomMemberKicked);
					railEventCallBackHandler(event_id, notifyRoomMemberKicked);
					break;
				}
				case RAILEventID.kRailEventAssetsMergeFinished:
				{
					MergeAssetsFinished mergeAssetsFinished = new MergeAssetsFinished();
					RailConverter.Cpp2Csharp(data, mergeAssetsFinished);
					railEventCallBackHandler(event_id, mergeAssetsFinished);
					break;
				}
				case RAILEventID.kRailEventInGamePurchaseAllProductsInfoReceived:
				{
					RailInGamePurchaseRequestAllProductsResponse railInGamePurchaseRequestAllProductsResponse = new RailInGamePurchaseRequestAllProductsResponse();
					RailConverter.Cpp2Csharp(data, railInGamePurchaseRequestAllProductsResponse);
					railEventCallBackHandler(event_id, railInGamePurchaseRequestAllProductsResponse);
					break;
				}
				case RAILEventID.kRailEventNetChannelChannelNetDelay:
				{
					ChannelNetDelay channelNetDelay = new ChannelNetDelay();
					RailConverter.Cpp2Csharp(data, channelNetDelay);
					railEventCallBackHandler(event_id, channelNetDelay);
					break;
				}
				case RAILEventID.kRailEventFriendsAddFriendResult:
				{
					RailFriendsAddFriendResult railFriendsAddFriendResult = new RailFriendsAddFriendResult();
					RailConverter.Cpp2Csharp(data, railFriendsAddFriendResult);
					railEventCallBackHandler(event_id, railFriendsAddFriendResult);
					break;
				}
				case RAILEventID.kRailEventVoiceChannelMemberChangedEvent:
				{
					VoiceChannelMemeberChangedEvent voiceChannelMemeberChangedEvent = new VoiceChannelMemeberChangedEvent();
					RailConverter.Cpp2Csharp(data, voiceChannelMemeberChangedEvent);
					railEventCallBackHandler(event_id, voiceChannelMemeberChangedEvent);
					break;
				}
				case RAILEventID.kRailEventStorageAsyncListStreamFileResult:
				{
					AsyncListFileResult asyncListFileResult = new AsyncListFileResult();
					RailConverter.Cpp2Csharp(data, asyncListFileResult);
					railEventCallBackHandler(event_id, asyncListFileResult);
					break;
				}
				case RAILEventID.kRailEventFriendsGetFriendPlayedGamesResult:
				{
					RailFriendsQueryFriendPlayedGamesResult railFriendsQueryFriendPlayedGamesResult = new RailFriendsQueryFriendPlayedGamesResult();
					RailConverter.Cpp2Csharp(data, railFriendsQueryFriendPlayedGamesResult);
					railEventCallBackHandler(event_id, railFriendsQueryFriendPlayedGamesResult);
					break;
				}
				case RAILEventID.kRailEventInGamePurchaseAllPurchasableProductsInfoReceived:
				{
					RailInGamePurchaseRequestAllPurchasableProductsResponse railInGamePurchaseRequestAllPurchasableProductsResponse = new RailInGamePurchaseRequestAllPurchasableProductsResponse();
					RailConverter.Cpp2Csharp(data, railInGamePurchaseRequestAllPurchasableProductsResponse);
					railEventCallBackHandler(event_id, railInGamePurchaseRequestAllPurchasableProductsResponse);
					break;
				}
				case RAILEventID.kRailEventAssetsExchangeAssetsFinished:
				{
					ExchangeAssetsFinished exchangeAssetsFinished = new ExchangeAssetsFinished();
					RailConverter.Cpp2Csharp(data, exchangeAssetsFinished);
					railEventCallBackHandler(event_id, exchangeAssetsFinished);
					break;
				}
				case RAILEventID.kRailEventBrowserPaint:
				{
					BrowserNeedsPaintRequest browserNeedsPaintRequest = new BrowserNeedsPaintRequest();
					RailConverter.Cpp2Csharp(data, browserNeedsPaintRequest);
					railEventCallBackHandler(event_id, browserNeedsPaintRequest);
					break;
				}
				case RAILEventID.kRailEventRoomNotifyRoomDestroyed:
				{
					NotifyRoomDestroy notifyRoomDestroy = new NotifyRoomDestroy();
					RailConverter.Cpp2Csharp(data, notifyRoomDestroy);
					railEventCallBackHandler(event_id, notifyRoomDestroy);
					break;
				}
				case RAILEventID.kRailEventRoomListResult:
				{
					RoomInfoList roomInfoList = new RoomInfoList();
					RailConverter.Cpp2Csharp(data, roomInfoList);
					railEventCallBackHandler(event_id, roomInfoList);
					break;
				}
				case RAILEventID.kRailEventUserSpaceGetMyFavoritesWorksResult:
				{
					AsyncGetMyFavoritesWorksResult asyncGetMyFavoritesWorksResult = new AsyncGetMyFavoritesWorksResult();
					RailConverter.Cpp2Csharp(data, asyncGetMyFavoritesWorksResult);
					railEventCallBackHandler(event_id, asyncGetMyFavoritesWorksResult);
					break;
				}
				case RAILEventID.kRailEventStorageAsyncWriteFileResult:
				{
					AsyncWriteFileResult asyncWriteFileResult = new AsyncWriteFileResult();
					RailConverter.Cpp2Csharp(data, asyncWriteFileResult);
					railEventCallBackHandler(event_id, asyncWriteFileResult);
					break;
				}
				case RAILEventID.kRailEventAssetsDirectConsumeFinished:
				{
					DirectConsumeAssetsFinished directConsumeAssetsFinished = new DirectConsumeAssetsFinished();
					RailConverter.Cpp2Csharp(data, directConsumeAssetsFinished);
					railEventCallBackHandler(event_id, directConsumeAssetsFinished);
					break;
				}
				case RAILEventID.kRailEventVoiceChannelCreateResult:
				{
					CreateVoiceChannelResult createVoiceChannelResult = new CreateVoiceChannelResult();
					RailConverter.Cpp2Csharp(data, createVoiceChannelResult);
					railEventCallBackHandler(event_id, createVoiceChannelResult);
					break;
				}
				case RAILEventID.kRailEventUserSpaceModifyFavoritesWorksResult:
				{
					AsyncModifyFavoritesWorksResult asyncModifyFavoritesWorksResult = new AsyncModifyFavoritesWorksResult();
					RailConverter.Cpp2Csharp(data, asyncModifyFavoritesWorksResult);
					railEventCallBackHandler(event_id, asyncModifyFavoritesWorksResult);
					break;
				}
				case RAILEventID.kRailEventUsersGetUserLimitsResult:
				{
					RailUsersGetUserLimitsResult railUsersGetUserLimitsResult = new RailUsersGetUserLimitsResult();
					RailConverter.Cpp2Csharp(data, railUsersGetUserLimitsResult);
					railEventCallBackHandler(event_id, railUsersGetUserLimitsResult);
					break;
				}
				case RAILEventID.kRailPlatformNotifyEventJoinGameByRoom:
				{
					RailPlatformNotifyEventJoinGameByRoom railPlatformNotifyEventJoinGameByRoom = new RailPlatformNotifyEventJoinGameByRoom();
					RailConverter.Cpp2Csharp(data, railPlatformNotifyEventJoinGameByRoom);
					railEventCallBackHandler(event_id, railPlatformNotifyEventJoinGameByRoom);
					break;
				}
				case RAILEventID.kRailEventPlayerGetGamePurchaseKey:
				{
					PlayerGetGamePurchaseKeyResult playerGetGamePurchaseKeyResult = new PlayerGetGamePurchaseKeyResult();
					RailConverter.Cpp2Csharp(data, playerGetGamePurchaseKeyResult);
					railEventCallBackHandler(event_id, playerGetGamePurchaseKeyResult);
					break;
				}
				case RAILEventID.kRailEventStorageAsyncReadFileResult:
				{
					AsyncReadFileResult asyncReadFileResult = new AsyncReadFileResult();
					RailConverter.Cpp2Csharp(data, asyncReadFileResult);
					railEventCallBackHandler(event_id, asyncReadFileResult);
					break;
				}
				case RAILEventID.kRailEventBrowserDamageRectPaint:
				{
					BrowserDamageRectNeedsPaintRequest browserDamageRectNeedsPaintRequest = new BrowserDamageRectNeedsPaintRequest();
					RailConverter.Cpp2Csharp(data, browserDamageRectNeedsPaintRequest);
					railEventCallBackHandler(event_id, browserDamageRectNeedsPaintRequest);
					break;
				}
				case RAILEventID.kRailEventDlcInstallStartResult:
				{
					DlcInstallStartResult dlcInstallStartResult = new DlcInstallStartResult();
					RailConverter.Cpp2Csharp(data, dlcInstallStartResult);
					railEventCallBackHandler(event_id, dlcInstallStartResult);
					break;
				}
				case RAILEventID.kRailEventUsersCancelInviteResult:
				{
					RailUsersCancelInviteResult railUsersCancelInviteResult = new RailUsersCancelInviteResult();
					RailConverter.Cpp2Csharp(data, railUsersCancelInviteResult);
					railEventCallBackHandler(event_id, railUsersCancelInviteResult);
					break;
				}
				case RAILEventID.kRailEventFinalize:
				{
					RailFinalize railFinalize = new RailFinalize();
					RailConverter.Cpp2Csharp(data, railFinalize);
					railEventCallBackHandler(event_id, railFinalize);
					break;
				}
				case RAILEventID.kRailEventRoomKickOffMemberResult:
				{
					KickOffMemberInfo kickOffMemberInfo = new KickOffMemberInfo();
					RailConverter.Cpp2Csharp(data, kickOffMemberInfo);
					railEventCallBackHandler(event_id, kickOffMemberInfo);
					break;
				}
				case RAILEventID.kRailEventShowFloatingWindow:
				{
					ShowFloatingWindowResult showFloatingWindowResult = new ShowFloatingWindowResult();
					RailConverter.Cpp2Csharp(data, showFloatingWindowResult);
					railEventCallBackHandler(event_id, showFloatingWindowResult);
					break;
				}
				case RAILEventID.kRailEventRoomNotifyRoomOwnerChanged:
				{
					NotifyRoomOwnerChange notifyRoomOwnerChange = new NotifyRoomOwnerChange();
					RailConverter.Cpp2Csharp(data, notifyRoomOwnerChange);
					railEventCallBackHandler(event_id, notifyRoomOwnerChange);
					break;
				}
				case RAILEventID.kRailEventStorageAsyncRenameStreamFileResult:
				{
					AsyncRenameStreamFileResult asyncRenameStreamFileResult = new AsyncRenameStreamFileResult();
					RailConverter.Cpp2Csharp(data, asyncRenameStreamFileResult);
					railEventCallBackHandler(event_id, asyncRenameStreamFileResult);
					break;
				}
				case RAILEventID.kRailEventFriendsReportPlayedWithUserListResult:
				{
					RailFriendsReportPlayedWithUserListResult railFriendsReportPlayedWithUserListResult = new RailFriendsReportPlayedWithUserListResult();
					RailConverter.Cpp2Csharp(data, railFriendsReportPlayedWithUserListResult);
					railEventCallBackHandler(event_id, railFriendsReportPlayedWithUserListResult);
					break;
				}
				case RAILEventID.kRailEventStorageAsyncWriteStreamFileResult:
				{
					AsyncWriteStreamFileResult asyncWriteStreamFileResult = new AsyncWriteStreamFileResult();
					RailConverter.Cpp2Csharp(data, asyncWriteStreamFileResult);
					railEventCallBackHandler(event_id, asyncWriteStreamFileResult);
					break;
				}
				case RAILEventID.kRailEventAchievementGlobalAchievementReceived:
				{
					GlobalAchievementReceived globalAchievementReceived = new GlobalAchievementReceived();
					RailConverter.Cpp2Csharp(data, globalAchievementReceived);
					railEventCallBackHandler(event_id, globalAchievementReceived);
					break;
				}
				case RAILEventID.kRailEventUserSpaceQuerySpaceWorksResult:
				{
					AsyncQuerySpaceWorksResult asyncQuerySpaceWorksResult = new AsyncQuerySpaceWorksResult();
					RailConverter.Cpp2Csharp(data, asyncQuerySpaceWorksResult);
					railEventCallBackHandler(event_id, asyncQuerySpaceWorksResult);
					break;
				}
				case RAILEventID.kRailEventUsersGetInviteDetailResult:
				{
					RailUsersGetInviteDetailResult railUsersGetInviteDetailResult = new RailUsersGetInviteDetailResult();
					RailConverter.Cpp2Csharp(data, railUsersGetInviteDetailResult);
					railEventCallBackHandler(event_id, railUsersGetInviteDetailResult);
					break;
				}
				case RAILEventID.kRailEventDlcInstallStart:
				{
					DlcInstallStart dlcInstallStart = new DlcInstallStart();
					RailConverter.Cpp2Csharp(data, dlcInstallStart);
					railEventCallBackHandler(event_id, dlcInstallStart);
					break;
				}
				case RAILEventID.kRailEventGameServerCreated:
				{
					CreateGameServerResult createGameServerResult = new CreateGameServerResult();
					RailConverter.Cpp2Csharp(data, createGameServerResult);
					railEventCallBackHandler(event_id, createGameServerResult);
					break;
				}
				case RAILEventID.kRailEventNetChannelInviteMemmberResult:
				{
					InviteMemmberResult inviteMemmberResult = new InviteMemmberResult();
					RailConverter.Cpp2Csharp(data, inviteMemmberResult);
					railEventCallBackHandler(event_id, inviteMemmberResult);
					break;
				}
				case RAILEventID.kRailEventVoiceChannelRemoveUsersResult:
				{
					VoiceChannelRemoveUsersResult voiceChannelRemoveUsersResult = new VoiceChannelRemoveUsersResult();
					RailConverter.Cpp2Csharp(data, voiceChannelRemoveUsersResult);
					railEventCallBackHandler(event_id, voiceChannelRemoveUsersResult);
					break;
				}
				case RAILEventID.kRailEventAchievementPlayerAchievementStored:
				{
					PlayerAchievementStored playerAchievementStored = new PlayerAchievementStored();
					RailConverter.Cpp2Csharp(data, playerAchievementStored);
					railEventCallBackHandler(event_id, playerAchievementStored);
					break;
				}
				case RAILEventID.kRailEventFriendsQueryPlayedWithFriendsGamesResult:
				{
					RailFriendsQueryPlayedWithFriendsGamesResult railFriendsQueryPlayedWithFriendsGamesResult = new RailFriendsQueryPlayedWithFriendsGamesResult();
					RailConverter.Cpp2Csharp(data, railFriendsQueryPlayedWithFriendsGamesResult);
					railEventCallBackHandler(event_id, railFriendsQueryPlayedWithFriendsGamesResult);
					break;
				}
				case RAILEventID.kRailEventAssetsSplitFinished:
				{
					SplitAssetsFinished splitAssetsFinished = new SplitAssetsFinished();
					RailConverter.Cpp2Csharp(data, splitAssetsFinished);
					railEventCallBackHandler(event_id, splitAssetsFinished);
					break;
				}
				case RAILEventID.kRailEventLeaderboardReceived:
				{
					LeaderboardReceived leaderboardReceived = new LeaderboardReceived();
					RailConverter.Cpp2Csharp(data, leaderboardReceived);
					railEventCallBackHandler(event_id, leaderboardReceived);
					break;
				}
				case RAILEventID.kRailEventBrowserTryNavigateNewPageRequest:
				{
					BrowserTryNavigateNewPageRequest browserTryNavigateNewPageRequest = new BrowserTryNavigateNewPageRequest();
					RailConverter.Cpp2Csharp(data, browserTryNavigateNewPageRequest);
					railEventCallBackHandler(event_id, browserTryNavigateNewPageRequest);
					break;
				}
				case RAILEventID.kRailEventUsersShowChatWindowWithFriendResult:
				{
					RailShowChatWindowWithFriendResult railShowChatWindowWithFriendResult = new RailShowChatWindowWithFriendResult();
					RailConverter.Cpp2Csharp(data, railShowChatWindowWithFriendResult);
					railEventCallBackHandler(event_id, railShowChatWindowWithFriendResult);
					break;
				}
				case RAILEventID.kRailEventLeaderboardAttachSpaceWork:
				{
					LeaderboardAttachSpaceWork leaderboardAttachSpaceWork = new LeaderboardAttachSpaceWork();
					RailConverter.Cpp2Csharp(data, leaderboardAttachSpaceWork);
					railEventCallBackHandler(event_id, leaderboardAttachSpaceWork);
					break;
				}
				case RAILEventID.kRailEventLeaderboardAsyncCreated:
				{
					LeaderboardCreated leaderboardCreated = new LeaderboardCreated();
					RailConverter.Cpp2Csharp(data, leaderboardCreated);
					railEventCallBackHandler(event_id, leaderboardCreated);
					break;
				}
				case RAILEventID.kRailEventFriendsQueryPlayedWithFriendsListResult:
				{
					RailFriendsQueryPlayedWithFriendsListResult railFriendsQueryPlayedWithFriendsListResult = new RailFriendsQueryPlayedWithFriendsListResult();
					RailConverter.Cpp2Csharp(data, railFriendsQueryPlayedWithFriendsListResult);
					railEventCallBackHandler(event_id, railFriendsQueryPlayedWithFriendsListResult);
					break;
				}
				case RAILEventID.kRailEventUsersNotifyInviter:
				{
					RailUsersNotifyInviter railUsersNotifyInviter = new RailUsersNotifyInviter();
					RailConverter.Cpp2Csharp(data, railUsersNotifyInviter);
					railEventCallBackHandler(event_id, railUsersNotifyInviter);
					break;
				}
				case RAILEventID.kRailEventDlcInstallProgress:
				{
					DlcInstallProgress dlcInstallProgress = new DlcInstallProgress();
					RailConverter.Cpp2Csharp(data, dlcInstallProgress);
					railEventCallBackHandler(event_id, dlcInstallProgress);
					break;
				}
				case RAILEventID.kRailPlatformNotifyEventJoinGameByUser:
				{
					RailPlatformNotifyEventJoinGameByUser railPlatformNotifyEventJoinGameByUser = new RailPlatformNotifyEventJoinGameByUser();
					RailConverter.Cpp2Csharp(data, railPlatformNotifyEventJoinGameByUser);
					railEventCallBackHandler(event_id, railPlatformNotifyEventJoinGameByUser);
					break;
				}
				case RAILEventID.kRailEventDlcInstallFinished:
				{
					DlcInstallFinished dlcInstallFinished = new DlcInstallFinished();
					RailConverter.Cpp2Csharp(data, dlcInstallFinished);
					railEventCallBackHandler(event_id, dlcInstallFinished);
					break;
				}
				case RAILEventID.kRailEventNetworkCreateSessionRequest:
				{
					CreateSessionRequest createSessionRequest = new CreateSessionRequest();
					RailConverter.Cpp2Csharp(data, createSessionRequest);
					railEventCallBackHandler(event_id, createSessionRequest);
					break;
				}
				case RAILEventID.kRailEventBrowserCloseResult:
				{
					CloseBrowserResult closeBrowserResult = new CloseBrowserResult();
					RailConverter.Cpp2Csharp(data, closeBrowserResult);
					railEventCallBackHandler(event_id, closeBrowserResult);
					break;
				}
				case RAILEventID.kRailEventRoomSetRoomMetadataResult:
				{
					SetRoomMetadataInfo setRoomMetadataInfo = new SetRoomMetadataInfo();
					RailConverter.Cpp2Csharp(data, setRoomMetadataInfo);
					railEventCallBackHandler(event_id, setRoomMetadataInfo);
					break;
				}
				case RAILEventID.kRailEventGameServerFavoriteGameServers:
				{
					AsyncGetFavoriteGameServersResult asyncGetFavoriteGameServersResult = new AsyncGetFavoriteGameServersResult();
					RailConverter.Cpp2Csharp(data, asyncGetFavoriteGameServersResult);
					railEventCallBackHandler(event_id, asyncGetFavoriteGameServersResult);
					break;
				}
				case RAILEventID.kRailEventInGamePurchasePurchaseProductsResult:
				{
					RailInGamePurchasePurchaseProductsResponse railInGamePurchasePurchaseProductsResponse = new RailInGamePurchasePurchaseProductsResponse();
					RailConverter.Cpp2Csharp(data, railInGamePurchasePurchaseProductsResponse);
					railEventCallBackHandler(event_id, railInGamePurchasePurchaseProductsResponse);
					break;
				}
				case RAILEventID.kRailEventAssetsStartConsumeFinished:
				{
					StartConsumeAssetsFinished startConsumeAssetsFinished = new StartConsumeAssetsFinished();
					RailConverter.Cpp2Csharp(data, startConsumeAssetsFinished);
					railEventCallBackHandler(event_id, startConsumeAssetsFinished);
					break;
				}
				case RAILEventID.kRailEventRoomNotifyMemberChanged:
				{
					NotifyRoomMemberChange notifyRoomMemberChange = new NotifyRoomMemberChange();
					RailConverter.Cpp2Csharp(data, notifyRoomMemberChange);
					railEventCallBackHandler(event_id, notifyRoomMemberChange);
					break;
				}
				case RAILEventID.kRailEventUserSpaceSubscribeResult:
				{
					AsyncSubscribeSpaceWorksResult asyncSubscribeSpaceWorksResult = new AsyncSubscribeSpaceWorksResult();
					RailConverter.Cpp2Csharp(data, asyncSubscribeSpaceWorksResult);
					railEventCallBackHandler(event_id, asyncSubscribeSpaceWorksResult);
					break;
				}
				case RAILEventID.kRailEventAssetsMergeToFinished:
				{
					MergeAssetsToFinished mergeAssetsToFinished = new MergeAssetsToFinished();
					RailConverter.Cpp2Csharp(data, mergeAssetsToFinished);
					railEventCallBackHandler(event_id, mergeAssetsToFinished);
					break;
				}
				case RAILEventID.kRailEventAssetsRequestAllAssetsFinished:
				{
					RequestAllAssetsFinished requestAllAssetsFinished = new RequestAllAssetsFinished();
					RailConverter.Cpp2Csharp(data, requestAllAssetsFinished);
					railEventCallBackHandler(event_id, requestAllAssetsFinished);
					break;
				}
				}
			}
		}
	}
}
