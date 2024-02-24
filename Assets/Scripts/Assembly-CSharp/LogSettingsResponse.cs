using System;
using Gendarme;

[Serializable]
[SuppressMessage("Subnautica.Rules", "AvoidDoubleInitializationRule")]
public class LogSettingsResponse
{
	public int session_log_resolution = 300;

	public int statistics_period;

	public bool[] category_settings = new bool[5];
}
