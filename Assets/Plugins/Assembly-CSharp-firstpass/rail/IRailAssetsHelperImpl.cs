using System;

namespace rail
{
	public class IRailAssetsHelperImpl : RailObject, IRailAssetsHelper
	{
		internal IRailAssetsHelperImpl(IntPtr cPtr)
		{
			swigCPtr_ = cPtr;
		}

		~IRailAssetsHelperImpl()
		{
		}

		public virtual IRailAssets OpenAssets()
		{
			IntPtr intPtr = RAIL_API_PINVOKE.IRailAssetsHelper_OpenAssets(swigCPtr_);
			if (!(intPtr == IntPtr.Zero))
			{
				return new IRailAssetsImpl(intPtr);
			}
			return null;
		}
	}
}
