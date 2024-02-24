using System.Collections.Generic;

public static class BeaconManager
{
	public static bool isDirty = false;

	private static HashSet<Beacon> beacons = new HashSet<Beacon>();

	public static void Add(Beacon beacon)
	{
		if (!beacons.Contains(beacon))
		{
			beacons.Add(beacon);
			isDirty = true;
		}
	}

	public static void Remove(Beacon beacon)
	{
		if (beacons.Remove(beacon))
		{
			isDirty = true;
		}
	}

	public static HashSet<Beacon>.Enumerator GetEnumerator()
	{
		return beacons.GetEnumerator();
	}

	public static int GetCount()
	{
		return beacons.Count;
	}
}
