using System;
using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class Rocket : MonoBehaviour, IProtoEventListener
{
	public enum RocketElevatorStates
	{
		Up = 0,
		Down = 1,
		AtTop = 2,
		AtBottom = 3
	}

	public enum RocketStages
	{
		RocketBase = 0,
		RocketBaseLadder = 1,
		RocketStage1 = 2,
		RocketStage2 = 3,
		RocketStage3 = 4
	}

	[AssertNotNull]
	public GameObject rocketConstructor;

	[AssertNotNull]
	public GameObject[] stageObjects;

	public TechType[] stageTech;

	[AssertNotNull]
	public SubName subName;

	[AssertNotNull]
	public Rigidbody rb;

	[AssertNotNull]
	public Transform elevatorTrans;

	[AssertNotNull]
	public Transform[] elevatorPositions;

	[AssertNotNull]
	public FMOD_CustomEmitter useLiftSFX;

	[AssertNotNull]
	public FMOD_CustomEmitter liftErrorSFX;

	[AssertNotNull]
	public FMOD_CustomEmitter rocketAmbienceSFX;

	[AssertNotNull]
	public FMOD_CustomEmitter rocketAdvanceStageSFX;

	[AssertNotNull]
	public FMOD_CustomEmitter rocketFinalStageSFX;

	public AnimationCurve elevatorDampening;

	public bool elevatorBlocked;

	public float timeToConstruct = 25f;

	private const int version = 1;

	[NonSerialized]
	[ProtoMember(1)]
	public int currentRocketStage;

	[NonSerialized]
	[ProtoMember(2)]
	public RocketElevatorStates elevatorState = RocketElevatorStates.AtBottom;

	[NonSerialized]
	[ProtoMember(3)]
	public float elevatorPosition;

	[NonSerialized]
	[ProtoMember(4)]
	public int currentVersion = 1;

	[NonSerialized]
	[ProtoMember(5)]
	public string rocketName = string.Empty;

	[NonSerialized]
	[ProtoMember(6, OverwriteList = true)]
	public Vector3[] rocketColors;

	private bool isFinished;

	private bool isReady = true;

	private bool isBuilding;

	private float timeBuildCompleted = -1f;

	private float prevElevatorPosition;

	public static bool IsAnyRocketReady { get; private set; }

	private void Awake()
	{
		SetElevatorPosition();
	}

	private void Start()
	{
		base.gameObject.SetActive(value: true);
		rb.maxDepenetrationVelocity = 5f;
		if (currentRocketStage == 0)
		{
			isFinished = false;
			isReady = false;
			rb.isKinematic = true;
			rocketConstructor.SetActive(value: false);
			for (int i = 1; i < 5; i++)
			{
				stageObjects[i].SetActive(value: false);
			}
			stageObjects[0].SendMessage("StartConstruction", SendMessageOptions.RequireReceiver);
		}
		else
		{
			for (int j = 0; j < 5; j++)
			{
				if (j >= currentRocketStage)
				{
					stageObjects[j].SetActive(value: false);
				}
			}
		}
		BroadcastMessage("AdvanceToStage", currentRocketStage, SendMessageOptions.RequireReceiver);
		if (currentRocketStage == 5)
		{
			isFinished = true;
			IsAnyRocketReady = true;
		}
	}

	public void CallElevator(bool up)
	{
		bool flag = true;
		if (up && elevatorState != RocketElevatorStates.AtBottom)
		{
			flag = false;
		}
		if (!up && elevatorState != RocketElevatorStates.AtTop)
		{
			flag = false;
		}
		if (flag)
		{
			elevatorState = ((!up) ? RocketElevatorStates.Down : RocketElevatorStates.Up);
			useLiftSFX.Play();
		}
		else
		{
			liftErrorSFX.Play();
		}
	}

	public void ElevatorControlButtonActivate()
	{
		if (elevatorState == RocketElevatorStates.AtTop)
		{
			elevatorState = RocketElevatorStates.Down;
			useLiftSFX.Play();
		}
		else if (elevatorState == RocketElevatorStates.AtBottom)
		{
			elevatorState = RocketElevatorStates.Up;
			useLiftSFX.Play();
		}
		else
		{
			liftErrorSFX.Play();
		}
	}

	private void Update()
	{
		if (!rb.isKinematic && !isBuilding && timeBuildCompleted + 5f < Time.time && rb.velocity.sqrMagnitude < 0.1f)
		{
			rb.isKinematic = true;
		}
		if (elevatorState != RocketElevatorStates.AtTop && elevatorState != RocketElevatorStates.AtBottom)
		{
			float num = 0f;
			if (elevatorState == RocketElevatorStates.Up)
			{
				num = 1f;
			}
			if (elevatorState != RocketElevatorStates.Down || !elevatorBlocked || !(elevatorPosition < 0.2f) || !(Player.main.transform.position.y < elevatorTrans.position.y))
			{
				float num2 = elevatorDampening.Evaluate(elevatorPosition);
				prevElevatorPosition = elevatorPosition;
				elevatorPosition = Mathf.MoveTowards(elevatorPosition, num, Time.deltaTime / num2);
			}
			else if (elevatorPosition < 0.1f)
			{
				elevatorBlocked = false;
				Player.main.OnKill(DamageType.Normal);
			}
			SetElevatorPosition();
			if (Mathf.Approximately(elevatorPosition, num))
			{
				elevatorState = ((num == 0f) ? RocketElevatorStates.AtBottom : RocketElevatorStates.AtTop);
				useLiftSFX.Stop();
			}
		}
	}

	private void SetElevatorPosition()
	{
		elevatorTrans.position = Vector3.Lerp(elevatorPositions[0].position, elevatorPositions[1].position, elevatorPosition);
		if (elevatorPosition != prevElevatorPosition)
		{
			Physics.SyncTransforms();
		}
	}

	public void OnConstructionDone(GameObject constructedObject)
	{
		isBuilding = false;
		isReady = true;
		BroadcastMessage("AdvanceToStage", currentRocketStage, SendMessageOptions.RequireReceiver);
	}

	public void SubConstructionComplete()
	{
		isBuilding = false;
		timeBuildCompleted = Time.time;
		if (currentRocketStage == 0)
		{
			rb.isKinematic = false;
			rocketConstructor.SetActive(value: true);
			if (!isReady)
			{
				AdvanceRocketStage();
			}
			isReady = true;
		}
	}

	public void AdvanceRocketStage()
	{
		currentRocketStage++;
		if (currentRocketStage == 5)
		{
			isFinished = true;
			IsAnyRocketReady = true;
		}
		else
		{
			KnownTech.Add(GetCurrentStageTech());
		}
	}

	public bool IsRocketReady()
	{
		return isReady;
	}

	public GameObject StartRocketConstruction()
	{
		isBuilding = true;
		stageObjects[currentRocketStage].SetActive(value: true);
		VFXConstructing component = stageObjects[currentRocketStage].GetComponent<VFXConstructing>();
		if ((bool)component)
		{
			float num = (Application.isEditor ? 5f : timeToConstruct);
			component.timeToConstruct = num;
			component.StartConstruction();
		}
		rocketAdvanceStageSFX.Play();
		if (currentRocketStage == 4)
		{
			rocketFinalStageSFX.Play();
		}
		isReady = false;
		BroadcastMessage("SetBuildingAnimationScreen");
		AdvanceRocketStage();
		return stageObjects[currentRocketStage - 1];
	}

	public TechType GetCurrentStageTech()
	{
		return stageTech[currentRocketStage];
	}

	public bool IsFinished()
	{
		return isFinished;
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
		rocketName = subName.GetName();
		rocketColors = subName.GetColors();
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		subName.DeserializeName(rocketName);
		subName.DeserializeColors(rocketColors);
		rb.isKinematic = true;
	}
}
