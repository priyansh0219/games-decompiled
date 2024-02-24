using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class GhostLeviatanVoid : Creature
{
	[AssertNotNull]
	public SwimBehaviour swimBehaviour;

	[AssertNotNull]
	public LastTarget lastTarget;

	public float updateBehaviourRate = 2f;

	public float maxDistanceToPlayer = 180f;

	private bool updateBehaviour = true;

	public override void Start()
	{
		base.Start();
		InvokeRepeating("UpdateVoidBehaviour", 0f, updateBehaviourRate);
	}

	private void UpdateVoidBehaviour()
	{
		Player main = Player.main;
		VoidGhostLeviathansSpawner main2 = VoidGhostLeviathansSpawner.main;
		if (!main || Vector3.Distance(main.transform.position, base.transform.position) > maxDistanceToPlayer)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		bool flag = (updateBehaviour = (bool)main2 && main2.IsPlayerInVoid());
		AllowCreatureUpdates(updateBehaviour);
		if (flag)
		{
			Aggression.Add(1f);
			lastTarget.SetTarget(main.gameObject);
			return;
		}
		Vector3 vector = base.transform.position - main.transform.position;
		Vector3 targetPosition = base.transform.position + vector * maxDistanceToPlayer;
		targetPosition.y = Mathf.Min(targetPosition.y, -50f);
		swimBehaviour.SwimTo(targetPosition, 30f);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		VoidGhostLeviathansSpawner main = VoidGhostLeviathansSpawner.main;
		if ((bool)main)
		{
			main.OnGhostLeviathanDestroyed(base.gameObject);
		}
	}
}
