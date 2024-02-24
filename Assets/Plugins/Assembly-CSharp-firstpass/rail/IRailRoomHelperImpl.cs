using System;

namespace rail
{
	public class IRailRoomHelperImpl : RailObject, IRailRoomHelper
	{
		internal IRailRoomHelperImpl(IntPtr cPtr)
		{
			swigCPtr_ = cPtr;
		}

		~IRailRoomHelperImpl()
		{
		}

		public virtual void set_current_zone_id(ulong zone_id)
		{
			RAIL_API_PINVOKE.IRailRoomHelper_set_current_zone_id(swigCPtr_, zone_id);
		}

		public virtual ulong get_current_zone_id()
		{
			return RAIL_API_PINVOKE.IRailRoomHelper_get_current_zone_id(swigCPtr_);
		}

		public virtual IRailRoom CreateRoom(RoomOptions options, string room_name, out int result)
		{
			IntPtr intPtr = ((options == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RoomOptions__SWIG_0(0uL));
			if (options != null)
			{
				RailConverter.Csharp2Cpp(options, intPtr);
			}
			try
			{
				IntPtr intPtr2 = RAIL_API_PINVOKE.IRailRoomHelper_CreateRoom(swigCPtr_, intPtr, room_name, out result);
				return (intPtr2 == IntPtr.Zero) ? null : new IRailRoomImpl(intPtr2);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_RoomOptions(intPtr);
			}
		}

		public virtual IRailRoom AsyncCreateRoom(RoomOptions options, string room_name, string user_data)
		{
			IntPtr intPtr = ((options == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RoomOptions__SWIG_0(0uL));
			if (options != null)
			{
				RailConverter.Csharp2Cpp(options, intPtr);
			}
			try
			{
				IntPtr intPtr2 = RAIL_API_PINVOKE.IRailRoomHelper_AsyncCreateRoom(swigCPtr_, intPtr, room_name, user_data);
				return (intPtr2 == IntPtr.Zero) ? null : new IRailRoomImpl(intPtr2);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_RoomOptions(intPtr);
			}
		}

		public virtual IRailRoom OpenRoom(ulong zone_id, ulong room_id, out int result)
		{
			IntPtr intPtr = RAIL_API_PINVOKE.IRailRoomHelper_OpenRoom(swigCPtr_, zone_id, room_id, out result);
			if (!(intPtr == IntPtr.Zero))
			{
				return new IRailRoomImpl(intPtr);
			}
			return null;
		}

		public virtual RailResult AsyncGetUserRoomList(string user_data)
		{
			return (RailResult)RAIL_API_PINVOKE.IRailRoomHelper_AsyncGetUserRoomList(swigCPtr_, user_data);
		}
	}
}
