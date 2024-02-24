using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class FairRandomizer : MonoBehaviour
{
	[NonSerialized]
	[ProtoMember(1)]
	public int version = 1;

	[ProtoMember(2)]
	public float timeLastCheck = -1f;

	[ProtoMember(3)]
	public float entropy;

	public bool CheckChance(float percentChance)
	{
		if (percentChance <= 0f)
		{
			return false;
		}
		if (percentChance >= 1f)
		{
			return true;
		}
		bool result = false;
		entropy += percentChance;
		if (entropy > UnityEngine.Random.value)
		{
			entropy -= 1f;
			result = true;
		}
		return result;
	}
}
