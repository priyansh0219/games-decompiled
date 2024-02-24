using UnityEngine;

public static class LavaDatabaseUtils
{
	public static bool IsLava(this LavaDatabase database, Vector3 position, Vector3 normal)
	{
		try
		{
			LargeWorldStreamer main = LargeWorldStreamer.main;
			if (main == null)
			{
				return false;
			}
			byte b = 0;
			for (int i = 0; i < 4; i++)
			{
				b = main.GetBlockType(position);
				if (b != 0)
				{
					break;
				}
				position -= normal * 0.5f;
			}
			if (b == 0)
			{
				return false;
			}
			float inclination = Vector3.Angle(normal, Vector3.up);
			return database.IsLava(b, inclination);
		}
		finally
		{
		}
	}
}
