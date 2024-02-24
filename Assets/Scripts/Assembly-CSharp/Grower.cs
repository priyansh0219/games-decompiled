using ProtoBuf;

[ProtoContract]
public class Grower : Creature
{
	public float growInterval;

	public float growSize;

	public float growAmount;

	public float pulseAmount;

	public float pulseTime;

	public override void Start()
	{
		base.Start();
		InvokeRepeating("Pulse", 0f, growInterval);
	}

	private void Pulse()
	{
		if (base.gameObject.activeInHierarchy)
		{
			AlterTerrain();
		}
	}

	private void AlterTerrain()
	{
		float x = base.transform.localScale.x;
		TerrainChanger.AlterTerrain(base.transform.position, (int)(growSize * x), growAmount * x);
	}
}
