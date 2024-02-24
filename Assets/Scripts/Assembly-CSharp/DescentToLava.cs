using UnityEngine;

public class DescentToLava : CreatureAction
{
	public float scanForLavaInterval = 0.5f;

	[AssertNotNull]
	public LavaDatabase lavaDatabase;

	[AssertNotNull]
	public LavaShell lavaShell;

	[AssertNotNull]
	public ConstantForce descendForce;

	private bool isAboveLava;

	private void Start()
	{
		InvokeRepeating("ScanForLava", 0f, scanForLavaInterval);
	}

	public override float Evaluate(Creature creature, float time)
	{
		if (isAboveLava)
		{
			return Mathf.Lerp(GetEvaluatePriority(), 0f, lavaShell.GetArmorFraction());
		}
		return 0f;
	}

	public override void StartPerform(Creature creature, float time)
	{
		descendForce.enabled = true;
		descendForce.force = new Vector3(0f, -10f, 0f);
	}

	public override void StopPerform(Creature creature, float time)
	{
		descendForce.enabled = false;
	}

	private void ScanForLava()
	{
		isAboveLava = false;
		if (Physics.Raycast(base.transform.position, Vector3.down, out var hitInfo, Voxeland.GetTerrainLayerMask()))
		{
			isAboveLava = lavaDatabase.IsLava(hitInfo.point, hitInfo.normal);
		}
	}
}
