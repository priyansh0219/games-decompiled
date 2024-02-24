using UnityEngine;

public static class LayerID
{
	public static readonly int Default = LayerMask.NameToLayer("Default");

	public static readonly int Useable = LayerMask.NameToLayer("Useable");

	public static readonly int NotUseable = LayerMask.NameToLayer("NotUseable");

	public static readonly int Player = LayerMask.NameToLayer("Player");

	public static readonly int TerrainCollider = LayerMask.NameToLayer("TerrainCollider");

	public static readonly int UI = LayerMask.NameToLayer("UI");

	public static readonly int Trigger = LayerMask.NameToLayer("Trigger");

	public static readonly int BaseClipProxy = LayerMask.NameToLayer("BaseClipProxy");

	public static readonly int OnlyVehicle = LayerMask.NameToLayer("OnlyVehicle");

	public static readonly int Vehicle = LayerMask.NameToLayer("Vehicle");

	public static readonly int SubRigidbodyExclude = LayerMask.NameToLayer("SubRigidbodyExclude");

	public static readonly int DefaultCollisionMask = GetAllCollisionsMask(Default);

	public static int GetAllCollisionsMask(int testLayer)
	{
		int num = 0;
		for (int i = 0; i < 32; i++)
		{
			if (!Physics.GetIgnoreLayerCollision(testLayer, i))
			{
				num |= 1 << i;
			}
		}
		return num;
	}

	public static bool IsMaskContainsLayer(int mask, int layer)
	{
		return (mask & (1 << layer)) != 0;
	}
}
