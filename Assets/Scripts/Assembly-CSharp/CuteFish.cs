using System;
using ProtoBuf;
using Story;
using UWE;
using UnityEngine;

[ProtoContract]
public class CuteFish : Creature
{
	[AssertNotNull]
	public GameObject cinematicTarget;

	[AssertNotNull]
	public CreatureFollowPlayer creatureFollowPlayer;

	[AssertNotNull]
	public PlayAnimation playAnimation;

	[AssertNotNull]
	public LargeWorldEntity largeWorldEntity;

	[AssertNotNull]
	public WaterParkCreature waterParkCreature;

	[AssertNotNull]
	public StoryGoal cuteFishGoal;

	public float warpDistance = 40f;

	public float warpInterval = 5f;

	[NonSerialized]
	[ProtoMember(1)]
	public bool _followingPlayer = true;

	[NonSerialized]
	[ProtoMember(2)]
	public bool _goodbyePlayed;

	public bool followingPlayer
	{
		get
		{
			return _followingPlayer;
		}
		set
		{
			creatureFollowPlayer.enabled = value;
			playAnimation.enabled = value;
			ScanCreatureActions();
			_followingPlayer = value;
			base.transform.SetParent(null);
			largeWorldEntity.cellLevel = ((!value) ? LargeWorldEntity.CellLevel.Medium : LargeWorldEntity.CellLevel.Global);
			if ((bool)LargeWorldStreamer.main && LargeWorldStreamer.main.cellManager != null)
			{
				LargeWorldStreamer.main.cellManager.RegisterEntity(largeWorldEntity);
			}
		}
	}

	public bool goodbyePlayed
	{
		get
		{
			return _goodbyePlayed;
		}
		set
		{
			_goodbyePlayed = value;
			if (_goodbyePlayed)
			{
				followingPlayer = false;
			}
		}
	}

	public override void Start()
	{
		followingPlayer = _followingPlayer;
		base.Start();
		SetFriend(Player.main.gameObject);
		InvokeRepeating("WarpToPlayer", UnityEngine.Random.value * warpInterval, warpInterval);
		cuteFishGoal.Trigger();
		DevConsole.RegisterConsoleCommand(this, "hellofish");
	}

	private void OnAddToWaterPark(WaterParkCreature waterParkCreature)
	{
		cinematicTarget.SetActive(value: false);
	}

	public override void OnDrop()
	{
		base.OnDrop();
		bool flag = waterParkCreature.IsInsideWaterPark();
		cinematicTarget.SetActive(!flag);
	}

	private void WarpToPlayer()
	{
		if (!_followingPlayer || Player.main.GetBiomeString().StartsWith("precursor", StringComparison.OrdinalIgnoreCase))
		{
			return;
		}
		Vector3 vector = Player.main.transform.position - base.transform.position;
		if (!(vector.magnitude > warpDistance))
		{
			return;
		}
		Vector3 position = Player.main.transform.position - vector.normalized * warpDistance;
		position.y = Mathf.Min(position.y, -1f);
		int num = UWE.Utils.OverlapSphereIntoSharedBuffer(base.transform.position, 5f);
		for (int i = 0; i < num; i++)
		{
			if ((bool)UWE.Utils.sharedColliderBuffer[i].GetComponentInParent<SubRoot>())
			{
				return;
			}
		}
		base.transform.position = position;
	}

	public override void OnKill()
	{
		base.OnKill();
		followingPlayer = false;
	}

	private void OnConsoleCommand_hellofish(NotificationCenter.Notification n)
	{
		goodbyePlayed = false;
	}
}
