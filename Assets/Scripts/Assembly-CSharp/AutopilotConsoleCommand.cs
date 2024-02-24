using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutopilotConsoleCommand : MonoBehaviour
{
	public static AutopilotConsoleCommand main;

	private static readonly int WaypointsCapacity = 7;

	private static readonly int MaxStuckCount = 10;

	private static readonly float ArriveDistance = 10f;

	private Stack<Vector3> waypoints = new Stack<Vector3>(WaypointsCapacity);

	private int stuckCount;

	[AssertNotNull]
	public TeleportCommandData data;

	private Vector3 m_LastPosition;

	private Vector3 m_StartPosition;

	private Quaternion m_StartOrientation;

	private void Awake()
	{
		main = this;
		DevConsole.RegisterConsoleCommand(this, "autopilot");
		DevConsole.RegisterConsoleCommand(this, "autopilotstop");
	}

	private void OnConsoleCommand_autopilot(NotificationCenter.Notification n)
	{
		Vehicle vehicle = Player.main.GetVehicle();
		if (vehicle == null)
		{
			ErrorMessage.AddDebug("Can't find vehicle");
			return;
		}
		NextWaypoint(vehicle);
		StartCoroutine(ContinueAutopilotRandom());
		m_LastPosition = vehicle.transform.position;
		m_StartPosition = m_LastPosition;
		m_StartOrientation = vehicle.transform.rotation;
	}

	private void OnConsoleCommand_autopilotstop(NotificationCenter.Notification n)
	{
		ErrorMessage.AddDebug("Stopping Autopilot");
		Vehicle vehicle = Player.main.GetVehicle();
		if (vehicle == null)
		{
			ErrorMessage.AddDebug("Can't find vehicle");
		}
		else
		{
			vehicle.DisableAutopilot();
		}
	}

	private IEnumerator ContinueAutopilotRandom()
	{
		while (true)
		{
			Vehicle vehicle = Player.main.GetVehicle();
			if (vehicle == null)
			{
				ErrorMessage.AddDebug("Coroutine can't find vehicle");
				yield break;
			}
			if (!vehicle.IsAutopilotEnabled)
			{
				break;
			}
			Vector3 position = vehicle.transform.position;
			Vector3 distanceToEndVec = position - vehicle.AutopilotDestination;
			float magnitude = distanceToEndVec.magnitude;
			ErrorMessage.AddDebug($"Distance Left: {magnitude}");
			if (magnitude < ArriveDistance)
			{
				NextWaypoint(vehicle);
			}
			m_LastPosition = position;
			yield return new WaitForSeconds(3f);
			Vector3 position2 = vehicle.transform.position;
			float magnitude2 = (m_LastPosition - position2).magnitude;
			ErrorMessage.AddDebug($"Travelled: {Mathf.Sqrt(magnitude2)}");
			if (magnitude2 < 2f)
			{
				Vector3 vector = distanceToEndVec;
				vector.y = 0f;
				if (vector.magnitude < ArriveDistance)
				{
					NextWaypoint(vehicle);
				}
				else if (!TryResolveVehicleStucked(vehicle))
				{
					ResetToStartPosition(vehicle);
					vehicle.DisableAutopilot();
					yield return new WaitForSeconds(1f);
					NextWaypoint(vehicle);
				}
			}
		}
		ErrorMessage.AddDebug("Autopilot is not enabled");
	}

	private void NextWaypoint(Vehicle vehicle)
	{
		if (waypoints.Count > 0)
		{
			waypoints.Pop();
		}
		if (waypoints.Count == 0)
		{
			PushRandomPosition(vehicle);
		}
		SetAutopilotToNextWaypoint(vehicle);
	}

	private void SetAutopilotToNextWaypoint(Vehicle vehicle)
	{
		Vector3 vector = waypoints.Peek();
		int count = waypoints.Count;
		ErrorMessage.AddDebug($"Autopiloting to position({count}): {vector}");
		vehicle.SetAutopilotDestination(vector);
		vehicle.EnableAutopilot();
	}

	private bool TryResolveVehicleStucked(Vehicle vehicle)
	{
		if (waypoints.Count < WaypointsCapacity && stuckCount < MaxStuckCount)
		{
			stuckCount++;
			float y = Random.Range(0.25f, 0.75f) * m_LastPosition.y;
			Vector3 vector = (m_LastPosition + waypoints.Peek()) / 2f;
			vector.y = y;
			waypoints.Push(vector);
			Vector3 vector2 = waypoints.Peek() - m_LastPosition;
			vector2.y = 0f;
			Vector3 vector3 = m_LastPosition - vector2.normalized * 30f;
			vector3.y *= Random.Range(0.25f, 0.75f);
			waypoints.Push(vector3);
			ErrorMessage.AddDebug("Intermediate waypoint: " + vector3);
			ErrorMessage.AddDebug("Intermediate waypoint: " + vector);
			SetAutopilotToNextWaypoint(vehicle);
			return true;
		}
		return false;
	}

	private void PushRandomPosition(Vehicle vehicle)
	{
		stuckCount = 0;
		int num = Random.Range(0, data.locations.Length);
		TeleportPosition teleportPosition = data.locations[num];
		ErrorMessage.AddDebug("Autopiloting target: " + teleportPosition.position);
		waypoints.Push(teleportPosition.position);
	}

	private void ResetToStartPosition(Vehicle vehicle)
	{
		waypoints.Clear();
		vehicle.TeleportVehicle(m_StartPosition, m_StartOrientation, keepRigidBodyKinematicState: true);
	}
}
