using Gendarme;
using ProtoBuf;
using UnityEngine;

[ExecuteInEditMode]
public class ExecutionLogger : MonoBehaviour, ISerializationCallbackReceiver, IProtoEventListener, IProtoTreeEventListener
{
	public static bool logInspectorCalls;

	[SuppressMessage("Subnautica.Rules", "AvoidBoxingRule")]
	public ExecutionLogger()
	{
		Debug.LogFormat("Inst {0}: Constructor", GetInstanceID());
	}

	public void Log(string message)
	{
		Debug.LogFormat(this, "Inst '{0}' ({1}) at frame {2} (time {3}): {4}", base.name, GetInstanceID(), Time.frameCount, Time.time, message);
	}

	[ProtoBeforeSerialization]
	private void ProtoBeforeSerialization()
	{
		Log("ProtoBeforeSerialization");
	}

	[ProtoAfterSerialization]
	private void ProtoAfterSerialization()
	{
		Log("ProtoAfterSerialization");
	}

	[ProtoBeforeDeserialization]
	private void ProtoBeforeDeserialization()
	{
		Log("ProtoBeforeDeserialization");
	}

	[ProtoAfterDeserialization]
	private void ProtoAfterDeserialization()
	{
		Log("ProtoAfterDeserialization");
	}

	private void Reset()
	{
		Log("Reset");
	}

	private void Awake()
	{
		Log("Awake");
	}

	private void OnEnable()
	{
		Log("OnEnable");
	}

	private void OnDisable()
	{
		Log("OnDisable");
	}

	private void Start()
	{
		Log("Start");
	}

	private void OnDestroy()
	{
		Log("OnDestroy");
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
		Log("OnProtoSerialize");
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		Log("OnProtoDeserialize");
	}

	public void OnAfterDeserialize()
	{
		if (logInspectorCalls)
		{
			Log("OnAfterDeserialize");
		}
	}

	public void OnBeforeSerialize()
	{
		if (logInspectorCalls)
		{
			Log("OnBeforeSerialize");
		}
	}

	public void OnProtoSerializeObjectTree(ProtobufSerializer serializer)
	{
		Log("OnProtoSerializeObjectTree");
	}

	public void OnProtoDeserializeObjectTree(ProtobufSerializer serializer)
	{
		Log("OnProtoDeserializeObjectTree");
	}
}
