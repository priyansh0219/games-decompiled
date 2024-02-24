using System;
using FMOD.Studio;
using UnityEngine;

public class FMODGameParams : MonoBehaviour, ICompileTimeCheckable
{
	public enum InteriorState
	{
		Always = 0,
		OnlyOutside = 1,
		OnlyInside = 2,
		OnlyInSubOrBase = 3,
		OnlyInSub = 4,
		OnlyInBase = 5
	}

	[AssertNotNull]
	public FMOD_CustomLoopingEmitter loopingEmitter;

	public bool alwaysActive;

	[Tooltip("Prefix matching, ignoring case")]
	public string onlyInBiome = "";

	public InteriorState interiorState = InteriorState.OnlyOutside;

	public bool debug;

	public bool isPlaying;

	private PARAMETER_ID depthParamIndex = FMODUWE.invalidParameterId;

	private PARAMETER_ID playerDamageParamIndex = FMODUWE.invalidParameterId;

	private PARAMETER_ID oxygenLeftParamIndex = FMODUWE.invalidParameterId;

	private PARAMETER_ID timeParamIndex = FMODUWE.invalidParameterId;

	private const string depthParamName = "depth";

	private const string playerDamageParamName = "playerDamage";

	private const string oxygenLeftParamName = "oxygenLeft";

	private const string timeParamName = "time";

	private void Start()
	{
		depthParamIndex = loopingEmitter.GetParameterIndex("depth");
		playerDamageParamIndex = loopingEmitter.GetParameterIndex("playerDamage");
		oxygenLeftParamIndex = loopingEmitter.GetParameterIndex("oxygenLeft");
		timeParamIndex = loopingEmitter.GetParameterIndex("time");
		if (FMODUWE.IsValidParameterId(oxygenLeftParamIndex))
		{
			Utils.GetLocalPlayerComp().tookBreathEvent.AddHandler(base.gameObject, OnTookBreath);
		}
		InvokeRepeating("UpdateParams", 0f, 0.5f);
	}

	private void OnTookBreath(Player player)
	{
		if ((bool)player)
		{
			float oxygenAvailable = Player.main.GetOxygenAvailable();
			loopingEmitter.SetParameterValue(oxygenLeftParamIndex, oxygenAvailable);
			if (debug)
			{
				Debug.Log(base.gameObject.name + ".FMODGameParams() - Setting \"oxygenLeft\" to " + oxygenAvailable);
			}
		}
	}

	private void UpdateParams()
	{
		if (!(loopingEmitter != null) || !base.gameObject.activeInHierarchy)
		{
			return;
		}
		bool flag = isPlaying;
		isPlaying = false;
		Player localPlayerComp = Utils.GetLocalPlayerComp();
		switch (interiorState)
		{
		case InteriorState.Always:
			isPlaying = true;
			break;
		case InteriorState.OnlyOutside:
			isPlaying = !localPlayerComp.IsInsideWalkable();
			break;
		case InteriorState.OnlyInside:
			isPlaying = localPlayerComp.IsInsideWalkable();
			break;
		case InteriorState.OnlyInSubOrBase:
			isPlaying = localPlayerComp.IsInSub() && localPlayerComp.currentWaterPark == null;
			break;
		case InteriorState.OnlyInSub:
			isPlaying = localPlayerComp.IsInSubmarine();
			break;
		case InteriorState.OnlyInBase:
			isPlaying = localPlayerComp.IsInBase() && localPlayerComp.currentWaterPark == null;
			break;
		default:
			isPlaying = false;
			break;
		}
		if (onlyInBiome.Length > 0)
		{
			string biomeString = localPlayerComp.GetBiomeString();
			isPlaying &= biomeString.StartsWith(onlyInBiome, StringComparison.OrdinalIgnoreCase);
		}
		isPlaying |= alwaysActive;
		if (isPlaying)
		{
			loopingEmitter.Play();
		}
		else
		{
			loopingEmitter.Stop();
		}
		DebugSoundConsoleCommand main = DebugSoundConsoleCommand.main;
		if (isPlaying != flag && (bool)main && main.debugMusic)
		{
			string arg = "<unknown>";
			if ((bool)loopingEmitter.asset)
			{
				arg = loopingEmitter.asset.name;
			}
			ErrorMessage.AddDebug(string.Format("{0} {1} ('{2}*')", isPlaying ? "Playing" : "Stopping", arg, onlyInBiome));
		}
		if (!isPlaying)
		{
			return;
		}
		if (FMODUWE.IsValidParameterId(depthParamIndex))
		{
			float y = Utils.GetLocalPlayerComp().transform.position.y;
			loopingEmitter.SetParameterValue(depthParamIndex, y);
			if (debug)
			{
				Debug.Log(base.gameObject.name + ".FMODGameParams() - Setting \"depth\" to " + y);
			}
		}
		if (FMODUWE.IsValidParameterId(playerDamageParamIndex))
		{
			float healthFraction = Utils.GetLocalPlayerComp().gameObject.GetComponent<LiveMixin>().GetHealthFraction();
			loopingEmitter.SetParameterValue(playerDamageParamIndex, healthFraction);
			if (debug)
			{
				Debug.Log(base.gameObject.name + ".FMODGameParams() - Setting \"playerDamage\" to " + healthFraction);
			}
		}
		if (FMODUWE.IsValidParameterId(timeParamIndex) && DayNightCycle.main != null)
		{
			loopingEmitter.SetParameterValue(timeParamIndex, DayNightCycle.main.GetDayScalar() * 24f);
		}
	}

	public string CompileTimeCheck()
	{
		return null;
	}
}
