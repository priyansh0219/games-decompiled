using System;
using System.Collections.Generic;

namespace rail
{
	public class IRailZoneHelperImpl : RailObject, IRailZoneHelper
	{
		internal IRailZoneHelperImpl(IntPtr cPtr)
		{
			swigCPtr_ = cPtr;
		}

		~IRailZoneHelperImpl()
		{
		}

		public virtual RailResult AsyncGetZoneList(string user_data)
		{
			return (RailResult)RAIL_API_PINVOKE.IRailZoneHelper_AsyncGetZoneList(swigCPtr_, user_data);
		}

		public virtual RailResult AsyncGetRoomListInZone(ulong zone_id, uint start_index, uint end_index, List<RoomInfoListSorter> sorter, List<RoomInfoListFilter> filter, string user_data)
		{
			IntPtr intPtr = ((sorter == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailArrayRoomInfoListSorter__SWIG_0());
			if (sorter != null)
			{
				RailConverter.Csharp2Cpp(sorter, intPtr);
			}
			IntPtr intPtr2 = ((filter == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailArrayRoomInfoListFilter__SWIG_0());
			if (filter != null)
			{
				RailConverter.Csharp2Cpp(filter, intPtr2);
			}
			try
			{
				return (RailResult)RAIL_API_PINVOKE.IRailZoneHelper_AsyncGetRoomListInZone(swigCPtr_, zone_id, start_index, end_index, intPtr, intPtr2, user_data);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_RailArrayRoomInfoListSorter(intPtr);
				RAIL_API_PINVOKE.delete_RailArrayRoomInfoListFilter(intPtr2);
			}
		}
	}
}
