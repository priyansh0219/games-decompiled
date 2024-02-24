public class BlockTypeClassification
{
	public int blockType;

	public float minInclination;

	public float maxInclination;

	public string material;

	public BlockTypeClassification()
	{
	}

	public BlockTypeClassification(int blockType, float minInclination, float maxInclination, string material)
	{
		this.blockType = blockType;
		this.minInclination = minInclination;
		this.maxInclination = maxInclination;
		this.material = material;
	}

	public bool ContainsInclination(float inclination)
	{
		if (inclination >= minInclination)
		{
			return inclination <= maxInclination;
		}
		return false;
	}
}
