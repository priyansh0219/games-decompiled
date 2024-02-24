using System;

namespace rail
{
	public class IRailIMEHelperImpl : RailObject, IRailIMEHelper
	{
		internal IRailIMEHelperImpl(IntPtr cPtr)
		{
			swigCPtr_ = cPtr;
		}

		~IRailIMEHelperImpl()
		{
		}

		public virtual RailResult EnableIMEHelperTextInputWindow(bool enable, RailWindowPosition position)
		{
			IntPtr intPtr = ((position == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailWindowPosition__SWIG_0());
			if (position != null)
			{
				RailConverter.Csharp2Cpp(position, intPtr);
			}
			try
			{
				return (RailResult)RAIL_API_PINVOKE.IRailIMEHelper_EnableIMEHelperTextInputWindow(swigCPtr_, enable, intPtr);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_RailWindowPosition(intPtr);
			}
		}

		public virtual RailResult UpdateIMEHelperTextInputWindowPosition(RailWindowPosition position)
		{
			IntPtr intPtr = ((position == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailWindowPosition__SWIG_0());
			if (position != null)
			{
				RailConverter.Csharp2Cpp(position, intPtr);
			}
			try
			{
				return (RailResult)RAIL_API_PINVOKE.IRailIMEHelper_UpdateIMEHelperTextInputWindowPosition(swigCPtr_, intPtr);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_RailWindowPosition(intPtr);
			}
		}
	}
}
