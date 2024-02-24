using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class Signal : MonoBehaviour, IProtoEventListener
{
	[AssertNotNull]
	public GameObject pingPrefab;

	[AssertNotNull]
	public LargeWorldEntity largeWorldEntity;

	public bool randomizeTarget;

	[ProtoMember(1)]
	public Vector3 targetPosition;

	[ProtoMember(2)]
	public string targetDescription;

	private bool isNewBorn = true;

	private GameObject pingObject;

	private void Start()
	{
		if (randomizeTarget && isNewBorn)
		{
			SignalInfo randomEntry = LargeWorld.main.signalDatabase.GetRandomEntry();
			targetPosition = randomEntry.position.ToVector3();
			targetDescription = randomEntry.description;
		}
	}

	public void Initialize()
	{
		pingObject = Object.Instantiate(pingPrefab, targetPosition, Quaternion.identity);
		pingObject.GetComponent<PingInstance>().SetLabel(Language.main.Get(targetDescription));
	}

	public void CleanUp()
	{
		Object.Destroy(pingObject);
	}

	public void OnProtoSerialize(ProtobufSerializer serializer)
	{
	}

	public void OnProtoDeserialize(ProtobufSerializer serializer)
	{
		isNewBorn = false;
		if (Vector3.Distance(targetPosition, new Vector3(-129f, -521f, -266f)) <= 10f)
		{
			targetPosition = new Vector3(-368f, -198f, -226f);
		}
	}
}
