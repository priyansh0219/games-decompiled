using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class PrisonManager : MonoBehaviour
{
	private const int currentVersion = 1;

	[ProtoMember(1)]
	public int version = 1;

	[ProtoMember(2)]
	public int numCreatures;

	[ProtoMember(3)]
	public Vector3 exitPoint = Vector3.zero;

	[ProtoMember(4)]
	public bool babiesHatched;

	public int kMaxTreatments;

	public int kMaxCures;

	public static PrisonManager main { get; private set; }

	private void Awake()
	{
		main = this;
	}

	public void RegisterExitPoint(Vector3 pos)
	{
		exitPoint = pos;
	}

	public bool GetTeleportExitPoint(out Vector3 outPoint)
	{
		bool result = false;
		outPoint = Vector3.zero;
		if (exitPoint != Vector3.zero)
		{
			outPoint = exitPoint;
			result = true;
		}
		return result;
	}

	public static bool IsInsideAquarium(Vector3 position)
	{
		return new Bounds(new Vector3(325f, -1603f, -455f), new Vector3(500f, 299f, 500f)).Contains(position);
	}
}
