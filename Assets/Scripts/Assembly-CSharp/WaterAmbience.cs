using UnityEngine;

public class WaterAmbience : MonoBehaviour
{
	[AssertNotNull]
	public FMOD_CustomEmitter reachSurface;

	[AssertNotNull]
	public FMOD_CustomEmitter reachSurfaceWithTank;

	[AssertNotNull]
	public FMOD_CustomEmitter reachSurfaceLowOxygen;

	[AssertNotNull]
	public FMOD_CustomEmitter diveStart;

	[AssertNotNull]
	public FMOD_CustomEmitter diveStartSplash;

	[AssertNotNull]
	public FMOD_CustomLoopingEmitter swim_surface;

	private bool wasUnderWater;

	private float timeReachSurfaceSoundPlayed;

	private Player player;

	private void Start()
	{
		player = Player.main;
	}

	private void PlayReachSurfaceSound()
	{
		if (!(Time.time < timeReachSurfaceSoundPlayed + 1f))
		{
			timeReachSurfaceSoundPlayed = Time.time;
			if (player.GetOxygenAvailable() <= 5f)
			{
				reachSurfaceLowOxygen.Play();
			}
			else if (player.oxygenMgr.HasOxygenTank())
			{
				reachSurfaceWithTank.Play();
			}
			else
			{
				reachSurface.Play();
			}
		}
	}

	private void Update()
	{
		bool flag = Player.main.IsUnderwater();
		if (flag != wasUnderWater)
		{
			if (flag)
			{
				((0f - player.playerController.velocity.y > 5.5f) ? diveStartSplash : diveStart).Play();
			}
			else
			{
				PlayReachSurfaceSound();
			}
			wasUnderWater = flag;
		}
		if (!Player.main.precursorOutOfWater)
		{
			swim_surface.Play();
		}
		else
		{
			swim_surface.Stop();
		}
	}
}
