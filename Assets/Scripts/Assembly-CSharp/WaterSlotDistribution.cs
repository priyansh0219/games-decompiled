public class WaterSlotDistribution
{
	public string biome;

	public float probability;

	public float minDepth;

	public float maxDepth;

	public float terrainDistance;

	public float propDistance;

	public float slotDistance;

	public string slotFilename;

	public WaterSlotDistribution()
	{
	}

	public WaterSlotDistribution(string biome, float probability, float minDepth, float maxDepth, float propDistance, float terrainDistance, float slotDistance, string slotFilename)
	{
		this.biome = biome;
		this.probability = probability;
		this.minDepth = minDepth;
		this.maxDepth = maxDepth;
		this.terrainDistance = terrainDistance;
		this.propDistance = propDistance;
		this.slotDistance = slotDistance;
		this.slotFilename = slotFilename;
	}
}
