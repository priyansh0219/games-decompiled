using System.Collections.Generic;

namespace rail
{
	public interface IRailVoiceChannel : IRailComponent
	{
		RailVoiceChannelID GetVoiceChannelID();

		string GetVoiceChannelName();

		EnumRailVoiceChannelJoinState GetJoinState();

		RailResult AsyncJoinVoiceChannel(string user_data);

		RailResult AsyncLeaveVoiceChannel(string user_data);

		RailResult GetUsers(List<RailID> user_list);

		RailResult AsyncAddUsers(List<RailID> user_list, string user_data);

		RailResult AsyncRemoveUsers(List<RailID> user_list, string user_data);

		RailResult CloseChannel();

		RailResult SetMicrophoneEnableState(bool could_speak);

		bool GetMicrophoneEnableState();

		RailResult AsyncSetUsersSpeakingState(List<RailVoiceChannelUserSpeakingState> users_speaking_state, string user_data);

		RailResult GetUsersSpeakingState(List<RailVoiceChannelUserSpeakingState> users_speaking_state);
	}
}
