using System;
using ProtoBuf;
using Story;
using UnityEngine;

[ProtoContract]
public class AuroraWarnings : MonoBehaviour, ICompileTimeCheckable
{
	private const int currentVersion = 2;

	[NonSerialized]
	[ProtoMember(3)]
	public float timeSerialized;

	[NonSerialized]
	[ProtoMember(4)]
	public int version = 2;

	[AssertNotNull]
	public StoryGoal auroraWarning1;

	[AssertNotNull]
	public StoryGoal auroraWarning2;

	[AssertNotNull]
	public StoryGoal auroraWarning3;

	[AssertNotNull]
	public StoryGoal auroraWarning4;

	private Utils.ScalarMonitor timeMonitor = new Utils.ScalarMonitor(0f);

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
		timeSerialized = timeMonitor.Get();
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		timeMonitor.Init(timeSerialized);
	}

	private void Update()
	{
		if (!(LargeWorldStreamer.main == null) && LargeWorldStreamer.main.IsReady() && DayNightCycle.main != null && CrashedShipExploder.main != null)
		{
			timeMonitor.Update(DayNightCycle.main.timePassedAsFloat);
			float timeToStartWarning = CrashedShipExploder.main.GetTimeToStartWarning();
			float timeToStartCountdown = CrashedShipExploder.main.GetTimeToStartCountdown();
			if (timeMonitor.JustWentAbove(timeToStartCountdown))
			{
				auroraWarning4.Trigger();
			}
			else if (timeMonitor.JustWentAbove(Mathf.Lerp(timeToStartWarning, timeToStartCountdown, 0.8f)))
			{
				auroraWarning3.Trigger();
			}
			else if (timeMonitor.JustWentAbove(Mathf.Lerp(timeToStartWarning, timeToStartCountdown, 0.5f)))
			{
				auroraWarning2.Trigger();
			}
			else if (timeMonitor.JustWentAbove(Mathf.Lerp(timeToStartWarning, timeToStartCountdown, 0.2f)))
			{
				auroraWarning1.Trigger();
			}
		}
	}

	public string CompileTimeCheck()
	{
		return StoryGoalUtils.CheckStoryGoals(new StoryGoal[4] { auroraWarning1, auroraWarning2, auroraWarning3, auroraWarning4 });
	}
}
