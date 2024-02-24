using FMOD;
using FMOD.Studio;

public static class FMODCompatibilityExtensions
{
	public static RESULT setParameterValue(this EventInstance evt, string name, float value)
	{
		return evt.setParameterByName(name, value);
	}

	public static RESULT setParameterValueByIndex(this EventInstance evt, PARAMETER_ID index, float value)
	{
		return evt.setParameterByID(index, value);
	}
}
