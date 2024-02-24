using System;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf;
using UWE;
using UnityEngine;

[ProtoContract]
public class UnstuckPlayer : MonoBehaviour
{
	private const int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int version = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public readonly List<Vector3> previousPositions = new List<Vector3>();

	[SerializeField]
	private float updateInterval = 5f;

	[SerializeField]
	private float minDistance = 10f;

	private const int maxPositionsCount = 10;

	private readonly Vector3 basePosition = Vector3.zero;

	private GameObject endGameSpawnGO;

	private Coroutine trackPlayerPositionRoutine;

	private bool playerWasInside;

	private static UnstuckPlayer instance;

	public int managedUpdateIndex { get; set; }

	public string GetProfileTag()
	{
		return "UnstuckPlayer";
	}

	public static bool CanWarp()
	{
		Player main = Player.main;
		if (main.cinematicModeActive)
		{
			return false;
		}
		if (!main.playerController.inputEnabled)
		{
			return false;
		}
		if (main.IsSuffocating())
		{
			return false;
		}
		if (IntroLifepodDirector.IsActive)
		{
			return false;
		}
		if (main.GetMode() != 0)
		{
			if (!main.inExosuit && !main.inSeamoth)
			{
				return false;
			}
			Vehicle vehicle = main.GetVehicle();
			if (vehicle == null || !vehicle.playerFullyEntered)
			{
				return false;
			}
		}
		return true;
	}

	public static void TryUnstuck()
	{
		if (instance != null)
		{
			instance.WarpToLastPosition();
		}
	}

	private void Start()
	{
		instance = this;
		Player main = Player.main;
		if (main != null)
		{
			main.playerRespawnEvent.AddHandler(this, OnPlayerRespawn);
		}
	}

	private void OnDestroy()
	{
		Player main = Player.main;
		if (main != null)
		{
			main.playerRespawnEvent.RemoveHandler(this, OnPlayerRespawn);
		}
	}

	private void OnEnable()
	{
		trackPlayerPositionRoutine = StartCoroutine(TrackPlayerPositions());
	}

	private void OnDisable()
	{
		if (trackPlayerPositionRoutine != null)
		{
			StopCoroutine(trackPlayerPositionRoutine);
			trackPlayerPositionRoutine = null;
		}
	}

	private IEnumerator TrackPlayerPositions()
	{
		while (true)
		{
			yield return new WaitForSeconds(updateInterval);
			TrySavePosition();
		}
	}

	private void TrySavePosition()
	{
		Player main = Player.main;
		if (main.cinematicModeActive || main.precursorOutOfWater)
		{
			return;
		}
		if (!main.IsInsideWalkable())
		{
			if (!(main.playerController.activeController == main.groundMotor) || main.groundMotor.grounded)
			{
				Vector3 position = main.transform.position;
				if (!IsCloseToPreviousPoints(position))
				{
					SavePosition(position);
				}
				playerWasInside = false;
			}
		}
		else if (!playerWasInside)
		{
			SavePosition(basePosition);
			playerWasInside = true;
		}
	}

	private void WarpToLastPosition()
	{
		StartCoroutine(WarpToLastPositionAsync());
	}

	private IEnumerator WarpToLastPositionAsync()
	{
		if (CanWarp())
		{
			Player player = Player.main;
			bool warpToInterior = false;
			Vector3 warpPosition = GetAndForgetClosestPosition(player.transform.position);
			if (warpPosition == basePosition)
			{
				warpToInterior = true;
				warpPosition = GetFallbackPosition();
			}
			uGUI.main.respawning.Show();
			player.ToNormalMode(findNewPosition: false);
			player.FreezeStats();
			player.forceCinematicMode = true;
			player.playerController.SetEnabled(enabled: false);
			yield return null;
			if (warpToInterior)
			{
				player.MovePlayerToRespawnPoint();
			}
			else
			{
				player.SetPosition(warpPosition);
				player.OnPlayerPositionCheat();
			}
			yield return new WaitForSecondsRealtime(1f);
			LargeWorldStreamer streamer = LargeWorldStreamer.main;
			while (!streamer.IsWorldSettled())
			{
				yield return CoroutineUtils.waitForNextFrame;
			}
			uGUI.main.respawning.Hide();
			player.UnfreezeStats();
			player.cinematicModeActive = false;
			player.playerController.SetEnabled(enabled: true);
		}
	}

	private bool CanWarpTo(Vector3 position, bool isInterior)
	{
		if ((double)Vector3.SqrMagnitude(Player.main.transform.position - position) < 0.25 * (double)minDistance * (double)minDistance)
		{
			return false;
		}
		if (!isInterior && Physics.Raycast(position, Vector3.up, out var hitInfo, Base.cellSize.y, -1, QueryTriggerInteraction.Ignore) && hitInfo.collider.GetComponentInParent<SubRoot>() != null)
		{
			return false;
		}
		return true;
	}

	private bool IsCloseToPreviousPoints(Vector3 position)
	{
		foreach (Vector3 previousPosition in previousPositions)
		{
			if (previousPosition != basePosition && Vector3.SqrMagnitude(previousPosition - position) < minDistance * minDistance)
			{
				return true;
			}
		}
		return false;
	}

	private void SavePosition(Vector3 position)
	{
		if (position == basePosition)
		{
			previousPositions.Remove(basePosition);
		}
		if (previousPositions.Count >= 10)
		{
			previousPositions.RemoveAt(0);
		}
		previousPositions.Add(position);
	}

	private Vector3 GetAndForgetClosestPosition(Vector3 playerPosition)
	{
		Vector3 result = basePosition;
		int num = -1;
		float num2 = float.PositiveInfinity;
		for (int num3 = previousPositions.Count - 1; num3 >= 0; num3--)
		{
			Vector3 vector = previousPositions[num3];
			bool flag = vector == basePosition;
			if (flag)
			{
				vector = GetFallbackPosition();
			}
			if (!CanWarpTo(vector, flag))
			{
				previousPositions.RemoveAt(num3);
				num--;
			}
			else
			{
				float num4 = Vector3.SqrMagnitude(playerPosition - vector);
				if (num4 < num2)
				{
					num2 = num4;
					num = num3;
					result = (flag ? basePosition : vector);
				}
			}
		}
		if (num >= 0)
		{
			previousPositions.RemoveAt(num);
		}
		return result;
	}

	private Vector3 GetFallbackPosition()
	{
		return Player.main.GetRespawnPosition();
	}

	private void OnPlayerRespawn(Player player)
	{
		previousPositions.Clear();
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Vector3 to = Player.main.transform.position;
		for (int num = previousPositions.Count - 1; num >= 0; num--)
		{
			Vector3 vector = previousPositions[num];
			if (vector == basePosition)
			{
				vector = GetFallbackPosition();
			}
			Gizmos.DrawSphere(vector, 0.5f);
			Gizmos.DrawLine(vector, to);
			to = vector;
		}
		Gizmos.color = Color.green;
		Vector3 fallbackPosition = GetFallbackPosition();
		Gizmos.DrawSphere(fallbackPosition, 0.49f);
		Gizmos.DrawLine(fallbackPosition, to);
	}
}
