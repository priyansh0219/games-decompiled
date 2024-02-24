public class PassSpeedParam : PassSoundParam
{
	public override string GetParamName()
	{
		return "speed";
	}

	public override float GetParamValue()
	{
		return Utils.GetLocalPlayerComp().GetComponent<PlayerController>().velocity.magnitude;
	}
}
