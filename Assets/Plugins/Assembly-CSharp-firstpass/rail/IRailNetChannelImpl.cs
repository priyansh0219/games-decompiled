using System;
using System.Collections.Generic;

namespace rail
{
	public class IRailNetChannelImpl : RailObject, IRailNetChannel
	{
		internal IRailNetChannelImpl(IntPtr cPtr)
		{
			swigCPtr_ = cPtr;
		}

		~IRailNetChannelImpl()
		{
		}

		public virtual RailResult AsyncCreateChannel(RailID local_peer, string user_data)
		{
			IntPtr intPtr = ((local_peer == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailID__SWIG_0());
			if (local_peer != null)
			{
				RailConverter.Csharp2Cpp(local_peer, intPtr);
			}
			try
			{
				return (RailResult)RAIL_API_PINVOKE.IRailNetChannel_AsyncCreateChannel(swigCPtr_, intPtr, user_data);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_RailID(intPtr);
			}
		}

		public virtual RailResult AsyncJoinChannel(RailID local_peer, ulong channel_id, string user_data)
		{
			IntPtr intPtr = ((local_peer == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailID__SWIG_0());
			if (local_peer != null)
			{
				RailConverter.Csharp2Cpp(local_peer, intPtr);
			}
			try
			{
				return (RailResult)RAIL_API_PINVOKE.IRailNetChannel_AsyncJoinChannel(swigCPtr_, intPtr, channel_id, user_data);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_RailID(intPtr);
			}
		}

		public virtual RailResult AsyncInviteMemberToChannel(RailID local_peer, ulong channel_id, List<RailID> remote_peers, string user_data)
		{
			IntPtr intPtr = ((local_peer == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailID__SWIG_0());
			if (local_peer != null)
			{
				RailConverter.Csharp2Cpp(local_peer, intPtr);
			}
			IntPtr intPtr2 = ((remote_peers == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailArrayRailID__SWIG_0());
			if (remote_peers != null)
			{
				RailConverter.Csharp2Cpp(remote_peers, intPtr2);
			}
			try
			{
				return (RailResult)RAIL_API_PINVOKE.IRailNetChannel_AsyncInviteMemberToChannel(swigCPtr_, intPtr, channel_id, intPtr2, user_data);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_RailID(intPtr);
				RAIL_API_PINVOKE.delete_RailArrayRailID(intPtr2);
			}
		}

		public virtual RailResult GetAllMembers(RailID local_peer, ulong channel_id, List<RailID> remote_peers)
		{
			IntPtr intPtr = ((local_peer == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailID__SWIG_0());
			if (local_peer != null)
			{
				RailConverter.Csharp2Cpp(local_peer, intPtr);
			}
			IntPtr intPtr2 = ((remote_peers == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailArrayRailID__SWIG_0());
			try
			{
				return (RailResult)RAIL_API_PINVOKE.IRailNetChannel_GetAllMembers(swigCPtr_, intPtr, channel_id, intPtr2);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_RailID(intPtr);
				if (remote_peers != null)
				{
					RailConverter.Cpp2Csharp(intPtr2, remote_peers);
				}
				RAIL_API_PINVOKE.delete_RailArrayRailID(intPtr2);
			}
		}

		public virtual RailResult SendDataToChannel(RailID local_peer, ulong channel_id, byte[] data_buf, uint data_len, uint message_type)
		{
			IntPtr intPtr = ((local_peer == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailID__SWIG_0());
			if (local_peer != null)
			{
				RailConverter.Csharp2Cpp(local_peer, intPtr);
			}
			try
			{
				return (RailResult)RAIL_API_PINVOKE.IRailNetChannel_SendDataToChannel__SWIG_0(swigCPtr_, intPtr, channel_id, data_buf, data_len, message_type);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_RailID(intPtr);
			}
		}

		public virtual RailResult SendDataToChannel(RailID local_peer, ulong channel_id, byte[] data_buf, uint data_len)
		{
			IntPtr intPtr = ((local_peer == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailID__SWIG_0());
			if (local_peer != null)
			{
				RailConverter.Csharp2Cpp(local_peer, intPtr);
			}
			try
			{
				return (RailResult)RAIL_API_PINVOKE.IRailNetChannel_SendDataToChannel__SWIG_1(swigCPtr_, intPtr, channel_id, data_buf, data_len);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_RailID(intPtr);
			}
		}

		public virtual RailResult SendDataToMember(RailID local_peer, ulong channel_id, RailID remote_peer, byte[] data_buf, uint data_len, uint message_type)
		{
			IntPtr intPtr = ((local_peer == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailID__SWIG_0());
			if (local_peer != null)
			{
				RailConverter.Csharp2Cpp(local_peer, intPtr);
			}
			IntPtr intPtr2 = ((remote_peer == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailID__SWIG_0());
			if (remote_peer != null)
			{
				RailConverter.Csharp2Cpp(remote_peer, intPtr2);
			}
			try
			{
				return (RailResult)RAIL_API_PINVOKE.IRailNetChannel_SendDataToMember__SWIG_0(swigCPtr_, intPtr, channel_id, intPtr2, data_buf, data_len, message_type);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_RailID(intPtr);
				RAIL_API_PINVOKE.delete_RailID(intPtr2);
			}
		}

		public virtual RailResult SendDataToMember(RailID local_peer, ulong channel_id, RailID remote_peer, byte[] data_buf, uint data_len)
		{
			IntPtr intPtr = ((local_peer == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailID__SWIG_0());
			if (local_peer != null)
			{
				RailConverter.Csharp2Cpp(local_peer, intPtr);
			}
			IntPtr intPtr2 = ((remote_peer == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailID__SWIG_0());
			if (remote_peer != null)
			{
				RailConverter.Csharp2Cpp(remote_peer, intPtr2);
			}
			try
			{
				return (RailResult)RAIL_API_PINVOKE.IRailNetChannel_SendDataToMember__SWIG_1(swigCPtr_, intPtr, channel_id, intPtr2, data_buf, data_len);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_RailID(intPtr);
				RAIL_API_PINVOKE.delete_RailID(intPtr2);
			}
		}

		public virtual bool IsDataReady(RailID local_peer, out ulong channel_id, out uint data_len, out uint message_type)
		{
			IntPtr intPtr = ((local_peer == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailID__SWIG_0());
			try
			{
				return RAIL_API_PINVOKE.IRailNetChannel_IsDataReady__SWIG_0(swigCPtr_, intPtr, out channel_id, out data_len, out message_type);
			}
			finally
			{
				if (local_peer != null)
				{
					RailConverter.Cpp2Csharp(intPtr, local_peer);
				}
				RAIL_API_PINVOKE.delete_RailID(intPtr);
			}
		}

		public virtual bool IsDataReady(RailID local_peer, out ulong channel_id, out uint data_len)
		{
			IntPtr intPtr = ((local_peer == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailID__SWIG_0());
			try
			{
				return RAIL_API_PINVOKE.IRailNetChannel_IsDataReady__SWIG_1(swigCPtr_, intPtr, out channel_id, out data_len);
			}
			finally
			{
				if (local_peer != null)
				{
					RailConverter.Cpp2Csharp(intPtr, local_peer);
				}
				RAIL_API_PINVOKE.delete_RailID(intPtr);
			}
		}

		public virtual RailResult ReadData(RailID local_peer, ulong channel_id, RailID remote_peer, byte[] data_buf, uint data_len, uint message_type)
		{
			IntPtr intPtr = ((local_peer == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailID__SWIG_0());
			if (local_peer != null)
			{
				RailConverter.Csharp2Cpp(local_peer, intPtr);
			}
			IntPtr intPtr2 = ((remote_peer == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailID__SWIG_0());
			try
			{
				return (RailResult)RAIL_API_PINVOKE.IRailNetChannel_ReadData__SWIG_0(swigCPtr_, intPtr, channel_id, intPtr2, data_buf, data_len, message_type);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_RailID(intPtr);
				if (remote_peer != null)
				{
					RailConverter.Cpp2Csharp(intPtr2, remote_peer);
				}
				RAIL_API_PINVOKE.delete_RailID(intPtr2);
			}
		}

		public virtual RailResult ReadData(RailID local_peer, ulong channel_id, RailID remote_peer, byte[] data_buf, uint data_len)
		{
			IntPtr intPtr = ((local_peer == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailID__SWIG_0());
			if (local_peer != null)
			{
				RailConverter.Csharp2Cpp(local_peer, intPtr);
			}
			IntPtr intPtr2 = ((remote_peer == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailID__SWIG_0());
			try
			{
				return (RailResult)RAIL_API_PINVOKE.IRailNetChannel_ReadData__SWIG_1(swigCPtr_, intPtr, channel_id, intPtr2, data_buf, data_len);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_RailID(intPtr);
				if (remote_peer != null)
				{
					RailConverter.Cpp2Csharp(intPtr2, remote_peer);
				}
				RAIL_API_PINVOKE.delete_RailID(intPtr2);
			}
		}

		public virtual RailResult BlockMessageType(RailID local_peer, ulong channel_id, uint message_type)
		{
			IntPtr intPtr = ((local_peer == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailID__SWIG_0());
			if (local_peer != null)
			{
				RailConverter.Csharp2Cpp(local_peer, intPtr);
			}
			try
			{
				return (RailResult)RAIL_API_PINVOKE.IRailNetChannel_BlockMessageType(swigCPtr_, intPtr, channel_id, message_type);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_RailID(intPtr);
			}
		}

		public virtual RailResult UnblockMessageType(RailID local_peer, ulong channel_id, uint message_type)
		{
			IntPtr intPtr = ((local_peer == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailID__SWIG_0());
			if (local_peer != null)
			{
				RailConverter.Csharp2Cpp(local_peer, intPtr);
			}
			try
			{
				return (RailResult)RAIL_API_PINVOKE.IRailNetChannel_UnblockMessageType(swigCPtr_, intPtr, channel_id, message_type);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_RailID(intPtr);
			}
		}

		public virtual RailResult ExitChannel(RailID local_peer, ulong channel_id)
		{
			IntPtr intPtr = ((local_peer == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailID__SWIG_0());
			if (local_peer != null)
			{
				RailConverter.Csharp2Cpp(local_peer, intPtr);
			}
			try
			{
				return (RailResult)RAIL_API_PINVOKE.IRailNetChannel_ExitChannel(swigCPtr_, intPtr, channel_id);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_RailID(intPtr);
			}
		}
	}
}
