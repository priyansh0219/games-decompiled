using System;
using System.Diagnostics;

namespace DataPlatform
{
	public class Events
	{
		[Conditional("UNITY_XBOXONE")]
		public static void SendAchievementUnlocked(string UserId, ref Guid PlayerSessionId, string UnlockId)
		{
		}

		[Conditional("UNITY_XBOXONE")]
		public static void SendGameProgress(string UserId, ref Guid PlayerSessionId, float CompletionPercent)
		{
		}

		[Conditional("UNITY_XBOXONE")]
		public static void SendMediaUsage(string AppSessionId, string AppSessionStartDateTime, uint UserIdType, string UserId, string SubscriptionTierType, string SubscriptionTier, string MediaType, string ProviderId, string ProviderMediaId, string ProviderMediaInstanceId, ref Guid BingId, ulong MediaLengthMs, uint MediaControlAction, float PlaybackSpeed, ulong MediaPositionMs, ulong PlaybackDurationMs, string AcquisitionType, string AcquisitionContext, string AcquisitionContextType, string AcquisitionContextId, int PlaybackIsStream, int PlaybackIsTethered, string MarketplaceLocation, string ContentLocale, float TimeZoneOffset, uint ScreenState)
		{
		}

		[Conditional("UNITY_XBOXONE")]
		public static void SendMultiplayerRoundEnd(string UserId, ref Guid RoundId, int SectionId, ref Guid PlayerSessionId, string MultiplayerCorrelationId, int GameplayModeId, int MatchTypeId, int DifficultyLevelId, float TimeInSeconds, int ExitStatusId)
		{
		}

		[Conditional("UNITY_XBOXONE")]
		public static void SendMultiplayerRoundStart(string UserId, ref Guid RoundId, int SectionId, ref Guid PlayerSessionId, string MultiplayerCorrelationId, int GameplayModeId, int MatchTypeId, int DifficultyLevelId)
		{
		}

		[Conditional("UNITY_XBOXONE")]
		public static void SendObjectiveEnd(string UserId, int SectionId, ref Guid PlayerSessionId, string MultiplayerCorrelationId, int GameplayModeId, int DifficultyLevelId, int ObjectiveId, int ExitStatusId)
		{
		}

		[Conditional("UNITY_XBOXONE")]
		public static void SendObjectiveStart(string UserId, int SectionId, ref Guid PlayerSessionId, string MultiplayerCorrelationId, int GameplayModeId, int DifficultyLevelId, int ObjectiveId)
		{
		}

		[Conditional("UNITY_XBOXONE")]
		public static void SendPageAction(string UserId, ref Guid PlayerSessionId, int ActionTypeId, int ActionInputMethodId, string Page, string TemplateId, string DestinationPage, string Content)
		{
		}

		[Conditional("UNITY_XBOXONE")]
		public static void SendPageView(string UserId, ref Guid PlayerSessionId, string Page, string RefererPage, int PageTypeId, string PageTags, string TemplateId, string Content)
		{
		}

		[Conditional("UNITY_XBOXONE")]
		public static void SendPlayerSessionEnd(string UserId, ref Guid PlayerSessionId, string MultiplayerCorrelationId, int GameplayModeId, int DifficultyLevelId, int ExitStatusId)
		{
		}

		[Conditional("UNITY_XBOXONE")]
		public static void SendPlayerSessionPause(string UserId, ref Guid PlayerSessionId, string MultiplayerCorrelationId)
		{
		}

		[Conditional("UNITY_XBOXONE")]
		public static void SendPlayerSessionResume(string UserId, ref Guid PlayerSessionId, string MultiplayerCorrelationId, int GameplayModeId, int DifficultyLevelId)
		{
		}

		[Conditional("UNITY_XBOXONE")]
		public static void SendPlayerSessionStart(string UserId, ref Guid PlayerSessionId, string MultiplayerCorrelationId, int GameplayModeId, int DifficultyLevelId)
		{
		}

		[Conditional("UNITY_XBOXONE")]
		public static void SendSectionEnd(string UserId, int SectionId, ref Guid PlayerSessionId, string MultiplayerCorrelationId, int GameplayModeId, int DifficultyLevelId, int ExitStatusId)
		{
		}

		[Conditional("UNITY_XBOXONE")]
		public static void SendSectionStart(string UserId, int SectionId, ref Guid PlayerSessionId, string MultiplayerCorrelationId, int GameplayModeId, int DifficultyLevelId)
		{
		}

		[Conditional("UNITY_XBOXONE")]
		public static void SendViewOffer(string UserId, ref Guid PlayerSessionId, ref Guid OfferGuid, ref Guid ProductGuid)
		{
		}
	}
}
