using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class BaseRoot : SubRoot
{
	[AssertNotNull]
	public BaseFloodSim flood;

	[AssertNotNull]
	public Base baseComp;

	private const float consumePowerInterval = 1f;

	public override void Start()
	{
		base.Start();
	}

	public override bool IsUnderwater(Vector3 wsPos)
	{
		return flood.IsUnderwater(wsPos);
	}

	public override bool IsLeaking()
	{
		return flood.tIsLeaking();
	}
}
