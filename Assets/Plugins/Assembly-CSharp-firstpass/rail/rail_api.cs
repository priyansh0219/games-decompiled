using System;

namespace rail
{
	public class rail_api
	{
		public static readonly int USE_MANUAL_ALLOC = RAIL_API_PINVOKE.USE_MANUAL_ALLOC_get();

		public static int RAIL_DEFAULT_MAX_ROOM_MEMBERS => RAIL_API_PINVOKE.RAIL_DEFAULT_MAX_ROOM_MEMBERS_get();

		public static uint kRailMaxGameDefinePlayingStateValue => RAIL_API_PINVOKE.kRailMaxGameDefinePlayingStateValue_get();

		public static bool RailNeedRestartAppForCheckingEnvironment(RailGameID game_id, int argc, string[] argv)
		{
			IntPtr intPtr = ((game_id == null) ? IntPtr.Zero : RAIL_API_PINVOKE.new_RailGameID__SWIG_0());
			if (game_id != null)
			{
				RailConverter.Csharp2Cpp(game_id, intPtr);
			}
			try
			{
				return RAIL_API_PINVOKE.RailNeedRestartAppForCheckingEnvironment(intPtr, argc, argv);
			}
			finally
			{
				RAIL_API_PINVOKE.delete_RailGameID(intPtr);
			}
		}

		public static bool RailInitialize()
		{
			return RAIL_API_PINVOKE.RailInitialize();
		}

		public static void RailFinalize()
		{
			RAIL_API_PINVOKE.RailFinalize();
		}

		public static void RailFireEvents()
		{
			RAIL_API_PINVOKE.RailFireEvents();
		}

		public static IRailFactory RailFactory()
		{
			IntPtr intPtr = RAIL_API_PINVOKE.RailFactory();
			if (!(intPtr == IntPtr.Zero))
			{
				return new IRailFactoryImpl(intPtr);
			}
			return null;
		}

		public static void RailGetSdkVersion(out string version, out string description)
		{
			IntPtr jarg = RAIL_API_PINVOKE.new_RailString__SWIG_0();
			IntPtr intPtr = RAIL_API_PINVOKE.new_RailString__SWIG_0();
			try
			{
				RAIL_API_PINVOKE.RailGetSdkVersion(jarg, intPtr);
			}
			finally
			{
				version = RAIL_API_PINVOKE.RailString_c_str(jarg);
				RAIL_API_PINVOKE.delete_RailString(jarg);
				description = RAIL_API_PINVOKE.RailString_c_str(intPtr);
				RAIL_API_PINVOKE.delete_RailString(intPtr);
			}
		}

		public static void CSharpRailRegisterEvent(RAILEventID event_id, RailEventCallBackFunction callback)
		{
			RAIL_API_PINVOKE.CSharpRailRegisterEvent((int)event_id, callback);
		}

		public static void CSharpRailUnRegisterEvent(RAILEventID event_id, RailEventCallBackFunction callback)
		{
			RAIL_API_PINVOKE.CSharpRailUnRegisterEvent((int)event_id, callback);
		}

		public static void CSharpRailUnRegisterAllEvent()
		{
			RAIL_API_PINVOKE.CSharpRailUnRegisterAllEvent();
		}
	}
}
