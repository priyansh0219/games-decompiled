using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class Hoverfish : Creature
{
	[AssertNotNull]
	public SwimBehaviour swimBehaviour;

	private float endCuriocityTime;

	private float nextCuriocityTime;

	private bool lookingAtTarget;

	public void OnApproachObject(Collider collider)
	{
		if (!(collider.attachedRigidbody == null) && !collider.attachedRigidbody.isKinematic && !lookingAtTarget && Scared.Value < 0.2f && Time.time > nextCuriocityTime && Random.value < 0.7f)
		{
			endCuriocityTime = Time.time + 5f * (0.5f + Random.value);
			nextCuriocityTime = endCuriocityTime + 5f * (0.5f + Random.value);
			swimBehaviour.LookAt(collider.gameObject.transform);
			lookingAtTarget = true;
		}
	}

	public void Update()
	{
		if (lookingAtTarget && (Time.time > endCuriocityTime || Scared.Value > 0.2f))
		{
			lookingAtTarget = false;
			swimBehaviour.LookForward();
		}
	}
}
