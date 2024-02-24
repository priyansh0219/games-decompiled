using System.Collections.Generic;

namespace rail
{
	public interface IRailRoom : IRailComponent
	{
		ulong GetRoomId();

		RailResult GetRoomName(out string name);

		ulong GetZoneId();

		RailID GetOwnerId();

		RailResult GetHasPassword(out bool has_password);

		EnumRoomType GetRoomType();

		bool SetNewOwner(RailID new_owner_id);

		RailResult AsyncGetRoomMembers(string user_data);

		void Leave();

		RailResult AsyncJoinRoom(string password, string user_data);

		RailResult AsyncGetAllRoomData(string user_data);

		RailResult AsyncKickOffMember(RailID member_id, string user_data);

		bool GetRoomMetadata(string key, out string value);

		bool SetRoomMetadata(string key, string value);

		RailResult AsyncSetRoomMetadata(List<RailKeyValue> key_values, string user_data);

		RailResult AsyncGetRoomMetadata(List<string> keys, string user_data);

		RailResult AsyncClearRoomMetadata(List<string> keys, string user_data);

		bool GetMemberMetadata(RailID member_id, string key, out string value);

		bool SetMemberMetadata(RailID member_id, string key, string value);

		RailResult AsyncGetMemberMetadata(RailID member_id, List<string> keys, string user_data);

		RailResult AsyncSetMemberMetadata(RailID member_id, List<RailKeyValue> key_values, string user_data);

		RailResult SendDataToMember(RailID remote_peer, byte[] data_buf, uint data_len, uint message_type);

		RailResult SendDataToMember(RailID remote_peer, byte[] data_buf, uint data_len);

		uint GetNumOfMembers();

		RailID GetMemberByIndex(uint index);

		RailResult GetMemberNameByIndex(uint index, out string name);

		uint GetMaxMembers();

		bool SetGameServerID(ulong game_server_rail_id);

		bool SetGameServerChannelID(ulong game_server_channel_id);

		bool GetGameServerID(out ulong game_server_rail_id);

		bool GetGameServerChannelID(out ulong game_server_channel_id);

		bool SetRoomJoinable(bool is_joinable);

		bool GetRoomJoinable();

		RailResult GetFriendsInRoom(List<RailID> friend_ids);

		bool IsUserInRoom(RailID user_rail_id);

		RailResult EnableTeamVoice(bool enable);
	}
}
