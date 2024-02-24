public class PlatformServicesArc : PlatformServicesNull
{
	public PlatformServicesArc()
		: base(PlatformServicesNull.DefaultSavePath)
	{
	}

	public override string GetName()
	{
		return "Arc";
	}

	public static bool IsPresent()
	{
		return PlatformServicesUtils.IsRuntimePluginDllPresent("ArcSDK");
	}
}
