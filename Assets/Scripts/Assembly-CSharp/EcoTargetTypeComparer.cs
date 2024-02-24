using System.Collections.Generic;

public class EcoTargetTypeComparer : IEqualityComparer<EcoTargetType>
{
	public bool Equals(EcoTargetType x, EcoTargetType y)
	{
		int num = (int)x;
		return num.Equals((int)y);
	}

	public int GetHashCode(EcoTargetType obj)
	{
		return (int)obj;
	}
}
