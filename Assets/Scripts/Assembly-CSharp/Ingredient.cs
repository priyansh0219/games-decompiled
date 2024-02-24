public class Ingredient
{
	private TechType _techType;

	private int _amount;

	public TechType techType => _techType;

	public int amount => _amount;

	public Ingredient(TechType techType, int amount)
	{
		_techType = techType;
		_amount = amount;
	}
}
