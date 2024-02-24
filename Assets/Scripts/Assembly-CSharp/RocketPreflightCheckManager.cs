using System;
using System.Collections;
using System.Collections.Generic;
using Gendarme;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
public class RocketPreflightCheckManager : MonoBehaviour
{
	public static readonly Dictionary<PreflightCheck, string> localizedPreflightNames = new Dictionary<PreflightCheck, string>
	{
		{
			PreflightCheck.LifeSupport,
			"PreflightCheck_LifeSupport"
		},
		{
			PreflightCheck.TimeCapsule,
			"PreflightCheck_TimeCapsule"
		},
		{
			PreflightCheck.PrimaryComputer,
			"PreflightCheck_Computer"
		},
		{
			PreflightCheck.CommunicationsArray,
			"PreflightCheck_CommunicationsArray"
		},
		{
			PreflightCheck.Hydraulics,
			"PreflightCheck_Hydraulics"
		},
		{
			PreflightCheck.AuxiliaryPowerUnit,
			"PreflightCheck_APU"
		}
	};

	public Transform preflightSounds;

	public Transform preflightCheckPDANotifications;

	public Transform preflightCheckScreenHolder;

	public Transform preflightSwitchHolder;

	public LaunchRocket launchRocketScript;

	[AssertNotNull]
	public FMOD_CustomEmitter rocketReadySFX;

	[AssertNotNull]
	public PDANotification rocketReadyVO;

	[AssertNotNull]
	public Animator stageThreeAnimator;

	private int totalPreflightChecks = 5;

	private bool rocketReadyDelay;

	private const int _currentVersion = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(2)]
	public readonly HashSet<PreflightCheck> preflightChecks = new HashSet<PreflightCheck>();

	private void Start()
	{
		preflightChecks.Remove(PreflightCheck.TimeCapsule);
		if (ReturnRocketReadyForLaunch(useDelay: false))
		{
			Invoke("RocketReady", 5f);
		}
	}

	public bool ReturnRocketReadyForLaunch(bool useDelay)
	{
		if (useDelay && preflightChecks.Count == totalPreflightChecks && rocketReadyDelay)
		{
			return true;
		}
		if (!useDelay && preflightChecks.Count == totalPreflightChecks)
		{
			return true;
		}
		return false;
	}

	public string ReturnLocalizedPreflightCheckName(PreflightCheck preflightCheck)
	{
		return Language.main.Get(localizedPreflightNames[preflightCheck]);
	}

	public bool GetPreflightComplete(PreflightCheck preflightCheck)
	{
		if (preflightChecks.Contains(preflightCheck))
		{
			return true;
		}
		if (preflightCheck == PreflightCheck.TimeCapsule)
		{
			return PlayerTimeCapsule.main.IsValid();
		}
		return false;
	}

	public void SetTimeCapsuleReady(bool isReady)
	{
		if (isReady)
		{
			preflightCheckScreenHolder.BroadcastMessage("SetPreflightCheckComplete", PreflightCheck.TimeCapsule, SendMessageOptions.RequireReceiver);
			StartCoroutine(PlayFlightCheckVO(PreflightCheck.TimeCapsule));
		}
		else
		{
			preflightCheckScreenHolder.BroadcastMessage("SetPreflightCheckIncomplete", PreflightCheck.TimeCapsule, SendMessageOptions.RequireReceiver);
		}
	}

	public void CompletePreflightCheck(PreflightCheck completeCheck)
	{
		preflightChecks.Add(completeCheck);
		preflightCheckScreenHolder.BroadcastMessage("SetPreflightCheckComplete", completeCheck, SendMessageOptions.RequireReceiver);
		StartCoroutine(PlayFlightCheckVO(completeCheck));
		if (ReturnRocketReadyForLaunch(useDelay: false))
		{
			Invoke("RocketReady", 10f);
		}
	}

	private void RocketReady()
	{
		rocketReadySFX.Play();
		rocketReadyVO.Play();
		stageThreeAnimator.SetBool("ready", value: true);
		rocketReadyDelay = true;
	}

	public IEnumerator PlayFlightCheckVO(PreflightCheck preflightCheck)
	{
		preflightSounds.BroadcastMessage("PlayPreflightSound", preflightCheck, SendMessageOptions.RequireReceiver);
		yield return new WaitForSeconds(4f);
		preflightCheckPDANotifications.BroadcastMessage("PlayPDANotification", preflightCheck, SendMessageOptions.RequireReceiver);
	}
}
