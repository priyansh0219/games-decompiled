using System.Collections.Generic;

namespace rail
{
	public class RailSpaceWorkDescriptor
	{
		public List<RailSpaceWorkVoteDetail> vote_details = new List<RailSpaceWorkVoteDetail>();

		public string description;

		public string preview_url;

		public SpaceWorkID id = new SpaceWorkID();

		public string detail_url;

		public List<RailID> uploader_ids = new List<RailID>();

		public string name;
	}
}
