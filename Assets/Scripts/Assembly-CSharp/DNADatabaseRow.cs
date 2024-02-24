public class DNADatabaseRow
{
	public string behavior;

	public string sampleName;

	public string gene;

	public string color;

	public string serumName;

	public string description;

	public string material;

	public string playerMaterial;

	public string transfuserMaterial;

	public void CopyFrom(DNADatabaseRow from)
	{
		behavior = from.behavior;
		sampleName = from.sampleName;
		gene = from.gene;
		color = from.color;
		serumName = from.serumName;
		description = from.description;
		material = from.material;
		playerMaterial = from.playerMaterial;
		transfuserMaterial = from.transfuserMaterial;
	}

	public override string ToString()
	{
		return "Behavior: " + behavior + " sampleName: " + sampleName + " gene: " + gene + " color: " + color + " serumName: " + serumName + " description: " + description + " material: " + material + " playerMaterial: " + playerMaterial + " transfuserMaterial: " + transfuserMaterial;
	}
}
