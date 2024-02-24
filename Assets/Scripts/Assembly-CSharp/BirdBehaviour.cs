using ProtoBuf;
using UnityEngine;

[ProtoContract]
[ProtoInclude(4210, typeof(Skyray))]
public class BirdBehaviour : Creature, IDrownableCreature
{
	[AssertNotNull]
	public WorldForces worldForces;

	private Vector3 roostPosition;

	public bool drowning { get; set; }

	protected override void InitializeOnce()
	{
		base.InitializeOnce();
		roostPosition = base.transform.position;
	}

	protected override void InitializeAgain()
	{
		base.InitializeAgain();
		roostPosition = leashPosition - Vector3.up * 20f;
	}

	public void Update()
	{
		AllowCreatureUpdates(!drowning);
		leashPosition = roostPosition + Vector3.up * 20f;
	}

	public override void OnKill()
	{
		base.OnKill();
		worldForces.aboveWaterGravity = 9.81f;
	}
}
