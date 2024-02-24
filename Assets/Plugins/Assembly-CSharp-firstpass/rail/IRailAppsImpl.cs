using System;

namespace rail
{
	public class IRailAppsImpl : RailObject, IRailApps
	{
		internal IRailAppsImpl(IntPtr cPtr)
		{
			swigCPtr_ = cPtr;
		}

		~IRailAppsImpl()
		{
		}

		public virtual bool IsGameInstalled(ulong game_id)
		{
			return RAIL_API_PINVOKE.IRailApps_IsGameInstalled(swigCPtr_, game_id);
		}

		public virtual RailResult AsyncQuerySubscribeWishPlayState(ulong game_id, string user_data)
		{
			return (RailResult)RAIL_API_PINVOKE.IRailApps_AsyncQuerySubscribeWishPlayState(swigCPtr_, game_id, user_data);
		}
	}
}
