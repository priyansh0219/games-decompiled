using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class PrecursorElevator : MonoBehaviour
{
	[AssertNotNull]
	public PrecursorElevatorTrigger[] elevatorPoints = new PrecursorElevatorTrigger[2];

	[NonSerialized]
	[ProtoMember(1)]
	public int elevatorPointIndex = -1;

	private void Awake()
	{
		for (int i = 0; i < 2; i++)
		{
			elevatorPoints[i].index = i;
		}
	}

	public void ActivateElevator(int index)
	{
		if (elevatorPointIndex == -1)
		{
			elevatorPointIndex = index;
		}
	}

	private void Update()
	{
		if (elevatorPointIndex != -1)
		{
			Vector3 position = Player.main.transform.position;
			Player.main.SetPosition(Vector3.Lerp(position, elevatorPoints[elevatorPointIndex].transform.position, Time.deltaTime * 1.5f));
			if (Vector3.Distance(position, elevatorPoints[elevatorPointIndex].transform.position) < 0.5f)
			{
				elevatorPointIndex = -1;
			}
		}
	}
}
