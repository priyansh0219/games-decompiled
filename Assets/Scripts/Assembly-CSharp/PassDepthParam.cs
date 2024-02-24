public class PassDepthParam : PassSoundParam
{
	public override string GetParamName()
	{
		return "depth";
	}

	public override float GetParamValue()
	{
		return Utils.GetLocalPlayerComp().transform.position.y;
	}
}
