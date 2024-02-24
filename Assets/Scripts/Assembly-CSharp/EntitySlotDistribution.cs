public class EntitySlotDistribution
{
	public string biome;

	public string material;

	public float probability;

	public float minInclination;

	public float maxInclination;

	public float propDistance;

	public float slotDistance;

	public float terrainOffset;

	public string slotFilename;

	public EntitySlotDistribution()
	{
	}

	public EntitySlotDistribution(string biome, string material, float probability, float minInclination, float maxInclination, float propDistance, float slotDistance, float terrainOffset, string slotFilename)
	{
		this.biome = biome;
		this.material = material;
		this.probability = probability;
		this.minInclination = minInclination;
		this.maxInclination = maxInclination;
		this.propDistance = propDistance;
		this.slotDistance = slotDistance;
		this.terrainOffset = terrainOffset;
		this.slotFilename = slotFilename;
	}
}
